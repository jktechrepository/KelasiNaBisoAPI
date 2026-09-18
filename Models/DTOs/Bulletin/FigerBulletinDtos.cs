using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs.Bulletin
{
    public class FigerBulletinRequestDto
    {
        public int? IdPeriode { get; set; }

        [StringLength(100)]
        public string? Periode { get; set; }

        public int? IdAnneeScolaire { get; set; }
    }

    public class FigerBulletinResultDto
    {
        public int Figes { get; set; }
        public int DejaFiges { get; set; }
        public IReadOnlyList<BulletinEleveDto> Bulletins { get; set; } = Array.Empty<BulletinEleveDto>();
    }
}
