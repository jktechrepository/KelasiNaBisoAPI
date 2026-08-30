# 🔐 Guide : Ajouter les Permissions S3 pour l'Utilisateur IAM

**Date** : 30 novembre 2025  
**Utilisateur IAM** : `kelasinabiso-s3-user`  
**Bucket** : `kansa-kelasinabiso-s3-bucket`  
**Région** : `eu-north-1`

---

## 🎯 Permissions Nécessaires

L'application a besoin des permissions suivantes pour gérer les fichiers dans S3 :

- ✅ `s3:PutObject` - Pour uploader des fichiers (devoirs)
- ✅ `s3:GetObject` - Pour télécharger des fichiers
- ✅ `s3:DeleteObject` - Pour supprimer des fichiers
- ✅ `s3:ListBucket` - Pour lister les fichiers (optionnel mais recommandé)

---

## 📋 Méthode 1 : Via la Console AWS (Recommandé)

### Étape 1 : Accéder à l'Utilisateur IAM

1. Connectez-vous à la **Console AWS** : https://console.aws.amazon.com
2. Allez dans **IAM** (Identity and Access Management)
3. Dans le menu de gauche, cliquez sur **Users** (Utilisateurs)
4. Recherchez et cliquez sur **`kelasinabiso-s3-user`**

### Étape 2 : Ajouter une Politique Inline

1. Dans l'onglet **Permissions**, vous verrez les politiques actuelles
2. Cliquez sur **Add permissions** (Ajouter des permissions)
3. Sélectionnez **Create inline policy** (Créer une politique inline)
4. Cliquez sur l'onglet **JSON**
5. Collez le JSON suivant :

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "AllowDevoirsOperations",
      "Effect": "Allow",
      "Action": [
        "s3:PutObject",
        "s3:GetObject",
        "s3:DeleteObject"
      ],
      "Resource": "arn:aws:s3:::kansa-kelasinabiso-s3-bucket/devoirs/*"
    },
    {
      "Sid": "AllowListBucket",
      "Effect": "Allow",
      "Action": [
        "s3:ListBucket"
      ],
      "Resource": "arn:aws:s3:::kansa-kelasinabiso-s3-bucket",
      "Condition": {
        "StringLike": {
          "s3:prefix": "devoirs/*"
        }
      }
    }
  ]
}
```

6. Cliquez sur **Next** (Suivant)
7. Donnez un nom à la politique : `S3DevoirsAccess`
8. Cliquez sur **Create policy** (Créer la politique)

### Étape 3 : Vérification

1. Vous devriez voir la nouvelle politique dans la liste des politiques inline
2. La politique devrait être active immédiatement

---

## 📋 Méthode 2 : Via une Politique Gérée (Alternative)

### Étape 1 : Créer une Politique Gérée

1. Allez dans **IAM** → **Policies** (Politiques)
2. Cliquez sur **Create policy** (Créer une politique)
3. Cliquez sur l'onglet **JSON**
4. Collez le même JSON que ci-dessus
5. Cliquez sur **Next**
6. Donnez un nom : `KelasiNaBiso-S3-Devoirs-Policy`
7. Ajoutez une description : `Permissions S3 pour les devoirs à domicile`
8. Cliquez sur **Create policy**

### Étape 2 : Attacher la Politique à l'Utilisateur

1. Retournez dans **IAM** → **Users** → **kelasinabiso-s3-user**
2. Cliquez sur **Add permissions** → **Attach policies directly**
3. Recherchez `KelasiNaBiso-S3-Devoirs-Policy`
4. Cochez la case
5. Cliquez sur **Next** → **Add permissions**

---

## 🔍 Vérification des Permissions

### Test 1 : Vérifier la Politique

1. Dans **IAM** → **Users** → **kelasinabiso-s3-user**
2. Onglet **Permissions**
3. Vous devriez voir la politique avec les permissions :
   - `s3:PutObject`
   - `s3:GetObject`
   - `s3:DeleteObject`
   - `s3:ListBucket`

### Test 2 : Tester l'Upload

Après avoir ajouté les permissions, testez l'upload depuis l'application :

```bash
# L'upload devrait maintenant fonctionner
curl -k -X POST "https://localhost:7102/api/DevoirADomicile" \
  -H "Authorization: Bearer $TOKEN" \
  -F "titre=Test Devoir Admin" \
  -F "description=Test" \
  -F "idClasse=43" \
  -F "dateLimite=2025-12-15T23:59:59" \
  -F "fichier=@/tmp/test-devoir-s3.pdf"
```

---

## ⚠️ Sécurité

### Bonnes Pratiques

1. **Principe du moindre privilège** : Les permissions sont limitées au dossier `devoirs/*` uniquement
2. **Pas d'accès root** : L'utilisateur IAM n'a pas d'accès administrateur
3. **Bucket spécifique** : Les permissions sont limitées à un seul bucket

### Permissions Limitées

La politique ci-dessus autorise uniquement :
- ✅ Upload dans `devoirs/*`
- ✅ Download depuis `devoirs/*`
- ✅ Suppression dans `devoirs/*`
- ✅ Liste du contenu de `devoirs/*`

**Interdit** :
- ❌ Accès aux autres dossiers du bucket
- ❌ Modification des paramètres du bucket
- ❌ Suppression du bucket

---

## 🐛 Dépannage

### Erreur : "Access Denied"

**Cause** : Les permissions ne sont pas encore propagées (peut prendre quelques secondes)

**Solution** : Attendez 10-30 secondes et réessayez

### Erreur : "Policy not found"

**Cause** : La politique n'a pas été créée correctement

**Solution** : Vérifiez que la politique existe dans **IAM** → **Policies**

### Erreur : "User not authorized"

**Cause** : La politique n'est pas attachée à l'utilisateur

**Solution** : Vérifiez dans **IAM** → **Users** → **kelasinabiso-s3-user** → **Permissions**

---

## 📝 Résumé de la Politique

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "s3:PutObject",      // Upload
        "s3:GetObject",      // Download
        "s3:DeleteObject"     // Suppression
      ],
      "Resource": "arn:aws:s3:::kansa-kelasinabiso-s3-bucket/devoirs/*"
    },
    {
      "Effect": "Allow",
      "Action": [
        "s3:ListBucket"      // Liste des fichiers
      ],
      "Resource": "arn:aws:s3:::kansa-kelasinabiso-s3-bucket",
      "Condition": {
        "StringLike": {
          "s3:prefix": "devoirs/*"
        }
      }
    }
  ]
}
```

---

## ✅ Checklist

- [ ] Connecté à la console AWS
- [ ] Accédé à IAM → Users → kelasinabiso-s3-user
- [ ] Créé la politique avec les permissions S3
- [ ] Attaché la politique à l'utilisateur
- [ ] Vérifié que les permissions sont actives
- [ ] Testé l'upload depuis l'application

---

## 🎯 Après Configuration

Une fois les permissions ajoutées :

1. ✅ L'upload de devoirs fonctionnera
2. ✅ Le téléchargement de devoirs fonctionnera
3. ✅ La suppression de devoirs fonctionnera
4. ✅ Les fichiers seront stockés dans S3 : `kansa-kelasinabiso-s3-bucket/devoirs/`

**Bon courage !** 🚀

