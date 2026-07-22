using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
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
        private readonly ILogger<EleveService> _logger;

        public EleveService(
            KelasiNaBisoDbContext context,
            IInscriptionRepository inscriptionRepository,
            ILogger<EleveService> logger)
        {
            _context = context;
            _inscriptionRepository = inscriptionRepository;
            _logger = logger;
        }

        // ✅ NOUVELLES MÉTHODES PAGINÉES
        public async Task<PagedResult<V_Eleve>> GetAllPagedAsync(PagedRequest request)
        {
            var query = _context.V_Eleves.AsQueryable();

            // Appliquer la recherche si présente
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(e =>
                    (e.NomComplet != null && e.NomComplet.ToLower().Contains(searchLower)) ||
                    (e.Matricule != null && e.Matricule.ToLower().Contains(searchLower))
                );
            }

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(e => e.Statut == true);
            }

            // Appliquer le tri
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.ApplySort(request.SortBy, request.SortDescending);
            }
            else
            {
                // Tri par défaut : NomComplet
                query = request.SortDescending
                    ? query.OrderByDescending(e => e.NomComplet)
                    : query.OrderBy(e => e.NomComplet);
            }

            return await query.ToPagedAsync(request);
        }

        public async Task<CursorPaginatedResult<V_Eleve>> GetAllCursorPagedAsync(CursorPaginationRequest request)
        {
            var query = _context.V_Eleves.AsQueryable();

            // Appliquer la recherche si présente
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(e =>
                    (e.NomComplet != null && e.NomComplet.ToLower().Contains(searchLower)) ||
                    (e.Matricule != null && e.Matricule.ToLower().Contains(searchLower))
                );
            }

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(e => e.Statut == true);
            }

            // Utiliser IdEleve comme curseur
            return await query.ToCursorPagedAsync(request, e => e.IdEleve);
        }

        public async Task<PagedResult<Eleve>> GetByClassePagedAsync(int idClasse, PagedRequest request)
        {
            var query = _context.Eleves
                .Include(e => e.Classe)
                .Include(e => e.Tuteur)
                .Where(e => e.IdClasse == idClasse);

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(e => e.Statut == true);
            }

            // Appliquer la recherche
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(e =>
                    (e.NomComplet != null && e.NomComplet.ToLower().Contains(searchLower)) ||
                    (e.Matricule != null && e.Matricule.ToLower().Contains(searchLower))
                );
            }

            return await query.ToPagedAsync(request, e => e.NomComplet);
        }

        public async Task<PagedResult<Eleve>> GetByEcoleByNomCompletPagedAsync(int idEcole, string nomComplet, PagedRequest request)
        {
            var query = _context.Eleves
                .Include(e => e.Classe)
                    .ThenInclude(c => c.Direction)
                .Include(e => e.Tuteur)
                .Where(e => e.Classe.Direction.IdEcole == idEcole);

            // Filtrer par nom complet si fourni
            if (!string.IsNullOrWhiteSpace(nomComplet))
            {
                var nomCompletLower = nomComplet.ToLower();
                query = query.Where(e => 
                    e.NomComplet != null && e.NomComplet.ToLower().Contains(nomCompletLower)
                );
            }

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(e => e.Statut == true);
            }

            // Appliquer la recherche supplémentaire si présente
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(e =>
                    (e.NomComplet != null && e.NomComplet.ToLower().Contains(searchLower)) ||
                    (e.Matricule != null && e.Matricule.ToLower().Contains(searchLower))
                );
            }

            // Appliquer le tri
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.ApplySort(request.SortBy, request.SortDescending);
            }
            else
            {
                // Tri par défaut : NomComplet
                query = request.SortDescending
                    ? query.OrderByDescending(e => e.NomComplet)
                    : query.OrderBy(e => e.NomComplet);
            }

            return await query.ToPagedAsync(request);
        }

        public async Task<PagedResult<Eleve>> GetByTuteurPagedAsync(int idTuteur, PagedRequest request)
        {
            var query = _context.Eleves
                .Include(e => e.Classe)
                .Include(e => e.Tuteur)
                .Where(e => e.IdTuteur == idTuteur);

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(e => e.Statut == true);
            }

            return await query.ToPagedAsync(request, e => e.NomComplet);
        }

        public async Task<PagedResult<Eleve>> GetByEcolePagedAsync(int idEcole, PagedRequest request)
        {
            var query = _context.Eleves
                .Include(e => e.Classe)
                    .ThenInclude(c => c.Direction)
                .Include(e => e.Tuteur)
                .Where(e => e.Classe.Direction.IdEcole == idEcole);

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(e => e.Statut == true);
            }

            // Appliquer la recherche
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(e =>
                    (e.NomComplet != null && e.NomComplet.ToLower().Contains(searchLower)) ||
                    (e.Matricule != null && e.Matricule.ToLower().Contains(searchLower))
                );
            }

            return await query.ToPagedAsync(request, e => e.NomComplet);
        }

        // ⚠️ DEPRECATED: Méthodes de base CRUD (conserver pour rétrocompatibilité)
        public async Task<IEnumerable<V_Eleve>> GetAllAsync()
        {
            return await _context.V_Eleves
                .OrderBy(e => e.NomComplet)
                .ToListAsync();
        }

        public async Task<Eleve> GetByIdAsync(int id)
        {
            return await _context.Eleves
                .Include(e => e.Classe)
                    .ThenInclude(c => c!.Direction)
                .Include(e => e.Tuteur)
                .Where(e => e.Statut == true) // ✅ Filtrer uniquement les élèves actifs
                .FirstOrDefaultAsync(e => e.IdEleve == id);
        }

        public async Task<Eleve> GetByReferenceAsync(Guid reference)
        {
            return await _context.Eleves
                .Include(e => e.Classe)
                .Include(e => e.Tuteur)
                .Where(e => e.Statut == true) // ✅ Filtrer uniquement les élèves actifs
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
        public async Task<IEnumerable<Eleve>> GetByClasseAsync(int idClasse)
        {
            return await _context.Eleves
                .Include(e => e.Tuteur)
                .Where(e => e.IdClasse == idClasse)
                .Where(e => e.Statut == true) // ✅ Filtrer uniquement les élèves actifs
                .OrderBy(e => e.NomComplet)
                .ToListAsync();
        }

        public async Task<IEnumerable<Eleve>> GetByTuteurAsync(int idTuteur)
        {
            return await _context.Eleves
                .Include(e => e.Classe)
                .Where(e => e.IdTuteur == idTuteur)
                .Where(e => e.Statut == true) // ✅ Filtrer uniquement les élèves actifs
                .OrderBy(e => e.NomComplet)
                .ToListAsync();
        }

        public async Task<IEnumerable<Eleve>> GetByEcoleAsync(int idEcole)
        {
            return await _context.Eleves
                .Include(e => e.Classe)
                .Include(e => e.Tuteur)
                .Where(e => e.Classe.Direction.Ecole.IdEcole == idEcole)
                .Where(e => e.Statut == true) // ✅ Filtrer uniquement les élèves actifs
                .OrderBy(e => e.NomComplet)
                .ToListAsync();
        }

        public async Task<IEnumerable<Eleve>> GetByStatutAsync(bool statut)
        {
            return await _context.Eleves
                .Include(e => e.Classe)
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
                .Include(e => e.Classe)
                    .ThenInclude(c => c!.Direction)
                .Include(e => e.Tuteur)
                .FirstOrDefaultAsync(e => e.Matricule == matricule);
        }

        // ✅ RÉCUPÉRATION PAR SERIAL NUMBER: Récupérer un élève par son numéro de série
        public async Task<Eleve> GetBySerialNumberAsync(string serialNumber)
        {
            return await _context.Eleves
                .Include(e => e.Classe)
                    .ThenInclude(c => c!.Direction)
                .Include(e => e.Tuteur)
                .Where(e => e.Statut == true) // ✅ Filtrer uniquement les élèves actifs
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
    }
}
