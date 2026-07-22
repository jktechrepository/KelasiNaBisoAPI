namespace KelasiNaBiso.Models.DTOs.DevoirADomicile
{
    /// <summary>
    /// DTO pour la réponse d'un devoir à domicile
    /// </summary>
    public class DevoirADomicileDto
    {
        public int IdDevoirADomicile { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Contenu { get; set; } // Contenu textuel du devoir
        public string? NomFichier { get; set; }
        public long? TailleFichier { get; set; }
        public string? TypeMIME { get; set; }
        
        // Relations
        public int IdEcole { get; set; }
        public string? NomEcole { get; set; } // Pour affichage
        
        public int IdDirection { get; set; }
        public string? NomDirection { get; set; } // Pour affichage
        
        public int IdAgent { get; set; }
        public string? NomAgent { get; set; } // Pour affichage (NomComplet)
        
        public int IdClasse { get; set; }
        public string? NomClasse { get; set; } // Pour affichage
        
        public int? IdCours { get; set; }
        public string? NomCours { get; set; } // Pour affichage
        
        // Dates
        public DateTime DatePublication { get; set; }
        public DateTime? DateLimite { get; set; }
        
        // Statistiques
        public int NombreTelechargements { get; set; }
        
        // Statut
        public bool Statut { get; set; }
    }
}

