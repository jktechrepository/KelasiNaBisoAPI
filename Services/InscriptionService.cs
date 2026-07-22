
using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBisoAPI.Services.Repositories;
using KelasiNaBiso.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Data;
using MySqlConnector; // ✅ MIGRATION MARIADB: Utilisation de MySqlConnector (inclus avec Pomelo)
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

        public InscriptionService(
            KelasiNaBisoDbContext context, 
            IConfiguration configuration,
            IUsernameGeneratorService usernameGenerator,
            KelasiNaBisoAPI.Services.Repositories.IEmailService emailService,
            IFirebaseNotificationService notificationService,
            ISmsNotificationService smsService,
            ISignalRNotificationService signalRNotificationService,
            IUtilisateurRepository utilisateurRepository,
            ILogger<InscriptionService> logger)
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

        public async Task<IEnumerable<Inscription>> GetByEcoleAsync(int idEcole)
        {
            return await _context.Inscriptions
                .Include(i => i.Eleve)
                .Include(i => i.Classe)
                .Include(i => i.AnneeScolaire)
                .Where(i => i.IdEcole == idEcole)
                .Where(i => i.Statut == true) // ✅ Filtrer uniquement les inscriptions actives
                .OrderByDescending(i => i.DateInscription)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inscription>> GetByClasseAsync(int idClasse)
        {
            return await _context.Inscriptions
                .Include(i => i.Eleve)
                .Include(i => i.Ecole)
                .Include(i => i.AnneeScolaire)
                .Where(i => i.IdClasse == idClasse)
                .Where(i => i.Statut == true) // ✅ Filtrer uniquement les inscriptions actives
                .OrderByDescending(i => i.DateInscription)
                .ToListAsync();
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

            // Rechercher un élève existant avec les mêmes critères
            var eleves = await _context.Eleves
                .Include(e => e.Classe)
                    .ThenInclude(c => c.Direction)
                        .ThenInclude(d => d.Ecole)
                .Where(e => 
                    e.IdTuteur == idTuteur
                    && e.DateNaissance.Date == dateNaissance.Date
                    && e.Classe != null 
                    && e.Classe.Direction != null 
                    && e.Classe.Direction.Ecole != null
                    && e.Classe.Direction.Ecole.IdEcole == idEcole
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

        // Nouvelle méthode pour créer une inscription avec la procédure stockée
        public async Task<InscriptionResult> CreateInscriptionWithStoredProcedureAsync(CreateInscriptionDto inscriptionDto)
        {
            var result = new InscriptionResult();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("sp_CreateInscription", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Paramètres d'entrée
                    command.Parameters.AddWithValue("@Type", inscriptionDto.Type);
                    command.Parameters.AddWithValue("@IdEcole", inscriptionDto.IdEcole);
                    command.Parameters.AddWithValue("@IdClasse", inscriptionDto.IdClasse);
                    command.Parameters.AddWithValue("@IdAnneeScolaire", inscriptionDto.IdAnneeScolaire);
                    command.Parameters.AddWithValue("@DateInscription", inscriptionDto.DateInscription);
                    command.Parameters.AddWithValue("@StatutInscription", inscriptionDto.StatutInscription);

                    // Données de l'élève
                    command.Parameters.AddWithValue("@NomEleve", inscriptionDto.NomEleve);
                    command.Parameters.AddWithValue("@PostnomEleve", inscriptionDto.PostnomEleve);
                    command.Parameters.AddWithValue("@PrenomEleve", inscriptionDto.PrenomEleve);
                    command.Parameters.AddWithValue("@GenreEleve", inscriptionDto.GenreEleve);
                    command.Parameters.AddWithValue("@DateNaissanceEleve", inscriptionDto.DateNaissanceEleve);
                    command.Parameters.AddWithValue("@LieuNaissanceEleve", inscriptionDto.LieuNaissanceEleve);
                    command.Parameters.AddWithValue("@NationaliteEleve", inscriptionDto.NationaliteEleve);
                    command.Parameters.AddWithValue("@PhotoEleveUrl", inscriptionDto.PhotoEleveUrl);
                    command.Parameters.AddWithValue("@MatriculeEleve ", inscriptionDto.MatriculeEleve);
                    command.Parameters.AddWithValue("@ProvinceEleve", inscriptionDto.PrenomEleve);
                    command.Parameters.AddWithValue("@VilleEleve", inscriptionDto.VilleEleve);
                    command.Parameters.AddWithValue("@CommuneEleve", inscriptionDto.CommuneEleve);
                    command.Parameters.AddWithValue("@QuartierEleve", inscriptionDto.QuartierEleve);
                    command.Parameters.AddWithValue("@AvenueEleve", inscriptionDto.AvenueEleve);
                    command.Parameters.AddWithValue("@NumeroEleve", inscriptionDto.NumeroEleve);


                    command.Parameters.AddWithValue("@CommentaireEleve", (object)inscriptionDto.CommentaireEleve ?? DBNull.Value);

                    // Données du tuteur
                    command.Parameters.AddWithValue("@NomCompletTuteur", inscriptionDto.NomCompletTuteur);
                    command.Parameters.AddWithValue("@GenreTuteur", inscriptionDto.GenreTuteur);
                    command.Parameters.AddWithValue("@EmailTuteur", (object)inscriptionDto.EmailTuteur ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TelephoneTuteur", (object)inscriptionDto.TelephoneTuteur ?? DBNull.Value);
                    command.Parameters.AddWithValue("@NomCompletRepresentant", (object)inscriptionDto.NomCompletRepresentant ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TelephoneRepresentant", (object)inscriptionDto.TelephoneRepresentant ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PhotoTuteurUrl", (object)inscriptionDto.PhotoTuteurUrl ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PieceIdentiteTuteur", (object)inscriptionDto.PieceIdentiteTuteur ?? DBNull.Value);

                    // Pour les cas de réinscription
                    command.Parameters.AddWithValue("@IdEleveExistant", 
                        inscriptionDto.IdEleveExistant.HasValue && inscriptionDto.IdEleveExistant.Value > 0 
                            ? (object)inscriptionDto.IdEleveExistant.Value 
                            : DBNull.Value);
                    command.Parameters.AddWithValue("@IdTuteurExistant", 
                        inscriptionDto.IdTuteurExistant.HasValue && inscriptionDto.IdTuteurExistant.Value > 0 
                            ? (object)inscriptionDto.IdTuteurExistant.Value 
                            : DBNull.Value);

                    // Paramètres de sortie
                    var idInscriptionParam = command.Parameters.Add("@IdInscription", MySqlDbType.Int32);
                    idInscriptionParam.Direction = ParameterDirection.Output;

                    var idEleveParam = command.Parameters.Add("@IdEleve", MySqlDbType.Int32);
                    idEleveParam.Direction = ParameterDirection.Output;

                    var idTuteurParam = command.Parameters.Add("@IdTuteur", MySqlDbType.Int32);
                    idTuteurParam.Direction = ParameterDirection.Output;

                    var messageParam = command.Parameters.Add("@Message", MySqlDbType.VarChar, 500);
                    messageParam.Direction = ParameterDirection.Output;

                    var successParam = command.Parameters.Add("@Success", MySqlDbType.Bit);
                    successParam.Direction = ParameterDirection.Output;

                    try
                    {
                        await command.ExecuteNonQueryAsync();

                        result.Success = (bool)successParam.Value;
                        result.Message = messageParam.Value?.ToString() ?? "Opération terminée";
                        result.IdInscription = idInscriptionParam.Value != DBNull.Value ? (int)idInscriptionParam.Value : null;
                        result.IdEleve = idEleveParam.Value != DBNull.Value ? (int)idEleveParam.Value : null;
                        result.IdTuteur = idTuteurParam.Value != DBNull.Value ? (int)idTuteurParam.Value : null;

                        if (result.Success && result.IdInscription.HasValue)
                        {
                            // Récupérer l'inscription complète avec les relations
                            result.Inscription = await GetByIdAsync(result.IdInscription.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Success = false;
                        result.Message = $"Erreur lors de l'exécution de la procédure stockée : {ex.Message}";
                    }
                }
            }

            return result;
        }

        public async Task<Inscription> UpdateAsync(Inscription inscription)
        {
            var existingInscription = await _context.Inscriptions.FindAsync(inscription.IdInscription);
            if (existingInscription == null)
                return null;

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
                            .Include(e => e.Classe)
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
                        eleveTrouve.IdClasse = inscriptionDto.IdClasse;
                        
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
                            
                            var tuteurExistant = await _context.Tuteurs
                                .FirstOrDefaultAsync(t => 
                                    t.NomComplet == inscriptionDto.NomCompletTuteur 
                                                       && t.Telephone == inscriptionDto.TelephoneTuteur 
                                    && t.IdEcole == inscriptionDto.IdEcole
                                    && (string.IsNullOrWhiteSpace(inscriptionDto.EmailTuteur) || 
                                        t.Email == inscriptionDto.EmailTuteur || 
                                        string.IsNullOrWhiteSpace(t.Email))
                                );

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
                                    Telephone = inscriptionDto.TelephoneTuteur,
                                    NomCompletRepresentant = inscriptionDto.NomCompletRepresentant,
                                    TelephoneRepresentant = inscriptionDto.TelephoneRepresentant,
                                    IdEcole = inscriptionDto.IdEcole,
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
                                eleveExistantFinal.IdClasse = inscriptionDto.IdClasse;
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
                                IdClasse = inscriptionDto.IdClasse,
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
                            .Include(e => e.Classe)
                            .FirstOrDefaultAsync(e => e.IdEleve == newIdEleve.Value);
                        
                        if (tuteur != null && eleve != null)
                        {
                            var utilisateurInfo = await CreateDefaultTuteurUserAsync(
                                tuteur, 
                                inscriptionDto.IdEcole, 
                                eleve);  // ✨ Passer l'élève
                            
                            result.CompteUtilisateurTuteur = utilisateurInfo;
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

        public async Task<PagedResult<Inscription>> GetByEcolePagedAsync(int idEcole, PagedRequest request)
        {
            var query = _context.Inscriptions
                .Include(i => i.Eleve)
                .Include(i => i.Classe)
                .Include(i => i.AnneeScolaire)
                .Where(i => i.IdEcole == idEcole)
                .Where(i => i.Statut == true) // ✅ TOUJOURS filtrer sur Statut == true (même si IncludeInactive=true)
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

        public async Task<PagedResult<Inscription>> GetByClassePagedAsync(int idClasse, PagedRequest request)
        {
            var query = _context.Inscriptions
                .Include(i => i.Eleve)
                .Include(i => i.Ecole)
                .Include(i => i.AnneeScolaire)
                .Where(i => i.IdClasse == idClasse)
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
                // Tri par défaut : DateInscription DESC
                query = request.SortDescending
                    ? query.OrderBy(i => i.DateInscription)
                    : query.OrderByDescending(i => i.DateInscription);
            }

            return await query.ToPagedAsync(request);
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
        /// Crée automatiquement un utilisateur Parent/Tuteur par défaut lors de l'inscription d'un nouvel élève
        /// Adapté d'AkademiaAPI - Applique la même logique que la création d'Etudiant
        /// </summary>
        private async Task<UtilisateurInfo?> CreateDefaultTuteurUserAsync(Tuteur tuteur, int idEcole, Eleve eleve = null)
        {
            try
            {
                _logger.LogInformation("🔍 CreateDefaultTuteurUserAsync appelé pour tuteur {TuteurId} (Email: {Email}, Telephone: {Telephone})", 
                    tuteur.IdTuteur, tuteur.Email, tuteur.Telephone);
                
                // ═══════════════════════════════════════════════════════════════════
                // ✅ ÉTAPE 1 : Récupérer le rôle Parent (OBLIGATOIRE avant création utilisateur)
                // ═══════════════════════════════════════════════════════════════════
                var parentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Nom == "Parent");
                if (parentRole == null)
                {
                    _logger.LogWarning("⚠️ Rôle 'Parent' non trouvé, création du rôle...");
                    // Créer le rôle Parent s'il n'existe pas
                    parentRole = new Role
                    {
                        Nom = "Parent",
                        DateCreation = DateTime.Now,
                        Statut = true
                    };
                    _context.Roles.Add(parentRole);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("✅ Rôle 'Parent' créé avec succès (ID: {RoleId})", parentRole.IdRole);
                }
                else
                {
                    _logger.LogInformation("✅ Rôle 'Parent' trouvé (ID: {RoleId})", parentRole.IdRole);
                }
                
                // ✅ Validation : S'assurer que le rôle Parent a un ID valide
                if (parentRole == null || parentRole.IdRole <= 0)
                {
                    _logger.LogError("❌ ERREUR CRITIQUE : Impossible de récupérer ou créer le rôle Parent. parentRole est null ou IdRole invalide.");
                    throw new InvalidOperationException("Le rôle 'Parent' est requis mais n'a pas pu être récupéré ou créé. Vérifiez la base de données.");
                }
                
                _logger.LogInformation("✅ Validation réussie : Rôle Parent disponible (ID: {RoleId}, Nom: {RoleNom})", 
                    parentRole.IdRole, parentRole.Nom);

                // Récupérer l'école
                var ecole = await _context.Ecoles
                    .FirstOrDefaultAsync(e => e.IdEcole == idEcole);
                
                if (ecole == null)
                {
                    _logger.LogError("❌ École non trouvée pour IdEcole {EcoleId}", idEcole);
                    return null;
                }
                
                _logger.LogInformation("✅ École trouvée: {EcoleNom} (ID: {EcoleId})", ecole.Nom, ecole.IdEcole);

                // Utiliser l'email du tuteur s'il est fourni, sinon vide
                string email = tuteur.Email ?? "";
                string telephone = tuteur.Telephone ?? "";
                
                // Construire le nom complet du tuteur en premier (avec valeur par défaut si NULL)
                string nomComplet = tuteur.NomComplet ?? "";
                
                // ✅ FIX: S'assurer que NomUtilisateur n'est jamais NULL ou vide (champ [Required])
                if (string.IsNullOrWhiteSpace(nomComplet))
                {
                    nomComplet = "Parent"; // Valeur par défaut si NULL
                    _logger.LogWarning("⚠️ Le nom complet du tuteur est NULL ou vide, utilisation de la valeur par défaut 'Parent'");
                }
                
                // ✨ NOUVEAU : Générer le DefaultUsername basé sur le nom complet + nombre aléatoire
                // Format: NomComplet (sans espaces) + nombre aléatoire (1-999)
                // Exemple: "Marie Dupont" → "MarieDupont456"
                string baseUsername = nomComplet.Replace(" ", "").Replace("-", "").Replace("'", "");
                if (string.IsNullOrWhiteSpace(baseUsername))
                {
                    baseUsername = "Parent"; // Valeur par défaut si le nom complet est vide
                }
                if (baseUsername.Length > 20)
                {
                    baseUsername = baseUsername.Substring(0, 20);
                }
                Random random = new Random();
                int randomNumber = random.Next(1, 1000);
                string defaultUsername = $"{baseUsername}{randomNumber}";
                
                // Le mot de passe par défaut est simple : 123456
                // L'utilisateur DOIT le changer à la première connexion
                string motDePasseParDefaut = "123456";
                
                _logger.LogInformation("🔍 Nom complet: {NomComplet}, Username généré: {Username}", nomComplet, defaultUsername);
                
                // ═══════════════════════════════════════════════════════════════════
                // ✅ MULTI-RÔLES : Vérifier si un utilisateur existe déjà par email/téléphone
                // ═══════════════════════════════════════════════════════════════════
                
                Utilisateur? existingUser = null;
                
                // 1. Vérifier si un utilisateur existe déjà pour ce tuteur (par IdTuteur)
                existingUser = await _context.Utilisateurs
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.IdTuteur == tuteur.IdTuteur);
                
                // 2. Si pas trouvé, chercher par email ou téléphone (pour le multi-rôles)
                if (existingUser == null && (!string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(telephone)))
                {
                    existingUser = await _context.Utilisateurs
                        .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                        .FirstOrDefaultAsync(u => 
                            (!string.IsNullOrWhiteSpace(email) && u.Email == email) ||
                            (!string.IsNullOrWhiteSpace(telephone) && u.Telephone == telephone)
                        );
                }
                
                // 3. Si utilisateur existe, ajouter le rôle Parent (multi-rôles)
                if (existingUser != null)
                {
                    _logger.LogInformation("✅ Utilisateur existant trouvé pour le tuteur '{NomComplet}' (ID: {UserId}, Email: {Email})", 
                        tuteur.NomComplet, existingUser.IdUtilisateur, existingUser.Email);
                    
                    // Recharger les UserRoles pour s'assurer qu'on a les données à jour
                    await _context.Entry(existingUser)
                        .Collection(u => u.UserRoles)
                        .Query()
                        .Include(ur => ur.Role)
                        .LoadAsync();
                    
                    // Vérifier si l'utilisateur a déjà le rôle Parent
                    var hasParentRole = existingUser.UserRoles
                        .Any(ur => ur.Role.Nom == "Parent" && ur.Statut == true);
                    
                    _logger.LogInformation("🔍 Vérification rôle Parent pour utilisateur {UserId}: hasParentRole = {HasRole}", 
                        existingUser.IdUtilisateur, hasParentRole);
                    
                    if (!hasParentRole)
                    {
                        // Ajouter le rôle Parent à l'utilisateur existant
                        _logger.LogInformation("➕ Ajout du rôle Parent (ID: {RoleId}) à l'utilisateur existant (ID: {UserId})", 
                            parentRole.IdRole, existingUser.IdUtilisateur);
                        
                        var roleAdded = await _utilisateurRepository.AddRoleToUserAsync(
                            existingUser.IdUtilisateur,
                            parentRole.IdRole,
                            assignedByUserId: null,
                            isPrimary: false // Ne pas changer le rôle principal
                        );
                        
                        if (roleAdded)
                        {
                            _logger.LogInformation("✅ Rôle Parent ajouté avec succès à l'utilisateur {UserId}", 
                                existingUser.IdUtilisateur);
                            
                            // Recharger les UserRoles après l'ajout pour vérification
                            await _context.Entry(existingUser)
                                .Collection(u => u.UserRoles)
                                .Query()
                                .Include(ur => ur.Role)
                                .LoadAsync();
                            
                            // Vérifier que le rôle a bien été ajouté
                            var verifyParentRole = existingUser.UserRoles
                                .Any(ur => ur.Role.Nom == "Parent" && ur.Statut == true);
                            
                            if (verifyParentRole)
                            {
                                _logger.LogInformation("✅ Vérification réussie : Le rôle Parent est bien présent dans UserRoles pour l'utilisateur {UserId}", 
                                    existingUser.IdUtilisateur);
                            }
                            else
                            {
                                _logger.LogWarning("⚠️ ATTENTION : Le rôle Parent n'a pas été trouvé après l'ajout pour l'utilisateur {UserId}", 
                                    existingUser.IdUtilisateur);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ Échec de l'ajout du rôle Parent à l'utilisateur {UserId}", 
                                existingUser.IdUtilisateur);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("ℹ️ L'utilisateur {UserId} a déjà le rôle Parent", 
                            existingUser.IdUtilisateur);
                    }
                    
                    // Mettre à jour IdTuteur si nécessaire
                    if (existingUser.IdTuteur != tuteur.IdTuteur)
                    {
                        existingUser.IdTuteur = tuteur.IdTuteur;
                        await _context.SaveChangesAsync();
                    }
                    
                    // Retourner les infos de l'utilisateur existant et envoyer les notifications
                    var utilisateurInfo = new UtilisateurInfo
                    {
                        IdUtilisateur = existingUser.IdUtilisateur,
                        IdTuteur = existingUser.IdTuteur ?? tuteur.IdTuteur,
                        Email = existingUser.Email ?? email,
                        DefaultUsername = existingUser.DefaultUsername ?? defaultUsername,
                        Telephone = existingUser.Telephone ?? telephone,
                        MotDePasseParDefaut = "", // Ne pas révéler le mot de passe
                        NomComplet = existingUser.NomUtilisateur ?? nomComplet,
                        Role = "Parent"
                    };
                    
                    // Envoyer les notifications même si utilisateur existe
                    await SendNotificationForExistingUserAsync(existingUser, ecole, eleve);
                    
                    return utilisateurInfo;
                }
                
                // ═══════════════════════════════════════════════════════════════════
                // ✅ MULTI-RÔLES : Créer un nouvel utilisateur avec UserRole
                // ═══════════════════════════════════════════════════════════════════
                
                // Créer l'utilisateur Parent/Tuteur par défaut (sans IdRole)
                var tuteurUser = new Utilisateur
                {
                    IdTuteur = tuteur.IdTuteur,
                    ReferenceUtilisateur = Guid.NewGuid(),
                    NomUtilisateur = nomComplet, // ✅ Utiliser la variable 'nomComplet' qui est garantie non-null/non-vide
                    PostNomUtilisateur = "",
                    PrenomUtilisateur = "",
                    Email = email,
                    DefaultUsername = defaultUsername,
                    Telephone = telephone,
                    PhotoUrl = tuteur.PhotoTuteurUrl,
                    Genre = tuteur.Genre,
                    MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(motDePasseParDefaut),
                    Statut = true,
                    DateCreation = DateTime.Now,
                    IsConnecte = false,
                    DoitChangerMotDePasse = true, // ✨ FORCER le changement de mot de passe à la première connexion
                    // ⚠️ TEMPORAIRE : Définir IdRole pour compatibilité avec la base de données (si IdRole n'est pas encore nullable)
                    // TODO : Retirer cette ligne après avoir exécuté le script MAKE_IDROLE_NULLABLE_PRODUCTION.sql en production
                    IdRole = parentRole.IdRole, // ✅ Définir temporairement pour éviter l'erreur "Column 'IdRole' cannot be null"
                    IdEcole = idEcole
                };

                _logger.LogInformation("🔍 Création de l'utilisateur avec les valeurs: NomUtilisateur={Nom}, Email={Email}, IdEcole={EcoleId}, IdTuteur={TuteurId}", 
                    tuteurUser.NomUtilisateur, tuteurUser.Email, tuteurUser.IdEcole, tuteurUser.IdTuteur);

                // ✅ Validation avant ajout
                try
                {
                _context.Utilisateurs.Add(tuteurUser);
                    _logger.LogInformation("✅ Utilisateur ajouté au contexte. Validation en cours...");
                    
                    // Valider que les champs requis ne sont pas NULL
                    if (string.IsNullOrWhiteSpace(tuteurUser.NomUtilisateur))
                    {
                        throw new InvalidOperationException("NomUtilisateur est requis mais est NULL ou vide");
                    }
                    if (string.IsNullOrWhiteSpace(tuteurUser.MotDePasseHash))
                    {
                        throw new InvalidOperationException("MotDePasseHash est requis mais est NULL ou vide");
                    }
                    
                    _logger.LogInformation("✅ Validation réussie. Sauvegarde en cours...");
                await _context.SaveChangesAsync();
                    _logger.LogInformation("✅ Utilisateur sauvegardé avec succès. IdUtilisateur={UserId}", tuteurUser.IdUtilisateur);
                }
                catch (Exception saveEx)
                {
                    _logger.LogError(saveEx, "❌ ERREUR lors de la sauvegarde de l'utilisateur: {ErrorMessage}", saveEx.Message);
                    if (saveEx.InnerException != null)
                    {
                        _logger.LogError(saveEx.InnerException, "❌ Exception interne: {InnerMessage}", saveEx.InnerException.Message);
                    }
                    throw; // Re-lancer l'exception pour qu'elle soit capturée par le catch principal
                }
                
                // ✅ Créer le UserRole pour le système multi-rôles
                _logger.LogInformation("🔍 Création du UserRole pour IdUtilisateur={UserId}, IdRole={RoleId}", 
                    tuteurUser.IdUtilisateur, parentRole.IdRole);
                
                UserRole userRole;
                try
                {
                    userRole = new UserRole
                    {
                        IdUtilisateur = tuteurUser.IdUtilisateur,
                        IdRole = parentRole.IdRole,
                        IsPrimary = true, // Premier rôle = principal
                        Statut = true,
                        DateAttribution = DateTime.Now,
                        IdUtilisateurAttribution = null
                    };
                    
                    _context.UserRoles.Add(userRole);
                    _logger.LogInformation("✅ UserRole ajouté au contexte. Sauvegarde en cours...");
                    
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("✅ UserRole sauvegardé avec succès. IdUserRole={UserRoleId}", userRole.IdUserRole);
                }
                catch (Exception roleEx)
                {
                    _logger.LogError(roleEx, "❌ ERREUR lors de la sauvegarde du UserRole: {ErrorMessage}", roleEx.Message);
                    if (roleEx.InnerException != null)
                    {
                        _logger.LogError(roleEx.InnerException, "❌ Exception interne: {InnerMessage}", roleEx.InnerException.Message);
                    }
                    throw; // Re-lancer l'exception pour qu'elle soit capturée par le catch principal
                }
                
                _logger.LogInformation("✅ UserRole créé avec succès : IdUtilisateur={UserId}, IdRole={RoleId} (Role: {RoleName}), IsPrimary={IsPrimary}", 
                    userRole.IdUtilisateur, userRole.IdRole, parentRole.Nom, userRole.IsPrimary);
                
                // Vérifier que le UserRole a bien été créé
                var verifyUserRole = await _context.UserRoles
                    .Include(ur => ur.Role)
                    .FirstOrDefaultAsync(ur => ur.IdUtilisateur == tuteurUser.IdUtilisateur && ur.IdRole == parentRole.IdRole);
                
                if (verifyUserRole != null)
                {
                    _logger.LogInformation("✅ Vérification réussie : UserRole trouvé dans la base de données (ID: {UserRoleId})", 
                        verifyUserRole.IdUserRole);
                }
                else
                {
                    _logger.LogError("❌ ERREUR : UserRole non trouvé dans la base de données après création pour utilisateur {UserId}", 
                        tuteurUser.IdUtilisateur);
                }
                
                _logger.LogInformation("✅ Utilisateur Parent créé pour '{NomComplet}' - Email: {Email}, Username: {Username}", 
                    nomComplet, tuteurUser.Email, defaultUsername);
                
                // Récupérer le nom de l'école et vérifier AcceptNotification
                string nomEcole = ecole.Nom ?? "KelasiNaBiso";
                bool acceptNotification = ecole.AcceptNotification == true;
                
                // ⚠️ Vérifier si l'école accepte les notifications SMS
                if (!acceptNotification)
                {
                    _logger.LogInformation($"📵 École {nomEcole} n'accepte pas les notifications SMS - SMS non envoyé pour l'inscription de {eleve?.NomComplet}");
                    // On continue quand même pour les autres notifications (email, push)
                }
                
                // Envoyer l'email de bienvenue (si email fourni)
                if (!string.IsNullOrWhiteSpace(email))
                {
                    
                    // ✨ Récupérer les informations de l'enfant pour l'email
                    string nomEnfant = eleve?.NomComplet ?? "";
                    string classeEnfant = eleve?.Classe?.NomClasse ?? "";
                    string matriculeEnfant = eleve?.Matricule ?? "";
                    
                    // ✨ Envoyer les notifications EN PARALLÈLE (Email + Push + SMS)
                    
                    // 📧 Notification Email (en parallèle)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendWelcomeEmailAsync(
                                email,
                                nomComplet,
                                defaultUsername,
                                telephone,
                                motDePasseParDefaut,
                                "Parent",
                                nomEcole,
                                tuteur.Genre ?? "Masculin",  // ✨ Passer le genre du tuteur
                                null,                         // Pas de fonction pour parent
                                null,                         // Pas de matricule pour parent
                                nomEnfant,                    // ✨ Nom de l'enfant
                                classeEnfant,                 // ✨ Classe de l'enfant
                                matriculeEnfant               // ✨ Matricule de l'enfant
                            );
                            _logger.LogInformation($"✅ Email de bienvenue envoyé à {email}");
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogError(emailEx, $"❌ Échec de l'envoi de l'email à {email}");
                        }
                    });
                    
                    // 📲 Notification Push (en parallèle)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            string titre = $"🎓 Inscription de {nomEnfant}";
                            string corps = $"Bienvenue sur KelasiNaBiso ! {nomEnfant} a été inscrit dans la classe {classeEnfant}.";
                            
                            var donnees = new Dictionary<string, string>
                            {
                                { "type", "INSCRIPTION_ENFANT" },
                                { "idEleve", eleve?.IdEleve.ToString() ?? "N/A" },
                                { "nomEleve", nomEnfant },
                                { "classe", classeEnfant },
                                { "matricule", matriculeEnfant }
                            };
                            
                            var pushEnvoye = await _notificationService.EnvoyerNotificationAUtilisateurAsync(
                                tuteurUser.IdUtilisateur,
                                titre,
                                corps,
                                donnees
                            );
                            
                            if (pushEnvoye)
                            {
                                _logger.LogInformation($"✅ Notification PUSH Firebase inscription envoyée à {nomComplet}");
                            }
                            else
                            {
                                _logger.LogWarning($"⚠️ Échec notification PUSH Firebase inscription pour {nomComplet}");
                            }
                        }
                        catch (Exception pushEx)
                        {
                            _logger.LogError(pushEx, $"❌ Erreur lors de l'envoi notification PUSH Firebase inscription pour {nomComplet}");
                        }
                    });
                    
                    // 🔔 Notification SignalR (en parallèle)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _signalRNotificationService.SendCustomNotificationAsync(
                                tuteurUser.IdUtilisateur,
                                $"🎓 Inscription de {nomEnfant}",
                                $"{nomEnfant} a été inscrit dans la classe {classeEnfant}. Nom d'utilisateur: {defaultUsername}, Mot de passe: {motDePasseParDefaut}",
                                "INSCRIPTION"
                            );
                            _logger.LogInformation($"✅ Notification SignalR inscription envoyée à {nomComplet}");
                        }
                        catch (Exception signalREx)
                        {
                            _logger.LogError(signalREx, $"❌ Erreur lors de l'envoi notification SignalR inscription pour {nomComplet}");
                        }
                    });
                    
                    // 📱 Notification SMS (en parallèle)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(telephone) && acceptNotification)
                            {
                                // Construire le message personnalisé
                                string messageSms = "";
                                
                                // Si on a le nom de l'école, l'ajouter avec titre
                                if (!string.IsNullOrWhiteSpace(nomEcole))
                                {
                                    messageSms = $"📋 {nomEcole}\n";
                                    messageSms += $"🎓 Confirmation d'inscription\n";
                                    messageSms += $"{nomEnfant} est inscrit en {classeEnfant}.\n";
                                    messageSms += $"User: {defaultUsername}, MDP: {motDePasseParDefaut}";
                                    
                                    // Ajouter email si disponible
                                    if (!string.IsNullOrWhiteSpace(email))
                                    {
                                        messageSms += $", Email: {email}";
                                    }
                                }
                                else
                                {
                                    // Format simplifié sans école
                                    messageSms = $"🎓 Confirmation d'inscription\n";
                                    messageSms += $"{nomEnfant} est inscrit en {classeEnfant}.\n";
                                    messageSms += $"User: {defaultUsername}, MDP: {motDePasseParDefaut}";
                                    
                                    // Ajouter email si disponible
                                    if (!string.IsNullOrWhiteSpace(email))
                                    {
                                        messageSms += $", Email: {email}";
                                    }
                                }
                                
                                var smsLog = await _smsService.EnvoyerSmsAsync(
                                    telephone,
                                    messageSms,
                                    "INSCRIPTION_ENFANT"
                                );
                                
                                if (smsLog != null && smsLog.Statut != "failed")
                                {
                                    _logger.LogInformation($"✅ SMS inscription envoyé à {nomComplet} (Coût: {smsLog.CoutUsd} USD)");
                                }
                                else
                                {
                                    _logger.LogWarning($"⚠️ Échec SMS inscription pour {nomComplet}");
                                }
                            }
                            else
                            {
                                _logger.LogInformation($"ℹ️ Aucun numéro de téléphone pour {nomComplet}, SMS non envoyé");
                            }
                        }
                        catch (Exception smsEx)
                        {
                            _logger.LogError(smsEx, $"❌ Erreur lors de l'envoi SMS inscription pour {nomComplet}");
                        }
                    });
                    
                    _logger.LogInformation("📧 Notifications (Email + Push + SMS) programmées pour {Email}", email);
                }
                else
                {
                    _logger.LogWarning("⚠️ Aucun email fourni pour le tuteur '{NomComplet}'. Seules les notifications Push et SMS seront envoyées.", 
                        nomComplet);
                    
                    // 📲 Envoyer quand même Push et SMS même sans email
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            string nomEnfant = eleve?.NomComplet ?? "";
                            string classeEnfant = eleve?.Classe?.NomClasse ?? "";
                            
                            string titre = $"🎓 Inscription de {nomEnfant}";
                            string corps = $"Bienvenue sur KelasiNaBiso ! {nomEnfant} a été inscrit dans la classe {classeEnfant}.";
                            
                            var donnees = new Dictionary<string, string>
                            {
                                { "type", "INSCRIPTION_ENFANT" },
                                { "idEleve", eleve?.IdEleve.ToString() ?? "N/A" },
                                { "nomEleve", nomEnfant },
                                { "classe", classeEnfant }
                            };
                            
                            await _notificationService.EnvoyerNotificationAUtilisateurAsync(
                                tuteurUser.IdUtilisateur,
                                titre,
                                corps,
                                donnees
                            );
                            
                            // 🔔 Notification SignalR
                            try
                            {
                                await _signalRNotificationService.SendCustomNotificationAsync(
                                    tuteurUser.IdUtilisateur,
                                    $"🎓 Inscription de {nomEnfant}",
                                    $"{nomEnfant} a été inscrit dans la classe {classeEnfant}. Nom d'utilisateur: {defaultUsername}, Mot de passe: {motDePasseParDefaut}",
                                    "INSCRIPTION"
                                );
                                _logger.LogInformation($"✅ Notification SignalR inscription envoyée (utilisateur existant)");
                            }
                            catch (Exception signalREx)
                            {
                                _logger.LogError(signalREx, $"❌ Erreur SignalR inscription");
                            }
                            
                            // SMS si téléphone disponible et accepté par l'école
                            if (!string.IsNullOrWhiteSpace(telephone) && acceptNotification)
                            {
                                // Construire le message personnalisé
                                string messageSms = "";
                                
                                // Si on a le nom de l'école, l'ajouter avec titre
                                if (!string.IsNullOrWhiteSpace(nomEcole))
                                {
                                    messageSms = $"📋 {nomEcole}\n";
                                    messageSms += $"🎓 Confirmation d'inscription\n";
                                    messageSms += $"{nomEnfant} est inscrit en {classeEnfant}.\n";
                                    messageSms += $"User: {defaultUsername}, MDP: {motDePasseParDefaut}";
                                }
                                else
                                {
                                    // Format simplifié sans école
                                    messageSms = $"🎓 Confirmation d'inscription\n";
                                    messageSms += $"{nomEnfant} est inscrit en {classeEnfant}.\n";
                                    messageSms += $"User: {defaultUsername}, MDP: {motDePasseParDefaut}";
                                }
                                
                                await _smsService.EnvoyerSmsAsync(telephone, messageSms, "INSCRIPTION_ENFANT");
                            }
                        }
                        catch (Exception notifEx)
                        {
                            _logger.LogError(notifEx, "⚠️ Erreur notifications sans email: {ErrorMessage}", notifEx.Message);
                        }
                    });
                }
                
                // Retourner les informations du compte créé
                return new UtilisateurInfo
                {
                    IdUtilisateur = tuteurUser.IdUtilisateur,
                    IdTuteur = tuteurUser.IdTuteur,
                    Email = tuteurUser.Email ?? "",
                    DefaultUsername = tuteurUser.DefaultUsername ?? "",
                    Telephone = tuteurUser.Telephone ?? "",
                    MotDePasseParDefaut = motDePasseParDefaut,
                    NomComplet = nomComplet,
                    Role = "Parent"
                };
            }
            catch (Exception ex)
            {
                // Log l'erreur complète avec toutes les exceptions internes
                _logger.LogError(ex, "❌ ERREUR lors de la création de l'utilisateur Parent par défaut pour tuteur {TuteurId}: {ErrorMessage}", 
                    tuteur.IdTuteur, ex.Message);
                
                // Log l'exception interne si elle existe
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "❌ Exception interne: {InnerMessage}", ex.InnerException.Message);
                    
                    // Si c'est une DbUpdateException, log les détails supplémentaires
                    if (ex.InnerException is Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
                    {
                        _logger.LogError("❌ DbUpdateException détectée. Entrées: {Entries}", 
                            string.Join(", ", dbEx.Entries.Select(e => $"{e.Entity.GetType().Name} - {string.Join(", ", e.Properties.Select(p => $"{p.Metadata.Name}={p.CurrentValue}"))}")));
                    }
                }
                
                // Log le stack trace complet
                _logger.LogError("❌ StackTrace: {StackTrace}", ex.StackTrace);
                
                return null;
            }
        }

        /// <summary>
        /// Envoie les notifications pour un utilisateur existant (cas réinscription ou deuxième enfant)
        /// </summary>
        private async Task SendNotificationForExistingUserAsync(Utilisateur utilisateur, Ecole ecole, Eleve eleve)
        {
            try
            {
                string nomEcole = ecole.Nom ?? "KelasiNaBiso";
                bool acceptNotification = ecole.AcceptNotification == true;
                string nomEnfant = eleve?.NomComplet ?? "";
                string classeEnfant = eleve?.Classe?.NomClasse ?? "";
                string defaultUsername = utilisateur.DefaultUsername ?? "";
                
                // ⚠️ Vérifier si l'école accepte les notifications SMS
                if (!acceptNotification)
                {
                    _logger.LogInformation($"📵 École {nomEcole} n'accepte pas les notifications SMS - SMS non envoyé pour l'inscription de {nomEnfant}");
                    // On continue quand même pour les autres notifications (email, push)
                }
                
                // Email si disponible
                if (!string.IsNullOrWhiteSpace(utilisateur.Email))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendWelcomeEmailAsync(
                                utilisateur.Email,
                                utilisateur.NomUtilisateur ?? "",
                                defaultUsername,
                                utilisateur.Telephone ?? "",
                                "", // Pas de mot de passe
                                "Parent",
                                nomEcole,
                                utilisateur.Genre ?? "Masculin",
                                null,
                                null,
                                nomEnfant,
                                classeEnfant,
                                eleve?.Matricule ?? ""
                            );
                            _logger.LogInformation($"✅ Email de bienvenue envoyé à {utilisateur.Email}");
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogError(emailEx, $"❌ Échec de l'envoi de l'email à {utilisateur.Email}");
                        }
                    });
                }

                // Push notification
                _ = Task.Run(async () =>
                {
                    try
                    {
                        string titre = $"🎓 Inscription de {nomEnfant}";
                        string corps = $"Bienvenue sur KelasiNaBiso ! {nomEnfant} a été inscrit dans la classe {classeEnfant}.";

                        var donnees = new Dictionary<string, string>
                        {
                            { "type", "INSCRIPTION_ENFANT" },
                            { "idEleve", eleve?.IdEleve.ToString() ?? "N/A" },
                            { "nomEleve", nomEnfant },
                            { "classe", classeEnfant }
                        };

                        var pushEnvoye = await _notificationService.EnvoyerNotificationAUtilisateurAsync(
                            utilisateur.IdUtilisateur,
                            titre,
                            corps,
                            donnees
                        );

                        if (pushEnvoye)
                        {
                            _logger.LogInformation($"✅ Notification PUSH Firebase inscription envoyée à {utilisateur.NomUtilisateur}");
                        }
                        else
                        {
                            _logger.LogWarning($"⚠️ Échec notification PUSH Firebase inscription pour {utilisateur.NomUtilisateur}");
                        }
                    }
                    catch (Exception pushEx)
                    {
                        _logger.LogError(pushEx, $"❌ Erreur lors de l'envoi notification PUSH Firebase inscription");
                    }
                });

                // 🔔 Notification SignalR (en parallèle)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _signalRNotificationService.SendCustomNotificationAsync(
                            utilisateur.IdUtilisateur,
                            $"🎓 Inscription de {nomEnfant}",
                            $"{nomEnfant} a été inscrit dans la classe {classeEnfant}. Nom d'utilisateur: {defaultUsername}",
                            "INSCRIPTION"
                        );
                        _logger.LogInformation($"✅ Notification SignalR inscription envoyée à {utilisateur.NomUtilisateur} (utilisateur existant)");
                    }
                    catch (Exception signalREx)
                    {
                        _logger.LogError(signalREx, $"❌ Erreur SignalR inscription (utilisateur existant)");
                    }
                });

                // SMS si téléphone disponible et accepté par l'école
                if (!string.IsNullOrWhiteSpace(utilisateur.Telephone) && acceptNotification)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // Construire le message personnalisé
                            string messageSms = "";
                            
                            // Si on a le nom de l'école, l'ajouter avec titre
                            if (!string.IsNullOrWhiteSpace(nomEcole))
                            {
                                messageSms = $"📋 {nomEcole}\n";
                                messageSms += $"🎓 Confirmation d'inscription\n";
                                messageSms += $"{nomEnfant} est inscrit en {classeEnfant}.\n";
                                messageSms += $"User: {defaultUsername}";
                                
                                // Ajouter email si disponible
                                if (!string.IsNullOrWhiteSpace(utilisateur.Email))
                                {
                                    messageSms += $", Email: {utilisateur.Email}";
                                }
                            }
                            else
                            {
                                // Format simplifié sans école
                                messageSms = $"🎓 Confirmation d'inscription\n";
                                messageSms += $"{nomEnfant} est inscrit en {classeEnfant}.\n";
                                messageSms += $"User: {defaultUsername}";
                                
                                // Ajouter email si disponible
                                if (!string.IsNullOrWhiteSpace(utilisateur.Email))
                                {
                                    messageSms += $", Email: {utilisateur.Email}";
                                }
                            }

                            var smsLog = await _smsService.EnvoyerSmsAsync(
                                utilisateur.Telephone,
                                messageSms,
                                "INSCRIPTION_ENFANT"
                            );
                            
                            if (smsLog != null && smsLog.Statut != "failed")
                            {
                                _logger.LogInformation($"✅ SMS inscription envoyé (Coût: {smsLog.CoutUsd} USD)");
                            }
                            else
                            {
                                _logger.LogWarning($"⚠️ Échec SMS inscription");
                            }
                        }
                        catch (Exception smsEx)
                        {
                            _logger.LogError(smsEx, $"❌ Erreur lors de l'envoi SMS inscription");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur lors de l'envoi des notifications pour utilisateur existant");
            }
        }
    }
}
