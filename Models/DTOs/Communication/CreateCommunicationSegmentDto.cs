using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs.Communication
{
    public class CreateCommunicationSegmentDto
    {
        [Required]
        [MaxLength(150)]
        public string NomSegment { get; set; } = string.Empty;

        /// <summary>
        /// Type de segment : Classe, Direction, Niveau, Tag, AdHoc
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TypeSegment { get; set; } = string.Empty;

        public bool IsReusable { get; set; }

        /// <summary>
        /// Identifiants de classes ciblées (Type = Classe)
        /// </summary>
        public List<int>? ClasseIds { get; set; }

        /// <summary>
        /// Identifiants de directions ciblées (Type = Direction)
        /// </summary>
        public List<int>? DirectionIds { get; set; }

        /// <summary>
        /// Identifiants d'utilisateurs (parents) ciblés (Type = AdHoc)
        /// </summary>
        public List<int>? UtilisateurIds { get; set; }

        /// <summary>
        /// Identifiants de tuteurs ciblés (Type = AdHoc)
        /// </summary>
        public List<int>? TuteurIds { get; set; }

        /// <summary>
        /// Niveau d'enseignement ciblé (Type = Niveau)
        /// </summary>
        [MaxLength(50)]
        public string? Niveau { get; set; }

        /// <summary>
        /// Tags personnalisés (Type = Tag)
        /// </summary>
        public List<string>? Tags { get; set; }
    }
}

