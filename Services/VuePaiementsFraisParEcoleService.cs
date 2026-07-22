using KelasiNaBiso.Data;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class VuePaiementsFraisParEcoleService : IVuePaiementsFraisParEcoleRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public VuePaiementsFraisParEcoleService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetAllAsync()
        {
            return await _context.VuePaiementsFraisParEcole
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<VuePaiementsFraisParEcoleDTO?> GetByIdAsync(int idPaiement)
        {
            return await _context.VuePaiementsFraisParEcole
                .FirstOrDefaultAsync(p => p.IdPaiement == idPaiement);
        }

        // Filtres par école
        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByEcoleAsync(int idEcole)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.IdEcole == idEcole)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByEcoleNameAsync(string nomEcole)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.NomEcole.Contains(nomEcole))
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByTypeEcoleAsync(string typeEcole)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.TypeEcole == typeEcole)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        // Filtres par élève
        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByEleveAsync(int idEleve)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.IdEleve == idEleve)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByEleveReferenceAsync(Guid referenceEleve)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.ReferenceEleve == referenceEleve)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByEleveMatriculeAsync(string matricule)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.Matricule.Contains(matricule))
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByEleveNameAsync(string nomEleve)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.NomCompletFormate.Contains(nomEleve) || p.NomCompletOriginal.Contains(nomEleve))
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByEleveGenreAsync(string genre)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.Genre == genre)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByEleveStatutAsync(bool statut)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.StatutEleve == statut)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        // Filtres par classe
        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByClasseAsync(int idClasse)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.IdClasse == idClasse)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByClasseNameAsync(string nomClasse)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.NomClasse.Contains(nomClasse))
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        // Filtres par section
        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetBySectionAsync(int idSection)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.IdSection == idSection)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetBySectionNameAsync(string nomSection)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.NomSection.Contains(nomSection))
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        // Filtres par direction
        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByDirectionAsync(int idDirection)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.IdDirection == idDirection)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByDirectionNameAsync(string nomDirection)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.NomDirection.Contains(nomDirection))
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        // Filtres par option
        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByOptionAsync(int idOption)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.IdOption == idOption)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByOptionNameAsync(string nomOption)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.NomOption.Contains(nomOption))
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        // Filtres par tuteur
        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByTuteurAsync(int idTuteur)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.IdTuteur == idTuteur)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByTuteurNameAsync(string nomTuteur)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.NomTuteur.Contains(nomTuteur))
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByTuteurContactAsync(string contact)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.TelephoneTuteur == contact || p.EmailTuteur == contact || p.TelephoneRepresentant == contact)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByTuteurStatutAsync(bool statut)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.StatutTuteur == statut)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        // Filtres par frais
        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByFraisAsync(int idFrais)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.IdFrais == idFrais)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByFraisLibelleAsync(string libelleFrais)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.LibelleFrais.Contains(libelleFrais))
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByFraisMontantRangeAsync(double minMontant, double maxMontant)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.MontantFrais >= minMontant && p.MontantFrais <= maxMontant)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        // Filtres par paiement
        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByPaiementStatutAsync(string statut)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.StatutPaiement == statut)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByModePaiementAsync(string modePaiement)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.ModePaiement == modePaiement)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByDeviseAsync(string devise)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.Devise == devise)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByMontantRangeAsync(double minMontant, double maxMontant)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.Montant >= minMontant && p.Montant <= maxMontant)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByDatePaiementRangeAsync(DateTime dateDebut, DateTime dateFin)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.DatePaiement >= dateDebut && p.DatePaiement <= dateFin)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByDatePaiementAsync(DateTime datePaiement)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.DatePaiement.Value.Date == datePaiement.Date)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByReferenceTransactionAsync(string referenceTransaction)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.ReferenceTransaction.Contains(referenceTransaction))
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        // Filtres par localisation
        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByProvinceAsync(string province)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.ProvinceEleve == province || p.ProvinceEcole == province)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByVilleAsync(string ville)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.VilleEleve == ville || p.VilleEcole == ville)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> GetByCommuneAsync(string commune)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.CommuneEleve == commune || p.CommuneEcole == commune)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        // Recherche générale
        public async Task<IEnumerable<VuePaiementsFraisParEcoleDTO>> SearchAsync(string searchTerm)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.NomCompletFormate.Contains(searchTerm) ||
                           p.Matricule.Contains(searchTerm) ||
                           p.NomEcole.Contains(searchTerm) ||
                           p.NomClasse.Contains(searchTerm) ||
                           p.NomTuteur.Contains(searchTerm) ||
                           p.LibelleFrais.Contains(searchTerm) ||
                           p.ReferenceTransaction.Contains(searchTerm))
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        // Comptages
        public async Task<int> GetCountByEcoleAsync(int idEcole)
        {
            return await _context.VuePaiementsFraisParEcole
                .CountAsync(p => p.IdEcole == idEcole);
        }

        public async Task<int> GetCountByClasseAsync(int idClasse)
        {
            return await _context.VuePaiementsFraisParEcole
                .CountAsync(p => p.IdClasse == idClasse);
        }

        public async Task<int> GetCountBySectionAsync(int idSection)
        {
            return await _context.VuePaiementsFraisParEcole
                .CountAsync(p => p.IdSection == idSection);
        }

        public async Task<int> GetCountByDirectionAsync(int idDirection)
        {
            return await _context.VuePaiementsFraisParEcole
                .CountAsync(p => p.IdDirection == idDirection);
        }

        public async Task<int> GetCountByTuteurAsync(int idTuteur)
        {
            return await _context.VuePaiementsFraisParEcole
                .CountAsync(p => p.IdTuteur == idTuteur);
        }

        public async Task<int> GetCountByFraisAsync(int idFrais)
        {
            return await _context.VuePaiementsFraisParEcole
                .CountAsync(p => p.IdFrais == idFrais);
        }

        public async Task<int> GetCountByPaiementStatutAsync(string statut)
        {
            return await _context.VuePaiementsFraisParEcole
                .CountAsync(p => p.StatutPaiement == statut);
        }

        public async Task<int> GetCountByModePaiementAsync(string modePaiement)
        {
            return await _context.VuePaiementsFraisParEcole
                .CountAsync(p => p.ModePaiement == modePaiement);
        }

        public async Task<int> GetCountByDatePaiementRangeAsync(DateTime dateDebut, DateTime dateFin)
        {
            return await _context.VuePaiementsFraisParEcole
                .CountAsync(p => p.DatePaiement >= dateDebut && p.DatePaiement <= dateFin);
        }

        // Statistiques
        public async Task<double> GetTotalMontantByEcoleAsync(int idEcole)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.IdEcole == idEcole && p.Montant.HasValue)
                .SumAsync(p => p.Montant.Value);
        }

        public async Task<double> GetTotalMontantByDateRangeAsync(DateTime dateDebut, DateTime dateFin)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.DatePaiement >= dateDebut && p.DatePaiement <= dateFin && p.Montant.HasValue)
                .SumAsync(p => p.Montant.Value);
        }

        public async Task<double> GetTotalMontantByPaiementStatutAsync(string statut)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.StatutPaiement == statut && p.Montant.HasValue)
                .SumAsync(p => p.Montant.Value);
        }

        public async Task<double> GetTotalMontantByModePaiementAsync(string modePaiement)
        {
            return await _context.VuePaiementsFraisParEcole
                .Where(p => p.ModePaiement == modePaiement && p.Montant.HasValue)
                .SumAsync(p => p.Montant.Value);
        }
    }
}



