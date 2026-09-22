namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// Entrée publique minimale du registre élève (endpoint anonyme).
    /// Pas d'idEleve, pas de matricule, pas de contacts.
    /// </summary>
    public class RegistreEleveDto
    {
        public string? Nom { get; set; }
        public string? Postnom { get; set; }
        public string? Prenom { get; set; }
        public string? NomClasse { get; set; }
        public string? NomEcole { get; set; }
        public string? LibelleAnneeScolaire { get; set; }
    }
}
