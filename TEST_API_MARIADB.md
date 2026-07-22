# 🧪 Guide de Test - API KelasiNaBiso avec MariaDB

## ✅ L'API est démarrée et fonctionne !

**URL API** : http://localhost:5002  
**Swagger** : http://localhost:5002/swagger  
**Base de données** : MariaDB 10.11  

---

## 🎯 Tests rapides (5 minutes)

### Test 1 : Accès à Swagger ✅

**Action** : Ouvrez votre navigateur et allez sur :
```
http://localhost:5002/swagger
```

**Résultat attendu** :
- Page Swagger s'affiche
- Liste de tous les endpoints visible
- Interface interactive disponible

---

### Test 2 : Authentification Super-Admin ✅

**Endpoint** : `POST /api/Auth/login`

**Dans Swagger** :
1. Cliquez sur `POST /api/Auth/login`
2. Cliquez sur "Try it out"
3. Entrez ces données :
```json
{
  "email": "superadmin@kelasinabiso.cd",
  "password": "Super-Admin"
}
```
4. Cliquez sur "Execute"

**Résultat attendu** :
```json
{
  "message": "Connexion réussie",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "nom": "Super",
    "postnom": "Admin",
    "prenom": "Administrateur",
    "email": "superadmin@kelasinabiso.cd",
    "role": "Super-Admin",
    "ecole": "Ekelasi School"
  }
}
```

---

### Test 3 : Récupérer les écoles ✅

**Endpoint** : `GET /api/Ecole`

**Dans Swagger** :
1. Cliquez sur `GET /api/Ecole`
2. Cliquez sur "Try it out"
3. Cliquez sur "Execute"

**Résultat attendu** :
```json
[
  {
    "idEcole": 1,
    "nom": "Ekelasi School",
    "slogan": "Excellence et Innovation",
    "type": "Privée",
    ...
  }
]
```

---

### Test 4 : Récupérer les utilisateurs ✅

**Endpoint** : `GET /api/V_Utilisateur`

**Dans Swagger** :
1. Cliquez sur `GET /api/V_Utilisateur`
2. Cliquez sur "Try it out"
3. Cliquez sur "Execute"

**Résultat attendu** :
```json
[
  {
    "idUtilisateur": 1,
    "nomUtilisateur": "Super",
    "postNomUtilisateur": "Admin",
    "prenomUtilisateur": "Administrateur",
    "email": "superadmin@kelasinabiso.cd",
    "nomRole": "Super-Admin",
    "nomEcole": "Ekelasi School",
    ...
  }
]
```

---

### Test 5 : Vérifier les agents ✅

**Endpoint** : `GET /api/Agent`

**Dans Swagger** :
1. Cliquez sur `GET /api/Agent`
2. Cliquez sur "Try it out"
3. Cliquez sur "Execute"

**Résultat attendu** :
```json
[]  // Liste vide si aucun agent créé encore
```

**Status** : ✅ Pas d'erreur = succès !

---

## 🧪 Tests via PowerShell

### Test authentification
```powershell
$body = @{
    email = "superadmin@kelasinabiso.cd"
    password = "Super-Admin"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5002/api/Auth/login" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"

$response
```

### Test récupération écoles
```powershell
Invoke-RestMethod -Uri "http://localhost:5002/api/Ecole" -Method Get
```

### Test récupération agents
```powershell
Invoke-RestMethod -Uri "http://localhost:5002/api/Agent" -Method Get
```

---

## 🧪 Tests SQL directs dans MariaDB

### Se connecter à MariaDB
```bash
mysql -u kansa -pkansa2025
```

### Vérifier la version
```sql
SELECT VERSION();
-- Résultat attendu: 10.11.x-MariaDB
```

### Vérifier les tables
```sql
USE KelasiNaBisoDb;
SHOW TABLES;
-- Devrait afficher ~30 tables
```

### Vérifier les vues
```sql
SELECT TABLE_NAME 
FROM information_schema.VIEWS 
WHERE TABLE_SCHEMA = 'KelasiNaBisoDb';

-- Résultat attendu:
-- V_Utilisateur
-- V_Eleve
-- EleveParEcole
-- VuePaiementsFraisParEcole
-- VuePointagePresenceParEcole
-- Vue_RepertoireAgentsParParent
```

### Vérifier les données
```sql
-- Rôles
SELECT * FROM Roles;
-- Devrait contenir: Super-Admin

-- Écoles
SELECT * FROM Ecoles;
-- Devrait contenir: Ekelasi School

-- Utilisateurs
SELECT IdUtilisateur, Email, NomUtilisateur FROM Utilisateurs;
-- Devrait contenir: superadmin@kelasinabiso.cd

-- Agents
SELECT * FROM Agents;
-- Peut être vide si aucun agent créé
```

---

## 🎯 Checklist complète

### Infrastructure
- [x] ✅ MariaDB 10.11 installé
- [x] ✅ Service MariaDB démarré
- [x] ✅ Base de données créée
- [x] ✅ Utilisateur kansa configuré

### Code
- [x] ✅ Packages NuGet restaurés
- [x] ✅ Projet compilé sans erreur
- [x] ✅ Configuration MariaDB active
- [x] ✅ Code obsolète supprimé

### Base de données
- [x] ✅ Migrations appliquées
- [x] ✅ Tables créées (~30)
- [x] ✅ Vues créées (6)
- [x] ✅ Données initialisées

### API
- [x] ✅ Application démarrée
- [x] ✅ Ports HTTP/HTTPS actifs
- [x] ✅ Swagger accessible
- [ ] 🧪 Authentification testée
- [ ] 🧪 Endpoints testés

---

## 📝 Tests fonctionnels complets

### Scénario 1 : Créer une nouvelle école

```json
POST /api/Ecole
{
  "nom": "École Test",
  "slogan": "Apprendre ensemble",
  "type": "Privée",
  "provinceEducationnel": "Kinshasa",
  "nomCompletResponsable": "Jean Dupont",
  "description": "École de test",
  "province": "Kinshasa",
  "ville": "Kinshasa"
}
```

**Résultat attendu** :
- École créée ✅
- Utilisateur Admin créé automatiquement ✅

---

### Scénario 2 : Créer un agent/enseignant

```json
POST /api/Agent
{
  "matricule": "AG001",
  "nom": "Kabila",
  "postnom": "Mbuyi",
  "prenom": "Joseph",
  "genre": "M",
  "dateNaissance": "1980-05-15",
  "telephoneAgent": "+243999111222",
  "emailAgent": "joseph.kabila@test.cd",
  "etatCivil": "Marié",
  "fonction": "Enseignant",
  "roleAgent": "Professeur de Mathématiques",
  "idEcole": 1
}
```

**Résultat attendu** :
- Agent créé ✅
- ID retourné ✅

---

### Scénario 3 : Récupérer les agents d'une école

```
GET /api/Agent/ecole/1
```

**Résultat attendu** :
- Liste des agents de l'école ID 1 ✅

---

### Scénario 4 : Vue répertoire agents pour parents

```
GET /api/VueRepertoireAgentsParParent/ecole/1
```

**Résultat attendu** :
- Répertoire complet avec agents, cours, élèves ✅

---

## ⚡ Tests de performance

### Test de charge simple
```powershell
# Test 100 requêtes
1..100 | ForEach-Object {
    Invoke-RestMethod -Uri "http://localhost:5002/api/Ecole" -Method Get
}
```

**Résultat attendu** :
- Toutes les requêtes réussies ✅
- Temps de réponse < 200ms ✅

---

## 🐛 Debugging

### Si l'API ne démarre pas

1. **Vérifier MariaDB**
```bash
# Vérifier le service
Get-Service -Name *maria*,*mysql*

# Tester la connexion
mysql -u kansa -pkansa2025 -e "SELECT VERSION();"
```

2. **Vérifier les logs de l'API**
```bash
dotnet run --verbosity detailed
```

3. **Vérifier la chaîne de connexion**
```bash
# Dans appsettings.json
"KelasiConnection": "Server=localhost;Database=KelasiNaBisoDb;User=kansa;Password=kansa2025;Port=3306;..."
```

---

### Si les endpoints retournent des erreurs

1. **Vérifier les migrations**
```bash
dotnet ef database update
```

2. **Vérifier les vues SQL**
```sql
SHOW FULL TABLES WHERE Table_type = 'VIEW';
```

3. **Consulter les logs de l'API**
Regardez la console pour les erreurs détaillées.

---

## 🎊 Félicitations !

Votre API **KelasiNaBiso** fonctionne maintenant avec **MariaDB 10.11** et est **nettoyée** de tout code obsolète !

### Prochaines actions
1. 🧪 Testez tous les endpoints dans Swagger
2. 📱 Mettez à jour votre frontend (si applicable)
3. 📝 Mettez à jour votre collection Postman
4. 🚀 Déployez en production (après sécurisation)

---

**Bon testing ! 🚀**

---

*Guide de test créé le: 23 octobre 2025*  
*API: KelasiNaBiso v1*  
*Base de données: MariaDB 10.11 LTS*

