# Architecture KelasiNaBiso API

## Vue d'ensemble

Cette API suit une architecture en couches avec le pattern Repository et une séparation claire des responsabilités.

## Structure de l'Architecture

```
📁 KelasiNaBisoAPI/
├── 📁 Controllers/           # Couche de présentation (API Controllers)
├── 📁 Services/             # Couche de service (Business Logic)
│   └── 📁 Repositories/     # Interfaces des repositories
├── 📁 Models/               # Couche de données (Entities)
├── 📁 Data/                 # Couche d'accès aux données
└── 📁 Program.cs            # Configuration et DI
```

## Couches de l'Architecture

### 1. **Couche de Présentation (Controllers)**
Responsable de :
- Réception des requêtes HTTP
- Validation des données d'entrée
- Retour des réponses HTTP
- Gestion des erreurs

**Contrôleurs disponibles :**
- `EleveController` - Gestion des élèves
- `EcoleController` - Gestion des écoles
- `ClasseController` - Gestion des classes
- `UtilisateurController` - Gestion des utilisateurs
- `TuteurController` - Gestion des tuteurs
- `NoteController` - Gestion des notes
- `CoursController` - Gestion des cours
- `InscriptionController` - Gestion des inscriptions
- `PresenceController` - Gestion des présences
- `VacationController` - Gestion des Vacations
- `EnseignantController` - Gestion des enseignants
- `AnneeScolaireController` - Gestion des années scolaires
- `FraisController` - Gestion des frais
- `PaiementController` - Gestion des paiements
- `RoleController` - Gestion des rôles
- `SectionController` - Gestion des sections
- `OptionController` - Gestion des options
- `MessageController` - Gestion des messages
- `GroupeMessageController` - Gestion des groupes de messages
- `DocumentController` - Gestion des documents
- `RessourcePedagogiqueController` - Gestion des ressources pédagogiques
- `EvaluationController` - Gestion des évaluations

### 2. **Couche de Service (Services)**
Responsable de :
- Logique métier
- Orchestration des opérations
- Validation métier
- Gestion des transactions

**Services disponibles :**
- `EleveService` - Logique métier des élèves
- `EcoleService` - Logique métier des écoles
- `ClasseService` - Logique métier des classes
- `UtilisateurService` - Logique métier des utilisateurs
- `TuteurService` - Logique métier des tuteurs
- `NoteService` - Logique métier des notes
- `CoursService` - Logique métier des cours
- `InscriptionService` - Logique métier des inscriptions
- `PresenceService` - Logique métier des présences
- `VacationService` - Logique métier des Vacations
- `EnseignantService` - Logique métier des enseignants
- `AnneeScolaireService` - Logique métier des années scolaires
- `FraisService` - Logique métier des frais
- `PaiementService` - Logique métier des paiements
- `RoleService` - Logique métier des rôles
- `SectionService` - Logique métier des sections
- `OptionService` - Logique métier des options
- `MessageService` - Logique métier des messages
- `GroupeMessageService` - Logique métier des groupes de messages
- `DocumentService` - Logique métier des documents
- `RessourcePedagogiqueService` - Logique métier des ressources pédagogiques
- `EvaluationService` - Logique métier des évaluations

### 3. **Couche d'Accès aux Données (Repositories)**
Responsable de :
- Accès aux données
- Opérations CRUD
- Requêtes complexes
- Gestion des relations

**Interfaces Repository disponibles :**
- `IEleveRepository`
- `IEcoleRepository`
- `IClasseRepository`
- `IUtilisateurRepository`
- `ITuteurRepository`
- `INoteRepository`
- `ICoursRepository`
- `IInscriptionRepository`
- `IPresenceRepository`
- `IVacationRepository`
- `IEnseignantRepository`
- `IAnneeScolaireRepository`
- `IFraisRepository`
- `IPaiementRepository`
- `IRoleRepository`
- `ISectionRepository`
- `IOptionRepository`
- `IMessageRepository`
- `IGroupeMessageRepository`
- `IDocumentRepository`
- `IRessourcePedagogiqueRepository`
- `IEvaluationRepository`

### 4. **Couche de Données (Models)**
Responsable de :
- Définition des entités
- Relations entre entités
- Validation des données
- Configuration Entity Framework

**Modèles disponibles :**
- `Eleve` - Entité élève
- `Ecole` - Entité école
- `Classe` - Entité classe
- `Utilisateur` - Entité utilisateur
- `Tuteur` - Entité tuteur
- `Note` - Entité note
- `Cours` - Entité cours
- `Inscription` - Entité inscription
- `Presence` - Entité présence
- `Vacation` - Entité Vacation
- `Enseignant` - Entité enseignant
- `AnneeScolaire` - Entité année scolaire
- `Frais` - Entité frais
- `Paiement` - Entité paiement
- `Role` - Entité rôle
- `Section` - Entité section
- `Option` - Entité option
- `Message` - Entité message
- `GroupeMessage` - Entité groupe de messages
- `Document` - Entité document
- `RessourcePedagogique` - Entité ressource pédagogique
- `Evaluation` - Entité évaluation
- `Adresse` - Classe abstraite pour les adresses

## Pattern Repository

Chaque entité suit le pattern Repository avec les opérations standard :

```csharp
public interface IEntityRepository
{
    Task<IEnumerable<Entity>> GetAllAsync();
    Task<Entity> GetByIdAsync(int id);
    Task<Entity> CreateAsync(Entity entity);
    Task<Entity> UpdateAsync(Entity entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    // Méthodes spécifiques selon l'entité
}
```

## Injection de Dépendances

Tous les services sont enregistrés dans le conteneur DI dans `Program.cs` :

```csharp
// Enregistrement des repositories
builder.Services.AddScoped<IEleveRepository, EleveService>();
builder.Services.AddScoped<IEcoleRepository, EcoleService>();
// ... autres services
```

## Flux de Données

1. **Requête HTTP** → Controller
2. **Controller** → Service (Business Logic)
3. **Service** → Repository (Data Access)
4. **Repository** → Entity Framework → Base de données
5. **Réponse** → Repository → Service → Controller → Client

## Avantages de cette Architecture

### ✅ **Séparation des Responsabilités**
- Chaque couche a une responsabilité claire
- Facilite la maintenance et les tests

### ✅ **Testabilité**
- Interfaces permettent le mocking
- Tests unitaires facilités
- Tests d'intégration possibles

### ✅ **Évolutivité**
- Ajout facile de nouvelles fonctionnalités
- Modification d'une couche sans affecter les autres
- Réutilisation du code

### ✅ **Maintenabilité**
- Code organisé et structuré
- Documentation claire
- Patterns standards

### ✅ **Flexibilité**
- Changement de base de données facile
- Ajout de nouvelles couches possible
- Support de multiples clients

## Endpoints API Standard

Chaque contrôleur expose les endpoints REST standard :

```
GET    /api/Entity           # Récupérer toutes les entités
GET    /api/Entity/{id}      # Récupérer une entité par ID
POST   /api/Entity           # Créer une nouvelle entité
PUT    /api/Entity/{id}      # Mettre à jour une entité
DELETE /api/Entity/{id}      # Supprimer une entité
```

Plus des endpoints spécifiques selon les besoins métier.

## Validation et Sécurité

- **Validation des modèles** avec Data Annotations
- **Exclusion de la sérialisation** avec `[JsonIgnore]`
- **Exclusion de la validation** avec `[ValidateNever]`
- **Authentification** intégrée
- **Gestion des erreurs** centralisée

## Configuration

- **Base de données** : SQL Server avec Entity Framework Core
- **Ports** : HTTP 5002, HTTPS 7102
- **CORS** : Configuration pour le frontend
- **Swagger** : Documentation API automatique

Cette architecture garantit un code robuste, maintenable et évolutif pour votre système de gestion scolaire.
