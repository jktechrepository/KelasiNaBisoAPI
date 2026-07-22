# 🧪 Guide de Test : Support des Images pour les Devoirs

**Date** : 30 janvier 2025  
**Script** : `test-upload-images-devoirs.sh`

---

## 📋 Prérequis

1. **Application en cours d'exécution** sur `https://localhost:7102`
2. **Python 3** avec la bibliothèque `Pillow` installée :
   ```bash
   pip3 install Pillow
   ```
3. **jq** installé (pour le parsing JSON) :
   ```bash
   brew install jq  # macOS
   # ou
   sudo apt-get install jq  # Linux
   ```

---

## 🚀 Exécution du Test

### Option 1 : Exécution directe

```bash
cd /Users/mac/Desktop/KelasiNaBisoAPI
./test-upload-images-devoirs.sh
```

### Option 2 : Exécution avec bash

```bash
bash test-upload-images-devoirs.sh
```

---

## 📊 Tests Effectués

Le script teste automatiquement :

### ✅ Test 1 : Upload PDF
- **Objectif** : Vérifier que les PDF fonctionnent toujours
- **Fichier** : `/tmp/test-devoir.pdf` (créé automatiquement)
- **Résultat attendu** : ✅ Succès

### ✅ Test 2 : Upload JPG
- **Objectif** : Vérifier le nouveau support des images JPG
- **Fichier** : `/tmp/test-devoir.jpg` (créé automatiquement, 100x100px rouge)
- **Résultat attendu** : ✅ Succès

### ✅ Test 3 : Upload PNG
- **Objectif** : Vérifier le nouveau support des images PNG
- **Fichier** : `/tmp/test-devoir.png` (créé automatiquement, 100x100px bleue)
- **Résultat attendu** : ✅ Succès

### ❌ Test 4 : Upload fichier non autorisé
- **Objectif** : Vérifier que les fichiers non autorisés sont rejetés
- **Fichier** : `/tmp/test-devoir.txt` (fichier texte)
- **Résultat attendu** : ❌ Rejeté (Code HTTP 400)

### 📥 Test 5 : Téléchargement
- **Objectif** : Vérifier que les fichiers uploadés peuvent être téléchargés
- **Résultat attendu** : ✅ Tous les fichiers téléchargés avec succès

---

## 🔧 Configuration

Vous pouvez modifier les paramètres au début du script :

```bash
BASE_URL="https://localhost:7102"  # URL de l'API
EMAIL="dondej@kelasinabiso.cd"     # Compte Enseignant
PASSWORD="123456"                   # Mot de passe
CLASSE_ID=80                        # ID de la classe (5ème Primaire)
```

---

## 📝 Résultats Attendus

### Succès ✅

```
✅ Test 1 (PDF) : RÉUSSI
✅ Test 2 (JPG) : RÉUSSI
✅ Test 3 (PNG) : RÉUSSI
✅ Test 4 (TXT rejeté) : RÉUSSI
✅ Test 5 (Téléchargement) : RÉUSSI

Total : 5 réussis / 0 échecs
```

### Exemple de réponse JSON (upload réussi)

```json
{
  "idDevoirADomicile": 7,
  "titre": "Test JPG - Support Images",
  "nomFichier": "test-devoir.jpg",
  "typeMIME": "image/jpeg",
  "tailleFichier": 1234
}
```

---

## ⚠️ Dépannage

### Erreur : "Pillow not found"

```bash
pip3 install Pillow
```

### Erreur : "jq: command not found"

```bash
# macOS
brew install jq

# Linux
sudo apt-get install jq
```

### Erreur : "Connection refused"

- Vérifier que l'application est en cours d'exécution
- Vérifier l'URL dans `BASE_URL`

### Erreur : "401 Unauthorized"

- Vérifier les identifiants (`EMAIL` et `PASSWORD`)
- Vérifier que le compte a les permissions nécessaires

---

## 🧹 Nettoyage

Le script nettoie automatiquement les fichiers temporaires à la fin. Si vous voulez nettoyer manuellement :

```bash
rm -f /tmp/test-devoir.* /tmp/downloaded-*.*
```

---

## 📚 Documentation Associée

- `ANALYSE_SUPPORT_IMAGES_DEVOIRS.md` : Analyse détaillée du support des images
- `Services/FileStorageService.cs` : Service de stockage local
- `Services/S3FileStorageService.cs` : Service de stockage S3
- `Services/AntivirusService.cs` : Service de validation des fichiers

