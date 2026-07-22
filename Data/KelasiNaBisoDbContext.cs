using KelasiNaBiso.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml;
using System.Linq;
using System;
using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Data
{
    public class KelasiNaBisoDbContext : DbContext
    {
        public KelasiNaBisoDbContext(DbContextOptions<KelasiNaBisoDbContext> options)
            : base(options)
        {
        }

        // DbSets pour tous les modÃ¨les
        public DbSet<Eleve> Eleves { get; set; }
        public DbSet<Ecole> Ecoles { get; set; }
        public DbSet<Classe> Classes { get; set; }
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Tuteur> Tuteurs { get; set; }
        public DbSet<Inscription> Inscriptions { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Cours> Cours { get; set; }
        public DbSet<AffectationCours> AffectationsCours { get; set; }
        public DbSet<TitulaireClasse> TitulairesClasses { get; set; } // ✅ NOUVEAU : Gestion titulaires Maternelle/Primaire
        public DbSet<Message> Messages { get; set; }
        public DbSet<Presence> Presences { get; set; }
        public DbSet<Vacation> Vacations { get; set; }
        public DbSet<Agent> Agents { get; set; }
        // ❌ OBSOLÈTE: DbSet<Enseignant> supprimé - Remplacé par DbSet<Agent>
        public DbSet<AnneeScolaire> AnneeScolaires { get; set; }
        public DbSet<Frais> Frais { get; set; }
        public DbSet<Paiement> Paiements { get; set; }
        public DbSet<Role> Roles { get; set; }
        
        // ✅ MULTI-RÔLES - Table de liaison Utilisateur-Role (N-N)
        public DbSet<UserRole> UserRoles { get; set; }
        
        // ✅ AUDIT TRAIL - Table d'audit pour tracer toutes les modifications
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Permission> Permissions { get; set; } // ✅ NOUVEAU : Système RBAC avec permissions
        public DbSet<RolePermission> RolePermissions { get; set; } // ✅ NOUVEAU : Liaison Rôle-Permission (N-N)
        public DbSet<UserPermission> UserPermissions { get; set; } // ✅ NOUVEAU : Permissions personnalisées par utilisateur
        public DbSet<Section> Sections { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<Direction> Directions { get; set; }
        public DbSet<GroupeMessage> GroupeMessages { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<RessourcePedagogique> RessourcePedagogiques { get; set; }
        public DbSet<Evaluation> Evaluations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }
        public DbSet<SmsLog> SmsLogs { get; set; } // ✅ NOUVEAU : Historique des SMS Twilio
        public DbSet<Horaire> Horaires { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; } // ✅ REFRESH TOKEN : Tokens de rafraîchissement JWT
        public DbSet<CommunicationCampaign> CommunicationCampaigns { get; set; }
        public DbSet<CommunicationSegment> CommunicationSegments { get; set; }
        public DbSet<CampaignRecipient> CampaignRecipients { get; set; }
        public DbSet<CommunicationTemplate> CommunicationTemplates { get; set; }
        public DbSet<CommunicationHistory> CommunicationHistory { get; set; }
        public DbSet<ParentCommunicationPreference> ParentCommunicationPreferences { get; set; }
        public DbSet<V_Utilisateur> V_Utilisateurs { get; set; }
        public DbSet<V_Eleve> V_Eleves { get; set; }
        public DbSet<EleveParEcoleDTO> EleveParEcole { get; set; }
        public DbSet<VuePaiementsFraisParEcoleDTO> VuePaiementsFraisParEcole { get; set; }
        public DbSet<VuePointagePresenceParEcoleDTO> VuePointagePresenceParEcole { get; set; }
        public DbSet<VueRepertoireAgentsParParentDTO> VueRepertoireAgentsParParent { get; set; }
        // ❌ OBSOLÈTE: VueRepertoireEnseignantsParParentDTO supprimé - Remplacé par VueRepertoireAgentsParParentDTO
        public DbSet<DevoirADomicile> DevoirsADomicile { get; set; } // ✅ NOUVEAU : Devoirs à domicile
        public DbSet<PaiementCrashed> PaiementsCrashed { get; set; } // ✅ NOUVEAU : Paiements échoués lors du bulk insert

        // MOKO Afrika — paiements Mobile Money / carte
        public DbSet<EcoleInfoPaiementMobile> EcolesInfoPaiementMobile { get; set; }
        public DbSet<EcoleBeneficiaireMomo> EcolesBeneficiairesMomo { get; set; }
        public DbSet<EcoleWallet> EcolesWallets { get; set; }
        public DbSet<EcoleWalletMouvement> EcolesWalletMouvements { get; set; }
        public DbSet<TransactionMoko> TransactionsMoko { get; set; }
        public DbSet<FilePayoutMoko> FilePayoutsMoko { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration des relations et contraintes
            modelBuilder.Entity<Eleve>()
                .HasOne(e => e.Classe)
                .WithMany(c => c.Eleves)
                .HasForeignKey(e => e.IdClasse)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Eleve>()
                .HasOne(e => e.Tuteur)
                .WithMany(t => t.Eleves)
                .HasForeignKey(e => e.IdTuteur)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Classe>()
                .HasOne(c => c.Direction)
                .WithMany(e => e.Classes)
                .HasForeignKey(c => c.IdDirection)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Classe>()
                .HasOne(c => c.Section)
                .WithMany(s => s.Classes)
                .HasForeignKey(c => c.IdSection)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Classe>()
                .HasOne(c => c.Option)
                .WithMany(o => o.Classes)
                .HasForeignKey(c => c.IdOption)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Utilisateur>()
                .HasOne(u => u.Ecole)
                .WithMany(e => e.Utilisateurs)
                .HasForeignKey(u => u.IdEcole)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Rétrocompatibilité : Relation avec Role (optionnelle pour le système multi-rôles)
            modelBuilder.Entity<Utilisateur>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Utilisateurs)
                .HasForeignKey(u => u.IdRole)
                .IsRequired(false) // ✅ Permet IdRole NULL pour le système multi-rôles
                .OnDelete(DeleteBehavior.Cascade);
            
            // ✅ Configuration explicite pour IdRole (nullable)
            modelBuilder.Entity<Utilisateur>()
                .Property(u => u.IdRole)
                .IsRequired(false);

            // ═══════════════════════════════════════════════════════════════════
            // ✅ MULTI-RÔLES : Configuration de la relation N-N Utilisateur-Role
            // ═══════════════════════════════════════════════════════════════════

            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => new { ur.IdUtilisateur, ur.IdRole })
                .IsUnique()
                .HasDatabaseName("IX_UserRole_Utilisateur_Role_Unique");

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Utilisateur)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.IdUtilisateur)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.IdRole)
                .OnDelete(DeleteBehavior.Restrict);

            // Index pour performance
            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => ur.IdUtilisateur)
                .HasDatabaseName("IX_UserRole_IdUtilisateur");

            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => ur.IdRole)
                .HasDatabaseName("IX_UserRole_IdRole");

            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => new { ur.IdUtilisateur, ur.Statut })
                .HasDatabaseName("IX_UserRole_Utilisateur_Statut");

            // ✅ AUDIT TRAIL: Configuration des index pour performance optimale
            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => new { a.TableName, a.RecordId })
                .HasDatabaseName("IX_AuditLog_Table_Record");

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.UserId)
                .HasDatabaseName("IX_AuditLog_UserId");

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.DateAction)
                .HasDatabaseName("IX_AuditLog_DateAction");

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.IdEcole)
                .HasDatabaseName("IX_AuditLog_IdEcole");

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.Action)
                .HasDatabaseName("IX_AuditLog_Action");

            // ✅ UNICITÉ EMAIL: Index unique sur l'email
            modelBuilder.Entity<Utilisateur>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Utilisateurs_Email_Unique");

            // ✅ UNICITÉ MATRICULE ÉLÈVE: Index unique sur le matricule
            modelBuilder.Entity<Eleve>()
                .HasIndex(e => e.Matricule)
                .IsUnique()
                .HasDatabaseName("IX_Eleves_Matricule_Unique");

            // ✅ UNICITÉ SERIAL NUMBER ÉLÈVE: Index unique sur le SerialNumber
            modelBuilder.Entity<Eleve>()
                .HasIndex(e => e.SerialNumber)
                .IsUnique()
                .HasDatabaseName("IX_Eleves_SerialNumber_Unique");

            // ✅ UNICITÉ COMPOSITE ÉLÈVE: Index unique composite pour prévenir les doublons
            // Empêche la création de doublons basés sur : Nom + Postnom + Prenom + DateNaissance + IdTuteur + IdClasse
            // Note : MariaDB/MySQL ne supporte pas les index filtrés, le filtre par Statut = 1 est géré dans le code C#
            modelBuilder.Entity<Eleve>()
                .HasIndex(e => new { e.Nom, e.Postnom, e.Prenom, e.DateNaissance, e.IdTuteur, e.IdClasse })
                .IsUnique()
                .HasDatabaseName("IX_Eleves_Unique_Nom_Prenom_DateNaissance_Tuteur_Classe");

            // ✅ UNICITÉ MATRICULE AGENT: Index unique sur le matricule
            modelBuilder.Entity<Agent>()
                .HasIndex(a => a.Matricule)
                .IsUnique()
                .HasDatabaseName("IX_Agents_Matricule_Unique");

            // ✅ UNICITÉ EMAIL AGENT: Index unique sur l'email
            modelBuilder.Entity<Agent>()
                .HasIndex(a => a.EmailAgent)
                .IsUnique()
                .HasDatabaseName("IX_Agents_Email_Unique");

            // ✅ UNICITÉ SERIAL NUMBER AGENT: Index unique sur le SerialNumber
            modelBuilder.Entity<Agent>()
                .HasIndex(a => a.SerialNumber)
                .IsUnique()
                .HasDatabaseName("IX_Agents_SerialNumber_Unique");

            // ✅ UNICITÉ EMAIL TUTEUR: Index unique sur l'email
            modelBuilder.Entity<Tuteur>()
                .HasIndex(t => t.Email)
                .IsUnique()
                .HasDatabaseName("IX_Tuteurs_Email_Unique");

            modelBuilder.Entity<Tuteur>()
                .HasOne(t => t.Ecole)
                .WithMany(e => e.Tuteurs)
                .HasForeignKey(t => t.IdEcole)
                .OnDelete(DeleteBehavior.NoAction);

            // ✅ FIX: Configuration explicite pour gérer les valeurs NULL en base de données
            // S'assure que toutes les propriétés string nullable acceptent bien les valeurs NULL
            modelBuilder.Entity<Agent>(entity =>
            {
                // Configuration de la relation avec Ecole (optionnelle)
                entity.HasOne(e => e.Ecole)
                    .WithMany(ec => ec.Agents)
                    .HasForeignKey(e => e.IdEcole)
                    .IsRequired(false) // ✅ Permet IdEcole NULL
                    .OnDelete(DeleteBehavior.Restrict);

                // Configuration explicite de toutes les propriétés nullable
                // IsRequired(false) indique à EF Core que ces colonnes peuvent être NULL
                entity.Property(e => e.Matricule).IsRequired(false);
                entity.Property(e => e.Nom).IsRequired(false);
                entity.Property(e => e.Postnom).IsRequired(false);
                entity.Property(e => e.Prenom).IsRequired(false);
                entity.Property(e => e.Genre).IsRequired(false);
                entity.Property(e => e.TelephoneAgent).IsRequired(false);
                entity.Property(e => e.EmailAgent).IsRequired(false);
                entity.Property(e => e.Statut).IsRequired(false);
                entity.Property(e => e.EtatCivil).IsRequired(false);
                entity.Property(e => e.SerialNumber).IsRequired(false);
                entity.Property(e => e.Fonction).IsRequired(false);
                entity.Property(e => e.RoleAgent).IsRequired(false);
                entity.Property(e => e.PhotoUrl).IsRequired(false);
                entity.Property(e => e.IdEcole).IsRequired(false);
                // Propriétés héritées de Adresse
                entity.Property(e => e.Province).IsRequired(false);
                entity.Property(e => e.Ville).IsRequired(false);
                entity.Property(e => e.Commune).IsRequired(false);
                entity.Property(e => e.Quartier).IsRequired(false);
                entity.Property(e => e.Avenue).IsRequired(false);
                entity.Property(e => e.Numero).IsRequired(false);
            });

            modelBuilder.Entity<Section>()
                .HasOne(s => s.Ecole)
                .WithMany(e => e.Sections)
                .HasForeignKey(s => s.IdEcole)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Direction>()
                .HasOne(d => d.Ecole)
                .WithMany(e => e.Directions)
                .HasForeignKey(d => d.IdEcole)
                .OnDelete(DeleteBehavior.Cascade);

            // Index unique composite sur NomDirection et IdEcole
            modelBuilder.Entity<Direction>()
                .HasIndex(d => new { d.NomDirection, d.IdEcole })
                .IsUnique();

            modelBuilder.Entity<Option>()
                .HasOne(o => o.Section)
                .WithMany(s => s.Options)
                .HasForeignKey(o => o.IdSection)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AnneeScolaire>()
                .HasOne(a => a.Ecole)
                .WithMany(e => e.AnneeScolaires)
                .HasForeignKey(a => a.IdEcole)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cours>()
                .HasOne(c => c.Classe)
                .WithMany(cl => cl.Cours)
                .HasForeignKey(c => c.IdClasse)
                .OnDelete(DeleteBehavior.NoAction);

            // Configuration des relations pour AffectationCours
            modelBuilder.Entity<AffectationCours>()
                .HasOne(ac => ac.Agent)
                .WithMany(e => e.AffectationsCours)
                .HasForeignKey(ac => ac.IdAgent)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AffectationCours>()
                .HasOne(ac => ac.Cours)
                .WithMany(c => c.AffectationsCours)
                .HasForeignKey(ac => ac.IdCours)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AffectationCours>()
                .HasOne(ac => ac.AnneeScolaire)
                .WithMany()
                .HasForeignKey(ac => ac.IdAnneeScolaire)
                .OnDelete(DeleteBehavior.NoAction);

            // ✅ NOUVEAU : Configuration TitulaireClasse (Maternelle/Primaire)
            modelBuilder.Entity<TitulaireClasse>()
                .HasOne(tc => tc.Agent)
                .WithMany()
                .HasForeignKey(tc => tc.IdAgent)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TitulaireClasse>()
                .HasOne(tc => tc.Classe)
                .WithMany()
                .HasForeignKey(tc => tc.IdClasse)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TitulaireClasse>()
                .HasOne(tc => tc.AnneeScolaire)
                .WithMany()
                .HasForeignKey(tc => tc.IdAnneeScolaire)
                .OnDelete(DeleteBehavior.NoAction);

            // ✅ Index unique : Un seul titulaire actif par classe et par année scolaire
            modelBuilder.Entity<TitulaireClasse>()
                .HasIndex(tc => new { tc.IdClasse, tc.IdAnneeScolaire, tc.Statut })
                .HasFilter("[Statut] = 1")
                .IsUnique();

            modelBuilder.Entity<Note>()
                .HasOne(n => n.Eleve)
                .WithMany(e => e.Notes)
                .HasForeignKey(n => n.IdEleve)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Note>()
                .HasOne(n => n.Evaluation)
                .WithMany(e => e.Notes)
                .HasForeignKey(n => n.IdEvaluation)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Note>()
                .HasOne(n => n.Professeur)
                .WithMany()
                .HasForeignKey(n => n.IdProfesseur)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Note>()
                .HasOne(n => n.AnneeScolaire)
                .WithMany(a => a.Notes)
                .HasForeignKey(n => n.IdAnneeScolaire)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Inscription>()
                .HasOne(i => i.Eleve)
                .WithMany(e => e.Inscriptions)
                .HasForeignKey(i => i.IdEleve)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Inscription>()
                .HasOne(i => i.Classe)
                .WithMany(c => c.Inscriptions)
                .HasForeignKey(i => i.IdClasse)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Inscription>()
                .HasOne(i => i.Ecole)
                .WithMany(e => e.Inscriptions)
                .HasForeignKey(i => i.IdEcole)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Inscription>()
                .HasOne(i => i.AnneeScolaire)
                .WithMany(a => a.Inscriptions)
                .HasForeignKey(i => i.IdAnneeScolaire)
                .OnDelete(DeleteBehavior.NoAction);

            // ✅ POINTAGE FLEXIBLE: Configuration des relations Presence
            modelBuilder.Entity<Presence>()
                .HasOne(p => p.Eleve)
                .WithMany(e => e.Presences)
                .HasForeignKey(p => p.IdEleve)
                .IsRequired(false) // Nullable car peut être un agent
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Presence>()
                .HasOne(p => p.Agent)
                .WithMany(a => a.Presences)
                .HasForeignKey(p => p.IdAgent)
                .IsRequired(false) // Nullable car peut être un élève
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Presence>()
                .HasOne(p => p.Vacation)
                .WithMany(v => v.Presences)
                .HasForeignKey(p => p.IdVacation)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Vacation>()
                .HasOne(h => h.Ecole)
                .WithMany(e => e.Vacations)
                .HasForeignKey(h => h.IdEcole)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Frais>()
                .HasOne(f => f.Direction)
                .WithMany(d => d.Frais)
                .HasForeignKey(f => f.IdDirection)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Paiement>()
                .HasOne(p => p.Eleve)
                .WithMany(e => e.Paiements)
                .HasForeignKey(p => p.IdEleve)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Paiement>()
                .HasOne(p => p.Utilisateur)
                .WithMany(u => u.Paiements)
                .HasForeignKey(p => p.IdUtilisateur)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Paiement>()
                .HasOne(p => p.Frais)
                .WithMany(f => f.Paiements)
                .HasForeignKey(p => p.IdFrais)
                .OnDelete(DeleteBehavior.NoAction);

            // ✅ Configuration PaiementCrashed : Relations avec les entités
            modelBuilder.Entity<PaiementCrashed>()
                .HasOne(pc => pc.Ecole)
                .WithMany()
                .HasForeignKey(pc => pc.IdEcole)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PaiementCrashed>()
                .HasOne(pc => pc.Eleve)
                .WithMany()
                .HasForeignKey(pc => pc.IdEleve)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PaiementCrashed>()
                .HasOne(pc => pc.Frais)
                .WithMany()
                .HasForeignKey(pc => pc.IdFrais)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PaiementCrashed>()
                .HasOne(pc => pc.Utilisateur)
                .WithMany()
                .HasForeignKey(pc => pc.IdUtilisateur)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PaiementCrashed>()
                .HasOne(pc => pc.PaiementCree)
                .WithMany()
                .HasForeignKey(pc => pc.IdPaiementCree)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // ✅ Configuration explicite des propriétés nullable pour PaiementCrashed
            // Ces configurations sont critiques pour éviter les erreurs de cast DBNull -> DateTime
            modelBuilder.Entity<PaiementCrashed>()
                .Property(pc => pc.DateEchec)
                .IsRequired(false)
                .HasConversion(
                    v => v.HasValue ? v.Value : (DateTime?)null,
                    v => v.HasValue ? v : null);

            modelBuilder.Entity<PaiementCrashed>()
                .Property(pc => pc.DateCreation)
                .IsRequired(false)
                .HasConversion(
                    v => v.HasValue ? v.Value : (DateTime?)null,
                    v => v.HasValue ? v : null);

            // ═══════════════════════════════════════════════════════════════════
            // MOKO Afrika — configuration entités paiement mobile / wallet
            // ═══════════════════════════════════════════════════════════════════

            modelBuilder.Entity<EcoleInfoPaiementMobile>()
                .HasIndex(e => e.IdEcole)
                .IsUnique()
                .HasDatabaseName("IX_EcoleInfoPaiementMobile_IdEcole_Unique");

            modelBuilder.Entity<EcoleInfoPaiementMobile>()
                .HasOne(e => e.Ecole)
                .WithOne(ec => ec.InfoPaiementMobile)
                .HasForeignKey<EcoleInfoPaiementMobile>(e => e.IdEcole)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EcoleBeneficiaireMomo>()
                .HasOne(b => b.InfoPaiementMobile)
                .WithMany(i => i.Beneficiaires)
                .HasForeignKey(b => b.IdEcoleInfoPaiementMobile)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EcoleBeneficiaireMomo>()
                .HasOne(b => b.Ecole)
                .WithMany(e => e.BeneficiairesMomo)
                .HasForeignKey(b => b.IdEcole)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EcoleBeneficiaireMomo>()
                .HasIndex(b => new { b.IdEcole, b.Methode, b.Numero })
                .IsUnique()
                .HasDatabaseName("IX_EcoleBeneficiaireMomo_Ecole_Methode_Numero");

            modelBuilder.Entity<EcoleWallet>()
                .HasIndex(w => w.IdEcole)
                .IsUnique()
                .HasDatabaseName("IX_EcoleWallet_IdEcole_Unique");

            modelBuilder.Entity<EcoleWallet>()
                .HasOne(w => w.Ecole)
                .WithOne(e => e.Wallet)
                .HasForeignKey<EcoleWallet>(w => w.IdEcole)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EcoleWallet>()
                .HasOne(w => w.InfoPaiementMobile)
                .WithOne(i => i.Wallet)
                .HasForeignKey<EcoleWallet>(w => w.IdEcoleInfoPaiementMobile)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EcoleWalletMouvement>()
                .HasOne(m => m.Wallet)
                .WithMany(w => w.Mouvements)
                .HasForeignKey(m => m.IdEcoleWallet)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EcoleWalletMouvement>()
                .HasOne(m => m.TransactionMoko)
                .WithMany()
                .HasForeignKey(m => m.IdTransactionMoko)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<EcoleWalletMouvement>()
                .HasOne(m => m.Paiement)
                .WithMany()
                .HasForeignKey(m => m.IdPaiement)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<EcoleWalletMouvement>()
                .HasIndex(m => m.IdEcole)
                .HasDatabaseName("IX_EcoleWalletMouvement_IdEcole");

            modelBuilder.Entity<TransactionMoko>()
                .HasIndex(t => t.Reference)
                .IsUnique()
                .HasDatabaseName("IX_TransactionMoko_Reference_Unique");

            modelBuilder.Entity<TransactionMoko>()
                .HasOne(t => t.Paiement)
                .WithMany()
                .HasForeignKey(t => t.IdPaiement)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TransactionMoko>()
                .HasOne(t => t.Ecole)
                .WithMany()
                .HasForeignKey(t => t.IdEcole)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionMoko>()
                .HasIndex(t => t.IdPaiement)
                .HasDatabaseName("IX_TransactionMoko_IdPaiement");

            modelBuilder.Entity<TransactionMoko>()
                .HasIndex(t => t.ParentReference)
                .HasDatabaseName("IX_TransactionMoko_ParentReference");

            modelBuilder.Entity<FilePayoutMoko>()
                .HasOne(f => f.TransactionMokoPayIn)
                .WithMany(t => t.FilePayouts)
                .HasForeignKey(f => f.IdTransactionMokoPayIn)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FilePayoutMoko>()
                .HasOne(f => f.TransactionMokoPayOut)
                .WithMany()
                .HasForeignKey(f => f.IdTransactionMokoPayOut)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FilePayoutMoko>()
                .HasOne(f => f.Ecole)
                .WithMany()
                .HasForeignKey(f => f.IdEcole)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FilePayoutMoko>()
                .HasOne(f => f.Paiement)
                .WithMany()
                .HasForeignKey(f => f.IdPaiement)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FilePayoutMoko>()
                .HasIndex(f => f.PayInReference)
                .HasDatabaseName("IX_FilePayoutMoko_PayInReference");

            modelBuilder.Entity<FilePayoutMoko>()
                .HasIndex(f => new { f.Status, f.ScheduledAt })
                .HasDatabaseName("IX_FilePayoutMoko_Status_ScheduledAt");

            // ✅ NOUVEAU : Configuration DevoirADomicile
            modelBuilder.Entity<DevoirADomicile>()
                .HasOne(d => d.Ecole)
                .WithMany()
                .HasForeignKey(d => d.IdEcole)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DevoirADomicile>()
                .HasOne(d => d.Direction)
                .WithMany()
                .HasForeignKey(d => d.IdDirection)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DevoirADomicile>()
                .HasOne(d => d.Agent)
                .WithMany()
                .HasForeignKey(d => d.IdAgent)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DevoirADomicile>()
                .HasOne(d => d.Classe)
                .WithMany()
                .HasForeignKey(d => d.IdClasse)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DevoirADomicile>()
                .HasOne(d => d.Cours)
                .WithMany()
                .HasForeignKey(d => d.IdCours)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Index pour performance
            modelBuilder.Entity<DevoirADomicile>()
                .HasIndex(d => d.IdClasse)
                .HasDatabaseName("IX_DevoirADomicile_IdClasse");

            modelBuilder.Entity<DevoirADomicile>()
                .HasIndex(d => d.IdAgent)
                .HasDatabaseName("IX_DevoirADomicile_IdAgent");

            modelBuilder.Entity<DevoirADomicile>()
                .HasIndex(d => d.IdEcole)
                .HasDatabaseName("IX_DevoirADomicile_IdEcole");

            modelBuilder.Entity<DevoirADomicile>()
                .HasIndex(d => d.IdDirection)
                .HasDatabaseName("IX_DevoirADomicile_IdDirection");

            modelBuilder.Entity<DevoirADomicile>()
                .HasIndex(d => d.DatePublication)
                .HasDatabaseName("IX_DevoirADomicile_DatePublication");

            modelBuilder.Entity<DevoirADomicile>()
                .HasIndex(d => new { d.IdClasse, d.Statut })
                .HasDatabaseName("IX_DevoirADomicile_Classe_Statut");

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Expediteur)
                .WithMany(u => u.MessagesEnvoyes)
                .HasForeignKey(m => m.IdExpediteur)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Destinateur)
                .WithMany(u => u.MessagesRecus)
                .HasForeignKey(m => m.IdDestinateur)
                .OnDelete(DeleteBehavior.NoAction) ;

            modelBuilder.Entity<Message>()
                .HasOne(m => m.GroupeMessage)
                .WithMany(g => g.Messages)
                .HasForeignKey(m => m.IdGroupe)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(t => t.Token)
                .IsUnique();

            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(t => t.Utilisateur)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(t => t.IdUtilisateur)
                .OnDelete(DeleteBehavior.Cascade);

            // Communication Campaigns
            modelBuilder.Entity<CommunicationCampaign>()
                .HasOne(c => c.Ecole)
                .WithMany(e => e.CommunicationCampaigns)
                .HasForeignKey(c => c.IdEcole)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommunicationCampaign>()
                .HasOne(c => c.Auteur)
                .WithMany(u => u.CampaignsCreees)
                .HasForeignKey(c => c.IdAuteur)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommunicationCampaign>()
                .HasOne(c => c.Validateur)
                .WithMany(u => u.CampaignsValidees)
                .HasForeignKey(c => c.ValidatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CommunicationCampaign>()
                .HasIndex(c => new { c.IdEcole, c.Statut });

            modelBuilder.Entity<CommunicationSegment>()
                .HasOne(s => s.Campaign)
                .WithMany(c => c.Segments)
                .HasForeignKey(s => s.IdCampaign)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommunicationSegment>()
                .HasOne(s => s.Ecole)
                .WithMany(e => e.SegmentsCommunication)
                .HasForeignKey(s => s.IdEcole)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CommunicationSegment>()
                .HasOne(s => s.Createur)
                .WithMany(u => u.SegmentsCree)
                .HasForeignKey(s => s.CreatedBy)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommunicationSegment>()
                .HasIndex(s => s.IdEcole);

            modelBuilder.Entity<ParentCommunicationPreference>()
                .HasIndex(p => new { p.IdTuteur, p.Canal })
                .IsUnique();

            modelBuilder.Entity<ParentCommunicationPreference>()
                .HasOne(p => p.Tuteur)
                .WithMany(t => t.PreferencesCommunication)
                .HasForeignKey(p => p.IdTuteur)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CampaignRecipient>()
                .HasOne(cr => cr.Campaign)
                .WithMany(c => c.Destinataires)
                .HasForeignKey(cr => cr.IdCampaign)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CampaignRecipient>()
                .HasOne(cr => cr.Utilisateur)
                .WithMany(u => u.CampaignRecipients)
                .HasForeignKey(cr => cr.IdUtilisateur)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CampaignRecipient>()
                .HasOne(cr => cr.Tuteur)
                .WithMany(t => t.CampaignRecipients)
                .HasForeignKey(cr => cr.IdTuteur)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CampaignRecipient>()
                .HasOne(cr => cr.Eleve)
                .WithMany(e => e.CampaignRecipients)
                .HasForeignKey(cr => cr.IdEleve)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CampaignRecipient>()
                .HasOne(cr => cr.Notification)
                .WithMany(n => n.CampaignRecipients)
                .HasForeignKey(cr => cr.IdNotification)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CampaignRecipient>()
                .HasIndex(cr => cr.IdCampaign);

            modelBuilder.Entity<CampaignRecipient>()
                .HasIndex(cr => cr.IdUtilisateur);

            modelBuilder.Entity<CampaignRecipient>()
                .HasIndex(cr => cr.Status);

            modelBuilder.Entity<CommunicationTemplate>()
                .HasOne(ct => ct.Ecole)
                .WithMany(e => e.CommunicationTemplates)
                .HasForeignKey(ct => ct.IdEcole)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CommunicationTemplate>()
                .HasOne(ct => ct.Createur)
                .WithMany(u => u.TemplatesCree)
                .HasForeignKey(ct => ct.CreatedBy)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommunicationTemplate>()
                .HasOne(ct => ct.Modificateur)
                .WithMany(u => u.TemplatesMisAJour)
                .HasForeignKey(ct => ct.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CommunicationTemplate>()
                .HasIndex(ct => new { ct.IdEcole, ct.IsActif });

            modelBuilder.Entity<CommunicationHistory>()
                .HasOne(ch => ch.Campaign)
                .WithMany(c => c.Historique)
                .HasForeignKey(ch => ch.IdCampaign)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommunicationHistory>()
                .HasOne(ch => ch.Utilisateur)
                .WithMany(u => u.CampaignHistoryActions)
                .HasForeignKey(ch => ch.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CommunicationHistory>()
                .HasIndex(ch => new { ch.IdCampaign, ch.Action });

            modelBuilder.Entity<GroupeMessage>()
                .HasOne(g => g.Utilisateur)
                .WithMany(u => u.GroupeMessages)
                .HasForeignKey(g => g.CreePar)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GroupeMessage>()
                .HasOne(g => g.Ecole)
                .WithMany(e => e.GroupesMessages)
                .HasForeignKey(g => g.IdEcole)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.Eleve)
                .WithMany(e => e.Documents)
                .HasForeignKey(d => d.IdEleve)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.Utilisateur)
                .WithMany()
                .HasForeignKey(d => d.IdUtilisateur)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RessourcePedagogique>()
                .HasOne(r => r.Cours)
                .WithMany(c => c.RessourcesPedagogiques)
                .HasForeignKey(r => r.IdCours)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Evaluations)
                .HasForeignKey(e => e.IdCours)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Classe)
                .WithMany(c => c.Evaluations)
                .HasForeignKey(e => e.IdClasse)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Evaluation>()
                .HasMany(e => e.Notes)
                .WithOne(n => n.Evaluation)
                .HasForeignKey(n => n.IdEvaluation)
                .OnDelete(DeleteBehavior.NoAction);

            // Configuration des relations pour Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Expediteur)
                .WithMany(u => u.NotificationsEnvoyees)
                .HasForeignKey(n => n.IdExpediteur)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Destinataire)
                .WithMany(u => u.NotificationsRecues)
                .HasForeignKey(n => n.IdDestinataire)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Ecole)
                .WithMany(e => e.Notifications)
                .HasForeignKey(n => n.IdEcole)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Classe)
                .WithMany(c => c.Notifications)
                .HasForeignKey(n => n.IdClasse)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Eleve)
                .WithMany(e => e.Notifications)
                .HasForeignKey(n => n.IdEleve)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Cours)
                .WithMany(c => c.Notifications)
                .HasForeignKey(n => n.IdCours)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.AnneeScolaire)
                .WithMany(a => a.Notifications)
                .HasForeignKey(n => n.IdAnneeScolaire)
                .OnDelete(DeleteBehavior.NoAction);

            // Configuration de l'entitÃe Horaire
            modelBuilder.Entity<Horaire>()
                .HasOne(h => h.Classe)
                .WithMany(c => c.Horaires)
                .HasForeignKey(h => h.IdClasse)
                .OnDelete(DeleteBehavior.NoAction);


            // Configuration de la vue V_Utilisateur
            modelBuilder.Entity<V_Utilisateur>()
                .ToView("V_Utilisateur")
                .HasKey(v => v.IdUtilisateur);

            // Configuration de la vue V_Eleve
            modelBuilder.Entity<V_Eleve>()
                .ToView("V_Eleve")
                .HasKey(v => v.IdEleve);

            // Configuration de la vue EleveParEcole
            modelBuilder.Entity<EleveParEcoleDTO>()
                .ToView("EleveParEcole")
                .HasKey(e => e.IdEleve);

            // Configuration de la vue VuePaiementsFraisParEcole
            modelBuilder.Entity<VuePaiementsFraisParEcoleDTO>()
                .ToView("VuePaiementsFraisParEcole")
                .HasKey(p => p.IdPaiement);

            // Configuration de la vue VuePointagePresenceParEcole
            modelBuilder.Entity<VuePointagePresenceParEcoleDTO>()
                .ToView("VuePointagePresenceParEcole")
                .HasKey(p => p.IdPresence);

            modelBuilder.Entity<VueRepertoireAgentsParParentDTO>()
                .ToView("Vue_RepertoireAgentsParParent")
                .HasKey(r => new { r.IdAgent, r.IdCours, r.IdEleve });

            // ❌ OBSOLÈTE: VueRepertoireEnseignantsParParent supprimé - Remplacé par VueRepertoireAgentsParParent

            // Configuration de la contrainte unique sur le nom du rôle
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Nom)
                .IsUnique();


        }

        // ═══════════════════════════════════════════════════════════════════════════════════
        // 🛠️ MÉTHODE UTILITAIRE : VÉRIFICATION EXISTENCE VUE
        // ═══════════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Vérifie si une vue existe dans la base de données
        /// </summary>
        /// <param name="viewName">Nom de la vue à vérifier</param>
        /// <returns>True si la vue existe, False sinon</returns>
        private bool ViewExists(string viewName)
        {
            try
            {
                var sql = $@"
                    SELECT COUNT(*) 
                    FROM information_schema.views 
                    WHERE table_schema = DATABASE() 
                    AND table_name = '{viewName}'";

                var connection = Database.GetDbConnection();
                
                // Ouvrir la connexion si elle est fermée
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }
                
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                var result = command.ExecuteScalar();
                
                // NE PAS fermer la connexion ici, EF Core s'en charge
                
                return result != null && Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Erreur lors de la vérification de la vue {viewName}: {ex.Message}");
                return false; // En cas d'erreur, on considère que la vue n'existe pas (pour forcer la recréation)
            }
        }

        /// <summary>
        /// Calcule un hash MD5 d'une chaîne SQL pour détecter les changements
        /// </summary>
        /// <param name="sql">Requête SQL à hasher</param>
        /// <returns>Hash MD5 de la requête</returns>
        private string GetSqlHash(string sql)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = System.Text.Encoding.ASCII.GetBytes(sql.Trim());
                var hashBytes = md5.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes);
            }
        }

        /// <summary>
        /// Récupère le hash stocké pour une vue (via un commentaire dans la vue elle-même)
        /// </summary>
        /// <param name="viewName">Nom de la vue</param>
        /// <returns>Hash stocké ou null si non trouvé</returns>
        private string? GetStoredViewHash(string viewName)
        {
            try
            {
                var sql = $@"
                    SELECT VIEW_DEFINITION 
                    FROM information_schema.views 
                    WHERE table_schema = DATABASE() 
                    AND table_name = '{viewName}'";

                var connection = Database.GetDbConnection();
                
                // Ouvrir la connexion si elle est fermée
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }
                
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                var result = command.ExecuteScalar();
                
                // NE PAS fermer la connexion ici, EF Core s'en charge
                
                if (result == null)
                    return null;
                
                var viewDefinition = result.ToString();
                
                if (string.IsNullOrEmpty(viewDefinition))
                    return null;

                // Extraire le hash du commentaire (format: /* HASH:xxxxx */)
                var match = System.Text.RegularExpressions.Regex.Match(viewDefinition, @"/\* HASH:([A-F0-9]+) \*/");
                return match.Success ? match.Groups[1].Value : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Crée ou met à jour une vue uniquement si nécessaire (vue absente ou définition changée)
        /// </summary>
        /// <param name="viewName">Nom de la vue</param>
        /// <param name="createViewSql">SQL de création de la vue</param>
        /// <param name="forceRecreate">Forcer la recréation même si la vue existe</param>
        private void CreateOrUpdateViewIfNeeded(string viewName, string createViewSql, bool forceRecreate = false)
        {
            try
            {
                bool viewExists = ViewExists(viewName);
                string currentHash = GetSqlHash(createViewSql);

                if (!viewExists)
                {
                    Console.WriteLine($"🆕 Vue '{viewName}' n'existe pas → Création...");
                    
                    // Ajouter le hash dans un commentaire pour tracking
                    var sqlWithHash = $"/* HASH:{currentHash} */ {createViewSql}";
                    
                    Database.ExecuteSqlRaw(sqlWithHash);
                    Console.WriteLine($"✅ Vue '{viewName}' créée avec succès (Hash: {currentHash})");
                }
                else if (forceRecreate)
                {
                    Console.WriteLine($"🔄 Recréation forcée de la vue '{viewName}'...");
                    Database.ExecuteSqlRaw($"DROP VIEW IF EXISTS {viewName}");
                    
                    var sqlWithHash = $"/* HASH:{currentHash} */ {createViewSql}";
                    Database.ExecuteSqlRaw(sqlWithHash);
                    Console.WriteLine($"✅ Vue '{viewName}' recréée avec succès (Hash: {currentHash})");
                }
                else
                {
                    string? storedHash = GetStoredViewHash(viewName);
                    
                    if (storedHash == null || storedHash != currentHash)
                    {
                        Console.WriteLine($"🔄 Vue '{viewName}' a changé (Hash: {storedHash} → {currentHash}) → Mise à jour...");
                        Database.ExecuteSqlRaw($"DROP VIEW IF EXISTS {viewName}");
                        
                        var sqlWithHash = $"/* HASH:{currentHash} */ {createViewSql}";
                        Database.ExecuteSqlRaw(sqlWithHash);
                        Console.WriteLine($"✅ Vue '{viewName}' mise à jour avec succès");
                    }
                    else
                    {
                        Console.WriteLine($"✓ Vue '{viewName}' déjà à jour (Hash: {currentHash})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de la création/mise à jour de la vue '{viewName}': {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════════════
        // 📊 CRÉATION DES VUES SQL (OPTIMISÉ AVEC VÉRIFICATION)
        // ═══════════════════════════════════════════════════════════════════════════════════
        public void CreateViewUtilisateur()
        {
            var sql = @"CREATE VIEW V_Utilisateur AS
                                       SELECT 
                        u.IdUtilisateur,
                        u.ReferenceUtilisateur,
                        u.NomUtilisateur,
                        u.PostNomUtilisateur,
                        u.PrenomUtilisateur,
                        a.Matricule As Email,
                        u.Telephone,
                        u.PhotoUrl,
                        u.LieuNaissance,
                        u.DateNaissance,
                        u.Genre,
                        u.Statut,
                        u.DateCreation,
                        u.IsConnecte,
                        u.Province,
                        u.Ville,
                        u.Commune,
                        u.Quartier,
                        u.Avenue,
                        u.Numero,
                        r.IdRole,
                        a.Fonction AS NomRole,
                        r.DateCreation AS DateCreationRole,
                        e.IdEcole,
                        e.Nom AS NomEcole,
                        e.Slogan AS SloganEcole,
                        e.Type AS TypeEcole,
                        e.Logo AS LogoUrl,
                        e.Telephone AS TelephoneEcole,
                        e.EmailContact AS EmailContactEcole,
                        e.SiteWeb AS SiteWebEcole,
                        e.ProvinceEducationnel,
                        e.NomCompletResponsable,
                        e.Description AS DescriptionEcole,
                        e.DateCreation AS DateCreationEcole,
                        e.Province AS ProvinceEcole,
                        e.Ville AS VilleEcole,
                        e.Commune AS CommuneEcole,
                        e.Quartier AS QuartierEcole,
                        e.Avenue AS AvenueEcole,
                        e.Numero AS NumeroEcole
                    FROM Utilisateurs u   
                    LEFT JOIN agents  a ON u.Email =  a.EmailAgent
                    LEFT JOIN Roles   r ON u.IdRole  =  r.IdRole
                    LEFT JOIN Ecoles  e ON u.IdEcole =  e.IdEcole
                    where u.idRole <> 4 and a.SerialNumber is null  ";

            CreateOrUpdateViewIfNeeded("V_Utilisateur", sql);
        }
        public void CreateViewEleveParEcole()
        {
            var sql = @"CREATE VIEW EleveParEcole AS
                SELECT 
                    -- Eleve
                    e.IdEleve,
                    e.ReferenceEleve,
                    CONCAT(e.Prenom, ' ', e.Nom, ' ', e.Postnom) AS NomCompletEleve,
                    e.Genre,
                    e.DateNaissance,
                    YEAR(CURDATE()) - YEAR(e.DateNaissance) AS Age,
                    e.LieuNaissance,
                    e.PhotoUrl,
                    e.Nationalite,
                    e.Matricule,
                    e.Province AS ProvinceEleve,
                    e.Ville AS VilleEleve,
                    e.Commune AS CommuneEleve,
                    e.Quartier AS QuartierEleve,
                    e.Avenue AS AvenueEleve,
                    e.Numero AS NumeroEleve,
                    e.Commentaire,
                    e.Statut,

                    -- Classe
                    c.IdClasse,
                    c.NomClasse,

                    -- Direction
                    d.IdDirection,
                    d.NomDirection,

                    -- Option
                    o.IdOption,
                    o.NomOption,

                    -- Tuteur
                    t.IdTuteur,
                    t.NomComplet AS NomCompletTuteur,
                    t.Genre AS GenreTuteur,
                    t.Email AS EmailTuteur,
                    t.Telephone AS TelephoneTuteur,
                    t.NomCompletRepresentant,
                    t.TelephoneRepresentant,
                    t.Statut AS StatutTuteur,
                    t.PhotoTuteurUrl,
                    t.PieceIdentiteTuteur,

                    -- Ecole
                    ec.IdEcole,
                    ec.Nom AS NomEcole,
                    ec.Slogan,
                    ec.Longitute,
                    ec.Latitude,
                    ec.Type,
                    ec.Logo AS LogoUrl,
                    ec.Telephone AS TelephoneEcole,
                    ec.EmailContact,
                    ec.SiteWeb,
                    ec.ProvinceEducationnel,
                    ec.NomCompletResponsable,
                    ec.Description,
                    ec.Province AS ProvinceEcole,
                    ec.Ville AS VilleEcole,
                    ec.Commune AS CommuneEcole,
                    ec.Quartier AS QuartierEcole,
                    ec.Avenue AS AvenueEcole,
                    ec.Numero AS NumeroEcole

                FROM Eleves e
                LEFT JOIN Classes c ON e.IdClasse = c.IdClasse
                LEFT JOIN Directions d ON c.IdDirection = d.IdDirection
                LEFT JOIN Options o ON c.IdOption = o.IdOption
                LEFT JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
                LEFT JOIN Ecoles ec ON d.IdEcole = ec.IdEcole 
                where e.SerialNumber is null and e.STATUT is true ";

            CreateOrUpdateViewIfNeeded("EleveParEcole", sql);
        }
        public void CreateViewEleve()
        {
            var sql = @"CREATE VIEW V_Eleve AS
                SELECT 
                    e.IdEleve,
                    e.ReferenceEleve,
                    
                    -- Champs Ã‰lÃ¨ve
                    e.Matricule,
                    e.Nom,
                    e.Postnom,
                    e.Prenom,
                    e.NomComplet,
                    e.Genre,
                    e.DateNaissance,
                    e.LieuNaissance,
                    e.PhotoUrl,
                    e.Nationalite,
                    e.Commentaire,
                    e.Statut,
                    e.DateCreation,
                    
                    -- Champs Adresse Ã‰lÃ¨ve
                    e.Province,
                    e.Ville,
                    e.Commune,
                    e.Quartier,
                    e.Avenue,
                    e.Numero,
                    
                    -- Champs Classe
                    c.IdClasse,
                    c.NomClasse,
                    c.DateCreation AS DateCreationClasse,
                    
                    -- Champs Section
                    s.IdSection,
                    s.NomSection,
                    s.DateCreation AS DateCreationSection,
                    
                    -- Champs Option
                    o.IdOption,
                    o.NomOption,
                    o.DateCreation AS DateCreationOption,
                    
                    -- Champs Tuteur
                    t.IdTuteur,
                    t.NomComplet AS NomCompletTuteur,
                    t.Genre AS GenreTuteur,
                    t.Email AS EmailTuteur,
                    t.Telephone AS TelephoneTuteur,
                    t.NomCompletRepresentant,
                    t.TelephoneRepresentant,
                    t.PhotoTuteurUrl,
                    t.PieceIdentiteTuteur,
                    t.Statut AS StatutTuteur,
                    t.DateCreation AS DateCreationTuteur,
                    
                    -- Champs Ã‰cole (via Tuteur)
                    ec.IdEcole,
                    ec.Nom AS NomEcole,
                    ec.Slogan AS SloganEcole,
                    ec.Type AS TypeEcole,
                    ec.Logo AS LogoUrlEcole,
                    ec.Telephone AS TelephoneEcole,
                    ec.EmailContact AS EmailContactEcole,
                    ec.SiteWeb AS SiteWebEcole,
                    ec.ProvinceEducationnel AS ProvinceEducationnel,
                    ec.NomCompletResponsable AS NomCompletResponsable,
                    ec.Description AS DescriptionEcole,
                    ec.DateCreation AS DateCreationEcole,
                    
                    -- Champs Adresse Ã‰cole
                    ec.Province AS ProvinceEcole,
                    ec.Ville AS VilleEcole,
                    ec.Commune AS CommuneEcole,
                    ec.Quartier AS QuartierEcole,
                    ec.Avenue AS AvenueEcole,
                    ec.Numero AS NumeroEcole
                    
                FROM Eleves e
                LEFT JOIN Classes c ON e.IdClasse = c.IdClasse
                LEFT JOIN Sections s ON c.IdSection = s.IdSection
                LEFT JOIN Options o ON c.IdOption = o.IdOption
                LEFT JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
                LEFT JOIN Ecoles ec ON t.IdEcole = ec.IdEcole";

            CreateOrUpdateViewIfNeeded("V_Eleve", sql);
        }

        // DÉSACTIVÉ TEMPORAIREMENT - À RÉÉCRIRE POUR MARIADB/MYSQL
        // Cette procédure utilise la syntaxe T-SQL (SQL Server) incompatible avec MariaDB
        public void CreateInscriptionStoredProcedure()
        {
            // TODO: Réécrire cette procédure stockée en syntaxe MySQL/MariaDB
            /*
            var sql = @"
                -- =============================================
                -- ProcÃedure stockÃee pour crÃeer une inscription
                -- GÃ¨re les 3 cas : Nouveau ÃelÃ¨ve + Nouveau tuteur, Nouveau ÃelÃ¨ve + Ancien tuteur, Ancien ÃelÃ¨ve (RÃeinscription)
                -- =============================================
                CREATE PROCEDURE sp_CreateInscription
                    IN p_Type VARCHAR(50),                    -- 'Inscription' ou 'RÃeinscription'
                    IN p_IdEcole INT,
                    IN p_IdClasse INT,
                    IN p_IdAnneeScolaire INT,
                    IN p_DateInscription DATETIME,
                    IN p_StatutInscription VARCHAR(20),
                    
                    -- DonnÃees de l'ÃelÃ¨ve
                    IN p_NomEleve VARCHAR(100),
                    IN p_PostnomEleve VARCHAR(100),
                    IN p_PrenomEleve VARCHAR(100),
                    IN p_GenreEleve VARCHAR(10),
                    IN p_DateNaissanceEleve DATE,
                    IN p_LieuNaissanceEleve VARCHAR(100),
                    IN p_NationaliteEleve VARCHAR(50),
                    IN p_CommentaireEleve TEXT,
                    IN p_PhotoEleveUrl TEXT,
                    IN p_MatriculeEleve VARCHAR(100),
                    IN p_ProvinceEleve VARCHAR(100),
                    IN p_VilleEleve VARCHAR(100),
                    IN p_CommuneEleve VARCHAR(100),
                    IN p_QuartierEleve VARCHAR(100),
                    IN p_AvenueEleve VARCHAR(100),
                    IN p_NumeroEleve VARCHAR(100),
                    
                    -- DonnÃees du tuteur
                    IN p_NomCompletTuteur VARCHAR(150),
                    IN p_GenreTuteur VARCHAR(10),
                    IN p_EmailTuteur VARCHAR(100),
                    IN p_TelephoneTuteur VARCHAR(20),
                    IN p_NomCompletRepresentant VARCHAR(150),
                    IN p_TelephoneRepresentant VARCHAR(20),
                    IN p_PhotoTuteurUrl TEXT,
                    IN p_PieceIdentiteTuteur VARCHAR(100),
                    
                    -- Pour les cas de rÃeinscription
                    IN p_IdEleveExistant INT,
                    IN p_IdTuteurExistant INT,
                    
                    -- ParamÃ¨tres de sortie
                    OUT p_IdInscription INT,
                    OUT p_IdEleve INT,
                    OUT p_IdTuteur INT,
                    OUT p_Message VARCHAR(500),
                    OUT p_Success BOOLEAN
                BEGIN
                    
                    BEGIN TRY
                        BEGIN TRANSACTION;
                        
                        DECLARE @NewIdTuteur INT = NULL;
                        DECLARE @NewIdEleve INT = NULL;
                        DECLARE @NewIdInscription INT = NULL;
                        DECLARE @TuteurExists BIT = 0;
                        DECLARE @EleveExists BIT = 0;
                        
                        -- VÃerifier si c'est une RÃeinscription
                        IF @IdEleveExistant IS NOT NULL
                        BEGIN
                            -- Cas d'une RÃeinscription  
                            SET  @Type =  'RÃeinscription' ;
                            SET @EleveExists = 1;
                            SET @NewIdEleve = @IdEleveExistant;
                            
                            -- Mettre Ã  jour le statut de l'ÃelÃ¨ve
                            UPDATE Eleves 
                            SET Statut = 'True', IdClasse=@IdClasse 
                            WHERE IdEleve = @IdEleveExistant;
                            
                            -- Mettre Ã  jour le statut du tuteur associÃe
                            UPDATE Tuteurs 
                            SET Statut = 'True' 
                            WHERE IdTuteur = (SELECT IdTuteur FROM Eleves WHERE IdEleve = @IdEleveExistant);
                            
                            SET @Message = 'RÃeinscription effectuÃee avec succÃ¨s.';

                           
                        END
                        ELSE
                        BEGIN
                            -- Cas d'un nouvel ÃelÃ¨ve
                            SET  @Type =  'Inscription' ;

                            -- VÃerifier si le tuteur existe dÃejÃ 
                            IF @IdTuteurExistant IS NOT NULL
                            BEGIN
                                SET @TuteurExists = 1;
                                SET @NewIdTuteur = @IdTuteurExistant;
                                
                                -- Mettre Ã  jour le statut du tuteur existant
                                UPDATE Tuteurs 
                                SET Statut = 'True' 
                                WHERE IdTuteur = @IdTuteurExistant;
                                
                                SET @Message = 'Inscription effectuÃee avec succÃ¨s.';
                            END
                            ELSE
                            BEGIN
                                -- VÃerifier si un tuteur avec les mÃªmes coordonnÃees existe dÃejÃ 
                                SELECT @NewIdTuteur = IdTuteur, @TuteurExists = 1
                                FROM Tuteurs 
                                WHERE NomComplet = @NomCompletTuteur 
                                  AND Telephone = @TelephoneTuteur 
                                  AND IdEcole = @IdEcole;
                                
                                IF @TuteurExists = 1
                                BEGIN
                                    -- Mettre Ã  jour le statut du tuteur existant
                                    UPDATE Tuteurs 
                                    SET Statut = 'True' 
                                    WHERE IdTuteur = @NewIdTuteur;
                                    
                                    SET @Message = 'Inscription effectuÃee avec succÃ¨s.';
                                END
                                ELSE
                                BEGIN
                                    -- CrÃeer un nouveau tuteur
                                    INSERT INTO Tuteurs (
                                        NomComplet,              Genre,                        Email, 
                                        Telephone,               NomCompletRepresentant,       TelephoneRepresentant, 
                                        IdEcole,                 Statut,                       DateCreation, 
                                        PhotoTuteurUrl,          PieceIdentiteTuteur
                                    )
                                    VALUES (
                                        @NomCompletTuteur,       @GenreTuteur,                  @EmailTuteur, 
                                        @TelephoneTuteur,        @NomCompletRepresentant,       @TelephoneRepresentant,
                                        @IdEcole,                'True',                        GETDATE(),
                                        @PhotoTuteurUrl,          @PieceIdentiteTuteur
                                    );
                                    
                                    SET @NewIdTuteur = SCOPE_IDENTITY();
                                    SET @Message = 'Inscription effectuÃee avec succÃ¨s. Nouveau tuteur crÃeÃe.';
                                END
                            END
                            
                            -- CrÃeer le nouvel ÃelÃ¨ve
                            INSERT INTO Eleves (
                                ReferenceEleve,             Nom,               Postnom, 
                                Prenom,                     NomComplet,        Genre, 
                                DateNaissance,              LieuNaissance,     Nationalite,
                                Commentaire,                IdClasse,          IdTuteur, 
                                Statut,                     DateCreation,      PhotoUrl,          
                                Matricule,
                                -- Champs d'adresse hÃeritÃes
                                Province,         Ville,         Commune,         Quartier,        Avenue,       Numero
                            )
                            VALUES (
                                NEWID(),                    @NomEleve,                                                       @PostnomEleve, 
                                @PrenomEleve,               @NomEleve + ' ' + @PostnomEleve + ' ' + @PrenomEleve,            @GenreEleve, 
                                @DateNaissanceEleve,        @LieuNaissanceEleve,                                             @NationaliteEleve,
                                @CommentaireEleve,          @IdClasse,                                                       @NewIdTuteur, 
                                'True',                     GETDATE(),                                                       @PhotoEleveUrl,          
                                @MatriculeEleve,
                                -- Valeurs  des Champs d'adresse hÃeritÃes
                                @ProvinceEleve,   @VilleEleve,    @CommuneEleve,  @QuartierEleve,    @AvenueEleve,  @NumeroEleve
                            );
                            
                            SET @NewIdEleve = SCOPE_IDENTITY();
                        END
                        
                        -- CrÃeer l'inscription
                        INSERT INTO Inscriptions (
                            Type,                IdEleve,           IdEcole,              
                            IdClasse,            IdAnneeScolaire,   DateInscription, 
                            StatutInscription,   DateCreation
                        )
                        VALUES (
                            @Type,               @NewIdEleve,        @IdEcole, 
                            @IdClasse,           @IdAnneeScolaire,   @DateInscription, 
                            @StatutInscription,  GETDATE()
                        );
                        
                        SET @NewIdInscription = SCOPE_IDENTITY();
                        
                        -- Assigner les valeurs de sortie
                        SET @IdInscription = @NewIdInscription;
                        SET @IdEleve = @NewIdEleve;
                        SET @IdTuteur = @NewIdTuteur;
                        SET @Success = 1;
                        
                        COMMIT TRANSACTION;
                        
                    END TRY
                    BEGIN CATCH
                        IF @@TRANCOUNT > 0
                            ROLLBACK TRANSACTION;
                            
                        SET @Success = 0;
                        SET @Message = 'Erreur lors de l''inscription : ' + ERROR_MESSAGE();
                        SET @IdInscription = NULL;
                        SET @IdEleve = NULL;
                        SET @IdTuteur = NULL;
                    END CATCH
                END";

            Database.ExecuteSqlRaw(sql);
            */
            Console.WriteLine("⚠️  Procédure stockée sp_CreateInscription désactivée (migration vers MariaDB en cours)");
        }

        // DÉSACTIVÉ - REMPLACÉ PAR InitializeDefaultDataAsync()
        // Cette méthode utilise la syntaxe T-SQL (SQL Server) incompatible avec MariaDB
        public void InitializeDefaultData()
        {
            // Utilisez plutôt InitializeDefaultDataAsync() qui utilise Entity Framework
            Console.WriteLine("⚠️  InitializeDefaultData() désactivée. Utilisation de InitializeDefaultDataAsync() à la place.");
            /*
            var sql = @"
                -- =============================================
                -- Script d'initialisation des donnÃees par dÃefaut
                -- CrÃee le rÃ´le Super-Admin, l'Ãecole Ekelasi School et l'utilisateur Super-Admin
                -- =============================================
                BEGIN TRY
                    BEGIN TRANSACTION;
                    
                    DECLARE @SuperAdminRoleId INT = NULL;
                    DECLARE @EkelasiSchoolId INT = NULL;
                    DECLARE @SuperAdminUserId INT = NULL;
                    DECLARE @CurrentDate DATETIME2 = GETDATE();
                    
                    -- 1. VÃerifier et crÃeer le rÃ´le Super-Admin
                    SELECT @SuperAdminRoleId = IdRole 
                    FROM Roles 
                    WHERE Nom = 'Super-Admin';
                    
                    IF @SuperAdminRoleId IS NULL
                    BEGIN
                        INSERT INTO Roles (Nom, DateCreation)
                        VALUES ('Super-Admin', @CurrentDate);
                        SET @SuperAdminRoleId = SCOPE_IDENTITY();
                        PRINT 'RÃ´le Super-Admin crÃeÃe avec l''ID: ' + CAST(@SuperAdminRoleId AS NVARCHAR(10));
                    END
                    ELSE
                    BEGIN
                        PRINT 'RÃ´le Super-Admin existe dÃejÃ  avec l''ID: ' + CAST(@SuperAdminRoleId AS NVARCHAR(10));
                    END
                    
                    -- 2. VÃerifier et crÃeer l'Ãecole Ekelasi School
                    SELECT @EkelasiSchoolId = IdEcole 
                    FROM Ecoles 
                    WHERE Nom = 'Ekelasi School';
                    
                    IF @EkelasiSchoolId IS NULL
                    BEGIN
                        INSERT INTO Ecoles (
                            Nom, 
                            Slogan, 
                            Type, 
                            ProvinceEducationnel, 
                            NomCompletResponsable, 
                            Description, 
                            DateCreation
                        )
                        VALUES (
                            'Ekelasi School', 
                            'Excellence et Innovation', 
                            'PrivÃee', 
                            1000, 
                            50, 
                            'Ã‰cole d''excellence offrant une Ãeducation de qualitÃe', 
                            @CurrentDate
                        );
                        SET @EkelasiSchoolId = SCOPE_IDENTITY();
                        PRINT 'Ã‰cole Ekelasi School crÃeÃee avec l''ID: ' + CAST(@EkelasiSchoolId AS NVARCHAR(10));
                    END
                    ELSE
                    BEGIN
                        PRINT 'Ã‰cole Ekelasi School existe dÃejÃ  avec l''ID: ' + CAST(@EkelasiSchoolId AS NVARCHAR(10));
                    END
                    
                    -- 3. VÃerifier et crÃeer l'utilisateur Super-Admin
                    SELECT @SuperAdminUserId = IdUtilisateur 
                    FROM Utilisateurs 
                    WHERE IdRole = @SuperAdminRoleId AND IdEcole = @EkelasiSchoolId;
                    
                    IF @SuperAdminUserId IS NULL
                    BEGIN
                        INSERT INTO Utilisateurs (
                            ReferenceUtilisateur,
                            NomUtilisateur,
                            PostNomUtilisateur,
                            PrenomUtilisateur,
                            Email,
                            Telephone,
                            MotDePasseHash,
                            Statut,
                            IdRole,
                            IdEcole,
                            DateCreation,
                            IsConnecte
                        )
                        VALUES (
                            NEWID(),
                            'Super',
                            'Admin',
                            'Administrateur',
                            'superadmin@kelasinabiso.cd',
                            '+243999999999',
                            '$2a$11$hbjgXqTyFeirjjTLxA/UTOkBCtpQ04pJGUlpgv8K2Er8bo0OY5Dka', -- Mot de passe: 'Super-Admin'
                            1,
                            @SuperAdminRoleId,
                            @EkelasiSchoolId,
                            @CurrentDate,
                            0
                        );
                        SET @SuperAdminUserId = SCOPE_IDENTITY();
                        PRINT 'Utilisateur Super-Admin crÃeÃe avec l''ID: ' + CAST(@SuperAdminUserId AS NVARCHAR(10));
                        PRINT 'Email: superadmin@ekelasi.com';
                        PRINT 'Telephone: +243999999999';
                        PRINT 'Mot de passe: Super-Admin';
                    END
                    ELSE
                    BEGIN
                        PRINT 'Utilisateur Super-Admin existe dÃejÃ  avec l''ID: ' + CAST(@SuperAdminUserId AS NVARCHAR(10));
                    END
                    
                    COMMIT TRANSACTION;
                    PRINT 'Initialisation des donnÃees par dÃefaut terminÃee avec succÃ¨s!';
                    
                END TRY
                BEGIN CATCH
                    IF @@TRANCOUNT > 0
                        ROLLBACK TRANSACTION;
                        
                    PRINT 'Erreur lors de l''initialisation: ' + ERROR_MESSAGE();
                    THROW;
                END CATCH";

            Database.ExecuteSqlRaw(sql);
            */
        }

        // =============================================
        // NOUVELLE MÃ‰THODE SANS SCRIPT SQL
        // =============================================
        
        /// <summary>
        /// Initialise les donnÃees par dÃefaut du systÃ¨me (Super-Admin, Ekelasi School, etc.) en utilisant Entity Framework
        /// </summary>
        public async Task InitializeDefaultDataAsync()
        {
            using (var transaction = await Database.BeginTransactionAsync())
            {
                try
                {
                    var currentDate = DateTime.Now;
                    
                    // 1. CrÃeer ou rÃecupÃerer le rÃ´le Super-Admin
                    var superAdminRole = await CreateOrGetSuperAdminRoleAsync(currentDate);
                    
                    // 2. CrÃeer ou rÃecupÃerer l'Ãecole Ekelasi School
                    var ekelasiSchool = await CreateOrGetEkelasiSchoolAsync(currentDate);
                    
                    // 3. ✅ NOUVELLE LOGIQUE: Créer d'abord un Agent Manager Général, puis l'Utilisateur Super-Admin lié
                    var superAdminUser = await CreateOrGetSuperAdminWithAgentAsync(superAdminRole, ekelasiSchool, currentDate);
                    
                    await transaction.CommitAsync();
                    
                    // Logs de succÃ¨s
                    Console.WriteLine($"RÃ´le Super-Admin: ID {superAdminRole.IdRole}");
                    Console.WriteLine($"Ã‰cole Ekelasi School: ID {ekelasiSchool.IdEcole}");
                    Console.WriteLine($"Utilisateur Super-Admin: ID {superAdminUser.IdUtilisateur}");
                    Console.WriteLine("Email: superadmin@kelasinabiso.cd");
                    Console.WriteLine("Telephone: +243999999999");
                    Console.WriteLine("Mot de passe: Super-Admin");
                    Console.WriteLine("Initialisation des donnÃees par dÃefaut terminÃee avec succÃ¨s!");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Erreur lors de l'initialisation: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// CrÃee ou rÃecupÃ¨re le rÃ´le Super-Admin
        /// </summary>
        private async Task<Role> CreateOrGetSuperAdminRoleAsync(DateTime currentDate)
        {
            var existingRole = await Roles.FirstOrDefaultAsync(r => r.Nom == "Super-Admin");
            
            if (existingRole != null)
            {
                Console.WriteLine($"RÃ´le Super-Admin existe dÃejÃ  avec l'ID: {existingRole.IdRole}");
                return existingRole;
            }
            
            var newRole = new Role
            {
                Nom = "Super-Admin",
                DateCreation = currentDate
            };
            
            Roles.Add(newRole);
            await SaveChangesAsync();
            
            Console.WriteLine($"RÃ´le Super-Admin crÃeÃe avec l'ID: {newRole.IdRole}");
            return newRole;
        }

        /// <summary>
        /// CrÃee ou rÃecupÃ¨re l'Ãecole Ekelasi School
        /// </summary>
        private async Task<Ecole> CreateOrGetEkelasiSchoolAsync(DateTime currentDate)
        {
            var existingSchool = await Ecoles.FirstOrDefaultAsync(e => e.Nom == "Ekelasi School");
            
            if (existingSchool != null)
            {
                Console.WriteLine($"Ã‰cole Ekelasi School existe dÃejÃ  avec l'ID: {existingSchool.IdEcole}");
                return existingSchool;
            }
            
            var newSchool = new Ecole
            {
                Nom = "Ekelasi School",
                Slogan = "Excellence et Innovation",
                Type = "PrivÃee",
                ProvinceEducationnel = "1000",
                NomCompletResponsable = "50",
                Description = "Ã‰cole d'excellence offrant une Ãeducation de qualitÃe",
                DateCreation = currentDate
            };
            
            Ecoles.Add(newSchool);
            await SaveChangesAsync();
            
            Console.WriteLine($"Ã‰cole Ekelasi School crÃeÃee avec l'ID: {newSchool.IdEcole}");
            return newSchool;
        }

        /// <summary>
        /// ✅ NOUVELLE LOGIQUE : Crée ou récupère un Agent Manager Général + Utilisateur Super-Admin
        /// Respecte la logique métier : Utilisateur = Agent OU Tuteur (jamais orphelin)
        /// </summary>
        private async Task<Utilisateur> CreateOrGetSuperAdminWithAgentAsync(Role superAdminRole, Ecole ekelasiSchool, DateTime currentDate)
        {
            // Vérifier si l'utilisateur Super-Admin existe déjà
            var existingUser = await Utilisateurs
                .FirstOrDefaultAsync(u => u.IdRole == superAdminRole.IdRole && u.IdEcole == ekelasiSchool.IdEcole);
            
            if (existingUser != null)
            {
                Console.WriteLine($"Utilisateur Super-Admin existe dÃejÃ  avec l'ID: {existingUser.IdUtilisateur}");
                return existingUser;
            }
            
            // 1️⃣ CRÉER L'AGENT MANAGER GÉNÉRAL pour Ekelasi School
            var managerAgent = await CreateOrGetManagerGeneralAgentAsync(ekelasiSchool, currentDate);
            
            // 2️⃣ CRÉER L'UTILISATEUR SUPER-ADMIN LIÉ À CET AGENT
            var newUser = new Utilisateur
            {
                IdAgent = managerAgent.IdAgent, // ✨ LIEN ESSENTIEL AVEC L'AGENT
                ReferenceUtilisateur = Guid.NewGuid(),
                NomUtilisateur = managerAgent.Nom,
                PostNomUtilisateur = managerAgent.Postnom,
                PrenomUtilisateur = managerAgent.Prenom,
                Email = "superadmin@kelasinabiso.cd",
                DefaultUsername = "SuperAdmin",
                Telephone = "+243999999999",
                MotDePasseHash = "$2a$11$hbjgXqTyFeirjjTLxA/UTOkBCtpQ04pJGUlpgv8K2Er8bo0OY5Dka", // Mot de passe: 'Super-Admin'
                Genre = managerAgent.Genre,
                DateNaissance = managerAgent.DateNaissance,
                Statut = true,
                IdRole = superAdminRole.IdRole,
                IdEcole = ekelasiSchool.IdEcole,
                DateCreation = currentDate,
                IsConnecte = false,
                DoitChangerMotDePasse = false // Super-Admin n'est pas obligé de changer
            };
            
            Utilisateurs.Add(newUser);
            await SaveChangesAsync();
            
            Console.WriteLine($"Utilisateur Super-Admin crÃeÃe avec l'ID: {newUser.IdUtilisateur} (lié à l'Agent {managerAgent.IdAgent})");
            return newUser;
        }

        /// <summary>
        /// Crée ou récupère l'Agent Manager Général pour Ekelasi School
        /// </summary>
        private async Task<Agent> CreateOrGetManagerGeneralAgentAsync(Ecole ekelasiSchool, DateTime currentDate)
        {
            // Vérifier si un Agent Manager Général existe déjà pour cette école
            var existingManager = await Agents
                .FirstOrDefaultAsync(a => a.IdEcole == ekelasiSchool.IdEcole && a.Fonction == "Manager Général");
            
            if (existingManager != null)
            {
                Console.WriteLine($"Agent Manager Général existe déjà avec l'ID: {existingManager.IdAgent}");
                return existingManager;
            }
            
            // Créer un nouveau Agent Manager Général
            var managerAgent = new Agent
            {
                Nom = "Super",
                Postnom = "Admin",
                Prenom = "Administrateur",
                Genre = "Masculin",
                DateNaissance = DateTime.Now.AddYears(-40), // 40 ans par défaut
                TelephoneAgent = "+243999999999",
                EmailAgent = "superadmin@kelasinabiso.cd",
                Statut = true,
                EtatCivil = "Marié",
                Fonction = "Manager Général", // ✨ Manager Général de Ekelasi School
                RoleAgent = "Super-Administrateur",
                Matricule = await GenerateUniqueMatriculeAgentAsync(), // Généré unique
                IdEcole = ekelasiSchool.IdEcole,
                DateCreation = currentDate
            };
            
            Agents.Add(managerAgent);
            await SaveChangesAsync();
            
            Console.WriteLine($"Agent Manager Général créé avec l'ID: {managerAgent.IdAgent} - Matricule: {managerAgent.Matricule}");
            return managerAgent;
        }

        /// <summary>
        /// Génère un matricule unique pour un Agent
        /// Format: NAT[Année]-[GUID(6)]
        /// </summary>
        private async Task<string> GenerateUniqueMatriculeAgentAsync()
        {
            string matricule;
            
            do
            {
                string annee = DateTime.Now.Year.ToString().Substring(2);
                string guid = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                matricule = $"NAT{annee}-{guid}";
                
            } while (await Agents.AnyAsync(a => a.Matricule == matricule));
            
            return matricule;
        }
        
        public void CreateViewVuePaiementsFraisParEcole()
        {
            var sql = @"CREATE VIEW VuePaiementsFraisParEcole AS
                SELECT DISTINCT
                    -- Paiement
                    p.IdPaiement,
                    p.DatePaiement,
                    p.Montant,
                    p.Devise,
                    p.ModePaiement,
                    p.Statut AS StatutPaiement,
                 --   p.ReferenceTransaction,
                    p.JustificatifUrl,
                    p.Commentaire AS CommentairePaiement,
                    p.DateEnregistrement,
                    p.ReferencePaiemenet,
                    p.DateCreation AS DateCreationPaiement,

                    -- Ã‰lÃ¨ve
                    e.IdEleve,
                    e.ReferenceEleve,
                    e.Prenom,
                    e.Nom,
                    e.Postnom,
                    CONCAT(e.Prenom, ' ', e.Nom, ' ', e.Postnom) AS NomCompletFormatÃe,
                    e.NomComplet AS NomCompletOriginal,
                    e.Genre,
                    e.DateNaissance,
                    YEAR(CURDATE()) - YEAR(e.DateNaissance) AS Age,
                    e.LieuNaissance,
                    e.PhotoUrl,
                    e.Nationalite,
                    e.Matricule,
                    e.Province AS ProvinceEleve,
                    e.Ville AS VilleEleve,
                    e.Commune AS CommuneEleve,
                    e.Quartier AS QuartierEleve,
                    e.Avenue AS AvenueEleve,
                    e.Numero AS NumeroEleve,
                    e.Commentaire AS CommentaireEleve,
                    e.Statut AS StatutEleve,
                    e.DateCreation AS DateCreationEleve,

                    -- Classe
                    c.IdClasse,
                    c.NomClasse,
                    c.DateCreation AS DateCreationClasse,

                    -- Section
                    s.IdSection,
                    s.NomSection,
                    s.DateCreation AS DateCreationSection,

                    -- Direction
                    d.IdDirection,
                    d.NomDirection,
                    d.DateCreation AS DateCreationDirection,

                    -- Option
                    o.IdOption,
                    o.NomOption,
                    o.DateCreation AS DateCreationOption,

                    -- Ã‰cole
                    ec.IdEcole,
                    ec.Nom AS NomEcole,
                    ec.Slogan,
                    ec.Longitute,
                    ec.Latitude,
                    ec.Type AS TypeEcole,
                    ec.Logo AS LogoUrl,
                    ec.Telephone AS TelephoneEcole,
                    ec.EmailContact,
                    ec.SiteWeb,
                    ec.ProvinceEducationnel,
                    ec.NomCompletResponsable,
                    ec.Description AS DescriptionEcole,
                    ec.Province AS ProvinceEcole,
                    ec.Ville AS VilleEcole,
                    ec.Commune AS CommuneEcole,
                    ec.Quartier AS QuartierEcole,
                    ec.Avenue AS AvenueEcole,
                    ec.Numero AS NumeroEcole,
                    ec.DateCreation AS DateCreationEcole,

                    -- Frais
                    f.IdFrais,
                    f.LibelleFrais,
                    f.Montant AS MontantFrais,
                    f.Devise AS DeviseFrais,
                    f.DateCreation AS DateCreationFrais,

                    -- Tuteur
                    t.IdTuteur,
                    t.NomComplet AS NomTuteur,
                    t.Genre AS GenreTuteur,
                    t.Email AS EmailTuteur,
                    t.Telephone AS TelephoneTuteur,
                    t.NomCompletRepresentant,
                    t.TelephoneRepresentant,
                    t.Statut AS StatutTuteur,
                    t.PhotoTuteurUrl,
                    t.PieceIdentiteTuteur,
                    t.DateCreation AS DateCreationTuteur

                FROM Paiements p
                INNER JOIN Eleves e ON p.IdEleve = e.IdEleve
                INNER JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
                INNER JOIN Classes c ON e.IdClasse = c.IdClasse
                LEFT JOIN Sections s ON c.IdSection = s.IdSection
                INNER JOIN Directions d ON c.IdDirection = d.IdDirection
                LEFT JOIN Options o ON c.IdOption = o.IdOption
                INNER JOIN Frais f ON p.IdFrais = f.IdFrais
                INNER JOIN Ecoles ec ON d.IdEcole = ec.IdEcole";

            CreateOrUpdateViewIfNeeded("VuePaiementsFraisParEcole", sql);
        }

        public void CreateViewVuePointagePresenceParEcole()
        {
            var sql = @"CREATE VIEW VuePointagePresenceParEcole AS
                SELECT DISTINCT
                    -- PrÃesence
                    p.IdPresence,
                    p.DateDuJour,
                    p.HeureArrivee,
                    p.HeureDepart,
                    p.Statut AS StatutPresence,
                    p.Longitute,
                    p.Latitude,
                    p.DateCreation AS DateCreationPresence,

                    -- Ã‰lÃ¨ve
                    e.IdEleve,
                    e.ReferenceEleve,
                    e.Prenom,
                    e.Nom,
                    e.Postnom,
                    CONCAT(e.Prenom, ' ', e.Nom, ' ', e.Postnom) AS NomCompletFormatÃe,
                    e.Genre,
                    e.DateNaissance,
                    YEAR(CURDATE()) - YEAR(e.DateNaissance) AS Age,
                    e.LieuNaissance,
                    e.PhotoUrl,
                    e.Nationalite,
                    e.Matricule,
                    e.Province AS ProvinceEleve,
                    e.Ville AS VilleEleve,
                    e.Commune AS CommuneEleve,
                    e.Quartier AS QuartierEleve,
                    e.Avenue AS AvenueEleve,
                    e.Numero AS NumeroEleve,
                    e.Statut AS StatutEleve,
                    e.DateCreation AS DateCreationEleve,

                    -- Classe
                    c.IdClasse,
                    c.NomClasse,
                    c.DateCreation AS DateCreationClasse,

                    -- Option
                    o.IdOption,
                    o.NomOption,
                    o.DateCreation AS DateCreationOption,

                    -- Section
                    s.IdSection,
                    s.NomSection,
                    s.DateCreation AS DateCreationSection,

                    -- Direction
                    d.IdDirection,
                    d.NomDirection,
                    d.DateCreation AS DateCreationDirection,

                    -- Ã‰cole
                    ec.IdEcole,
                    ec.Nom AS NomEcole,
                    ec.Slogan,
                    ec.Longitute AS LongituteEcole,
                    ec.Latitude AS LatitudeEcole,
                    ec.Type AS TypeEcole,
                    ec.Logo AS LogoUrl,
                    ec.Telephone AS TelephoneEcole,
                    ec.EmailContact,
                    ec.SiteWeb,
                    ec.ProvinceEducationnel,
                    ec.NomCompletResponsable,
                    ec.Description AS DescriptionEcole,
                    ec.Province AS ProvinceEcole,
                    ec.Ville AS VilleEcole,
                    ec.Commune AS CommuneEcole,
                    ec.Quartier AS QuartierEcole,
                    ec.Avenue AS AvenueEcole,
                    ec.Numero AS NumeroEcole,
                    ec.DateCreation AS DateCreationEcole,

                    -- Horaire / Vacation
                    v.IdVacation,
                    v.NomVacation,
                    v.HeureDebut,
                    v.HeureFin,
                    v.HeureDebutPause,
                    v.HeureFinPause,
                    v.NombreJoursParSemaine,
                    v.DateCreation AS DateCreationVacation,

                    -- Tuteur
                    t.IdTuteur,
                    t.NomComplet AS NomTuteur,
                    t.Genre AS GenreTuteur,
                    t.Email AS EmailTuteur,
                    t.Telephone AS TelephoneTuteur,
                    t.NomCompletRepresentant,
                    t.TelephoneRepresentant,
                    t.Statut AS StatutTuteur,
                    t.PhotoTuteurUrl,
                    t.PieceIdentiteTuteur,
                    t.DateCreation AS DateCreationTuteur

                FROM Presences p
                INNER JOIN Eleves e ON p.IdEleve = e.IdEleve
                INNER JOIN Tuteurs t ON e.IdTuteur = t.IdTuteur
                INNER JOIN Classes c ON e.IdClasse = c.IdClasse
                LEFT JOIN Options o ON c.IdOption = o.IdOption
                LEFT JOIN Sections s ON c.IdSection = s.IdSection
                INNER JOIN Directions d ON c.IdDirection = d.IdDirection
                LEFT JOIN Vacations v ON p.IdVacation = v.IdVacation
                INNER JOIN Ecoles ec ON d.IdEcole = ec.IdEcole";

            CreateOrUpdateViewIfNeeded("VuePointagePresenceParEcole", sql);
        }

        public void CreateViewVueRepertoireAgentsParParent()
        {
            var sql = @"CREATE VIEW Vue_RepertoireAgentsParParent AS
                SELECT 
                    e.IdAgent,
                    CONCAT(e.Nom, ' ', e.Postnom, ' ', e.Prenom) AS NomCompletAgent,
                    e.Genre AS GenreAgent,
                    e.Numero AS TelephoneAgent,
                    e.EmailAgent, 
                    e.PhotoUrl AS PhotoAgent,
                    e.DateCreation AS DateCreationAgent,

                    c.IdCours,
                    c.NomCours,
                    c.Description AS DescriptionCours, 
                    c.DateCreation AS DateCreationCours,

                    cl.IdClasse,
                    cl.NomClasse,
                    cl.DateCreation AS DateCreationClasse,

                    an.IdAnneeScolaire,
                    an.LibelleAnneeScolaire,
                    an.DateDebut,
                    an.DateFin,
                    an.DateCreation AS DateCreationAnnee,

                    el.IdEleve,
                    el.NomComplet AS NomCompletEleve,
                    el.Genre AS GenreEleve,
                    el.Matricule,
                    el.Statut AS StatutEleve,
                    el.DateCreation AS DateCreationEleve,

                    tut.IdTuteur,
                    tut.NomComplet AS NomCompletTuteur,
                    tut.Genre AS GenreTuteur,
                    tut.Telephone AS TelephoneTuteur,
                    tut.Email AS EmailTuteur,
                    tut.NomCompletRepresentant,
                    tut.TelephoneRepresentant,
                    tut.Statut AS StatutTuteur,
                    tut.DateCreation AS DateCreationTuteur,

                    ec.IdEcole,
                    ec.Nom AS NomEcole,
                    ec.Type AS TypeEcole,
                    ec.DateCreation AS DateCreationEcole

                FROM Eleves el
                INNER JOIN Tuteurs tut ON el.IdTuteur = tut.IdTuteur
                INNER JOIN Classes cl ON el.IdClasse = cl.IdClasse
                INNER JOIN Cours c ON cl.IdClasse = c.IdClasse
                INNER JOIN AffectationsCours ac ON c.IdCours = ac.IdCours
                INNER JOIN Agents e ON ac.IdAgent = e.IdAgent
                INNER JOIN Directions d ON cl.IdDirection = d.IdDirection
                INNER JOIN Ecoles ec ON d.IdEcole = ec.IdEcole
                LEFT JOIN AnneeScolaires an ON an.IdEcole = ec.IdEcole
                WHERE el.Statut = 'True' AND tut.Statut = 'True' AND ac.Statut = 1";

            CreateOrUpdateViewIfNeeded("Vue_RepertoireAgentsParParent", sql);
        }

        // ❌ OBSOLÈTE: CreateViewVueRepertoireEnseignantsParParent() supprimé
        // ✅ NOUVEAU: Utilisez CreateViewVueRepertoireAgentsParParent() à la place

    }
}
 







