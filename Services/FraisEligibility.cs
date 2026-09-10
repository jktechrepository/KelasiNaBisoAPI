using KelasiNaBiso.Models;
using KelasiNaBiso.Models.Enums;

namespace KelasiNaBiso.Services
{
    /// <summary>
    /// Éligibilité d'un frais pour une inscription (école + année + portée XOR).
    /// </summary>
    public static class FraisEligibility
    {
        public static IQueryable<Frais> FilterForInscription(
            IQueryable<Frais> query,
            int idEcole,
            int idDirection,
            int idAnneeScolaire,
            int idClasse)
        {
            return query.Where(f =>
                f.Statut == true
                && f.IdEcole == idEcole
                && f.IdAnneeScolaire == idAnneeScolaire
                && (
                    (f.Portee == PorteeFrais.Classe
                        && f.FraisClasses.Any(fc => fc.IdClasse == idClasse))
                    || (f.Portee == PorteeFrais.Direction
                        && f.FraisDirections.Any(fd => fd.IdDirection == idDirection))
                ));
        }

        public static bool IsEligibleForInscription(
            Frais frais,
            int idEcole,
            int idDirection,
            int idAnneeScolaire,
            int idClasse)
        {
            if (frais.Statut != true)
                return false;
            if (frais.IdEcole != idEcole)
                return false;
            if (frais.IdAnneeScolaire != idAnneeScolaire)
                return false;

            if (frais.Portee == PorteeFrais.Classe)
                return frais.FraisClasses != null && frais.FraisClasses.Any(fc => fc.IdClasse == idClasse);

            return frais.FraisDirections != null && frais.FraisDirections.Any(fd => fd.IdDirection == idDirection);
        }
    }
}
