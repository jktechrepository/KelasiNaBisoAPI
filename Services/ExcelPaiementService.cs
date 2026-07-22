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
    /// Service pour traiter les fichiers Excel de paiements
    /// </summary>
    public class ExcelPaiementService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly IPaiementRepository _paiementService;
        private readonly ILogger<ExcelPaiementService> _logger;
        private const int BATCH_SIZE = 50; // Traiter 50 paiements à la fois
        private const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10 MB maximum

        // Colonnes attendues dans le fichier Excel
        private static readonly string[] REQUIRED_COLUMNS = {
            "DatePaiement", "Montant", "IdEleve", "IdFrais"
        };

        public ExcelPaiementService(
            KelasiNaBisoDbContext context,
            IPaiementRepository paiementService,
            ILogger<ExcelPaiementService> logger)
        {
            _context = context;
            _paiementService = paiementService;
            _logger = logger;
            
            // Configurer EPPlus pour la licence non-commerciale
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Traite un fichier Excel de paiements
        /// </summary>
        public async Task<BulkPaiementResult> ProcessExcelFileAsync(IFormFile file)
        {
            var result = new BulkPaiementResult();

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
                var paiementsExcel = await ReadExcelFileAsync(file);
                result.TotalLignes = paiementsExcel.Count;

                if (paiementsExcel.Count == 0)
                {
                    result.Success = false;
                    result.Message = "Le fichier Excel est vide ou ne contient pas de données valides.";
                    return result;
                }

                // 3. Validation des données
                await ValidatePaiementsAsync(paiementsExcel);

                // 4. Déduplication dans le fichier (basée sur DatePaiement + IdEleve + IdFrais + Montant)
                DeduplicateInFile(paiementsExcel, result);

                // 5. Séparer les lignes valides et invalides
                var lignesValides = paiementsExcel.Where(p => p.Erreurs.Count == 0).ToList();
                var lignesInvalides = paiementsExcel.Where(p => p.Erreurs.Count > 0).ToList();

                result.LignesAvecErreurs = lignesInvalides;
                result.LignesEchouees = lignesInvalides.Count;

                // 6. Traitement par lots des lignes valides
                if (lignesValides.Count > 0)
                {
                    await ProcessBatchesAsync(lignesValides, result);
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
        private async Task<List<PaiementExcelDto>> ReadExcelFileAsync(IFormFile file)
        {
            var paiements = new List<PaiementExcelDto>();

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

                        var paiement = new PaiementExcelDto
                        {
                            NumeroLigne = row
                        };

                        // Lire les valeurs des cellules
                        paiement.DatePaiement = GetDateTimeValue(worksheet, row, headers, "DatePaiement");
                        paiement.Montant = GetDoubleValue(worksheet, row, headers, "Montant");
                        paiement.Devise = GetCellValue(worksheet, row, headers, "Devise") ?? "USD";
                        paiement.ModePaiement = GetCellValue(worksheet, row, headers, "ModePaiement");
                        paiement.StatutPaiement = GetCellValue(worksheet, row, headers, "StatutPaiement") ?? "Confirmé";
                        paiement.ReferenceTransaction = GetCellValue(worksheet, row, headers, "ReferenceTransaction");
                        paiement.JustificatifUrl = GetCellValue(worksheet, row, headers, "JustificatifUrl");
                        paiement.Commentaire = GetCellValue(worksheet, row, headers, "Commentaire");
                        paiement.IdEleve = GetIntValue(worksheet, row, headers, "IdEleve");
                        paiement.IdFrais = GetIntValue(worksheet, row, headers, "IdFrais");
                        paiement.IdUtilisateur = GetIntValue(worksheet, row, headers, "IdUtilisateur");
                        
                        // Statut (par défaut true)
                        var statutValue = GetCellValue(worksheet, row, headers, "Statut");
                        if (!string.IsNullOrEmpty(statutValue))
                        {
                            paiement.Statut = statutValue.ToLower() == "true" || statutValue == "1" || statutValue.ToLower() == "actif";
                        }
                        else
                        {
                            paiement.Statut = true;
                        }

                        paiements.Add(paiement);
                    }
                }
            }

            return paiements;
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
        /// Obtient la valeur d'une cellule comme double?
        /// </summary>
        private double? GetDoubleValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> headers, string columnName)
        {
            if (!headers.ContainsKey(columnName))
                return null;

            var cell = worksheet.Cells[row, headers[columnName]];
            var value = cell.Value;

            // Si c'est un double
            if (value is double doubleValue)
                return doubleValue;

            // Si c'est un decimal
            if (value is decimal decimalValue)
                return (double)decimalValue;

            // Essayer de parser comme string
            var stringValue = value?.ToString()?.Trim();
            if (string.IsNullOrEmpty(stringValue))
                return null;

            if (double.TryParse(stringValue, out double parsedValue))
                return parsedValue;

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
        /// Valide les paiements lus depuis Excel
        /// </summary>
        private async Task ValidatePaiementsAsync(List<PaiementExcelDto> paiements)
        {
            foreach (var paiement in paiements)
            {
                // Validation des champs obligatoires
                if (!paiement.DatePaiement.HasValue)
                    paiement.Erreurs.Add("La date de paiement est obligatoire");
                else if (paiement.DatePaiement.Value > DateTime.Now.AddDays(1))
                    paiement.Erreurs.Add("La date de paiement ne peut pas être dans le futur");

                if (!paiement.Montant.HasValue)
                    paiement.Erreurs.Add("Le montant est obligatoire");
                else if (paiement.Montant.Value <= 0)
                    paiement.Erreurs.Add("Le montant doit être supérieur à 0");

                if (!paiement.IdEleve.HasValue || paiement.IdEleve.Value <= 0)
                    paiement.Erreurs.Add("L'ID de l'élève est obligatoire et doit être supérieur à 0");

                if (!paiement.IdFrais.HasValue || paiement.IdFrais.Value <= 0)
                    paiement.Erreurs.Add("L'ID des frais est obligatoire et doit être supérieur à 0");

                // Validation de la devise
                if (!string.IsNullOrWhiteSpace(paiement.Devise))
                {
                    var devisesValides = new[] { "USD", "CDF", "EUR" };
                    if (!devisesValides.Contains(paiement.Devise.ToUpper()))
                        paiement.Erreurs.Add($"La devise '{paiement.Devise}' n'est pas valide. Devises acceptées : USD, CDF, EUR");
                }

                // Validation du mode de paiement
                if (!string.IsNullOrWhiteSpace(paiement.ModePaiement))
                {
                    var modesValides = new[] { "Cash", "Carte", "Mobile Money", "Virement", "Chèque" };
                    if (!modesValides.Any(m => m.Equals(paiement.ModePaiement, StringComparison.OrdinalIgnoreCase)))
                        paiement.Erreurs.Add($"Le mode de paiement '{paiement.ModePaiement}' n'est pas valide. Modes acceptés : Cash, Carte, Mobile Money, Virement, Chèque");
                }

                // Validation du statut de paiement
                if (!string.IsNullOrWhiteSpace(paiement.StatutPaiement))
                {
                    var statutsValides = new[] { "En attente", "Confirmé", "Echoué", "Annulé" };
                    if (!statutsValides.Any(s => s.Equals(paiement.StatutPaiement, StringComparison.OrdinalIgnoreCase)))
                        paiement.Erreurs.Add($"Le statut de paiement '{paiement.StatutPaiement}' n'est pas valide. Statuts acceptés : En attente, Confirmé, Echoué, Annulé");
                }

                // Vérifier que l'élève existe
                if (paiement.IdEleve.HasValue && paiement.IdEleve.Value > 0)
                {
                    var eleveExists = await _context.Eleves
                        .AnyAsync(e => e.IdEleve == paiement.IdEleve.Value && e.Statut == true);
                    if (!eleveExists)
                        paiement.Erreurs.Add($"L'élève avec l'ID {paiement.IdEleve.Value} n'existe pas ou n'est pas actif");
                }

                // Vérifier que les frais existent
                if (paiement.IdFrais.HasValue && paiement.IdFrais.Value > 0)
                {
                    var fraisExists = await _context.Frais
                        .AnyAsync(f => f.IdFrais == paiement.IdFrais.Value && f.Statut == true);
                    if (!fraisExists)
                        paiement.Erreurs.Add($"Les frais avec l'ID {paiement.IdFrais.Value} n'existent pas ou ne sont pas actifs");
                }

                // Vérifier que l'utilisateur existe (si fourni)
                if (paiement.IdUtilisateur.HasValue && paiement.IdUtilisateur.Value > 0)
                {
                    var utilisateurExists = await _context.Utilisateurs
                        .AnyAsync(u => u.IdUtilisateur == paiement.IdUtilisateur.Value && u.Statut == true);
                    if (!utilisateurExists)
                        paiement.Erreurs.Add($"L'utilisateur avec l'ID {paiement.IdUtilisateur.Value} n'existe pas ou n'est pas actif");
                }
            }
        }

        /// <summary>
        /// Déduplique les paiements dans le fichier (même paiement plusieurs fois)
        /// </summary>
        private void DeduplicateInFile(List<PaiementExcelDto> paiements, BulkPaiementResult result)
        {
            var seen = new HashSet<string>();
            var duplicates = new List<PaiementExcelDto>();

            foreach (var paiement in paiements)
            {
                if (paiement.Erreurs.Count > 0)
                    continue; // Ignorer les lignes déjà invalides

                // Créer une clé unique basée sur DatePaiement + IdEleve + IdFrais + Montant
                var key = $"{paiement.DatePaiement:yyyy-MM-dd}|{paiement.IdEleve}|{paiement.IdFrais}|{paiement.Montant}";

                if (seen.Contains(key))
                {
                    paiement.Erreurs.Add("Doublon détecté dans le fichier Excel (même paiement présent plusieurs fois)");
                    duplicates.Add(paiement);
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
        /// Traite les paiements par lots avec transactions
        /// </summary>
        private async Task ProcessBatchesAsync(
            List<PaiementExcelDto> lignesValides,
            BulkPaiementResult result)
        {
            var batches = lignesValides
                .Select((item, index) => new { item, index })
                .GroupBy(x => x.index / BATCH_SIZE)
                .Select(g => g.Select(x => x.item).ToList())
                .ToList();

            _logger.LogInformation($"📦 Traitement de {lignesValides.Count} paiements en {batches.Count} lot(s)");

            foreach (var batch in batches)
            {
                await ProcessBatchAsync(batch, result);
            }
        }

        /// <summary>
        /// Traite un lot de paiements avec transaction
        /// </summary>
        private async Task ProcessBatchAsync(
            List<PaiementExcelDto> batch,
            BulkPaiementResult result)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var paiementExcel in batch)
                {
                    try
                    {
                        var paiement = paiementExcel.ToPaiement();
                        
                        // Créer le paiement
                        var paiementCree = await _paiementService.CreateAsync(paiement);

                        if (paiementCree != null)
                        {
                            result.PaiementsCrees.Add(paiementCree);
                            result.LignesReussies++;
                        }
                        else
                        {
                            paiementExcel.Erreurs.Add("Erreur lors de la création du paiement");
                            result.LignesAvecErreurs.Add(paiementExcel);
                            result.LignesEchouees++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"❌ Erreur lors de la création du paiement ligne {paiementExcel.NumeroLigne}");
                        paiementExcel.Erreurs.Add($"Erreur : {ex.Message}");
                        result.LignesAvecErreurs.Add(paiementExcel);
                        result.LignesEchouees++;
                    }
                }

                // Si tous les paiements du lot ont réussi, commit
                if (batch.All(b => b.Erreurs.Count == 0))
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation($"✅ Lot de {batch.Count} paiements traité avec succès");
                }
                else
                {
                    // Rollback si au moins un paiement a échoué
                    await transaction.RollbackAsync();
                    _logger.LogWarning($"⚠️ Rollback du lot : {batch.Count(b => b.Erreurs.Count > 0)} paiement(s) échoué(s)");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "❌ Erreur lors du traitement du lot, rollback effectué");
                
                // Marquer tous les paiements du lot comme échoués
                foreach (var paiementExcel in batch)
                {
                    paiementExcel.Erreurs.Add($"Erreur lors du traitement du lot : {ex.Message}");
                    result.LignesAvecErreurs.Add(paiementExcel);
                    result.LignesEchouees++;
                }
            }
        }

        /// <summary>
        /// Génère le message de résultat
        /// </summary>
        private string GenerateResultMessage(BulkPaiementResult result)
        {
            var message = $"Traitement terminé : {result.LignesReussies} paiement(s) réussi(s) sur {result.TotalLignes} ligne(s)";
            
            if (result.LignesEchouees > 0)
            {
                message += $", {result.LignesEchouees} échoué(s)";
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
                var worksheet = package.Workbook.Worksheets.Add("Paiements");
                
                // En-têtes (ligne 1)
                var headers = new[]
                {
                    "DatePaiement", "Montant", "Devise", "ModePaiement", "StatutPaiement",
                    "ReferenceTransaction", "JustificatifUrl", "Commentaire",
                    "IdEleve", "IdFrais", "IdUtilisateur", "Statut"
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
                worksheet.Cells[2, 1].Value = DateTime.Now.ToString("dd/MM/yyyy");
                worksheet.Cells[2, 2].Value = 100.00;
                worksheet.Cells[2, 3].Value = "USD";
                worksheet.Cells[2, 4].Value = "Cash";
                worksheet.Cells[2, 5].Value = "Confirmé";
                worksheet.Cells[2, 6].Value = "REF-001";
                worksheet.Cells[2, 9].Value = 1; // IdEleve
                worksheet.Cells[2, 10].Value = 1; // IdFrais
                worksheet.Cells[2, 12].Value = true; // Statut

                // Ajuster la largeur des colonnes
                worksheet.Cells.AutoFitColumns();

                package.Save();
            }

            stream.Position = 0;
            return stream;
        }
    }
}
