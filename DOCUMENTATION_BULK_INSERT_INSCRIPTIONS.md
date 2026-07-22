# 📚 Documentation : Module Bulk Insert pour les Inscriptions

## 📋 Table des Matières

1. [Vue d'ensemble](#vue-densemble)
2. [Architecture](#architecture)
3. [Format Excel](#format-excel)
4. [Endpoints API](#endpoints-api)
5. [Flux de Traitement](#flux-de-traitement)
6. [Gestion des Erreurs](#gestion-des-erreurs)
7. [Optimisations](#optimisations)
8. [Exemples d'Utilisation](#exemples-dutilisation)
9. [Cas d'Usage](#cas-dusage)
10. [Dépannage](#dépannage)

---

## 🎯 Vue d'ensemble

Le module **Bulk Insert pour les Inscriptions** permet d'importer en masse des inscriptions d'élèves depuis un fichier Excel. Ce module est optimisé pour traiter des centaines d'inscriptions en une seule requête, avec validation automatique, gestion des erreurs et sauvegarde transactionnelle.

### Caractéristiques Principales

- ✅ **Import en masse** : Traite jusqu'à plusieurs centaines d'inscriptions en une seule requête
- ✅ **Format simplifié** : Utilise des noms (élève, tuteur) au lieu d'IDs
- ✅ **Validation robuste** : Validation automatique de toutes les données
- ✅ **Gestion des erreurs** : Rapport détaillé des lignes en erreur
- ✅ **Déduplication** : Détection automatique des doublons
- ✅ **Traitement par lots** : Traite 50 inscriptions à la fois pour optimiser les performances
- ✅ **Template Excel** : Génération automatique d'un template pour faciliter la saisie

### Fichiers Concernés

- **Service** : `Services/ExcelInscriptionServiceV2.cs`
- **Contrôleur** : `Controllers/InscriptionController.cs`
- **DTOs** : `Models/DTOs/InscriptionExcelDto.cs`

---

## 🏗️ Architecture

### Structure du Module

```
ExcelInscriptionServiceV2
├── ProcessExcelFileAsync()          // Point d'entrée principal
├── ValidateFile()                   // Validation du fichier
├── ReadExcelFileAsync()             // Lecture du fichier Excel
├── ConvertToInscriptionExcelDto()   // Conversion et enrichissement
├── ValidateInscriptions()          // Validation des données
├── DeduplicateInFile()             // Détection des doublons
└── ProcessBatchesAsync()           // Traitement par lots
```

### Flux de Données

```
Fichier Excel (.xlsx)
    ↓
ReadExcelFileAsync()
    ↓
InscriptionExcelRaw[] (données brutes)
    ↓
ConvertToInscriptionExcelDto()
    ↓
InscriptionExcelDto[] (enrichi avec IDs)
    ↓
ValidateInscriptions()
    ↓
DeduplicateInFile()
    ↓
Séparation : Lignes valides / Lignes invalides
    ↓
ProcessBatchesAsync() (par lots de 50)
    ↓
CreateInscriptionAsync() (pour chaque inscription)
    ↓
BulkInscriptionResult (résultat final)
```

---

## 📊 Format Excel

### Colonnes Requises

Le fichier Excel doit contenir **obligatoirement** les colonnes suivantes :

| Colonne | Type | Obligatoire | Description | Exemple |
|---------|------|-------------|-------------|---------|
| `DateInscription` | Date | ✅ | Date d'inscription de l'élève | `2025-01-15` |
| `NomEleve` | Texte | ✅ | Nom de famille de l'élève | `MUKENDI` |
| `PostnomEleve` | Texte | ✅ | Postnom de l'élève | `KALALA` |
| `PrenomEleve` | Texte | ✅ | Prénom de l'élève | `Jean` |
| `GenreEleve` | Texte | ✅ | Genre (M ou F) | `M` |
| `DateNaissanceEleve` | Date | ✅ | Date de naissance | `2010-05-15` |
| `LieuNaissanceEleve` | Texte | ✅ | Lieu de naissance | `Kinshasa` |
| `NationaliteEleve` | Texte | ✅ | Nationalité | `Congolaise` |
| `NomCompletTuteur` | Texte | ✅ | Nom complet du tuteur | `MUKENDI Pierre` |
| `GenreTuteur` | Texte | ✅ | Genre du tuteur (M ou F) | `M` |

### Colonnes Optionnelles

| Colonne | Type | Description | Exemple |
|---------|------|-------------|---------|
| `ProvinceEleve` | Texte | Province de l'élève | `Kinshasa` |
| `VilleEleve` | Texte | Ville de l'élève | `Kinshasa` |
| `CommuneEleve` | Texte | Commune de l'élève | `Gombe` |
| `QuartierEleve` | Texte | Quartier de l'élève | `Centre-ville` |
| `AvenueEleve` | Texte | Avenue de l'élève | `Avenue X` |
| `NumeroEleve` | Texte | Numéro de l'adresse | `123` |
| `CommentaireEleve` | Texte | Commentaire sur l'élève | `Allergie aux arachides` |
| `PhotoEleveUrl` | Texte | URL de la photo | `https://...` |
| `MatriculeEleve` | Texte | Matricule (si déjà attribué) | `ECAM2025-001` |
| `EmailTuteur` | Texte | Email du tuteur | `pierre.mukendi@email.com` |
| `TelephoneTuteur` | Texte | Téléphone du tuteur | `+243900000000` |
| `NomCompletRepresentant` | Texte | Nom du représentant légal | `MUKENDI Marie` |
| `TelephoneRepresentant` | Texte | Téléphone du représentant | `+243900000001` |

### Exemple de Fichier Excel

| DateInscription | NomEleve | PostnomEleve | PrenomEleve | GenreEleve | DateNaissanceEleve | LieuNaissanceEleve | NationaliteEleve | NomCompletTuteur | GenreTuteur | EmailTuteur | TelephoneTuteur |
|-----------------|----------|--------------|-------------|------------|-------------------|-------------------|------------------|-----------------|-------------|-------------|-----------------|
| 2025-01-15 | MUKENDI | KALALA | Jean | M | 2010-05-15 | Kinshasa | Congolaise | MUKENDI Pierre | M | pierre@email.com | +243900000000 |
| 2025-01-15 | KALALA | MUKENDI | Marie | F | 2011-03-20 | Kinshasa | Congolaise | KALALA Paul | M | paul@email.com | +243900000001 |

---

## 🔌 Endpoints API

### 1. Télécharger le Template Excel

**Endpoint** : `GET /api/Inscription/template-excel`

**Authentification** : Requise (JWT Token)

**Autorisation** : `Admin`, `Super-Admin`, `Directeur`

**Réponse** : Fichier Excel (.xlsx)

**Exemple de requête** :
```http
GET /api/Inscription/template-excel
Authorization: Bearer {token}
```

**Réponse** :
- **Content-Type** : `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- **Nom du fichier** : `Template_Inscriptions_20250116.xlsx`

---

### 2. Importer les Inscriptions en Lot

**Endpoint** : `POST /api/Inscription/bulk-excel`

**Authentification** : Requise (JWT Token)

**Autorisation** : `Admin`, `Super-Admin`, `Directeur`

**Content-Type** : `multipart/form-data`

**Paramètres de Requête (Query Parameters)** :

| Paramètre | Type | Obligatoire | Description |
|-----------|------|-------------|-------------|
| `idClasse` | int | ✅ | ID de la classe dans laquelle inscrire les élèves |
| `idAnneeScolaire` | int | ✅ | ID de l'année scolaire |
| `typeInscription` | string | ❌ | Type d'inscription (`"Inscription"`, `"Réinscription"`, `"Transfert"`). Par défaut : `"Inscription"` |

**Body (FormData)** :

| Champ | Type | Obligatoire | Description |
|-------|------|-------------|-------------|
| `file` | File | ✅ | Fichier Excel (.xlsx) contenant les inscriptions |

**Exemple de requête** :
```http
POST /api/Inscription/bulk-excel?idClasse=5&idAnneeScolaire=3&typeInscription=Inscription
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: [fichier Excel]
```

**Réponse Succès (200 OK)** :
```json
{
  "success": true,
  "message": "Traitement terminé : 45 inscription(s) réussie(s) sur 50 ligne(s), 5 échouée(s)",
  "totalLignes": 50,
  "lignesReussies": 45,
  "lignesEchouees": 5,
  "doublonsDetectes": 2,
  "lignesAvecErreurs": [
    {
      "numeroLigne": 12,
      "nomEleve": "MUKENDI",
      "postnomEleve": "KALALA",
      "prenomEleve": "Jean",
      "erreurs": [
        "Le genre de l'élève doit être M ou F",
        "La date de naissance de l'élève est obligatoire"
      ]
    }
  ],
  "inscriptionsCrees": [
    {
      "success": true,
      "idInscription": 123,
      "idEleve": 456,
      "idTuteur": 789,
      "message": "Inscription créée avec succès"
    }
  ],
  "dateTraitement": "2025-01-16T10:30:00Z"
}
```

**Réponse Erreur (400 Bad Request)** :
```json
{
  "success": false,
  "message": "Le fichier Excel est vide ou ne contient pas de données valides.",
  "totalLignes": 0,
  "lignesReussies": 0,
  "lignesEchouees": 0,
  "doublonsDetectes": 0,
  "lignesAvecErreurs": [],
  "inscriptionsCrees": [],
  "dateTraitement": "2025-01-16T10:30:00Z"
}
```

---

## 🔄 Flux de Traitement

### Étape 1 : Validation du Fichier

**Fichier** : `Services/ExcelInscriptionServiceV2.cs` - Méthode `ValidateFile()`

**Validations** :
- ✅ Fichier non vide
- ✅ Taille maximale : 10 MB
- ✅ Format : `.xlsx` uniquement

**Erreurs possibles** :
- `"Le fichier est vide ou n'a pas été fourni"`
- `"Le fichier dépasse la taille maximale autorisée (10 MB)"`
- `"Le fichier doit être au format .xlsx (Excel 2007+)"`

---

### Étape 2 : Validation des Paramètres

**Validations** :
- ✅ La classe existe et appartient à l'école de l'utilisateur
- ✅ L'année scolaire existe et appartient à l'école de l'utilisateur
- ✅ Le type d'inscription est valide (`"Inscription"`, `"Réinscription"`, `"Transfert"`)

**Erreurs possibles** :
- `"La classe {idClasse} n'existe pas ou n'appartient pas à l'école {idEcole}"`
- `"L'année scolaire {idAnneeScolaire} n'existe pas ou n'appartient pas à l'école {idEcole}"`
- `"Type d'inscription invalide : '{typeInscription}'. Types valides : Inscription, Réinscription, Transfert"`

---

### Étape 3 : Lecture du Fichier Excel

**Fichier** : `Services/ExcelInscriptionServiceV2.cs` - Méthode `ReadExcelFileAsync()`

**Processus** :
1. Lecture de la première feuille du classeur Excel
2. Détection des en-têtes (ligne 1)
3. Vérification que toutes les colonnes requises sont présentes
4. Lecture des données ligne par ligne (lignes 2 à N)
5. Conversion en objets `InscriptionExcelRaw`

**Erreurs possibles** :
- `"Le fichier Excel ne contient aucune feuille de calcul"`
- `"La feuille de calcul est vide"`
- `"Colonnes manquantes dans le fichier Excel : {colonnes}"`

---

### Étape 4 : Conversion et Enrichissement

**Fichier** : `Services/ExcelInscriptionServiceV2.cs` - Méthode `ConvertToInscriptionExcelDto()`

**Processus** :
1. Conversion de `InscriptionExcelRaw` → `InscriptionExcelDto`
2. Enrichissement avec les IDs fournis :
   - `IdEcole` : Extrait du token JWT
   - `IdClasse` : Fourni en paramètre
   - `IdAnneeScolaire` : Fourni en paramètre
   - `Type` : Fourni en paramètre
3. Valeurs par défaut :
   - `DateInscription` : Date actuelle si non fournie
   - `StatutInscription` : `"En attente"`

---

### Étape 5 : Validation des Données

**Fichier** : `Services/ExcelInscriptionServiceV2.cs` - Méthode `ValidateInscriptions()`

**Validations par ligne** :

| Champ | Règle | Message d'erreur |
|-------|-------|------------------|
| `NomEleve` | Non vide | `"Le nom de l'élève est obligatoire"` |
| `PostnomEleve` | Non vide | `"Le postnom de l'élève est obligatoire"` |
| `PrenomEleve` | Non vide | `"Le prénom de l'élève est obligatoire"` |
| `GenreEleve` | M ou F | `"Le genre de l'élève doit être M ou F"` |
| `DateNaissanceEleve` | Non null | `"La date de naissance de l'élève est obligatoire"` |
| `LieuNaissanceEleve` | Non vide | `"Le lieu de naissance de l'élève est obligatoire"` |
| `NationaliteEleve` | Non vide | `"La nationalité de l'élève est obligatoire"` |
| `NomCompletTuteur` | Non vide | `"Le nom complet du tuteur est obligatoire"` |
| `GenreTuteur` | M ou F | `"Le genre du tuteur doit être M ou F"` |

**Résultat** : Chaque ligne avec erreurs a sa liste `Erreurs` remplie

---

### Étape 6 : Déduplication

**Fichier** : `Services/ExcelInscriptionServiceV2.cs` - Méthode `DeduplicateInFile()`

**Processus** :
1. Création d'une clé unique par élève : `"{NomEleve}_{PostnomEleve}_{PrenomEleve}_{DateNaissanceEleve:yyyy-MM-dd}_{IdClasse}"`
2. Détection des doublons dans le fichier
3. Marquage des doublons avec l'erreur : `"Doublon détecté dans le fichier"`

**Note** : La déduplication avec la base de données est gérée par `CreateInscriptionAsync()` qui vérifie si l'élève existe déjà.

---

### Étape 7 : Séparation des Lignes

**Processus** :
- **Lignes valides** : `Erreurs.Count == 0`
- **Lignes invalides** : `Erreurs.Count > 0`

Les lignes invalides sont ajoutées à `result.LignesAvecErreurs` et ne seront pas traitées.

---

### Étape 8 : Traitement par Lots

**Fichier** : `Services/ExcelInscriptionServiceV2.cs` - Méthode `ProcessBatchesAsync()`

**Processus** :
1. Division des lignes valides en lots de **50 inscriptions**
2. Pour chaque lot :
   - Pour chaque inscription :
     - Conversion en `CreateInscriptionDto`
     - Génération du matricule si nécessaire
     - Appel à `CreateInscriptionAsync()` (gère sa propre transaction)
     - Ajout au résultat si succès
     - Ajout aux erreurs si échec

**Note** : Chaque inscription est traitée individuellement car `CreateInscriptionAsync()` gère déjà ses propres transactions et la logique métier complexe (création élève, tuteur, utilisateur parent, etc.).

---

### Étape 9 : Génération du Résultat

**Fichier** : `Services/ExcelInscriptionServiceV2.cs` - Méthode `GenerateResultMessage()`

**Messages possibles** :
- ✅ **Tous réussis** : `"Traitement terminé : {X} inscription(s) réussie(s) sur {Y} ligne(s)"`
- ❌ **Tous échoués** : `"Aucune inscription créée : {X} erreur(s) sur {Y} ligne(s)"`
- ⚠️ **Partiel** : `"Traitement terminé : {X} inscription(s) réussie(s) sur {Y} ligne(s), {Z} échouée(s)"`

---

## ⚠️ Gestion des Erreurs

### Types d'Erreurs

#### 1. Erreurs de Fichier

| Erreur | Cause | Solution |
|--------|-------|----------|
| Fichier vide | Aucun fichier fourni | Vérifier que le fichier est bien attaché |
| Format invalide | Fichier n'est pas .xlsx | Convertir en format Excel 2007+ |
| Taille excessive | Fichier > 10 MB | Réduire le nombre de lignes ou compresser |

#### 2. Erreurs de Colonnes

| Erreur | Cause | Solution |
|--------|-------|----------|
| Colonnes manquantes | Colonnes requises absentes | Utiliser le template Excel fourni |
| Nom de colonne incorrect | Faute de frappe dans l'en-tête | Vérifier l'orthographe exacte |

#### 3. Erreurs de Validation

| Erreur | Cause | Solution |
|--------|-------|----------|
| Champ obligatoire manquant | Valeur vide dans une colonne requise | Remplir toutes les colonnes obligatoires |
| Format invalide | Genre ≠ M ou F, date invalide, etc. | Vérifier le format des données |
| Doublon dans le fichier | Même élève plusieurs fois | Supprimer les lignes en double |

#### 4. Erreurs Métier

| Erreur | Cause | Solution |
|--------|-------|----------|
| Élève existe déjà | L'élève est déjà inscrit | Utiliser le type "Réinscription" ou corriger les données |
| Classe invalide | La classe n'appartient pas à l'école | Vérifier l'ID de la classe |
| Année scolaire invalide | L'année scolaire n'appartient pas à l'école | Vérifier l'ID de l'année scolaire |

---

## 🚀 Optimisations

### 1. Chargement en Mémoire

**Avant** : Requêtes à la base de données pour chaque ligne  
**Après** : Chargement unique de toutes les classes et années scolaires en mémoire

**Impact** : Réduction drastique du nombre de requêtes SQL

### 2. Traitement par Lots

**Taille du lot** : 50 inscriptions

**Avantages** :
- ✅ Évite les timeouts
- ✅ Permet de traiter partiellement même en cas d'erreur
- ✅ Meilleure gestion mémoire

### 3. Validation Précoce

**Processus** :
1. Validation du fichier avant traitement
2. Validation des données avant insertion
3. Séparation des lignes valides/invalides

**Avantages** :
- ✅ Évite les insertions inutiles
- ✅ Rapport d'erreurs complet avant traitement
- ✅ Meilleure expérience utilisateur

---

## 💡 Exemples d'Utilisation

### Exemple 1 : Import Simple (JavaScript/Fetch)

```javascript
async function importerInscriptions(file, idClasse, idAnneeScolaire, typeInscription = "Inscription") {
    const formData = new FormData();
    formData.append('file', file);
    
    const token = localStorage.getItem('token');
    
    const response = await fetch(
        `/api/Inscription/bulk-excel?idClasse=${idClasse}&idAnneeScolaire=${idAnneeScolaire}&typeInscription=${typeInscription}`,
        {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`
            },
            body: formData
        }
    );
    
    const result = await response.json();
    
    if (result.success) {
        console.log(`✅ ${result.lignesReussies} inscriptions créées`);
        if (result.lignesEchouees > 0) {
            console.warn(`⚠️ ${result.lignesEchouees} lignes en erreur`);
            result.lignesAvecErreurs.forEach(ligne => {
                console.error(`Ligne ${ligne.numeroLigne}:`, ligne.erreurs);
            });
        }
    } else {
        console.error('❌ Erreur:', result.message);
    }
    
    return result;
}
```

### Exemple 2 : Téléchargement du Template

```javascript
async function telechargerTemplate() {
    const token = localStorage.getItem('token');
    
    const response = await fetch('/api/Inscription/template-excel', {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Template_Inscriptions_${new Date().toISOString().split('T')[0]}.xlsx`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
}
```

### Exemple 3 : Affichage des Résultats

```javascript
function afficherResultats(result) {
    const stats = `
        Total lignes : ${result.totalLignes}
        ✅ Réussies : ${result.lignesReussies}
        ❌ Échouées : ${result.lignesEchouees}
        🔄 Doublons : ${result.doublonsDetectes}
    `;
    
    console.log(stats);
    
    if (result.lignesAvecErreurs.length > 0) {
        console.table(result.lignesAvecErreurs.map(l => ({
            Ligne: l.numeroLigne,
            Élève: `${l.prenomEleve} ${l.nomEleve}`,
            Erreurs: l.erreurs.join(', ')
        })));
    }
}
```

---

## 📝 Cas d'Usage

### Cas 1 : Inscription de Nouveaux Élèves

**Scénario** : Inscrire 100 nouveaux élèves en début d'année scolaire

**Processus** :
1. Télécharger le template Excel
2. Remplir les 100 lignes avec les données des élèves
3. Uploader le fichier avec `typeInscription="Inscription"`
4. Vérifier le rapport d'erreurs
5. Corriger les erreurs si nécessaire et réimporter

**Temps estimé** : ~5-10 minutes (au lieu de plusieurs heures)

---

### Cas 2 : Réinscription d'Élèves Existants

**Scénario** : Réinscrire 50 élèves existants pour la nouvelle année scolaire

**Processus** :
1. Télécharger le template Excel
2. Remplir les données (le système détectera automatiquement les élèves existants)
3. Uploader avec `typeInscription="Réinscription"`
4. Le système réutilisera les élèves et tuteurs existants

**Avantage** : Pas de duplication de données

---

### Cas 3 : Import avec Erreurs

**Scénario** : Fichier avec 50 lignes, dont 5 en erreur

**Résultat** :
- ✅ 45 inscriptions créées avec succès
- ❌ 5 lignes en erreur avec détails
- 📊 Rapport complet avec numéros de ligne et erreurs

**Action** : Corriger les 5 lignes et réimporter uniquement ces lignes

---

## 🔧 Dépannage

### Problème : "Colonnes manquantes"

**Cause** : Les en-têtes du fichier Excel ne correspondent pas exactement aux noms attendus

**Solution** :
1. Télécharger le template Excel officiel
2. Vérifier l'orthographe exacte des colonnes (sensible à la casse)
3. Ne pas modifier les noms des colonnes

---

### Problème : "Tous les élèves sont en erreur"

**Cause** : Format de données incorrect (dates, genres, etc.)

**Solution** :
1. Vérifier le format des dates (YYYY-MM-DD)
2. Vérifier que les genres sont exactement "M" ou "F"
3. Vérifier que tous les champs obligatoires sont remplis

---

### Problème : "Doublons détectés"

**Cause** : Plusieurs lignes avec le même élève dans le fichier

**Solution** :
1. Supprimer les lignes en double dans le fichier Excel
2. Vérifier que chaque élève n'apparaît qu'une seule fois

---

### Problème : "La classe n'existe pas"

**Cause** : L'ID de la classe fourni n'appartient pas à l'école de l'utilisateur

**Solution** :
1. Vérifier que l'ID de la classe est correct
2. Vérifier que la classe appartient bien à l'école de l'utilisateur connecté

---

## 📊 Statistiques et Performances

### Performances Typiques

| Nombre de Lignes | Temps de Traitement | Mémoire Utilisée |
|------------------|---------------------|------------------|
| 50 inscriptions | ~10-15 secondes | ~50 MB |
| 100 inscriptions | ~20-30 secondes | ~80 MB |
| 200 inscriptions | ~40-60 secondes | ~120 MB |
| 500 inscriptions | ~2-3 minutes | ~200 MB |

### Limitations

- ⚠️ **Taille maximale du fichier** : 10 MB
- ⚠️ **Format requis** : `.xlsx` uniquement
- ⚠️ **Taille du lot** : 50 inscriptions par lot (fixe)

---

## 🔐 Sécurité

### Authentification

- ✅ **JWT Token requis** : Toutes les requêtes nécessitent un token valide
- ✅ **Extraction automatique** : `IdEcole` et `IdUtilisateur` extraits du token (pas de manipulation possible)

### Autorisation

- ✅ **Rôles autorisés** : `Admin`, `Super-Admin`, `Directeur` uniquement
- ✅ **Validation école** : Les classes et années scolaires doivent appartenir à l'école de l'utilisateur

### Validation

- ✅ **Validation côté serveur** : Toutes les données sont validées avant insertion
- ✅ **Pas d'injection SQL** : Utilisation d'Entity Framework Core (paramétré)

---

## 📌 Notes Importantes

1. **Format des Dates** : Les dates doivent être au format Excel standard (YYYY-MM-DD ou format Excel natif)
2. **Genres** : Utiliser exactement "M" ou "F" (majuscule)
3. **Matricules** : Si non fourni, le système génère automatiquement un matricule unique
4. **Tuteurs** : Si un tuteur existe déjà (même nom complet), il sera réutilisé automatiquement
5. **Élèves** : Pour les réinscriptions, le système détecte automatiquement les élèves existants

---

**Version** : 2.0  
**Date de création** : 2025-01-16  
**Dernière mise à jour** : 2025-01-16
