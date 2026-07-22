using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace KelasiNaBiso.Models.DTOs
{
    public class CreateVacationDto
    {
        [Required]
        [MaxLength(50)]
        public string NomVacation { get; set; }

        [Required]
        public string HeureDebut { get; set; }

        [Required]
        public string HeureFin { get; set; }

        public string? HeureDebutPause { get; set; }

        public string? HeureFinPause { get; set; }

        public int NombreJoursParSemaine { get; set; } = 5;

        [Required]
        public int IdEcole { get; set; }

        // Méthodes de conversion
        public TimeSpan GetHeureDebut()
        {
            return ParseTimeString(HeureDebut);
        }

        public TimeSpan GetHeureFin()
        {
            return ParseTimeString(HeureFin);
        }

        public TimeSpan? GetHeureDebutPause()
        {
            return string.IsNullOrEmpty(HeureDebutPause) ? null : ParseTimeString(HeureDebutPause);
        }

        public TimeSpan? GetHeureFinPause()
        {
            return string.IsNullOrEmpty(HeureFinPause) ? null : ParseTimeString(HeureFinPause);
        }

        private TimeSpan ParseTimeString(string timeString)
        {
            // Formats acceptés
            string[] formats = { "HH:mm", "H:mm", "HH:mm:ss", "H:mm:ss", "HHhmm", "Hhmm" };

            foreach (var format in formats)
            {
                if (TimeSpan.TryParseExact(timeString, format, CultureInfo.InvariantCulture, out TimeSpan result))
                {
                    return result;
                }
            }

            // Essayer de parser avec le format standard
            if (TimeSpan.TryParse(timeString, out TimeSpan standardResult))
            {
                return standardResult;
            }

            throw new ArgumentException($"Format de temps invalide: {timeString}. Utilisez le format HH:mm (ex: 07:30) ou HHhmm (ex: 7h30)");
        }
    }
}
