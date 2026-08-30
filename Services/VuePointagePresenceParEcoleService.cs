using KelasiNaBiso.Data;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class VuePointagePresenceParEcoleService : IVuePointagePresenceParEcoleRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public VuePointagePresenceParEcoleService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetAllAsync()
        {
            return await _context.VuePointagePresenceParEcole
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<VuePointagePresenceParEcoleDTO?> GetByIdAsync(int idPresence)
        {
            return await _context.VuePointagePresenceParEcole
                .FirstOrDefaultAsync(p => p.IdPresence == idPresence);
        }

        // Filtres par école
        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByEcoleAsync(int idEcole)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.IdEcole == idEcole)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByEcoleNameAsync(string nomEcole)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.NomEcole.Contains(nomEcole))
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByTypeEcoleAsync(string typeEcole)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.TypeEcole == typeEcole)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        // Filtres par élève
        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByEleveAsync(int idEleve)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.IdEleve == idEleve)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByEleveReferenceAsync(Guid referenceEleve)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.ReferenceEleve == referenceEleve)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByEleveMatriculeAsync(string matricule)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.Matricule.Contains(matricule))
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByEleveNameAsync(string nomEleve)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.NomCompletFormate.Contains(nomEleve))
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByEleveGenreAsync(string genre)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.Genre == genre)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByEleveStatutAsync(bool statut)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.StatutEleve == statut)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        // Filtres par classe
        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByClasseAsync(int idClasse)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.IdClasse == idClasse)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByClasseNameAsync(string nomClasse)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.NomClasse.Contains(nomClasse))
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        // Filtres par section
        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetBySectionAsync(int idSection)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.IdSection == idSection)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetBySectionNameAsync(string nomSection)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.NomSection.Contains(nomSection))
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        // Filtres par direction
        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByDirectionAsync(int idDirection)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.IdDirection == idDirection)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByDirectionNameAsync(string nomDirection)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.NomDirection.Contains(nomDirection))
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        // Filtres par option
        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByOptionAsync(int idOption)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.IdOption == idOption)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByOptionNameAsync(string nomOption)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.NomOption.Contains(nomOption))
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        // Filtres par tuteur
        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByTuteurAsync(int idTuteur)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.IdTuteur == idTuteur)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByTuteurNameAsync(string nomTuteur)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.NomTuteur.Contains(nomTuteur))
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByTuteurContactAsync(string contact)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.TelephoneTuteur == contact || p.EmailTuteur == contact || p.TelephoneRepresentant == contact)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByTuteurStatutAsync(bool statut)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.StatutTuteur == statut)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        // Filtres par horaire/vacation
        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByVacationAsync(int IdHoraire)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.IdHoraire == IdHoraire)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByVacationNameAsync(string nomVacation)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.NomVacation.Contains(nomVacation))
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        // Filtres par présence
        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByPresenceStatutAsync(string statut)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.StatutPresence == statut)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByDateDuJourAsync(DateTime dateDuJour)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.DateDuJour.Value.Date == dateDuJour.Date)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.DateDuJour >= dateDebut && p.DateDuJour <= dateFin)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByHeureArriveeRangeAsync(TimeSpan heureDebut, TimeSpan heureFin)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.HeureArrivee >= heureDebut && p.HeureArrivee <= heureFin)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByHeureDepartRangeAsync(TimeSpan heureDebut, TimeSpan heureFin)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.HeureDepart >= heureDebut && p.HeureDepart <= heureFin)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByRetardAsync(TimeSpan heureLimite)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.HeureArrivee > heureLimite)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByAbsenceAsync()
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.StatutPresence == "Absent" || p.StatutPresence == "Absence")
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByPresenceAsync()
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.StatutPresence == "Présent" || p.StatutPresence == "Present")
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        // Filtres par localisation
        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByProvinceAsync(string province)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.ProvinceEleve == province || p.ProvinceEcole == province)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByVilleAsync(string ville)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.VilleEleve == ville || p.VilleEcole == ville)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByCommuneAsync(string commune)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.CommuneEleve == commune || p.CommuneEcole == commune)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetByLocalisationRangeAsync(string latMin, string latMax, string longMin, string longMax)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => !string.IsNullOrEmpty(p.Latitude) && !string.IsNullOrEmpty(p.Longitute) &&
                           p.Latitude.CompareTo(latMin) >= 0 && p.Latitude.CompareTo(latMax) <= 0 &&
                           p.Longitute.CompareTo(longMin) >= 0 && p.Longitute.CompareTo(longMax) <= 0)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        // Recherche générale
        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> SearchAsync(string searchTerm)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.NomCompletFormate.Contains(searchTerm) ||
                           p.Matricule.Contains(searchTerm) ||
                           p.NomEcole.Contains(searchTerm) ||
                           p.NomClasse.Contains(searchTerm) ||
                           p.NomTuteur.Contains(searchTerm) ||
                           p.NomVacation.Contains(searchTerm))
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        // Comptages
        public async Task<int> GetCountByEcoleAsync(int idEcole)
        {
            return await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.IdEcole == idEcole);
        }

        public async Task<int> GetCountByClasseAsync(int idClasse)
        {
            return await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.IdClasse == idClasse);
        }

        public async Task<int> GetCountBySectionAsync(int idSection)
        {
            return await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.IdSection == idSection);
        }

        public async Task<int> GetCountByDirectionAsync(int idDirection)
        {
            return await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.IdDirection == idDirection);
        }

        public async Task<int> GetCountByTuteurAsync(int idTuteur)
        {
            return await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.IdTuteur == idTuteur);
        }

        public async Task<int> GetCountByVacationAsync(int IdHoraire)
        {
            return await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.IdHoraire == IdHoraire);
        }

        public async Task<int> GetCountByPresenceStatutAsync(string statut)
        {
            return await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.StatutPresence == statut);
        }

        public async Task<int> GetCountByDateRangeAsync(DateTime dateDebut, DateTime dateFin)
        {
            return await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.DateDuJour >= dateDebut && p.DateDuJour <= dateFin);
        }

        public async Task<int> GetCountByDateDuJourAsync(DateTime dateDuJour)
        {
            return await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.DateDuJour.Value.Date == dateDuJour.Date);
        }

        // Statistiques de présence
        public async Task<int> GetCountPresenceByEcoleAsync(int idEcole, DateTime dateDuJour)
        {
            return await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.IdEcole == idEcole &&
                                p.DateDuJour.Value.Date == dateDuJour.Date &&
                                (p.StatutPresence == "Présent" || p.StatutPresence == "Present"));
        }

        public async Task<int> GetCountAbsenceByEcoleAsync(int idEcole, DateTime dateDuJour)
        {
            return await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.IdEcole == idEcole &&
                                p.DateDuJour.Value.Date == dateDuJour.Date &&
                                (p.StatutPresence == "Absent" || p.StatutPresence == "Absence"));
        }

        public async Task<int> GetCountRetardByEcoleAsync(int idEcole, DateTime dateDuJour)
        {
            return await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.IdEcole == idEcole &&
                                p.DateDuJour.Value.Date == dateDuJour.Date &&
                                p.StatutPresence == "Retard");
        }

        public async Task<double> GetTauxPresenceByEcoleAsync(int idEcole, DateTime dateDebut, DateTime dateFin)
        {
            var total = await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.IdEcole == idEcole &&
                                p.DateDuJour >= dateDebut && p.DateDuJour <= dateFin);

            var presents = await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.IdEcole == idEcole &&
                                p.DateDuJour >= dateDebut && p.DateDuJour <= dateFin &&
                                (p.StatutPresence == "Présent" || p.StatutPresence == "Present"));

            return total > 0 ? (double)presents / total * 100 : 0;
        }

        public async Task<double> GetTauxPresenceByClasseAsync(int idClasse, DateTime dateDebut, DateTime dateFin)
        {
            var total = await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.IdClasse == idClasse &&
                                p.DateDuJour >= dateDebut && p.DateDuJour <= dateFin);

            var presents = await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.IdClasse == idClasse &&
                                p.DateDuJour >= dateDebut && p.DateDuJour <= dateFin &&
                                (p.StatutPresence == "Présent" || p.StatutPresence == "Present"));

            return total > 0 ? (double)presents / total * 100 : 0;
        }

        public async Task<double> GetTauxPresenceByEleveAsync(int idEleve, DateTime dateDebut, DateTime dateFin)
        {
            var total = await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.IdEleve == idEleve &&
                                p.DateDuJour >= dateDebut && p.DateDuJour <= dateFin);

            var presents = await _context.VuePointagePresenceParEcole
                .CountAsync(p => p.IdEleve == idEleve &&
                                p.DateDuJour >= dateDebut && p.DateDuJour <= dateFin &&
                                (p.StatutPresence == "Présent" || p.StatutPresence == "Present"));

            return total > 0 ? (double)presents / total * 100 : 0;
        }

        // Statistiques temporelles
        public async Task<TimeSpan> GetMoyenneHeureArriveeByEcoleAsync(int idEcole, DateTime dateDuJour)
        {
            var heures = await _context.VuePointagePresenceParEcole
                .Where(p => p.IdEcole == idEcole &&
                           p.DateDuJour.Value.Date == dateDuJour.Date &&
                           p.HeureArrivee.HasValue)
                .Select(p => p.HeureArrivee.Value)
                .ToListAsync();

            if (!heures.Any()) return TimeSpan.Zero;

            var totalTicks = heures.Sum(h => h.Ticks);
            return TimeSpan.FromTicks(totalTicks / heures.Count);
        }

        public async Task<TimeSpan> GetMoyenneHeureDepartByEcoleAsync(int idEcole, DateTime dateDuJour)
        {
            var heures = await _context.VuePointagePresenceParEcole
                .Where(p => p.IdEcole == idEcole &&
                           p.DateDuJour.Value.Date == dateDuJour.Date &&
                           p.HeureDepart.HasValue)
                .Select(p => p.HeureDepart.Value)
                .ToListAsync();

            if (!heures.Any()) return TimeSpan.Zero;

            var totalTicks = heures.Sum(h => h.Ticks);
            return TimeSpan.FromTicks(totalTicks / heures.Count);
        }

        public async Task<TimeSpan> GetMoyenneHeureArriveeByClasseAsync(int idClasse, DateTime dateDuJour)
        {
            var heures = await _context.VuePointagePresenceParEcole
                .Where(p => p.IdClasse == idClasse &&
                           p.DateDuJour.Value.Date == dateDuJour.Date &&
                           p.HeureArrivee.HasValue)
                .Select(p => p.HeureArrivee.Value)
                .ToListAsync();

            if (!heures.Any()) return TimeSpan.Zero;

            var totalTicks = heures.Sum(h => h.Ticks);
            return TimeSpan.FromTicks(totalTicks / heures.Count);
        }

        public async Task<TimeSpan> GetMoyenneHeureDepartByClasseAsync(int idClasse, DateTime dateDuJour)
        {
            var heures = await _context.VuePointagePresenceParEcole
                .Where(p => p.IdClasse == idClasse &&
                           p.DateDuJour.Value.Date == dateDuJour.Date &&
                           p.HeureDepart.HasValue)
                .Select(p => p.HeureDepart.Value)
                .ToListAsync();

            if (!heures.Any()) return TimeSpan.Zero;

            var totalTicks = heures.Sum(h => h.Ticks);
            return TimeSpan.FromTicks(totalTicks / heures.Count);
        }

        // === IMPLÉMENTATION DES NOUVEAUX ENDPOINTS POUR LE SUIVI PARENTAL ===

        // Suivi détaillé par parent
        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetSuiviEnfantByParentAsync(string contactParent)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => p.TelephoneTuteur == contactParent ||
                           p.EmailTuteur == contactParent ||
                           p.TelephoneRepresentant == contactParent)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetSuiviEnfantByParentAndDateAsync(string contactParent, DateTime date)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => (p.TelephoneTuteur == contactParent ||
                           p.EmailTuteur == contactParent ||
                           p.TelephoneRepresentant == contactParent) &&
                           p.DateDuJour.Value.Date == date.Date)
                .OrderBy(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetSuiviEnfantByParentAndDateRangeAsync(string contactParent, DateTime dateDebut, DateTime dateFin)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => (p.TelephoneTuteur == contactParent ||
                           p.EmailTuteur == contactParent ||
                           p.TelephoneRepresentant == contactParent) &&
                           p.DateDuJour >= dateDebut && p.DateDuJour <= dateFin)
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetRetardsEnfantByParentAsync(string contactParent, DateTime dateDebut, DateTime dateFin)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => (p.TelephoneTuteur == contactParent ||
                           p.EmailTuteur == contactParent ||
                           p.TelephoneRepresentant == contactParent) &&
                           p.DateDuJour >= dateDebut && p.DateDuJour <= dateFin &&
                           p.StatutPresence == "Retard")
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetAbsencesEnfantByParentAsync(string contactParent, DateTime dateDebut, DateTime dateFin)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => (p.TelephoneTuteur == contactParent ||
                           p.EmailTuteur == contactParent ||
                           p.TelephoneRepresentant == contactParent) &&
                           p.DateDuJour >= dateDebut && p.DateDuJour <= dateFin &&
                           (p.StatutPresence == "Absent" || p.StatutPresence == "Absence"))
                .OrderByDescending(p => p.DateDuJour)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePointagePresenceParEcoleDTO>> GetPresencesEnfantByParentAsync(string contactParent, DateTime dateDebut, DateTime dateFin)
        {
            return await _context.VuePointagePresenceParEcole
                .Where(p => (p.TelephoneTuteur == contactParent ||
                           p.EmailTuteur == contactParent ||
                           p.TelephoneRepresentant == contactParent) &&
                           p.DateDuJour >= dateDebut && p.DateDuJour <= dateFin &&
                           (p.StatutPresence == "Présent" || p.StatutPresence == "Present"))
                .OrderByDescending(p => p.DateDuJour)
                .ThenByDescending(p => p.HeureArrivee)
                .ToListAsync();
        }

        // Statistiques hebdomadaires
        public async Task<object> GetStatistiquesHebdomadairesEnfantAsync(string contactParent, DateTime dateDebutSemaine)
        {
            var dateFinSemaine = dateDebutSemaine.AddDays(6);
            var presences = await GetSuiviEnfantByParentAndDateRangeAsync(contactParent, dateDebutSemaine, dateFinSemaine);

            var total = presences.Count();
            var presents = presences.Count(p => p.StatutPresence == "Présent" || p.StatutPresence == "Present");
            var absents = presences.Count(p => p.StatutPresence == "Absent" || p.StatutPresence == "Absence");
            var retards = presences.Count(p => p.StatutPresence == "Retard");

            var tauxPresence = total > 0 ? (double)presents / total * 100 : 0;

            var heuresArrivee = presences.Where(p => p.HeureArrivee.HasValue).Select(p => p.HeureArrivee.Value).ToList();
            var moyenneArrivee = heuresArrivee.Any() ? TimeSpan.FromTicks(heuresArrivee.Sum(h => h.Ticks) / heuresArrivee.Count) : TimeSpan.Zero;

            var heuresDepart = presences.Where(p => p.HeureDepart.HasValue).Select(p => p.HeureDepart.Value).ToList();
            var moyenneDepart = heuresDepart.Any() ? TimeSpan.FromTicks(heuresDepart.Sum(h => h.Ticks) / heuresDepart.Count) : TimeSpan.Zero;

            return new
            {
                Periode = new { Debut = dateDebutSemaine, Fin = dateFinSemaine },
                TotalJours = total,
                Presences = presents,
                Absences = absents,
                Retards = retards,
                TauxPresence = Math.Round(tauxPresence, 2),
                MoyenneHeureArrivee = moyenneArrivee.ToString(@"hh\:mm"),
                MoyenneHeureDepart = moyenneDepart.ToString(@"hh\:mm"),
                Details = presences.Select(p => new
                {
                    Date = p.DateDuJour?.ToString("dd/MM/yyyy"),
                    Statut = p.StatutPresence,
                    HeureArrivee = p.HeureArrivee?.ToString(@"hh\:mm"),
                    HeureDepart = p.HeureDepart?.ToString(@"hh\:mm"),
                    Enfant = p.NomCompletFormate,
                    Classe = p.NomClasse
                })
            };
        }

        public async Task<object> GetStatistiquesHebdomadairesEnfantAsync(string contactParent, int semaine, int annee)
        {
            var dateDebut = GetDateDebutSemaine(semaine, annee);
            return await GetStatistiquesHebdomadairesEnfantAsync(contactParent, dateDebut);
        }

        // Statistiques mensuelles
        public async Task<object> GetStatistiquesMensuellesEnfantAsync(string contactParent, int mois, int annee)
        {
            var dateDebut = new DateTime(annee, mois, 1);
            var dateFin = dateDebut.AddMonths(1).AddDays(-1);
            var presences = await GetSuiviEnfantByParentAndDateRangeAsync(contactParent, dateDebut, dateFin);

            var total = presences.Count();
            var presents = presences.Count(p => p.StatutPresence == "Présent" || p.StatutPresence == "Present");
            var absents = presences.Count(p => p.StatutPresence == "Absent" || p.StatutPresence == "Absence");
            var retards = presences.Count(p => p.StatutPresence == "Retard");

            var tauxPresence = total > 0 ? (double)presents / total * 100 : 0;

            // Statistiques par semaine
            var semaines = new List<object>();
            var dateCourante = dateDebut;
            while (dateCourante <= dateFin)
            {
                var statsSemaine = await GetStatistiquesHebdomadairesEnfantAsync(contactParent, dateCourante);
                semaines.Add(statsSemaine);
                dateCourante = dateCourante.AddDays(7);
            }

            return new
            {
                Periode = new { Mois = mois, Annee = annee, Debut = dateDebut, Fin = dateFin },
                TotalJours = total,
                Presences = presents,
                Absences = absents,
                Retards = retards,
                TauxPresence = Math.Round(tauxPresence, 2),
                StatistiquesParSemaine = semaines
            };
        }

        public async Task<object> GetStatistiquesMensuellesEnfantAsync(string contactParent, DateTime date)
        {
            return await GetStatistiquesMensuellesEnfantAsync(contactParent, date.Month, date.Year);
        }

        // Statistiques trimestrielles
        public async Task<object> GetStatistiquesTrimestriellesEnfantAsync(string contactParent, int trimestre, int annee)
        {
            var (dateDebut, dateFin) = GetDatesTrimestre(trimestre, annee);
            var presences = await GetSuiviEnfantByParentAndDateRangeAsync(contactParent, dateDebut, dateFin);

            var total = presences.Count();
            var presents = presences.Count(p => p.StatutPresence == "Présent" || p.StatutPresence == "Present");
            var absents = presences.Count(p => p.StatutPresence == "Absent" || p.StatutPresence == "Absence");
            var retards = presences.Count(p => p.StatutPresence == "Retard");

            var tauxPresence = total > 0 ? (double)presents / total * 100 : 0;

            return new
            {
                Periode = new { Trimestre = trimestre, Annee = annee, Debut = dateDebut, Fin = dateFin },
                TotalJours = total,
                Presences = presents,
                Absences = absents,
                Retards = retards,
                TauxPresence = Math.Round(tauxPresence, 2)
            };
        }

        public async Task<object> GetStatistiquesTrimestriellesEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin)
        {
            var presences = await GetSuiviEnfantByParentAndDateRangeAsync(contactParent, dateDebut, dateFin);

            var total = presences.Count();
            var presents = presences.Count(p => p.StatutPresence == "Présent" || p.StatutPresence == "Present");
            var absents = presences.Count(p => p.StatutPresence == "Absent" || p.StatutPresence == "Absence");
            var retards = presences.Count(p => p.StatutPresence == "Retard");

            var tauxPresence = total > 0 ? (double)presents / total * 100 : 0;

            return new
            {
                Periode = new { Debut = dateDebut, Fin = dateFin },
                TotalJours = total,
                Presences = presents,
                Absences = absents,
                Retards = retards,
                TauxPresence = Math.Round(tauxPresence, 2)
            };
        }

        // Statistiques annuelles
        public async Task<object> GetStatistiquesAnnuellesEnfantAsync(string contactParent, int annee)
        {
            var dateDebut = new DateTime(annee, 1, 1);
            var dateFin = new DateTime(annee, 12, 31);
            var presences = await GetSuiviEnfantByParentAndDateRangeAsync(contactParent, dateDebut, dateFin);

            var total = presences.Count();
            var presents = presences.Count(p => p.StatutPresence == "Présent" || p.StatutPresence == "Present");
            var absents = presences.Count(p => p.StatutPresence == "Absent" || p.StatutPresence == "Absence");
            var retards = presences.Count(p => p.StatutPresence == "Retard");

            var tauxPresence = total > 0 ? (double)presents / total * 100 : 0;

            // Statistiques par mois
            var mois = new List<object>();
            for (int m = 1; m <= 12; m++)
            {
                var statsMois = await GetStatistiquesMensuellesEnfantAsync(contactParent, m, annee);
                mois.Add(statsMois);
            }

            return new
            {
                Annee = annee,
                TotalJours = total,
                Presences = presents,
                Absences = absents,
                Retards = retards,
                TauxPresence = Math.Round(tauxPresence, 2),
                StatistiquesParMois = mois
            };
        }

        // Moyennes et tendances
        public async Task<object> GetMoyennePresenceEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin)
        {
            var presences = await GetSuiviEnfantByParentAndDateRangeAsync(contactParent, dateDebut, dateFin);

            var total = presences.Count();
            var presents = presences.Count(p => p.StatutPresence == "Présent" || p.StatutPresence == "Present");
            var tauxPresence = total > 0 ? (double)presents / total * 100 : 0;

            return new
            {
                Periode = new { Debut = dateDebut, Fin = dateFin },
                TotalJours = total,
                Presences = presents,
                TauxPresence = Math.Round(tauxPresence, 2)
            };
        }

        public async Task<object> GetMoyenneHeuresArriveeEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin)
        {
            var presences = await GetSuiviEnfantByParentAndDateRangeAsync(contactParent, dateDebut, dateFin);
            var heuresArrivee = presences.Where(p => p.HeureArrivee.HasValue).Select(p => p.HeureArrivee.Value).ToList();

            if (!heuresArrivee.Any()) return new { Message = "Aucune donnée d'heure d'arrivée disponible" };

            var moyenne = TimeSpan.FromTicks(heuresArrivee.Sum(h => h.Ticks) / heuresArrivee.Count);
            var plusTot = heuresArrivee.Min();
            var plusTard = heuresArrivee.Max();

            return new
            {
                Periode = new { Debut = dateDebut, Fin = dateFin },
                NombreJours = heuresArrivee.Count,
                MoyenneArrivee = moyenne.ToString(@"hh\:mm"),
                PlusTot = plusTot.ToString(@"hh\:mm"),
                PlusTard = plusTard.ToString(@"hh\:mm")
            };
        }

        public async Task<object> GetMoyenneHeuresDepartEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin)
        {
            var presences = await GetSuiviEnfantByParentAndDateRangeAsync(contactParent, dateDebut, dateFin);
            var heuresDepart = presences.Where(p => p.HeureDepart.HasValue).Select(p => p.HeureDepart.Value).ToList();

            if (!heuresDepart.Any()) return new { Message = "Aucune donnée d'heure de départ disponible" };

            var moyenne = TimeSpan.FromTicks(heuresDepart.Sum(h => h.Ticks) / heuresDepart.Count);
            var plusTot = heuresDepart.Min();
            var plusTard = heuresDepart.Max();

            return new
            {
                Periode = new { Debut = dateDebut, Fin = dateFin },
                NombreJours = heuresDepart.Count,
                MoyenneDepart = moyenne.ToString(@"hh\:mm"),
                PlusTot = plusTot.ToString(@"hh\:mm"),
                PlusTard = plusTard.ToString(@"hh\:mm")
            };
        }

        public async Task<object> GetTendancePresenceEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin)
        {
            var presences = await GetSuiviEnfantByParentAndDateRangeAsync(contactParent, dateDebut, dateFin);

            // Grouper par semaine pour voir la tendance
            var tendance = presences
                .GroupBy(p => p.DateDuJour.Value.AddDays(-(int)p.DateDuJour.Value.DayOfWeek))
                .Select(g => new
                {
                    Semaine = g.Key.ToString("dd/MM/yyyy"),
                    Total = g.Count(),
                    Presences = g.Count(p => p.StatutPresence == "Présent" || p.StatutPresence == "Present"),
                    Taux = g.Count() > 0 ? Math.Round((double)g.Count(p => p.StatutPresence == "Présent" || p.StatutPresence == "Present") / g.Count() * 100, 2) : 0
                })
                .OrderBy(x => x.Semaine)
                .ToList();

            return new
            {
                Periode = new { Debut = dateDebut, Fin = dateFin },
                Tendance = tendance
            };
        }

        // Alertes et notifications
        public async Task<object> GetAlertesRetardEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin)
        {
            var retards = await GetRetardsEnfantByParentAsync(contactParent, dateDebut, dateFin);

            return new
            {
                Periode = new { Debut = dateDebut, Fin = dateFin },
                NombreRetards = retards.Count(),
                Retards = retards.Select(r => new
                {
                    Date = r.DateDuJour?.ToString("dd/MM/yyyy"),
                    HeureArrivee = r.HeureArrivee?.ToString(@"hh\:mm"),
                    Enfant = r.NomCompletFormate,
                    Classe = r.NomClasse
                })
            };
        }

        public async Task<object> GetAlertesAbsenceEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin)
        {
            var absences = await GetAbsencesEnfantByParentAsync(contactParent, dateDebut, dateFin);

            return new
            {
                Periode = new { Debut = dateDebut, Fin = dateFin },
                NombreAbsences = absences.Count(),
                Absences = absences.Select(a => new
                {
                    Date = a.DateDuJour?.ToString("dd/MM/yyyy"),
                    Enfant = a.NomCompletFormate,
                    Classe = a.NomClasse
                })
            };
        }

        public async Task<object> GetResumeJourEnfantAsync(string contactParent, DateTime date)
        {
            var presences = await GetSuiviEnfantByParentAndDateAsync(contactParent, date);

            if (!presences.Any()) return new { Message = "Aucune donnée pour cette date" };

            var presence = presences.First();

            return new
            {
                Date = date.ToString("dd/MM/yyyy"),
                Enfant = presence.NomCompletFormate,
                Classe = presence.NomClasse,
                Statut = presence.StatutPresence,
                HeureArrivee = presence.HeureArrivee?.ToString(@"hh\:mm"),
                HeureDepart = presence.HeureDepart?.ToString(@"hh\:mm"),
                Ecole = presence.NomEcole
            };
        }

        // Comparaisons
        public async Task<object> GetComparaisonPresenceEnfantAsync(string contactParent, DateTime periode1Debut, DateTime periode1Fin, DateTime periode2Debut, DateTime periode2Fin)
        {
            var stats1 = await GetMoyennePresenceEnfantAsync(contactParent, periode1Debut, periode1Fin);
            var stats2 = await GetMoyennePresenceEnfantAsync(contactParent, periode2Debut, periode2Fin);

            return new
            {
                Periode1 = new { Debut = periode1Debut, Fin = periode1Fin, Statistiques = stats1 },
                Periode2 = new { Debut = periode2Debut, Fin = periode2Fin, Statistiques = stats2 }
            };
        }

        public async Task<object> GetComparaisonAvecClasseEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin)
        {
            var presencesEnfant = await GetSuiviEnfantByParentAndDateRangeAsync(contactParent, dateDebut, dateFin);

            if (!presencesEnfant.Any()) return new { Message = "Aucune donnée pour cet enfant" };

            var classe = presencesEnfant.First().IdClasse;
            var presencesClasse = await _context.VuePointagePresenceParEcole
                .Where(p => p.IdClasse == classe && p.DateDuJour >= dateDebut && p.DateDuJour <= dateFin)
                .ToListAsync();

            var statsEnfant = await GetMoyennePresenceEnfantAsync(contactParent, dateDebut, dateFin);

            var totalClasse = presencesClasse.Count();
            var presentsClasse = presencesClasse.Count(p => p.StatutPresence == "Présent" || p.StatutPresence == "Present");
            var tauxClasse = totalClasse > 0 ? (double)presentsClasse / totalClasse * 100 : 0;

            return new
            {
                Periode = new { Debut = dateDebut, Fin = dateFin },
                Enfant = statsEnfant,
                Classe = new
                {
                    TotalJours = totalClasse,
                    Presences = presentsClasse,
                    TauxPresence = Math.Round(tauxClasse, 2)
                }
            };
        }

        // Rapports détaillés
        public async Task<object> GetRapportCompletEnfantAsync(string contactParent, DateTime dateDebut, DateTime dateFin)
        {
            var presences = await GetSuiviEnfantByParentAndDateRangeAsync(contactParent, dateDebut, dateFin);

            if (!presences.Any()) return new { Message = "Aucune donnée pour cette période" };

            var enfant = presences.First();
            var stats = await GetMoyennePresenceEnfantAsync(contactParent, dateDebut, dateFin);
            var moyennesArrivee = await GetMoyenneHeuresArriveeEnfantAsync(contactParent, dateDebut, dateFin);
            var moyennesDepart = await GetMoyenneHeuresDepartEnfantAsync(contactParent, dateDebut, dateFin);
            var retards = await GetAlertesRetardEnfantAsync(contactParent, dateDebut, dateFin);
            var absences = await GetAlertesAbsenceEnfantAsync(contactParent, dateDebut, dateFin);

            return new
            {
                InformationsEnfant = new
                {
                    Nom = enfant.NomCompletFormate,
                    Classe = enfant.NomClasse,
                    Ecole = enfant.NomEcole
                },
                Periode = new { Debut = dateDebut, Fin = dateFin },
                Statistiques = stats,
                MoyennesHeures = new
                {
                    Arrivee = moyennesArrivee,
                    Depart = moyennesDepart
                },
                Alertes = new
                {
                    Retards = retards,
                    Absences = absences
                },
                Details = presences.Select(p => new
                {
                    Date = p.DateDuJour?.ToString("dd/MM/yyyy"),
                    Statut = p.StatutPresence,
                    HeureArrivee = p.HeureArrivee?.ToString(@"hh\:mm"),
                    HeureDepart = p.HeureDepart?.ToString(@"hh\:mm")
                })
            };
        }

        public async Task<object> GetRapportMensuelEnfantAsync(string contactParent, int mois, int annee)
        {
            return await GetStatistiquesMensuellesEnfantAsync(contactParent, mois, annee);
        }

        public async Task<object> GetRapportTrimestrielEnfantAsync(string contactParent, int trimestre, int annee)
        {
            return await GetStatistiquesTrimestriellesEnfantAsync(contactParent, trimestre, annee);
        }

        // Méthodes utilitaires privées
        private DateTime GetDateDebutSemaine(int semaine, int annee)
        {
            var date = new DateTime(annee, 1, 1);
            date = date.AddDays((semaine - 1) * 7);
            return date.AddDays(-(int)date.DayOfWeek);
        }

        private (DateTime debut, DateTime fin) GetDatesTrimestre(int trimestre, int annee)
        {
            var debut = new DateTime(annee, (trimestre - 1) * 3 + 1, 1);
            var fin = debut.AddMonths(3).AddDays(-1);
            return (debut, fin);
        }

        // Vues : appliquer Migrations/AddReportingViews (dotnet ef database update).
    }
}

