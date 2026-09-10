using KelasiNaBiso.Models;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// Paiement enrichi pour les listes (école, journal) avec infos élève.
    /// </summary>
    public class PaiementListItemDto
    {
        public int IdPaiement { get; set; }
        public DateTime DatePaiement { get; set; }
        public double Montant { get; set; }
        public string? Devise { get; set; }
        public string? CodeDevisePaiement { get; set; }
        public string? CodeDevisePrincipale { get; set; }
        public decimal? TauxVersDevisePrincipale { get; set; }
        public decimal? MontantPayeDevisePrincipale { get; set; }
        public string? ModePaiement { get; set; }
        public bool? Statut { get; set; }
        public string? StatutPaiement { get; set; }
        public string? ReferenceTransaction { get; set; }
        public decimal? MontantNet { get; set; }
        public decimal? MontantCollecte { get; set; }
        public string? OperateurMobileMoney { get; set; }
        public string? JustificatifUrl { get; set; }
        public string? Commentaire { get; set; }
        public DateTime DateEnregistrement { get; set; }
        public string ReferencePaiemenet { get; set; } = string.Empty;
        public int? IdFrais { get; set; }
        public int? IdEleve { get; set; }
        public int? IdUtilisateur { get; set; }
        public string? MatriculeEleve { get; set; }
        public string? NomCompletEleve { get; set; }

        public static PaiementListItemDto FromEntity(Models.Paiement p) => new()
        {
            IdPaiement = p.IdPaiement,
            DatePaiement = p.DatePaiement,
            Montant = p.Montant,
            Devise = p.Devise,
            CodeDevisePaiement = p.CodeDevisePaiement,
            CodeDevisePrincipale = p.CodeDevisePrincipale,
            TauxVersDevisePrincipale = p.TauxVersDevisePrincipale,
            MontantPayeDevisePrincipale = p.MontantPayeDevisePrincipale,
            ModePaiement = p.ModePaiement,
            Statut = p.Statut,
            StatutPaiement = p.StatutPaiement,
            ReferenceTransaction = p.ReferenceTransaction,
            MontantNet = p.MontantNet,
            MontantCollecte = p.MontantCollecte,
            OperateurMobileMoney = p.OperateurMobileMoney,
            JustificatifUrl = p.JustificatifUrl,
            Commentaire = p.Commentaire,
            DateEnregistrement = p.DateEnregistrement,
            ReferencePaiemenet = p.ReferencePaiemenet,
            IdFrais = p.IdFrais,
            IdEleve = p.IdEleve,
            IdUtilisateur = p.IdUtilisateur,
            MatriculeEleve = p.Eleve?.Matricule,
            NomCompletEleve = p.Eleve?.NomComplet
        };
    }
}
