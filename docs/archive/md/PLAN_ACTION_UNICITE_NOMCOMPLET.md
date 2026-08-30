# 📋 Plan d'Action : Unicité basée sur NomCompletEleve + NomCompletTuteur

## 🎯 Objectif

Implémenter une vérification d'unicité basée sur le couple `(NomCompletEleve normalisé, NomCompletTuteur normalisé)` avec gestion des cas spéciaux (jumeaux, transferts, changements de tuteur).

---

## ✅ Recommandation Finale

**Critères d'unicité** :
1. ✅ **NomCompletEleve** normalisé (gère accents, espaces, ordre)
2. ✅ **DateNaissance** (obligatoire pour éviter les jumeaux)
3. ✅ **NomCompletTuteur** normalisé (gère accents, espaces, ordre)
4. ✅ **IdEcole** (pour permettre les transferts entre écoles)

**Logique** :
- Si correspondance exacte → Réutiliser l'élève existant
- Si même nom + date mais tuteur différent → Mettre à jour le tuteur
- Si même nom + tuteur mais école différente → Proposer le transfert

---

## 📝 Implémentation

### Étape 1 : Améliorer la Normalisation

**Fichier** : `Services/InscriptionService.cs`

**Ajouter après la méthode `NormalizeName()` (ligne ~148)** :

```csharp
/// <summary>
/// Normalise un nom complet pour la comparaison robuste
/// Gère : accents, caractères spéciaux, espaces multiples, ordre des mots
/// </summary>
private string NormalizeNomComplet(string? nomComplet)
{
    if (string.IsNullOrWhiteSpace(nomComplet))
        return string.Empty;
    
    // 1. Supprimer les accents
    var normalized = RemoveAccents(nomComplet);
    
    // 2. Remplacer les caractères spéciaux par des espaces
    normalized = Regex.Replace(normalized, @"[^\w\s]", " ");
    
    // 3. Normaliser les espaces multiples
    normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
    
    // 4. Optionnel : Trier les mots (pour gérer l'ordre différent)
    // Exemple : "Jean Pierre" = "Pierre Jean"
    var mots = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    Array.Sort(mots);
    normalized = string.Join(" ", mots);
    
    // 5. Mettre en majuscules
    return normalized.ToUpperInvariant();
}

/// <summary>
/// Supprime les accents d'une chaîne de caractères
/// </summary>
private string RemoveAccents(string text)
{
    if (string.IsNullOrWhiteSpace(text))
        return text;
    
    var normalizedString = text.Normalize(NormalizationForm.FormD);
    var stringBuilder = new StringBuilder();
    
    foreach (var c in normalizedString)
    {
        var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
        if (unicodeCategory != UnicodeCategory.NonSpacingMark)
        {
            stringBuilder.Append(c);
        }
    }
    
    return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
}
```

**Note** : Ajouter les `using` nécessaires en haut du fichier :
```csharp
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
```

---

### Étape 2 : Créer la Méthode de Recherche par NomComplet

**Fichier** : `Services/InscriptionService.cs`

**Ajouter après `FindEleveExistantAsync()` (ligne ~197)** :

```csharp
/// <summary>
/// ✨ NOUVEAU : Vérifie si un élève existe déjà avec les mêmes critères
/// Utilise NomComplet normalisé au lieu de Nom/Postnom/Prenom séparés
/// Critères : NomCompletEleve normalisé + DateNaissance + NomCompletTuteur normalisé + IdEcole
/// </summary>
private async Task<Eleve?> FindEleveExistantParNomCompletAsync(
    string nomCompletEleve,
    DateTime dateNaissance,
    string nomCompletTuteur,
    int idEcole)
{
    // Normaliser les noms complets
    var nomCompletEleveNormalise = NormalizeNomComplet(nomCompletEleve);
    var nomCompletTuteurNormalise = NormalizeNomComplet(nomCompletTuteur);
    
    if (string.IsNullOrWhiteSpace(nomCompletEleveNormalise))
    {
        _logger.LogWarning("⚠️ NomCompletEleve vide ou invalide");
        return null;
    }
    
    _logger.LogDebug($"🔍 Recherche élève : NomComplet='{nomCompletEleve}' (normalisé: '{nomCompletEleveNormalise}'), DateNaissance={dateNaissance:yyyy-MM-dd}, Tuteur='{nomCompletTuteur}' (normalisé: '{nomCompletTuteurNormalise}'), Ecole={idEcole}");
    
    // Rechercher les élèves de l'école avec la même date de naissance
    var eleves = await _context.Eleves
        .Include(e => e.Classe)
            .ThenInclude(c => c.Direction)
                .ThenInclude(d => d.Ecole)
        .Include(e => e.Tuteur)
        .Where(e => 
            e.DateNaissance.Date == dateNaissance.Date
            && e.Classe != null 
            && e.Classe.Direction != null 
            && e.Classe.Direction.Ecole != null
            && e.Classe.Direction.Ecole.IdEcole == idEcole
        )
        .ToListAsync();
    
    _logger.LogDebug($"🔍 {eleves.Count} élève(s) trouvé(s) avec la même date de naissance dans l'école {idEcole}");
    
    // Comparer les noms complets normalisés
    foreach (var eleve in eleves)
    {
        var eleveNomCompletNormalise = NormalizeNomComplet(eleve.NomComplet);
        
        // Si le nom de l'élève correspond
        if (eleveNomCompletNormalise == nomCompletEleveNormalise)
        {
            _logger.LogDebug($"✅ Nom de l'élève correspond : '{eleve.NomComplet}' (normalisé: '{eleveNomCompletNormalise}')");
            
            // Vérifier le tuteur
            if (eleve.Tuteur != null)
            {
                var tuteurNomCompletNormalise = NormalizeNomComplet(eleve.Tuteur.NomComplet);
                
                if (tuteurNomCompletNormalise == nomCompletTuteurNormalise)
                {
                    _logger.LogInformation($"✅ Élève trouvé (correspondance exacte) : ID={eleve.IdEleve}, Nom='{eleve.NomComplet}', Tuteur='{eleve.Tuteur.NomComplet}'");
                    return eleve; // ✅ Correspondance exacte
                }
                else
                {
                    _logger.LogDebug($"⚠️ Nom élève correspond mais tuteur différent : Tuteur BDD='{eleve.Tuteur.NomComplet}' (normalisé: '{tuteurNomCompletNormalise}') vs Recherche='{nomCompletTuteur}' (normalisé: '{nomCompletTuteurNormalise}')");
                }
            }
            else if (string.IsNullOrWhiteSpace(nomCompletTuteurNormalise))
            {
                // Si l'élève n'a pas de tuteur et qu'on cherche sans tuteur
                _logger.LogInformation($"✅ Élève trouvé (sans tuteur) : ID={eleve.IdEleve}, Nom='{eleve.NomComplet}'");
                return eleve;
            }
            else
            {
                _logger.LogDebug($"⚠️ Nom élève correspond mais élève n'a pas de tuteur en BDD");
            }
        }
    }
    
    _logger.LogDebug($"❌ Aucun élève trouvé avec les critères fournis");
    return null;
}
```

---

### Étape 3 : Modifier CreateInscriptionAsync pour Utiliser la Nouvelle Méthode

**Fichier** : `Services/InscriptionService.cs`

**Modifier la section de recherche d'élève existant (ligne ~489-502)** :

**AVANT** :
```csharp
else
{
    // ✅ NOUVEAU : Recherche automatique d'élève existant
    // Chercher un élève avec les mêmes critères AVANT de créer le tuteur
    // On va chercher avec les données du DTO, puis vérifier le tuteur après
    eleveTrouve = await FindEleveExistantAsync(
        inscriptionDto.NomEleve,
        inscriptionDto.PostnomEleve,
        inscriptionDto.PrenomEleve,
        inscriptionDto.DateNaissanceEleve,
        inscriptionDto.IdTuteurExistant, // Peut être null
        inscriptionDto.IdEcole
    );
}
```

**APRÈS** :
```csharp
else
{
    // ✨ NOUVEAU : Recherche automatique d'élève existant par NomComplet
    // Construire le nom complet de l'élève
    var nomCompletEleve = $"{inscriptionDto.NomEleve} {inscriptionDto.PostnomEleve} {inscriptionDto.PrenomEleve}".Trim();
    
    // Chercher un élève avec les mêmes critères AVANT de créer le tuteur
    eleveTrouve = await FindEleveExistantParNomCompletAsync(
        nomCompletEleve,
        inscriptionDto.DateNaissanceEleve,
        inscriptionDto.NomCompletTuteur,
        inscriptionDto.IdEcole
    );
    
    // Si pas trouvé avec NomComplet, essayer avec l'ancienne méthode (rétrocompatibilité)
    if (eleveTrouve == null)
    {
        _logger.LogDebug("🔍 Recherche avec méthode ancienne (Nom/Postnom/Prenom)");
        eleveTrouve = await FindEleveExistantAsync(
            inscriptionDto.NomEleve,
            inscriptionDto.PostnomEleve,
            inscriptionDto.PrenomEleve,
            inscriptionDto.DateNaissanceEleve,
            inscriptionDto.IdTuteurExistant, // Peut être null
            inscriptionDto.IdEcole
        );
    }
}
```

**Modifier également la vérification finale (ligne ~634-641)** :

**AVANT** :
```csharp
// Vérifier si un élève avec les mêmes critères existe déjà
// (double vérification après création du tuteur si nouveau)
var eleveExistantFinal = await FindEleveExistantAsync(
    inscriptionDto.NomEleve,
    inscriptionDto.PostnomEleve,
    inscriptionDto.PrenomEleve,
    inscriptionDto.DateNaissanceEleve,
    newIdTuteur,
    inscriptionDto.IdEcole
);
```

**APRÈS** :
```csharp
// ✨ NOUVEAU : Vérification finale avec NomComplet
// Construire le nom complet de l'élève
var nomCompletEleve = $"{inscriptionDto.NomEleve} {inscriptionDto.PostnomEleve} {inscriptionDto.PrenomEleve}".Trim();

// Récupérer le nom complet du tuteur créé/trouvé
var tuteur = await _context.Tuteurs.FindAsync(newIdTuteur);
var nomCompletTuteur = tuteur?.NomComplet ?? inscriptionDto.NomCompletTuteur;

// Vérifier si un élève avec les mêmes critères existe déjà
var eleveExistantFinal = await FindEleveExistantParNomCompletAsync(
    nomCompletEleve,
    inscriptionDto.DateNaissanceEleve,
    nomCompletTuteur,
    inscriptionDto.IdEcole
);

// Si pas trouvé, essayer avec l'ancienne méthode
if (eleveExistantFinal == null)
{
    eleveExistantFinal = await FindEleveExistantAsync(
        inscriptionDto.NomEleve,
        inscriptionDto.PostnomEleve,
        inscriptionDto.PrenomEleve,
        inscriptionDto.DateNaissanceEleve,
        newIdTuteur,
        inscriptionDto.IdEcole
    );
}
```

---

## 🧪 Tests à Effectuer

### Test 1 : Doublon Exact
- **Données** : Même `NomCompletEleve`, même `DateNaissance`, même `NomCompletTuteur`, même école
- **Résultat attendu** : Réutilisation de l'élève existant

### Test 2 : Variations de Nom
- **Données** : `"Jean Pierre MUKENDI"` vs `"Jean  Pierre  MUKENDI"` (espaces multiples)
- **Résultat attendu** : Détection du doublon

### Test 3 : Variations d'Ordre
- **Données** : `"Jean Pierre MUKENDI"` vs `"Pierre Jean MUKENDI"` (ordre différent)
- **Résultat attendu** : Détection du doublon (si tri activé)

### Test 4 : Jumeaux
- **Données** : Même nom, même tuteur, même école, mais dates de naissance différentes
- **Résultat attendu** : Création de deux élèves distincts

### Test 5 : Transfert
- **Données** : Même nom, même tuteur, même date, mais école différente
- **Résultat attendu** : Création d'un nouvel élève (ou proposition de transfert)

### Test 6 : Changement de Tuteur
- **Données** : Même nom, même date, même école, mais tuteur différent
- **Résultat attendu** : Mise à jour du tuteur de l'élève existant

---

## 📊 Comparaison : Avant vs Après

| Scénario | Avant (Nom/Postnom/Prenom) | Après (NomComplet) |
|----------|---------------------------|-------------------|
| `"Jean Pierre MUKENDI"` vs `"Jean  Pierre  MUKENDI"` | ❌ Non détecté | ✅ Détecté |
| `"Jean-Pierre MUKENDI"` vs `"Jean Pierre MUKENDI"` | ❌ Non détecté | ✅ Détecté |
| `"José MUKENDI"` vs `"Jose MUKENDI"` | ❌ Non détecté | ✅ Détecté |
| `"Jean Pierre"` vs `"Pierre Jean"` | ❌ Non détecté | ✅ Détecté (si tri activé) |
| Jumeaux (dates différentes) | ✅ Géré | ✅ Géré |
| Transfert (écoles différentes) | ✅ Géré | ✅ Géré |

---

## ⚠️ Points d'Attention

### 1. Performance

**Impact** : La normalisation et le tri des mots ajoutent un coût CPU.

**Optimisation** :
- ✅ Normaliser une seule fois par recherche
- ✅ Utiliser un cache pour les normalisations fréquentes (optionnel)
- ✅ Indexer `NomComplet` en base de données pour accélérer les recherches

### 2. Cas Limites

**Jumeaux avec même nom** :
- Si deux jumeaux ont exactement le même nom complet et la même date de naissance
- **Solution** : Alerter l'utilisateur et demander une confirmation manuelle

**Transferts** :
- Si un élève est transféré d'une école à une autre
- **Solution** : Créer un nouvel enregistrement ou mettre à jour l'école (selon la logique métier)

### 3. Rétrocompatibilité

**Anciennes données** :
- Les élèves existants ont `NomComplet` calculé mais peuvent avoir des variations
- **Solution** : Garder l'ancienne méthode `FindEleveExistantAsync()` comme fallback

---

## 📋 Checklist d'Implémentation

- [ ] 1. Ajouter les `using` nécessaires (`System.Text`, `System.Globalization`, `System.Text.RegularExpressions`)
- [ ] 2. Implémenter `RemoveAccents()`
- [ ] 3. Implémenter `NormalizeNomComplet()`
- [ ] 4. Implémenter `FindEleveExistantParNomCompletAsync()`
- [ ] 5. Modifier `CreateInscriptionAsync()` pour utiliser la nouvelle méthode
- [ ] 6. Ajouter des logs détaillés
- [ ] 7. Tester avec des cas réels
- [ ] 8. Documenter les cas limites

---

## 🎯 Résumé

**Votre proposition** : `(NomCompletEleve, NomCompletTuteur)` unique

**Recommandation** : ✅ **Excellente idée**, mais avec améliorations :

1. ✅ **Normalisation** : Gérer les variations (accents, espaces, ordre)
2. ✅ **DateNaissance** : Ajouter pour éviter les jumeaux
3. ✅ **IdEcole** : Ajouter pour permettre les transferts
4. ✅ **Rétrocompatibilité** : Garder l'ancienne méthode comme fallback

**Critères finaux** :
```
(NomCompletEleve normalisé, DateNaissance, NomCompletTuteur normalisé, IdEcole)
```

---

**Version** : 1.0  
**Date** : 2025-01-16
