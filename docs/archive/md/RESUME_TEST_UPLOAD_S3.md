# 📋 Résumé - Test Upload S3

**Date** : 30 novembre 2025  
**Status** : ⚠️ En cours de correction

---

## ✅ Corrections Effectuées

1. **Ajout du claim `AgentId` dans le token JWT**
   - Fichier modifié : `Services/SimpleJwtService.cs`
   - Le claim `AgentId` est maintenant ajouté au token si l'utilisateur a un `IdAgent`

2. **Vérification des permissions Admin**
   - ✅ Authentification : Fonctionne
   - ✅ Voir mes devoirs : Fonctionne (Admin voit tous les devoirs de son école)
   - ✅ Voir devoirs classe : Fonctionne

---

## ⚠️ Problème Actuel

L'upload d'un devoir échoue avec l'erreur :
```
Vous devez être un agent pour publier un devoir
```

**Cause** : Le claim `AgentId` n'est pas présent dans le token JWT, même si l'utilisateur a un `idAgent: 139` dans la base de données.

**Solution** : 
1. Vérifier que l'utilisateur chargé lors de l'authentification a bien son `IdAgent` chargé
2. Vérifier que le token JWT contient bien le claim `AgentId` après authentification

---

## 🧪 Test Manuel

### 1. Vérifier le token JWT

Décoder le token JWT pour vérifier s'il contient le claim `AgentId` :

```bash
TOKEN=$(curl -k -s -X POST "https://localhost:7102/api/Utilisateur/authentifier" \
  -H "Content-Type: application/json" \
  -d '{"emailOuTelephone":"jk2@kelasinabiso.cd","motDePasse":"12345678"}' \
  | jq -r '.accessToken')

# Décoder le token (partie payload)
echo "$TOKEN" | cut -d. -f2 | base64 -d | jq '.'
```

**Vérifier** : Le token doit contenir `"AgentId": "139"`

### 2. Tester l'upload

```bash
TOKEN=$(curl -k -s -X POST "https://localhost:7102/api/Utilisateur/authentifier" \
  -H "Content-Type: application/json" \
  -d '{"emailOuTelephone":"jk2@kelasinabiso.cd","motDePasse":"12345678"}' \
  | jq -r '.accessToken')

CLASSE_ID=$(curl -k -s -X GET "https://localhost:7102/api/Classe" \
  -H "Authorization: Bearer $TOKEN" \
  | jq -r '.[0].idClasse')

curl -k -X POST "https://localhost:7102/api/DevoirADomicile" \
  -H "Authorization: Bearer $TOKEN" \
  -F "titre=Test Devoir Admin - Upload S3" \
  -F "description=Test upload S3" \
  -F "idClasse=$CLASSE_ID" \
  -F "dateLimite=2025-12-15T23:59:59" \
  -F "fichier=@/tmp/test-devoir-s3.pdf"
```

---

## 🔍 Vérifications à Faire

1. **Vérifier que l'utilisateur a bien un `IdAgent` dans la base de données**
   ```sql
   SELECT IdUtilisateur, Email, IdAgent, IdEcole 
   FROM Utilisateurs 
   WHERE Email = 'jk2@kelasinabiso.cd';
   ```

2. **Vérifier que l'utilisateur chargé lors de l'authentification a bien son `IdAgent`**
   - Vérifier dans `Controllers/UtilisateurController.cs` ligne 1352-1357
   - L'utilisateur est chargé avec `.Include()` mais `IdAgent` est une propriété simple, donc elle devrait être chargée automatiquement

3. **Vérifier les logs au démarrage**
   - Chercher le message : `🔍 GenerateToken - Utilisateur {id}: IdAgent = {value}`
   - Cela confirmera si l'`IdAgent` est bien chargé lors de la génération du token

---

## 📝 Prochaines Étapes

1. ✅ Code modifié pour ajouter `AgentId` au token
2. ⏳ Vérifier que l'application utilise bien le nouveau code (redémarrer si nécessaire)
3. ⏳ Vérifier que le token contient bien le claim `AgentId` après authentification
4. ⏳ Tester l'upload d'un devoir
5. ⏳ Vérifier que le fichier est bien uploadé vers S3

---

## 🆘 Si le Problème Persiste

Si le claim `AgentId` n'est toujours pas dans le token après redémarrage :

1. Vérifier que l'utilisateur a bien un `IdAgent` dans la base de données
2. Vérifier que l'utilisateur chargé lors de l'authentification a bien son `IdAgent` chargé
3. Vérifier les logs pour voir si le message de debug apparaît
4. Vérifier que le code modifié est bien compilé et utilisé

---

**Note** : Le fichier PDF de test est disponible à `/tmp/test-devoir-s3.pdf`

