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
    }
}

