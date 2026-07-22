using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace KelasiNaBiso.Services
{
    /// <summary>
    /// Service optimisé pour traiter les fichiers Excel d'inscriptions (Version 2)
    /// Utilise le chargement en mémoire des classes et années scolaires pour optimiser les performances
    /// Format simplifié : nomClasse et libelleAnneeScolaire au lieu d'IDs
    /// </summary>
    public class ExcelInscriptionServiceV2
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IInscriptionRepository _inscriptionService;
        private readonly ILogger<ExcelInscriptionServiceV2> _logger;
        private const int BATCH_SIZE = 50; // Traiter 50 inscriptions à la fois
        private const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10 MB maximum

        // Colonnes attendues dans le nouveau format Excel simplifié
        // Colonnes supprimées : Type, NomClasse, LibelleAnneeScolaire (passées comme paramètres)
        // Colonnes supprimées précédemment : IdEcole, IdClasse, IdAnneeScolaire, IdTuteurExistant, IdEleveExistant, PieceIdentiteTuteur, PhotoTuteurUrl, StatutInscription
        private static readonly string[] REQUIRED_COLUMNS = {
            "DateInscription",
            "NomEleve", "PostnomEleve", "PrenomEleve", "GenreEleve", "DateNaissanceEleve",
            "LieuNaissanceEleve", "NationaliteEleve",
            "NomCompletTuteur", "GenreTuteur"
        };

        public ExcelInscriptionServiceV2(
            KelasiNaBisoDbContext context,
            IInscriptionRepository inscriptionService,
            ILogger<ExcelInscriptionServiceV2> logger)
        {
            _context = context;
            _inscriptionService = inscriptionService;
            _logger = logger;
            
            // Configurer EPPlus pour la licence non-commerciale
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Traite un fichier Excel d'inscriptions avec le nouveau format simplifié
        /// </summary>
        /// <param name="file">Fichier Excel à traiter</param>
        /// <param name="idEcole">ID de l'école</param>
        /// <param name="idUtilisateur">ID de l'utilisateur effectuant l'import (extrait du token JWT côté contrôleur)</param>
        /// <param name="idClasse">ID de la classe (fourni par le frontend)</param>
        /// <param name="idAnneeScolaire">ID de l'année scolaire (fourni par le frontend)</param>
        /// <param name="typeInscription">Type d'inscription (fourni par le frontend, ex: "Inscription", "Réinscription")</param>
        public async Task<BulkInscriptionResult> ProcessExcelFileAsync(
            IFormFile file, 
            int idEcole, 
            int idUtilisateur,
            int idClasse,
            int idAnneeScolaire,
            string typeInscription = "Inscription")
        {
            var result = new BulkInscriptionResult();

            try
            {
                // 1. Validation du fichier
                var fileValidation = ValidateFile(file);
                if (!fileValidation.Success)
                {
                    result.Success = false;
                    result.Message = fileValidation.Message;
                    return result;
                }

                // 2. Valider que la classe et l'année scolaire appartiennent bien à l'école
                _logger.LogInformation($"🔍 Validation de la classe {idClasse} et de l'année scolaire {idAnneeScolaire} pour l'école {idEcole}...");
                
                var classeValide = await _context.Classes
                    .Include(c => c.Direction)
                    .AnyAsync(c => c.IdClasse == idClasse && c.Direction.IdEcole == idEcole && c.Statut == true);
                
                if (!classeValide)
                {
                    result.Success = false;
                    result.Message = $"La classe {idClasse} n'existe pas ou n'appartient pas à l'école {idEcole}.";
                    return result;
                }

                var anneeScolaireValide = await _context.AnneeScolaires
                    .AnyAsync(a => a.IdAnneeScolaire == idAnneeScolaire && a.IdEcole == idEcole && a.Statut == true);
                
                if (!anneeScolaireValide)
                {
                    result.Success = false;
                    result.Message = $"L'année scolaire {idAnneeScolaire} n'existe pas ou n'appartient pas à l'école {idEcole}.";
                    return result;
                }

                _logger.LogInformation($"✅ Classe {idClasse} et année scolaire {idAnneeScolaire} validées");

                // 3. Valider le type d'inscription
                var typesValides = new[] { "Inscription", "Réinscription", "Transfert" };
                if (!typesValides.Contains(typeInscription, StringComparer.OrdinalIgnoreCase))
                {
                    result.Success = false;
                    result.Message = $"Type d'inscription invalide : '{typeInscription}'. Types valides : {string.Join(", ", typesValides)}.";
                    return result;
                }

                // 4. Lire et parser le fichier Excel
                var inscriptionsRaw = await ReadExcelFileAsync(file);
                result.TotalLignes = inscriptionsRaw.Count;

                if (inscriptionsRaw.Count == 0)
                {
                    result.Success = false;
                    result.Message = "Le fichier Excel est vide ou ne contient pas de données valides.";
                    return result;
                }

                // 5. Convertir en InscriptionExcelDto et enrichir avec les IDs fournis
                var inscriptionsExcel = ConvertToInscriptionExcelDto(inscriptionsRaw, idEcole, idClasse, idAnneeScolaire, typeInscription);

                // 6. Validation des données
                ValidateInscriptions(inscriptionsExcel);

                // 7. Déduplication dans le fichier
                DeduplicateInFile(inscriptionsExcel, result);

                // 8. Séparer les lignes valides et invalides
                var lignesValides = inscriptionsExcel.Where(i => i.Erreurs.Count == 0).ToList();
                var lignesInvalides = inscriptionsExcel.Where(i => i.Erreurs.Count > 0).ToList();

                result.LignesAvecErreurs = lignesInvalides;
                result.LignesEchouees = lignesInvalides.Count;

                // 9. Traitement par lots des lignes valides
                if (lignesValides.Count > 0)
                {
                    await ProcessBatchesAsync(lignesValides, idEcole, result);
                }

                // 9. Générer le message de résultat
                result.Success = result.LignesReussies > 0;
                result.Message = GenerateResultMessage(result);

                _logger.LogInformation($"✅ Traitement Excel terminé : {result.LignesReussies}/{result.TotalLignes} réussies");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du traitement du fichier Excel");
                result.Success = false;
                result.Message = $"Erreur lors du traitement : {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Classe intermédiaire pour stocker les données Excel avant enrichissement
        /// </summary>
        private class InscriptionExcelRaw
        {
            public int NumeroLigne { get; set; }
            public DateTime? DateInscription { get; set; }
            public string NomEleve { get; set; } = string.Empty;
            public string PostnomEleve { get; set; } = string.Empty;
            public string PrenomEleve { get; set; } = string.Empty;
            public string GenreEleve { get; set; } = string.Empty;
            public DateTime? DateNaissanceEleve { get; set; }
            public string LieuNaissanceEleve { get; set; } = string.Empty;
            public string NationaliteEleve { get; set; } = string.Empty;
            public string? ProvinceEleve { get; set; }
            public string? VilleEleve { get; set; }
            public string? CommuneEleve { get; set; }
            public string? QuartierEleve { get; set; }
            public string? AvenueEleve { get; set; }
            public string? NumeroEleve { get; set; }
            public string? CommentaireEleve { get; set; }
            public string? PhotoEleveUrl { get; set; }
            public string? MatriculeEleve { get; set; }
            public string NomCompletTuteur { get; set; } = string.Empty;
            public string GenreTuteur { get; set; } = string.Empty;
            public string? EmailTuteur { get; set; }
            public string? TelephoneTuteur { get; set; }
            public string? NomCompletRepresentant { get; set; }
            public string? TelephoneRepresentant { get; set; }
            // Colonnes supprimées : Type, NomClasse, LibelleAnneeScolaire (passées comme paramètres)
        }

        /// <summary>
        /// Charge toutes les classes actives d'une école en mémoire
        /// </summary>
        private async Task<Dictionary<string, int>> LoadClassesByEcoleAsync(int idEcole)
        {
            var classes = await _context.Classes
                .Include(c => c.Direction)
                .Where(c => c.Direction.IdEcole == idEcole && c.Statut == true)
                .Select(c => new { c.IdClasse, c.NomClasse })
                .ToListAsync();

            // Créer un dictionnaire pour recherche rapide (insensible à la casse et espaces)
            var dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var classe in classes)
            {
                var nomNormalise = NormalizeName(classe.NomClasse);
                if (!dictionary.ContainsKey(nomNormalise))
                {
                    dictionary[nomNormalise] = classe.IdClasse;
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Charge toutes les années scolaires actives d'une école en mémoire
        /// </summary>
        private async Task<Dictionary<string, int>> LoadAnneeScolairesByEcoleAsync(int idEcole)
        {
            var anneesScolaires = await _context.AnneeScolaires
                .Where(a => a.IdEcole == idEcole && a.Statut == true)
                .Select(a => new { a.IdAnneeScolaire, a.LibelleAnneeScolaire })
                .ToListAsync();

            // Créer un dictionnaire pour recherche rapide (insensible à la casse et espaces)
            var dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var annee in anneesScolaires)
            {
                var libelleNormalise = NormalizeName(annee.LibelleAnneeScolaire);
                if (!dictionary.ContainsKey(libelleNormalise))
                {
                    dictionary[libelleNormalise] = annee.IdAnneeScolaire;
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Normalise un nom pour la comparaison (supprime espaces multiples, casse, accents)
        /// </summary>
        private string NormalizeName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            
            // Supprimer les espaces multiples et trim
            var normalized = Regex.Replace(name.Trim(), @"\s+", " ");
            return normalized.ToUpperInvariant();
        }

        /// <summary>
        /// Convertit les données brutes en InscriptionExcelDto et enrichit avec les IDs
        /// </summary>
        private List<InscriptionExcelDto> ConvertToInscriptionExcelDto(
            List<InscriptionExcelRaw> inscriptionsRaw,
            int idEcole,
            int idClasse,
            int idAnneeScolaire,
            string typeInscription)
        {
            var inscriptionsExcel = new List<InscriptionExcelDto>();

            foreach (var raw in inscriptionsRaw)
            {
                var inscription = new InscriptionExcelDto
                {
                    NumeroLigne = raw.NumeroLigne,
                    Type = typeInscription, // Utiliser le type fourni par le frontend
                    IdEcole = idEcole,
                    IdClasse = idClasse, // Utiliser directement l'ID fourni par le frontend
                    IdAnneeScolaire = idAnneeScolaire, // Utiliser directement l'ID fourni par le frontend
                    DateInscription = raw.DateInscription ?? DateTime.Now,
                    StatutInscription = "En attente", // Défini par défaut
                    NomEleve = raw.NomEleve,
                    PostnomEleve = raw.PostnomEleve,
                    PrenomEleve = raw.PrenomEleve,
                    GenreEleve = raw.GenreEleve,
                    DateNaissanceEleve = raw.DateNaissanceEleve,
                    LieuNaissanceEleve = raw.LieuNaissanceEleve,
                    NationaliteEleve = raw.NationaliteEleve,
                    ProvinceEleve = raw.ProvinceEleve,
                    VilleEleve = raw.VilleEleve,
                    CommuneEleve = raw.CommuneEleve,
                    QuartierEleve = raw.QuartierEleve,
                    AvenueEleve = raw.AvenueEleve,
                    NumeroEleve = raw.NumeroEleve,
                    CommentaireEleve = raw.CommentaireEleve,
                    PhotoEleveUrl = raw.PhotoEleveUrl,
                    MatriculeEleve = raw.MatriculeEleve,
                    NomCompletTuteur = raw.NomCompletTuteur,
                    GenreTuteur = raw.GenreTuteur,
                    EmailTuteur = raw.EmailTuteur,
                    TelephoneTuteur = raw.TelephoneTuteur,
                    NomCompletRepresentant = raw.NomCompletRepresentant,
                    TelephoneRepresentant = raw.TelephoneRepresentant
                    // Colonnes supprimées : PhotoTuteurUrl, PieceIdentiteTuteur, IdTuteurExistant, IdEleveExistant
                    // Colonnes supprimées : Type, NomClasse, LibelleAnneeScolaire (passées comme paramètres)
                };

                inscriptionsExcel.Add(inscription);
            }

            return inscriptionsExcel;
        }

        /// <summary>
        /// Valide le fichier Excel uploadé
        /// </summary>
        private (bool Success, string Message) ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "Le fichier est vide ou n'a pas été fourni");
            }

            if (file.Length > MAX_FILE_SIZE)
            {
                return (false, $"Le fichier dépasse la taille maximale autorisée ({MAX_FILE_SIZE / 1024 / 1024} MB)");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xlsx")
            {
                return (false, "Le fichier doit être au format .xlsx (Excel 2007+)");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Lit le fichier Excel et extrait les données
        /// </summary>
        private async Task<List<InscriptionExcelRaw>> ReadExcelFileAsync(IFormFile file)
        {
            var inscriptions = new List<InscriptionExcelRaw>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    if (package.Workbook.Worksheets.Count == 0)
                    {
                        _logger.LogWarning("⚠️ Le fichier Excel ne contient aucune feuille de calcul");
                        throw new InvalidOperationException("Le fichier Excel ne contient aucune feuille de calcul");
                    }

                    var worksheet = package.Workbook.Worksheets[0];
                    
                    if (worksheet.Dimension == null)
                    {
                        _logger.LogWarning("⚠️ La feuille de calcul est vide");
                        throw new InvalidOperationException("La feuille de calcul est vide");
                    }

                    var rowCount = worksheet.Dimension.Rows;
                    var colCount = worksheet.Dimension.Columns;

                    _logger.LogInformation($"📊 Fichier Excel : {rowCount} lignes, {colCount} colonnes");

                    if (rowCount < 2)
                    {
                        _logger.LogWarning("⚠️ Le fichier Excel doit contenir au moins une ligne d'en-tête et une ligne de données");
                        return inscriptions;
                    }

                    // Vérifier les en-têtes
                    var headers = new Dictionary<string, int>();
                    _logger.LogInformation($"📋 Lecture des en-têtes : {colCount} colonnes détectées");
                    for (int col = 1; col <= colCount; col++)
                    {
                        try
                        {
                            var headerValue = worksheet.Cells[1, col].Text?.Trim();
                            if (!string.IsNullOrWhiteSpace(headerValue))
                            {
                                headers[headerValue] = col;
                                _logger.LogDebug($"   Colonne {col}: '{headerValue}'");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"⚠️ Erreur lors de la lecture de la colonne {col}: {ex.Message}");
                        }
                    }

                    _logger.LogInformation($"✅ {headers.Count} en-têtes trouvés");

                    // Vérifier que toutes les colonnes requises sont présentes
                    var missingColumns = REQUIRED_COLUMNS.Where(c => !headers.ContainsKey(c)).ToList();
                    if (missingColumns.Any())
                    {
                        var errorMsg = $"Colonnes manquantes dans le fichier Excel : {string.Join(", ", missingColumns)}. Colonnes attendues : {string.Join(", ", REQUIRED_COLUMNS)}. Colonnes trouvées : {string.Join(", ", headers.Keys)}";
                        _logger.LogWarning($"⚠️ {errorMsg}");
                        throw new InvalidOperationException(errorMsg);
                    }

                    // Lire les données (lignes 2 à rowCount)
                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            // Vérifier que la ligne n'est pas vide
                            var hasData = false;
                            foreach (var colName in REQUIRED_COLUMNS)
                            {
                                var cellValue = GetCellValue(worksheet, row, headers, colName);
                                if (!string.IsNullOrWhiteSpace(cellValue))
                                {
                                    hasData = true;
                                    break;
                                }
                            }

                            if (!hasData)
                            {
                                _logger.LogDebug($"Ligne {row} vide, ignorée");
                                continue;
                            }

                            var inscription = new InscriptionExcelRaw
                            {
                                NumeroLigne = row,
                                // Type, NomClasse, et LibelleAnneeScolaire sont maintenant fournis par le frontend
                                DateInscription = ParseDate(GetCellObject(worksheet, row, headers, "DateInscription")),
                                NomEleve = GetCellValue(worksheet, row, headers, "NomEleve") ?? string.Empty,
                                PostnomEleve = GetCellValue(worksheet, row, headers, "PostnomEleve") ?? string.Empty,
                                PrenomEleve = GetCellValue(worksheet, row, headers, "PrenomEleve") ?? string.Empty,
                                GenreEleve = GetCellValue(worksheet, row, headers, "GenreEleve") ?? string.Empty,
                                DateNaissanceEleve = ParseDate(GetCellObject(worksheet, row, headers, "DateNaissanceEleve")),
                                LieuNaissanceEleve = GetCellValue(worksheet, row, headers, "LieuNaissanceEleve") ?? string.Empty,
                                NationaliteEleve = GetCellValue(worksheet, row, headers, "NationaliteEleve") ?? string.Empty,
                                NomCompletTuteur = GetCellValue(worksheet, row, headers, "NomCompletTuteur") ?? string.Empty,
                                GenreTuteur = GetCellValue(worksheet, row, headers, "GenreTuteur") ?? string.Empty
                            };

                            // Colonnes optionnelles
                            if (headers.ContainsKey("ProvinceEleve"))
                                inscription.ProvinceEleve = GetCellValue(worksheet, row, headers, "ProvinceEleve");
                            if (headers.ContainsKey("VilleEleve"))
                                inscription.VilleEleve = GetCellValue(worksheet, row, headers, "VilleEleve");
                            if (headers.ContainsKey("CommuneEleve"))
                                inscription.CommuneEleve = GetCellValue(worksheet, row, headers, "CommuneEleve");
                            if (headers.ContainsKey("QuartierEleve"))
                                inscription.QuartierEleve = GetCellValue(worksheet, row, headers, "QuartierEleve");
                            if (headers.ContainsKey("AvenueEleve"))
                                inscription.AvenueEleve = GetCellValue(worksheet, row, headers, "AvenueEleve");
                            if (headers.ContainsKey("NumeroEleve"))
                                inscription.NumeroEleve = GetCellValue(worksheet, row, headers, "NumeroEleve");
                            if (headers.ContainsKey("CommentaireEleve"))
                                inscription.CommentaireEleve = GetCellValue(worksheet, row, headers, "CommentaireEleve");
                            if (headers.ContainsKey("PhotoEleveUrl"))
                                inscription.PhotoEleveUrl = GetCellValue(worksheet, row, headers, "PhotoEleveUrl");
                            if (headers.ContainsKey("MatriculeEleve"))
                                inscription.MatriculeEleve = GetCellValue(worksheet, row, headers, "MatriculeEleve");
                            if (headers.ContainsKey("EmailTuteur"))
                                inscription.EmailTuteur = GetCellValue(worksheet, row, headers, "EmailTuteur");
                            if (headers.ContainsKey("TelephoneTuteur"))
                                inscription.TelephoneTuteur = GetCellValue(worksheet, row, headers, "TelephoneTuteur");
                            if (headers.ContainsKey("NomCompletRepresentant"))
                                inscription.NomCompletRepresentant = GetCellValue(worksheet, row, headers, "NomCompletRepresentant");
                            if (headers.ContainsKey("TelephoneRepresentant"))
                                inscription.TelephoneRepresentant = GetCellValue(worksheet, row, headers, "TelephoneRepresentant");

                            inscriptions.Add(inscription);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"⚠️ Erreur ligne {row}: {ex.Message}");
                        }
                    }
                }
            }

            return inscriptions;
        }

        /// <summary>
        /// Helper pour lire une valeur de cellule de manière sécurisée
        /// </summary>
        private string? GetCellValue(OfficeOpenXml.ExcelWorksheet worksheet, int row, Dictionary<string, int> headers, string columnName)
        {
            if (!headers.ContainsKey(columnName))
                return null;

            try
            {
                var col = headers[columnName];
                if (col < 1 || col > worksheet.Dimension?.Columns)
                    return null;

                return worksheet.Cells[row, col].Text?.Trim();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Helper pour lire un objet de cellule de manière sécurisée
        /// </summary>
        private object? GetCellObject(OfficeOpenXml.ExcelWorksheet worksheet, int row, Dictionary<string, int> headers, string columnName)
        {
            if (!headers.ContainsKey(columnName))
                return null;

            try
            {
                var col = headers[columnName];
                if (col < 1 || col > worksheet.Dimension?.Columns)
                    return null;

                return worksheet.Cells[row, col].Value;
            }
            catch
            {
                return null;
            }
        }

        private DateTime? ParseDate(object? value)
        {
            if (value == null) return null;

            if (value is DateTime dt)
                return dt;

            if (value is double dbl)
                return DateTime.FromOADate(dbl);

            if (DateTime.TryParse(value.ToString(), out DateTime result))
                return result;

            return null;
        }

        /// <summary>
        /// Valide les données des inscriptions
        /// </summary>
        private void ValidateInscriptions(List<InscriptionExcelDto> inscriptions)
        {
            foreach (var inscription in inscriptions)
            {
                if (string.IsNullOrWhiteSpace(inscription.NomEleve))
                {
                    inscription.Erreurs.Add("Le nom de l'élève est obligatoire");
                }

                if (string.IsNullOrWhiteSpace(inscription.PostnomEleve))
                {
                    inscription.Erreurs.Add("Le postnom de l'élève est obligatoire");
                }

                if (string.IsNullOrWhiteSpace(inscription.PrenomEleve))
                {
                    inscription.Erreurs.Add("Le prénom de l'élève est obligatoire");
                }

                if (string.IsNullOrWhiteSpace(inscription.GenreEleve) || !new[] { "M", "F" }.Contains(inscription.GenreEleve.ToUpper()))
                {
                    inscription.Erreurs.Add("Le genre de l'élève doit être M ou F");
                }

                if (inscription.DateNaissanceEleve == null)
                {
                    inscription.Erreurs.Add("La date de naissance de l'élève est obligatoire");
                }

                if (string.IsNullOrWhiteSpace(inscription.LieuNaissanceEleve))
                {
                    inscription.Erreurs.Add("Le lieu de naissance de l'élève est obligatoire");
                }

                if (string.IsNullOrWhiteSpace(inscription.NationaliteEleve))
                {
                    inscription.Erreurs.Add("La nationalité de l'élève est obligatoire");
                }

                if (string.IsNullOrWhiteSpace(inscription.NomCompletTuteur))
                {
                    inscription.Erreurs.Add("Le nom complet du tuteur est obligatoire");
                }

                if (string.IsNullOrWhiteSpace(inscription.GenreTuteur) || !new[] { "M", "F" }.Contains(inscription.GenreTuteur.ToUpper()))
                {
                    inscription.Erreurs.Add("Le genre du tuteur doit être M ou F");
                }

                // Les erreurs de classe/année scolaire ont déjà été ajoutées dans ConvertToInscriptionExcelDto
            }
        }

        /// <summary>
        /// Détecte et marque les doublons dans le fichier
        /// </summary>
        private void DeduplicateInFile(List<InscriptionExcelDto> inscriptions, BulkInscriptionResult result)
        {
            var seen = new HashSet<string>();

            foreach (var inscription in inscriptions)
            {
                if (inscription.DateNaissanceEleve == null || string.IsNullOrWhiteSpace(inscription.NomEleve) || 
                    string.IsNullOrWhiteSpace(inscription.PostnomEleve) || string.IsNullOrWhiteSpace(inscription.PrenomEleve))
                    continue;

                var key = $"{inscription.NomEleve}_{inscription.PostnomEleve}_{inscription.PrenomEleve}_{inscription.DateNaissanceEleve:yyyy-MM-dd}_{inscription.IdClasse}";

                if (seen.Contains(key))
                {
                    inscription.Erreurs.Add("Doublon détecté dans le fichier");
                    result.DoublonsDetectes++;
                }
                else
                {
                    seen.Add(key);
                }
            }
        }

        /// <summary>
        /// Traite les inscriptions par lots
        /// </summary>
        private async Task ProcessBatchesAsync(List<InscriptionExcelDto> lignesValides, int idEcole, BulkInscriptionResult result)
        {
            var batches = lignesValides
                .Select((inscription, index) => new { inscription, index })
                .GroupBy(x => x.index / BATCH_SIZE)
                .Select(g => g.Select(x => x.inscription).ToList())
                .ToList();

            _logger.LogInformation($"📦 Traitement de {batches.Count} lot(s) de {BATCH_SIZE} inscriptions max");

            foreach (var batch in batches)
            {
                // Note: CreateInscriptionAsync gère déjà ses propres transactions
                // Pas besoin de transaction ici pour éviter les conflits
                foreach (var inscriptionDto in batch)
                {
                    try
                    {
                        var dto = inscriptionDto.ToCreateInscriptionDto();
                        
                        // Générer le matricule si nécessaire
                        if (string.IsNullOrWhiteSpace(dto.MatriculeEleve))
                        {
                            var ecole = await _context.Ecoles.FindAsync(idEcole);
                            var nomEcole = ecole?.Nom ?? $"Ecole{idEcole}";
                            dto.MatriculeEleve = _inscriptionService.GenerateMatriculeEleve(nomEcole, dto);
                        }

                        // Créer l'inscription (gère sa propre transaction)
                        var inscriptionResult = await _inscriptionService.CreateInscriptionAsync(dto);

                        if (inscriptionResult.Success)
                        {
                            result.InscriptionsCrees.Add(inscriptionResult);
                            result.LignesReussies++;
                            _logger.LogDebug($"✅ Inscription créée ligne {inscriptionDto.NumeroLigne}");
                        }
                        else
                        {
                            inscriptionDto.Erreurs.Add(inscriptionResult.Message);
                            result.LignesAvecErreurs.Add(inscriptionDto);
                            result.LignesEchouees++;
                            _logger.LogWarning($"⚠️ Échec inscription ligne {inscriptionDto.NumeroLigne}: {inscriptionResult.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"❌ Erreur lors de la création de l'inscription ligne {inscriptionDto.NumeroLigne}");
                        inscriptionDto.Erreurs.Add($"Erreur : {ex.Message}");
                        result.LignesAvecErreurs.Add(inscriptionDto);
                        result.LignesEchouees++;
                    }
                }

                _logger.LogInformation($"✅ Lot traité : {batch.Count(b => b.Erreurs.Count == 0)}/{batch.Count} inscriptions créées");
            }
        }

        /// <summary>
        /// Génère le message de résultat
        /// </summary>
        private string GenerateResultMessage(BulkInscriptionResult result)
        {
            if (result.LignesReussies == result.TotalLignes)
            {
                return $"Traitement terminé : {result.LignesReussies} inscription(s) réussie(s) sur {result.TotalLignes} ligne(s)";
            }
            else if (result.LignesReussies == 0)
            {
                return $"Aucune inscription créée : {result.LignesEchouees} erreur(s) sur {result.TotalLignes} ligne(s)";
            }
            else
            {
                return $"Traitement terminé : {result.LignesReussies} inscription(s) réussie(s) sur {result.TotalLignes} ligne(s), {result.LignesEchouees} échouée(s)";
            }
        }

        /// <summary>
        /// Génère un fichier Excel template pour l'import
        /// </summary>
        public byte[] GenerateExcelTemplate()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Inscriptions");

                // En-têtes (colonnes requises)
                // Note : Type, NomClasse, et LibelleAnneeScolaire sont maintenant fournis par le frontend
                var requiredHeaders = new[] { 
                    "DateInscription",
                    "NomEleve", "PostnomEleve", "PrenomEleve", "GenreEleve", "DateNaissanceEleve",
                    "LieuNaissanceEleve", "NationaliteEleve",
                    "NomCompletTuteur", "GenreTuteur"
                };

                // En-têtes optionnels
                var optionalHeaders = new[] {
                    "ProvinceEleve", "VilleEleve", "CommuneEleve", "QuartierEleve", "AvenueEleve", "NumeroEleve",
                    "CommentaireEleve", "PhotoEleveUrl", "MatriculeEleve",
                    "EmailTuteur", "TelephoneTuteur", "NomCompletRepresentant", "TelephoneRepresentant"
                };

                var allHeaders = requiredHeaders.Concat(optionalHeaders).ToArray();

                for (int i = 0; i < allHeaders.Length; i++)
                {
                    var cell = worksheet.Cells[1, i + 1];
                    cell.Value = allHeaders[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Exemples
                var today = DateTime.Now;
                // Exemples de données (sans Type, NomClasse, LibelleAnneeScolaire)
                worksheet.Cells[2, 1].Value = today; // DateInscription
                worksheet.Cells[2, 1].Style.Numberformat.Format = "yyyy-mm-dd";
                worksheet.Cells[2, 2].Value = "MUKENDI"; // NomEleve
                worksheet.Cells[2, 3].Value = "KALALA"; // PostnomEleve
                worksheet.Cells[2, 4].Value = "Jean"; // PrenomEleve
                worksheet.Cells[2, 5].Value = "M"; // GenreEleve
                worksheet.Cells[2, 6].Value = new DateTime(2010, 5, 15); // DateNaissanceEleve
                worksheet.Cells[2, 6].Style.Numberformat.Format = "yyyy-mm-dd";
                worksheet.Cells[2, 7].Value = "Kinshasa"; // LieuNaissanceEleve
                worksheet.Cells[2, 8].Value = "Congolaise"; // NationaliteEleve
                worksheet.Cells[2, 9].Value = "MUKENDI Pierre"; // NomCompletTuteur
                worksheet.Cells[2, 10].Value = "M"; // GenreTuteur

                // Auto-fit colonnes
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }
        }
    }
}

