using KelasiNaBiso.Models;

namespace KelasiNaBiso.Tests.Unit.Helpers
{
    /// <summary>
    /// Builder pour créer des données de test
    /// </summary>
    public static class TestDataBuilder
    {
        public static Role CreateRole(int id, string nom, int? niveau = null, bool statut = true)
        {
            return new Role
            {
                IdRole = id,
                Nom = nom,
                Description = $"Description pour {nom}",
                Niveau = niveau ?? id,
                Statut = statut
            };
        }

        public static Utilisateur CreateUtilisateur(
            int id,
            string email,
            string nom = "Test",
            int? idEcole = null,
            bool statut = true)
        {
            return new Utilisateur
            {
                IdUtilisateur = id,
                Email = email,
                NomUtilisateur = nom,
                PostNomUtilisateur = "User",
                PrenomUtilisateur = "Test",
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                Statut = statut,
                IdEcole = idEcole,
                DateCreation = DateTime.Now
            };
        }

        public static UserRole CreateUserRole(
            int idUtilisateur,
            int idRole,
            bool isPrimary = false,
            bool statut = true)
        {
            return new UserRole
            {
                IdUtilisateur = idUtilisateur,
                IdRole = idRole,
                IsPrimary = isPrimary,
                Statut = statut,
                DateAttribution = DateTime.Now
            };
        }

        public static Permission CreatePermission(int id, string nom, string categorie = "General")
        {
            return new Permission
            {
                IdPermission = id,
                Nom = nom,
                Description = $"Description pour {nom}",
                Categorie = categorie,
                Statut = true
            };
        }

        public static RolePermission CreateRolePermission(int idRole, int idPermission)
        {
            return new RolePermission
            {
                IdRole = idRole,
                IdPermission = idPermission
            };
        }

        public static Ecole CreateEcole(int id, string nom, bool statut = true)
        {
            return new Ecole
            {
                IdEcole = id,
                Nom = nom,
                Statut = statut,
                DateCreation = DateTime.Now
            };
        }

        public static Direction CreateDirection(int id, int idEcole, string nomDirection = "Direction Test", bool statut = true)
        {
            return new Direction
            {
                IdDirection = id,
                IdEcole = idEcole,
                NomDirection = nomDirection,
                Statut = statut
            };
        }

        public static Section CreateSection(int id, string nomSection, int? idEcole = null, bool statut = true)
        {
            return new Section
            {
                IdSection = id,
                NomSection = nomSection,
                IdEcole = idEcole,
                Statut = statut,
                DateCreation = DateTime.Now
            };
        }

        public static Option CreateOption(int id, string nomOption, int idSection, bool statut = true)
        {
            return new Option
            {
                IdOption = id,
                NomOption = nomOption,
                IdSection = idSection,
                Statut = statut,
                DateCreation = DateTime.Now
            };
        }

        public static Classe CreateClasse(
            int id,
            string nomClasse,
            int? idDirection = null,
            int? idSection = null,
            int? idOption = null,
            bool statut = true)
        {
            return new Classe
            {
                IdClasse = id,
                NomClasse = nomClasse,
                IdDirection = idDirection,
                IdSection = idSection,
                IdOption = idOption,
                Statut = statut,
                DateCreation = DateTime.Now
            };
        }

        public static AnneeScolaire CreateAnneeScolaire(int id, int idEcole, string libelle, DateTime? debut = null, DateTime? fin = null, bool statut = true)
        {
            debut ??= new DateTime(DateTime.UtcNow.Year, 9, 1);
            fin ??= debut.Value.AddYears(1).AddDays(-1);
            return new AnneeScolaire
            {
                IdAnneeScolaire = id,
                IdEcole = idEcole,
                LibelleAnneeScolaire = libelle,
                DateDebut = debut.Value,
                DateFin = fin.Value,
                Statut = statut,
                DateCreation = DateTime.Now
            };
        }

        public static Agent CreateAgent(int id, string nom, int? idEcole = null, bool statut = true)
        {
            return new Agent
            {
                IdAgent = id,
                Nom = nom,
                Postnom = "Test",
                Prenom = "Agent",
                Genre = "M",
                DateNaissance = new DateTime(1985, 1, 1),
                IdEcole = idEcole,
                Statut = statut,
                DateCreation = DateTime.Now
            };
        }

        public static Tuteur CreateTuteur(int id, string nom, string? telephone = null, string? email = null, bool statut = true)
        {
            return new Tuteur
            {
                IdTuteur = id,
                NomComplet = nom,
                Genre = "M",
                Telephone = telephone ?? $"+243900000{id:D3}",
                Email = email,
                Statut = statut,
                DateCreation = DateTime.Now
            };
        }

        public static Eleve CreateEleve(int id, int? idTuteur, string nom = "Eleve", bool statut = true)
        {
            return new Eleve
            {
                IdEleve = id,
                Nom = nom,
                Postnom = "Test",
                Prenom = "Jean",
                NomComplet = $"{nom} Test Jean",
                Genre = "M",
                DateNaissance = new DateTime(2015, 5, 1),
                Nationalite = "RDC",
                IdTuteur = idTuteur,
                Statut = statut,
                DateCreation = DateTime.Now
            };
        }

        public static Inscription CreateInscription(
            int id,
            int idEleve,
            int idEcole,
            int idClasse,
            int idAnneeScolaire,
            string statutInscription = "Confirmé",
            bool statut = true,
            DateTime? dateInscription = null)
        {
            return new Inscription
            {
                IdInscription = id,
                Type = "Inscription",
                IdEleve = idEleve,
                IdEcole = idEcole,
                IdClasse = idClasse,
                IdAnneeScolaire = idAnneeScolaire,
                DateInscription = dateInscription ?? DateTime.UtcNow,
                StatutInscription = statutInscription,
                Statut = statut,
                DateCreation = DateTime.UtcNow
            };
        }
    }
}

