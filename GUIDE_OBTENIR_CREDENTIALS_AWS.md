# Guide : Comment obtenir AccessKeyId et SecretAccessKey AWS

Ce guide vous explique étape par étape comment créer un utilisateur IAM et obtenir vos credentials AWS pour S3.

## 📋 Prérequis

- Un compte AWS actif
- Accès à la console AWS (https://console.aws.amazon.com)

## 🔧 Étapes détaillées

### Étape 1 : Se connecter à la console AWS

1. Allez sur https://console.aws.amazon.com
2. Connectez-vous avec votre compte AWS
3. Une fois connecté, vous verrez le tableau de bord AWS

### Étape 2 : Accéder au service IAM

1. Dans la barre de recherche en haut, tapez **"IAM"**
2. Cliquez sur **"IAM"** dans les résultats
3. Vous êtes maintenant dans la console IAM (Identity and Access Management)

### Étape 3 : Créer un utilisateur IAM

1. Dans le menu de gauche, cliquez sur **"Users"** (Utilisateurs)
2. Cliquez sur le bouton **"Create user"** (Créer un utilisateur) en haut à droite

### Étape 4 : Configurer l'utilisateur

1. **Nom d'utilisateur** : Entrez un nom, par exemple `kelasinabiso-s3-user`
2. **Type d'accès** : Cochez **"Provide user access to the AWS Management Console"** (optionnel)
   - **OU** cochez uniquement **"Access key - Programmatic access"** (recommandé pour une application)
3. Cliquez sur **"Next"** (Suivant)

### Étape 5 : Attacher des permissions

1. Sélectionnez **"Attach policies directly"**
2. Dans la barre de recherche, tapez **"S3"**
3. Cochez la politique **"AmazonS3FullAccess"** (pour un accès complet)
   - **OU** créez une politique personnalisée (voir section "Politique personnalisée" ci-dessous)
4. Cliquez sur **"Next"**

### Étape 6 : Réviser et créer

1. Vérifiez les informations
2. Cliquez sur **"Create user"** (Créer un utilisateur)

### Étape 7 : Obtenir les credentials ⭐

**⚠️ IMPORTANT : Cette étape est CRITIQUE - vous ne pourrez plus voir le SecretAccessKey après !**

1. Une fois l'utilisateur créé, vous verrez une page de confirmation
2. **Si vous avez choisi "Programmatic access"** :
   - Vous verrez directement l'**Access Key ID** et le **Secret Access Key**
   - **COPIEZ IMMÉDIATEMENT** ces deux valeurs dans un endroit sûr
   - Le Secret Access Key ne sera affiché qu'une seule fois !

3. **Si vous ne voyez pas les credentials** :
   - Cliquez sur le nom de l'utilisateur que vous venez de créer
   - Allez dans l'onglet **"Security credentials"** (Identifiants de sécurité)
   - Cliquez sur **"Create access key"** (Créer une clé d'accès)
   - Sélectionnez **"Application running outside AWS"** ou **"Other"**
   - Cliquez sur **"Next"**
   - Optionnel : Ajoutez une description (ex: "Pour KelasiNaBiso API")
   - Cliquez sur **"Create access key"**
   - **COPIEZ IMMÉDIATEMENT** l'Access Key ID et le Secret Access Key

### Étape 8 : Configurer dans l'application

Une fois que vous avez les credentials, ajoutez-les dans `appsettings.json` :

```json
{
  "AWS": {
    "S3": {
      "BucketName": "kansa-kelasinabiso-s3-bucket",
      "Region": "eu-north-1",
      "AccessKeyId": "AKIAIOSFODNN7EXAMPLE",
      "SecretAccessKey": "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY"
    }
  }
}
```

**⚠️ SÉCURITÉ :**
- Ne partagez JAMAIS ces credentials
- Ne les commitez JAMAIS dans Git
- Utilisez `appsettings.Development.json` pour le développement (déjà dans .gitignore)

## 🔒 Politique personnalisée (Recommandé pour la production)

Pour plus de sécurité, créez une politique qui donne uniquement les permissions nécessaires :

1. Dans IAM, allez dans **"Policies"** → **"Create policy"**
2. Cliquez sur l'onglet **"JSON"**
3. Collez ce code (remplacez `kansa-kelasinabiso-s3-bucket` par votre nom de bucket) :

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "s3:PutObject",
        "s3:GetObject",
        "s3:DeleteObject",
        "s3:ListBucket"
      ],
      "Resource": [
        "arn:aws:s3:::kansa-kelasinabiso-s3-bucket",
        "arn:aws:s3:::kansa-kelasinabiso-s3-bucket/*"
      ]
    }
  ]
}
```

4. Cliquez sur **"Next"**
5. Donnez un nom à la politique (ex: `KelasiNaBisoS3Policy`)
6. Cliquez sur **"Create policy"**
7. Attachez cette politique à votre utilisateur au lieu de `AmazonS3FullAccess`

## 🆘 Si vous avez perdu le Secret Access Key

Si vous avez perdu le Secret Access Key, vous devez en créer un nouveau :

1. Allez dans IAM → Users → Votre utilisateur
2. Onglet **"Security credentials"**
3. Dans la section **"Access keys"**, cliquez sur **"Create access key"**
4. **Important** : Supprimez l'ancienne clé si elle n'est plus utilisée

## ✅ Vérification

Pour vérifier que tout fonctionne :

1. Redémarrez votre application
2. Vérifiez les logs au démarrage - vous devriez voir :
   ```
   ✅ Stockage AWS S3 configuré et activé
   ```
3. Testez l'upload d'un fichier via l'API

## 📸 Aide visuelle

### Où trouver IAM dans la console AWS :
```
Console AWS → Recherche "IAM" → IAM Dashboard
```

### Structure du menu IAM :
```
IAM
├── Users (Utilisateurs)
│   └── Create user
├── Policies (Politiques)
│   └── Create policy
└── ...
```

### Où trouver les credentials :
```
IAM → Users → [Votre utilisateur] → Security credentials → Access keys
```

## 🔐 Bonnes pratiques de sécurité

1. **Principe du moindre privilège** : Donnez uniquement les permissions nécessaires
2. **Rotation des clés** : Changez les clés régulièrement (tous les 90 jours recommandé)
3. **Ne partagez jamais** : Les credentials sont personnels et confidentiels
4. **Variables d'environnement** : En production, utilisez des variables d'environnement au lieu de fichiers de configuration
5. **Monitoring** : Activez CloudTrail pour surveiller l'utilisation des credentials

## 🐛 Problèmes courants

### "Access Denied" lors de l'upload
- Vérifiez que la politique IAM est bien attachée à l'utilisateur
- Vérifiez que le nom du bucket est correct dans la politique
- Vérifiez que les credentials sont corrects

### "Invalid credentials"
- Vérifiez que vous avez copié les credentials sans espaces
- Vérifiez que le Secret Access Key est complet (il est long, ~40 caractères)
- Créez une nouvelle clé d'accès si nécessaire

### "Bucket not found"
- Vérifiez que le bucket existe dans la bonne région
- Vérifiez que le nom du bucket est exactement le même (sensible à la casse)

## 📞 Support

Si vous rencontrez des problèmes :
1. Vérifiez les logs de l'application
2. Vérifiez la console CloudWatch pour les erreurs AWS
3. Consultez la documentation AWS : https://docs.aws.amazon.com/IAM/latest/UserGuide/id_credentials_access-keys.html

