# ✅ Résultats des Tests - Permissions Admin

**Date** : 30 novembre 2025  
**Compte testé** : Admin (jk2@kelasinabiso.cd)

---

## 🔐 Test 1 : Authentification ✅

**Status** : ✅ **RÉUSSI**

**Réponse** :
- Token JWT généré avec succès
- Rôle : `Admin` (avec rôles multiples : Admin, Enseignant, Financier, Directeur)
- ID École : `13` (Ekelasi School)
- ID Agent : `139`
- Permissions : 70+ permissions chargées

**Vérifications** :
- ✅ Token valide
- ✅ Rôle Admin confirmé
- ✅ ID École présent
- ✅ ID Agent présent

---

## 📚 Test 2 : Voir mes devoirs (Admin) ✅

**Status** : ✅ **RÉUSSI**

**Comportement observé** :
- L'Admin peut voir **tous les devoirs de son école** (ID 13)
- Retourne une liste de devoirs publiés par différents agents de l'école
- Exemple de devoirs retournés :
  - Devoir publié par "Kabasele Holabo Elie" (Agent 285)
  - Devoir publié par Admin (Agent 139)

**Vérifications** :
- ✅ Status code : `200 OK`
- ✅ Retourne une liste de devoirs
- ✅ **IMPORTANT** : Retourne tous les devoirs de l'école, pas seulement ceux de l'Admin
- ✅ Tous les devoirs ont `idEcole: 13` (même école que l'Admin)

**Conclusion** : ✅ **Permissions correctes** - L'Admin voit bien tous les devoirs de son école

---

## 📖 Test 3 : Voir les devoirs d'une classe

**À tester** : Vérifier que l'Admin peut voir les devoirs d'une classe de son école

**Endpoint** :
```http
GET /api/DevoirADomicile/classe/{idClasse}
Authorization: Bearer {TOKEN}
```

---

## 📤 Test 4 : Publier un devoir (Admin)

**À tester** : Vérifier que l'Admin peut publier un devoir pour une classe de son école

**Endpoint** :
```http
POST /api/DevoirADomicile
Authorization: Bearer {TOKEN}
Content-Type: multipart/form-data

{
  "titre": "Test Devoir Admin",
  "description": "Description",
  "idClasse": {ID_CLASSE},
  "dateLimite": "2025-12-15T23:59:59",
  "fichier": [FICHIER_PDF]
}
```

**Vérifications à faire** :
- [ ] Le devoir est créé avec succès
- [ ] Le fichier est uploadé vers S3
- [ ] Le fichier apparaît dans le bucket `kansa-kelasinabiso-s3-bucket/devoirs/`
- [ ] Le devoir apparaît dans la liste "mes-devoirs"

---

## 🔍 Test 5 : Vérification S3

**À vérifier** :
1. Consulter les logs au démarrage de l'application
2. Chercher le message : `✅ Stockage AWS S3 configuré et activé`
3. Vérifier dans la console AWS S3 que les fichiers sont bien uploadés

---

## 📊 Résumé des Tests

| Test | Status | Notes |
|------|--------|-------|
| Authentification | ✅ RÉUSSI | Token généré, rôle Admin confirmé |
| Voir mes devoirs | ✅ RÉUSSI | Retourne tous les devoirs de l'école |
| Voir devoirs classe | ⏳ À TESTER | Nécessite un ID de classe valide |
| Publier devoir | ⏳ À TESTER | Nécessite un fichier PDF et ID classe |
| Télécharger devoir | ⏳ À TESTER | Nécessite un ID de devoir valide |
| Configuration S3 | ⏳ À VÉRIFIER | Vérifier les logs au démarrage |

---

## 🎯 Prochaines Étapes

1. ✅ Authentification : **FONCTIONNE**
2. ✅ Voir mes devoirs : **FONCTIONNE** (permissions correctes)
3. ⏳ Tester la publication d'un devoir avec un fichier PDF
4. ⏳ Vérifier que le fichier est bien uploadé vers S3
5. ⏳ Tester le téléchargement depuis S3

---

## 📝 Commandes Utiles

### Obtenir un token
```bash
TOKEN=$(curl -k -s -X POST "https://localhost:7102/api/Utilisateur/authentifier" \
  -H "Content-Type: application/json" \
  -d '{"emailOuTelephone":"jk2@kelasinabiso.cd","motDePasse":"12345678"}' \
  | jq -r '.accessToken')
```

### Voir mes devoirs
```bash
curl -k -X GET "https://localhost:7102/api/DevoirADomicile/mes-devoirs" \
  -H "Authorization: Bearer $TOKEN" | jq '.'
```

### Voir les devoirs d'une classe
```bash
curl -k -X GET "https://localhost:7102/api/DevoirADomicile/classe/43" \
  -H "Authorization: Bearer $TOKEN" | jq '.'
```

---

**Tests en cours...** 🧪

