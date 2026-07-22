# 📋 Guide de Test : Bulk Insert depuis Excel

**Date** : 1er décembre 2025

---

## 🎯 Objectifs des Tests

1. ✅ Tester le téléchargement du template Excel
2. ✅ Tester l'upload d'un fichier Excel valide
3. ✅ Tester la validation des données (erreurs)
4. ✅ Tester la détection des doublons
5. ✅ Tester le traitement par lots
6. ✅ Vérifier le rapport détaillé

---

## 📦 Prérequis

### 1. Outils Nécessaires

- **curl** : Pour les appels API
- **jq** (optionnel) : Pour formater les réponses JSON
  ```bash
  # macOS
  brew install jq
  
  # Linux
  apt-get install jq
  ```

- **Python 3** (optionnel) : Pour créer un fichier Excel de test
  ```bash
  pip install openpyxl
  ```

### 2. Configuration

- L'application doit être démarrée
- Avoir un compte avec les permissions (Admin, Super-Admin, ou Directeur)
- Avoir des données de base (École, Classe, Année Scolaire) existantes

---

## 🚀 Tests Disponibles

### Test 1 : Script Bash Automatique

**Fichier** : `test-bulk-insert-excel.sh`

**Utilisation** :
```bash
# Configuration par défaut
./test-bulk-insert-excel.sh

# Avec variables d'environnement
BASE_URL="https://localhost:7102" \
EMAIL="admin@kelasinabiso.cd" \
PASSWORD="12345678" \
NOM_ECOLE="Ecole Test" \
./test-bulk-insert-excel.sh
```

**Ce que fait le script** :
1. ✅ Vérifie que l'API est accessible
2. ✅ S'authentifie avec les credentials
3. ✅ Télécharge le template Excel
4. ✅ Crée un fichier de test
5. ✅ Upload le fichier Excel
6. ✅ Affiche les résultats détaillés

---

### Test 2 : Créer un Fichier Excel de Test avec Python

**Fichier** : `test-bulk-insert-excel-python.py`

**Utilisation** :
```bash
# Installer openpyxl
pip install openpyxl

# Créer le fichier de test
python3 test-bulk-insert-excel-python.py test_inscriptions.xlsx
```

**Contenu du fichier généré** :
- ✅ **2 lignes valides** (lignes 2-3)
- ❌ **1 ligne avec nom manquant** (ligne 4)
- ❌ **1 ligne avec genre invalide** (ligne 5)
- ❌ **1 ligne avec email invalide** (ligne 6)
- ❌ **1 doublon** (ligne 7, même élève que ligne 2)

---

## 📝 Tests Manuels

### Test 1 : Télécharger le Template

```bash
# 1. Authentification
TOKEN=$(curl -k -s -X POST "https://localhost:7102/api/Utilisateur/Authentifier" \
  -H "Content-Type: application/json" \
  -d '{"emailOuTelephone":"admin@kelasinabiso.cd","motDePasse":"12345678"}' \
  | jq -r '.token')

# 2. Télécharger le template
curl -k -X GET "https://localhost:7102/api/Inscription/template-excel" \
  -H "Authorization: Bearer ${TOKEN}" \
  -o template_inscriptions.xlsx
```

**Résultat attendu** : Fichier `template_inscriptions.xlsx` téléchargé

---

### Test 2 : Upload d'un Fichier Valide

```bash
# Uploader le fichier
curl -k -X POST "https://localhost:7102/api/Inscription/bulk-excel/Ecole%20Test" \
  -H "Authorization: Bearer ${TOKEN}" \
  -F "file=@test_inscriptions.xlsx" \
  | jq '.'
```

**Résultat attendu** :
```json
{
  "success": true,
  "message": "Traitement terminé : 2 inscription(s) réussie(s) sur 6 ligne(s), 4 échouée(s), 1 doublon(s) détecté(s)",
  "totalLignes": 6,
  "lignesReussies": 2,
  "lignesEchouees": 4,
  "doublonsDetectes": 1,
  "lignesAvecErreurs": [
    {
      "numeroLigne": 4,
      "erreurs": ["Le nom de l'élève est obligatoire"]
    },
    {
      "numeroLigne": 5,
      "erreurs": ["Le genre de l'élève doit être 'M' ou 'F'"]
    },
    {
      "numeroLigne": 6,
      "erreurs": ["L'email du tuteur n'est pas valide"]
    },
    {
      "numeroLigne": 7,
      "erreurs": ["Doublon détecté dans le fichier Excel (même élève présent plusieurs fois)"]
    }
  ],
  "inscriptionsCrees": [...],
  "dateTraitement": "2025-12-01T10:30:00Z"
}
```

---

### Test 3 : Vérifier les Erreurs de Validation

Le fichier de test contient plusieurs types d'erreurs :

1. **Nom manquant** (ligne 4)
   - Erreur attendue : "Le nom de l'élève est obligatoire"

2. **Genre invalide** (ligne 5)
   - Erreur attendue : "Le genre de l'élève doit être 'M' ou 'F'"

3. **Email invalide** (ligne 6)
   - Erreur attendue : "L'email du tuteur n'est pas valide"

4. **Doublon** (ligne 7)
   - Erreur attendue : "Doublon détecté dans le fichier Excel"

---

## 🔍 Vérifications à Effectuer

### 1. Validation du Fichier

- ✅ Fichier .xlsx accepté
- ✅ Fichier .xls accepté
- ❌ Fichier .pdf rejeté
- ❌ Fichier > 10 MB rejeté

### 2. Validation des Colonnes

- ✅ Toutes les colonnes requises présentes
- ❌ Colonnes manquantes → Erreur claire

### 3. Validation des Données

- ✅ Données valides → Inscription créée
- ❌ Champs obligatoires manquants → Erreur
- ❌ Types invalides → Erreur
- ❌ Formats invalides (email, téléphone) → Erreur

### 4. Déduplication

- ✅ Doublons dans le fichier détectés
- ✅ Doublons rejetés avec message

### 5. Traitement par Lots

- ✅ 50 inscriptions par lot
- ✅ Transaction par lot (rollback si échec)
- ✅ Autres lots continuent même si un lot échoue

### 6. Rapport Détaillé

- ✅ Nombre total de lignes
- ✅ Nombre de lignes réussies
- ✅ Nombre de lignes échouées
- ✅ Liste des erreurs avec détails
- ✅ Liste des inscriptions créées

---

## ⚠️ Points d'Attention

### 1. Données de Base Requises

Avant de tester, assurez-vous que :
- ✅ Une école existe avec `IdEcole = 1` (ou ajustez dans le fichier Excel)
- ✅ Une classe existe avec `IdClasse = 1` (ou ajustez dans le fichier Excel)
- ✅ Une année scolaire existe avec `IdAnneeScolaire = 1` (ou ajustez dans le fichier Excel)

### 2. Permissions

- ✅ Seuls les rôles `Admin`, `Super-Admin`, et `Directeur` peuvent utiliser cette fonctionnalité
- ✅ Vérifiez que votre compte a les bonnes permissions

### 3. Taille du Fichier

- ✅ Maximum : 10 MB
- ✅ Recommandé : < 1000 lignes pour de meilleures performances

---

## 📊 Scénarios de Test Recommandés

### Scénario 1 : Fichier Valide (10 inscriptions)

**Objectif** : Vérifier que les inscriptions valides sont créées

**Résultat attendu** :
- ✅ 10 inscriptions créées
- ✅ 0 erreur
- ✅ Rapport de succès

---

### Scénario 2 : Fichier avec Erreurs

**Objectif** : Vérifier la validation et le rapport d'erreurs

**Résultat attendu** :
- ✅ Inscriptions valides créées
- ✅ Erreurs détectées et rapportées
- ✅ Rapport détaillé avec toutes les erreurs

---

### Scénario 3 : Fichier avec Doublons

**Objectif** : Vérifier la détection des doublons

**Résultat attendu** :
- ✅ Doublons détectés
- ✅ Doublons rejetés
- ✅ Message clair dans le rapport

---

### Scénario 4 : Gros Fichier (100+ inscriptions)

**Objectif** : Vérifier les performances et le traitement par lots

**Résultat attendu** :
- ✅ Toutes les inscriptions traitées
- ✅ Traitement par lots de 50
- ✅ Temps de traitement acceptable (< 30 secondes pour 100 inscriptions)

---

## 🐛 Dépannage

### Erreur : "Le fichier Excel est requis"

**Cause** : Le fichier n'a pas été uploadé correctement

**Solution** : Vérifiez que le paramètre `file` est bien envoyé avec `-F "file=@fichier.xlsx"`

---

### Erreur : "Colonnes manquantes"

**Cause** : Le fichier Excel n'a pas toutes les colonnes requises

**Solution** : Utilisez le template téléchargé depuis l'API

---

### Erreur : "L'école avec l'ID X n'existe pas"

**Cause** : L'ID de l'école dans le fichier Excel n'existe pas en base

**Solution** : Vérifiez les IDs en base et ajustez le fichier Excel

---

### Erreur : "Authentification échouée"

**Cause** : Token invalide ou expiré

**Solution** : Réauthentifiez-vous et obtenez un nouveau token

---

## ✅ Checklist de Test

- [ ] Template Excel téléchargé avec succès
- [ ] Fichier Excel de test créé
- [ ] Upload d'un fichier valide → Inscriptions créées
- [ ] Upload d'un fichier avec erreurs → Erreurs détectées
- [ ] Upload d'un fichier avec doublons → Doublons détectés
- [ ] Rapport détaillé affiché correctement
- [ ] Performance acceptable pour gros fichiers

---

## 📝 Notes

- Les fichiers de test peuvent être supprimés après les tests
- Le template Excel peut être réutilisé pour créer de vrais fichiers d'inscription
- Les erreurs sont détaillées ligne par ligne pour faciliter la correction

---

## 🎯 Conclusion

Une fois tous les tests passés avec succès, la fonctionnalité est prête pour la production.



