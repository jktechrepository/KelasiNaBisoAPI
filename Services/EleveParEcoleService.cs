using KelasiNaBiso.Data;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class EleveParEcoleService : IEleveParEcoleRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly EleveAnneeScopeHelper _scope;

        public EleveParEcoleService(KelasiNaBisoDbContext context, EleveAnneeScopeHelper scope)
        {
            _context = context;
            _scope = scope;
        }

        private async Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> QueryEcoleScopedAsync(
            int idEcole,
            int? idAnneeScolaire,
            Func<IQueryable<EleveParEcoleDTO>, IQueryable<EleveParEcoleDTO>>? filter = null)
        {
            var (ecole, annee) = await _scope.ResolveEcoleAnneeAsync(idEcole, idAnneeScolaire);
            var ids = _scope.GetEleveIdsInEcoleAnnee(ecole, annee);
            var query = _context.EleveParEcole.AsNoTracking().Where(e => ids.Contains(e.IdEleve));
            if (filter != null)
                query = filter(query);

            var data = await query.OrderBy(e => e.NomCompletEleve).ToListAsync();
            return EleveAnneeScopeHelper.Wrap<IEnumerable<EleveParEcoleDTO>>(data, ecole, annee);
        }

        private async Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> QueryClasseScopedAsync(
            int idClasse,
            int? idAnneeScolaire,
            Func<IQueryable<EleveParEcoleDTO>, IQueryable<EleveParEcoleDTO>>? filter = null)
        {
            var (ecole, annee) = await _scope.ResolveClasseAnneeAsync(idClasse, idAnneeScolaire);
            var ids = _scope.GetEleveIdsInClasseAnnee(idClasse, annee);
            var query = _context.EleveParEcole.AsNoTracking().Where(e => ids.Contains(e.IdEleve));
            if (filter != null)
                query = filter(query);

            var data = await query.OrderBy(e => e.NomCompletEleve).ToListAsync();
            return EleveAnneeScopeHelper.Wrap<IEnumerable<EleveParEcoleDTO>>(data, ecole, annee);
        }

        private async Task<ElevesAnneeScopedResult<int>> CountEcoleScopedAsync(
            int idEcole,
            int? idAnneeScolaire,
            Func<IQueryable<EleveParEcoleDTO>, IQueryable<EleveParEcoleDTO>>? filter = null)
        {
            var result = await QueryEcoleScopedAsync(idEcole, idAnneeScolaire, filter);
            return EleveAnneeScopeHelper.Wrap(result.Data.Count(), result.IdEcole, result.IdAnneeScolaire);
        }

        private async Task<ElevesAnneeScopedResult<int>> CountClasseScopedAsync(
            int idClasse,
            int? idAnneeScolaire,
            Func<IQueryable<EleveParEcoleDTO>, IQueryable<EleveParEcoleDTO>>? filter = null)
        {
            var result = await QueryClasseScopedAsync(idClasse, idAnneeScolaire, filter);
            return EleveAnneeScopeHelper.Wrap(result.Data.Count(), result.IdEcole, result.IdAnneeScolaire);
        }

        public Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetAllAsync(int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire);

        public async Task<EleveParEcoleDTO?> GetByIdAsync(int idEleve) =>
            await _context.EleveParEcole.AsNoTracking().FirstOrDefaultAsync(e => e.IdEleve == idEleve);

        public Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByEcoleAsync(int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(e => e.IdEcole == idEcole));

        public Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByClasseAsync(int idClasse, int? idAnneeScolaire = null) =>
            QueryClasseScopedAsync(idClasse, idAnneeScolaire, q => q.Where(e => e.IdClasse == idClasse));

        public async Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByDirectionAsync(
            int idDirection, int? idAnneeScolaire = null)
        {
            var idEcole = await _scope.ResolveIdEcoleForDirectionAsync(idDirection);
            return await QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(e => e.IdDirection == idDirection));
        }

        public async Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByOptionAsync(
            int idOption, int? idAnneeScolaire = null)
        {
            var idEcole = await _scope.ResolveIdEcoleForOptionAsync(idOption);
            return await QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(e => e.IdOption == idOption));
        }

        public Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByTuteurAsync(
            int idTuteur, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(e => e.IdTuteur == idTuteur));

        public Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByStatutAsync(
            bool statut, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(e => e.Statut == statut));

        public Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByGenreAsync(
            string genre, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(e => e.Genre == genre));

        public Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByAgeRangeAsync(
            int minAge, int maxAge, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(e => e.Age >= minAge && e.Age <= maxAge));

        public Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByProvinceAsync(
            string province, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(e => e.ProvinceEleve == province));

        public Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByVilleAsync(
            string ville, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(e => e.VilleEleve == ville));

        public Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByCommuneAsync(
            string commune, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(e => e.CommuneEleve == commune));

        public Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> GetByTuteurContactAsync(
            string contact, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q =>
                q.Where(e => e.TelephoneTuteur == contact
                    || e.EmailTuteur == contact
                    || e.TelephoneRepresentant == contact));

        public Task<ElevesAnneeScopedResult<IEnumerable<EleveParEcoleDTO>>> SearchAsync(
            string searchTerm, int idEcole, int? idAnneeScolaire = null) =>
            QueryEcoleScopedAsync(idEcole, idAnneeScolaire, q =>
                q.Where(e =>
                    (e.NomCompletEleve != null && e.NomCompletEleve.Contains(searchTerm))
                    || (e.Matricule != null && e.Matricule.Contains(searchTerm))
                    || (e.NomEcole != null && e.NomEcole.Contains(searchTerm))
                    || (e.NomClasse != null && e.NomClasse.Contains(searchTerm))));

        public Task<ElevesAnneeScopedResult<int>> GetCountByEcoleAsync(int idEcole, int? idAnneeScolaire = null) =>
            CountEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(e => e.IdEcole == idEcole));

        public Task<ElevesAnneeScopedResult<int>> GetCountByClasseAsync(int idClasse, int? idAnneeScolaire = null) =>
            CountClasseScopedAsync(idClasse, idAnneeScolaire, q => q.Where(e => e.IdClasse == idClasse));

        public async Task<ElevesAnneeScopedResult<int>> GetCountByDirectionAsync(int idDirection, int? idAnneeScolaire = null)
        {
            var idEcole = await _scope.ResolveIdEcoleForDirectionAsync(idDirection);
            return await CountEcoleScopedAsync(idEcole, idAnneeScolaire, q => q.Where(e => e.IdDirection == idDirection));
        }
    }
}
