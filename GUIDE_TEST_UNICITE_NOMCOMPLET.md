# 🧪 Guide de Test : Critères d'Unicité par NomComplet

## 🎯 Objectif

Tester l'implémentation des critères d'unicité : `(NomCompletEleve normalisé, DateNaissance, NomCompletTuteur normalisé)`

---

## 📋 Prérequis

1. ✅ API en cours d'exécution
2. ✅ Token JWT valide (obtenu via authentification)
3. ✅ École, Classe et Année scolaire existantes en base de données

---

## 🚀 Méthode 1 : Test via Fichier HTTP (Recommandé)

### Étape 1 : Ouvrir le fichier de test

Ouvrez le fichier : `test-unicite-nomcomplet.http` dans VS Code (avec l'extension REST Client)

### Étape 2 : Configurer le token

1. Exécutez d'abord la requête d'authentification (ÉTAPE 0)
2. Copiez le token JWT retourné
3. Remplacez `YOUR_JWT_TOKEN_HERE` par votre token dans la variable `@token`

### Étape 3 : Exécuter les tests

Exécutez les tests dans l'ordre (Test 1 → Test 8)

**Résultats attendus** :
- ✅ **Test 1-5** : Doublon détecté → Message : `"Élève existant réutilisé (ID: X)"`
- ✅ **Test 6** : Jumeaux → Deux élèves distincts créés
- ✅ **Test 7** : Recherche globale → Doublon détecté même dans autre école
- ⚠️ **Test 8** : Tuteur différent → Nouvel élève créé (comportement attendu)

---

## 🚀 Méthode 2 : Test Manuel via Swagger

### Étape 1 : Ouvrir Swagger

Accédez à : `https://votre-api.com/swagger`

### Étape 2 : Authentification

1. Appelez `POST /api/Utilisateur/authentifier`
2. Copiez le token JWT

### Étape 3 : Tester l'endpoint

1. Cliquez sur `POST /api/Inscription`
2. Cliquez sur "Authorize" et entrez : `Bearer {votre_token}`
3. Utilisez les exemples de données ci-dessous

---

## 📝 Exemples de Données de Test

### Test 1 : Doublon Exact

**Première inscription** :
```json
{
  "type": "Inscription",
  "idEcole": 1,
  "idClasse": 5,
  "idAnneeScolaire": 3,
  "dateInscription": "2025-01-16T10:00:00Z",
  "statutInscription": "En attente",
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jean",
  "genreEleve": "M",
  "dateNaissanceEleve": "2010-05-15T00:00:00Z",
  "lieuNaissanceEleve": "Kinshasa",
  "nationaliteEleve": "Congolaise",
  "nomCompletTuteur": "MUKENDI Pierre",
  "genreTuteur": "M"
}
```

**Deuxième inscription (DOUBLON)** :
```json
{
  "type": "Inscription",
  "idEcole": 1,
  "idClasse": 5,
  "idAnneeScolaire": 3,
  "dateInscription": "2025-01-16T10:00:00Z",
  "statutInscription": "En attente",
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jean",
  "genreEleve": "M",
  "dateNaissanceEleve": "2010-05-15T00:00:00Z",
  "lieuNaissanceEleve": "Kinshasa",
  "nationaliteEleve": "Congolaise",
  "nomCompletTuteur": "MUKENDI Pierre",
  "genreTuteur": "M"
}
```

**Résultat attendu** :
- ✅ Première inscription : `success: true`, `idEleve: X` (nouvel élève)
- ✅ Deuxième inscription : `success: true`, `idEleve: X` (même ID), `message: "Élève existant réutilisé"`

---

### Test 2 : Variations d'Espacement

**Première inscription** :
```json
{
  "prenomEleve": "Jean",
  "nomCompletTuteur": "MUKENDI Pierre"
}
```

**Deuxième inscription (ESPACES MULTIPLES)** :
```json
{
  "prenomEleve": "Jean  Pierre",
  "nomCompletTuteur": "MUKENDI  Pierre"
}
```

**Résultat attendu** : ✅ Doublon détecté

---

### Test 3 : Variations d'Accents

**Première inscription** :
```json
{
  "prenomEleve": "José",
  "nomCompletTuteur": "MUKENDI François"
}
```

**Deuxième inscription (SANS ACCENTS)** :
```json
{
  "prenomEleve": "Jose",
  "nomCompletTuteur": "MUKENDI Francois"
}
```

**Résultat attendu** : ✅ Doublon détecté

---

### Test 4 : Variations de Caractères Spéciaux

**Première inscription** :
```json
{
  "prenomEleve": "Jean-Pierre"
}
```

**Deuxième inscription (SANS TIRET)** :
```json
{
  "prenomEleve": "Jean Pierre"
}
```

**Résultat attendu** : ✅ Doublon détecté

---

### Test 5 : Ordre des Mots

**Première inscription** :
```json
{
  "prenomEleve": "Jean Pierre",
  "nomCompletTuteur": "MUKENDI Pierre"
}
```

**Deuxième inscription (ORDRE DIFFÉRENT)** :
```json
{
  "prenomEleve": "Pierre Jean",
  "nomCompletTuteur": "Pierre MUKENDI"
}
```

**Résultat attendu** : ✅ Doublon détecté (grâce au tri des mots)

---

### Test 6 : Jumeaux (Dates Différentes)

**Première inscription** :
```json
{
  "prenomEleve": "Jean",
  "dateNaissanceEleve": "2012-06-15T00:00:00Z",
  "nomCompletTuteur": "MUKENDI Pierre"
}
```

**Deuxième inscription (DATE DIFFÉRENTE)** :
```json
{
  "prenomEleve": "Jean",
  "dateNaissanceEleve": "2012-06-16T00:00:00Z",
  "nomCompletTuteur": "MUKENDI Pierre"
}
```

**Résultat attendu** : ✅ Deux élèves distincts créés (dates différentes)

---

### Test 7 : Recherche Globale (Autre École)

**Première inscription (École 1)** :
```json
{
  "idEcole": 1,
  "prenomEleve": "Marie",
  "dateNaissanceEleve": "2013-03-20T00:00:00Z",
  "nomCompletTuteur": "MUKENDI Pierre"
}
```

**Deuxième inscription (École 2)** :
```json
{
  "idEcole": 2,
  "prenomEleve": "Marie",
  "dateNaissanceEleve": "2013-03-20T00:00:00Z",
  "nomCompletTuteur": "MUKENDI Pierre"
}
```

**Résultat attendu** : ✅ Doublon détecté (recherche globale) et réutilisation de l'élève

---

### Test 8 : Tuteur Différent

**Première inscription** :
```json
{
  "prenomEleve": "Paul",
  "dateNaissanceEleve": "2014-04-10T00:00:00Z",
  "nomCompletTuteur": "MUKENDI Pierre"
}
```

**Deuxième inscription (TUTEUR DIFFÉRENT)** :
```json
{
  "prenomEleve": "Paul",
  "dateNaissanceEleve": "2014-04-10T00:00:00Z",
  "nomCompletTuteur": "KALALA Marie"
}
```

**Résultat attendu** : ⚠️ Nouvel élève créé (tuteur différent = élève différent selon les critères)

---

## 🔍 Vérification des Logs

Pendant les tests, vérifiez les logs de l'API pour voir :

1. ✅ `"🔍 Recherche élève par NomComplet : ..."`
2. ✅ `"✅ Nom de l'élève correspond : ..."`
3. ✅ `"✅ Élève trouvé (critères d'unicité) : ..."`
4. ✅ `"✅ Élève existant réutilisé : ..."`

---

## 📊 Tableau de Résultats

| Test | Scénario | Résultat Attendu | Comment Vérifier |
|------|----------|------------------|------------------|
| 1 | Doublon exact | ✅ Réutilisation | `idEleve` identique dans les 2 réponses |
| 2 | Espaces multiples | ✅ Détection | Message "Élève existant réutilisé" |
| 3 | Accents | ✅ Détection | Message "Élève existant réutilisé" |
| 4 | Caractères spéciaux | ✅ Détection | Message "Élève existant réutilisé" |
| 5 | Ordre des mots | ✅ Détection | Message "Élève existant réutilisé" |
| 6 | Jumeaux (dates différentes) | ✅ Création distincte | `idEleve` différents dans les 2 réponses |
| 7 | Recherche globale | ✅ Détection | Message "Élève existant réutilisé" même avec `idEcole` différent |
| 8 | Tuteur différent | ⚠️ Nouvel élève | `idEleve` différents (comportement attendu) |

---

## ✅ Checklist de Test

- [ ] Test 1 : Doublon exact → ✅ Réutilisation
- [ ] Test 2 : Espaces multiples → ✅ Détection
- [ ] Test 3 : Accents → ✅ Détection
- [ ] Test 4 : Caractères spéciaux → ✅ Détection
- [ ] Test 5 : Ordre des mots → ✅ Détection
- [ ] Test 6 : Jumeaux → ✅ Création distincte
- [ ] Test 7 : Recherche globale → ✅ Détection
- [ ] Test 8 : Tuteur différent → ⚠️ Nouvel élève

---

## 🐛 Dépannage

### Problème : "Token invalide"

**Solution** :
1. Vérifiez que vous êtes authentifié
2. Vérifiez que le token n'a pas expiré
3. Régénérez un nouveau token

---

### Problème : "École/Classe/Année scolaire non trouvée"

**Solution** :
1. Vérifiez que les IDs existent en base de données
2. Utilisez des IDs valides dans vos tests

---

### Problème : "Doublon non détecté"

**Vérifications** :
1. Vérifiez les logs de l'API pour voir si la recherche est effectuée
2. Vérifiez que les dates de naissance sont exactement identiques
3. Vérifiez que les noms complets sont bien normalisés

---

## 📝 Notes Importantes

1. **Dates** : Les dates doivent être exactement identiques (même jour, même mois, même année)
2. **Normalisation** : Les variations (accents, espaces, ordre) sont gérées automatiquement
3. **Recherche globale** : La recherche se fait sur toutes les écoles (pas de filtre par école)
4. **Tuteur différent** : Si le tuteur est différent, un nouvel élève sera créé (comportement attendu)

---

**Version** : 1.0  
**Date** : 2025-01-16
