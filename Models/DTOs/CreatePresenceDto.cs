using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace KelasiNaBiso.Models.DTOs
{
    public class CreatePresenceDto
    {
        // ✅ POINTAGE FLEXIBLE: Peut concerner un élève OU un agent
        // Au moins un des deux doit être renseigné (validation dans le service)
        public int? IdEleve { get; set; }
        public int? IdAgent { get; set; }

        // ✅ INDICATEUR DE PRÉSENCE: Indique si la personne est effectivement présente
        public bool? IsPresent { get; set; }

        [Required]
        public string HeureArrivee { get; set; } = string.Empty;

        // ✅ HeureDepart peut être null (départ pas encore enregistré)
        public string? HeureDepart { get; set; }

        [Required]
        public DateTime DateDuJour { get; set; }

        // ✅ OBSERVATION: Note ou observation sur la présence
        [MaxLength(500)]
        public string? Observation { get; set; }

        public string? Longitute { get; set; }
        public string? Latitude { get; set; }

        // ✅ IdVacation peut être null (vacation non spécifiée)
        public int? IdVacation { get; set; }

        public TimeSpan GetHeureArrivee()
        {
            return ParseTime(HeureArrivee);
        }

        public TimeSpan? GetHeureDepart()
        {
            // Si HeureDepart est null ou vide, retourner null
            if (string.IsNullOrWhiteSpace(HeureDepart))
            {
                return null;
            }
            return ParseTime(HeureDepart);
        }

        private TimeSpan ParseTime(string timeString)
        {
            // Nettoyer la chaîne
            timeString = timeString.Trim().ToLower();

            // Remplacer "h" par ":" pour le format "7h30" -> "7:30"
            if (timeString.Contains("h"))
            {
                timeString = timeString.Replace("h", ":");
            }

            // Essayer de parser avec différents formats
            string[] formats = { "HH:mm", "H:mm", "HH:mm:ss", "H:mm:ss" };

            foreach (var format in formats)
            {
                if (TimeSpan.TryParseExact(timeString, format, CultureInfo.InvariantCulture, out TimeSpan result))
                {
                    return result;
                }
            }

            // Essayer le parsing standard
            if (TimeSpan.TryParse(timeString, out TimeSpan standardResult))
            {
                return standardResult;
            }

            throw new ArgumentException($"Format de temps invalide: {timeString}. Utilisez le format HH:mm (ex: 07:30) ou HHhmm (ex: 7h30)");
        }
    }
}
