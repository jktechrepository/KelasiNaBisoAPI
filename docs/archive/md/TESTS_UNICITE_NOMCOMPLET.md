# 🧪 Tests : Critères d'Unicité par NomComplet

## 📋 Scénarios de Test

### Test 1 : Doublon Exact ✅

**Objectif** : Vérifier que le système détecte un doublon exact

**Données d'entrée** :
```json
{
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jean",
  "dateNaissanceEleve": "2010-05-15",
  "nomCompletTuteur": "MUKENDI Pierre",
  "idEcole": 1,
  "idClasse": 5,
  "idAnneeScolaire": 3
}
```

**Étape 1** : Créer la première inscription
- **Résultat attendu** : ✅ Inscription créée avec succès

**Étape 2** : Créer la deuxième inscription avec les mêmes données
- **Résultat attendu** : ✅ Élève existant réutilisé (pas de doublon créé)
- **Message attendu** : `"Inscription effectuée avec succès. Élève existant réutilisé (ID: X)"`

---

### Test 2 : Variations d'Espacement ✅

**Objectif** : Vérifier que le système détecte un doublon même avec des espaces multiples

**Données d'entrée (première inscription)** :
```json
{
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jean",
  "nomCompletTuteur": "MUKENDI Pierre"
}
```

**Données d'entrée (deuxième inscription - avec espaces multiples)** :
```json
{
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jean  Pierre",  // ⚠️ Espaces multiples dans le prénom
  "nomCompletTuteur": "MUKENDI  Pierre"  // ⚠️ Espaces multiples dans le tuteur
}
```

**Résultat attendu** : ✅ Détection du doublon (normalisation des espaces)

---

### Test 3 : Variations d'Accents ✅

**Objectif** : Vérifier que le système détecte un doublon même avec des accents différents

**Données d'entrée (première inscription)** :
```json
{
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "José",
  "nomCompletTuteur": "MUKENDI François"
}
```

**Données d'entrée (deuxième inscription - sans accents)** :
```json
{
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jose",  // ⚠️ Sans accent
  "nomCompletTuteur": "MUKENDI Francois"  // ⚠️ Sans accent
}
```

**Résultat attendu** : ✅ Détection du doublon (normalisation des accents)

---

### Test 4 : Variations de Caractères Spéciaux ✅

**Objectif** : Vérifier que le système détecte un doublon même avec des caractères spéciaux

**Données d'entrée (première inscription)** :
```json
{
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jean-Pierre",
  "nomCompletTuteur": "MUKENDI Pierre"
}
```

**Données d'entrée (deuxième inscription - sans tiret)** :
```json
{
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jean Pierre",  // ⚠️ Sans tiret
  "nomCompletTuteur": "MUKENDI Pierre"
}
```

**Résultat attendu** : ✅ Détection du doublon (normalisation des caractères spéciaux)

---

### Test 5 : Ordre des Mots ✅

**Objectif** : Vérifier que le système détecte un doublon même si l'ordre des mots est différent

**Données d'entrée (première inscription)** :
```json
{
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jean Pierre",
  "nomCompletTuteur": "MUKENDI Pierre"
}
```

**Données d'entrée (deuxième inscription - ordre différent)** :
```json
{
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Pierre Jean",  // ⚠️ Ordre différent
  "nomCompletTuteur": "Pierre MUKENDI"  // ⚠️ Ordre différent
}
```

**Résultat attendu** : ✅ Détection du doublon (tri des mots)

---

### Test 6 : Jumeaux (Dates Différentes) ✅

**Objectif** : Vérifier que le système permet l'inscription de jumeaux avec des dates de naissance différentes

**Données d'entrée (première inscription)** :
```json
{
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jean",
  "dateNaissanceEleve": "2010-05-15",
  "nomCompletTuteur": "MUKENDI Pierre"
}
```

**Données d'entrée (deuxième inscription - jumeau)** :
```json
{
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jean",
  "dateNaissanceEleve": "2010-05-16",  // ⚠️ Date différente (jumeau)
  "nomCompletTuteur": "MUKENDI Pierre"
}
```

**Résultat attendu** : ✅ Création de deux élèves distincts (dates différentes)

---

### Test 7 : Recherche Globale (Autre École) ✅

**Objectif** : Vérifier que le système détecte un doublon même si l'élève est dans une autre école

**Données d'entrée (première inscription - École 1)** :
```json
{
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jean",
  "dateNaissanceEleve": "2010-05-15",
  "nomCompletTuteur": "MUKENDI Pierre",
  "idEcole": 1
}
```

**Données d'entrée (deuxième inscription - École 2)** :
```json
{
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jean",
  "dateNaissanceEleve": "2010-05-15",
  "nomCompletTuteur": "MUKENDI Pierre",
  "idEcole": 2  // ⚠️ Autre école
}
```

**Résultat attendu** : ✅ Détection du doublon (recherche globale) et réutilisation de l'élève existant

---

### Test 8 : Tuteur Différent ❌

**Objectif** : Vérifier le comportement si le tuteur est différent

**Données d'entrée (première inscription)** :
```json
{
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jean",
  "dateNaissanceEleve": "2010-05-15",
  "nomCompletTuteur": "MUKENDI Pierre"
}
```

**Données d'entrée (deuxième inscription - tuteur différent)** :
```json
{
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jean",
  "dateNaissanceEleve": "2010-05-15",
  "nomCompletTuteur": "KALALA Marie"  // ⚠️ Tuteur différent
}
```

**Résultat attendu** : ⚠️ Création d'un nouvel élève (tuteur différent = élève différent selon les critères)

**Note** : Ce comportement peut être souhaité (changement de tuteur) ou non (même élève, tuteur différent). À discuter selon la logique métier.

---

## 🔍 Vérification des Logs

Lors de l'exécution des tests, vérifier les logs pour confirmer :

1. ✅ `"🔍 Recherche élève par NomComplet : ..."`
2. ✅ `"✅ Nom de l'élève correspond : ..."`
3. ✅ `"✅ Élève trouvé (critères d'unicité) : ..."`
4. ✅ `"✅ Élève existant réutilisé : ..."`

---

## 📊 Résultats Attendus

| Test | Scénario | Résultat Attendu | Statut |
|------|----------|------------------|--------|
| 1 | Doublon exact | ✅ Réutilisation | À tester |
| 2 | Espaces multiples | ✅ Détection | À tester |
| 3 | Accents | ✅ Détection | À tester |
| 4 | Caractères spéciaux | ✅ Détection | À tester |
| 5 | Ordre des mots | ✅ Détection | À tester |
| 6 | Jumeaux (dates différentes) | ✅ Création distincte | À tester |
| 7 | Recherche globale | ✅ Détection | À tester |
| 8 | Tuteur différent | ⚠️ Nouvel élève | À tester |

---

## 🚀 Instructions pour Tester

### Option 1 : Test Manuel via API

1. **Créer la première inscription** :
```bash
POST /api/Inscription
Authorization: Bearer {token}
Content-Type: application/json

{
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jean",
  "dateNaissanceEleve": "2010-05-15",
  "nomCompletTuteur": "MUKENDI Pierre",
  "idEcole": 1,
  "idClasse": 5,
  "idAnneeScolaire": 3
}
```

2. **Vérifier la réponse** :
- `success: true`
- `idEleve: X` (nouvel élève créé)

3. **Créer la deuxième inscription (doublon)** :
```bash
POST /api/Inscription
Authorization: Bearer {token}
Content-Type: application/json

{
  "nomEleve": "MUKENDI",
  "postnomEleve": "KALALA",
  "prenomEleve": "Jean",
  "dateNaissanceEleve": "2010-05-15",
  "nomCompletTuteur": "MUKENDI Pierre",
  "idEcole": 1,
  "idClasse": 5,
  "idAnneeScolaire": 3
}
```

4. **Vérifier la réponse** :
- `success: true`
- `idEleve: X` (même ID que la première inscription)
- `message: "Inscription effectuée avec succès. Élève existant réutilisé (ID: X)"`

---

### Option 2 : Test via Swagger

1. Ouvrir Swagger UI : `https://votre-api.com/swagger`
2. Naviguer vers `POST /api/Inscription`
3. Tester les différents scénarios ci-dessus

---

### Option 3 : Test Unitaires (À créer)

Créer un fichier de test unitaire pour automatiser les tests.

---

## 📝 Checklist de Test

- [ ] Test 1 : Doublon exact
- [ ] Test 2 : Variations d'espacement
- [ ] Test 3 : Variations d'accents
- [ ] Test 4 : Variations de caractères spéciaux
- [ ] Test 5 : Ordre des mots
- [ ] Test 6 : Jumeaux (dates différentes)
- [ ] Test 7 : Recherche globale (autre école)
- [ ] Test 8 : Tuteur différent

---

**Version** : 1.0  
**Date** : 2025-01-16
