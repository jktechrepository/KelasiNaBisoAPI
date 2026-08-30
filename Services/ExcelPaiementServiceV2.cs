using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;

namespace KelasiNaBiso.Services
{
    /// <summary>
    /// Service optimisé pour traiter les fichiers Excel de paiements (Version 2)
    /// Utilise le chargement en mémoire des élèves et frais pour optimiser les performances
    /// </summary>
    public class ExcelPaiementServiceV2
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly ILogger<ExcelPaiementServiceV2> _logger;
        private readonly EleveAnneeScopeHelper _scope;
        private const int BATCH_SIZE = 50; // Traiter 50 paiements à la fois
        private const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10 MB maximum

        // Colonnes attendues dans le nouveau format Excel
        private static readonly string[] REQUIRED_COLUMNS = {
            "DatePaiement", "Montant", "Devise", "ModePaiement", "NomCompletEleve", "LibelleFrais"
        };

        public ExcelPaiementServiceV2(
            KelasiNaBisoDbContext context,
            ILogger<ExcelPaiementServiceV2> logger,
            EleveAnneeScopeHelper scope)
        {
            _context = context;
            _logger = logger;
            _scope = scope;
            
            // Configurer EPPlus pour la licence non-commerciale
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // Dictionnaire global pour stocker les données brutes (utilisé pour sauvegarder les paiements échoués)
        private Dictionary<int, PaiementExcelRaw>? _paiementsRawDict;
        private int _idEcole;
        private int _idUtilisateur;
        private string? _nomFichier;

        /// <summary>
        /// Traite un fichier Excel de paiements avec le nouveau format simplifié
        /// </summary>
        public async Task<BulkPaiementResult> ProcessExcelFileAsync(IFormFile file, int idEcole, int idUtilisateur)
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

                // 2. Charger TOUS les élèves et frais de l'école en mémoire (OPTIMISATION)
                _logger.LogInformation($"🔄 Chargement des élèves et frais de l'école {idEcole}...");
                var (elevesEcoleDict, elevesEcoleInfo) = await LoadElevesByEcoleAsync(idEcole);
                var (fraisEcoleDict, fraisEcoleInfo) = await LoadFraisByEcoleAsync(idEcole);

                _logger.LogInformation($"✅ {elevesEcoleDict.Count} élèves (exact) et {elevesEcoleInfo.Count} élèves (détails) et {fraisEcoleDict.Count} frais (exact) et {fraisEcoleInfo.Count} frais (détails) chargés");

                // 3. Lire et parser le fichier Excel
                var paiementsRaw = await ReadExcelFileAsync(file);
                result.TotalLignes = paiementsRaw.Count;

                if (paiementsRaw.Count == 0)
                {
                    result.Success = false;
                    result.Message = "Le fichier Excel est vide ou ne contient pas de données valides.";
                    return result;
                }

                // 4. Convertir en PaiementExcelDto et enrichir avec les IDs
                var paiementsExcel = ConvertToPaiementExcelDto(paiementsRaw, elevesEcoleDict, elevesEcoleInfo, fraisEcoleDict, fraisEcoleInfo, idEcole);
                
                // Créer un dictionnaire pour retrouver les données brutes par numéro de ligne
                var paiementsRawDict = paiementsRaw.ToDictionary(r => r.NumeroLigne);
                
                // Stocker dans les variables de classe pour utilisation dans ProcessBatchesAsync
                _paiementsRawDict = paiementsRawDict;
                _idEcole = idEcole;
                _idUtilisateur = idUtilisateur;
                _nomFichier = file.FileName;

                // 5. Validation des données
                ValidatePaiements(paiementsExcel);

                // 6. Déduplication dans le fichier
                DeduplicateInFile(paiementsExcel, result);

                // 7. Séparer les lignes valides et invalides
                var lignesValides = paiementsExcel.Where(p => p.Erreurs.Count == 0).ToList();
                var lignesInvalides = paiementsExcel.Where(p => p.Erreurs.Count > 0).ToList();

                result.LignesAvecErreurs = lignesInvalides;
                result.LignesEchouees = lignesInvalides.Count;

                // 8. Sauvegarder les paiements échoués dans PaiementCrashed
                if (lignesInvalides.Count > 0)
                {
                    await SaveCrashedPaiementsAsync(lignesInvalides, paiementsRawDict, idEcole, idUtilisateur, file.FileName);
                }

                // 9. Traitement par lots des lignes valides
                if (lignesValides.Count > 0)
                {
                    await ProcessBatchesAsync(lignesValides, result, idUtilisateur);
                }

                // 10. Générer le message de résultat
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
        /// Structure pour stocker les informations d'un élève avec ses mots normalisés
        /// </summary>
        private class EleveInfo
        {
            public int IdEleve { get; set; }
            public int? IdClasse { get; set; }
            public string NomCompletOriginal { get; set; } = string.Empty;
            public string NomNormalise { get; set; } = string.Empty;
            public HashSet<string> MotsNormalises { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Charge tous les élèves actifs d'une école en mémoire avec leurs informations de recherche
        /// </summary>
        private async Task<(Dictionary<string, int> DictionaryExact, Dictionary<int, EleveInfo> ElevesInfo)> LoadElevesByEcoleAsync(int idEcole)
        {
            var idAnnee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, null);
            var eleves = await _context.Eleves
                .Where(e => e.Inscriptions.Any(i => i.IdEcole == idEcole
                    && i.IdAnneeScolaire == idAnnee
                    && i.Statut == true
                    && (i.StatutInscription == "Confirmé" || i.StatutInscription == "Confirme" || i.StatutInscription.StartsWith("Confirm")))
                    && e.Statut == true)
                .Select(e => new
                {
                    e.IdEleve,
                    e.NomComplet,
                    IdClasse = e.Inscriptions
                        .Where(i => i.IdEcole == idEcole
                            && i.IdAnneeScolaire == idAnnee
                            && i.Statut == true
                            && (i.StatutInscription == "Confirmé" || i.StatutInscription == "Confirme" || i.StatutInscription.StartsWith("Confirm")))
                        .Select(i => (int?)i.IdClasse)
                        .FirstOrDefault()
                })
                .ToListAsync();

            // Dictionnaire pour recherche exacte (nom normalisé sans espaces)
            var dictionaryExact = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            // Dictionnaire pour recherche par mots (contient toutes les infos)
            var elevesInfo = new Dictionary<int, EleveInfo>();
            var collisions = new Dictionary<string, List<int>>(); // Pour détecter les collisions
            
            foreach (var eleve in eleves)
            {
                var nomNormalise = NormalizeName(eleve.NomComplet);
                
                // Extraire les mots individuels (avant suppression des espaces)
                var mots = ExtractWords(eleve.NomComplet);
                var motsNormalises = mots.Select(m => NormalizeWord(m)).Where(m => !string.IsNullOrEmpty(m)).ToHashSet(StringComparer.OrdinalIgnoreCase);
                
                var info = new EleveInfo
                {
                    IdEleve = eleve.IdEleve,
                    IdClasse = eleve.IdClasse,
                    NomCompletOriginal = eleve.NomComplet ?? string.Empty,
                    NomNormalise = nomNormalise,
                    MotsNormalises = motsNormalises
                };
                
                elevesInfo[eleve.IdEleve] = info;
                
                // Ajouter au dictionnaire exact si pas de collision
                if (!dictionaryExact.ContainsKey(nomNormalise))
                {
                    dictionaryExact[nomNormalise] = eleve.IdEleve;
                }
                else
                {
                    // Collision détectée : plusieurs élèves ont le même nom normalisé
                    if (!collisions.ContainsKey(nomNormalise))
                    {
                        collisions[nomNormalise] = new List<int> { dictionaryExact[nomNormalise] };
                    }
                    collisions[nomNormalise].Add(eleve.IdEleve);
                    
                    _logger.LogWarning(
                        "⚠️ Collision de nom normalisé détectée : '{NomNormalise}' pour les élèves ID {Ids}. " +
                        "Seul le premier (ID: {PremierId}) sera utilisé pour la recherche exacte.",
                        nomNormalise, 
                        string.Join(", ", collisions[nomNormalise]),
                        dictionaryExact[nomNormalise]);
                }
            }
            
            if (collisions.Count > 0)
            {
                _logger.LogWarning($"⚠️ {collisions.Count} collision(s) de nom normalisé détectée(s) sur {eleves.Count()} élèves. " +
                    "La recherche par mots individuels sera utilisée pour ces cas.");
            }

            return (dictionaryExact, elevesInfo);
        }

        /// <summary>
        /// Structure pour stocker les informations d'un frais avec ses mots normalisés
        /// </summary>
        private class FraisInfo
        {
            public int IdFrais { get; set; }
            public int? IdClasse { get; set; }
            public string LibelleFraisOriginal { get; set; } = string.Empty;
            public string LibelleNormalise { get; set; } = string.Empty;
            public HashSet<string> MotsNormalises { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Charge tous les frais actifs d'une école en mémoire avec leurs informations de recherche
        /// </summary>
        private async Task<(Dictionary<string, int> DictionaryExact, Dictionary<int, FraisInfo> FraisInfo)> LoadFraisByEcoleAsync(int idEcole)
        {
            var idAnnee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, null);
            var frais = await _context.Frais
                .Where(f => f.Direction.IdEcole == idEcole
                    && f.IdAnneeScolaire == idAnnee
                    && f.Statut == true)
                .Select(f => new { f.IdFrais, f.LibelleFrais, f.IdClasse })
                .ToListAsync();

            // Dictionnaire pour recherche exacte (libellé normalisé sans espaces)
            var dictionaryExact = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            // Dictionnaire pour recherche par mots (contient toutes les infos)
            var fraisInfo = new Dictionary<int, FraisInfo>();
            var collisions = new Dictionary<string, List<int>>(); // Pour détecter les collisions
            
            foreach (var fraisItem in frais)
            {
                var libelleNormalise = NormalizeName(fraisItem.LibelleFrais);
                
                // Extraire les mots individuels (avant suppression des espaces)
                var mots = ExtractWords(fraisItem.LibelleFrais);
                var motsNormalises = mots.Select(m => NormalizeWord(m)).Where(m => !string.IsNullOrEmpty(m)).ToHashSet(StringComparer.OrdinalIgnoreCase);
                
                var info = new FraisInfo
                {
                    IdFrais = fraisItem.IdFrais,
                    IdClasse = fraisItem.IdClasse,
                    LibelleFraisOriginal = fraisItem.LibelleFrais ?? string.Empty,
                    LibelleNormalise = libelleNormalise,
                    MotsNormalises = motsNormalises
                };
                
                fraisInfo[fraisItem.IdFrais] = info;
                
                // Dictionnaire exact : préfère le frais direction-wide (IdClasse null) en index.
                // La résolution finale privilégie la classe de l'élève via PickBestFrais.
                if (!dictionaryExact.ContainsKey(libelleNormalise))
                {
                    dictionaryExact[libelleNormalise] = fraisItem.IdFrais;
                }
                else
                {
                    if (!collisions.ContainsKey(libelleNormalise))
                    {
                        collisions[libelleNormalise] = new List<int> { dictionaryExact[libelleNormalise] };
                    }
                    collisions[libelleNormalise].Add(fraisItem.IdFrais);

                    var current = fraisInfo[dictionaryExact[libelleNormalise]];
                    if (current.IdClasse.HasValue && !fraisItem.IdClasse.HasValue)
                    {
                        dictionaryExact[libelleNormalise] = fraisItem.IdFrais;
                    }
                    
                    _logger.LogWarning(
                        "⚠️ Collision de libellé normalisé détectée : '{LibelleNormalise}' pour les frais ID {Ids}. " +
                        "Résolution par classe élève (sinon IdClasse null).",
                        libelleNormalise, 
                        string.Join(", ", collisions[libelleNormalise]));
                }
            }
            
            if (collisions.Count > 0)
            {
                _logger.LogWarning(
                    "⚠️ {Count} collision(s) de libellé normalisé détectée(s) sur {Total} frais. " +
                    "La recherche par mots individuels sera utilisée pour ces cas.",
                    collisions.Count, frais.Count);
            }

            return (dictionaryExact, fraisInfo);
        }

        /// <summary>
        /// Extrait les mots individuels d'un nom (séparés par espaces ou caractères spéciaux)
        /// </summary>
        private List<string> ExtractWords(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return new List<string>();
            
            // Supprimer les accents d'abord
            var normalized = RemoveAccents(name);
            
            // Remplacer les caractères spéciaux par des espaces
            normalized = Regex.Replace(normalized, @"[^\w\s]", " ");
            
            // Normaliser les espaces multiples
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
            
            // Extraire les mots
            return normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        /// <summary>
        /// Normalise un mot individuel (supprime accents, caractères spéciaux, met en majuscules)
        /// </summary>
        private string NormalizeWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return string.Empty;
            
            // Supprimer les accents
            var normalized = RemoveAccents(word);
            
            // Supprimer les caractères non-alphanumériques
            normalized = Regex.Replace(normalized, @"[^\w]", "");
            
            // Mettre en majuscules
            return normalized.ToUpperInvariant();
        }

        /// <summary>
        /// Normalise un nom pour la comparaison robuste
        /// Gère : accents, caractères spéciaux, supprime les espaces entre les mots
        /// IMPORTANT : Respecte l'ordre des mots (pas de tri alphabétique)
        /// </summary>
        private string NormalizeName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            
            // 1. Supprimer les accents (é → e, è → e, etc.)
            var normalized = RemoveAccents(name);
            
            // 2. Remplacer les caractères spéciaux par des espaces (tirets, apostrophes, etc.)
            // Exemple : "Jean-Pierre" → "Jean Pierre"
            normalized = Regex.Replace(normalized, @"[^\w\s]", " ");
            
            // 3. Normaliser les espaces multiples en un seul espace, puis trim
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
            
            // 4. Supprimer TOUS les espaces entre les mots (garder l'ordre)
            // Exemple : "Jean Pierre MUKENDI" → "JeanPierreMUKENDI"
            normalized = normalized.Replace(" ", "");
            
            // 5. Mettre en majuscules
            return normalized.ToUpperInvariant();
        }

        /// <summary>
        /// Supprime les accents d'une chaîne de caractères
        /// Exemple : "José" → "Jose", "François" → "Francois"
        /// </summary>
        private string RemoveAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            
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
        /// Recherche un élève par son nom avec deux stratégies :
        /// 1. Recherche exacte (nom normalisé sans espaces)
        /// 2. Recherche par mots individuels (si recherche exacte échoue)
        /// </summary>
        private int? FindEleveByName(
            string nomRecherche,
            Dictionary<string, int> elevesEcoleDict,
            Dictionary<int, EleveInfo> elevesEcoleInfo)
        {
            if (string.IsNullOrWhiteSpace(nomRecherche))
                return null;

            // Étape 1 : Recherche exacte (nom normalisé sans espaces)
            var nomNormalise = NormalizeName(nomRecherche);
            if (elevesEcoleDict.TryGetValue(nomNormalise, out int idEleveExact))
            {
                _logger.LogDebug("✅ Élève trouvé (recherche exacte) : '{NomRecherche}' → ID {IdEleve}", nomRecherche, idEleveExact);
                return idEleveExact;
            }

            // Étape 2 : Recherche par mots individuels (si recherche exacte échoue)
            var motsRecherche = ExtractWords(nomRecherche);
            if (motsRecherche.Count < 2)
            {
                // Si moins de 2 mots, la recherche exacte aurait dû fonctionner
                return null;
            }

            var motsRechercheNormalises = motsRecherche
                .Select(m => NormalizeWord(m))
                .Where(m => !string.IsNullOrEmpty(m))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (motsRechercheNormalises.Count == 0)
                return null;

            // Chercher un élève qui contient TOUS les mots (peu importe l'ordre)
            var correspondances = elevesEcoleInfo
                .Where(kvp =>
                {
                    // Vérifier que tous les mots de recherche sont présents dans les mots de l'élève
                    return motsRechercheNormalises.All(motRecherche => 
                        kvp.Value.MotsNormalises.Contains(motRecherche));
                })
                .ToList();

            if (correspondances.Count == 0)
            {
                _logger.LogDebug("❌ Élève non trouvé : '{NomRecherche}' (recherche exacte et par mots échouées)", nomRecherche);
                return null;
            }

            if (correspondances.Count == 1)
            {
                _logger.LogInformation(
                    "✅ Élève trouvé (recherche par mots) : '{NomRecherche}' → ID {IdEleve} (Nom BDD: '{NomBDD}')",
                    nomRecherche, correspondances[0].Key, correspondances[0].Value.NomCompletOriginal);
                return correspondances[0].Key;
            }

            // Plusieurs correspondances trouvées
            _logger.LogWarning(
                "⚠️ Plusieurs élèves correspondants trouvés pour '{NomRecherche}' : {Ids}. " +
                "Le premier (ID: {PremierId}) sera utilisé.",
                nomRecherche,
                string.Join(", ", correspondances.Select(c => c.Key)),
                correspondances[0].Key);
            
            return correspondances[0].Key;
        }

        /// <summary>
        /// Recherche un frais par son libellé avec deux stratégies :
        /// 1. Recherche exacte (libellé normalisé sans espaces)
        /// 2. Recherche par mots individuels (si recherche exacte échoue)
        /// </summary>
        private int? FindFraisByName(
            string libelleRecherche,
            Dictionary<string, int> fraisEcoleDict,
            Dictionary<int, FraisInfo> fraisEcoleInfo,
            int? idClasseEleve = null)
        {
            if (string.IsNullOrWhiteSpace(libelleRecherche))
                return null;

            var libelleNormalise = NormalizeName(libelleRecherche);
            var exactMatches = fraisEcoleInfo.Values
                .Where(f => string.Equals(f.LibelleNormalise, libelleNormalise, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (exactMatches.Count > 0)
            {
                var picked = PickBestFrais(exactMatches, idClasseEleve);
                _logger.LogDebug("✅ Frais trouvé (recherche exacte) : '{LibelleRecherche}' → ID {IdFrais}", libelleRecherche, picked.IdFrais);
                return picked.IdFrais;
            }

            var motsRecherche = ExtractWords(libelleRecherche);
            if (motsRecherche.Count < 1)
                return null;

            var motsRechercheNormalises = motsRecherche
                .Select(m => NormalizeWord(m))
                .Where(m => !string.IsNullOrEmpty(m))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (motsRechercheNormalises.Count == 0)
                return null;

            var correspondances = fraisEcoleInfo.Values
                .Where(f => motsRechercheNormalises.All(mot => f.MotsNormalises.Contains(mot)))
                .ToList();

            if (correspondances.Count == 0)
            {
                _logger.LogDebug("❌ Frais non trouvé : '{LibelleRecherche}' (recherche exacte et par mots échouées)", libelleRecherche);
                return null;
            }

            var best = PickBestFrais(correspondances, idClasseEleve);
            _logger.LogInformation(
                "✅ Frais trouvé (recherche par mots) : '{LibelleRecherche}' → ID {IdFrais} (Libellé BDD: '{LibelleBDD}')",
                libelleRecherche, best.IdFrais, best.LibelleFraisOriginal);
            return best.IdFrais;
        }

        /// <summary>
        /// Préférence : frais spécifique à la classe de l'élève, sinon frais direction-wide (IdClasse null).
        /// </summary>
        private static FraisInfo PickBestFrais(List<FraisInfo> candidats, int? idClasseEleve)
        {
            IEnumerable<FraisInfo> eligible = candidats;
            if (idClasseEleve.HasValue && idClasseEleve.Value > 0)
            {
                eligible = candidats.Where(f => f.IdClasse == null || f.IdClasse == idClasseEleve.Value);
            }

            var list = eligible.ToList();
            if (list.Count == 0)
                list = candidats;

            if (idClasseEleve.HasValue && idClasseEleve.Value > 0)
            {
                var classSpecific = list.FirstOrDefault(f => f.IdClasse == idClasseEleve.Value);
                if (classSpecific != null)
                    return classSpecific;
            }

            return list.FirstOrDefault(f => f.IdClasse == null) ?? list[0];
        }

        /// <summary>
        /// Convertit les données brutes en PaiementExcelDto et enrichit avec les IDs
        /// </summary>
        private List<PaiementExcelDto> ConvertToPaiementExcelDto(
            List<PaiementExcelRaw> paiementsRaw,
            Dictionary<string, int> elevesEcoleDict,
            Dictionary<int, EleveInfo> elevesEcoleInfo,
            Dictionary<string, int> fraisEcoleDict,
            Dictionary<int, FraisInfo> fraisEcoleInfo,
            int idEcole)
        {
            var paiementsExcel = new List<PaiementExcelDto>();

            foreach (var raw in paiementsRaw)
            {
                var paiement = new PaiementExcelDto
                {
                    NumeroLigne = raw.NumeroLigne,
                    DatePaiement = raw.DatePaiement,
                    Montant = raw.Montant,
                    Devise = raw.Devise,
                    ModePaiement = raw.ModePaiement,
                    StatutPaiement = "Confirmé",
                    Statut = true
                };

                // Rechercher l'élève (recherche exacte + recherche par mots)
                var idEleve = FindEleveByName(raw.NomCompletEleve, elevesEcoleDict, elevesEcoleInfo);
                if (idEleve.HasValue)
                {
                    paiement.IdEleve = idEleve.Value;
                }
                else
                {
                    paiement.Erreurs.Add($"Élève '{raw.NomCompletEleve}' introuvable dans l'école {idEcole}");
                }

                int? idClasseEleve = null;
                if (idEleve.HasValue && elevesEcoleInfo.TryGetValue(idEleve.Value, out var eleveInfo))
                    idClasseEleve = eleveInfo.IdClasse;

                var idFrais = FindFraisByName(raw.LibelleFrais, fraisEcoleDict, fraisEcoleInfo, idClasseEleve);
                if (idFrais.HasValue)
                {
                    paiement.IdFrais = idFrais.Value;
                }
                else
                {
                    paiement.Erreurs.Add($"Frais '{raw.LibelleFrais}' introuvable dans l'école {idEcole}");
                }

                paiementsExcel.Add(paiement);
            }

            return paiementsExcel;
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
        /// Classe intermédiaire pour stocker les données Excel avant enrichissement
        /// </summary>
        private class PaiementExcelRaw
        {
            public int NumeroLigne { get; set; }
            public DateTime? DatePaiement { get; set; }
            public double? Montant { get; set; }
            public string Devise { get; set; } = "USD";
            public string ModePaiement { get; set; } = "Cash";
            public string NomCompletEleve { get; set; } = string.Empty;
            public string LibelleFrais { get; set; } = string.Empty;
        }

        /// <summary>
        /// Lit le fichier Excel et extrait les données
        /// </summary>
        private async Task<List<PaiementExcelRaw>> ReadExcelFileAsync(IFormFile file)
        {
            var paiements = new List<PaiementExcelRaw>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension?.Rows ?? 0;
                    var colCount = worksheet.Dimension?.Columns ?? 0;

                    if (rowCount < 2)
                    {
                        return paiements;
                    }

                    // Vérifier les en-têtes
                    var headers = new Dictionary<string, int>();
                    for (int col = 1; col <= colCount; col++)
                    {
                        var headerValue = worksheet.Cells[1, col].Text?.Trim();
                        if (!string.IsNullOrWhiteSpace(headerValue))
                        {
                            headers[headerValue] = col;
                        }
                    }

                    // Vérifier que toutes les colonnes requises sont présentes
                    var missingColumns = REQUIRED_COLUMNS.Where(c => !headers.ContainsKey(c)).ToList();
                    if (missingColumns.Any())
                    {
                        var errorMsg = $"Colonnes manquantes dans le fichier Excel : {string.Join(", ", missingColumns)}. Colonnes attendues : {string.Join(", ", REQUIRED_COLUMNS)}";
                        _logger.LogWarning($"⚠️ {errorMsg}");
                        throw new InvalidOperationException(errorMsg);
                    }

                    // Lire les données (lignes 2 à rowCount)
                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            var paiement = new PaiementExcelRaw
                            {
                                NumeroLigne = row,
                                DatePaiement = ParseDate(worksheet.Cells[row, headers["DatePaiement"]].Value),
                                Montant = ParseDouble(worksheet.Cells[row, headers["Montant"]].Value),
                                Devise = worksheet.Cells[row, headers["Devise"]].Text?.Trim() ?? "USD",
                                ModePaiement = worksheet.Cells[row, headers["ModePaiement"]].Text?.Trim() ?? "Cash",
                                NomCompletEleve = worksheet.Cells[row, headers["NomCompletEleve"]].Text?.Trim() ?? string.Empty,
                                LibelleFrais = worksheet.Cells[row, headers["LibelleFrais"]].Text?.Trim() ?? string.Empty
                            };

                            paiements.Add(paiement);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"⚠️ Erreur ligne {row}: {ex.Message}");
                        }
                    }
                }
            }

            return paiements;
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

        private double? ParseDouble(object? value)
        {
            if (value == null) return null;

            if (value is double dbl)
                return dbl;

            if (double.TryParse(value.ToString(), out double result))
                return result;

            return null;
        }

        /// <summary>
        /// Valide les données des paiements
        /// </summary>
        private void ValidatePaiements(List<PaiementExcelDto> paiements)
        {
            foreach (var paiement in paiements)
            {
                if (paiement.DatePaiement == null)
                {
                    paiement.Erreurs.Add("La date de paiement est obligatoire");
                }

                if (paiement.Montant == null || paiement.Montant <= 0)
                {
                    paiement.Erreurs.Add("Le montant doit être supérieur à 0");
                }

                if (!new[] { "USD", "CDF", "EUR" }.Contains(paiement.Devise))
                {
                    paiement.Erreurs.Add("La devise doit être USD, CDF ou EUR");
                }

                if (string.IsNullOrWhiteSpace(paiement.ModePaiement))
                {
                    paiement.Erreurs.Add("Le mode de paiement est obligatoire");
                }

                // Les erreurs d'élève/frais ont déjà été ajoutées dans EnrichWithIds
            }
        }

        /// <summary>
        /// Détecte et marque les doublons dans le fichier
        /// </summary>
        private void DeduplicateInFile(List<PaiementExcelDto> paiements, BulkPaiementResult result)
        {
            var seen = new HashSet<string>();

            foreach (var paiement in paiements)
            {
                if (paiement.DatePaiement == null || paiement.IdEleve == null || paiement.IdFrais == null)
                    continue;

                var key = $"{paiement.DatePaiement:yyyy-MM-dd}_{paiement.IdEleve}_{paiement.IdFrais}_{paiement.Montant}";

                if (seen.Contains(key))
                {
                    paiement.Erreurs.Add("Doublon détecté dans le fichier");
                    result.DoublonsDetectes++;
                }
                else
                {
                    seen.Add(key);
                }
            }
        }

        /// <summary>
        /// Traite les paiements par lots
        /// </summary>
        private async Task ProcessBatchesAsync(List<PaiementExcelDto> lignesValides, BulkPaiementResult result, int idUtilisateur)
        {
            var batches = lignesValides
                .Select((paiement, index) => new { paiement, index })
                .GroupBy(x => x.index / BATCH_SIZE)
                .Select(g => g.Select(x => x.paiement).ToList())
                .ToList();

            _logger.LogInformation($"📦 Traitement de {batches.Count} lot(s) de {BATCH_SIZE} paiements max");

            foreach (var batch in batches)
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var paiementDto in batch)
                        {
                            var paiement = paiementDto.ToPaiement();
                            paiement.IdUtilisateur = idUtilisateur;
                            
                            _context.Paiements.Add(paiement);
                            await _context.SaveChangesAsync();

                            result.PaiementsCrees.Add(paiement);
                            result.LignesReussies++;
                        }

                        await transaction.CommitAsync();
                        _logger.LogInformation($"✅ Lot traité : {batch.Count} paiements insérés");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, $"❌ Erreur lors du traitement d'un lot");

                        foreach (var paiementDto in batch)
                        {
                            paiementDto.Erreurs.Add($"Erreur d'insertion : {ex.Message}");
                            result.LignesAvecErreurs.Add(paiementDto);
                            result.LignesEchouees++;
                        }
                        
                        // Sauvegarder les paiements échoués du lot dans PaiementCrashed
                        if (_paiementsRawDict != null)
                        {
                            await SaveCrashedPaiementsAsync(batch, _paiementsRawDict, _idEcole, _idUtilisateur, _nomFichier ?? "unknown.xlsx");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sauvegarde les paiements échoués dans la table PaiementCrashed
        /// </summary>
        private async Task SaveCrashedPaiementsAsync(
            List<PaiementExcelDto> paiementsEchoues,
            Dictionary<int, PaiementExcelRaw> paiementsRawDict,
            int idEcole,
            int idUtilisateur,
            string nomFichier)
        {
            try
            {
                var paiementsCrashed = new List<PaiementCrashed>();

                foreach (var paiementDto in paiementsEchoues)
                {
                    // Récupérer les données brutes pour avoir le nom d'élève et libellé de frais originaux
                    var raw = paiementsRawDict.ContainsKey(paiementDto.NumeroLigne)
                        ? paiementsRawDict[paiementDto.NumeroLigne]
                        : null;

                    // ✅ CORRECTION : Sérialiser ErreursJson de manière sécurisée
                    string erreursJson;
                    try
                    {
                        erreursJson = paiementDto.Erreurs != null && paiementDto.Erreurs.Count > 0
                            ? JsonSerializer.Serialize(paiementDto.Erreurs)
                            : "[]";
                    }
                    catch (Exception jsonEx)
                    {
                        _logger.LogWarning(jsonEx, $"Erreur lors de la sérialisation des erreurs pour la ligne {paiementDto.NumeroLigne}");
                        erreursJson = "[]";
                    }

                    var paiementCrashed = new PaiementCrashed
                    {
                        DatePaiement = paiementDto.DatePaiement,
                        Montant = paiementDto.Montant,
                        Devise = paiementDto.Devise ?? "USD",
                        ModePaiement = paiementDto.ModePaiement,
                        Statut = paiementDto.Statut ?? true,
                        StatutPaiement = paiementDto.StatutPaiement ?? "Confirmé",
                        ReferenceTransaction = paiementDto.ReferenceTransaction,
                        JustificatifUrl = paiementDto.JustificatifUrl,
                        Commentaire = paiementDto.Commentaire,
                        IdEleve = paiementDto.IdEleve,
                        IdFrais = paiementDto.IdFrais,
                        IdUtilisateur = idUtilisateur,
                        NomCompletEleve = raw?.NomCompletEleve, // Nom original du fichier Excel
                        LibelleFrais = raw?.LibelleFrais, // Libellé original du fichier Excel
                        ErreursJson = erreursJson, // ✅ CORRECTION : Sérialisation sécurisée
                        NumeroLigne = paiementDto.NumeroLigne > 0 ? paiementDto.NumeroLigne : 0, // ✅ CORRECTION : Validation NumeroLigne
                        IdEcole = idEcole,
                        NomFichierOriginal = nomFichier,
                        DateEchec = DateTime.UtcNow, // ✅ CORRECTION : Utiliser UtcNow pour cohérence
                        EstResolu = false,
                        DateCreation = DateTime.UtcNow // ✅ CORRECTION : Utiliser UtcNow pour cohérence
                    };

                    paiementsCrashed.Add(paiementCrashed);
                }

                if (paiementsCrashed.Count > 0)
                {
                    _context.PaiementsCrashed.AddRange(paiementsCrashed);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"💾 {paiementsCrashed.Count} paiement(s) échoué(s) sauvegardé(s) dans PaiementCrashed");
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "❌ Erreur de base de données lors de la sauvegarde des paiements échoués");
                
                // Log des détails supplémentaires
                if (dbEx.InnerException != null)
                {
                    _logger.LogError($"Détails : {dbEx.InnerException.Message}");
                }
                
                // Ne pas faire échouer le traitement principal
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la sauvegarde des paiements échoués");
                // Ne pas faire échouer le traitement principal si la sauvegarde échoue
            }
        }

        /// <summary>
        /// Génère le message de résultat
        /// </summary>
        private string GenerateResultMessage(BulkPaiementResult result)
        {
            if (result.LignesReussies == result.TotalLignes)
            {
                return $"Traitement terminé : {result.LignesReussies} paiement(s) réussi(s) sur {result.TotalLignes} ligne(s)";
            }
            else if (result.LignesReussies == 0)
            {
                return $"Aucun paiement créé : {result.LignesEchouees} erreur(s) sur {result.TotalLignes} ligne(s)";
            }
            else
            {
                return $"Traitement terminé : {result.LignesReussies} paiement(s) réussi(s) sur {result.TotalLignes} ligne(s), {result.LignesEchouees} échoué(s)";
            }
        }

        /// <summary>
        /// Génère un fichier Excel template pour l'import
        /// </summary>
        public byte[] GenerateExcelTemplate()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Paiements");

                // En-têtes
                var headers = new[] { "DatePaiement", "Montant", "Devise", "ModePaiement", "NomCompletEleve", "LibelleFrais" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[1, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Exemples
                worksheet.Cells[2, 1].Value = DateTime.Now;
                worksheet.Cells[2, 1].Style.Numberformat.Format = "yyyy-mm-dd";
                worksheet.Cells[2, 2].Value = 100;
                worksheet.Cells[2, 3].Value = "USD";
                worksheet.Cells[2, 4].Value = "Cash";
                worksheet.Cells[2, 5].Value = "MUKENDI Jean Pierre";
                worksheet.Cells[2, 6].Value = "Minerval";

                worksheet.Cells[3, 1].Value = DateTime.Now;
                worksheet.Cells[3, 1].Style.Numberformat.Format = "yyyy-mm-dd";
                worksheet.Cells[3, 2].Value = 50;
                worksheet.Cells[3, 3].Value = "CDF";
                worksheet.Cells[3, 4].Value = "Mobile Money";
                worksheet.Cells[3, 5].Value = "KALALA Marie";
                worksheet.Cells[3, 6].Value = "Frais examen";

                // Auto-fit colonnes
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }
        }
    }
}

