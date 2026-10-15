using KelasiNaBiso.Models;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// Affectation titulaire encore ouverte (DateFin null, Statut actif).
    /// </summary>
    public class TitulaireActifDto
    {
        public int IdTitulaireClasse { get; set; }
        public int IdClasse { get; set; }
        public string? NomClasse { get; set; }
        public int IdAnneeScolaire { get; set; }
        public string? LibelleAnneeScolaire { get; set; }
        public DateTime DateDebut { get; set; }
    }

    /// <summary>
    /// GET /api/Agent/{id} : champs agent à la racine + titulairesActifs (liste, vide si aucun).
    /// </summary>
    public class AgentDetailDto
    {
        public int IdAgent { get; set; }
        public string? Matricule { get; set; }
        public string? Nom { get; set; }
        public string? Postnom { get; set; }
        public string? Prenom { get; set; }
        public string? Genre { get; set; }
        public DateTime DateNaissance { get; set; }
        public string? TelephoneAgent { get; set; }
        public string? EmailAgent { get; set; }
        public bool? Statut { get; set; }
        public string? EtatCivil { get; set; }
        public string? SerialNumber { get; set; }
        public string? Fonction { get; set; }
        public string? RoleAgent { get; set; }
        public string? PhotoUrl { get; set; }
        public int? IdEcole { get; set; }
        public string? Province { get; set; }
        public string? Ville { get; set; }
        public string? Commune { get; set; }
        public string? Quartier { get; set; }
        public string? Avenue { get; set; }
        public string? Numero { get; set; }

        public List<TitulaireActifDto> TitulairesActifs { get; set; } = new();

        public static AgentDetailDto From(Agent agent, IReadOnlyList<TitulaireActifDto> titulairesActifs) =>
            new()
            {
                IdAgent = agent.IdAgent,
                Matricule = agent.Matricule,
                Nom = agent.Nom,
                Postnom = agent.Postnom,
                Prenom = agent.Prenom,
                Genre = agent.Genre,
                DateNaissance = agent.DateNaissance,
                TelephoneAgent = agent.TelephoneAgent,
                EmailAgent = agent.EmailAgent,
                Statut = agent.Statut,
                EtatCivil = agent.EtatCivil,
                SerialNumber = agent.SerialNumber,
                Fonction = agent.Fonction,
                RoleAgent = agent.RoleAgent,
                PhotoUrl = agent.PhotoUrl,
                IdEcole = agent.IdEcole,
                Province = agent.Province,
                Ville = agent.Ville,
                Commune = agent.Commune,
                Quartier = agent.Quartier,
                Avenue = agent.Avenue,
                Numero = agent.Numero,
                TitulairesActifs = titulairesActifs.ToList()
            };
    }
}
