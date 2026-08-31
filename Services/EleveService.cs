using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KelasiNaBiso.Services
{
    public class EleveService : IEleveRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IInscriptionRepository _inscriptionRepository;
        private readonly IInscriptionActiveResolver _inscriptionResolver;
        private readonly EleveAnneeScopeHelper _scope;
        private readonly ILogger<EleveService> _logger;

        public EleveService(
            KelasiNaBisoDbContext context,
            IInscriptionRepository inscriptionRepository,
            IInscriptionActiveResolver inscriptionResolver,
            EleveAnneeScopeHelper scope,
            ILogger<EleveService> logger)
        {
            _context = context;
            _inscriptionRepository = inscriptionRepository;
            _inscriptionResolver = inscriptionResolver;
            _scope = scope;
            _logger = logger;
        }

        private static ElevesAnneeScopedResult<T> Scoped<T>(T data, int idEcole, int idAnneeScolaire) =>
            EleveAnneeScopeHelper.Wrap(data, idEcole, idAnneeScolaire);

        private async Task<List<EleveParEcoleListItemDto>> MapElevesParEcoleAsync(
            IQueryable<Eleve> elevesQuery,
            int idEcole,
            int idAnneeScolaire)
        {
            var eleves = await elevesQuery
                .Include(e => e.Tuteur)
                .OrderBy(e => e.NomComplet)
                .ToListAsync();

            if (eleves.Count == 0)
                return new List<EleveParEcoleListItemDto>();

            var eleveIds = eleves.Select(e => e.IdEleve).ToList();
            var inscriptions = await _context.Inscriptions
                .AsNoTracking()
                .Include(i => i.Classe)
                .Include(i => i.AnneeScolaire)
                .Where(i => eleveIds.Contains(i.IdEleve)
                    && i.IdEcole == idEcole
                    && i.IdAnneeScolaire == idAnneeScolaire
                    && i.Statut == true
                    && i.StatutInscription != null
                    && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                        || i.StatutInscription == "Confirme"
                        || i.StatutInscription.StartsWith("Confirm")))
                .ToListAsync();

            var inscriptionByEleve = inscriptions
                .GroupBy(i => i.IdEleve)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(i => i.DateInscription).First());

            return eleves.Select(e =>
            {
                inscriptionByEleve.TryGetValue(e.IdEleve, out var ins);
                return new EleveParEcoleListItemDto
                {
                    IdEleve = e.IdEleve,
                    ReferenceEleve = e.ReferenceEleve,
                    Matricule = e.Matricule,
                    Nom = e.Nom,
                    Postnom = e.Postnom,
                    Prenom = e.Prenom,
                    NomComplet = e.NomComplet,
                    Genre = e.Genre,
                    DateNaissance = e.DateNaissance,
                    PhotoUrl = e.PhotoUrl,
                    Statut = e.Statut,
                    IdTuteur = e.IdTuteur,
                    NomCompletTuteur = e.Tuteur?.NomComplet,
                    TelephoneTuteur = e.Tuteur?.Telephone,
                    IdInscription = ins?.IdInscription,
                    IdClasse = ins?.IdClasse,
                    NomClasse = ins?.Classe?.NomClasse,
                    IdAnneeScolaire = ins?.IdAnneeScolaire,
                    LibelleAnneeScolaire = ins?.AnneeScolaire?.LibelleAnneeScolaire,
                    IdEcole = ins?.IdEcole ?? idEcole
                };
            }).ToList();
        }

        private async Task ValidatePagedListFiltersAsync(
            int idEcole,
            int? idClasse,
            int? idDirection)
        {
            if (idClasse.HasValue && idClasse.Value > 0)
            {
                var ecoleClasse = await _scope.ResolveIdEcoleForClasseAsync(idClasse.Value);
                if (ecoleClasse != idEcole)
                {
                    throw new InvalidOperationException(
                        $"La classe {idClasse.Value} n'appartient pas à l'école {idEcole}.");
                }

                if (idDirection.HasValue && idDirection.Value > 0)
                {
                    var classeDirection = await _context.Classes
                        .AsNoTracking()
                        .Where(c => c.IdClasse == idClasse.Value)
                        .Select(c => c.IdDirection)
                        .FirstOrDefaultAsync();

                    if (classeDirection != idDirection.Value)
                    {
                        throw new InvalidOperationException(
                            $"La classe {idClasse.Value} n'appartient pas à la direction {idDirection.Value}.");
                    }
                }
            }
            else if (idDirection.HasValue && idDirection.Value > 0)
            {
                var ecoleDirection = await _scope.ResolveIdEcoleForDirectionAsync(idDirection.Value);
                if (ecoleDirection != idEcole)
                {
                    throw new InvalidOperationException(
                        $"La direction {idDirection.Value} n'appartient pas à l'école {idEcole}.");
                }
            }
        }

        private IQueryable<int> GetEleveIdsForPagedList(
            int idEcole,
            int idAnneeScolaire,
            int? idClasse,
            int? idDirection)
        {
            if (idClasse.HasValue && idClasse.Value > 0)
            {
                return _scope.GetEleveIdsInClasseAnnee(idClasse.Value, idAnneeScolaire);
            }

            if (idDirection.HasValue && idDirection.Value > 0)
            {
                return _scope.GetEleveIdsInDirectionAnnee(idDirection.Value, idAnneeScolaire);
            }

            return _inscriptionResolver
                .FilterElevesInEcole(_context.Eleves, idEcole, idAnneeScolaire)
                .Select(e => e.IdEleve);
        }

        private IQueryable<V_Eleve> BuildVElevesQueryForEcoleAnnee(
            int idEcole,
            int idAnneeScolaire,
            bool includeInactive,
            string? searchTerm,
            int? idClasse = null,
            int? idDirection = null)
        {
            var elevesIds = GetEleveIdsForPagedList(idEcole, idAnneeScolaire, idClasse, idDirection);

            var query = _context.V_Eleves.Where(v => elevesIds.Contains(v.IdEleve));

            if (!includeInactive)
                query = query.Where(e => e.Statut == true);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(e =>
                    (e.NomComplet != null && e.NomComplet.ToLower().Contains(searchLower)) ||
                    (e.Matricule != null && e.Matricule.ToLower().Contains(searchLower)));
            }

            return query;
        }

        // ✅ NOUVELLES MÉTHODES PAGINÉES
        public async Task<ElevesAnneeScopedResult<PagedResult<V_Eleve>>> GetAllPagedAsync(
            int idEcole, PagedRequest request, int? idAnneeScolaire = null, int? idClasse = null, int? idDirection = null)
        {
            var resolvedAnnee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, idAnneeScolaire);
            await ValidatePagedListFiltersAsync(idEcole, idClasse, idDirection);
            var query = BuildVElevesQueryForEcoleAnnee(
                idEcole, resolvedAnnee, request.IncludeInactive, request.SearchTerm, idClasse, idDirection);

            if (!string.IsNullOrWhiteSpace(request.SortBy))
                query = query.ApplySort(request.SortBy, request.SortDescending);
            else
            {
                query = request.SortDescending
                    ? query.OrderByDescending(e => e.NomComplet)
                    : query.OrderBy(e => e.NomComplet);
            }

            var page = await query.ToPagedAsync(request);
            return Scoped(page, idEcole, resolvedAnnee);
        }

        public async Task<ElevesAnneeScopedResult<CursorPaginatedResult<V_Eleve>>> GetAllCursorPagedAsync(
            int idEcole, CursorPaginationRequest request, int? idAnneeScolaire = null, int? idClasse = null, int? idDirection = null)
        {
            var resolvedAnnee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, idAnneeScolaire);
            await ValidatePagedListFiltersAsync(idEcole, idClasse, idDirection);
            var query = BuildVElevesQueryForEcoleAnnee(
                idEcole, resolvedAnnee, request.IncludeInactive, request.SearchTerm, idClasse, idDirection);

            var page = await query.ToCursorPagedAsync(request, e => e.IdEleve);
            return Scoped(page, idEcole, resolvedAnnee);
        }

        public async Task<ElevesAnneeScopedResult<PagedResult<Eleve>>> GetByClassePagedAsync(
            int idClasse, PagedRequest request, int? idAnneeScolaire = null)
        {
            var idEcole = await _scope.ResolveIdEcoleForClasseAsync(idClasse);
            var resolvedAnnee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, idAnneeScolaire);

            var query = _inscriptionResolver.FilterElevesInClasse(
                _context.Eleves.Include(e => e.Tuteur),
                idClasse,
                resolvedAnnee);

            if (!request.IncludeInactive)
                query = query.Where(e => e.Statut == true);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(e =>
                    (e.NomComplet != null && e.NomComplet.ToLower().Contains(searchLower)) ||
                    (e.Matricule != null && e.Matricule.ToLower().Contains(searchLower)));
            }

            var page = await query.ToPagedAsync(request, e => e.NomComplet);
            return Scoped(page, idEcole, resolvedAnnee);
        }

        public async Task<ElevesAnneeScopedResult<PagedResult<EleveParEcoleListItemDto>>> GetByEcoleByNomCompletPagedAsync(
            int idEcole, string nomComplet, PagedRequest request, int? idAnneeScolaire = null)
        {
            var resolvedAnnee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, idAnneeScolaire);
            var query = _inscriptionResolver.FilterElevesInEcole(
                _context.Eleves.Include(e => e.Tuteur),
                idEcole,
                resolvedAnnee);

            if (!string.IsNullOrWhiteSpace(nomComplet))
            {
                var nomCompletLower = nomComplet.ToLower();
                query = query.Where(e =>
                    e.NomComplet != null && e.NomComplet.ToLower().Contains(nomCompletLower));
            }

            if (!request.IncludeInactive)
                query = query.Where(e => e.Statut == true);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(e =>
                    (e.NomComplet != null && e.NomComplet.ToLower().Contains(searchLower)) ||
                    (e.Matricule != null && e.Matricule.ToLower().Contains(searchLower)));
            }

            var all = await MapElevesParEcoleAsync(query, idEcole, resolvedAnnee);
            var total = all.Count;
            var pageNumber = Math.Max(1, request.PageNumber);
            var size = Math.Clamp(request.PageSize, 1, 100);
            var items = all.Skip((pageNumber - 1) * size).Take(size).ToList();

            var page = new PagedResult<EleveParEcoleListItemDto>
            {
                Data = items,
                PageNumber = pageNumber,
                PageSize = size,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)size)
            };
            return Scoped(page, idEcole, resolvedAnnee);
        }

        public async Task<PagedResult<Eleve>> GetByTuteurPagedAsync(int idTuteur, PagedRequest request)
        {
            var query = _context.Eleves
                .Include(e => e.Tuteur)
                .Where(e => e.IdTuteur == idTuteur);

            if (!request.IncludeInactive)
                query = query.Where(e => e.Statut == true);

            return await query.ToPagedAsync(request, e => e.NomComplet);
        }

        public async Task<ElevesAnneeScopedResult<PagedResult<EleveParEcoleListItemDto>>> GetByEcolePagedAsync(
            int idEcole, PagedRequest request, int? idAnneeScolaire = null)
        {
            var resolvedAnnee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, idAnneeScolaire);
            var query = _inscriptionResolver.FilterElevesInEcole(
                _context.Eleves.Include(e => e.Tuteur),
                idEcole,
                resolvedAnnee);

            if (!request.IncludeInactive)
                query = query.Where(e => e.Statut == true);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(e =>
                    (e.NomComplet != null && e.NomComplet.ToLower().Contains(searchLower)) ||
                    (e.Matricule != null && e.Matricule.ToLower().Contains(searchLower)));
            }

            var all = await MapElevesParEcoleAsync(query, idEcole, resolvedAnnee);
            var total = all.Count;
            var pageNumber = Math.Max(1, request.PageNumber);
            var size = Math.Clamp(request.PageSize, 1, 100);
            var items = all.Skip((pageNumber - 1) * size).Take(size).ToList();

            var page = new PagedResult<EleveParEcoleListItemDto>
            {
                Data = items,
                PageNumber = pageNumber,
                PageSize = size,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)size)
            };
            return Scoped(page, idEcole, resolvedAnnee);
        }

        // ⚠️ DEPRECATED: Méthodes de base CRUD (conserver pour rétrocompatibilité)
        public async Task<ElevesAnneeScopedResult<IReadOnlyList<EleveParEcoleListItemDto>>> GetAllAsync(
            int idEcole, int? idAnneeScolaire = null)
        {
            return await GetByEcoleAsync(idEcole, idAnneeScolaire);
        }

        public async Task<Eleve> GetByIdAsync(int id)
        {
            return await _context.Eleves
                .Include(e => e.Tuteur)
                .Where(e => e.Statut == true)
                .FirstOrDefaultAsync(e => e.IdEleve == id);
        }

        public async Task<Eleve> GetByReferenceAsync(Guid reference)
        {
            return await _context.Eleves
                .Include(e => e.Tuteur)
                .Where(e => e.Statut == true)
                .FirstOrDefaultAsync(e => e.ReferenceEleve == reference);
        }

        public async Task<Eleve> CreateAsync(Eleve eleve)
        {
            // ✅ UNICITÉ SERIAL NUMBER ÉLÈVE: Vérifier que le SerialNumber n'existe pas déjà
            if (!string.IsNullOrEmpty(eleve.SerialNumber))
            {
                var serialNumberExists = await ExistsBySerialNumberAsync(eleve.SerialNumber);
                if (serialNumberExists)
                {
                    throw new InvalidOperationException(
                        $"Un élève avec le SerialNumber '{eleve.SerialNumber}' existe déjà. " +
                        $"Chaque SerialNumber doit être unique dans le système. " +
                        $"Cet appareil est peut-être déjà lié à un autre élève."
                    );
                }
            }

            eleve.ReferenceEleve = Guid.NewGuid();
            eleve.DateCreation = DateTime.Now;
            eleve.NomComplet = $"{eleve.Nom} {eleve.Postnom} {eleve.Prenom}";
            
            _context.Eleves.Add(eleve);
            await _context.SaveChangesAsync();
            return eleve;
        }

        public async Task<IEnumerable<Eleve>> CreateBatchAsync(IEnumerable<Eleve> eleves)
        {
            var eleveList = eleves.ToList();
            var createdEleves = new List<Eleve>();
            var errors = new List<string>();

            Console.WriteLine($"🔄 Début création batch de {eleveList.Count} élèves");

            foreach (var eleve in eleveList)
            {
                try
                {
                    Console.WriteLine($"   → Traitement élève: {eleve.Nom} {eleve.Prenom}");
                    
                    // ✅ UNICITÉ SERIAL NUMBER ÉLÈVE: Vérifier que le SerialNumber n'existe pas déjà
                    if (!string.IsNullOrEmpty(eleve.SerialNumber))
                    {
                        var serialNumberExists = await ExistsBySerialNumberAsync(eleve.SerialNumber);
                        if (serialNumberExists)
                        {
                            var error = $"Élève {eleve.Nom} {eleve.Prenom}: SerialNumber '{eleve.SerialNumber}' existe déjà";
                            Console.WriteLine($"   ❌ {error}");
                            errors.Add(error);
                            continue;
                        }
                    }

                    eleve.ReferenceEleve = Guid.NewGuid();
                    eleve.DateCreation = DateTime.Now;
                    eleve.NomComplet = $"{eleve.Nom} {eleve.Postnom} {eleve.Prenom}";
                    
                    _context.Eleves.Add(eleve);
                    createdEleves.Add(eleve);
                    Console.WriteLine($"   ✅ Élève {eleve.Nom} {eleve.Prenom} ajouté à la liste");
                }
                catch (Exception ex)
                {
                    var error = $"Élève {eleve.Nom} {eleve.Prenom}: {ex.Message}";
                    Console.WriteLine($"   ❌ ERREUR: {error}");
                    Console.WriteLine($"   Stack: {ex.StackTrace}");
                    errors.Add(error);
                }
            }

            // Sauvegarder tous les élèves valides
            if (createdEleves.Any())
            {
                Console.WriteLine($"💾 Sauvegarde de {createdEleves.Count} élèves...");
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ {createdEleves.Count} élèves sauvegardés");
            }
            else
            {
                Console.WriteLine($"⚠️ Aucun élève à sauvegarder");
            }

            if (errors.Any())
            {
                Console.WriteLine($"⚠️ Erreurs lors de la création par lot ({errors.Count}): {string.Join("; ", errors)}");
            }

            return createdEleves;
        }

        public async Task<Eleve> UpdateAsync(Eleve eleve)
        {
            var existingEleve = await _context.Eleves.FindAsync(eleve.IdEleve);
            if (existingEleve == null)
                return null;

            // Mettre à jour le nom complet
            eleve.NomComplet = $"{eleve.Nom} {eleve.Postnom} {eleve.Prenom}";

            _context.Entry(existingEleve).CurrentValues.SetValues(eleve);
            await _context.SaveChangesAsync();
            return existingEleve;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var eleve = await _context.Eleves.FindAsync(id);
            if (eleve == null)
                return false;

            _context.Eleves.Remove(eleve);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Eleves.AnyAsync(e => e.IdEleve == id);
        }

        public async Task<bool> ExistsByReferenceAsync(Guid reference)
        {
            return await _context.Eleves.AnyAsync(e => e.ReferenceEleve.Equals(reference));
        }

        public async Task<bool> ExistsBySerialNumberAsync(string serialNumber)
        {
            return await _context.Eleves.AnyAsync(e => e.SerialNumber == serialNumber);
        }

        // Méthodes de recherche par critères
        public async Task<ElevesAnneeScopedResult<IReadOnlyList<Eleve>>> GetByClasseAsync(
            int idClasse, int? idAnneeScolaire = null)
        {
            var idEcole = await _scope.ResolveIdEcoleForClasseAsync(idClasse);
            var resolvedAnnee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, idAnneeScolaire);

            var eleves = await _inscriptionResolver
                .FilterElevesInClasse(_context.Eleves.Include(e => e.Tuteur), idClasse, resolvedAnnee)
                .OrderBy(e => e.NomComplet)
                .ToListAsync();

            return Scoped((IReadOnlyList<Eleve>)eleves, idEcole, resolvedAnnee);
        }

        public async Task<IEnumerable<Eleve>> GetByTuteurAsync(int idTuteur)
        {
            return await _context.Eleves
                .Include(e => e.Tuteur)
                .Where(e => e.IdTuteur == idTuteur)
                .Where(e => e.Statut == true)
                .OrderBy(e => e.NomComplet)
                .ToListAsync();
        }

        public async Task<ElevesAnneeScopedResult<IReadOnlyList<EleveParEcoleListItemDto>>> GetByEcoleAsync(
            int idEcole, int? idAnneeScolaire = null)
        {
            var resolvedAnnee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, idAnneeScolaire);
            var query = _inscriptionResolver.FilterElevesInEcole(
                _context.Eleves.Include(e => e.Tuteur),
                idEcole,
                resolvedAnnee);
            var eleves = await MapElevesParEcoleAsync(query, idEcole, resolvedAnnee);
            return Scoped((IReadOnlyList<EleveParEcoleListItemDto>)eleves, idEcole, resolvedAnnee);
        }

        public async Task<IEnumerable<Eleve>> GetByStatutAsync(bool statut)
        {
            return await _context.Eleves
                .Include(e => e.Tuteur)
                .Where(e => e.Statut == statut)
                .OrderBy(e => e.NomComplet)
                .ToListAsync();
        }

        // Méthodes pour récupérer les données associées
        public async Task<IEnumerable<Note>> GetNotesAsync(int idEleve)
        {
            return await _context.Notes
                .Include(n => n.Evaluation)
                .Include(n => n.Professeur)
                .Include(n => n.AnneeScolaire)
                .Where(n => n.IdEleve == idEleve)
                .OrderByDescending(n => n.DateCreation)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inscription>> GetInscriptionsAsync(int idEleve)
        {
            return await _context.Inscriptions
                .Include(i => i.Classe)
                .Include(i => i.Ecole)
                .Include(i => i.AnneeScolaire)
                .Where(i => i.IdEleve == idEleve)
                .OrderByDescending(i => i.DateInscription)
                .ToListAsync();
        }

        public async Task<IEnumerable<Paiement>> GetPaiementsAsync(int idEleve)
        {
            return await _context.Paiements
                .Include(p => p.Utilisateur)
                .Include(p => p.Frais)
                .Where(p => p.IdEleve == idEleve)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
        }

        //public async Task<IEnumerable<Presence>> GetPresencesAsync(int idEleve)
        //{
        //    return await _context.Presences
        //        .Include(p => p.Vacation)
        //        .Where(p => p.IdEleve == idEleve)
        //        .OrderByDescending(p => p.DatePresence)
        //        .ToListAsync();
        //}

        public async Task<IEnumerable<Document>> GetDocumentsAsync(int idEleve)
        {
            return await _context.Documents
                .Include(d => d.Utilisateur)
                .Where(d => d.IdEleve == idEleve)
                .OrderByDescending(d => d.DateCreation)
                .ToListAsync();
        }

        // Méthode utilitaire pour générer une référence unique
        private string GenerateReferenceEleve()
        {
            return $"ELEVE-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        // ✅ SOFT DELETE: Toggle le statut d'un élève (actif <-> inactif)
        /// <summary>
        /// Toggle le statut d'un élève (actif ↔ inactif)
        /// Lors de la désactivation d'un élève, désactive automatiquement toutes ses inscriptions actives (cascade logicielle)
        /// </summary>
        /// <param name="id">ID de l'élève</param>
        /// <returns>True si la modification a réussi, False si l'élève n'existe pas</returns>
        public async Task<bool> ToggleStatutAsync(int id)
        {
            try
            {
                var eleve = await _context.Eleves.FindAsync(id);
                if (eleve == null)
                {
                    _logger.LogWarning($"⚠️ Tentative de toggle statut pour un élève inexistant (ID: {id})");
                    return false;
                }

                var ancienStatut = eleve.Statut;
                eleve.Statut = eleve.Statut != true;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Statut de l'élève ID: {id} modifié de {ancienStatut} à {eleve.Statut}");

                // ✅ CASCADE SOFT DELETE : Si l'élève est désactivé, désactiver automatiquement ses inscriptions actives
                if (ancienStatut == true && eleve.Statut == false)
                {
                    _logger.LogInformation($"🔄 Désactivation automatique des inscriptions de l'élève ID: {id} (cascade logicielle)");
                    var nombreInscriptionsDesactivees = await _inscriptionRepository.DesactiverInscriptionsParEleveAsync(id);
                    _logger.LogInformation($"✅ {nombreInscriptionsDesactivees} inscription(s) désactivée(s) automatiquement pour l'élève ID: {id}");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur lors du toggle statut de l'élève ID: {id}");
                throw;
            }
        }

        // ✅ MISE À JOUR DU SERIAL NUMBER: Récupérer un élève par son matricule
        public async Task<Eleve> GetByMatriculeAsync(string matricule)
        {
            return await _context.Eleves
                .Include(e => e.Tuteur)
                .FirstOrDefaultAsync(e => e.Matricule == matricule);
        }

        public async Task<Eleve> GetBySerialNumberAsync(string serialNumber)
        {
            return await _context.Eleves
                .Include(e => e.Tuteur)
                .Where(e => e.Statut == true)
                .FirstOrDefaultAsync(e => e.SerialNumber == serialNumber);
        }

        // ✅ MISE À JOUR DU SERIAL NUMBER: Par IdEleve
        public async Task<bool> UpdateSerialNumberByIdAsync(int idEleve, string serialNumber)
        {
            var eleve = await _context.Eleves.FindAsync(idEleve);
            if (eleve == null)
                return false;

            // ✅ UNICITÉ SERIAL NUMBER: Vérifier que le nouveau SerialNumber n'est pas déjà utilisé par un autre élève
            if (!string.IsNullOrEmpty(serialNumber) && serialNumber != eleve.SerialNumber)
            {
                var serialNumberExistsByOtherEleve = await _context.Eleves
                    .AnyAsync(e => e.SerialNumber == serialNumber && e.IdEleve != idEleve);
                
                if (serialNumberExistsByOtherEleve)
                {
                    throw new InvalidOperationException(
                        $"Un autre élève avec le SerialNumber '{serialNumber}' existe déjà. " +
                        $"Chaque SerialNumber doit être unique dans le système. " +
                        $"Cet appareil est peut-être déjà lié à un autre élève."
                    );
                }
            }

            eleve.SerialNumber = serialNumber;
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ MISE À JOUR DU SERIAL NUMBER: Par Matricule
        public async Task<bool> UpdateSerialNumberByMatriculeAsync(string matricule, string serialNumber)
        {
            var eleve = await _context.Eleves
                .FirstOrDefaultAsync(e => e.Matricule == matricule);
            
            if (eleve == null)
                return false;

            // ✅ UNICITÉ SERIAL NUMBER: Vérifier que le nouveau SerialNumber n'est pas déjà utilisé par un autre élève
            if (!string.IsNullOrEmpty(serialNumber) && serialNumber != eleve.SerialNumber)
            {
                var serialNumberExistsByOtherEleve = await _context.Eleves
                    .AnyAsync(e => e.SerialNumber == serialNumber && e.IdEleve != eleve.IdEleve);
                
                if (serialNumberExistsByOtherEleve)
                {
                    throw new InvalidOperationException(
                        $"Un autre élève avec le SerialNumber '{serialNumber}' existe déjà. " +
                        $"Chaque SerialNumber doit être unique dans le système. " +
                        $"Cet appareil est peut-être déjà lié à un autre élève."
                    );
                }
            }

            eleve.SerialNumber = serialNumber;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<EleveReinscriptionPrefillDto?> GetReinscriptionPrefillByMatriculeAsync(
            int idEcole,
            string matricule,
            int? idAnneeScolaire = null,
            int? idClasse = null)
        {
            if (string.IsNullOrWhiteSpace(matricule))
                return null;

            var resolvedAnnee = await _scope.ResolveIdAnneeScolaireReferenceForReinscriptionAsync(idEcole, idAnneeScolaire);
            var query = BuildReinscriptionElevesQuery(idEcole, resolvedAnnee, idClasse);

            var eleve = await query
                .FirstOrDefaultAsync(e => e.Matricule == matricule);

            if (eleve == null)
                return null;

            var mapped = await MapReinscriptionPrefillAsync(new List<Eleve> { eleve }, idEcole, resolvedAnnee);
            return mapped.FirstOrDefault();
        }

        public async Task<ElevesAnneeScopedResult<PagedResult<EleveReinscriptionPrefillDto>>> SearchReinscriptionPrefillByNomCompletPagedAsync(
            int idEcole,
            string nomComplet,
            PagedRequest request,
            int? idAnneeScolaire = null,
            int? idClasse = null)
        {
            var resolvedAnnee = await _scope.ResolveIdAnneeScolaireReferenceForReinscriptionAsync(idEcole, idAnneeScolaire);
            var query = BuildReinscriptionElevesQuery(idEcole, resolvedAnnee, idClasse);

            var nomCompletLower = nomComplet.ToLower();
            query = query.Where(e =>
                e.NomComplet != null && e.NomComplet.ToLower().Contains(nomCompletLower));

            if (!request.IncludeInactive)
                query = query.Where(e => e.Statut == true);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(e =>
                    (e.NomComplet != null && e.NomComplet.ToLower().Contains(searchLower)) ||
                    (e.Matricule != null && e.Matricule.ToLower().Contains(searchLower)));
            }

            var allEleves = await query.OrderBy(e => e.NomComplet).ToListAsync();
            var allMapped = await MapReinscriptionPrefillAsync(allEleves, idEcole, resolvedAnnee);

            var total = allMapped.Count;
            var pageNumber = Math.Max(1, request.PageNumber);
            var size = Math.Clamp(request.PageSize, 1, 100);
            var items = allMapped.Skip((pageNumber - 1) * size).Take(size).ToList();

            var page = new PagedResult<EleveReinscriptionPrefillDto>
            {
                Data = items,
                PageNumber = pageNumber,
                PageSize = size,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)size)
            };

            return Scoped(page, idEcole, resolvedAnnee);
        }

        private IQueryable<Eleve> BuildReinscriptionElevesQuery(
            int idEcole,
            int idAnneeScolaire,
            int? idClasse)
        {
            var baseQuery = _context.Eleves.Include(e => e.Tuteur);

            if (idClasse.HasValue && idClasse.Value > 0)
            {
                return _inscriptionResolver.FilterElevesInClasse(baseQuery, idClasse.Value, idAnneeScolaire);
            }

            return _inscriptionResolver.FilterElevesInEcole(baseQuery, idEcole, idAnneeScolaire);
        }

        private async Task<List<EleveReinscriptionPrefillDto>> MapReinscriptionPrefillAsync(
            List<Eleve> eleves,
            int idEcole,
            int idAnneeReference)
        {
            if (eleves.Count == 0)
                return new List<EleveReinscriptionPrefillDto>();

            var eleveIds = eleves.Select(e => e.IdEleve).ToList();

            var refInscriptions = await _context.Inscriptions
                .AsNoTracking()
                .Include(i => i.Classe)
                .Include(i => i.AnneeScolaire)
                .Where(i => eleveIds.Contains(i.IdEleve)
                    && i.IdEcole == idEcole
                    && i.IdAnneeScolaire == idAnneeReference
                    && i.Statut == true
                    && i.StatutInscription != null
                    && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                        || i.StatutInscription == "Confirme"
                        || i.StatutInscription.StartsWith("Confirm")))
                .ToListAsync();

            var refByEleve = refInscriptions
                .GroupBy(i => i.IdEleve)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(i => i.DateInscription).First());

            var refAnnee = await _context.AnneeScolaires
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAnneeScolaire == idAnneeReference);

            var idAnneeCourante = await _scope.TryGetIdAnneeCouranteAsync(idEcole);
            Dictionary<int, Inscription> currentByEleve = new();

            if (idAnneeCourante.HasValue)
            {
                var currentInscriptions = await _context.Inscriptions
                    .AsNoTracking()
                    .Where(i => eleveIds.Contains(i.IdEleve)
                        && i.IdEcole == idEcole
                        && i.IdAnneeScolaire == idAnneeCourante.Value
                        && i.Statut == true
                        && i.StatutInscription != null
                        && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                            || i.StatutInscription == "Confirme"
                            || i.StatutInscription.StartsWith("Confirm")))
                    .ToListAsync();

                currentByEleve = currentInscriptions
                    .GroupBy(i => i.IdEleve)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(i => i.DateInscription).First());
            }

            return eleves.Select(e =>
            {
                refByEleve.TryGetValue(e.IdEleve, out var refIns);
                currentByEleve.TryGetValue(e.IdEleve, out var currentIns);

                return new EleveReinscriptionPrefillDto
                {
                    Type = "Réinscription",
                    IdEleveExistant = e.IdEleve,
                    IdTuteurExistant = e.IdTuteur,
                    IdEcole = idEcole,
                    NomEleve = e.Nom,
                    PostnomEleve = e.Postnom,
                    PrenomEleve = e.Prenom,
                    PhotoEleveUrl = e.PhotoUrl,
                    MatriculeEleve = e.Matricule,
                    GenreEleve = e.Genre,
                    DateNaissanceEleve = e.DateNaissance,
                    LieuNaissanceEleve = e.LieuNaissance,
                    NationaliteEleve = e.Nationalite,
                    ProvinceEleve = e.Province,
                    VilleEleve = e.Ville,
                    CommuneEleve = e.Commune,
                    QuartierEleve = e.Quartier,
                    AvenueEleve = e.Avenue,
                    NumeroEleve = e.Numero,
                    CommentaireEleve = e.Commentaire,
                    NomCompletTuteur = e.Tuteur?.NomComplet,
                    GenreTuteur = e.Tuteur?.Genre,
                    EmailTuteur = e.Tuteur?.Email,
                    TelephoneTuteur = e.Tuteur?.Telephone,
                    NomCompletRepresentant = e.Tuteur?.NomCompletRepresentant,
                    TelephoneRepresentant = e.Tuteur?.TelephoneRepresentant,
                    PhotoTuteurUrl = e.Tuteur?.PhotoTuteurUrl,
                    PieceIdentiteTuteur = e.Tuteur?.PieceIdentiteTuteur,
                    IdAnneeScolaireReference = idAnneeReference,
                    LibelleAnneeScolaireReference = refAnnee?.LibelleAnneeScolaire ?? refIns?.AnneeScolaire?.LibelleAnneeScolaire,
                    IdInscriptionReference = refIns?.IdInscription,
                    IdClassePrecedente = refIns?.IdClasse,
                    NomClassePrecedente = refIns?.Classe?.NomClasse,
                    DejaInscritAnneeCourante = currentIns != null,
                    IdInscriptionAnneeCourante = currentIns?.IdInscription
                };
            }).ToList();
        }
    }
}
