
using KelasiNaBiso.Data;
using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBisoAPI.Services.Repositories;
using KelasiNaBiso.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace KelasiNaBiso.Services
{
    public class InscriptionService : IInscriptionRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly string _connectionString;
        private readonly IUsernameGeneratorService _usernameGenerator;
        private readonly KelasiNaBisoAPI.Services.Repositories.IEmailService _emailService;
        private readonly IFirebaseNotificationService _notificationService;
        private readonly ISmsNotificationService _smsService;
        private readonly ISignalRNotificationService _signalRNotificationService;
        private readonly IUtilisateurRepository _utilisateurRepository;
        private readonly ILogger<InscriptionService> _logger;
        private readonly IInscriptionActiveResolver _inscriptionResolver;
        private readonly EleveAnneeScopeHelper _scope;
        private readonly IEleveCompteService _eleveCompteService;
        private readonly ITuteurCompteService _tuteurCompteService;

        public InscriptionService(
            KelasiNaBisoDbContext context, 
            IConfiguration configuration,
            IUsernameGeneratorService usernameGenerator,
            KelasiNaBisoAPI.Services.Repositories.IEmailService emailService,
            IFirebaseNotificationService notificationService,
            ISmsNotificationService smsService,
            ISignalRNotificationService signalRNotificationService,
            IUtilisateurRepository utilisateurRepository,
            ILogger<InscriptionService> logger,
            IInscriptionActiveResolver inscriptionResolver,
            EleveAnneeScopeHelper scope,
            IEleveCompteService eleveCompteService,
            ITuteurCompteService? tuteurCompteService = null)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("KelasiConnection");
            _usernameGenerator = usernameGenerator;
            _emailService = emailService;
            _notificationService = notificationService;
            _smsService = smsService;
            _signalRNotificationService = signalRNotificationService;
            _utilisateurRepository = utilisateurRepository;
            _logger = logger;
            _inscriptionResolver = inscriptionResolver;
            _scope = scope;
            _eleveCompteService = eleveCompteService;
            _tuteurCompteService = tuteurCompteService ?? new TuteurCompteService(
                context,
                utilisateurRepository,
                emailService,
                notificationService,
                smsService,
                signalRNotificationService,
                inscriptionResolver,
                NullLogger<TuteurCompteService>.Instance);
        }

        public async Task<IEnumerable<Inscription>> GetAllAsync()
        {
            return await _context.Inscriptions
                .Where(i => i.Statut == true) // ✅ Filtrer uniquement les inscriptions actives
                .OrderByDescending(i => i.DateInscription)
                .ToListAsync();
        }

        public async Task<Inscription> GetByIdAsync(int id)
        {
            return await _context.Inscriptions
                .Include(i => i.Eleve)
                .Include(i => i.Classe)
                .Include(i => i.Ecole)
                .Include(i => i.AnneeScolaire)
                .Where(i => i.Statut == true) // ✅ Filtrer uniquement les inscriptions actives
                .FirstOrDefaultAsync(i => i.IdInscription == id);
        }

        public async Task<ElevesAnneeScopedResult<IEnumerable<Inscription>>> GetByEcoleAsync(
            int idEcole, int? idAnneeScolaire = null)
        {
            var (ecole, annee) = await _scope.ResolveEcoleAnneeAsync(idEcole, idAnneeScolaire);
            var data = await _context.Inscriptions
                .Include(i => i.Eleve)
                .Include(i => i.Classe)
                .Include(i => i.AnneeScolaire)
                .Where(i => i.IdEcole == ecole && i.IdAnneeScolaire == annee)
                .Where(i => i.Statut == true)
                .OrderByDescending(i => i.DateInscription)
                .ThenByDescending(i => i.IdInscription)
                .ToListAsync();
            return EleveAnneeScopeHelper.Wrap<IEnumerable<Inscription>>(data, ecole, annee);
        }

        public async Task<ElevesAnneeScopedResult<IEnumerable<Inscription>>> GetByClasseAsync(
            int idClasse, int? idAnneeScolaire = null)
        {
            var (idEcole, annee) = await _scope.ResolveClasseAnneeAsync(idClasse, idAnneeScolaire);
            var data = await _context.Inscriptions
                .Include(i => i.Eleve)
                .Include(i => i.Ecole)
                .Include(i => i.AnneeScolaire)
                .Where(i => i.IdClasse == idClasse && i.IdAnneeScolaire == annee)
                .Where(i => i.Statut == true)
                .OrderByDescending(i => i.DateInscription)
                .ThenByDescending(i => i.IdInscription)
                .ToListAsync();
            return EleveAnneeScopeHelper.Wrap<IEnumerable<Inscription>>(data, idEcole, annee);
        }

        public async Task<IEnumerable<Inscription>> GetByAnneeScolaireAsync(int idAnneeScolaire)
        {
            return await _context.Inscriptions
                .Include(i => i.Eleve)
                .Include(i => i.Classe)
                .Include(i => i.Ecole)
                .Where(i => i.IdAnneeScolaire == idAnneeScolaire)
                .Where(i => i.Statut == true) // ✅ Filtrer uniquement les inscriptions actives
                .OrderByDescending(i => i.DateInscription)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inscription>> GetByEleveAsync(int idEleve)
        {
            return await _context.Inscriptions
                .Include(i => i.Classe)
                .Include(i => i.Ecole)
                .Include(i => i.AnneeScolaire)
                .Where(i => i.IdEleve == idEleve)
                .Where(i => i.Statut == true) // ✅ Filtrer uniquement les inscriptions actives
                .OrderByDescending(i => i.DateInscription)
                .ToListAsync();
        }

        // ═══════════════════════════════════════════════════════════════════
        // MÉTHODES HELPER : Normalisation et Vérification d'Unicité
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Normalise un nom pour la comparaison (supprime accents, espaces, etc.)
        /// </summary>
        private string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;
            
            return name.Trim()
                .ToUpperInvariant()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("'", "")
                .Replace("É", "E")
                .Replace("È", "E")
                .Replace("Ê", "E")
                .Replace("Ë", "E")
                .Replace("À", "A")
                .Replace("Â", "A")
                .Replace("Ä", "A")
                .Replace("Ô", "O")
                .Replace("Ö", "O")
                .Replace("Ù", "U")
                .Replace("Û", "U")
                .Replace("Ü", "U")
                .Replace("Ç", "C")
                .Replace("Î", "I")
                .Replace("Ï", "I");
        }

        /// <summary>
        /// Vérifie si un élève existe déjà avec les mêmes critères (nom, prénom, date de naissance, tuteur, école)
        /// </summary>
        private async Task<Eleve?> FindEleveExistantAsync(
            string nom,
            string postnom,
            string prenom,
            DateTime dateNaissance,
            int? idTuteur,
            int idEcole)
        {
            // Normaliser les noms pour la comparaison
            var nomNormalise = NormalizeName(nom);
            var postnomNormalise = NormalizeName(postnom);
            var prenomNormalise = NormalizeName(prenom);

            // Rechercher un élève existant avec les mêmes critères (école via inscription)
            var eleves = await _context.Eleves
                .Include(e => e.Inscriptions).ThenInclude(i => i.Classe)
                .Where(e =>
                    e.IdTuteur == idTuteur
                    && e.DateNaissance.Date == dateNaissance.Date
                    && e.Inscriptions.Any(i =>
                        i.Statut == true
                        && i.IdEcole == idEcole
                        && i.StatutInscription != null
                        && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                            || i.StatutInscription == "Confirme"
                            || i.StatutInscription.StartsWith("Confirm")))
                )
                .ToListAsync();

            // Comparer les noms normalisés
            foreach (var eleve in eleves)
            {
                var eleveNomNormalise = NormalizeName(eleve.Nom ?? "");
                var elevePostnomNormalise = NormalizeName(eleve.Postnom ?? "");
                var elevePrenomNormalise = NormalizeName(eleve.Prenom ?? "");

                if (eleveNomNormalise == nomNormalise
                    && elevePostnomNormalise == postnomNormalise
                    && elevePrenomNormalise == prenomNormalise)
                {
                    return eleve;
                }
            }

            return null;
        }

        /// <summary>
        /// ✨ NOUVEAU : Supprime les accents d'une chaîne de caractères
        /// Exemple : "José" → "Jose", "François" → "Francois"
        /// </summary>
        private string RemoveAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;
            
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            
            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// ✨ NOUVEAU : Normalise un nom complet pour la comparaison robuste
        /// Gère : accents, caractères spéciaux, espaces multiples, ordre des mots
        /// Critères d'unicité : (NomCompletEleve normalisé, DateNaissance, NomCompletTuteur normalisé)
        /// </summary>
        private string NormalizeNomComplet(string? nomComplet)
        {
            if (string.IsNullOrWhiteSpace(nomComplet))
                return string.Empty;
            
            // 1. Supprimer les accents
            var normalized = RemoveAccents(nomComplet);
            
            // 2. Remplacer les caractères spéciaux par des espaces (tirets, apostrophes, etc.)
            normalized = Regex.Replace(normalized, @"[^\w\s]", " ");
            
            // 3. Normaliser les espaces multiples en un seul espace, puis trim
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
            
            // 4. Optionnel : Trier les mots (pour gérer l'ordre différent)
            // Exemple : "Jean Pierre" = "Pierre Jean"
            var mots = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Array.Sort(mots);
            normalized = string.Join(" ", mots);
            
            // 5. Mettre en majuscules
            return normalized.ToUpperInvariant();
        }

        /// <summary>
        /// ✨ NOUVEAU : Vérifie si un élève existe déjà avec les critères d'unicité
        /// Critères : (NomCompletEleve normalisé, DateNaissance, NomCompletTuteur normalisé)
        /// Recherche GLOBALE (toutes écoles confondues)
        /// </summary>
        private async Task<Eleve?> FindEleveExistantParNomCompletAsync(
            string nomCompletEleve,
            DateTime dateNaissance,
            string nomCompletTuteur)
        {
            // Normaliser les noms complets
            var nomCompletEleveNormalise = NormalizeNomComplet(nomCompletEleve);
            var nomCompletTuteurNormalise = NormalizeNomComplet(nomCompletTuteur);
            
            if (string.IsNullOrWhiteSpace(nomCompletEleveNormalise))
            {
                _logger.LogWarning("⚠️ NomCompletEleve vide ou invalide lors de la recherche d'unicité");
                return null;
            }
            
            _logger.LogDebug($"🔍 Recherche élève par NomComplet : NomEleve='{nomCompletEleve}' (normalisé: '{nomCompletEleveNormalise}'), DateNaissance={dateNaissance:yyyy-MM-dd}, Tuteur='{nomCompletTuteur}' (normalisé: '{nomCompletTuteurNormalise}')");
            
            // Rechercher TOUS les élèves avec la même date de naissance (recherche globale)
            var eleves = await _context.Eleves
                .Include(e => e.Tuteur)
                .Where(e => e.DateNaissance.Date == dateNaissance.Date)
                .ToListAsync();
            
            _logger.LogDebug($"🔍 {eleves.Count} élève(s) trouvé(s) avec la même date de naissance ({dateNaissance:yyyy-MM-dd})");
            
            // Comparer les noms complets normalisés
            foreach (var eleve in eleves)
            {
                var eleveNomCompletNormalise = NormalizeNomComplet(eleve.NomComplet);
                
                // Si le nom de l'élève correspond
                if (eleveNomCompletNormalise == nomCompletEleveNormalise)
                {
                    _logger.LogDebug($"✅ Nom de l'élève correspond : '{eleve.NomComplet}' (normalisé: '{eleveNomCompletNormalise}')");
                    
                    // Vérifier le tuteur
                    if (eleve.Tuteur != null)
                    {
                        var tuteurNomCompletNormalise = NormalizeNomComplet(eleve.Tuteur.NomComplet);
                        
                        if (tuteurNomCompletNormalise == nomCompletTuteurNormalise)
                        {
                            _logger.LogInformation($"✅ Élève trouvé (critères d'unicité) : ID={eleve.IdEleve}, Nom='{eleve.NomComplet}', Tuteur='{eleve.Tuteur.NomComplet}', DateNaissance={eleve.DateNaissance:yyyy-MM-dd}");
                            return eleve; // ✅ Correspondance exacte
                        }
                        else
                        {
                            _logger.LogDebug($"⚠️ Nom élève correspond mais tuteur différent : Tuteur BDD='{eleve.Tuteur.NomComplet}' (normalisé: '{tuteurNomCompletNormalise}') vs Recherche='{nomCompletTuteur}' (normalisé: '{nomCompletTuteurNormalise}')");
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(nomCompletTuteurNormalise))
                    {
                        // Si l'élève n'a pas de tuteur et qu'on cherche sans tuteur
                        _logger.LogInformation($"✅ Élève trouvé (sans tuteur) : ID={eleve.IdEleve}, Nom='{eleve.NomComplet}', DateNaissance={eleve.DateNaissance:yyyy-MM-dd}");
                        return eleve;
                    }
                    else
                    {
                        _logger.LogDebug($"⚠️ Nom élève correspond mais élève n'a pas de tuteur en BDD");
                    }
                }
            }
            
            _logger.LogDebug($"❌ Aucun élève trouvé avec les critères d'unicité (NomCompletEleve normalisé, DateNaissance, NomCompletTuteur normalisé)");
            return null;
        }

        /// <summary>
        /// ✨ NOUVEAU: Génère un matricule unique avec GUID partiel (version synchrone - pour compatibilité)
        /// Format: [Ecole(3)][Année(2)]-[GUID(6)]
        /// Exemple: "ESK25-A3F2B1" (12 caractères)
        /// Unicité garantie avec ~16.7 millions de combinaisons par école/année
        /// </summary>
        public string GenerateMatriculeEleve(string nomEcole, CreateInscriptionDto inscriptionDto)
        {
            string matriculeEleve = string.Empty;

            // A. Les 3 premiers caractères du nom de l'école
            var motsEcole = nomEcole.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (motsEcole.Length >= 3)
            {
                matriculeEleve += string.Concat(motsEcole.Take(3).Select(m => char.ToUpper(m[0])));
            }
            else if (motsEcole.Length == 2)
            {
                var mot1 = motsEcole[0];
                var mot2 = motsEcole[1];
                matriculeEleve += char.ToUpper(mot1[0]);
                matriculeEleve += mot1.Length > 1 ? char.ToUpper(mot1[1]) : 'X';
                matriculeEleve += char.ToUpper(mot2[0]);
            }
            else if (motsEcole.Length == 1)
            {
                var mot = motsEcole[0];
                matriculeEleve += mot.Length >= 3
                    ? mot.Substring(0, 3).ToUpper()
                    : mot.ToUpper().PadRight(3, 'X');
            }

            // B. Deux derniers chiffres de l'année en cours
            matriculeEleve += DateTime.Now.Year.ToString().Substring(2);

            // C. Séparateur
            matriculeEleve += "-";

            // D. ✨ GUID partiel de 6 caractères hexadécimaux (garantit l'unicité)
            // 16^6 = 16,777,216 combinaisons possibles
            string guid = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            matriculeEleve += guid;

            return matriculeEleve;
        }

        /// <summary>
        /// ✨ NOUVEAU: Génère un matricule unique avec vérification en base de données (version asynchrone)
        /// Format: [Ecole(3)][Année(2)]-[GUID(6)]
        /// Exemple: "ESK25-A3F2B1" (12 caractères)
        /// </summary>
        public async Task<string> GenerateMatriculeEleveAsync(string nomEcole)
        {
            string codeEcole = string.Empty;

            // A. Les 3 premiers caractères du nom de l'école
            var motsEcole = nomEcole.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (motsEcole.Length >= 3)
            {
                codeEcole = string.Concat(motsEcole.Take(3).Select(m => char.ToUpper(m[0])));
            }
            else if (motsEcole.Length == 2)
            {
                var mot1 = motsEcole[0];
                var mot2 = motsEcole[1];
                codeEcole = $"{char.ToUpper(mot1[0])}{(mot1.Length > 1 ? char.ToUpper(mot1[1]) : 'X')}{char.ToUpper(mot2[0])}";
            }
            else if (motsEcole.Length == 1)
            {
                var mot = motsEcole[0];
                codeEcole = mot.Length >= 3
                    ? mot.Substring(0, 3).ToUpper()
                    : mot.ToUpper().PadRight(3, 'X');
            }

            // B. Année actuelle
            string annee = DateTime.Now.Year.ToString().Substring(2);

            // C. Générer et vérifier l'unicité
            string matricule;
            do
            {
                string guid = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                matricule = $"{codeEcole}{annee}-{guid}";
            }
            while (await _context.Eleves.AnyAsync(e => e.Matricule == matricule));

            return matricule;
        }

        // Obsolète : la SP sp_CreateInscription utilisait Eleves.IdClasse / Tuteurs.IdEcole (colonnes retirées).
        [Obsolete("Utiliser CreateInscriptionAsync. Délègue vers le chemin EF.")]
        public async Task<InscriptionResult> CreateInscriptionWithStoredProcedureAsync(CreateInscriptionDto inscriptionDto)
        {
            _logger.LogWarning(
                "CreateInscriptionWithStoredProcedureAsync est obsolète (SP legacy Eleves.IdClasse / Tuteurs.IdEcole) — délégation vers CreateInscriptionAsync");
            return await CreateInscriptionAsync(inscriptionDto);
        }

        public async Task<Inscription> UpdateAsync(Inscription inscription)
        {
            var existingInscription = await _context.Inscriptions.FindAsync(inscription.IdInscription);
            if (existingInscription == null)
                return null;

            inscription.StatutInscription =
                InscriptionActiveRules.NormalizeStatutInscription(inscription.StatutInscription);

            _context.Entry(existingInscription).CurrentValues.SetValues(inscription);
            await _context.SaveChangesAsync();
            return existingInscription;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var inscription = await _context.Inscriptions.FindAsync(id);
            if (inscription == null)
                return false;

            _context.Inscriptions.Remove(inscription);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Inscriptions.AnyAsync(i => i.IdInscription == id);
        }

        // =============================================
        // NOUVELLE MÉTHODE SANS PROCÉDURE STOCKÉE
        // =============================================
        
        /// <summary>
        /// Crée une inscription en gérant les 3 cas : Nouveau élève + Nouveau tuteur, Nouveau élève + Ancien tuteur, Ancien élève (Réinscription)
        /// </summary>
        public async Task<InscriptionResult> CreateInscriptionAsync(CreateInscriptionDto inscriptionDto)
        {
            var result = new InscriptionResult();

            inscriptionDto.StatutInscription =
                InscriptionActiveRules.NormalizeStatutInscription(inscriptionDto.StatutInscription);

            // ✅ VALIDATION : Vérifier que l'année scolaire existe
            var anneeScolaireExists = await _context.AnneeScolaires
                .AnyAsync(a => a.IdAnneeScolaire == inscriptionDto.IdAnneeScolaire && a.Statut == true);
            
            if (!anneeScolaireExists)
            {
                result.Success = false;
                result.Message = $"❌ L'année scolaire avec l'ID {inscriptionDto.IdAnneeScolaire} n'existe pas ou n'est pas active. Veuillez créer ou activer l'année scolaire avant d'inscrire un élève.";
                return result;
            }

            // ✅ VALIDATION : Vérifier que la classe existe
            var classeExists = await _context.Classes
                .AnyAsync(c => c.IdClasse == inscriptionDto.IdClasse && c.Statut == true);
            
            if (!classeExists)
            {
                result.Success = false;
                result.Message = $"❌ La classe avec l'ID {inscriptionDto.IdClasse} n'existe pas ou n'est pas active.";
                return result;
            }

            // ✅ VALIDATION : Vérifier que l'école existe
            var ecoleExists = await _context.Ecoles
                .AnyAsync(e => e.IdEcole == inscriptionDto.IdEcole && e.Statut == true);
            
            if (!ecoleExists)
            {
                result.Success = false;
                result.Message = $"❌ L'école avec l'ID {inscriptionDto.IdEcole} n'existe pas ou n'est pas active.";
                return result;
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    int? newIdTuteur = null;
                    int? newIdEleve = null;
                    int? newIdInscription = null;
                    bool tuteurExists = false;
                    bool eleveExists = false;

                    // ═══════════════════════════════════════════════════════════════════
                    // ✅ AMÉLIORATION RÉINSCRIPTION : Recherche automatique si IdEleveExistant non fourni
                    // ═══════════════════════════════════════════════════════════════════
                    
                    Eleve? eleveTrouve = null;
                    
                    // Si IdEleveExistant est fourni, utiliser directement
                    if (inscriptionDto.IdEleveExistant.HasValue && inscriptionDto.IdEleveExistant.Value > 0)
                    {
                        eleveTrouve = await _context.Eleves
                            .Include(e => e.Inscriptions).ThenInclude(i => i.Classe)
                                .ThenInclude(c => c.Direction)
                                    .ThenInclude(d => d.Ecole)
                            .FirstOrDefaultAsync(e => e.IdEleve == inscriptionDto.IdEleveExistant.Value);
                    }
                    else
                    {
                        // ✨ NOUVEAU : Recherche automatique d'élève existant par NomComplet
                        // Critères d'unicité : (NomCompletEleve normalisé, DateNaissance, NomCompletTuteur normalisé)
                        // Construire le nom complet de l'élève
                        var nomCompletEleve = $"{inscriptionDto.NomEleve} {inscriptionDto.PostnomEleve} {inscriptionDto.PrenomEleve}".Trim();
                        
                        // Chercher un élève avec les critères d'unicité AVANT de créer le tuteur
                        eleveTrouve = await FindEleveExistantParNomCompletAsync(
                            nomCompletEleve,
                            inscriptionDto.DateNaissanceEleve,
                            inscriptionDto.NomCompletTuteur
                        );
                        
                        // Si pas trouvé avec NomComplet, essayer avec l'ancienne méthode (rétrocompatibilité)
                        if (eleveTrouve == null)
                        {
                            _logger.LogDebug("🔍 Recherche avec méthode ancienne (Nom/Postnom/Prenom) pour rétrocompatibilité");
                        eleveTrouve = await FindEleveExistantAsync(
                            inscriptionDto.NomEleve,
                            inscriptionDto.PostnomEleve,
                            inscriptionDto.PrenomEleve,
                            inscriptionDto.DateNaissanceEleve,
                            inscriptionDto.IdTuteurExistant, // Peut être null
                            inscriptionDto.IdEcole
                        );
                        }
                    }

                    // Si un élève existe déjà
                    if (eleveTrouve != null)
                    {
                        // Cas d'une Réinscription
                        inscriptionDto.Type = "Réinscription";
                        eleveExists = true;
                        newIdEleve = eleveTrouve.IdEleve;

                        // Mettre à jour le statut de l'élève
                        eleveTrouve.Statut = true;
                        
                        // Mettre à jour le tuteur si nécessaire
                        if (eleveTrouve.IdTuteur.HasValue && newIdTuteur.HasValue && eleveTrouve.IdTuteur != newIdTuteur)
                        {
                            // Si le tuteur a changé, mettre à jour
                            eleveTrouve.IdTuteur = newIdTuteur;
                        }
                        else if (!eleveTrouve.IdTuteur.HasValue && newIdTuteur.HasValue)
                        {
                            // Si l'élève n'avait pas de tuteur, l'assigner
                            eleveTrouve.IdTuteur = newIdTuteur;
                        }
                        else if (eleveTrouve.IdTuteur.HasValue)
                        {
                            // Utiliser le tuteur existant de l'élève
                            newIdTuteur = eleveTrouve.IdTuteur;
                        }
                        
                        await _context.SaveChangesAsync();

                        // Mettre à jour le statut du tuteur associé
                        if (newIdTuteur.HasValue)
                        {
                            var tuteur = await _context.Tuteurs.FindAsync(newIdTuteur.Value);
                            if (tuteur != null)
                            {
                                tuteur.Statut = true;
                                await _context.SaveChangesAsync();
                            }
                        }

                        result.Message = $"Réinscription effectuée avec succès. Élève existant réutilisé (ID: {eleveTrouve.IdEleve}).";
                        _logger.LogInformation($"✅ Élève existant réutilisé : {eleveTrouve.NomComplet} (ID: {eleveTrouve.IdEleve})");
                    }
                    else
                    {
                        // Cas d'un nouvel élève
                        inscriptionDto.Type = "Inscription";

                        // Vérifier si le tuteur existe déjà
                        if (inscriptionDto.IdTuteurExistant.HasValue && inscriptionDto.IdTuteurExistant.Value > 0)
                        {
                            tuteurExists = true;
                            newIdTuteur = inscriptionDto.IdTuteurExistant.Value;

                            // Mettre à jour le statut du tuteur existant
                            var tuteurExistantToUpdate = await _context.Tuteurs.FindAsync(inscriptionDto.IdTuteurExistant.Value);
                            if (tuteurExistantToUpdate != null)
                            {
                                tuteurExistantToUpdate.Statut = true;
                                await _context.SaveChangesAsync();
                            }

                            result.Message = "Inscription effectuée avec succès.";
                        }
                        else
                        {
                            // ═══════════════════════════════════════════════════════════════════
                            // ✅ MULTI-RÔLES : Vérifier si un tuteur existe déjà (nom + téléphone + email + école)
                            // ═══════════════════════════════════════════════════════════════════
                            
                            // Dédoublonnage global (téléphone / email), sans IdEcole
                            var telephoneTuteurNorm = TelephoneNormalizer.Normalize(inscriptionDto.TelephoneTuteur);
                            var tuteurExistant = await _context.Tuteurs
                                .FirstOrDefaultAsync(t =>
                                    t.Telephone != null
                                    && telephoneTuteurNorm != null
                                    && (t.Telephone == telephoneTuteurNorm
                                        || t.Telephone.Replace(" ", "").Replace("-", "") == telephoneTuteurNorm)
                                    && (
                                        (!string.IsNullOrWhiteSpace(inscriptionDto.EmailTuteur) && t.Email == inscriptionDto.EmailTuteur)
                                        || t.NomComplet == inscriptionDto.NomCompletTuteur
                                    ));

                            if (tuteurExistant != null)
                            {
                                tuteurExists = true;
                                newIdTuteur = tuteurExistant.IdTuteur;

                                // Mettre à jour le statut et l'email si nécessaire
                                tuteurExistant.Statut = true;
                                if (!string.IsNullOrWhiteSpace(inscriptionDto.EmailTuteur) && 
                                    string.IsNullOrWhiteSpace(tuteurExistant.Email))
                                {
                                    tuteurExistant.Email = inscriptionDto.EmailTuteur;
                                }
                                await _context.SaveChangesAsync();

                                result.Message = "Inscription effectuée avec succès.";
                            }
                            else
                            {
                                // Créer un nouveau tuteur
                                var nouveauTuteur = new Tuteur
                                {
                                    NomComplet = inscriptionDto.NomCompletTuteur,
                                    Genre = inscriptionDto.GenreTuteur,
                                    Email = inscriptionDto.EmailTuteur,
                                    Telephone = TelephoneNormalizer.Normalize(inscriptionDto.TelephoneTuteur),
                                    NomCompletRepresentant = inscriptionDto.NomCompletRepresentant,
                                    TelephoneRepresentant = TelephoneNormalizer.Normalize(inscriptionDto.TelephoneRepresentant),
                                    Statut = true,
                                    DateCreation = DateTime.Now,
                                    PhotoTuteurUrl = inscriptionDto.PhotoTuteurUrl,
                                    PieceIdentiteTuteur = inscriptionDto.PieceIdentiteTuteur
                                };

                                _context.Tuteurs.Add(nouveauTuteur);
                                await _context.SaveChangesAsync();
                                newIdTuteur = nouveauTuteur.IdTuteur;

                                result.Message = "Inscription effectuée avec succès. Nouveau tuteur créé.";
                            }
                        }

                        // ═══════════════════════════════════════════════════════════════════
                        // ✨ VÉRIFICATION D'UNICITÉ : Vérifier une dernière fois avant création
                        // Critères : (NomCompletEleve normalisé, DateNaissance, NomCompletTuteur normalisé)
                        // ═══════════════════════════════════════════════════════════════════
                        
                        // Construire le nom complet de l'élève
                        var nomCompletEleve = $"{inscriptionDto.NomEleve} {inscriptionDto.PostnomEleve} {inscriptionDto.PrenomEleve}".Trim();
                        
                        // Récupérer le nom complet du tuteur créé/trouvé
                        var tuteur = await _context.Tuteurs.FindAsync(newIdTuteur);
                        var nomCompletTuteur = tuteur?.NomComplet ?? inscriptionDto.NomCompletTuteur;
                        
                        // Vérifier si un élève avec les critères d'unicité existe déjà
                        var eleveExistantFinal = await FindEleveExistantParNomCompletAsync(
                            nomCompletEleve,
                            inscriptionDto.DateNaissanceEleve,
                            nomCompletTuteur
                        );
                        
                        // Si pas trouvé, essayer avec l'ancienne méthode (rétrocompatibilité)
                        if (eleveExistantFinal == null)
                        {
                            _logger.LogDebug("🔍 Vérification finale avec méthode ancienne (Nom/Postnom/Prenom) pour rétrocompatibilité");
                            eleveExistantFinal = await FindEleveExistantAsync(
                            inscriptionDto.NomEleve,
                            inscriptionDto.PostnomEleve,
                            inscriptionDto.PrenomEleve,
                            inscriptionDto.DateNaissanceEleve,
                            newIdTuteur,
                            inscriptionDto.IdEcole
                        );
                        }

                        if (eleveExistantFinal != null)
                        {
                            // Élève existe déjà, réutiliser au lieu de créer
                            newIdEleve = eleveExistantFinal.IdEleve;
                            eleveExists = true;
                            
                            // Réactiver l'élève si nécessaire
                            if (eleveExistantFinal.Statut == false)
                            {
                                eleveExistantFinal.Statut = true;
                                await _context.SaveChangesAsync();
                            }
                            
                            result.Message = $"Inscription effectuée avec succès. Élève existant réutilisé (ID: {eleveExistantFinal.IdEleve}).";
                            _logger.LogInformation($"✅ Élève existant réutilisé (vérification finale) : {eleveExistantFinal.NomComplet} (ID: {eleveExistantFinal.IdEleve})");
                        }
                        else
                        {
                            // Créer le nouvel élève
                            var nouvelEleve = new Eleve
                            {
                                ReferenceEleve = Guid.NewGuid(),
                                Nom = inscriptionDto.NomEleve,
                                Postnom = inscriptionDto.PostnomEleve,
                                Prenom = inscriptionDto.PrenomEleve,
                                NomComplet = $"{inscriptionDto.NomEleve} {inscriptionDto.PostnomEleve} {inscriptionDto.PrenomEleve}",
                                Genre = inscriptionDto.GenreEleve,
                                DateNaissance = inscriptionDto.DateNaissanceEleve,
                                LieuNaissance = inscriptionDto.LieuNaissanceEleve,
                                Nationalite = inscriptionDto.NationaliteEleve,
                                Commentaire = inscriptionDto.CommentaireEleve,
                                IdTuteur = newIdTuteur,
                                Statut = true,
                                DateCreation = DateTime.Now,
                                PhotoUrl = inscriptionDto.PhotoEleveUrl,
                                Matricule = inscriptionDto.MatriculeEleve,
                                // Champs d'adresse hérités
                                Province = inscriptionDto.ProvinceEleve,
                                Ville = inscriptionDto.VilleEleve,
                                Commune = inscriptionDto.CommuneEleve,
                                Quartier = inscriptionDto.QuartierEleve,
                                Avenue = inscriptionDto.AvenueEleve,
                                Numero = inscriptionDto.NumeroEleve
                            };

                            _context.Eleves.Add(nouvelEleve);
                            await _context.SaveChangesAsync();
                            newIdEleve = nouvelEleve.IdEleve;
                            
                            _logger.LogInformation($"✅ Nouvel élève créé : {nouvelEleve.NomComplet} (ID: {nouvelEleve.IdEleve})");
                        }
                    }

                    // Créer l'inscription
                    var inscription = new Inscription
                    {
                        Type = inscriptionDto.Type,
                        IdEleve = newIdEleve.Value,
                        IdEcole = inscriptionDto.IdEcole,
                        IdClasse = inscriptionDto.IdClasse,
                        IdAnneeScolaire = inscriptionDto.IdAnneeScolaire,
                        DateInscription = inscriptionDto.DateInscription,
                        StatutInscription = inscriptionDto.StatutInscription,
                        DateCreation = DateTime.Now
                    };

                    _context.Inscriptions.Add(inscription);
                    await _context.SaveChangesAsync();
                    newIdInscription = inscription.IdInscription;

                    // Assigner les valeurs de sortie
                    result.IdInscription = newIdInscription;
                    result.IdEleve = newIdEleve;
                    result.IdTuteur = newIdTuteur;
                    result.Success = true;

                    // ✨ NOUVEAU : Créer le compte utilisateur pour le tuteur APRÈS la création de l'élève
                    // Cela permet de passer les informations de l'enfant dans l'email
                    if (newIdTuteur.HasValue && newIdEleve.HasValue)
                    {
                        var tuteur = await _context.Tuteurs.FindAsync(newIdTuteur.Value);
                        var eleve = await _context.Eleves
                            .Include(e => e.Inscriptions).ThenInclude(i => i.Classe)
                            .FirstOrDefaultAsync(e => e.IdEleve == newIdEleve.Value);
                        
                        if (tuteur != null && eleve != null)
                        {
                            var utilisateurInfo = await CreateDefaultTuteurUserAsync(
                                tuteur, 
                                inscriptionDto.IdEcole, 
                                eleve);  // ✨ Passer l'élève
                            
                            result.CompteUtilisateurTuteur = utilisateurInfo;
                        }

                        // Compte Élève : login = matricule, MDP initial 123456
                        if (eleve != null && !string.IsNullOrWhiteSpace(eleve.Matricule))
                        {
                            var eleveCompte = await _eleveCompteService.CreateDefaultEleveUserAsync(
                                eleve, inscriptionDto.IdEcole);
                            result.CompteUtilisateurEleve = ToCompteElevePapier(eleveCompte);
                        }
                        else if (newIdEleve.HasValue)
                        {
                            var elevePourCompte = await _context.Eleves
                                .FirstOrDefaultAsync(e => e.IdEleve == newIdEleve.Value);
                            if (elevePourCompte != null && !string.IsNullOrWhiteSpace(elevePourCompte.Matricule))
                            {
                                var eleveCompte = await _eleveCompteService.CreateDefaultEleveUserAsync(
                                    elevePourCompte, inscriptionDto.IdEcole);
                                result.CompteUtilisateurEleve = ToCompteElevePapier(eleveCompte);
                            }
                        }
                    }
                    else if (newIdEleve.HasValue)
                    {
                        // Inscription sans nouveau tuteur (réinscription) : créer le compte Élève si absent
                        var elevePourCompte = await _context.Eleves
                            .FirstOrDefaultAsync(e => e.IdEleve == newIdEleve.Value);
                        if (elevePourCompte != null && !string.IsNullOrWhiteSpace(elevePourCompte.Matricule))
                        {
                            var eleveCompte = await _eleveCompteService.CreateDefaultEleveUserAsync(
                                elevePourCompte, inscriptionDto.IdEcole);
                            result.CompteUtilisateurEleve = ToCompteElevePapier(eleveCompte);
                        }
                    }

                    // Récupérer l'inscription complète avec les relations
                    if (result.Success && result.IdInscription.HasValue)
                    {
                        result.Inscription = await GetByIdAsync(result.IdInscription.Value);
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result.Success = false;
                    result.Message = $"Erreur lors de l'inscription : {ex.Message}";
                    result.IdInscription = null;
                    result.IdEleve = null;
                    result.IdTuteur = null;
                }
            }

            return result;
        }

        // ✅ SOFT DELETE: Toggle le statut d'une inscription (actif <-> inactif)
        public async Task<bool> ToggleStatutAsync(int id)
        {
            var inscription = await _context.Inscriptions.FindAsync(id);
            if (inscription == null)
                return false;

            inscription.Statut = inscription.Statut != true;
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ CASCADE SOFT DELETE : Désactiver toutes les inscriptions actives d'un élève
        /// <summary>
        /// Désactive toutes les inscriptions actives d'un élève (cascade logicielle)
        /// Utilisé lors de la désactivation d'un élève pour maintenir la cohérence des données
        /// </summary>
        /// <param name="idEleve">ID de l'élève dont les inscriptions doivent être désactivées</param>
        /// <returns>Nombre d'inscriptions désactivées</returns>
        public async Task<int> DesactiverInscriptionsParEleveAsync(int idEleve)
        {
            try
            {
                _logger.LogInformation($"🔄 Désactivation des inscriptions actives pour l'élève ID: {idEleve}");

                // Récupérer toutes les inscriptions actives de l'élève
                var inscriptions = await _context.Inscriptions
                    .Where(i => i.IdEleve == idEleve && (i.Statut == true || i.Statut == null))
                    .ToListAsync();

                if (!inscriptions.Any())
                {
                    _logger.LogInformation($"ℹ️ Aucune inscription active trouvée pour l'élève ID: {idEleve}");
                    return 0;
                }

                // Désactiver toutes les inscriptions
                foreach (var inscription in inscriptions)
                {
                    inscription.Statut = false;
                    _logger.LogDebug($"   → Inscription #{inscription.IdInscription} désactivée");
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ {inscriptions.Count} inscription(s) désactivée(s) pour l'élève ID: {idEleve}");
                return inscriptions.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur lors de la désactivation des inscriptions pour l'élève ID: {idEleve}");
                throw;
            }
        }

        public async Task<IEnumerable<Inscription>> GetByStatutAsync(bool statut)
        {
            return await _context.Inscriptions
                .Include(i => i.Eleve)
                .Include(i => i.Classe)
                .Include(i => i.Ecole)
                .Include(i => i.AnneeScolaire)
                .Where(i => i.Statut == statut)
                .OrderByDescending(i => i.DateInscription)
                .ToListAsync();
        }

        // ✅ PAGINATION: Méthodes paginées
        public async Task<PagedResult<Inscription>> GetAllPagedAsync(PagedRequest request)
        {
            var query = _context.Inscriptions
                .Include(i => i.Eleve)
                .Include(i => i.Classe)
                .Include(i => i.Ecole)
                .Include(i => i.AnneeScolaire)
                .AsQueryable();

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(i => i.Statut == true);
            }

            // Appliquer la recherche
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(i =>
                    (i.Type != null && i.Type.ToLower().Contains(searchLower)) ||
                    (i.StatutInscription != null && i.StatutInscription.ToLower().Contains(searchLower)) ||
                    (i.Eleve != null && i.Eleve.NomComplet != null && i.Eleve.NomComplet.ToLower().Contains(searchLower)) ||
                    (i.Eleve != null && i.Eleve.Matricule != null && i.Eleve.Matricule.ToLower().Contains(searchLower))
                );
            }

            // Appliquer le tri
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.ApplySort(request.SortBy, request.SortDescending);
            }
            else
            {
                // Tri par défaut : DateInscription DESC (plus récent en premier)
                query = request.SortDescending
                    ? query.OrderBy(i => i.DateInscription)
                    : query.OrderByDescending(i => i.DateInscription);
            }

            return await query.ToPagedAsync(request);
        }

        public async Task<PagedResult<Inscription>> GetByElevePagedAsync(int idEleve, PagedRequest request)
        {
            var query = _context.Inscriptions
                .Include(i => i.Classe)
                .Include(i => i.Ecole)
                .Include(i => i.AnneeScolaire)
                .Where(i => i.IdEleve == idEleve)
                .AsQueryable();

            // Filtrer par statut
            if (!request.IncludeInactive)
            {
                query = query.Where(i => i.Statut == true);
            }

            // Appliquer la recherche
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(i =>
                    (i.Type != null && i.Type.ToLower().Contains(searchLower)) ||
                    (i.StatutInscription != null && i.StatutInscription.ToLower().Contains(searchLower))
                );
            }

            // Appliquer le tri
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.ApplySort(request.SortBy, request.SortDescending);
            }
            else
            {
                // Tri par défaut : DateInscription DESC
                query = request.SortDescending
                    ? query.OrderBy(i => i.DateInscription)
                    : query.OrderByDescending(i => i.DateInscription);
            }

            return await query.ToPagedAsync(request);
        }

        public async Task<ElevesAnneeScopedResult<PagedResult<Inscription>>> GetByEcolePagedAsync(
            int idEcole, PagedRequest request, int? idAnneeScolaire = null)
        {
            var (ecole, annee) = await _scope.ResolveEcoleAnneeAsync(idEcole, idAnneeScolaire);
            var query = BuildInscriptionListQuery()
                .Where(i => i.IdEcole == ecole && i.IdAnneeScolaire == annee);
            query = ApplyInscriptionListFilters(query, request);
            var paged = await query.ToPagedAsync(request);
            return EleveAnneeScopeHelper.Wrap(paged, ecole, annee);
        }

        public async Task<ElevesAnneeScopedResult<PagedResult<Inscription>>> GetByClassePagedAsync(
            int idClasse, PagedRequest request, int? idAnneeScolaire = null)
        {
            var (idEcole, annee) = await _scope.ResolveClasseAnneeAsync(idClasse, idAnneeScolaire);
            var query = BuildInscriptionListQuery()
                .Where(i => i.IdClasse == idClasse && i.IdAnneeScolaire == annee);
            query = ApplyInscriptionListFilters(query, request);
            var paged = await query.ToPagedAsync(request);
            return EleveAnneeScopeHelper.Wrap(paged, idEcole, annee);
        }

        private IQueryable<Inscription> BuildInscriptionListQuery() =>
            _context.Inscriptions
                .Include(i => i.Eleve)
                .Include(i => i.Classe)
                .Include(i => i.AnneeScolaire)
                .Where(i => i.Statut == true)
                .AsQueryable();

        private static IQueryable<Inscription> ApplyInscriptionListFilters(
            IQueryable<Inscription> query, PagedRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(i =>
                    (i.Type != null && i.Type.ToLower().Contains(searchLower)) ||
                    (i.StatutInscription != null && i.StatutInscription.ToLower().Contains(searchLower)) ||
                    (i.Eleve != null && i.Eleve.NomComplet != null && i.Eleve.NomComplet.ToLower().Contains(searchLower)) ||
                    (i.Eleve != null && i.Eleve.Matricule != null && i.Eleve.Matricule.ToLower().Contains(searchLower)));
            }

            if (!string.IsNullOrWhiteSpace(request.SortBy))
                query = query.ApplySort(request.SortBy, request.SortDescending);
            else
                query = request.SortDescending
                    ? query.OrderBy(i => i.DateInscription).ThenBy(i => i.IdInscription)
                    : query.OrderByDescending(i => i.DateInscription).ThenByDescending(i => i.IdInscription);

            return query;
        }

        public async Task<PagedResult<Inscription>> GetByStatutPagedAsync(bool statut, PagedRequest request)
        {
            var query = _context.Inscriptions
                .Include(i => i.Eleve)
                .Include(i => i.Classe)
                .Include(i => i.Ecole)
                .Include(i => i.AnneeScolaire)
                .Where(i => i.Statut == statut)
                .AsQueryable();

            // Appliquer la recherche
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(i =>
                    (i.Type != null && i.Type.ToLower().Contains(searchLower)) ||
                    (i.StatutInscription != null && i.StatutInscription.ToLower().Contains(searchLower)) ||
                    (i.Eleve != null && i.Eleve.NomComplet != null && i.Eleve.NomComplet.ToLower().Contains(searchLower)) ||
                    (i.Eleve != null && i.Eleve.Matricule != null && i.Eleve.Matricule.ToLower().Contains(searchLower))
                );
            }

            // Appliquer le tri
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                query = query.ApplySort(request.SortBy, request.SortDescending);
            }
            else
            {
                // Tri par défaut : DateInscription DESC
                query = request.SortDescending
                    ? query.OrderBy(i => i.DateInscription)
                    : query.OrderByDescending(i => i.DateInscription);
            }

            return await query.ToPagedAsync(request);
        }

        /// <summary>
        /// Délègue la création / liaison du compte Parent à <see cref="ITuteurCompteService"/>.
        /// </summary>
        private async Task<UtilisateurInfo?> CreateDefaultTuteurUserAsync(Tuteur tuteur, int idEcole, Eleve eleve = null)
        {
            return await _tuteurCompteService.CreateDefaultTuteurUserAsync(
                tuteur, idEcole, eleve, sendInscriptionNotifications: true);
        }

        private static UtilisateurInfo? ToCompteElevePapier(EleveCompteCreateResult createResult)
        {
            if (createResult.Info == null)
                return null;

            var info = createResult.Info;
            if (createResult.Outcome != EleveCompteCreateOutcome.Created)
                info.MotDePasseParDefaut = "";
            return info;
        }

    }
}
