# 📱 Analyse : Contenu de la Notification Push - Création de Devoir

**Date** : 2 décembre 2025  
**Fichier analysé** : `Controllers/DevoirADomicileController.cs` (méthode `EnvoyerNotificationsDevoirAuxParentsAsync`)

---

## 📋 Vue d'ensemble

Lors de la création d'un devoir à domicile, une notification push Firebase est envoyée à tous les parents (tuteurs) des élèves de la classe concernée.

**Méthode** : `EnvoyerNotificationsDevoirAuxParentsAsync` (lignes 754-1054)  
**Service utilisé** : `FirebaseNotificationService.EnvoyerNotificationAUtilisateurAsync`

---

## 🔍 Structure de la Notification Push

### 1. **Titre de la Notification**

```csharp
var titre = $"📚 Nouveau devoir - {nomClasse}";
```

**Exemple** :
```
📚 Nouveau devoir - 5ème A
```

**Caractéristiques** :
- ✅ Emoji 📚 pour identifier visuellement le type de notification
- ✅ Nom de la classe inclus
- ✅ Format court et clair

---

### 2. **Corps de la Notification (Body)**

```csharp
var corpsPush = $"Votre enfant {(parent.Enfants.Count == 1 ? nomsEnfants : "vos enfants")} a un nouveau devoir :\n" +
              $"📝 {devoir.Titre}\n" +
              $"👨‍🏫 Enseignant : {nomAgent}\n" +
              $"📅 Date limite : {dateLimiteStr}\n" +
              $"📎 Type : {typeDevoir}";
```

**Exemple complet** :
```
Votre enfant Jean KABONGO a un nouveau devoir :
📝 Exercices de Mathématiques - Chapitre 3
👨‍🏫 Enseignant : Pierre MULUMBA
📅 Date limite : 15/12/2025 à 18:00
📎 Type : 📎 Fichier (PDF)
```

**Détails du contenu** :

| Élément | Description | Format |
|--------|-------------|--------|
| **Message d'introduction** | Personnalisé selon le nombre d'enfants | "Votre enfant {nom}" ou "vos enfants" |
| **Titre du devoir** | Titre saisi par l'enseignant | Avec emoji 📝 |
| **Enseignant** | Nom complet de l'agent/enseignant | Format : "Prénom NOM" |
| **Date limite** | Date et heure de remise | Format : "dd/MM/yyyy à HH:mm" |
| **Type de devoir** | Type de contenu | "📎 Fichier (PDF)", "📝 Contenu textuel", ou "📄 Devoir" |

**Logique de détermination du type** :
```csharp
var typeDevoir = !string.IsNullOrWhiteSpace(devoir.NomFichier)
    ? $"📎 Fichier ({extensionFichier.ToUpper()})"
    : !string.IsNullOrWhiteSpace(devoir.Contenu)
        ? "📝 Contenu textuel"
        : "📄 Devoir";
```

---

### 3. **Données Additionnelles (Data Payload)**

```csharp
var donneesPush = new Dictionary<string, string>
{
    { "Type", "DevoirADomicile" },
    { "IdDevoir", devoir.IdDevoirADomicile.ToString() },
    { "IdClasse", devoir.IdClasse.ToString() },
    { "Titre", devoir.Titre },
    { "DateLimite", devoir.DateLimite?.ToString("yyyy-MM-ddTHH:mm:ss") ?? "" }
};
```

**Structure JSON** :
```json
{
  "Type": "DevoirADomicile",
  "IdDevoir": "42",
  "IdClasse": "15",
  "Titre": "Exercices de Mathématiques - Chapitre 3",
  "DateLimite": "2025-12-15T18:00:00"
}
```

**Utilisation** :
- ✅ Permet à l'application mobile de naviguer directement vers le devoir
- ✅ Identifie le type de notification pour le routage
- ✅ Contient les IDs nécessaires pour récupérer les détails complets
- ✅ Format ISO 8601 pour la date limite (facilite le parsing)

---

## 🔄 Flux d'Envoi

### Étape 1 : Récupération des Parents

```csharp
var parentIds = await dbContext.Utilisateurs
    .Where(u => u.IdTuteur != null 
        && u.Statut == true
        && dbContext.Eleves.Any(e => 
            e.IdTuteur == u.IdTuteur 
            && e.IdClasse == classe.IdClasse 
            && e.Statut == true))
    .Select(u => u.IdUtilisateur)
    .Distinct()
    .ToListAsync();
```

**Critères** :
- ✅ Utilisateur avec `IdTuteur` non null
- ✅ Utilisateur actif (`Statut == true`)
- ✅ Au moins un élève actif dans la classe concernée

---

### Étape 2 : Préparation des Messages

Pour chaque parent :
1. Récupération des enfants dans la classe
2. Construction du nom complet du parent
3. Génération des messages personnalisés (Push, SMS, Email)

---

### Étape 3 : Envoi de la Notification Push

```csharp
var result = await firebaseServiceToUse.EnvoyerNotificationAUtilisateurAsync(
    parent.IdUtilisateur,
    titre,
    corpsPush,
    donneesPush
);
```

**Service Firebase** : `FirebaseNotificationService.EnvoyerNotificationAUtilisateurAsync`

**Processus interne** :
1. Récupération de tous les tokens FCM actifs de l'utilisateur
2. Création d'un `MulticastMessage` avec :
   - Liste des tokens
   - Notification (titre + corps)
   - Data payload (données additionnelles)
3. Envoi via Firebase Admin SDK
4. Gestion des tokens invalides

---

## 📊 Format Final de la Notification Firebase

### Structure Firebase Cloud Messaging

```json
{
  "message": {
    "tokens": ["token1", "token2", ...],
    "notification": {
      "title": "📚 Nouveau devoir - 5ème A",
      "body": "Votre enfant Jean KABONGO a un nouveau devoir :\n📝 Exercices de Mathématiques - Chapitre 3\n👨‍🏫 Enseignant : Pierre MULUMBA\n📅 Date limite : 15/12/2025 à 18:00\n📎 Type : 📎 Fichier (PDF)"
    },
    "data": {
      "Type": "DevoirADomicile",
      "IdDevoir": "42",
      "IdClasse": "15",
      "Titre": "Exercices de Mathématiques - Chapitre 3",
      "DateLimite": "2025-12-15T18:00:00"
    }
  }
}
```

---

## 🎯 Points Clés

### ✅ Points Forts

1. **Personnalisation** : Message adapté selon le nombre d'enfants
2. **Informations complètes** : Titre, enseignant, date limite, type
3. **Emojis** : Améliore la lisibilité et l'identification visuelle
4. **Données structurées** : Payload JSON pour navigation dans l'app
5. **Envoi en parallèle** : Tous les parents reçoivent la notification simultanément

### ⚠️ Points d'Attention

1. **Longueur du message** : Le corps peut être long (limite Firebase ~4KB)
2. **Format de date** : Format français "dd/MM/yyyy à HH:mm" (peut nécessiter localisation)
3. **Gestion des erreurs** : Les échecs sont loggés mais n'empêchent pas l'envoi aux autres parents

---

## 🔧 Variables Utilisées

| Variable | Source | Description |
|----------|--------|-------------|
| `nomClasse` | `classe.NomClasse` | Nom de la classe |
| `devoir.Titre` | DTO de création | Titre du devoir |
| `nomAgent` | `GetAgentNomComplet(agent)` | Nom complet de l'enseignant |
| `dateLimiteStr` | `devoir.DateLimite` | Format : "dd/MM/yyyy à HH:mm" |
| `typeDevoir` | Logique basée sur `NomFichier` et `Contenu` | Type de contenu |
| `nomsEnfants` | Liste des élèves de la classe | Noms des enfants du parent |

---

## 📝 Exemple Complet

### Scénario
- **Classe** : 5ème A
- **Devoir** : "Exercices de Mathématiques - Chapitre 3"
- **Enseignant** : Pierre MULUMBA
- **Date limite** : 15 décembre 2025 à 18:00
- **Type** : Fichier PDF
- **Parent** : Jean KABONGO (1 enfant dans la classe)

### Notification Push

**Titre** :
```
📚 Nouveau devoir - 5ème A
```

**Corps** :
```
Votre enfant Jean KABONGO a un nouveau devoir :
📝 Exercices de Mathématiques - Chapitre 3
👨‍🏫 Enseignant : Pierre MULUMBA
📅 Date limite : 15/12/2025 à 18:00
📎 Type : 📎 Fichier (PDF)
```

**Data Payload** :
```json
{
  "Type": "DevoirADomicile",
  "IdDevoir": "42",
  "IdClasse": "15",
  "Titre": "Exercices de Mathématiques - Chapitre 3",
  "DateLimite": "2025-12-15T18:00:00"
}
```

---

## 🔗 Fichiers Concernés

1. **`Controllers/DevoirADomicileController.cs`**
   - Méthode : `EnvoyerNotificationsDevoirAuxParentsAsync` (lignes 754-1054)
   - Préparation du contenu de la notification

2. **`Services/FirebaseNotificationService.cs`**
   - Méthode : `EnvoyerNotificationAUtilisateurAsync` (lignes 60-150)
   - Envoi effectif via Firebase Cloud Messaging

3. **`Services/Repositories/IUserDeviceRepository.cs`**
   - Récupération des tokens FCM actifs par utilisateur

---

## 📌 Notes Techniques

- **Format de date** : ISO 8601 dans le data payload (`yyyy-MM-ddTHH:mm:ss`)
- **Format d'affichage** : Format français dans le corps (`dd/MM/yyyy à HH:mm`)
- **Gestion des nulls** : Vérifications pour éviter les erreurs (date limite, cours, etc.)
- **Performance** : Envoi en parallèle avec `Task.Run` pour chaque parent
- **Logging** : Logs détaillés pour chaque étape (succès/échec)

---

**Dernière mise à jour** : 2 décembre 2025

