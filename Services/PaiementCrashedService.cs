using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace KelasiNaBiso.Services
{
    /// <summary>
    /// Service pour gérer les paiements échoués lors du bulk insert Excel
    /// </summary>
    public class PaiementCrashedService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly ILogger<PaiementCrashedService> _logger;

        public PaiementCrashedService(
            KelasiNaBisoDbContext context,
            ILogger<PaiementCrashedService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Récupère tous les paiements échoués pour une école
        /// </summary>
        public async Task<IEnumerable<PaiementCrashedDto>> GetAllByEcoleAsync(int idEcole, bool? estResolu = null)
        {
            try
            {
                // Essayer d'abord avec une requête normale
                var query = _context.PaiementsCrashed
                    .AsNoTracking()
                    .Where(pc => pc.IdEcole == idEcole);

                if (estResolu.HasValue)
                {
                    query = query.Where(pc => pc.EstResolu == estResolu.Value);
                }

                // Charger les paiements sans Include pour éviter les problèmes de cast
                var paiementsCrashed = await query
                    .OrderByDescending(pc => pc.DateEchec ?? DateTime.MinValue)
                    .ToListAsync();

                // Charger les entités liées séparément si nécessaire
                var idsEleves = paiementsCrashed
                    .Where(pc => pc.IdEleve.HasValue)
                    .Select(pc => pc.IdEleve!.Value)
                    .Distinct()
                    .ToList();

                var idsFrais = paiementsCrashed
                    .Where(pc => pc.IdFrais.HasValue)
                    .Select(pc => pc.IdFrais!.Value)
                    .Distinct()
                    .ToList();

                var eleves = idsEleves.Any() 
                    ? await _context.Eleves
                        .AsNoTracking()
                        .Where(e => idsEleves.Contains(e.IdEleve))
                        .ToDictionaryAsync(e => e.IdEleve)
                    : new Dictionary<int, Eleve>();

                var frais = idsFrais.Any()
                    ? await _context.Frais
                        .AsNoTracking()
                        .Where(f => idsFrais.Contains(f.IdFrais))
                        .ToDictionaryAsync(f => f.IdFrais)
                    : new Dictionary<int, Frais>();

                // Mapper les entités liées
                foreach (var pc in paiementsCrashed)
                {
                    if (pc.IdEleve.HasValue && eleves.TryGetValue(pc.IdEleve.Value, out var eleve))
                    {
                        pc.Eleve = eleve;
                    }
                    if (pc.IdFrais.HasValue && frais.TryGetValue(pc.IdFrais.Value, out var fraisItem))
                    {
                        pc.Frais = fraisItem;
                    }
                }

                return paiementsCrashed.Select(pc => MapToDto(pc));
            }
            catch (InvalidCastException ex)
            {
                _logger.LogError(ex, $"Erreur de cast lors de la récupération des paiements échoués pour l'école {idEcole}. " +
                    "Cela indique que la base de données contient des valeurs NULL pour DateEchec ou DateCreation alors que le schéma les attend comme NOT NULL. " +
                    "Veuillez exécuter le script SQL MAKE_PAIEMENT_CRASHED_DATES_NULLABLE_PRODUCTION.sql pour corriger le schéma.");
                
                // Relancer l'exception pour que l'utilisateur soit informé
                throw new InvalidOperationException(
                    "Erreur de récupération des paiements échoués. Le schéma de la base de données doit être mis à jour. " +
                    "Veuillez exécuter le script SQL : MAKE_PAIEMENT_CRASHED_DATES_NULLABLE_PRODUCTION.sql", ex);
            }
        }

        /// <summary>
        /// Récupère un paiement échoué par son ID
        /// </summary>
        public async Task<PaiementCrashedDto?> GetByIdAsync(int id)
        {
            var paiementCrashed = await _context.PaiementsCrashed
                .Include(pc => pc.Eleve)
                .Include(pc => pc.Frais)
                .FirstOrDefaultAsync(pc => pc.IdPaiementCrashed == id);

            return paiementCrashed != null ? MapToDto(paiementCrashed) : null;
        }

        /// <summary>
        /// Met à jour un paiement échoué
        /// </summary>
        public async Task<PaiementCrashedDto?> UpdateAsync(int id, UpdatePaiementCrashedDto dto)
        {
            var paiementCrashed = await _context.PaiementsCrashed
                .FirstOrDefaultAsync(pc => pc.IdPaiementCrashed == id);

            if (paiementCrashed == null)
                return null;

            // Mettre à jour les champs fournis
            if (dto.IdEleve.HasValue)
                paiementCrashed.IdEleve = dto.IdEleve.Value;

            if (dto.IdFrais.HasValue)
                paiementCrashed.IdFrais = dto.IdFrais.Value;

            if (dto.DatePaiement.HasValue)
                paiementCrashed.DatePaiement = dto.DatePaiement.Value;

            if (dto.Montant.HasValue)
                paiementCrashed.Montant = dto.Montant.Value;

            if (!string.IsNullOrWhiteSpace(dto.Devise))
                paiementCrashed.Devise = dto.Devise;

            if (!string.IsNullOrWhiteSpace(dto.ModePaiement))
                paiementCrashed.ModePaiement = dto.ModePaiement;

            if (!string.IsNullOrWhiteSpace(dto.StatutPaiement))
                paiementCrashed.StatutPaiement = dto.StatutPaiement;

            if (!string.IsNullOrWhiteSpace(dto.ReferenceTransaction))
                paiementCrashed.ReferenceTransaction = dto.ReferenceTransaction;

            if (dto.Commentaire != null)
                paiementCrashed.Commentaire = dto.Commentaire;

            paiementCrashed.DateCorrection = DateTime.Now;
            paiementCrashed.DateModification = DateTime.Now;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        /// <summary>
        /// Met à jour plusieurs paiements échoués en masse
        /// </summary>
        public async Task<BulkUpdateResult> BulkUpdateAsync(BulkUpdatePaiementCrashedDto dto)
        {
            var result = new BulkUpdateResult
            {
                Total = dto.Ids.Count,
                Reussis = 0,
                Echoues = 0
            };

            var paiementsCrashed = await _context.PaiementsCrashed
                .Where(pc => dto.Ids.Contains(pc.IdPaiementCrashed))
                .ToListAsync();

            foreach (var pc in paiementsCrashed)
            {
                try
                {
                    if (dto.IdEleve.HasValue)
                        pc.IdEleve = dto.IdEleve.Value;

                    if (dto.IdFrais.HasValue)
                        pc.IdFrais = dto.IdFrais.Value;

                    if (dto.DatePaiement.HasValue)
                        pc.DatePaiement = dto.DatePaiement.Value;

                    if (dto.Montant.HasValue)
                        pc.Montant = dto.Montant.Value;

                    if (!string.IsNullOrWhiteSpace(dto.Devise))
                        pc.Devise = dto.Devise;

                    if (!string.IsNullOrWhiteSpace(dto.ModePaiement))
                        pc.ModePaiement = dto.ModePaiement;

                    if (!string.IsNullOrWhiteSpace(dto.StatutPaiement))
                        pc.StatutPaiement = dto.StatutPaiement;

                    pc.DateCorrection = DateTime.Now;
                    pc.DateModification = DateTime.Now;

                    result.Reussis++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erreur lors de la mise à jour du paiement échoué {pc.IdPaiementCrashed}");
                    result.Echoues++;
                }
            }

            await _context.SaveChangesAsync();

            result.Message = $"Mise à jour terminée : {result.Reussis} réussi(s), {result.Echoues} échoué(s) sur {result.Total}";
            return result;
        }

        /// <summary>
        /// Tente de réinjecter un ou plusieurs paiements échoués
        /// </summary>
        public async Task<ReinjectPaiementCrashedResult> ReinjectAsync(List<int> ids, int idUtilisateur, bool forcerReinjection = false)
        {
            var result = new ReinjectPaiementCrashedResult
            {
                TotalTentes = ids.Count
            };

            var paiementsCrashed = await _context.PaiementsCrashed
                .Include(pc => pc.Eleve)
                .Include(pc => pc.Frais)
                .Where(pc => ids.Contains(pc.IdPaiementCrashed) && !pc.EstResolu)
                .ToListAsync();

            foreach (var pc in paiementsCrashed)
            {
                try
                {
                    // Validation avant réinjection
                    var erreurs = ValidatePaiementCrashed(pc);

                    if (erreurs.Count > 0 && !forcerReinjection)
                    {
                        // Mettre à jour les erreurs
                        pc.ErreursJson = JsonSerializer.Serialize(erreurs);
                        pc.DateReinjection = DateTime.Now;
                        result.PaiementsEchoues.Add(MapToDto(pc));
                        result.Echoues++;
                        continue;
                    }

                    // Créer le paiement
                    // Note: Certains champs sont NOT NULL en base de données selon la migration SQL
                    // Mais dans le modèle C#, ils sont nullable. Il faut utiliser la même logique que ToPaiement()
                    // Créer le paiement avec des valeurs par défaut pour les champs NOT NULL
                    // Même si le modèle C# les accepte comme nullable, la base de données les exige comme NOT NULL
                    var paiement = new Paiement
                    {
                        DatePaiement = pc.DatePaiement ?? DateTime.Now,
                        Montant = pc.Montant ?? 0,
                        Devise = pc.Devise ?? "USD",
                        ModePaiement = pc.ModePaiement ?? "Cash",
                        Statut = pc.Statut ?? true,
                        StatutPaiement = pc.StatutPaiement ?? "Confirmé",
                        // Ces champs sont NOT NULL en base de données, donc on utilise string.Empty au lieu de null
                        ReferenceTransaction = !string.IsNullOrWhiteSpace(pc.ReferenceTransaction) ? pc.ReferenceTransaction : string.Empty,
                        JustificatifUrl = !string.IsNullOrWhiteSpace(pc.JustificatifUrl) ? pc.JustificatifUrl : string.Empty,
                        Commentaire = !string.IsNullOrWhiteSpace(pc.Commentaire) ? pc.Commentaire : string.Empty,
                        IdEleve = pc.IdEleve,
                        IdFrais = pc.IdFrais,
                        IdUtilisateur = idUtilisateur,
                        DateEnregistrement = DateTime.Now,
                        ReferencePaiemenet = DateTime.Now.TimeOfDay.ToString().Replace(":", "").Replace(".", ""),
                        DateCreation = DateTime.Now
                    };

                    _logger.LogInformation($"🔍 Création paiement - ReferenceTransaction: '{paiement.ReferenceTransaction}', JustificatifUrl: '{paiement.JustificatifUrl}', Commentaire: '{paiement.Commentaire}'");

                    _context.Paiements.Add(paiement);
                    
                    // SOLUTION TEMPORAIRE : Forcer les valeurs NOT NULL directement dans le contexte
                    // Car Entity Framework peut ignorer les valeurs nullables même si on les assigne
                    var entry = _context.Entry(paiement);
                    if (entry.Property(p => p.JustificatifUrl).CurrentValue == null)
                        entry.Property(p => p.JustificatifUrl).CurrentValue = string.Empty;
                    if (entry.Property(p => p.ReferenceTransaction).CurrentValue == null)
                        entry.Property(p => p.ReferenceTransaction).CurrentValue = string.Empty;
                    if (entry.Property(p => p.Commentaire).CurrentValue == null)
                        entry.Property(p => p.Commentaire).CurrentValue = string.Empty;
                    
                    await _context.SaveChangesAsync();

                    // Marquer comme résolu
                    pc.EstResolu = true;
                    pc.IdPaiementCree = paiement.IdPaiement;
                    pc.DateReinjection = DateTime.Now;
                    pc.DateModification = DateTime.Now;
                    pc.ErreursJson = "[]"; // Plus d'erreurs

                    await _context.SaveChangesAsync();

                    result.Reussis++;
                    result.IdsReussis.Add(pc.IdPaiementCrashed);
                }
                catch (Exception ex)
                {
                    // Log détaillé de l'erreur
                    _logger.LogError(ex, $"❌ Erreur lors de la réinjection du paiement échoué {pc.IdPaiementCrashed}");
                    _logger.LogError($"   - IdEleve: {pc.IdEleve}, IdFrais: {pc.IdFrais}, IdUtilisateur: {idUtilisateur}");
                    _logger.LogError($"   - Montant: {pc.Montant}, Devise: {pc.Devise}, ModePaiement: {pc.ModePaiement}");
                    
                    // Récupérer le message d'erreur complet avec toutes les inner exceptions
                    var errorMessages = new List<string> { ex.Message };
                    var innerEx = ex.InnerException;
                    int depth = 0;
                    while (innerEx != null && depth < 5)
                    {
                        errorMessages.Add($"Inner[{depth}]: {innerEx.Message}");
                        innerEx = innerEx.InnerException;
                        depth++;
                    }
                    
                    // Log de la stack trace si disponible
                    if (!string.IsNullOrEmpty(ex.StackTrace))
                    {
                        _logger.LogError($"Stack trace: {ex.StackTrace.Substring(0, Math.Min(500, ex.StackTrace.Length))}");
                    }
                    
                    // Mettre à jour les erreurs
                    var erreurs = new List<string> { $"Erreur de réinjection : {string.Join(" | ", errorMessages)}" };
                    pc.ErreursJson = JsonSerializer.Serialize(erreurs);
                    pc.DateReinjection = DateTime.Now;
                    
                    // Sauvegarder les erreurs
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception saveEx)
                    {
                        _logger.LogError(saveEx, $"Erreur lors de la sauvegarde des erreurs pour PaiementCrashed {pc.IdPaiementCrashed}");
                    }
                    
                    result.PaiementsEchoues.Add(MapToDto(pc));
                    result.Echoues++;
                }
            }

            result.Message = $"Réinjection terminée : {result.Reussis} réussi(s), {result.Echoues} échoué(s) sur {result.TotalTentes}";
            return result;
        }

        /// <summary>
        /// Valide un paiement échoué avant réinjection
        /// </summary>
        private List<string> ValidatePaiementCrashed(PaiementCrashed pc)
        {
            var erreurs = new List<string>();

            if (pc.DatePaiement == null)
                erreurs.Add("La date de paiement est obligatoire");

            if (pc.Montant == null || pc.Montant <= 0)
                erreurs.Add("Le montant doit être supérieur à 0");

            if (string.IsNullOrWhiteSpace(pc.Devise))
                erreurs.Add("La devise est obligatoire");

            if (!new[] { "USD", "CDF", "EUR" }.Contains(pc.Devise))
                erreurs.Add("La devise doit être USD, CDF ou EUR");

            if (string.IsNullOrWhiteSpace(pc.ModePaiement))
                erreurs.Add("Le mode de paiement est obligatoire");

            if (!pc.IdEleve.HasValue || pc.IdEleve.Value <= 0)
                erreurs.Add("L'ID de l'élève est obligatoire");

            if (!pc.IdFrais.HasValue || pc.IdFrais.Value <= 0)
                erreurs.Add("L'ID des frais est obligatoire");

            // Vérifier que l'élève existe
            if (pc.IdEleve.HasValue)
            {
                var eleveExists = _context.Eleves.Any(e => e.IdEleve == pc.IdEleve.Value && e.Statut == true);
                if (!eleveExists)
                    erreurs.Add($"L'élève avec l'ID {pc.IdEleve.Value} n'existe pas ou est inactif");
            }

            // Vérifier que les frais existent
            if (pc.IdFrais.HasValue)
            {
                var fraisExists = _context.Frais.Any(f => f.IdFrais == pc.IdFrais.Value && f.Statut == true);
                if (!fraisExists)
                    erreurs.Add($"Les frais avec l'ID {pc.IdFrais.Value} n'existent pas ou sont inactifs");
            }

            return erreurs;
        }

        /// <summary>
        /// Supprime un paiement échoué (après réinjection réussie)
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var paiementCrashed = await _context.PaiementsCrashed
                .FirstOrDefaultAsync(pc => pc.IdPaiementCrashed == id);

            if (paiementCrashed == null)
                return false;

            _context.PaiementsCrashed.Remove(paiementCrashed);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Supprime plusieurs paiements échoués résolus
        /// </summary>
        public async Task<int> DeleteResolvedAsync(int idEcole)
        {
            var paiementsResolus = await _context.PaiementsCrashed
                .Where(pc => pc.IdEcole == idEcole && pc.EstResolu == true)
                .ToListAsync();

            var count = paiementsResolus.Count;
            _context.PaiementsCrashed.RemoveRange(paiementsResolus);
            await _context.SaveChangesAsync();

            return count;
        }

        /// <summary>
        /// Convertit un PaiementCrashed en DTO
        /// </summary>
        private PaiementCrashedDto MapToDto(PaiementCrashed pc)
        {
            var erreurs = new List<string>();
            try
            {
                if (!string.IsNullOrWhiteSpace(pc.ErreursJson))
                {
                    erreurs = JsonSerializer.Deserialize<List<string>>(pc.ErreursJson) ?? new List<string>();
                }
            }
            catch
            {
                erreurs = new List<string> { "Erreur lors de la désérialisation des erreurs" };
            }

            return new PaiementCrashedDto
            {
                IdPaiementCrashed = pc.IdPaiementCrashed,
                DatePaiement = pc.DatePaiement,
                Montant = pc.Montant,
                Devise = pc.Devise,
                ModePaiement = pc.ModePaiement,
                StatutPaiement = pc.StatutPaiement,
                ReferenceTransaction = pc.ReferenceTransaction,
                Commentaire = pc.Commentaire,
                IdEleve = pc.IdEleve,
                IdFrais = pc.IdFrais,
                NomCompletEleve = pc.NomCompletEleve,
                LibelleFrais = pc.LibelleFrais,
                Erreurs = erreurs,
                NumeroLigne = pc.NumeroLigne,
                NomFichierOriginal = pc.NomFichierOriginal,
                DateEchec = pc.DateEchec,
                DateCorrection = pc.DateCorrection,
                DateReinjection = pc.DateReinjection,
                IdPaiementCree = pc.IdPaiementCree,
                EstResolu = pc.EstResolu,
                NomEleve = pc.Eleve?.NomComplet,
                NomFrais = pc.Frais?.LibelleFrais
            };
        }
    }

    /// <summary>
    /// Résultat d'une mise à jour en masse
    /// </summary>
    public class BulkUpdateResult
    {
        public int Total { get; set; }
        public int Reussis { get; set; }
        public int Echoues { get; set; }
        public string Message { get; set; } = string.Empty;
    }

}

