# 🏫 CRÉATION MANAGER GÉNÉRAL ÉCOLE - Documentation Finale

## 📅 Date de finalisation
**27 octobre 2025**

---

## 🎯 Principe fondamental

**Règle métier stricte :** Un utilisateur dans le système est **TOUJOURS** :
- 🧑‍🏫 Un **Agent** (enseignant, manager, personnel)
- 👨‍👩‍👧 Un **Parent** d'élève (Tuteur)

**Il ne peut pas exister d'utilisateur "orphelin" sans lien.**

---

## 👔 Rôle du Manager Général

Le **Manager Général** est :
- ✅ Un **Agent** avec la fonction spécifique "Manager Général"
- ✅ Le **responsable principal** de l'école
- ✅ Créé **automatiquement** lors de la création de l'école
- ✅ Possède un compte **Utilisateur avec rôle Admin**

**Différence avec Directeur :**
- **Directeur** : Responsable pédagogique (peut être créé manuellement)
- **Manager Général** : Responsable administratif et opérationnel global de l'école

---

## 🔄 Processus de création automatique

### Vue d'ensemble

```
POST /api/Ecole
    ↓
1️⃣ École créée en BDD
    ↓
2️⃣ Agent "Manager Général" créé automatiquement
    ├─ Fonction : "Manager Général"
    ├─ Matricule : NAT25-A3F2B1 (généré unique)
    ├─ EmailAgent : ecole.EmailContact
    └─ IdEcole : lié à l'école
    ↓
3️⃣ Utilisateur Admin créé automatiquement
    ├─ IdAgent : lié au Manager Général ✅
    ├─ IdRole : Admin
    ├─ Email : emailAgent du Manager
    ├─ Username : généré unique (ex: JeanMukendi7342)
    └─ MotDePasse : "Admin" (doit changer)
    ↓
4️⃣ Email de bienvenue envoyé
    └─ Contient : Matricule, Fonction, Username, MotDePasse
```

---

## ⚙️ Implémentation détaillée

### 1️⃣ **Parsing du nom du responsable**

```csharp
string nomCompletResponsable = "Jean Mukendi Kalala";
string[] partiesNom = nomCompletResponsable.Split(' ');

string prenom = partiesNom[0];   // "Jean"
string nom = partiesNom[1];      // "Mukendi"
string postnom = partiesNom[2];  // "Kalala"
```

**Gestion des cas :**
- 1 mot : `"Manager"` → Prenom="Manager", Nom="General", Postnom=""
- 2 mots : `"Jean Mukendi"` → Prenom="Jean", Nom="Mukendi", Postnom=""
- 3+ mots : `"Jean Mukendi Kalala"` → Prenom="Jean", Nom="Mukendi", Postnom="Kalala"

---

### 2️⃣ **Création de l'Agent Manager Général**

```csharp
var managerAgent = new Agent
{
    // Identité (parsée du NomCompletResponsable)
    Prenom = "Jean",
    Nom = "Mukendi",
    Postnom = "Kalala",
    Genre = ecole.GenreResponsable ?? "Masculin",
    DateNaissance = DateTime.Now.AddYears(-35), // 35 ans par défaut
    
    // Contact
    TelephoneAgent = ecole.Telephone,
    EmailAgent = ecole.EmailContact,
    
    // Fonction spécifique
    Fonction = "Manager Général",    // ✨ FONCTION CLÉ
    RoleAgent = "Administrateur",
    EtatCivil = "Marié",
    
    // Matricule (généré unique)
    Matricule = "NAT25-C9D3E7",
    
    // Liens
    IdEcole = ecole.IdEcole,
    
    // Adresse (copiée de l'école)
    Province = ecole.Province,
    Ville = ecole.Ville,
    Commune = ecole.Commune,
    Quartier = ecole.Quartier,
    Avenue = ecole.Avenue,
    Numero = ecole.Numero,
    
    // Technique
    Statut = true,
    DateCreation = DateTime.Now
};
```

---

### 3️⃣ **Génération du matricule**

```csharp
private async Task<string> GenerateMatriculeManagerGeneral(Ecole ecole)
{
    string matricule;
    
    do
    {
        // Format national standard pour tous les agents
        string annee = DateTime.Now.Year.ToString().Substring(2); // "25"
        string guid = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(); // "C9D3E7"
        matricule = $"NAT{annee}-{guid}"; // "NAT25-C9D3E7"
        
    } while (await _context.Agents.AnyAsync(a => a.Matricule == matricule));
    
    return matricule;
}
```

**Format :** `NAT[Année]-[GUID(6)]`  
**Exemple :** `NAT25-C9D3E7`  
**Unicité :** Garantie par boucle de vérification

---

### 4️⃣ **Création de l'Utilisateur Admin**

```csharp
var adminUser = new Utilisateur
{
    IdAgent = managerAgent.IdAgent,      // ✨ LIEN ESSENTIEL
    IdRole = adminRole.IdRole,            // Rôle "Admin"
    
    // Identité (copiée de l'agent)
    NomUtilisateur = managerAgent.Nom,
    PostNomUtilisateur = managerAgent.Postnom,
    PrenomUtilisateur = managerAgent.Prenom,
    Genre = managerAgent.Genre,
    DateNaissance = managerAgent.DateNaissance,
    
    // Authentification
    Email = managerAgent.EmailAgent,
    DefaultUsername = "JeanMukendiKalala7342", // Généré unique
    MotDePasseHash = BCrypt.HashPassword("Admin"),
    DoitChangerMotDePasse = true,         // ✨ Forcer changement
    
    // Liens
    IdEcole = ecole.IdEcole,
    
    // Contact et adresse
    Telephone = managerAgent.TelephoneAgent,
    PhotoUrl = managerAgent.PhotoUrl,
    Province = managerAgent.Province,
    // ... autres champs adresse
};
```

---

## 📊 Relations créées dans la base de données

```
┌─────────────────┐
│  ÉCOLE          │
│  IdEcole = 5    │
└────────┬────────┘
         │
         ├──────────────────────────────────┐
         │                                  │
         ▼                                  ▼
┌─────────────────┐              ┌─────────────────┐
│  AGENT          │              │  UTILISATEUR    │
│  IdAgent = 12   │◄─────────────┤  IdUtilisateur  │
│  Fonction =     │   IdAgent=12 │  = 25           │
│  "Manager       │              │  IdRole = 2     │
│   Général"      │              │  (Admin)        │
│  Matricule =    │              │  Email = ...    │
│  NAT25-C9D3E7   │              │  Username = ... │
└─────────────────┘              └─────────────────┘
```

---

## ✅ Validations appliquées

### 1. Vérification unicité email (avant Agent)
```csharp
var emailExists = await _context.Utilisateurs.AnyAsync(u => u.Email == emailDirecteur);
if (emailExists)
{
    Console.WriteLine("⚠️ Email déjà utilisé. Agent Manager non créé.");
    return; // Arrêt gracieux
}
```

### 2. Vérification unicité matricule (boucle)
```csharp
do {
    matricule = $"NAT{annee}-{GUID}";
} while (await _context.Agents.AnyAsync(a => a.Matricule == matricule));
```

### 3. Vérification unicité username (boucle + fallback)
```csharp
// Méthode GenerateUniqueUsernameAsync()
// - Max 100 tentatives avec vérification BDD
// - Fallback GUID si toutes les tentatives échouent
// - Plage 1-9999 (au lieu de 1-999)
```

### 4. Vérification unicité email (avant Utilisateur - double sécurité)
```csharp
var emailExists = await _context.Utilisateurs.AnyAsync(u => u.Email == emailAdmin);
if (emailExists)
{
    Console.WriteLine("⚠️ Email déjà utilisé. Utilisateur admin non créé.");
    return;
}
```

---

## 🎯 Fonctionnalités du Manager Général

Le Manager Général peut :

| Fonctionnalité | Disponible | Détails |
|----------------|------------|---------|
| **Se connecter** | ✅ Oui | Email + mot de passe "Admin" |
| **Accès admin complet** | ✅ Oui | Rôle "Admin" sur toute l'école |
| **Pointer sa présence** | ✅ Oui | Comme tout agent |
| **Être affecté à des cours** | ✅ Oui | Optionnel |
| **Apparaître dans liste Agents** | ✅ Oui | Avec fonction "Manager Général" |
| **Gérer l'école** | ✅ Oui | Droits administrateur |
| **Avoir un matricule** | ✅ Oui | Format NAT25-... |
| **Recevoir notifications** | ✅ Oui | Via email et/ou app |

---

## 🧪 Scénario de test complet

### Création d'une école

**Requête :**
```http
POST /api/Ecole
Content-Type: application/json

{
  "nom": "École Primaire Kasai",
  "emailContact": "manager@kasai.cd",
  "telephone": "+243999111222",
  "nomCompletResponsable": "Jean Mukendi Kalala",
  "genreResponsable": "Masculin",
  "province": "Kasaï",
  "ville": "Kananga"
}
```

**Résultats en BDD :**

```sql
-- 1. ÉCOLE
INSERT INTO Ecoles (Nom, EmailContact, Telephone, NomCompletResponsable, ...)
VALUES ('École Primaire Kasai', 'manager@kasai.cd', '+243999111222', 'Jean Mukendi Kalala', ...);
-- IdEcole = 5

-- 2. AGENT MANAGER GÉNÉRAL
INSERT INTO Agents (Prenom, Nom, Postnom, Fonction, Matricule, EmailAgent, IdEcole, ...)
VALUES ('Jean', 'Mukendi', 'Kalala', 'Manager Général', 'NAT25-C9D3E7', 'manager@kasai.cd', 5, ...);
-- IdAgent = 12

-- 3. UTILISATEUR ADMIN
INSERT INTO Utilisateurs (IdAgent, IdRole, Email, DefaultUsername, MotDePasseHash, IdEcole, ...)
VALUES (12, 2, 'manager@kasai.cd', 'JeanMukendiKalala7342', '$2a$11$...', 5, ...);
-- IdUtilisateur = 25
```

**Email envoyé à `manager@kasai.cd` :**
```
Bonjour Jean Mukendi Kalala,

Votre compte Manager Général a été créé pour l'école "École Primaire Kasai".

Vos identifiants de connexion :
- Email : manager@kasai.cd
- Username : JeanMukendiKalala7342
- Téléphone : +243999111222
- Mot de passe : Admin

Votre matricule agent : NAT25-C9D3E7
Votre fonction : Manager Général

⚠️ Vous devez changer votre mot de passe à la première connexion.

Bienvenue dans KelasiNaBiso !
```

---

## 🔍 Requêtes SQL utiles

### Trouver le Manager Général d'une école

```sql
SELECT 
    a.IdAgent,
    a.Matricule,
    a.Fonction,
    CONCAT(a.Prenom, ' ', a.Nom, ' ', a.Postnom) AS NomComplet,
    a.EmailAgent,
    u.DefaultUsername,
    u.Email AS EmailUtilisateur,
    r.Nom AS Role
FROM Agents a
INNER JOIN Utilisateurs u ON u.IdAgent = a.IdAgent
INNER JOIN Roles r ON u.IdRole = r.IdRole
WHERE a.IdEcole = 5 
  AND a.Fonction = 'Manager Général';
```

### Lister tous les Managers Généraux du système

```sql
SELECT 
    e.Nom AS NomEcole,
    a.Matricule,
    CONCAT(a.Prenom, ' ', a.Nom, ' ', a.Postnom) AS ManagerGeneral,
    a.EmailAgent,
    u.DefaultUsername
FROM Agents a
INNER JOIN Ecoles e ON e.IdEcole = a.IdEcole
INNER JOIN Utilisateurs u ON u.IdAgent = a.IdAgent
WHERE a.Fonction = 'Manager Général'
ORDER BY e.Nom;
```

---

## 📋 Hiérarchie typique d'une école

```
ÉCOLE "Kasai School"
    │
    ├─ Manager Général (Agent + Utilisateur Admin)
    │  └─ Gestion administrative globale
    │
    ├─ Directeur (Agent + Utilisateur, peut être créé manuellement)
    │  └─ Gestion pédagogique
    │
    ├─ Enseignants (Agents + Utilisateurs)
    │  └─ Enseignement des cours
    │
    └─ Personnel (Agents + Utilisateurs)
       └─ Support administratif
```

**Le Manager Général est au sommet de la hiérarchie administrative.**

---

## 🔐 Droits et permissions du Manager Général

### Droits Admin (complets)

| Action | Autorisé |
|--------|----------|
| Créer/modifier agents | ✅ Oui |
| Créer/modifier classes | ✅ Oui |
| Créer/modifier élèves | ✅ Oui |
| Consulter toutes les données | ✅ Oui |
| Gérer les inscriptions | ✅ Oui |
| Gérer les paiements | ✅ Oui |
| Pointer sa présence | ✅ Oui |
| Créer d'autres utilisateurs | ✅ Oui |
| Modifier paramètres école | ✅ Oui |

**C'est le compte le plus puissant de l'école.**

---

## 📝 Données créées (exemple complet)

### École
```json
{
  "idEcole": 5,
  "nom": "École Primaire Kasai",
  "emailContact": "manager@kasai.cd",
  "telephone": "+243999111222",
  "nomCompletResponsable": "Jean Mukendi Kalala",
  "genreResponsable": "Masculin",
  "statut": true
}
```

### Agent Manager Général
```json
{
  "idAgent": 12,
  "prenom": "Jean",
  "nom": "Mukendi",
  "postnom": "Kalala",
  "genre": "Masculin",
  "dateNaissance": "1990-10-27",
  "telephoneAgent": "+243999111222",
  "emailAgent": "manager@kasai.cd",
  "fonction": "Manager Général",
  "roleAgent": "Administrateur",
  "matricule": "NAT25-C9D3E7",
  "etatCivil": "Marié",
  "idEcole": 5,
  "statut": true
}
```

### Utilisateur Admin
```json
{
  "idUtilisateur": 25,
  "idAgent": 12,
  "idRole": 2,
  "nomUtilisateur": "Mukendi",
  "postnomUtilisateur": "Kalala",
  "prenomUtilisateur": "Jean",
  "email": "manager@kasai.cd",
  "defaultUsername": "JeanMukendiKalala7342",
  "telephone": "+243999111222",
  "genre": "Masculin",
  "doitChangerMotDePasse": true,
  "idEcole": 5,
  "statut": true
}
```

---

## 🎯 Cas d'usage pratiques

### 1. Premier login du Manager Général

```javascript
// Le Manager Général se connecte pour la première fois
const response = await fetch('/api/Utilisateur/authentifier', {
    method: 'POST',
    body: JSON.stringify({
        emailOuTelephone: "manager@kasai.cd",
        motDePasse: "Admin"
    })
});

if (response.ok) {
    const data = await response.json();
    
    // Vérifier si doit changer mot de passe
    if (data.doitChangerMotDePasse) {
        // Rediriger vers page changement mot de passe
        redirectToChangePassword();
    }
}
```

---

### 2. Le Manager Général crée un Directeur

```javascript
// Le Manager Général peut créer un Directeur pédagogique
await fetch('/api/Agent', {
    method: 'POST',
    headers: { 'Authorization': `Bearer ${token}` },
    body: JSON.stringify({
        nom: "Kabongo",
        prenom: "Paul",
        postnom: "Mbuyi",
        fonction: "Directeur", // ✨ Directeur pédagogique
        emailAgent: "directeur@kasai.cd",
        idEcole: 5
    })
});

// Un compte utilisateur sera créé automatiquement pour ce Directeur
```

---

### 3. Pointage de présence du Manager

```javascript
// Le Manager Général peut pointer sa présence
await fetch('/api/Presence', {
    method: 'POST',
    headers: { 'Authorization': `Bearer ${token}` },
    body: JSON.stringify({
        idAgent: 12, // ID du Manager Général
        dateDuJour: "2025-10-27",
        heureArrivee: "08:00:00"
    })
});
```

---

## ⚠️ Cas d'échec et gestion

### Cas 1 : Email déjà utilisé

**Situation :** L'email `manager@kasai.cd` existe déjà dans Utilisateurs

**Comportement :**
```
École créée ✅
    ↓
Vérification email → ❌ Existe déjà
    ↓
Agent Manager NON créé ⚠️
Utilisateur Admin NON créé ⚠️
    ↓
Log : "Un utilisateur avec l'email 'manager@kasai.cd' existe déjà. 
       Agent directeur non créé pour l'école 'École Primaire Kasai'."
```

**Solution manuelle :**
1. Créer manuellement un Agent avec fonction "Manager Général"
2. Le système créera automatiquement son Utilisateur
3. Utiliser un email différent

---

### Cas 2 : Nom responsable invalide

**Situation :** `nomCompletResponsable = NULL` ou vide

**Comportement :**
```
Valeur par défaut utilisée : "Manager General"
    ↓
Agent créé :
    Prenom = "Manager"
    Nom = "General"
    Postnom = ""
```

---

## 🔗 Cohérence avec le reste du système

| Entité créée | Processus | Résultat |
|--------------|-----------|----------|
| **École** | Création école → Agent Manager + Utilisateur Admin | IdAgent lié ✅ |
| **Agent normal** | Création agent → Utilisateur Agent | IdAgent lié ✅ |
| **Élève** | Inscription → Élève créé (Tuteur déjà existe) | IdTuteur lié ✅ |
| **Tuteur** | Création tuteur → Peut créer Utilisateur Parent | IdTuteur lié ✅ |

**Principe unifié :** Toujours un lien Agent OU Tuteur, jamais orphelin.

---

## 📊 Différenciation des fonctions

| Fonction | Niveau | Responsabilité | Création |
|----------|--------|----------------|----------|
| **Manager Général** | École | Administration globale | Automatique (création école) |
| **Directeur** | Pédagogique | Direction pédagogique | Manuelle |
| **Professeur** | Classe/Cours | Enseignement | Manuelle |
| **Secrétaire** | Administratif | Support admin | Manuelle |

---

## ✅ Checklist d'implémentation

- [x] Fonction changée : "Directeur" → "Manager Général"
- [x] Méthode renommée : `GenerateMatriculeManagerGeneral()`
- [x] Variable renommée : `directeurAgent` → `managerAgent`
- [x] Commentaires mis à jour
- [x] Logs mis à jour
- [x] Email de bienvenue mis à jour
- [x] Documentation créée

---

## 🚀 Prochaines étapes suggérées

### Gestion hiérarchique
- [ ] Créer un champ `IdManager` dans `Agent` pour lier les agents à leur manager
- [ ] Interface pour voir la hiérarchie de l'école
- [ ] Délégation de permissions du Manager vers Directeur

### Améliorations futures
- [ ] Permettre plusieurs Managers par école (Manager Général + Managers de sites)
- [ ] Système de signature électronique pour le Manager
- [ ] Tableau de bord spécifique Manager Général
- [ ] Notifications automatiques au Manager pour événements importants

---

**🎉 Le Manager Général de l'école est maintenant correctement créé selon votre logique métier !**

Un utilisateur est **toujours** lié à un Agent (Manager, Enseignant, etc.) OU à un Tuteur (Parent), garantissant la cohérence du système.

