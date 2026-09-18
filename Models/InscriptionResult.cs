using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Models
{
    // Classe pour le résultat de l'inscription
    public class InscriptionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? IdInscription { get; set; }
        public int? IdEleve { get; set; }
        public int? IdTuteur { get; set; }
        public Inscription Inscription { get; set; }
        
        // Informations du compte utilisateur créé automatiquement pour le tuteur
        public UtilisateurInfo? CompteUtilisateurTuteur { get; set; }

        // Informations du compte utilisateur créé automatiquement pour l'élève (matricule / MDP)
        public UtilisateurInfo? CompteUtilisateurEleve { get; set; }
    }
}
