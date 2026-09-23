using KelasiNaBiso.Data;
using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class TuteurService : ITuteurRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IInscriptionActiveResolver _inscriptionResolver;
        private readonly EleveAnneeScopeHelper _scope;
        private readonly ITuteurCompteService _tuteurCompteService;

        public TuteurService(
            KelasiNaBisoDbContext context,
            IInscriptionActiveResolver inscriptionResolver,
            EleveAnneeScopeHelper scope,
            ITuteurCompteService tuteurCompteService)
        {
            _context = context;
            _inscriptionResolver = inscriptionResolver;
            _scope = scope;
            _tuteurCompteService = tuteurCompteService;
        }

        public async Task<IEnumerable<Tuteur>> GetAllAsync()
        {
            return await _context.Tuteurs
              //  .Include(t => t.Ecole)
              //  .Include(t => t.Eleves)
                .Where(t => t.Statut == true) // ✅ Filtrer uniquement les tuteurs actifs
                .ToListAsync();
        }

        public async Task<Tuteur> GetByIdAsync(int id)
        {
            return await _context.Tuteurs
              //  .Include(t => t.Ecole)
              //  .Include(t => t.Eleves)
                .Where(t => t.Statut == true) // ✅ Filtrer uniquement les tuteurs actifs
                .FirstOrDefaultAsync(t => t.IdTuteur == id);
        }

        public async Task<ElevesAnneeScopedResult<IEnumerable<Tuteur>>> GetByEcoleAsync(
            int idEcole,
            int? idAnneeScolaire = null)
        {
            var (ecole, annee) = await _scope.ResolveEcoleAnneeAsync(idEcole, idAnneeScolaire);
            var data = await _inscriptionResolver
                .FilterTuteursInEcole(_context.Tuteurs.AsNoTracking(), ecole, annee)
                .OrderBy(t => t.NomComplet)
                .ToListAsync();

            return EleveAnneeScopeHelper.Wrap<IEnumerable<Tuteur>>(data, ecole, annee);
        }

        public async Task<Tuteur> CreateAsync(Tuteur tuteur)
        {
            // ✅ UNICITÉ EMAIL TUTEUR: Vérifier que l'email n'existe pas déjà
            if (!string.IsNullOrEmpty(tuteur.Email))
            {
                var emailExists = await ExistsByEmailAsync(tuteur.Email);
                if (emailExists)
                {
                    throw new InvalidOperationException(
                        $"Un tuteur avec l'email '{tuteur.Email}' existe déjà. " +
                        $"Chaque email tuteur doit être unique dans le système."
                    );
                }
            }
            
            tuteur.DateCreation = DateTime.Now;
            
            _context.Tuteurs.Add(tuteur);
            await _context.SaveChangesAsync();
            return tuteur;
        }

        public async Task<CreateTuteurResultDto> CreateWithCompteAsync(CreateTuteurDto dto, int idEcole)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (idEcole <= 0)
                throw new InvalidOperationException("idEcole est obligatoire.");

            var ecoleExists = await _context.Ecoles.AsNoTracking()
                .AnyAsync(e => e.IdEcole == idEcole);
            if (!ecoleExists)
                throw new KeyNotFoundException($"École {idEcole} introuvable.");

            var telephone = TelephoneNormalizer.Normalize(dto.Telephone) ?? dto.Telephone.Trim();

            var tuteur = new Tuteur
            {
                NomComplet = dto.NomComplet.Trim(),
                Genre = dto.Genre.Trim(),
                Telephone = telephone,
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
                PhotoTuteurUrl = dto.PhotoTuteurUrl,
                PieceIdentiteTuteur = dto.PieceIdentiteTuteur,
                NomCompletRepresentant = string.IsNullOrWhiteSpace(dto.NomCompletRepresentant)
                    ? null
                    : dto.NomCompletRepresentant.Trim(),
                TelephoneRepresentant = string.IsNullOrWhiteSpace(dto.TelephoneRepresentant)
                    ? null
                    : dto.TelephoneRepresentant.Trim(),
                Statut = true,
                DateCreation = DateTime.Now
            };

            var created = await CreateAsync(tuteur);

            var compte = await _tuteurCompteService.CreateDefaultTuteurUserAsync(
                created,
                idEcole,
                eleve: null,
                sendInscriptionNotifications: false);

            return new CreateTuteurResultDto
            {
                Tuteur = created,
                CompteUtilisateur = compte
            };
        }

        public async Task<Tuteur> UpdateAsync(Tuteur tuteur)
        {
            var existingTuteur = await _context.Tuteurs.FindAsync(tuteur.IdTuteur);
            if (existingTuteur == null)
                return null;

            // ✅ UNICITÉ EMAIL TUTEUR: Vérifier que le nouvel email n'est pas déjà utilisé par un autre tuteur
            if (!string.IsNullOrEmpty(tuteur.Email) && tuteur.Email != existingTuteur.Email)
            {
                var emailExistsByOtherTuteur = await _context.Tuteurs
                    .AnyAsync(t => t.Email == tuteur.Email && t.IdTuteur != tuteur.IdTuteur);
                
                if (emailExistsByOtherTuteur)
                {
                    throw new InvalidOperationException(
                        $"Un autre tuteur avec l'email '{tuteur.Email}' existe déjà. " +
                        $"Chaque email tuteur doit être unique dans le système."
                    );
                }
            }

            _context.Entry(existingTuteur).CurrentValues.SetValues(tuteur);

            await SyncTuteurUtilisateurAsync(existingTuteur);

            await _context.SaveChangesAsync();
            return existingTuteur;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tuteur = await _context.Tuteurs.FindAsync(id);
            if (tuteur == null)
                return false;

            _context.Tuteurs.Remove(tuteur);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Tuteurs.AnyAsync(t => t.IdTuteur == id);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Tuteurs.AnyAsync(t => t.Email == email);
        }

        public async Task<IEnumerable<TuteurEleveListItemDto>> GetElevesAsync(
            int idTuteur,
            string? searchTerm = null,
            string? libelleAnneeScolaire = null)
        {
            var query = _context.Inscriptions
                .AsNoTracking()
                .Include(i => i.Eleve)
                .Include(i => i.Ecole)
                .Include(i => i.Classe)
                .Include(i => i.AnneeScolaire)
                .Where(i => i.Eleve != null
                    && i.Eleve.IdTuteur == idTuteur
                    && i.Eleve.Statut == true
                    && i.Statut == true
                    && i.StatutInscription != null
                    && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                        || i.StatutInscription == "Confirme"
                        || i.StatutInscription.StartsWith("Confirm")));

            if (!string.IsNullOrWhiteSpace(libelleAnneeScolaire))
            {
                var libelle = libelleAnneeScolaire.Trim().ToLower();
                query = query.Where(i => i.AnneeScolaire != null
                    && i.AnneeScolaire.LibelleAnneeScolaire.ToLower() == libelle);
            }
            else
            {
                var now = DateTime.Now;
                query = query.Where(i => i.AnneeScolaire != null
                    && i.AnneeScolaire.Statut == true
                    && i.AnneeScolaire.DateDebut <= now
                    && i.AnneeScolaire.DateFin >= now);
            }

            var inscriptions = await query
                .OrderBy(i => i.Eleve!.NomComplet)
                .ThenByDescending(i => i.DateInscription)
                .ToListAsync();

            var inscriptionByEleve = inscriptions
                .GroupBy(i => new { i.IdEleve, i.IdEcole })
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(i => i.DateInscription).First());

            var mapped = inscriptionByEleve.Values
                .Select(i => TuteurEleveListItemDto.FromEleve(
                    i.Eleve!, i, i.IdEcole, i.Ecole?.Nom))
                .ToList();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                if (term.Length > 200)
                    term = term[..200];

                var searchLower = term.ToLower();
                mapped = mapped.Where(e =>
                    (e.NomComplet != null && e.NomComplet.ToLower().Contains(searchLower))
                    || (e.Matricule != null && e.Matricule.ToLower().Contains(searchLower))
                    || (e.NomClasse != null && e.NomClasse.ToLower().Contains(searchLower))
                    || (e.NomEcole != null && e.NomEcole.ToLower().Contains(searchLower)))
                    .ToList();
            }

            return mapped;
        }

        // ✅ SOFT DELETE: Toggle le statut d'un tuteur (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var tuteur = await _context.Tuteurs.FindAsync(id);
            if (tuteur == null)
                return false;

            tuteur.Statut = tuteur.Statut != true;
            await SyncTuteurUtilisateurAsync(tuteur);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task SyncTuteurUtilisateurAsync(Tuteur tuteur, CancellationToken cancellationToken = default)
        {
            var utilisateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.IdTuteur == tuteur.IdTuteur, cancellationToken);

            if (utilisateur == null)
            {
                return;
            }

            utilisateur.NomUtilisateur = tuteur.NomComplet;
            utilisateur.PrenomUtilisateur = null;
            utilisateur.PostNomUtilisateur = null;
            utilisateur.Telephone = tuteur.Telephone;
            utilisateur.Email = tuteur.Email;
            utilisateur.PhotoUrl = tuteur.PhotoTuteurUrl;
            utilisateur.Genre = tuteur.Genre;
            utilisateur.Statut = tuteur.Statut ?? utilisateur.Statut;
            // IdEcole utilisateur : ne plus dériver de Tuteur.IdEcole (parent multi-écoles)
        }
    }
}
