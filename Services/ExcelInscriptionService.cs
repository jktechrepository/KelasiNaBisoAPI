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
    /// Service pour traiter les fichiers Excel d'inscriptions
    /// </summary>
    public class ExcelInscriptionService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IInscriptionRepository _inscriptionService;
        private readonly ILogger<ExcelInscriptionService> _logger;
        private const int BATCH_SIZE = 50; // Traiter 50 inscriptions à la fois
        private const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10 MB maximum

        // Colonnes attendues dans le fichier Excel
        private static readonly string[] REQUIRED_COLUMNS = {
            "Type", "IdEcole", "IdClasse", "IdAnneeScolaire", "DateInscription",
            "NomEleve", "PostnomEleve", "PrenomEleve", "GenreEleve", "DateNaissanceEleve",
            "LieuNaissanceEleve", "NationaliteEleve",
            "NomCompletTuteur", "GenreTuteur"
        };

        public ExcelInscriptionService(
            KelasiNaBisoDbContext context,
            IInscriptionRepository inscriptionService,
            ILogger<ExcelInscriptionService> logger)
        {
            _context = context;
            _inscriptionService = inscriptionService;
            _logger = logger;
            
            // Configurer EPPlus pour la licence non-commerciale
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Traite un fichier Excel d'inscriptions
        /// </summary>
        public async Task<BulkInscriptionResult> ProcessExcelFileAsync(IFormFile file, string nomEcole)
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

                // 2. Lire et parser le fichier Excel
                var inscriptionsExcel = await ReadExcelFileAsync(file);
                result.TotalLignes = inscriptionsExcel.Count;

                if (inscriptionsExcel.Count == 0)
                {
                    result.Success = false;
                    result.Message = "Le fichier Excel est vide ou ne contient pas de données valides.";
                    return result;
                }

                // 3. Validation des données
                await ValidateInscriptionsAsync(inscriptionsExcel);

                // 4. Déduplication dans le fichier
                DeduplicateInFile(inscriptionsExcel, result);

                // 5. Séparer les lignes valides et invalides
                var lignesValides = inscriptionsExcel.Where(i => i.Erreurs.Count == 0).ToList();
                var lignesInvalides = inscriptionsExcel.Where(i => i.Erreurs.Count > 0).ToList();

                result.LignesAvecErreurs = lignesInvalides;
                result.LignesEchouees = lignesInvalides.Count;

                // 6. Traitement par lots des lignes valides
                if (lignesValides.Count > 0)
                {
                    await ProcessBatchesAsync(lignesValides, nomEcole, result);
                }

                // 7. Générer le message de résultat
                result.Success = result.LignesReussies > 0;
                result.Message = GenerateResultMessage(result);

                _logger.LogInformation($"✅ Traitement Excel terminé : {result.LignesReussies}/{result.TotalLignes} réussies");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du traitement du fichier Excel");
                result.Success = false;
                result.Message = $"Erreur lors du traitement du fichier : {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Valide le fichier uploadé
        /// </summary>
        private (bool Success, string Message) ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "Le fichier est vide ou null");
            }

            if (file.Length > MAX_FILE_SIZE)
            {
                return (false, $"Le fichier est trop volumineux. Taille maximum : {MAX_FILE_SIZE / (1024 * 1024)} MB");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
            {
                return (false, "Le fichier doit être au format Excel (.xlsx ou .xls)");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Lit et parse le fichier Excel
        /// </summary>
        private async Task<List<InscriptionExcelDto>> ReadExcelFileAsync(IFormFile file)
        {
            var inscriptions = new List<InscriptionExcelDto>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        throw new InvalidOperationException("Le fichier Excel ne contient aucune feuille de calcul");
                    }

                    // Vérifier les colonnes (ligne 1 = en-têtes)
                    var headers = new Dictionary<string, int>();
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        var headerValue = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(headerValue))
                        {
                            headers[headerValue] = col;
                        }
                    }

                    // Vérifier que toutes les colonnes requises sont présentes
                    var missingColumns = REQUIRED_COLUMNS.Where(c => !headers.ContainsKey(c)).ToList();
                    if (missingColumns.Any())
                    {
                        throw new InvalidOperationException(
                            $"Colonnes manquantes dans le fichier Excel : {string.Join(", ", missingColumns)}");
                    }

                    // Lire les données (ligne 2 et suivantes)
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        // Vérifier si la ligne est vide
                        var firstCellValue = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                        if (string.IsNullOrEmpty(firstCellValue))
                        {
                            continue; // Ligne vide, passer à la suivante
                        }

                        var inscription = new InscriptionExcelDto
                        {
                            NumeroLigne = row
                        };

                        // Lire les valeurs des cellules
                        inscription.Type = GetCellValue(worksheet, row, headers, "Type") ?? "Inscription";
                        inscription.IdEcole = GetIntValue(worksheet, row, headers, "IdEcole");
                        inscription.IdClasse = GetIntValue(worksheet, row, headers, "IdClasse");
                        inscription.IdAnneeScolaire = GetIntValue(worksheet, row, headers, "IdAnneeScolaire");
                        inscription.DateInscription = GetDateTimeValue(worksheet, row, headers, "DateInscription");
                        inscription.StatutInscription = InscriptionActiveRules.NormalizeStatutInscriptionForCreate(
                            GetCellValue(worksheet, row, headers, "StatutInscription"));

                        inscription.NomEleve = GetCellValue(worksheet, row, headers, "NomEleve") ?? string.Empty;
                        inscription.PostnomEleve = GetCellValue(worksheet, row, headers, "PostnomEleve") ?? string.Empty;
                        inscription.PrenomEleve = GetCellValue(worksheet, row, headers, "PrenomEleve") ?? string.Empty;
                        inscription.PhotoEleveUrl = GetCellValue(worksheet, row, headers, "PhotoEleveUrl");
                        inscription.MatriculeEleve = GetCellValue(worksheet, row, headers, "MatriculeEleve");
                        inscription.GenreEleve = GetCellValue(worksheet, row, headers, "GenreEleve") ?? string.Empty;
                        inscription.DateNaissanceEleve = GetDateTimeValue(worksheet, row, headers, "DateNaissanceEleve");
                        inscription.LieuNaissanceEleve = GetCellValue(worksheet, row, headers, "LieuNaissanceEleve") ?? string.Empty;
                        inscription.NationaliteEleve = GetCellValue(worksheet, row, headers, "NationaliteEleve") ?? string.Empty;
                        inscription.ProvinceEleve = GetCellValue(worksheet, row, headers, "ProvinceEleve");
                        inscription.VilleEleve = GetCellValue(worksheet, row, headers, "VilleEleve");
                        inscription.CommuneEleve = GetCellValue(worksheet, row, headers, "CommuneEleve");
                        inscription.QuartierEleve = GetCellValue(worksheet, row, headers, "QuartierEleve");
                        inscription.AvenueEleve = GetCellValue(worksheet, row, headers, "AvenueEleve");
                        inscription.NumeroEleve = GetCellValue(worksheet, row, headers, "NumeroEleve");
                        inscription.CommentaireEleve = GetCellValue(worksheet, row, headers, "CommentaireEleve");

                        inscription.NomCompletTuteur = GetCellValue(worksheet, row, headers, "NomCompletTuteur") ?? string.Empty;
                        inscription.GenreTuteur = GetCellValue(worksheet, row, headers, "GenreTuteur") ?? string.Empty;
                        inscription.EmailTuteur = GetCellValue(worksheet, row, headers, "EmailTuteur");
                        inscription.TelephoneTuteur = GetCellValue(worksheet, row, headers, "TelephoneTuteur");
                        inscription.NomCompletRepresentant = GetCellValue(worksheet, row, headers, "NomCompletRepresentant");
                        inscription.TelephoneRepresentant = GetCellValue(worksheet, row, headers, "TelephoneRepresentant");
                        inscription.PhotoTuteurUrl = GetCellValue(worksheet, row, headers, "PhotoTuteurUrl");
                        inscription.PieceIdentiteTuteur = GetCellValue(worksheet, row, headers, "PieceIdentiteTuteur");
                        inscription.IdEleveExistant = GetIntValue(worksheet, row, headers, "IdEleveExistant");
                        inscription.IdTuteurExistant = GetIntValue(worksheet, row, headers, "IdTuteurExistant");

                        inscriptions.Add(inscription);
                    }
                }
            }

            return inscriptions;
        }

        /// <summary>
        /// Obtient la valeur d'une cellule comme string
        /// </summary>
        private string? GetCellValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> headers, string columnName)
        {
            if (!headers.ContainsKey(columnName))
                return null;

            var value = worksheet.Cells[row, headers[columnName]].Value;
            return value?.ToString()?.Trim();
        }

        /// <summary>
        /// Obtient la valeur d'une cellule comme int?
        /// </summary>
        private int? GetIntValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> headers, string columnName)
        {
            var value = GetCellValue(worksheet, row, headers, columnName);
            if (string.IsNullOrEmpty(value))
                return null;

            if (int.TryParse(value, out int intValue))
                return intValue;

            return null;
        }

        /// <summary>
        /// Obtient la valeur d'une cellule comme DateTime?
        /// </summary>
        private DateTime? GetDateTimeValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> headers, string columnName)
        {
            if (!headers.ContainsKey(columnName))
                return null;

            var cell = worksheet.Cells[row, headers[columnName]];
            var value = cell.Value;

            // Si c'est un DateTime Excel
            if (value is DateTime dateTime)
                return dateTime;

            // Si c'est un double (nombre de jours depuis 1900)
            if (value is double doubleValue)
            {
                try
                {
                    return DateTime.FromOADate(doubleValue);
                }
                catch
                {
                    // Essayer de parser comme string
                }
            }

            // Essayer de parser comme string
            var stringValue = value?.ToString()?.Trim();
            if (string.IsNullOrEmpty(stringValue))
                return null;

            if (DateTime.TryParse(stringValue, out DateTime parsedDate))
                return parsedDate;

            return null;
        }

        /// <summary>
        /// Valide les inscriptions lues depuis Excel
        /// </summary>
        private async Task ValidateInscriptionsAsync(List<InscriptionExcelDto> inscriptions)
        {
            foreach (var inscription in inscriptions)
            {
                // Validation des champs obligatoires
                if (string.IsNullOrWhiteSpace(inscription.NomEleve))
                    inscription.Erreurs.Add("Le nom de l'élève est obligatoire");

                if (string.IsNullOrWhiteSpace(inscription.PostnomEleve))
                    inscription.Erreurs.Add("Le postnom de l'élève est obligatoire");

                if (string.IsNullOrWhiteSpace(inscription.PrenomEleve))
                    inscription.Erreurs.Add("Le prénom de l'élève est obligatoire");

                if (string.IsNullOrWhiteSpace(inscription.GenreEleve))
                    inscription.Erreurs.Add("Le genre de l'élève est obligatoire");
                else if (inscription.GenreEleve != "M" && inscription.GenreEleve != "F")
                    inscription.Erreurs.Add("Le genre de l'élève doit être 'M' ou 'F'");

                if (!inscription.DateNaissanceEleve.HasValue)
                    inscription.Erreurs.Add("La date de naissance de l'élève est obligatoire");
                else if (inscription.DateNaissanceEleve.Value > DateTime.Now)
                    inscription.Erreurs.Add("La date de naissance ne peut pas être dans le futur");

                if (string.IsNullOrWhiteSpace(inscription.LieuNaissanceEleve))
                    inscription.Erreurs.Add("Le lieu de naissance de l'élève est obligatoire");

                if (string.IsNullOrWhiteSpace(inscription.NationaliteEleve))
                    inscription.Erreurs.Add("La nationalité de l'élève est obligatoire");

                if (string.IsNullOrWhiteSpace(inscription.NomCompletTuteur))
                    inscription.Erreurs.Add("Le nom complet du tuteur est obligatoire");

                if (string.IsNullOrWhiteSpace(inscription.GenreTuteur))
                    inscription.Erreurs.Add("Le genre du tuteur est obligatoire");
                else if (inscription.GenreTuteur != "M" && inscription.GenreTuteur != "F")
                    inscription.Erreurs.Add("Le genre du tuteur doit être 'M' ou 'F'");

                if (!inscription.IdEcole.HasValue || inscription.IdEcole.Value <= 0)
                    inscription.Erreurs.Add("L'ID de l'école est obligatoire et doit être supérieur à 0");

                if (!inscription.IdClasse.HasValue || inscription.IdClasse.Value <= 0)
                    inscription.Erreurs.Add("L'ID de la classe est obligatoire et doit être supérieur à 0");

                if (!inscription.IdAnneeScolaire.HasValue || inscription.IdAnneeScolaire.Value <= 0)
                    inscription.Erreurs.Add("L'ID de l'année scolaire est obligatoire et doit être supérieur à 0");

                // Validation de l'email si fourni
                if (!string.IsNullOrWhiteSpace(inscription.EmailTuteur))
                {
                    var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                    if (!emailRegex.IsMatch(inscription.EmailTuteur))
                        inscription.Erreurs.Add("L'email du tuteur n'est pas valide");
                }

                // Validation du téléphone si fourni
                if (!string.IsNullOrWhiteSpace(inscription.TelephoneTuteur))
                {
                    var phoneRegex = new Regex(@"^\+?[0-9]{8,15}$");
                    if (!phoneRegex.IsMatch(inscription.TelephoneTuteur.Replace(" ", "").Replace("-", "")))
                        inscription.Erreurs.Add("Le téléphone du tuteur n'est pas valide (format attendu : 8-15 chiffres)");
                }

                // Vérifier que l'école existe
                if (inscription.IdEcole.HasValue && inscription.IdEcole.Value > 0)
                {
                    var ecoleExists = await _context.Ecoles
                        .AnyAsync(e => e.IdEcole == inscription.IdEcole.Value && e.Statut == true);
                    if (!ecoleExists)
                        inscription.Erreurs.Add($"L'école avec l'ID {inscription.IdEcole.Value} n'existe pas ou n'est pas active");
                }

                // Vérifier que la classe existe
                if (inscription.IdClasse.HasValue && inscription.IdClasse.Value > 0)
                {
                    var classeExists = await _context.Classes
                        .AnyAsync(c => c.IdClasse == inscription.IdClasse.Value && c.Statut == true);
                    if (!classeExists)
                        inscription.Erreurs.Add($"La classe avec l'ID {inscription.IdClasse.Value} n'existe pas ou n'est pas active");
                }

                // Vérifier que l'année scolaire existe
                if (inscription.IdAnneeScolaire.HasValue && inscription.IdAnneeScolaire.Value > 0)
                {
                    var anneeExists = await _context.AnneeScolaires
                        .AnyAsync(a => a.IdAnneeScolaire == inscription.IdAnneeScolaire.Value && a.Statut == true);
                    if (!anneeExists)
                        inscription.Erreurs.Add($"L'année scolaire avec l'ID {inscription.IdAnneeScolaire.Value} n'existe pas ou n'est pas active");
                }
            }
        }

        /// <summary>
        /// Déduplique les inscriptions dans le fichier (même élève plusieurs fois)
        /// </summary>
        private void DeduplicateInFile(List<InscriptionExcelDto> inscriptions, BulkInscriptionResult result)
        {
            var seen = new HashSet<string>();
            var duplicates = new List<InscriptionExcelDto>();

            foreach (var inscription in inscriptions)
            {
                if (inscription.Erreurs.Count > 0)
                    continue; // Ignorer les lignes déjà invalides

                // Créer une clé unique basée sur les critères d'unicité
                var key = $"{NormalizeName(inscription.NomEleve)}|{NormalizeName(inscription.PostnomEleve)}|{NormalizeName(inscription.PrenomEleve)}|{inscription.DateNaissanceEleve:yyyy-MM-dd}|{inscription.IdTuteurExistant}|{inscription.IdClasse}";

                if (seen.Contains(key))
                {
                    inscription.Erreurs.Add("Doublon détecté dans le fichier Excel (même élève présent plusieurs fois)");
                    duplicates.Add(inscription);
                }
                else
                {
                    seen.Add(key);
                }
            }

            result.DoublonsDetectes = duplicates.Count;
            _logger.LogInformation($"🔍 {result.DoublonsDetectes} doublon(s) détecté(s) dans le fichier Excel");
        }

        /// <summary>
        /// Normalise un nom pour la comparaison (même logique que InscriptionService)
        /// </summary>
        private string NormalizeName(string? name)
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
                .Replace("Î", "I")
                .Replace("Ï", "I")
                .Replace("Ô", "O")
                .Replace("Ö", "O")
                .Replace("Ù", "U")
                .Replace("Û", "U")
                .Replace("Ü", "U")
                .Replace("Ç", "C");
        }

        /// <summary>
        /// Traite les inscriptions par lots avec transactions
        /// </summary>
        private async Task ProcessBatchesAsync(
            List<InscriptionExcelDto> lignesValides,
            string nomEcole,
            BulkInscriptionResult result)
        {
            var batches = lignesValides
                .Select((item, index) => new { item, index })
                .GroupBy(x => x.index / BATCH_SIZE)
                .Select(g => g.Select(x => x.item).ToList())
                .ToList();

            _logger.LogInformation($"📦 Traitement de {lignesValides.Count} inscriptions en {batches.Count} lot(s)");

            foreach (var batch in batches)
            {
                await ProcessBatchAsync(batch, nomEcole, result);
            }
        }

        /// <summary>
        /// Traite un lot d'inscriptions avec transaction
        /// </summary>
        private async Task ProcessBatchAsync(
            List<InscriptionExcelDto> batch,
            string nomEcole,
            BulkInscriptionResult result)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var inscriptionExcel in batch)
                {
                    try
                    {
                        var dto = inscriptionExcel.ToCreateInscriptionDto();
                        
                        // Générer le matricule si nécessaire
                        if (string.IsNullOrWhiteSpace(dto.MatriculeEleve))
                        {
                            dto.MatriculeEleve = _inscriptionService.GenerateMatriculeEleve(nomEcole, dto);
                        }

                        // Créer l'inscription
                        var inscriptionResult = await _inscriptionService.CreateInscriptionAsync(dto);

                        if (inscriptionResult.Success)
                        {
                            result.InscriptionsCrees.Add(inscriptionResult);
                            result.LignesReussies++;
                        }
                        else
                        {
                            inscriptionExcel.Erreurs.Add(inscriptionResult.Message);
                            result.LignesAvecErreurs.Add(inscriptionExcel);
                            result.LignesEchouees++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"❌ Erreur lors de la création de l'inscription ligne {inscriptionExcel.NumeroLigne}");
                        inscriptionExcel.Erreurs.Add($"Erreur : {ex.Message}");
                        result.LignesAvecErreurs.Add(inscriptionExcel);
                        result.LignesEchouees++;
                    }
                }

                // Si toutes les inscriptions du lot ont réussi, commit
                if (batch.All(b => b.Erreurs.Count == 0))
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation($"✅ Lot de {batch.Count} inscriptions traité avec succès");
                }
                else
                {
                    // Rollback si au moins une inscription a échoué
                    await transaction.RollbackAsync();
                    _logger.LogWarning($"⚠️ Rollback du lot : {batch.Count(b => b.Erreurs.Count > 0)} inscription(s) échouée(s)");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "❌ Erreur lors du traitement du lot, rollback effectué");
                
                // Marquer toutes les inscriptions du lot comme échouées
                foreach (var inscriptionExcel in batch)
                {
                    inscriptionExcel.Erreurs.Add($"Erreur lors du traitement du lot : {ex.Message}");
                    result.LignesAvecErreurs.Add(inscriptionExcel);
                    result.LignesEchouees++;
                }
            }
        }

        /// <summary>
        /// Génère le message de résultat
        /// </summary>
        private string GenerateResultMessage(BulkInscriptionResult result)
        {
            var message = $"Traitement terminé : {result.LignesReussies} inscription(s) réussie(s) sur {result.TotalLignes} ligne(s)";
            
            if (result.LignesEchouees > 0)
            {
                message += $", {result.LignesEchouees} échouée(s)";
            }

            if (result.DoublonsDetectes > 0)
            {
                message += $", {result.DoublonsDetectes} doublon(s) détecté(s)";
            }

            return message;
        }

        /// <summary>
        /// Génère un template Excel avec les colonnes requises et des exemples
        /// </summary>
        public MemoryStream GenerateExcelTemplate()
        {
            var stream = new MemoryStream();
            
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("Inscriptions");
                
                // En-têtes (ligne 1)
                var headers = new[]
                {
                    "Type", "IdEcole", "IdClasse", "IdAnneeScolaire", "DateInscription", "StatutInscription",
                    "NomEleve", "PostnomEleve", "PrenomEleve", "GenreEleve", "DateNaissanceEleve",
                    "LieuNaissanceEleve", "NationaliteEleve", "ProvinceEleve", "VilleEleve", "CommuneEleve",
                    "QuartierEleve", "AvenueEleve", "NumeroEleve", "CommentaireEleve", "PhotoEleveUrl", "MatriculeEleve",
                    "NomCompletTuteur", "GenreTuteur", "EmailTuteur", "TelephoneTuteur",
                    "NomCompletRepresentant", "TelephoneRepresentant", "PhotoTuteurUrl", "PieceIdentiteTuteur",
                    "IdEleveExistant", "IdTuteurExistant"
                };

                // Écrire les en-têtes
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Exemple de données (ligne 2)
                worksheet.Cells[2, 1].Value = "Inscription";
                worksheet.Cells[2, 2].Value = 1; // IdEcole
                worksheet.Cells[2, 3].Value = 1; // IdClasse
                worksheet.Cells[2, 4].Value = 1; // IdAnneeScolaire
                worksheet.Cells[2, 5].Value = DateTime.Now.ToString("dd/MM/yyyy");
                worksheet.Cells[2, 6].Value = "Confirmé";
                worksheet.Cells[2, 7].Value = "KABEYA";
                worksheet.Cells[2, 8].Value = "MULENGA";
                worksheet.Cells[2, 9].Value = "Jean";
                worksheet.Cells[2, 10].Value = "M";
                worksheet.Cells[2, 11].Value = "15/05/2010";
                worksheet.Cells[2, 12].Value = "Kinshasa";
                worksheet.Cells[2, 13].Value = "Congolaise";
                worksheet.Cells[2, 23].Value = "KABEYA MULENGA Pierre";
                worksheet.Cells[2, 24].Value = "M";
                worksheet.Cells[2, 25].Value = "pierre@example.com";
                worksheet.Cells[2, 26].Value = "+243900123456";

                // Ajuster la largeur des colonnes
                worksheet.Cells.AutoFitColumns();

                package.Save();
            }

            stream.Position = 0;
            return stream;
        }
    }
}

