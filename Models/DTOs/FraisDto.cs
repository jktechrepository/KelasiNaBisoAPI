using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;

namespace KelasiNaBiso.Models.DTOs
{
    public class FraisDirectionItemDto
    {
        public int IdDirection { get; set; }
        public string? NomDirection { get; set; }
    }

    public class FraisClasseItemDto
    {
        public int IdClasse { get; set; }
        public string? NomClasse { get; set; }
        public int? IdDirection { get; set; }
    }

    public class FraisDto
    {
        public int IdFrais { get; set; }
        public string LibelleFrais { get; set; } = string.Empty;
        public double Montant { get; set; }
        public string Devise { get; set; } = string.Empty;
        public string? TypeFrais { get; set; }
        public string? Periodicite { get; set; }
        public string? Description { get; set; }
        public bool? Statut { get; set; }
        public int IdEcole { get; set; }
        public string? NomEcole { get; set; }
        public int IdAnneeScolaire { get; set; }
        public string? LibelleAnneeScolaire { get; set; }
        public PorteeFrais Portee { get; set; }
        public List<FraisDirectionItemDto> Directions { get; set; } = new();
        public List<FraisClasseItemDto> Classes { get; set; } = new();

        public static FraisDto FromEntity(Frais f) => new()
        {
            IdFrais = f.IdFrais,
            LibelleFrais = f.LibelleFrais,
            Montant = f.Montant,
            Devise = f.Devise,
            TypeFrais = f.TypeFrais,
            Periodicite = f.Periodicite,
            Description = f.Description,
            Statut = f.Statut,
            IdEcole = f.IdEcole,
            NomEcole = f.Ecole?.Nom,
            IdAnneeScolaire = f.IdAnneeScolaire,
            LibelleAnneeScolaire = f.AnneeScolaire?.LibelleAnneeScolaire,
            Portee = f.Portee,
            Directions = (f.FraisDirections ?? Array.Empty<FraisDirection>())
                .Select(fd => new FraisDirectionItemDto
                {
                    IdDirection = fd.IdDirection,
                    NomDirection = fd.Direction?.NomDirection
                })
                .OrderBy(d => d.NomDirection)
                .ToList(),
            Classes = (f.FraisClasses ?? Array.Empty<FraisClasse>())
                .Select(fc => new FraisClasseItemDto
                {
                    IdClasse = fc.IdClasse,
                    NomClasse = fc.Classe?.NomClasse,
                    IdDirection = fc.Classe?.IdDirection
                })
                .OrderBy(c => c.NomClasse)
                .ToList()
        };
    }
}
