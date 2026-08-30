using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class V_EleveService : IV_EleveRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly EleveAnneeScopeHelper _scope;

        public V_EleveService(KelasiNaBisoDbContext context, EleveAnneeScopeHelper scope)
        {
            _context = context;
            _scope = scope;
        }

        private async Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> QueryEcoleScopedAsync(
            int idEcole,
            int? idAnneeScolaire,
            Func<IQueryable<V_Eleve>, IQueryable<V_Eleve>>? filter = null)
        {
            var (ecole, annee) = await _scope.ResolveEcoleAnneeAsync(idEcole, idAnneeScolaire);
            var ids = _scope.GetEleveIdsInEcoleAnnee(ecole, annee);
            var query = _context.V_Eleves.AsNoTracking().Where(v => ids.Contains(v.IdEleve));
            if (filter != null)
                query = filter(query);

            var data = await query.OrderByDescending(v => v.DateCreation).ToListAsync();
            return EleveAnneeScopeHelper.Wrap<IEnumerable<V_Eleve>>(data, ecole, annee);
        }

        private async Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> QueryClasseScopedAsync(
            int idClasse,
            int? idAnneeScolaire,
            Func<IQueryable<V_Eleve>, IQueryable<V_Eleve>>? filter = null)
        {
            var (ecole, annee) = await _scope.ResolveClasseAnneeAsync(idClasse, idAnneeScolaire);
            var ids = _scope.GetEleveIdsInClasseAnnee(idClasse, annee);
            var query = _context.V_Eleves.AsNoTracking().Where(v => ids.Contains(v.IdEleve));
            if (filter != null)
                query = filter(query);

            var data = await query.OrderByDescending(v => v.DateCreation).ToListAsync();
            return EleveAnneeScopeHelper.Wrap<IEnumerable<V_Eleve>>(data, ecole, annee);
        }

        private async Task<ElevesAnneeScopedResult<int>> CountEcoleScopedAsync(
            int idEcole,
            int? idAnneeScolaire,
            Func<IQueryable<V_Eleve>, IQueryable<V_Eleve>>? filter = null)
        {
            var result = await QueryEcoleScopedAsync(idEcole, idAnneeScolaire, filter);
            var count = result.Data.Count();
            return EleveAnneeScopeHelper.Wrap(count, result.IdEcole, result.IdAnneeScolaire);
        }

        private async Task<ElevesAnneeScopedResult<int>> CountClasseScopedAsync(
            int idClasse,
            int? idAnneeScolaire,
            Func<IQueryable<V_Eleve>, IQueryable<V_Eleve>>? filter = null)
        {
            var result = await QueryClasseScopedAsync(idClasse, idAnneeScolaire, filter);
            var count = result.Data.Count();
            return EleveAnneeScopeHelper.Wrap(count, result.IdEcole, result.IdAnneeScolaire);
        }

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetAllAsync(int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire);

        public async Task<V_Eleve?> GetByIdAsync(int id) =>
            await _context.V_Eleves.AsNoTracking().FirstOrDefaultAsync(v => v.IdEleve == id);

        public async Task<V_Eleve?> GetByReferenceAsync(Guid reference) =>
            await _context.V_Eleves.AsNoTracking().FirstOrDefaultAsync(v => v.ReferenceEleve == reference);

        public async Task<V_Eleve?> GetByMatriculeAsync(string matricule) =>
            await _context.V_Eleves.AsNoTracking().FirstOrDefaultAsync(v => v.Matricule == matricule);

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByEcoleAsync(int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.IdEcole == idEcole));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByClasseAsync(int idClasse, int? idAnneeScolaire = null) =>
            QueryClasseScopedAsync(idClasse, idAnneeScolaire, q => q.Where(v => v.IdClasse == idClasse));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByTuteurAsync(
            int idTuteur, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.IdTuteur == idTuteur));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByStatutAsync(
            bool statut, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.Statut == statut));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByGenreAsync(
            string genre, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.Genre == genre));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByNationaliteAsync(
            string nationalite, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.Nationalite == nationalite));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByProvinceAsync(
            string province, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.Province == province));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByVilleAsync(
            string ville, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.Ville == ville));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByCommuneAsync(
            string commune, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.Commune == commune));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByDateCreationAsync(
            DateTime date, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.DateCreation.Date == date.Date));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByDateCreationRangeAsync(
            DateTime dateDebut, DateTime dateFin, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire,
                q => q.Where(v => v.DateCreation >= dateDebut && v.DateCreation <= dateFin));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByDateNaissanceAsync(
            DateTime dateNaissance, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire,
                q => q.Where(v => v.DateNaissance.Date == dateNaissance.Date));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByDateNaissanceRangeAsync(
            DateTime dateDebut, DateTime dateFin, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire,
                q => q.Where(v => v.DateNaissance >= dateDebut && v.DateNaissance <= dateFin));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByNomCompletAsync(
            string nomComplet, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire,
                q => q.Where(v => v.NomComplet != null && v.NomComplet.Contains(nomComplet)));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByNomAsync(
            string nom, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire,
                q => q.Where(v => v.Nom != null && v.Nom.Contains(nom)));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByPrenomAsync(
            string prenom, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire,
                q => q.Where(v => v.Prenom != null && v.Prenom.Contains(prenom)));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByLieuNaissanceAsync(
            string lieuNaissance, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire,
                q => q.Where(v => v.LieuNaissance != null && v.LieuNaissance.Contains(lieuNaissance)));

        public async Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetBySectionAsync(
            int idSection, int? idAnneeScolaire = null)
        {
            var idEcole = await _scope.ResolveIdEcoleForSectionAsync(idSection);
            return await QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.IdSection == idSection));
        }

        public async Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByOptionAsync(
            int idOption, int? idAnneeScolaire = null)
        {
            var idEcole = await _scope.ResolveIdEcoleForOptionAsync(idOption);
            return await QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.IdOption == idOption));
        }

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByStatutTuteurAsync(
            bool statutTuteur, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.StatutTuteur == statutTuteur));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByEmailTuteurAsync(
            string emailTuteur, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.EmailTuteur == emailTuteur));

        public Task<ElevesAnneeScopedResult<IEnumerable<V_Eleve>>> GetByTelephoneTuteurAsync(
            string telephoneTuteur, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.TelephoneTuteur == telephoneTuteur));

        public Task<bool> ExistsAsync(int id) =>
            _context.V_Eleves.AnyAsync(v => v.IdEleve == id);

        public Task<bool> ExistsByReferenceAsync(Guid reference) =>
            _context.V_Eleves.AnyAsync(v => v.ReferenceEleve == reference);

        public Task<bool> ExistsByMatriculeAsync(string matricule) =>
            _context.V_Eleves.AnyAsync(v => v.Matricule == matricule);

        public Task<ElevesAnneeScopedResult<int>> GetCountAsync(int idEcole, int? idAnneeScolaire = null) =>
            CountEcoleScopedAsync(idEcole, idAnneeScolaire);

        public Task<ElevesAnneeScopedResult<int>> GetCountByEcoleAsync(int idEcole, int? idAnneeScolaire = null) =>
            CountEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.IdEcole == idEcole));

        public Task<ElevesAnneeScopedResult<int>> GetCountByClasseAsync(int idClasse, int? idAnneeScolaire = null) =>
            CountClasseScopedAsync(idClasse, idAnneeScolaire, q => q.Where(v => v.IdClasse == idClasse));

        public Task<ElevesAnneeScopedResult<int>> GetCountByTuteurAsync(
            int idTuteur, int idEcole, int? idAnneeScolaire = null) =>
            CountEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.IdTuteur == idTuteur));

        public Task<ElevesAnneeScopedResult<int>> GetCountByStatutAsync(
            bool statut, int idEcole, int? idAnneeScolaire = null) =>
            CountEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.Statut == statut));

        public Task<ElevesAnneeScopedResult<int>> GetCountByGenreAsync(
            string genre, int idEcole, int? idAnneeScolaire = null) =>
            CountEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(v => v.Genre == genre));
    }
}
