# 📚 GUIDE COMPLET : PARTAGE DE RESSOURCES PÉDAGOGIQUES

**Date** : 1 novembre 2025  
**Contexte** : KelasiNaBisoAPI  
**Objectif** : Permettre aux enseignants de partager travaux, cours, exercices avec élèves/parents

---

## 🎯 QU'EST-CE QUE LE PARTAGE DE RESSOURCES ?

Une **bibliothèque numérique** où les enseignants peuvent :
- 📝 Publier des cours et notes
- 📄 Partager des devoirs et exercices
- 🎥 Uploader des vidéos éducatives
- 📊 Distribuer des corrigés d'examens
- 📚 Créer une base de connaissances accessible 24/7

### Analogie : Google Classroom pour votre école

```
Enseignant (Prof de Maths) → Upload "Chapitre 5 - Équations.pdf"
                           → Notifie classe de 5ème A

Élève (Jean, 5ème A)       → Reçoit notification push
                           → Télécharge le PDF
                           → Peut poser questions en commentaires

Parent (Maman de Jean)     → Voit que Jean a un nouveau cours
                           → Peut aussi télécharger pour aider
```

**Avantage** : Tout est centralisé, accessible partout, tout le temps ! 📱💻

---

## 🏗️ ARCHITECTURE DU SYSTÈME

### Vue d'ensemble

```
┌─────────────────────────────────────────────────────────────┐
│                    ENSEIGNANT                                │
│  • Crée une ressource                                        │
│  • Upload fichier (PDF, Word, Video)                        │
│  • Sélectionne classe(s) cible(s)                           │
│  • Publie                                                    │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│                    BACKEND API                               │
│  1. Stockage fichier (Azure Blob / Local Storage)          │
│  2. Enregistrement DB (métadonnées)                        │
│  3. Notifications (SignalR + FCM + SMS)                     │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│              ÉLÈVES & PARENTS                                │
│  • Reçoivent notification instantanée                       │
│  • Consultent la ressource                                  │
│  • Téléchargent si nécessaire                               │
│  • Peuvent commenter/poser questions                        │
└─────────────────────────────────────────────────────────────┘
```

---

## 📊 MODÈLE DE BASE DE DONNÉES

### 1. Table `RessourcePedagogique`

```sql
CREATE TABLE RessourcePedagogique (
    IdRessource INT PRIMARY KEY IDENTITY(1,1),
    Titre NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    TypeRessource NVARCHAR(50) NOT NULL, -- Cours, Devoir, Exercice, Corrigé, Vidéo
    
    -- Fichier
    NomFichier NVARCHAR(255) NOT NULL,
    CheminFichier NVARCHAR(500) NOT NULL, -- URL ou path local
    TailleFichier BIGINT, -- en bytes
    TypeMIME NVARCHAR(100), -- application/pdf, video/mp4, etc.
    
    -- Métadonnées
    IdMatiere INT FOREIGN KEY REFERENCES Cours(IdCours),
    IdEnseignant INT FOREIGN KEY REFERENCES Agent(IdAgent),
    IdEcole INT FOREIGN KEY REFERENCES Ecole(IdEcole),
    
    -- Partage
    DatePublication DATETIME NOT NULL DEFAULT GETDATE(),
    DateExpiration DATETIME NULL, -- NULL = permanent
    EstVisible BIT NOT NULL DEFAULT 1,
    
    -- Stats
    NbTelechargements INT DEFAULT 0,
    NbVues INT DEFAULT 0,
    
    -- Audit
    DateCreation DATETIME NOT NULL DEFAULT GETDATE(),
    DateModification DATETIME NULL,
    
    INDEX IX_RessourcePedagogique_Ecole (IdEcole),
    INDEX IX_RessourcePedagogique_Enseignant (IdEnseignant),
    INDEX IX_RessourcePedagogique_Matiere (IdMatiere),
    INDEX IX_RessourcePedagogique_DatePublication (DatePublication DESC)
);
```

### 2. Table `RessourceClasse` (Partage avec classes)

```sql
CREATE TABLE RessourceClasse (
    IdRessourceClasse INT PRIMARY KEY IDENTITY(1,1),
    IdRessource INT NOT NULL FOREIGN KEY REFERENCES RessourcePedagogique(IdRessource) ON DELETE CASCADE,
    IdClasse INT NOT NULL FOREIGN KEY REFERENCES Classe(IdClasse),
    DatePartage DATETIME NOT NULL DEFAULT GETDATE(),
    
    UNIQUE(IdRessource, IdClasse),
    INDEX IX_RessourceClasse_Ressource (IdRessource),
    INDEX IX_RessourceClasse_Classe (IdClasse)
);
```

### 3. Table `RessourceConsultation` (Tracking)

```sql
CREATE TABLE RessourceConsultation (
    IdConsultation INT PRIMARY KEY IDENTITY(1,1),
    IdRessource INT NOT NULL FOREIGN KEY REFERENCES RessourcePedagogique(IdRessource),
    IdUtilisateur INT NOT NULL FOREIGN KEY REFERENCES Utilisateur(IdUtilisateur),
    TypeAction NVARCHAR(20) NOT NULL, -- Vue, Téléchargement
    DateAction DATETIME NOT NULL DEFAULT GETDATE(),
    
    INDEX IX_RessourceConsultation_Ressource (IdRessource),
    INDEX IX_RessourceConsultation_Utilisateur (IdUtilisateur)
);
```

### 4. Table `RessourceCommentaire` (Interactions)

```sql
CREATE TABLE RessourceCommentaire (
    IdCommentaire INT PRIMARY KEY IDENTITY(1,1),
    IdRessource INT NOT NULL FOREIGN KEY REFERENCES RessourcePedagogique(IdRessource) ON DELETE CASCADE,
    IdUtilisateur INT NOT NULL FOREIGN KEY REFERENCES Utilisateur(IdUtilisateur),
    Contenu NVARCHAR(MAX) NOT NULL,
    IdCommentaireParent INT NULL FOREIGN KEY REFERENCES RessourceCommentaire(IdCommentaire), -- Pour réponses
    DateCreation DATETIME NOT NULL DEFAULT GETDATE(),
    EstModifie BIT DEFAULT 0,
    
    INDEX IX_RessourceCommentaire_Ressource (IdRessource),
    INDEX IX_RessourceCommentaire_Parent (IdCommentaireParent)
);
```

---

## 🔧 IMPLÉMENTATION BACKEND

### 1. Modèle C# - `RessourcePedagogique.cs`

```csharp
namespace KelasiNaBisoAPI.Models
{
    public class RessourcePedagogique
    {
        public int IdRessource { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Titre { get; set; }
        
        public string? Description { get; set; }
        
        [Required]
        public TypeRessource TypeRessource { get; set; }
        
        // Fichier
        [Required]
        public string NomFichier { get; set; }
        
        [Required]
        public string CheminFichier { get; set; }
        
        public long? TailleFichier { get; set; }
        public string? TypeMIME { get; set; }
        
        // Relations
        public int? IdMatiere { get; set; }
        public virtual Cours? Matiere { get; set; }
        
        public int IdEnseignant { get; set; }
        public virtual Agent Enseignant { get; set; }
        
        public int IdEcole { get; set; }
        public virtual Ecole Ecole { get; set; }
        
        // Partage
        public DateTime DatePublication { get; set; } = DateTime.UtcNow;
        public DateTime? DateExpiration { get; set; }
        public bool EstVisible { get; set; } = true;
        
        // Stats
        public int NbTelechargements { get; set; } = 0;
        public int NbVues { get; set; } = 0;
        
        // Audit
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        public DateTime? DateModification { get; set; }
        
        // Navigation
        public virtual ICollection<RessourceClasse> Classes { get; set; } = new List<RessourceClasse>();
        public virtual ICollection<RessourceConsultation> Consultations { get; set; } = new List<RessourceConsultation>();
        public virtual ICollection<RessourceCommentaire> Commentaires { get; set; } = new List<RessourceCommentaire>();
    }
    
    public enum TypeRessource
    {
        Cours,
        Devoir,
        Exercice,
        Corrige,
        Video,
        Audio,
        Presentation,
        Document,
        Autre
    }
    
    public class RessourceClasse
    {
        public int IdRessourceClasse { get; set; }
        public int IdRessource { get; set; }
        public virtual RessourcePedagogique Ressource { get; set; }
        public int IdClasse { get; set; }
        public virtual Classe Classe { get; set; }
        public DateTime DatePartage { get; set; } = DateTime.UtcNow;
    }
    
    public class RessourceConsultation
    {
        public int IdConsultation { get; set; }
        public int IdRessource { get; set; }
        public virtual RessourcePedagogique Ressource { get; set; }
        public int IdUtilisateur { get; set; }
        public virtual Utilisateur Utilisateur { get; set; }
        public TypeActionRessource TypeAction { get; set; }
        public DateTime DateAction { get; set; } = DateTime.UtcNow;
    }
    
    public enum TypeActionRessource
    {
        Vue,
        Telechargement
    }
    
    public class RessourceCommentaire
    {
        public int IdCommentaire { get; set; }
        public int IdRessource { get; set; }
        public virtual RessourcePedagogique Ressource { get; set; }
        public int IdUtilisateur { get; set; }
        public virtual Utilisateur Utilisateur { get; set; }
        
        [Required]
        public string Contenu { get; set; }
        
        public int? IdCommentaireParent { get; set; }
        public virtual RessourceCommentaire? CommentaireParent { get; set; }
        public virtual ICollection<RessourceCommentaire> Reponses { get; set; } = new List<RessourceCommentaire>();
        
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        public bool EstModifie { get; set; } = false;
    }
}
```

### 2. Service - `RessourceService.cs`

```csharp
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBisoAPI.Services
{
    public interface IRessourceService
    {
        Task<RessourcePedagogique> CreateAsync(RessourcePedagogique ressource, IFormFile fichier, List<int> classesIds);
        Task<RessourcePedagogique?> GetByIdAsync(int id, int userId);
        Task<List<RessourcePedagogique>> GetByClasseAsync(int classeId, int userId);
        Task<List<RessourcePedagogique>> GetByEnseignantAsync(int enseignantId);
        Task<bool> DeleteAsync(int id, int enseignantId);
        Task<string> GetDownloadUrlAsync(int id, int userId);
        Task TrackConsultationAsync(int ressourceId, int userId, TypeActionRessource typeAction);
        Task<RessourceCommentaire> AddCommentaireAsync(int ressourceId, int userId, string contenu, int? parentId = null);
    }

    public class RessourceService : IRessourceService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IFileStorageService _fileStorage;
        private readonly ISignalRNotificationService _signalRNotification;
        private readonly IFirebaseNotificationService _fcmNotification;
        private readonly ILogger<RessourceService> _logger;

        public RessourceService(
            KelasiNaBisoDbContext context,
            IFileStorageService fileStorage,
            ISignalRNotificationService signalRNotification,
            IFirebaseNotificationService fcmNotification,
            ILogger<RessourceService> logger)
        {
            _context = context;
            _fileStorage = fileStorage;
            _signalRNotification = signalRNotification;
            _fcmNotification = fcmNotification;
            _logger = logger;
        }

        /// <summary>
        /// Créer une nouvelle ressource pédagogique
        /// </summary>
        public async Task<RessourcePedagogique> CreateAsync(
            RessourcePedagogique ressource, 
            IFormFile fichier, 
            List<int> classesIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // 1. Upload du fichier
                var uploadResult = await _fileStorage.UploadFileAsync(fichier, "ressources");
                
                ressource.NomFichier = fichier.FileName;
                ressource.CheminFichier = uploadResult.FilePath;
                ressource.TailleFichier = fichier.Length;
                ressource.TypeMIME = fichier.ContentType;
                ressource.DatePublication = DateTime.UtcNow;
                
                // 2. Sauvegarder la ressource
                _context.RessourcesPedagogiques.Add(ressource);
                await _context.SaveChangesAsync();
                
                // 3. Associer aux classes
                foreach (var classeId in classesIds)
                {
                    _context.RessourcesClasses.Add(new RessourceClasse
                    {
                        IdRessource = ressource.IdRessource,
                        IdClasse = classeId
                    });
                }
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
                
                // 4. Notifier les élèves et parents (en arrière-plan)
                _ = NotifyClassesAsync(ressource, classesIds);
                
                _logger.LogInformation($"✅ Ressource {ressource.IdRessource} créée par enseignant {ressource.IdEnseignant}");
                
                return ressource;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erreur lors de la création de la ressource");
                throw;
            }
        }

        /// <summary>
        /// Récupérer une ressource par ID (avec vérification d'accès)
        /// </summary>
        public async Task<RessourcePedagogique?> GetByIdAsync(int id, int userId)
        {
            var ressource = await _context.RessourcesPedagogiques
                .Include(r => r.Enseignant)
                .Include(r => r.Matiere)
                .Include(r => r.Classes)
                    .ThenInclude(rc => rc.Classe)
                .Include(r => r.Commentaires.OrderByDescending(c => c.DateCreation).Take(10))
                    .ThenInclude(c => c.Utilisateur)
                .FirstOrDefaultAsync(r => r.IdRessource == id && r.EstVisible);
            
            if (ressource == null)
                return null;
            
            // Vérifier que l'utilisateur a accès
            var hasAccess = await CheckUserAccessAsync(ressource, userId);
            if (!hasAccess)
                return null;
            
            // Incrémenter le compteur de vues
            ressource.NbVues++;
            await _context.SaveChangesAsync();
            
            // Tracker la consultation
            await TrackConsultationAsync(id, userId, TypeActionRessource.Vue);
            
            return ressource;
        }

        /// <summary>
        /// Récupérer toutes les ressources d'une classe
        /// </summary>
        public async Task<List<RessourcePedagogique>> GetByClasseAsync(int classeId, int userId)
        {
            // Vérifier que l'utilisateur appartient à cette classe
            var user = await _context.Utilisateurs
                .Include(u => u.Eleve)
                .Include(u => u.Tuteur)
                    .ThenInclude(t => t.Eleves)
                .FirstOrDefaultAsync(u => u.IdUtilisateur == userId);
            
            bool hasAccess = false;
            
            // Élève de cette classe
            if (user?.Eleve?.IdClasse == classeId)
                hasAccess = true;
            
            // Parent d'un élève de cette classe
            if (user?.Tuteur?.Eleves.Any(e => e.IdClasse == classeId) == true)
                hasAccess = true;
            
            // Enseignant de cette classe
            if (user?.Agent != null)
            {
                var enseigne = await _context.CoursClasses
                    .AnyAsync(cc => cc.IdClasse == classeId && cc.Cours.IdAgent == user.Agent.IdAgent);
                if (enseigne)
                    hasAccess = true;
            }
            
            if (!hasAccess)
                return new List<RessourcePedagogique>();
            
            var ressources = await _context.RessourcesPedagogiques
                .Include(r => r.Enseignant)
                .Include(r => r.Matiere)
                .Where(r => r.Classes.Any(rc => rc.IdClasse == classeId) 
                         && r.EstVisible
                         && (r.DateExpiration == null || r.DateExpiration > DateTime.UtcNow))
                .OrderByDescending(r => r.DatePublication)
                .ToListAsync();
            
            return ressources;
        }

        /// <summary>
        /// Récupérer toutes les ressources d'un enseignant
        /// </summary>
        public async Task<List<RessourcePedagogique>> GetByEnseignantAsync(int enseignantId)
        {
            return await _context.RessourcesPedagogiques
                .Include(r => r.Matiere)
                .Include(r => r.Classes)
                    .ThenInclude(rc => rc.Classe)
                .Where(r => r.IdEnseignant == enseignantId)
                .OrderByDescending(r => r.DatePublication)
                .ToListAsync();
        }

        /// <summary>
        /// Supprimer une ressource (soft delete)
        /// </summary>
        public async Task<bool> DeleteAsync(int id, int enseignantId)
        {
            var ressource = await _context.RessourcesPedagogiques
                .FirstOrDefaultAsync(r => r.IdRessource == id && r.IdEnseignant == enseignantId);
            
            if (ressource == null)
                return false;
            
            // Soft delete
            ressource.EstVisible = false;
            ressource.DateModification = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"🗑️ Ressource {id} supprimée par enseignant {enseignantId}");
            
            return true;
        }

        /// <summary>
        /// Obtenir l'URL de téléchargement
        /// </summary>
        public async Task<string> GetDownloadUrlAsync(int id, int userId)
        {
            var ressource = await _context.RessourcesPedagogiques
                .FirstOrDefaultAsync(r => r.IdRessource == id && r.EstVisible);
            
            if (ressource == null)
                throw new Exception("Ressource introuvable");
            
            // Vérifier l'accès
            var hasAccess = await CheckUserAccessAsync(ressource, userId);
            if (!hasAccess)
                throw new UnauthorizedAccessException("Accès refusé");
            
            // Incrémenter le compteur
            ressource.NbTelechargements++;
            await _context.SaveChangesAsync();
            
            // Tracker le téléchargement
            await TrackConsultationAsync(id, userId, TypeActionRessource.Telechargement);
            
            return ressource.CheminFichier;
        }

        /// <summary>
        /// Tracker une consultation/téléchargement
        /// </summary>
        public async Task TrackConsultationAsync(int ressourceId, int userId, TypeActionRessource typeAction)
        {
            _context.RessourcesConsultations.Add(new RessourceConsultation
            {
                IdRessource = ressourceId,
                IdUtilisateur = userId,
                TypeAction = typeAction,
                DateAction = DateTime.UtcNow
            });
            
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Ajouter un commentaire
        /// </summary>
        public async Task<RessourceCommentaire> AddCommentaireAsync(
            int ressourceId, 
            int userId, 
            string contenu, 
            int? parentId = null)
        {
            var commentaire = new RessourceCommentaire
            {
                IdRessource = ressourceId,
                IdUtilisateur = userId,
                Contenu = contenu,
                IdCommentaireParent = parentId,
                DateCreation = DateTime.UtcNow
            };
            
            _context.RessourcesCommentaires.Add(commentaire);
            await _context.SaveChangesAsync();
            
            // Notifier l'enseignant du nouveau commentaire
            var ressource = await _context.RessourcesPedagogiques
                .FirstOrDefaultAsync(r => r.IdRessource == ressourceId);
            
            if (ressource != null && ressource.IdEnseignant != userId)
            {
                await _signalRNotification.SendCustomNotificationAsync(
                    ressource.IdEnseignant,
                    "Nouveau commentaire",
                    $"Un nouveau commentaire sur votre ressource '{ressource.Titre}'",
                    "ressource_commentaire"
                );
            }
            
            return commentaire;
        }

        /// <summary>
        /// Vérifier si un utilisateur a accès à une ressource
        /// </summary>
        private async Task<bool> CheckUserAccessAsync(RessourcePedagogique ressource, int userId)
        {
            var user = await _context.Utilisateurs
                .Include(u => u.Eleve)
                .Include(u => u.Tuteur)
                    .ThenInclude(t => t.Eleves)
                .Include(u => u.Agent)
                .FirstOrDefaultAsync(u => u.IdUtilisateur == userId);
            
            if (user == null)
                return false;
            
            // L'enseignant créateur a toujours accès
            if (ressource.IdEnseignant == user.Agent?.IdAgent)
                return true;
            
            // Admin de l'école a accès
            if (user.IdEcole == ressource.IdEcole && (user.Role?.Nom == "Admin" || user.Role?.Nom == "Super-Admin"))
                return true;
            
            // Vérifier si l'élève est dans une des classes cibles
            if (user.Eleve != null)
            {
                var isInClass = await _context.RessourcesClasses
                    .AnyAsync(rc => rc.IdRessource == ressource.IdRessource 
                                 && rc.IdClasse == user.Eleve.IdClasse);
                if (isInClass)
                    return true;
            }
            
            // Vérifier si c'est un parent d'un élève dans une des classes
            if (user.Tuteur != null)
            {
                var hasChildInClass = await _context.RessourcesClasses
                    .AnyAsync(rc => rc.IdRessource == ressource.IdRessource 
                                 && user.Tuteur.Eleves.Any(e => e.IdClasse == rc.IdClasse));
                if (hasChildInClass)
                    return true;
            }
            
            return false;
        }

        /// <summary>
        /// Notifier toutes les classes concernées
        /// </summary>
        private async Task NotifyClassesAsync(RessourcePedagogique ressource, List<int> classesIds)
        {
            try
            {
                // Récupérer tous les élèves des classes concernées
                var eleves = await _context.Eleves
                    .Include(e => e.Utilisateur)
                    .Include(e => e.Tuteur)
                        .ThenInclude(t => t.Utilisateur)
                    .Where(e => classesIds.Contains(e.IdClasse ?? 0))
                    .ToListAsync();
                
                var enseignant = await _context.Agents
                    .FirstOrDefaultAsync(a => a.IdAgent == ressource.IdEnseignant);
                
                var titre = $"📚 Nouvelle ressource : {ressource.Titre}";
                var message = $"{enseignant?.Prenom} {enseignant?.Nom} a partagé une nouvelle ressource";
                
                // Notifier chaque élève
                foreach (var eleve in eleves)
                {
                    if (eleve.Utilisateur != null)
                    {
                        // SignalR
                        await _signalRNotification.SendCustomNotificationAsync(
                            eleve.Utilisateur.IdUtilisateur,
                            titre,
                            message,
                            "nouvelle_ressource"
                        );
                        
                        // FCM si token disponible
                        if (!string.IsNullOrEmpty(eleve.Utilisateur.FcmToken))
                        {
                            await _fcmNotification.SendNotificationAsync(
                                eleve.Utilisateur.FcmToken,
                                titre,
                                message,
                                new Dictionary<string, string>
                                {
                                    { "type", "nouvelle_ressource" },
                                    { "ressourceId", ressource.IdRessource.ToString() }
                                }
                            );
                        }
                    }
                    
                    // Notifier aussi le parent
                    if (eleve.Tuteur?.Utilisateur != null && !string.IsNullOrEmpty(eleve.Tuteur.Utilisateur.FcmToken))
                    {
                        var messageParent = $"{message} pour {eleve.Prenom}";
                        
                        await _fcmNotification.SendNotificationAsync(
                            eleve.Tuteur.Utilisateur.FcmToken,
                            titre,
                            messageParent,
                            new Dictionary<string, string>
                            {
                                { "type", "nouvelle_ressource" },
                                { "ressourceId", ressource.IdRessource.ToString() },
                                { "eleveId", eleve.IdEleve.ToString() }
                            }
                        );
                    }
                }
                
                _logger.LogInformation($"📢 Notifications envoyées pour ressource {ressource.IdRessource} à {eleves.Count} élèves");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi des notifications pour ressource");
            }
        }
    }
}
```

### 3. Service de Stockage - `FileStorageService.cs`

```csharp
namespace KelasiNaBisoAPI.Services
{
    public interface IFileStorageService
    {
        Task<FileUploadResult> UploadFileAsync(IFormFile file, string subfolder);
        Task<bool> DeleteFileAsync(string filePath);
        Task<Stream> GetFileStreamAsync(string filePath);
    }

    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LocalFileStorageService> _logger;
        private const long MaxFileSize = 50 * 1024 * 1024; // 50 MB

        public LocalFileStorageService(
            IWebHostEnvironment environment,
            ILogger<LocalFileStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<FileUploadResult> UploadFileAsync(IFormFile file, string subfolder)
        {
            // Validation
            if (file == null || file.Length == 0)
                throw new ArgumentException("Fichier invalide");
            
            if (file.Length > MaxFileSize)
                throw new ArgumentException($"Fichier trop volumineux (max {MaxFileSize / 1024 / 1024} MB)");
            
            // Créer le dossier si nécessaire
            var uploadFolder = Path.Combine(_environment.ContentRootPath, "uploads", subfolder);
            Directory.CreateDirectory(uploadFolder);
            
            // Générer un nom de fichier unique
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadFolder, uniqueFileName);
            
            // Sauvegarder le fichier
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            _logger.LogInformation($"📁 Fichier uploadé : {uniqueFileName}");
            
            return new FileUploadResult
            {
                FileName = uniqueFileName,
                FilePath = $"/uploads/{subfolder}/{uniqueFileName}",
                FileSize = file.Length
            };
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.ContentRootPath, filePath.TrimStart('/'));
                
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"🗑️ Fichier supprimé : {filePath}");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur suppression fichier : {filePath}");
                return false;
            }
        }

        public async Task<Stream> GetFileStreamAsync(string filePath)
        {
            var fullPath = Path.Combine(_environment.ContentRootPath, filePath.TrimStart('/'));
            
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Fichier introuvable");
            
            return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        }
    }

    public class FileUploadResult
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
    }
}
```

### 4. Controller - `RessourceController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KelasiNaBisoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RessourceController : ControllerBase
    {
        private readonly IRessourceService _ressourceService;
        private readonly ILogger<RessourceController> _logger;

        public RessourceController(
            IRessourceService ressourceService,
            ILogger<RessourceController> logger)
        {
            _ressourceService = ressourceService;
            _logger = logger;
        }

        /// <summary>
        /// Créer une nouvelle ressource (Enseignant uniquement)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Enseignant,Admin,Super-Admin")]
        [RequestSizeLimit(52428800)] // 50 MB
        public async Task<IActionResult> CreateRessource(
            [FromForm] CreateRessourceDto dto,
            [FromForm] IFormFile fichier)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                // Récupérer l'agent associé
                var user = await _context.Utilisateurs
                    .Include(u => u.Agent)
                    .FirstOrDefaultAsync(u => u.IdUtilisateur == userId);
                
                if (user?.Agent == null)
                    return BadRequest(new { message = "Vous devez être enseignant pour créer une ressource" });
                
                var ressource = new RessourcePedagogique
                {
                    Titre = dto.Titre,
                    Description = dto.Description,
                    TypeRessource = dto.TypeRessource,
                    IdMatiere = dto.IdMatiere,
                    IdEnseignant = user.Agent.IdAgent,
                    IdEcole = user.IdEcole ?? 0,
                    DateExpiration = dto.DateExpiration
                };
                
                var result = await _ressourceService.CreateAsync(ressource, fichier, dto.ClassesIds);
                
                return Ok(new
                {
                    message = "Ressource créée avec succès",
                    ressource = MapToDto(result)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur création ressource");
                return StatusCode(500, new { message = "Erreur lors de la création" });
            }
        }

        /// <summary>
        /// Récupérer toutes les ressources d'une classe
        /// </summary>
        [HttpGet("classe/{classeId}")]
        public async Task<IActionResult> GetByClasse(int classeId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var ressources = await _ressourceService.GetByClasseAsync(classeId, userId);
            
            return Ok(ressources.Select(MapToDto));
        }

        /// <summary>
        /// Récupérer une ressource par ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var ressource = await _ressourceService.GetByIdAsync(id, userId);
            
            if (ressource == null)
                return NotFound(new { message = "Ressource introuvable ou accès refusé" });
            
            return Ok(MapToDetailDto(ressource));
        }

        /// <summary>
        /// Télécharger une ressource
        /// </summary>
        [HttpGet("{id}/download")]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var filePath = await _ressourceService.GetDownloadUrlAsync(id, userId);
                
                var stream = await _fileStorage.GetFileStreamAsync(filePath);
                var fileName = Path.GetFileName(filePath);
                
                return File(stream, "application/octet-stream", fileName);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur téléchargement ressource {id}");
                return StatusCode(500, new { message = "Erreur lors du téléchargement" });
            }
        }

        /// <summary>
        /// Ajouter un commentaire
        /// </summary>
        [HttpPost("{id}/commentaires")]
        public async Task<IActionResult> AddCommentaire(
            int id,
            [FromBody] AddCommentaireDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var commentaire = await _ressourceService.AddCommentaireAsync(
                    id, userId, dto.Contenu, dto.ParentId);
                
                return Ok(new { message = "Commentaire ajouté", commentaire });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur ajout commentaire");
                return StatusCode(500, new { message = "Erreur lors de l'ajout du commentaire" });
            }
        }

        /// <summary>
        /// Supprimer une ressource
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Enseignant,Admin,Super-Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var user = await _context.Utilisateurs
                .Include(u => u.Agent)
                .FirstOrDefaultAsync(u => u.IdUtilisateur == userId);
            
            if (user?.Agent == null)
                return BadRequest(new { message = "Agent introuvable" });
            
            var success = await _ressourceService.DeleteAsync(id, user.Agent.IdAgent);
            
            if (!success)
                return NotFound(new { message = "Ressource introuvable ou accès refusé" });
            
            return Ok(new { message = "Ressource supprimée" });
        }

        private object MapToDto(RessourcePedagogique r) => new
        {
            idRessource = r.IdRessource,
            titre = r.Titre,
            description = r.Description,
            typeRessource = r.TypeRessource.ToString(),
            nomFichier = r.NomFichier,
            tailleFichier = r.TailleFichier,
            typeMIME = r.TypeMIME,
            datePublication = r.DatePublication,
            dateExpiration = r.DateExpiration,
            nbTelechargements = r.NbTelechargements,
            nbVues = r.NbVues,
            enseignant = r.Enseignant != null ? new
            {
                nom = $"{r.Enseignant.Prenom} {r.Enseignant.Nom}",
                photo = r.Enseignant.Photo
            } : null,
            matiere = r.Matiere?.NomCours
        };

        private object MapToDetailDto(RessourcePedagogique r) => new
        {
            idRessource = r.IdRessource,
            titre = r.Titre,
            description = r.Description,
            typeRessource = r.TypeRessource.ToString(),
            nomFichier = r.NomFichier,
            cheminFichier = r.CheminFichier,
            tailleFichier = r.TailleFichier,
            typeMIME = r.TypeMIME,
            datePublication = r.DatePublication,
            dateExpiration = r.DateExpiration,
            nbTelechargements = r.NbTelechargements,
            nbVues = r.NbVues,
            enseignant = r.Enseignant != null ? new
            {
                idAgent = r.Enseignant.IdAgent,
                nom = $"{r.Enseignant.Prenom} {r.Enseignant.Nom}",
                photo = r.Enseignant.Photo
            } : null,
            matiere = r.Matiere != null ? new
            {
                idCours = r.Matiere.IdCours,
                nom = r.Matiere.NomCours
            } : null,
            classes = r.Classes.Select(rc => new
            {
                idClasse = rc.IdClasse,
                nom = rc.Classe.NomClasse
            }),
            commentaires = r.Commentaires.Select(c => new
            {
                idCommentaire = c.IdCommentaire,
                contenu = c.Contenu,
                dateCreation = c.DateCreation,
                utilisateur = new
                {
                    nom = c.Utilisateur.Nom
                },
                reponses = c.Reponses.Select(rep => new
                {
                    idCommentaire = rep.IdCommentaire,
                    contenu = rep.Contenu,
                    dateCreation = rep.DateCreation,
                    utilisateur = new { nom = rep.Utilisateur.Nom }
                })
            })
        };
    }

    public class CreateRessourceDto
    {
        [Required]
        public string Titre { get; set; }
        public string? Description { get; set; }
        
        [Required]
        public TypeRessource TypeRessource { get; set; }
        
        public int? IdMatiere { get; set; }
        
        [Required]
        public List<int> ClassesIds { get; set; }
        
        public DateTime? DateExpiration { get; set; }
    }

    public class AddCommentaireDto
    {
        [Required]
        public string Contenu { get; set; }
        public int? ParentId { get; set; }
    }
}
```

---

## 🎨 INTERFACE FRONTEND

### Vue.js - Composant Liste de Ressources

```vue
<!-- components/RessourcesList.vue -->
<template>
  <div class="ressources-container">
    <div class="header">
      <h2>📚 Ressources pédagogiques</h2>
      <button v-if="isEnseignant" @click="showUploadModal = true" class="btn-primary">
        ➕ Nouvelle ressource
      </button>
    </div>
    
    <div v-if="loading" class="loading">Chargement...</div>
    
    <div v-else class="ressources-grid">
      <div
        v-for="ressource in ressources"
        :key="ressource.idRessource"
        class="ressource-card"
        @click="viewRessource(ressource.idRessource)"
      >
        <div class="ressource-icon">
          {{ getIcon(ressource.typeRessource) }}
        </div>
        
        <div class="ressource-info">
          <h3>{{ ressource.titre }}</h3>
          <p class="description">{{ ressource.description }}</p>
          
          <div class="meta">
            <span class="enseignant">
              👤 {{ ressource.enseignant?.nom }}
            </span>
            <span class="date">
              📅 {{ formatDate(ressource.datePublication) }}
            </span>
          </div>
          
          <div class="stats">
            <span>👁️ {{ ressource.nbVues }} vues</span>
            <span>⬇️ {{ ressource.nbTelechargements }} téléchargements</span>
          </div>
        </div>
        
        <button
          @click.stop="downloadRessource(ressource.idRessource, ressource.nomFichier)"
          class="btn-download"
        >
          ⬇️ Télécharger
        </button>
      </div>
    </div>
    
    <!-- Modal Upload -->
    <UploadRessourceModal
      v-if="showUploadModal"
      @close="showUploadModal = false"
      @uploaded="refreshRessources"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { apiService } from '@/services/api.service';

const authStore = useAuthStore();
const ressources = ref([]);
const loading = ref(true);
const showUploadModal = ref(false);

const isEnseignant = computed(() => 
  authStore.userRole === 'Enseignant' || authStore.userRole === 'Admin'
);

const props = defineProps({
  classeId: Number
});

onMounted(async () => {
  await loadRessources();
  
  // Écouter les nouvelles ressources via SignalR
  signalRService.on('NouvelleRessource', (ressource) => {
    ressources.value.unshift(ressource);
    showNotification(`📚 Nouvelle ressource : ${ressource.titre}`);
  });
});

async function loadRessources() {
  loading.value = true;
  try {
    const response = await apiService.get(`/api/Ressource/classe/${props.classeId}`);
    ressources.value = response.data;
  } catch (error) {
    console.error('Erreur chargement ressources:', error);
  } finally {
    loading.value = false;
  }
}

async function downloadRessource(id, fileName) {
  try {
    const response = await apiService.get(`/api/Ressource/${id}/download`, {
      responseType: 'blob'
    });
    
    // Créer un lien de téléchargement
    const url = window.URL.createObjectURL(new Blob([response.data]));
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', fileName);
    document.body.appendChild(link);
    link.click();
    link.remove();
    
    showNotification('✅ Téléchargement réussi');
  } catch (error) {
    showNotification('❌ Erreur lors du téléchargement', 'error');
  }
}

function getIcon(type) {
  const icons = {
    Cours: '📖',
    Devoir: '📝',
    Exercice: '✏️',
    Corrige: '✅',
    Video: '🎥',
    Audio: '🎵',
    Presentation: '🎞️',
    Document: '📄'
  };
  return icons[type] || '📄';
}

function formatDate(date) {
  return new Date(date).toLocaleDateString('fr-FR');
}

function refreshRessources() {
  showUploadModal.value = false;
  loadRessources();
}

function viewRessource(id) {
  router.push(`/ressource/${id}`);
}
</script>

<style scoped>
.ressources-container {
  padding: 20px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.ressources-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 20px;
}

.ressource-card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
}

.ressource-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
}

.ressource-icon {
  font-size: 48px;
  margin-bottom: 15px;
}

.ressource-info h3 {
  margin: 0 0 10px 0;
  color: #333;
}

.description {
  color: #666;
  font-size: 14px;
  margin-bottom: 15px;
  line-height: 1.5;
}

.meta {
  display: flex;
  gap: 15px;
  margin-bottom: 10px;
  font-size: 13px;
  color: #888;
}

.stats {
  display: flex;
  gap: 15px;
  font-size: 12px;
  color: #999;
  margin-bottom: 15px;
}

.btn-download {
  width: 100%;
  padding: 10px;
  background: #007bff;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
  transition: background 0.2s;
}

.btn-download:hover {
  background: #0056b3;
}
</style>
```

---

## 📊 AVANTAGES DU SYSTÈME

| Avant (Physique) | Après (Numérique) |
|------------------|-------------------|
| **Papier → Perte facile** | **Cloud → Toujours accessible** |
| **1 copie seulement** | **Copies illimitées** |
| **Coût d'impression élevé** | **Économies** (papier + encre) |
| **Oubli à la maison** | **Accessible depuis mobile/PC** |
| **Pas de notifications** | **Notif push instantanée** |
| **Pas de stats** | **Tracking complet** (vues, DL) |

---

## ✅ RÉSUMÉ

### Fonctionnalités Principales :

1. **Upload de fichiers** (PDF, Word, Video, etc.)
2. **Partage ciblé** (par classe/matière)
3. **Notifications push** (SignalR + FCM)
4. **Téléchargement sécurisé**
5. **Système de commentaires** (Q&A)
6. **Statistiques** (vues, téléchargements)
7. **Contrôle d'accès** (qui peut voir quoi)
8. **Expiration automatique** (ressources temporaires)

### Implémentation complète fournie ! 🎉

Voulez-vous que je vous aide à implémenter ce système ? 😊

