# 🧪 Guide de Test - Permissions Admin pour Devoirs à Domicile

**Date** : 30 novembre 2025  
**Compte de test** : Admin (jk2@kelasinabiso.cd)

---

## 📋 Informations de Test

- **Email** : `jk2@kelasinabiso.cd`
- **Mot de passe** : `12345678`
- **Rôle** : Admin
- **URL API** : `https://localhost:7102`

---

## 🚀 Démarrage de l'Application

### Étape 1 : Démarrer l'API

```bash
cd /Users/mac/Desktop/KelasiNaBisoAPI
dotnet run
```

**Vérifications importantes au démarrage :**

1. ✅ **S3 Configuration** : Cherchez dans les logs :
   ```
   ✅ Stockage AWS S3 configuré et activé
   ```
   OU
   ```
   ⚠️  Credentials AWS S3 non configurés. Utilisation du stockage local.
   ```

2. ✅ **Pas d'erreurs** : Vérifiez qu'il n'y a pas d'erreurs de compilation ou de démarrage

3. ✅ **Swagger disponible** : Ouvrez https://localhost:7102/swagger

---

## 🔐 Test 1 : Authentification

### Requête

```http
POST https://localhost:7102/api/Utilisateur/authentifier
Content-Type: application/json

{
  "emailOuTelephone": "jk2@kelasinabiso.cd",
  "motDePasse": "12345678"
}
```

### Réponse attendue

```json
{
  "success": true,
  "message": "Authentification réussie",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 43200,
  "utilisateur": {
    "idUtilisateur": ...,
    "email": "jk2@kelasinabiso.cd",
    "role": "Admin",
    "idEcole": ...,
    "idAgent": ...
  }
}
```

### ✅ Vérifications

- [ ] Status code : `200 OK`
- [ ] `accessToken` présent et non vide
- [ ] `utilisateur.role` = `"Admin"`
- [ ] `utilisateur.idEcole` présent (Admin doit avoir une école)
- [ ] `utilisateur.idAgent` présent (Admin doit avoir un Agent)

---

## 📚 Test 2 : Voir mes devoirs (Admin)

**Comportement attendu** : Admin doit voir **TOUS les devoirs de son école**, pas seulement les siens.

### Requête

```http
GET https://localhost:7102/api/DevoirADomicile/mes-devoirs
Authorization: Bearer {TOKEN_OBTENU_ETAPE_1}
```

### Réponse attendue

```json
[
  {
    "idDevoirADomicile": 1,
    "titre": "...",
    "description": "...",
    "idEcole": ...,
    "idClasse": ...,
    "nomAgent": "...",
    ...
  },
  ...
]
```

### ✅ Vérifications

- [ ] Status code : `200 OK`
- [ ] Retourne une liste de devoirs
- [ ] **IMPORTANT** : Doit retourner **tous les devoirs de l'école**, pas seulement ceux publiés par l'Admin
- [ ] Tous les devoirs retournés ont le même `idEcole` que l'Admin

---

## 📖 Test 3 : Voir les devoirs d'une classe

**Comportement attendu** : Admin peut voir les devoirs de n'importe quelle classe de son école.

### Requête

```http
GET https://localhost:7102/api/DevoirADomicile/classe/{ID_CLASSE}
Authorization: Bearer {TOKEN}
```

**Note** : Remplacez `{ID_CLASSE}` par un ID de classe valide de l'école de l'Admin.

### ✅ Vérifications

- [ ] Status code : `200 OK` (si la classe appartient à son école)
- [ ] Status code : `403 Forbidden` (si la classe appartient à une autre école)
- [ ] Retourne les devoirs de la classe

---

## 📤 Test 4 : Publier un devoir (Admin)

**Comportement attendu** : Admin peut publier un devoir pour n'importe quelle classe de son école.

### Requête

```http
POST https://localhost:7102/api/DevoirADomicile
Authorization: Bearer {TOKEN}
Content-Type: multipart/form-data

{
  "titre": "Test Devoir Admin",
  "description": "Description du devoir de test",
  "idClasse": {ID_CLASSE},
  "idCours": {ID_COURS}, // Optionnel
  "dateLimite": "2025-12-15T23:59:59",
  "fichier": [FICHIER_PDF]
}
```

### ✅ Vérifications

- [ ] Status code : `200 OK` ou `201 Created`
- [ ] Le devoir est créé avec succès
- [ ] Le fichier est uploadé vers S3 (vérifiez dans la console AWS S3)
- [ ] Le devoir apparaît dans la liste des devoirs de l'école

### ⚠️ Test de sécurité

Essayez de publier un devoir pour une classe d'une **autre école** :
- [ ] Status code : `403 Forbidden`
- [ ] Message : "Vous n'êtes pas autorisé à publier pour cette classe"

---

## 📥 Test 5 : Télécharger un devoir

### Requête

```http
GET https://localhost:7102/api/DevoirADomicile/{ID_DEVOIR}/telecharger
Authorization: Bearer {TOKEN}
```

### ✅ Vérifications

- [ ] Status code : `200 OK`
- [ ] Le fichier est téléchargé depuis S3
- [ ] Le type de contenu est `application/pdf`
- [ ] Le nombre de téléchargements est incrémenté

---

## 🔍 Test 6 : Vérification S3

### Vérifier dans la console AWS

1. Allez dans **S3** → **kansa-kelasinabiso-s3-bucket**
2. Vérifiez le dossier `devoirs/`
3. Vous devriez voir les fichiers uploadés avec des noms comme `{guid}.pdf`

### Vérifier dans les logs de l'application

Cherchez dans les logs :
```
Fichier uploadé avec succès vers S3 : kansa-kelasinabiso-s3-bucket/devoirs/{guid}.pdf
```

---

## 📊 Checklist Complète

### Authentification
- [ ] Authentification réussie avec le compte Admin
- [ ] Token JWT obtenu
- [ ] Rôle vérifié : Admin
- [ ] ID École présent
- [ ] ID Agent présent

### Permissions Lecture
- [ ] Peut voir tous les devoirs de son école (mes-devoirs)
- [ ] Peut voir les devoirs d'une classe de son école
- [ ] Ne peut PAS voir les devoirs d'une classe d'une autre école (403)

### Permissions Écriture
- [ ] Peut publier un devoir pour une classe de son école
- [ ] Ne peut PAS publier pour une classe d'une autre école (403)
- [ ] Le fichier est uploadé vers S3
- [ ] Le fichier apparaît dans le bucket S3

### Permissions Téléchargement
- [ ] Peut télécharger un devoir de son école
- [ ] Le fichier est téléchargé depuis S3
- [ ] Le nombre de téléchargements est incrémenté

### S3
- [ ] Configuration S3 détectée au démarrage
- [ ] Fichiers uploadés vers S3
- [ ] Fichiers téléchargés depuis S3
- [ ] Pas d'erreurs S3 dans les logs

---

## 🐛 Dépannage

### Erreur : "401 Unauthorized"
- Vérifiez que le token est valide
- Vérifiez le format : `Authorization: Bearer {TOKEN}`
- Vérifiez que le token n'a pas expiré

### Erreur : "403 Forbidden"
- Vérifiez que la classe appartient à l'école de l'Admin
- Vérifiez les logs pour voir la raison du refus

### Erreur : "Access Denied" (S3)
- Vérifiez les credentials AWS dans `appsettings.Development.json`
- Vérifiez les permissions IAM de l'utilisateur
- Vérifiez que le bucket existe et est dans la bonne région

### Les fichiers ne s'uploadent pas vers S3
- Vérifiez les logs au démarrage
- Vérifiez la configuration AWS dans `appsettings.Development.json`
- Vérifiez la connexion internet
- Vérifiez que le bucket existe

---

## 📝 Notes

- Le token JWT expire après 5 minutes (selon la configuration)
- Si le token expire, ré-authentifiez-vous
- Les fichiers sont stockés dans S3 avec des noms uniques (GUID)
- Les fichiers sont organisés par sous-dossiers : `devoirs/{guid}.pdf`

---

## ✅ Résultat Attendu

Après tous les tests, vous devriez avoir :
- ✅ Authentification Admin fonctionnelle
- ✅ Permissions correctes (peut voir/publier pour son école)
- ✅ S3 fonctionnel (upload/download)
- ✅ Aucune erreur dans les logs

**Bon test !** 🎉

