using System.ComponentModel.DataAnnotations;

namespace KelasiNaBiso.Models.DTOs.Communication
{
    /// <summary>
    /// Sélection des canaux d'envoi pour une campagne de communication.
    /// </summary>
    public class CommunicationChannelsDto
    {
        [Required]
        public bool Push { get; set; } = true;

        [Required]
        public bool Email { get; set; } = false;

        [Required]
        public bool Sms { get; set; } = false;

        [Required]
        public bool InApp { get; set; } = true;

        /// <summary>
        /// Retourne la liste des canaux activés sous forme de tableau de chaînes.
        /// </summary>
        public string[] GetEnabledChannels()
        {
            var channels = new List<string>(4);
            if (Push) channels.Add("Push");
            if (Email) channels.Add("Email");
            if (Sms) channels.Add("Sms");
            if (InApp) channels.Add("InApp");
            return channels.ToArray();
        }
    }
}

