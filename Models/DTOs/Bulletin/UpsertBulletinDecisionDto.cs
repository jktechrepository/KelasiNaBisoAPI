using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs.Bulletin
{
    public class UpsertBulletinDecisionDto
    {
        /// <summary>Recommandé. Sinon résolution via Periode.</summary>
        public int? IdPeriode { get; set; }

        /// <summary>Code / libellé / alias si IdPeriode absent.</summary>
        [StringLength(100)]
        public string? Periode { get; set; }

        public int? IdAnneeScolaire { get; set; }

        [StringLength(100)]
        public string? Decision { get; set; }

        [StringLength(1000)]
        public string? AppreciationGenerale { get; set; }
    }

    public class BulletinDecisionDto
    {
        public int IdBulletinDecision { get; set; }
        public int IdEleve { get; set; }
        public int IdAnneeScolaire { get; set; }
        public int IdPeriode { get; set; }
        public string? Decision { get; set; }
        public string? AppreciationGenerale { get; set; }
        public int? IdAuteur { get; set; }
        public DateTime? DateModification { get; set; }
    }
}
