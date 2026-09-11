using KelasiNaBiso.Models;

namespace KelasiNaBiso.Models.DTOs
{
    /// <summary>
    /// Ligne de paiement élève paginée, enrichie du solde restant sur le frais.
    /// </summary>
    public class PaiementElevePagedItemDto : PaiementListItemDto
    {
        public string? LibelleFrais { get; set; }
        public double? MontantFrais { get; set; }
        public string? DeviseFrais { get; set; }
        public decimal? TotalPayeSurFrais { get; set; }
        public decimal? ResteAPayer { get; set; }
        public string? CodeDeviseReste { get; set; }

        public static PaiementElevePagedItemDto FromEntity(
            Models.Paiement p,
            string? libelleFrais = null,
            double? montantFrais = null,
            string? deviseFrais = null,
            decimal? totalPayeSurFrais = null)
        {
            var baseDto = PaiementListItemDto.FromEntity(p);
            var reste = montantFrais.HasValue && totalPayeSurFrais.HasValue
                ? Math.Max(0m, (decimal)montantFrais.Value - totalPayeSurFrais.Value)
                : (decimal?)null;

            return new PaiementElevePagedItemDto
            {
                IdPaiement = baseDto.IdPaiement,
                DatePaiement = baseDto.DatePaiement,
                Montant = baseDto.Montant,
                Devise = baseDto.Devise,
                CodeDevisePaiement = baseDto.CodeDevisePaiement,
                CodeDevisePrincipale = baseDto.CodeDevisePrincipale,
                TauxVersDevisePrincipale = baseDto.TauxVersDevisePrincipale,
                MontantPayeDevisePrincipale = baseDto.MontantPayeDevisePrincipale,
                ModePaiement = baseDto.ModePaiement,
                Statut = baseDto.Statut,
                StatutPaiement = baseDto.StatutPaiement,
                ReferenceTransaction = baseDto.ReferenceTransaction,
                MontantNet = baseDto.MontantNet,
                MontantCollecte = baseDto.MontantCollecte,
                OperateurMobileMoney = baseDto.OperateurMobileMoney,
                JustificatifUrl = baseDto.JustificatifUrl,
                Commentaire = baseDto.Commentaire,
                DateEnregistrement = baseDto.DateEnregistrement,
                ReferencePaiemenet = baseDto.ReferencePaiemenet,
                IdFrais = baseDto.IdFrais,
                IdEleve = baseDto.IdEleve,
                IdUtilisateur = baseDto.IdUtilisateur,
                MatriculeEleve = baseDto.MatriculeEleve,
                NomCompletEleve = baseDto.NomCompletEleve,
                LibelleFrais = libelleFrais,
                MontantFrais = montantFrais,
                DeviseFrais = deviseFrais,
                TotalPayeSurFrais = totalPayeSurFrais,
                ResteAPayer = reste,
                CodeDeviseReste = deviseFrais
            };
        }
    }
}
