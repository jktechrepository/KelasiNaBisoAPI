# 📁 Structure du Projet KelasiNaBiso API

> **Framework**: ASP.NET Core 8.0  
> **Base de données**: MySQL/MariaDB  
> **Architecture**: Clean Architecture avec Repository Pattern

---

## 🗂️ Vue d'Ensemble

```
KelasiNaBisoAPI/
├── 📁 Controllers/          # Contrôleurs API (endpoints)
├── 📁 Models/               # Entités et DTOs
├── 📁 Data/                 # DbContext et migrations
├── 📁 Repositories/         # Accès aux données
├── 📁 Services/             # Logique métier
├── 📁 Attributes/           # Attributs personnalisés (ex: [Permission])
├── 📁 Migrations/           # Migrations Entity Framework
├── 📄 Program.cs            # Point d'entrée de l'application
├── 📄 appsettings.json      # Configuration de l'application
└── 📄 KelasiNaBiso.csproj   # Fichier de projet
```

---

## 📂 Détails des Dossiers

### 1️⃣ **Controllers/** - Contrôleurs API

Les contrôleurs gèrent les **requêtes HTTP** et retournent les **réponses**.

```
Controllers/
├── UtilisateurController.cs     # Authentification, gestion des utilisateurs
├── EcoleController.cs           # CRUD des écoles
├── EleveController.cs           # Gestion des élèves
├── PaiementController.cs        # Gestion des paiements
├── NoteController.cs            # Gestion des notes
├── AgentController.cs           # Gestion des agents/enseignants
├── ClasseController.cs          # Gestion des classes
├── PermissionController.cs      # Gestion RBAC des permissions
└── ...
```

#### Exemple de Contrôleur

```csharp
// Controllers/EcoleController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize] // 🔒 Nécessite authentification
public class EcoleController : ControllerBase
{
    private readonly IEcoleRepository _repository;
    
    public EcoleController(IEcoleRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [Permission("Ecole.ReadAll")] // 🔐 Nécessite permission
    public async Task<ActionResult<IEnumerable<Ecole>>> GetEcoles()
    {
        var ecoles = await _repository.GetAllAsync();
        return Ok(ecoles);
    }
}
```

---

### 2️⃣ **Models/** - Entités et DTOs

Les modèles représentent les **tables de la base de données** et les **objets de transfert de données**.

```
Models/
├── 📊 Entités (Base de données)
│   ├── Ecole.cs                 # École (table Ecole)
│   ├── Utilisateur.cs           # Utilisateur (table Utilisateur)
│   ├── Eleve.cs                 # Élève (table Eleve)
│   ├── Paiement.cs              # Paiement (table Paiement)
│   ├── Note.cs                  # Note (table Note)
│   ├── Role.cs                  # Rôle (table Role)
│   ├── Permission.cs            # Permission (table Permission)
│   └── RolePermission.cs        # Relation Role-Permission
│
├── 📦 DTOs (Data Transfer Objects)
│   ├── AuthentificationRequest.cs    # DTO pour se connecter
│   ├── AuthentificationResponse.cs   # DTO de réponse de connexion
│   ├── CreateEcoleDto.cs            # DTO pour créer une école
│   └── PaginatedResult.cs           # DTO pour pagination
│
└── 📋 Enums
    ├── UserRoles.cs             # Énumération des rôles
    └── StatutEleve.cs           # Statuts des élèves
```

#### Exemple d'Entité

```csharp
// Models/Ecole.cs
public class Ecole
{
    [Key]
    public int IdEcole { get; set; }
    
    public string ReferenceEcole { get; set; }
    
    [Required]
    public string Nom { get; set; }
    
    public string? Slogan { get; set; }
    public string? Type { get; set; }
    public bool Statut { get; set; } = true;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    
    // Relations
    public ICollection<Utilisateur> Utilisateurs { get; set; }
    public ICollection<Eleve> Eleves { get; set; }
}
```

---

### 3️⃣ **Data/** - DbContext et Configuration

Contient le **contexte de base de données** et la **configuration EF Core**.

```
Data/
├── KelasiNaBisoDbContext.cs     # Contexte principal
├── PermissionSeeder.cs          # Seed des permissions par défaut
└── Configurations/              # Configurations Fluent API (si nécessaire)
```

#### Exemple de DbContext

```csharp
// Data/KelasiNaBisoDbContext.cs
public class KelasiNaBisoDbContext : DbContext
{
    public DbSet<Ecole> Ecoles { get; set; }
    public DbSet<Utilisateur> Utilisateurs { get; set; }
    public DbSet<Eleve> Eleves { get; set; }
    public DbSet<Paiement> Paiements { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    
    public KelasiNaBisoDbContext(DbContextOptions<KelasiNaBisoDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuration des relations
        modelBuilder.Entity<Utilisateur>()
            .HasOne(u => u.Ecole)
            .WithMany(e => e.Utilisateurs)
            .HasForeignKey(u => u.IdEcole);
        
        // Autres configurations...
    }
}
```

---

### 4️⃣ **Repositories/** - Accès aux Données

Les repositories **encapsulent** l'accès aux données et fournissent une **abstraction**.

```
Repositories/
├── IRepository.cs               # Interface générique
├── Repository.cs                # Implémentation générique
├── IEcoleRepository.cs          # Interface pour Ecole
├── EcoleRepository.cs           # Implémentation pour Ecole
├── IUtilisateurRepository.cs
├── UtilisateurRepository.cs
└── ...
```

#### Exemple de Repository

```csharp
// Repositories/IEcoleRepository.cs
public interface IEcoleRepository
{
    Task<IEnumerable<Ecole>> GetAllAsync();
    Task<Ecole?> GetByIdAsync(int id);
    Task<Ecole> CreateAsync(Ecole ecole);
    Task<Ecole> UpdateAsync(Ecole ecole);
    Task<bool> DeleteAsync(int id);
}

// Repositories/EcoleRepository.cs
public class EcoleRepository : IEcoleRepository
{
    private readonly KelasiNaBisoDbContext _context;

    public EcoleRepository(KelasiNaBisoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Ecole>> GetAllAsync()
    {
        return await _context.Ecoles
            .Where(e => e.Statut)
            .ToListAsync();
    }

    public async Task<Ecole?> GetByIdAsync(int id)
    {
        return await _context.Ecoles
            .FirstOrDefaultAsync(e => e.IdEcole == id);
    }
    
    // Autres méthodes...
}
```

---

### 5️⃣ **Services/** - Logique Métier

Les services contiennent la **logique métier** et les **règles applicatives**.

```
Services/
├── IPermissionService.cs        # Interface permissions
├── PermissionService.cs         # Implémentation permissions
├── ICurrentUserService.cs       # Interface utilisateur actuel
├── CurrentUserService.cs        # Implémentation utilisateur actuel
├── ISimpleJwtService.cs         # Interface JWT
├── SimpleJwtService.cs          # Génération de tokens
├── ITwilioSmsService.cs         # Interface SMS
├── TwilioSmsService.cs          # Envoi de SMS
└── ...
```

#### Exemple de Service

```csharp
// Services/IPermissionService.cs
public interface IPermissionService
{
    Task<IEnumerable<string>> GetUserPermissionsAsync(int userId);
    Task<bool> HasPermissionAsync(int userId, string permissionName);
    Task<IEnumerable<Permission>> GetAllPermissionsAsync();
}

// Services/PermissionService.cs
public class PermissionService : IPermissionService
{
    private readonly KelasiNaBisoDbContext _context;

    public PermissionService(KelasiNaBisoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId)
    {
        var permissions = await _context.Utilisateurs
            .Where(u => u.IdUtilisateur == userId)
            .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .SelectMany(u => u.Role.RolePermissions
                .Where(rp => rp.Permission.Statut)
                .Select(rp => rp.Permission.Nom))
            .Distinct()
            .ToListAsync();

        return permissions;
    }
    
    // Autres méthodes...
}
```

---

### 6️⃣ **Attributes/** - Attributs Personnalisés

Contient les **attributs personnalisés** pour la sécurité et la validation.

```
Attributes/
├── PermissionAttribute.cs       # Attribut [Permission("Nom")]
└── ...
```

#### Exemple d'Attribut

```csharp
// Attributes/PermissionAttribute.cs
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class PermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string _permissionName;

    public PermissionAttribute(string permissionName)
    {
        _permissionName = permissionName;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var permissionService = context.HttpContext.RequestServices
            .GetRequiredService<IPermissionService>();
        
        var userIdClaim = context.HttpContext.User
            .FindFirst(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userId = int.Parse(userIdClaim.Value);
        var hasPermission = permissionService
            .HasPermissionAsync(userId, _permissionName).Result;

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}
```

---

## 🔧 Fichiers de Configuration

### **Program.cs** - Point d'Entrée

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Configuration des services
builder.Services.AddControllers();
builder.Services.AddDbContext<KelasiNaBisoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// 2. Injection de dépendances
builder.Services.AddScoped<IEcoleRepository, EcoleRepository>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ISimpleJwtService, SimpleJwtService>();

// 3. Configuration JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])
            )
        };
    });

// 4. Swagger
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5. Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### **appsettings.json** - Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=kelasinabiso;User=root;Password=;"
  },
  "Jwt": {
    "SecretKey": "votre_cle_secrete_super_longue_et_securisee",
    "Issuer": "KelasiNaBisoAPI",
    "Audience": "KelasiNaBisoClients",
    "ExpirationMinutes": 1440
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## 🔐 Système RBAC (Role-Based Access Control)

### Tables de Permissions

```sql
-- Table Role (existante)
Role: IdRole, Nom, NiveauHierarchique, Statut

-- Table Permission (nouvelle)
Permission: IdPermission, Nom, Categorie, Action, Description, Statut

-- Table RolePermission (nouvelle, table de liaison)
RolePermission: IdRolePermission, IdRole, IdPermission
```

### Workflow d'Autorisation

1. **Utilisateur se connecte** → JWT généré avec `UserId`, `RoleId`, `EcoleId`
2. **Utilisateur accède à un endpoint** → Attribut `[Permission("Ecole.Create")]`
3. **Vérification automatique** :
   - `PermissionAttribute` extrait le `UserId` du JWT
   - `PermissionService.HasPermissionAsync()` vérifie dans la DB
   - Si ✅ permission OK → endpoint accessible
   - Si ❌ permission manquante → `403 Forbidden`

---

## 📚 Documentation Disponible

| Fichier | Description |
|---------|-------------|
| `API_DOCUMENTATION_FRONTEND.md` | Documentation complète des endpoints |
| `GUIDE_FRONTEND_PERMISSIONS.md` | Guide d'intégration des permissions frontend |
| `QUICK_START_FRONTEND.md` | Guide de démarrage rapide pour devs frontend |
| `RBAC_GUIDE_UTILISATION.md` | Guide d'utilisation du système RBAC |
| `EXEMPLE_SECURISATION_ENDPOINTS.md` | Exemples pratiques de sécurisation |
| `IMPLEMENTATION_RBAC_GUIDE_FINAL.md` | Guide d'implémentation RBAC backend |
| `MATRICE_ENDPOINTS_SECURITE.md` | Matrice de sécurité des endpoints |
| `KelasiNaBiso_API.postman_collection.json` | Collection Postman |
| `KelasiNaBiso_Dev.postman_environment.json` | Environnement Postman |

---

## 🚀 Commandes Utiles

```bash
# Créer une migration
dotnet ef migrations add NomDeLaMigration

# Appliquer les migrations
dotnet ef database update

# Lancer l'application
dotnet run

# Build de production
dotnet publish -c Release

# Restaurer les packages
dotnet restore
```

---

## 📞 Support

**Questions sur la structure ?**
- 📧 Email : dev@kelasinabiso.com
- 📱 WhatsApp : +243 999 999 999

---

**Bon développement ! 🚀**

