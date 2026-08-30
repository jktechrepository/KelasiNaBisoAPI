# Guide de Configuration AWS S3

Ce guide explique comment configurer le stockage AWS S3 pour remplacer le stockage local dans l'application KelasiNaBisoAPI.

## 📋 Vue d'ensemble

L'application utilise maintenant AWS S3 pour le stockage des fichiers (devoirs à domicile, ressources pédagogiques, etc.) au lieu du stockage local. Le système bascule automatiquement entre S3 et le stockage local selon la configuration.

## 🔧 Configuration

### 1. Créer un bucket S3 sur AWS

1. Connectez-vous à la console AWS
2. Allez dans **S3** → **Créer un bucket**
3. Choisissez un nom unique (ex: `kelasinabiso-uploads`)
4. Sélectionnez la région (ex: `us-east-1`)
5. Configurez les paramètres de sécurité selon vos besoins
6. **Important** : Notez le nom du bucket et la région

### 2. Créer des credentials IAM

1. Allez dans **IAM** → **Users** → **Add users**
2. Créez un utilisateur avec accès programmatique
3. Attachez une politique avec les permissions suivantes :
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
           "arn:aws:s3:::kelasinabiso-uploads",
           "arn:aws:s3:::kelasinabiso-uploads/*"
         ]
       }
     ]
   }
   ```
4. **Important** : Notez l'**Access Key ID** et le **Secret Access Key**

### 3. Configurer l'application

Modifiez les fichiers `appsettings.json` et `appsettings.Development.json` :

```json
{
  "AWS": {
    "S3": {
      "BucketName": "kelasinabiso-uploads",
      "Region": "us-east-1",
      "AccessKeyId": "VOTRE_ACCESS_KEY_ID",
      "SecretAccessKey": "VOTRE_SECRET_ACCESS_KEY"
    }
  }
}
```

**⚠️ Sécurité** : 
- Ne commitez JAMAIS les credentials dans Git
- Utilisez des variables d'environnement ou Azure Key Vault en production
- Pour le développement, utilisez `appsettings.Development.json` (déjà dans `.gitignore`)

### 4. Variables d'environnement (Recommandé pour la production)

Au lieu de mettre les credentials dans `appsettings.json`, utilisez des variables d'environnement :

```bash
export AWS__S3__AccessKeyId="VOTRE_ACCESS_KEY_ID"
export AWS__S3__SecretAccessKey="VOTRE_SECRET_ACCESS_KEY"
export AWS__S3__BucketName="kelasinabiso-uploads"
export AWS__S3__Region="us-east-1"
```

## 🔄 Fonctionnement

### Basculement automatique

Le système détecte automatiquement si les credentials AWS sont configurés :

- **Si les credentials sont présents** → Utilise `S3FileStorageService` (AWS S3)
- **Si les credentials sont absents** → Utilise `FileStorageService` (stockage local)

### Structure des fichiers dans S3

Les fichiers sont organisés par sous-dossiers dans le bucket :

```
kelasinabiso-uploads/
  ├── devoirs/
  │   ├── {guid}.pdf
  │   └── {guid}.pdf
  └── ressources/
      └── ...
```

Le `FilePath` stocké en base de données contient la clé S3 (ex: `devoirs/abc123.pdf`) au lieu du chemin local.

## 📝 Migration depuis le stockage local

Si vous avez déjà des fichiers en stockage local et souhaitez les migrer vers S3 :

1. **Option 1 : Migration manuelle**
   - Téléchargez tous les fichiers depuis `wwwroot/uploads/`
   - Uploadez-les vers S3 en respectant la structure
   - Mettez à jour les `CheminFichier` en base de données

2. **Option 2 : Script de migration** (à créer)
   - Script PowerShell/C# pour lire les fichiers locaux
   - Upload vers S3
   - Mise à jour de la base de données

## 🧪 Test

Pour tester la configuration :

1. Vérifiez les logs au démarrage :
   ```
   ✅ Stockage AWS S3 configuré et activé
   ```
   ou
   ```
   ⚠️  Credentials AWS S3 non configurés. Utilisation du stockage local.
   ```

2. Testez l'upload d'un devoir :
   - Créez un devoir avec un fichier PDF
   - Vérifiez dans la console S3 que le fichier est présent
   - Vérifiez que le téléchargement fonctionne

## 🔒 Sécurité

### Bonnes pratiques

1. **IAM Policy** : Utilisez le principe du moindre privilège
2. **Bucket Policy** : Configurez des règles de bucket pour restreindre l'accès
3. **Encryption** : Activez le chiffrement S3 (SSE-S3 ou SSE-KMS)
4. **Versioning** : Activez le versioning pour la récupération
5. **Lifecycle** : Configurez des règles de lifecycle pour archiver/supprimer les anciens fichiers

### Exemple de Bucket Policy (accès privé)

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Deny",
      "Principal": "*",
      "Action": "s3:*",
      "Resource": [
        "arn:aws:s3:::kelasinabiso-uploads",
        "arn:aws:s3:::kelasinabiso-uploads/*"
      ],
      "Condition": {
        "Bool": {
          "aws:SecureTransport": "false"
        }
      }
    }
  ]
}
```

## 🐛 Dépannage

### Erreur : "Access Denied"

- Vérifiez que les credentials IAM ont les bonnes permissions
- Vérifiez que le nom du bucket est correct
- Vérifiez que la région est correcte

### Erreur : "Bucket not found"

- Vérifiez que le bucket existe
- Vérifiez que le nom du bucket est correct (sensible à la casse)
- Vérifiez que vous êtes dans la bonne région AWS

### Les fichiers ne s'uploadent pas

- Vérifiez les logs de l'application
- Vérifiez la configuration dans `appsettings.json`
- Vérifiez la connexion internet
- Vérifiez les quotas AWS (limites de requêtes)

## 📚 Références

- [Documentation AWS S3 .NET SDK](https://docs.aws.amazon.com/sdk-for-net/latest/developer-guide/s3.html)
- [IAM Best Practices](https://docs.aws.amazon.com/IAM/latest/UserGuide/best-practices.html)
- [S3 Security Best Practices](https://docs.aws.amazon.com/AmazonS3/latest/userguide/security-best-practices.html)

## ✅ Checklist de déploiement

- [ ] Bucket S3 créé
- [ ] Utilisateur IAM créé avec les bonnes permissions
- [ ] Credentials configurés dans `appsettings.json` (dev) ou variables d'environnement (prod)
- [ ] Bucket Policy configurée
- [ ] Encryption activée
- [ ] Test d'upload réussi
- [ ] Test de téléchargement réussi
- [ ] Logs vérifiés (pas d'erreurs)

