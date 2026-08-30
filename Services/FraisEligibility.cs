using KelasiNaBiso.Models;

namespace KelasiNaBiso.Services
{
    /// <summary>
    /// Éligibilité d'un frais pour une inscription (direction + année + classe optionnelle).
    /// </summary>
    public static class FraisEligibility
    {
        public static IQueryable<Frais> FilterForInscription(
            IQueryable<Frais> query,
            int idDirection,
            int idAnneeScolaire,
            int idClasse)
        {
            return query.Where(f =>
                f.Statut == true
                && f.IdDirection == idDirection
                && f.IdAnneeScolaire == idAnneeScolaire
                && (f.IdClasse == null || f.IdClasse == idClasse));
        }

        public static bool IsEligibleForInscription(
            Frais frais,
            int idDirection,
            int idAnneeScolaire,
            int idClasse)
        {
            if (frais.Statut != true)
                return false;
            if (frais.IdDirection != idDirection)
                return false;
            if (frais.IdAnneeScolaire != idAnneeScolaire)
                return false;
            return !frais.IdClasse.HasValue || frais.IdClasse.Value == idClasse;
        }
    }
}
