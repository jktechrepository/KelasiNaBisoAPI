using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs.Reporting;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class PresenceReportingService : IPresenceReportingService
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly IInscriptionActiveResolver _inscriptionResolver;
        private readonly EleveAnneeScopeHelper _scope;
        private static readonly TimeSpan HeureRetardReference = TimeSpan.Parse("08:00");

        public PresenceReportingService(
            KelasiNaBisoDbContext context,
            ICacheService cacheService,
            IInscriptionActiveResolver inscriptionResolver,
            EleveAnneeScopeHelper scope)
        {
            _context = context;
            _cacheService = cacheService;
            _inscriptionResolver = inscriptionResolver;
            _scope = scope;
        }

        // ═══════════════════════════════════════════════════════
        // HELPERS - Gestion de la périodicité
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Convertit les paramètres de date en un objet PeriodeDto
        /// </summary>
        private PeriodeDto ResolvePeriode(DateTime? date, DateTime? dateDebut, DateTime? dateFin, string? periode)
        {
            DateTime debut, fin;
            string type, libelle;

            // Priorité : date unique > intervalle > période prédéfinie > mois en cours
            if (date.HasValue)
            {
                debut = fin = date.Value.Date;
                type = "jour";
                libelle = date.Value.ToString("dddd dd MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"));
            }
            else if (dateDebut.HasValue && dateFin.HasValue)
            {
                debut = dateDebut.Value.Date;
                fin = dateFin.Value.Date;
                type = "intervalle";
                libelle = $"{debut:dd MMM} - {fin:dd MMM yyyy}";
            }
            else if (!string.IsNullOrEmpty(periode))
            {
                (debut, fin, type, libelle) = GetPeriodePredéfinie(periode);
            }
            else
            {
                // Par défaut : mois en cours
                var aujourdhui = DateTime.Now;
                debut = new DateTime(aujourdhui.Year, aujourdhui.Month, 1);
                fin = new DateTime(aujourdhui.Year, aujourdhui.Month, DateTime.DaysInMonth(aujourdhui.Year, aujourdhui.Month));
                type = "mois";
                libelle = aujourdhui.ToString("MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"));
            }

            int joursOuvrables = CalculerJoursOuvrables(debut, fin);

            return new PeriodeDto
            {
                Type = type,
                Date = (type == "jour") ? debut : null,
                DateDebut = debut,
                DateFin = fin,
                JoursOuvrables = joursOuvrables,
                Libelle = libelle
            };
        }

        /// <summary>
        /// Calcule la période pour les périodes prédéfinies
        /// </summary>
        private (DateTime debut, DateTime fin, string type, string libelle) GetPeriodePredéfinie(string periode)
        {
            var aujourdhui = DateTime.Now.Date;

            return periode.ToLower() switch
            {
                "aujourdhui" or "aujourd'hui" => (
                    aujourdhui,
                    aujourdhui,
                    "jour",
                    aujourdhui.ToString("dddd dd MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"))
                ),

                "semaine" => (
                    aujourdhui.AddDays(-(int)aujourdhui.DayOfWeek + (int)DayOfWeek.Monday),
                    aujourdhui.AddDays(7 - (int)aujourdhui.DayOfWeek),
                    "semaine",
                    $"Semaine du {aujourdhui.AddDays(-(int)aujourdhui.DayOfWeek + 1):dd MMM}"
                ),

                "mois" => (
                    new DateTime(aujourdhui.Year, aujourdhui.Month, 1),
                    new DateTime(aujourdhui.Year, aujourdhui.Month, DateTime.DaysInMonth(aujourdhui.Year, aujourdhui.Month)),
                    "mois",
                    aujourdhui.ToString("MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"))
                ),

                "trimestre" => GetTrimestre(aujourdhui),

                "annee" => (
                    new DateTime(aujourdhui.Year, 1, 1),
                    new DateTime(aujourdhui.Year, 12, 31),
                    "annee",
                    aujourdhui.Year.ToString()
                ),

                _ => throw new ArgumentException($"Période invalide: {periode}. Valeurs acceptées: aujourdhui, semaine, mois, trimestre, annee")
            };
        }

        /// <summary>
        /// Calcule le trimestre en cours
        /// </summary>
        private (DateTime debut, DateTime fin, string type, string libelle) GetTrimestre(DateTime date)
        {
            int trimestre = ((date.Month - 1) / 3) + 1;
            int moisDebut = ((trimestre - 1) * 3) + 1;

            var debut = new DateTime(date.Year, moisDebut, 1);
            var fin = new DateTime(date.Year, moisDebut + 2, DateTime.DaysInMonth(date.Year, moisDebut + 2));

            return (debut, fin, "trimestre", $"Trimestre {trimestre} {date.Year}");
        }

        /// <summary>
        /// Calcule le nombre de jours ouvrables (lundi-vendredi) dans une période
        /// </summary>
        private int CalculerJoursOuvrables(DateTime debut, DateTime fin)
        {
            int jours = 0;
            for (var date = debut; date <= fin; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    jours++;
                }
            }
            return jours;
        }

        // ═══════════════════════════════════════════════════════
        // REPORTING ÉLÈVES - Implémentations
        // ═══════════════════════════════════════════════════════

        public async Task<PourcentageEleveDto> GetElevePourcentageAsync(
            int idEleve, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode)
        {
            var periodeDto = ResolvePeriode(null, dateDebut, dateFin, periode);

            // Récupérer l'élève avec inscriptions (classe via inscription active)
            var eleve = await _context.Eleves
                .Include(e => e.Inscriptions).ThenInclude(i => i.Classe)
                .FirstOrDefaultAsync(e => e.IdEleve == idEleve);

            if (eleve == null)
                throw new KeyNotFoundException($"Élève avec l'ID {idEleve} introuvable");

            var inscriptionActive = await _inscriptionResolver.GetInscriptionActiveAsync(idEleve);
            var nomClasse = inscriptionActive?.Classe?.NomClasse ?? "";

            // Récupérer les présences de l'élève sur la période
            var presences = await _context.Presences
                .Where(p => p.IdEleve == idEleve)
                .Where(p => p.DateDuJour.Date >= periodeDto.DateDebut && p.DateDuJour.Date <= periodeDto.DateFin)
                .Where(p => p.Statut == true)
                .ToListAsync();

            int nombrePresences = presences.Count(p => p.IsPresent == true);
            int nombreAbsences = periodeDto.JoursOuvrables - nombrePresences;
            int nombreRetards = presences.Count(p => p.HeureArrivee > TimeSpan.Parse("08:00")); // TODO: Rendre paramétrable
            int presencesPonctuelles = nombrePresences - nombreRetards;

            // Calcul des pourcentages
            decimal tauxPresence = periodeDto.JoursOuvrables > 0 
                ? Math.Round((decimal)nombrePresences / periodeDto.JoursOuvrables * 100, 2) 
                : 0;
            decimal tauxAbsence = periodeDto.JoursOuvrables > 0 
                ? Math.Round((decimal)nombreAbsences / periodeDto.JoursOuvrables * 100, 2) 
                : 0;
            decimal tauxRetard = periodeDto.JoursOuvrables > 0 
                ? Math.Round((decimal)nombreRetards / periodeDto.JoursOuvrables * 100, 2) 
                : 0;
            decimal tauxPonctualite = periodeDto.JoursOuvrables > 0 
                ? Math.Round((decimal)presencesPonctuelles / periodeDto.JoursOuvrables * 100, 2) 
                : 0;

            return new PourcentageEleveDto
            {
                Eleve = new EleveInfoDto
                {
                    IdEleve = eleve.IdEleve,
                    NomComplet = eleve.NomComplet ?? $"{eleve.Prenom} {eleve.Nom} {eleve.Postnom}",
                    Matricule = eleve.Matricule ?? "",
                    Classe = nomClasse,
                    PhotoUrl = eleve.PhotoUrl
                },
                Periode = periodeDto,
                Donnees = new DonneesBrutesDto
                {
                    JoursOuvrables = periodeDto.JoursOuvrables,
                    Presences = nombrePresences,
                    Absences = nombreAbsences,
                    Retards = nombreRetards,
                    PresencesPonctuelles = presencesPonctuelles
                },
                Pourcentages = new PourcentagesDto
                {
                    TauxPresence = tauxPresence,
                    TauxAbsence = tauxAbsence,
                    TauxRetard = tauxRetard,
                    TauxPonctualite = tauxPonctualite
                }
            };
        }

        public async Task<RetardsEleveDto> GetEleveRetardsAsync(
            int idEleve, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode, 
            TimeSpan heureReference)
        {
            var periodeDto = ResolvePeriode(null, dateDebut, dateFin, periode);

            var eleve = await _context.Eleves
                .Include(e => e.Inscriptions).ThenInclude(i => i.Classe)
                .FirstOrDefaultAsync(e => e.IdEleve == idEleve);

            if (eleve == null)
                throw new KeyNotFoundException($"Élève avec l'ID {idEleve} introuvable");

            var inscriptionActive = await _inscriptionResolver.GetInscriptionActiveAsync(idEleve);
            var nomClasse = inscriptionActive?.Classe?.NomClasse ?? "";

            // Récupérer les présences avec retards
            var presences = await _context.Presences
                .Where(p => p.IdEleve == idEleve)
                .Where(p => p.DateDuJour.Date >= periodeDto.DateDebut && p.DateDuJour.Date <= periodeDto.DateFin)
                .Where(p => p.Statut == true && p.IsPresent == true)
                .ToListAsync();

            var retards = presences
                .Where(p => p.HeureArrivee > heureReference)
                .ToList();

            // Calculer les statistiques
            var nombreRetards = retards.Count;
            var nombrePresences = presences.Count;
            var pourcentageRetards = nombrePresences > 0 
                ? Math.Round((decimal)nombreRetards / nombrePresences * 100, 2) 
                : 0;

            var dureesRetards = retards
                .Select(r => (r.HeureArrivee - heureReference).TotalMinutes)
                .ToList();

            var dureeMoyenne = dureesRetards.Any() 
                ? $"{Math.Round(dureesRetards.Average(), 0)} minutes" 
                : "0 minutes";
            var dureeMax = dureesRetards.Any() 
                ? $"{Math.Round(dureesRetards.Max(), 0)} minutes" 
                : "0 minutes";

            // Jour de semaine le plus fréquent
            var joursFrequents = retards
                .GroupBy(r => r.DateDuJour.ToString("dddd", new System.Globalization.CultureInfo("fr-FR")))
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            return new RetardsEleveDto
            {
                Eleve = new EleveInfoDto
                {
                    IdEleve = eleve.IdEleve,
                    NomComplet = eleve.NomComplet ?? $"{eleve.Prenom} {eleve.Nom}",
                    Matricule = eleve.Matricule ?? "",
                    Classe = nomClasse
                },
                Periode = periodeDto,
                HeureReference = heureReference,
                Statistiques = new StatistiquesRetardsDto
                {
                    NombreTotalRetards = nombreRetards,
                    NombrePresences = nombrePresences,
                    PourcentageRetards = pourcentageRetards,
                    DureeMoyenneRetard = dureeMoyenne,
                    DureeMaxRetard = dureeMax,
                    JourSemaineFrequent = joursFrequents?.Key
                },
                ListeRetards = retards.Select(r => new DetailRetardDto
                {
                    Date = r.DateDuJour,
                    JourSemaine = r.DateDuJour.ToString("dddd", new System.Globalization.CultureInfo("fr-FR")),
                    HeureReference = heureReference,
                    HeureArrivee = r.HeureArrivee,
                    DureeRetard = $"{Math.Round((r.HeureArrivee - heureReference).TotalMinutes, 0)} minutes",
                    Observation = r.Observation
                }).ToList()
            };
        }

        public async Task<ClassePresenceDto> GetClassePresencesAsync(
            int idClasse, 
            DateTime? date, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode, 
            bool includeDetails)
        {
            var periodeDto = ResolvePeriode(date, dateDebut, dateFin, periode);

            // Récupérer la classe avec ses relations
            var classe = await _context.Classes
                .Include(c => c.Section)
                .Include(c => c.Option)
                .Include(c => c.Direction)
                .ThenInclude(d => d.Ecole)
                .FirstOrDefaultAsync(c => c.IdClasse == idClasse);

            if (classe == null)
                throw new KeyNotFoundException($"Classe avec l'ID {idClasse} introuvable");

            // Effectif total de la classe
            var elevesClasseQuery = _inscriptionResolver.FilterElevesInClasse(_context.Eleves, idClasse);
            var effectifTotal = await elevesClasseQuery.CountAsync();
            var eleveIds = await elevesClasseQuery.Select(e => e.IdEleve).ToListAsync();

            // Récupérer les présences de tous les élèves de la classe sur la période
            var presencesClasse = await _context.Presences
                .Include(p => p.Eleve)
                .Where(p => p.IdEleve != null && eleveIds.Contains(p.IdEleve.Value))
                .Where(p => p.DateDuJour.Date >= periodeDto.DateDebut && p.DateDuJour.Date <= periodeDto.DateFin)
                .Where(p => p.Statut == true)
                .ToListAsync();

            int presencesAttendue = effectifTotal * periodeDto.JoursOuvrables;
            int presencesEffectives = presencesClasse.Count(p => p.IsPresent == true);
            int absences = presencesAttendue - presencesEffectives;
            TimeSpan heureRef = TimeSpan.Parse("08:00"); // TODO: Paramétrable
            int retards = presencesClasse.Count(p => p.HeureArrivee > heureRef);

            decimal tauxPresence = presencesAttendue > 0 
                ? Math.Round((decimal)presencesEffectives / presencesAttendue * 100, 2) 
                : 0;
            decimal tauxAbsence = presencesAttendue > 0 
                ? Math.Round((decimal)absences / presencesAttendue * 100, 2) 
                : 0;
            decimal tauxRetard = presencesAttendue > 0 
                ? Math.Round((decimal)retards / presencesAttendue * 100, 2) 
                : 0;

            var result = new ClassePresenceDto
            {
                Classe = new ClasseInfoDto
                {
                    IdClasse = classe.IdClasse,
                    NomClasse = classe.NomClasse ?? "",
                    Section = classe.Section?.NomSection,
                    Option = classe.Option?.NomOption,
                    Direction = classe.Direction?.NomDirection,
                    Ecole = classe.Direction?.Ecole?.Nom,
                    EffectifTotal = effectifTotal
                },
                Periode = periodeDto,
                Statistiques = new StatistiquesClasseDto
                {
                    PresencesAttendue = presencesAttendue,
                    PresencesEffectives = presencesEffectives,
                    Absences = absences,
                    Retards = retards,
                    TauxPresence = tauxPresence,
                    TauxAbsence = tauxAbsence,
                    TauxRetard = tauxRetard
                }
            };

            // Ajouter les détails par élève si demandé
            if (includeDetails)
            {
                var elevesClasse = await elevesClasseQuery.ToListAsync();

                result.ParEleve = new List<ElevePresenceDto>();

                foreach (var elv in elevesClasse)
                {
                    var presencesEleve = presencesClasse.Where(p => p.IdEleve == elv.IdEleve).ToList();
                    int presEleve = presencesEleve.Count(p => p.IsPresent == true);
                    int absEleve = periodeDto.JoursOuvrables - presEleve;
                    int retardsEleve = presencesEleve.Count(p => p.HeureArrivee > heureRef);

                    decimal tauxPresEleve = periodeDto.JoursOuvrables > 0 
                        ? Math.Round((decimal)presEleve / periodeDto.JoursOuvrables * 100, 2) 
                        : 0;
                    decimal tauxRetardEleve = periodeDto.JoursOuvrables > 0 
                        ? Math.Round((decimal)retardsEleve / periodeDto.JoursOuvrables * 100, 2) 
                        : 0;

                    result.ParEleve.Add(new ElevePresenceDto
                    {
                        Eleve = new EleveInfoDto
                        {
                            IdEleve = elv.IdEleve,
                            NomComplet = elv.NomComplet ?? $"{elv.Prenom} {elv.Nom}",
                            Matricule = elv.Matricule ?? ""
                        },
                        Presences = presEleve,
                        Absences = absEleve,
                        Retards = retardsEleve,
                        TauxPresence = tauxPresEleve,
                        TauxRetard = tauxRetardEleve
                    });
                }
            }

            // Ajouter les statistiques retards si période >= 1 jour
            if (periodeDto.JoursOuvrables > 0 && retards > 0)
            {
                var retardsPresences = presencesClasse.Where(p => p.HeureArrivee > heureRef).ToList();
                var elevesConcernes = retardsPresences.Select(p => p.IdEleve).Distinct().Count();
                
                var durees = retardsPresences
                    .Select(r => (r.HeureArrivee - heureRef).TotalMinutes)
                    .ToList();

                var dureeMoyenne = durees.Any() 
                    ? $"{Math.Round(durees.Average(), 0)} minutes" 
                    : "0 minutes";

                result.StatistiquesRetards = new StatistiquesRetardsClasseDto
                {
                    NombreRetards = retards,
                    ElevesConcernes = elevesConcernes,
                    PourcentageElevesRetardataires = effectifTotal > 0 
                        ? Math.Round((decimal)elevesConcernes / effectifTotal * 100, 2) 
                        : 0,
                    DureeMoyenne = dureeMoyenne
                };
            }

            return result;
        }

        /// <summary>
        /// Feuille d'appel nominative pour une classe et une date.
        /// </summary>
        public async Task<FeuilleAppelClasseDto> GetFeuilleAppelAsync(
            int idClasse,
            DateTime date,
            int? idAnneeScolaire = null)
        {
            var jour = date.Date;

            var classe = await _context.Classes
                .AsNoTracking()
                .Include(c => c.Direction!)
                    .ThenInclude(d => d.Ecole)
                .FirstOrDefaultAsync(c => c.IdClasse == idClasse);

            if (classe == null)
                throw new KeyNotFoundException($"Classe avec l'ID {idClasse} introuvable");

            var idEcole = classe.Direction?.IdEcole
                ?? throw new InvalidOperationException(
                    $"Classe {idClasse} introuvable ou non rattachée à une école.");

            if (idEcole <= 0)
                throw new InvalidOperationException(
                    $"Classe {idClasse} introuvable ou non rattachée à une école.");

            var nomEcole = classe.Direction?.Ecole?.Nom;
            var resolvedAnnee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, idAnneeScolaire);

            var eleves = await _inscriptionResolver
                .FilterElevesInClasse(_context.Eleves.AsNoTracking(), idClasse, resolvedAnnee)
                .OrderBy(e => e.NomComplet)
                .ToListAsync();

            var eleveIds = eleves.Select(e => e.IdEleve).ToList();

            var presencesDuJour = eleveIds.Count == 0
                ? new List<Presence>()
                : await _context.Presences
                    .AsNoTracking()
                    .Where(p => p.IdEleve != null
                        && eleveIds.Contains(p.IdEleve.Value)
                        && p.DateDuJour.Date == jour
                        && p.Statut == true)
                    .ToListAsync();

            var presenceByEleve = presencesDuJour
                .GroupBy(p => p.IdEleve!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(p => p.DateCreation).First());

            var lignes = new List<FeuilleAppelLigneDto>();
            foreach (var eleve in eleves)
            {
                presenceByEleve.TryGetValue(eleve.IdEleve, out var presence);
                var statutJour = ResolveStatutJour(presence);

                lignes.Add(new FeuilleAppelLigneDto
                {
                    IdEleve = eleve.IdEleve,
                    Matricule = eleve.Matricule,
                    NomComplet = eleve.NomComplet ?? $"{eleve.Prenom} {eleve.Nom}".Trim(),
                    Genre = eleve.Genre,
                    StatutJour = statutJour,
                    IdPresence = presence?.IdPresence,
                    IsPresent = presence?.IsPresent,
                    HeureArrivee = presence != null ? presence.HeureArrivee : null,
                    HeureDepart = presence?.HeureDepart,
                    Observation = presence?.Observation
                });
            }

            return new FeuilleAppelClasseDto
            {
                IdClasse = classe.IdClasse,
                NomClasse = classe.NomClasse ?? string.Empty,
                IdEcole = idEcole,
                NomEcole = nomEcole,
                IdAnneeScolaire = resolvedAnnee,
                Date = jour,
                Effectif = lignes.Count,
                NbPresents = lignes.Count(l => l.StatutJour == FeuilleAppelStatutJour.Present),
                NbAbsents = lignes.Count(l => l.StatutJour == FeuilleAppelStatutJour.Absent),
                NbRetards = lignes.Count(l => l.StatutJour == FeuilleAppelStatutJour.Retard),
                Lignes = lignes
            };
        }

        /// <summary>
        /// Feuille d'appel nominative pour les agents d'une école et une date.
        /// </summary>
        public async Task<FeuilleAppelAgentsDto> GetFeuilleAppelAgentsAsync(
            int idEcole,
            DateTime date,
            string? fonction = null)
        {
            var jour = date.Date;

            var ecole = await _context.Ecoles
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEcole == idEcole);

            if (ecole == null)
                throw new KeyNotFoundException($"École avec l'ID {idEcole} introuvable");

            var agentsQuery = _context.Agents
                .AsNoTracking()
                .Where(a => a.IdEcole == idEcole && a.Statut == true);

            if (!string.IsNullOrWhiteSpace(fonction))
            {
                var fonctionNorm = fonction.Trim();
                agentsQuery = agentsQuery.Where(a => a.Fonction == fonctionNorm);
            }

            var agents = await agentsQuery
                .OrderBy(a => a.Nom)
                .ThenBy(a => a.Postnom)
                .ThenBy(a => a.Prenom)
                .ToListAsync();

            var agentIds = agents.Select(a => a.IdAgent).ToList();

            var presencesDuJour = agentIds.Count == 0
                ? new List<Presence>()
                : await _context.Presences
                    .AsNoTracking()
                    .Where(p => p.IdAgent != null
                        && agentIds.Contains(p.IdAgent.Value)
                        && p.DateDuJour.Date == jour
                        && p.Statut == true)
                    .ToListAsync();

            var presenceByAgent = presencesDuJour
                .GroupBy(p => p.IdAgent!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(p => p.DateCreation).First());

            var lignes = new List<FeuilleAppelAgentLigneDto>();
            foreach (var agent in agents)
            {
                presenceByAgent.TryGetValue(agent.IdAgent, out var presence);
                var statutJour = ResolveStatutJour(presence);
                var nomComplet = string.Join(" ",
                    new[] { agent.Prenom, agent.Nom, agent.Postnom }
                        .Where(s => !string.IsNullOrWhiteSpace(s))).Trim();

                lignes.Add(new FeuilleAppelAgentLigneDto
                {
                    IdAgent = agent.IdAgent,
                    Matricule = agent.Matricule,
                    NomComplet = nomComplet,
                    Fonction = agent.Fonction,
                    Genre = agent.Genre,
                    StatutJour = statutJour,
                    IdPresence = presence?.IdPresence,
                    IsPresent = presence?.IsPresent,
                    HeureArrivee = presence != null ? presence.HeureArrivee : null,
                    HeureDepart = presence?.HeureDepart,
                    Observation = presence?.Observation
                });
            }

            return new FeuilleAppelAgentsDto
            {
                IdEcole = idEcole,
                NomEcole = ecole.Nom,
                FonctionFiltre = string.IsNullOrWhiteSpace(fonction) ? null : fonction.Trim(),
                Date = jour,
                Effectif = lignes.Count,
                NbPresents = lignes.Count(l => l.StatutJour == FeuilleAppelStatutJour.Present),
                NbAbsents = lignes.Count(l => l.StatutJour == FeuilleAppelStatutJour.Absent),
                NbRetards = lignes.Count(l => l.StatutJour == FeuilleAppelStatutJour.Retard),
                Lignes = lignes
            };
        }

        private static string ResolveStatutJour(Presence? presence)
        {
            if (presence == null || presence.IsPresent != true)
                return FeuilleAppelStatutJour.Absent;

            if (presence.HeureArrivee > HeureRetardReference)
                return FeuilleAppelStatutJour.Retard;

            return FeuilleAppelStatutJour.Present;
        }

        // ═══════════════════════════════════════════════════════
        // REPORTING AGENTS - Implémentations
        // ═══════════════════════════════════════════════════════

        public async Task<PourcentageAgentDto> GetAgentPourcentageAsync(
            int idAgent, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode)
        {
            var periodeDto = ResolvePeriode(null, dateDebut, dateFin, periode);

            var agent = await _context.Agents
                .Include(a => a.Ecole)
                .FirstOrDefaultAsync(a => a.IdAgent == idAgent);

            if (agent == null)
                throw new KeyNotFoundException($"Agent avec l'ID {idAgent} introuvable");

            var presences = await _context.Presences
                .Where(p => p.IdAgent == idAgent)
                .Where(p => p.DateDuJour.Date >= periodeDto.DateDebut && p.DateDuJour.Date <= periodeDto.DateFin)
                .Where(p => p.Statut == true)
                .ToListAsync();

            int nombrePresences = presences.Count(p => p.IsPresent == true);
            int nombreAbsences = periodeDto.JoursOuvrables - nombrePresences;
            TimeSpan heureRef = TimeSpan.Parse("07:30");
            int nombreRetards = presences.Count(p => p.HeureArrivee > heureRef);
            int presencesPonctuelles = nombrePresences - nombreRetards;

            decimal tauxPresence = periodeDto.JoursOuvrables > 0 
                ? Math.Round((decimal)nombrePresences / periodeDto.JoursOuvrables * 100, 2) 
                : 0;
            decimal tauxAbsence = periodeDto.JoursOuvrables > 0 
                ? Math.Round((decimal)nombreAbsences / periodeDto.JoursOuvrables * 100, 2) 
                : 0;
            decimal tauxRetard = periodeDto.JoursOuvrables > 0 
                ? Math.Round((decimal)nombreRetards / periodeDto.JoursOuvrables * 100, 2) 
                : 0;
            decimal tauxPonctualite = periodeDto.JoursOuvrables > 0 
                ? Math.Round((decimal)presencesPonctuelles / periodeDto.JoursOuvrables * 100, 2) 
                : 0;

            // Calcul des heures travaillées
            var presencesAvecDepart = presences.Where(p => p.HeureDepart.HasValue).ToList();
            var totalHeures = presencesAvecDepart
                .Sum(p => (p.HeureDepart!.Value - p.HeureArrivee).TotalHours);

            var heuresMoyennesArrivee = presences.Any() 
                ? TimeSpan.FromMinutes(presences.Average(p => p.HeureArrivee.TotalMinutes)) 
                : TimeSpan.Zero;
            var heuresMoyennesDepart = presencesAvecDepart.Any() 
                ? TimeSpan.FromMinutes(presencesAvecDepart.Average(p => p.HeureDepart!.Value.TotalMinutes)) 
                : TimeSpan.Zero;

            return new PourcentageAgentDto
            {
                Agent = new AgentInfoDto
                {
                    IdAgent = agent.IdAgent,
                    NomComplet = $"{agent.Prenom} {agent.Nom} {agent.Postnom}",
                    Matricule = agent.Matricule ?? "",
                    Fonction = agent.Fonction ?? "",
                    EmailAgent = agent.EmailAgent,
                    PhotoUrl = agent.PhotoUrl
                },
                Periode = periodeDto,
                Donnees = new DonneesBrutesDto
                {
                    JoursOuvrables = periodeDto.JoursOuvrables,
                    Presences = nombrePresences,
                    Absences = nombreAbsences,
                    Retards = nombreRetards,
                    PresencesPonctuelles = presencesPonctuelles
                },
                Pourcentages = new PourcentagesDto
                {
                    TauxPresence = tauxPresence,
                    TauxAbsence = tauxAbsence,
                    TauxRetard = tauxRetard,
                    TauxPonctualite = tauxPonctualite
                },
                Heures = new HeuresDto
                {
                    TotalHeuresTravaillees = Math.Round((decimal)totalHeures, 2),
                    HeureMoyenneArrivee = heuresMoyennesArrivee,
                    HeureMoyenneDepart = heuresMoyennesDepart,
                    DureeMoyenneJournaliere = presencesAvecDepart.Any() 
                        ? TimeSpan.FromHours(totalHeures / presencesAvecDepart.Count) 
                        : TimeSpan.Zero
                }
            };
        }

        public async Task<FonctionPresenceDto> GetFonctionPresencesAsync(
            string fonction, 
            int idEcole, 
            DateTime? date, 
            DateTime? dateDebut, 
            DateTime? dateFin, 
            string? periode)
        {
            var periodeDto = ResolvePeriode(date, dateDebut, dateFin, periode);

            var ecole = await _context.Ecoles.FindAsync(idEcole);
            if (ecole == null)
                throw new KeyNotFoundException($"École avec l'ID {idEcole} introuvable");

            // Récupérer tous les agents de cette fonction dans cette école
            var agents = await _context.Agents
                .Where(a => a.Fonction == fonction && a.IdEcole == idEcole && a.Statut == true)
                .ToListAsync();

            var effectifTotal = agents.Count;

            // Récupérer toutes les présences de ces agents sur la période
            var idsAgents = agents.Select(a => a.IdAgent).ToList();
            var presences = await _context.Presences
                .Where(p => idsAgents.Contains(p.IdAgent!.Value))
                .Where(p => p.DateDuJour.Date >= periodeDto.DateDebut && p.DateDuJour.Date <= periodeDto.DateFin)
                .Where(p => p.Statut == true)
                .ToListAsync();

            int presencesAttendue = effectifTotal * periodeDto.JoursOuvrables;
            int presencesEffectives = presences.Count(p => p.IsPresent == true);
            int absences = presencesAttendue - presencesEffectives;
            TimeSpan heureRef = TimeSpan.Parse("07:30");
            int retards = presences.Count(p => p.HeureArrivee > heureRef);

            decimal tauxPresence = presencesAttendue > 0 
                ? Math.Round((decimal)presencesEffectives / presencesAttendue * 100, 2) 
                : 0;
            decimal tauxAbsence = presencesAttendue > 0 
                ? Math.Round((decimal)absences / presencesAttendue * 100, 2) 
                : 0;
            decimal tauxRetard = presencesAttendue > 0 
                ? Math.Round((decimal)retards / presencesAttendue * 100, 2) 
                : 0;

            return new FonctionPresenceDto
            {
                Fonction = fonction,
                Ecole = ecole.Nom ?? "",
                Periode = periodeDto,
                Statistiques = new StatistiquesFonctionDto
                {
                    EffectifTotal = effectifTotal,
                    PresencesAttendue = presencesAttendue,
                    PresencesEffectives = presencesEffectives,
                    Absences = absences,
                    Retards = retards,
                    TauxPresence = tauxPresence,
                    TauxAbsence = tauxAbsence,
                    TauxRetard = tauxRetard
                }
            };
        }

        public async Task<DashboardPresenceDto> GetDashboardEcoleAsync(
            int idEcole, 
            DateTime? date, 
            DateTime? dateDebut, 
            DateTime? dateFin,
            int? idAnneeScolaire = null)
        {
            var periodeDto = ResolvePeriode(date, dateDebut, dateFin, null);
            var idAnnee = await _scope.ResolveIdAnneeScolaireAsync(idEcole, idAnneeScolaire);
            string cacheKey = $"dashboard_presence_{idEcole}_{idAnnee}_{periodeDto.DateDebut:yyyyMMdd}_{periodeDto.DateFin:yyyyMMdd}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await CalculerDashboardPresenceAsync(idEcole, idAnnee, periodeDto);
            }, TimeSpan.FromMinutes(5));
        }

        private async Task<DashboardPresenceDto> CalculerDashboardPresenceAsync(
            int idEcole,
            int idAnneeScolaire,
            PeriodeDto periodeDto)
        {
            var ecole = await _context.Ecoles.FindAsync(idEcole);
            if (ecole == null)
                throw new KeyNotFoundException($"École avec l'ID {idEcole} introuvable");

            // ÉLÈVES
            var elevesEcoleIds = await _inscriptionResolver
                .FilterElevesInEcole(_context.Eleves, idEcole, idAnneeScolaire)
                .Select(e => e.IdEleve)
                .ToListAsync();
            var effectifEleves = elevesEcoleIds.Count;

            var presencesEleves = await _context.Presences
                .Include(p => p.Eleve)
                .Where(p => p.IdEleve != null && elevesEcoleIds.Contains(p.IdEleve.Value))
                .Where(p => p.DateDuJour.Date >= periodeDto.DateDebut && p.DateDuJour.Date <= periodeDto.DateFin)
                .Where(p => p.Statut == true)
                .ToListAsync();

            int presencesElevesEffectives = presencesEleves.Count(p => p.IsPresent == true);
            // ✅ FIX: Compter uniquement les absences enregistrées (IsPresent = false)
            int absencesEleves = presencesEleves.Count(p => p.IsPresent == false);
            int retardsEleves = presencesEleves.Count(p => p.HeureArrivee > TimeSpan.Parse("08:00"));

            decimal tauxPresenceEleves = effectifEleves * periodeDto.JoursOuvrables > 0 
                ? Math.Round((decimal)presencesElevesEffectives / (effectifEleves * periodeDto.JoursOuvrables) * 100, 2) 
                : 0;

            // AGENTS
            var effectifAgents = await _context.Agents
                .Where(a => a.IdEcole == idEcole && a.Statut == true)
                .CountAsync();

            var presencesAgents = await _context.Presences
                .Include(p => p.Agent)
                    .ThenInclude(a => a.AffectationsCours)
                .Where(p => p.IdAgent != null)
                .Where(p => p.Agent!.IdEcole == idEcole)
                .Where(p => p.DateDuJour.Date >= periodeDto.DateDebut && p.DateDuJour.Date <= periodeDto.DateFin)
                .Where(p => p.Statut == true)
                .ToListAsync();

            int presencesAgentsEffectives = presencesAgents.Count(p => p.IsPresent == true);
            // ✅ FIX: Compter uniquement les absences enregistrées (IsPresent = false)
            int absencesAgents = presencesAgents.Count(p => p.IsPresent == false);
            int retardsAgents = presencesAgents.Count(p => p.HeureArrivee > TimeSpan.Parse("07:30"));

            decimal tauxPresenceAgents = effectifAgents * periodeDto.JoursOuvrables > 0 
                ? Math.Round((decimal)presencesAgentsEffectives / (effectifAgents * periodeDto.JoursOuvrables) * 100, 2) 
                : 0;

            // ✅ NOUVEAU: Identifier les classes problématiques (taux < 75%)
            var classesProblematiques = new List<ClasseProblematiqueDto>();
            var inscriptionsActives = await _context.Inscriptions
                .AsNoTracking()
                .Include(i => i.Classe)
                .Where(i => i.Statut == true && i.IdEcole == idEcole)
                .Where(i => i.StatutInscription != null
                    && (i.StatutInscription == InscriptionActiveRules.StatutConfirme
                        || i.StatutInscription == "Confirme"
                        || i.StatutInscription.StartsWith("Confirm")))
                .ToListAsync();

            var inscriptionParEleve = inscriptionsActives
                .GroupBy(i => i.IdEleve)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(i => i.DateInscription).First());

            var classeStats = presencesEleves
                .Where(p => p.IdEleve.HasValue && inscriptionParEleve.ContainsKey(p.IdEleve.Value))
                .GroupBy(p =>
                {
                    var ins = inscriptionParEleve[p.IdEleve!.Value];
                    return new { ins.IdClasse, NomClasse = ins.Classe?.NomClasse };
                })
                .Select(g => new
                {
                    IdClasse = g.Key.IdClasse,
                    g.Key.NomClasse,
                    Presents = g.Count(p => p.IsPresent == true),
                    Absents = g.Count(p => p.IsPresent == false),
                    Total = g.Count()
                })
                .Where(c => c.Total > 0 && c.IdClasse > 0)
                .ToList();

            foreach (var classe in classeStats)
            {
                decimal tauxPresenceClasse = Math.Round((decimal)classe.Presents / classe.Total * 100, 2);
                string status = tauxPresenceClasse < 60 ? "critique" 
                              : tauxPresenceClasse < 75 ? "attention" 
                              : "normal";

                if (status != "normal")
                {
                    classesProblematiques.Add(new ClasseProblematiqueDto
                    {
                        IdClasse = classe.IdClasse,
                        NomClasse = classe.NomClasse ?? "",
                        Presents = classe.Presents,
                        Absents = classe.Absents,
                        TauxPresence = tauxPresenceClasse,
                        Status = status
                    });
                }
            }

            // ✅ NOUVEAU: Identifier les agents absents avec cours affectés
            var agentsPresentsIds = presencesAgents
                .Where(p => p.IsPresent == true)
                .Select(p => p.IdAgent)
                .Distinct()
                .ToHashSet();

            var agentsAbsents = new List<AgentAbsentDto>();
            var todayAgentsAbsents = await _context.Agents
                .Include(a => a.AffectationsCours)
                .Where(a => a.IdEcole == idEcole && a.Statut == true)
                .Where(a => !agentsPresentsIds.Contains(a.IdAgent))
                .Where(a => a.AffectationsCours.Any(ac => ac.Statut == true))
                .ToListAsync();

            foreach (var agent in todayAgentsAbsents)
            {
                int coursAffectes = agent.AffectationsCours.Count(ac => ac.Statut == true);
                agentsAbsents.Add(new AgentAbsentDto
                {
                    IdAgent = agent.IdAgent,
                    NomComplet = $"{agent.Prenom} {agent.Nom}".Trim(),
                    Fonction = agent.Fonction ?? "Non spécifié",
                    CoursAffectes = coursAffectes,
                    Status = coursAffectes > 0 ? "critique" : "normal"
                });
            }

            // ✅ NOUVEAU: Générer les alertes
            var alertes = new List<AlerteDto>();

            // Alerte si taux de présence élèves critique (< 70%)
            if (tauxPresenceEleves < 70)
            {
                alertes.Add(new AlerteDto
                {
                    Type = "danger",
                    Message = $"⚠️ Taux de présence élèves critique : {tauxPresenceEleves}%",
                    Action = "Contacter les parents des élèves absents"
                });
            }
            else if (tauxPresenceEleves < 85)
            {
                alertes.Add(new AlerteDto
                {
                    Type = "warning",
                    Message = $"⚡ Taux de présence élèves sous la normale : {tauxPresenceEleves}%",
                    Action = "Surveiller les absences répétées"
                });
            }

            // Alerte si taux de présence agents critique (< 80%)
            if (tauxPresenceAgents < 80)
            {
                alertes.Add(new AlerteDto
                {
                    Type = "danger",
                    Message = $"⚠️ Taux de présence agents critique : {tauxPresenceAgents}%",
                    Action = "Vérifier les absences non justifiées"
                });
            }

            // Alerte pour les classes problématiques
            if (classesProblematiques.Any(c => c.Status == "critique"))
            {
                var nbClassesCritiques = classesProblematiques.Count(c => c.Status == "critique");
                alertes.Add(new AlerteDto
                {
                    Type = "danger",
                    Message = $"🚨 {nbClassesCritiques} classe(s) avec taux de présence < 60%",
                    Action = "Contacter les titulaires de classe concernés"
                });
            }

            // Alerte pour agents absents avec cours
            var agentsCritiques = agentsAbsents.Where(a => a.Status == "critique").ToList();
            if (agentsCritiques.Any())
            {
                alertes.Add(new AlerteDto
                {
                    Type = "warning",
                    Message = $"📚 {agentsCritiques.Count} agent(s) absent(s) avec cours affectés",
                    Action = "Organiser des remplacements"
                });
            }

            // Alerte si taux de retard élevé (> 15%)
            decimal tauxRetardEleves = effectifEleves * periodeDto.JoursOuvrables > 0 
                ? Math.Round((decimal)retardsEleves / (effectifEleves * periodeDto.JoursOuvrables) * 100, 2) 
                : 0;

            if (tauxRetardEleves > 15)
            {
                alertes.Add(new AlerteDto
                {
                    Type = "info",
                    Message = $"⏰ Taux de retard élèves élevé : {tauxRetardEleves}%",
                    Action = "Sensibiliser sur la ponctualité"
                });
            }

            return new DashboardPresenceDto
            {
                Ecole = new EcoleInfoDto
                {
                    IdEcole = ecole.IdEcole,
                    NomEcole = ecole.Nom ?? "",
                    Logo = ecole.Logo
                },
                Periode = periodeDto,
                ResumeEleves = new ResumePresenceDto
                {
                    EffectifTotal = effectifEleves,
                    Presents = presencesElevesEffectives,
                    Absents = absencesEleves,
                    Retards = retardsEleves,
                    TauxPresence = tauxPresenceEleves,
                    TauxAbsence = effectifEleves * periodeDto.JoursOuvrables > 0 
                        ? Math.Round((decimal)absencesEleves / (effectifEleves * periodeDto.JoursOuvrables) * 100, 2) 
                        : 0,
                    TauxRetard = effectifEleves * periodeDto.JoursOuvrables > 0 
                        ? Math.Round((decimal)retardsEleves / (effectifEleves * periodeDto.JoursOuvrables) * 100, 2) 
                        : 0
                },
                ResumeAgents = new ResumePresenceDto
                {
                    EffectifTotal = effectifAgents,
                    Presents = presencesAgentsEffectives,
                    Absents = absencesAgents,
                    Retards = retardsAgents,
                    TauxPresence = tauxPresenceAgents,
                    TauxAbsence = effectifAgents * periodeDto.JoursOuvrables > 0 
                        ? Math.Round((decimal)absencesAgents / (effectifAgents * periodeDto.JoursOuvrables) * 100, 2) 
                        : 0,
                    TauxRetard = effectifAgents * periodeDto.JoursOuvrables > 0 
                        ? Math.Round((decimal)retardsAgents / (effectifAgents * periodeDto.JoursOuvrables) * 100, 2) 
                        : 0
                },
                // ✅ NOUVEAU: Alertes, classes problématiques et agents absents
                Alertes = alertes.Any() ? alertes : null,
                ClassesProblematiques = classesProblematiques.Any() ? classesProblematiques : null,
                AgentsAbsents = agentsAbsents.Any() ? agentsAbsents : null
            };
        }

        // ═══════════════════════════════════════════════════════
        // MÉTHODES NON ENCORE IMPLÉMENTÉES (à compléter progressivement)
        // ═══════════════════════════════════════════════════════

        public Task<object> GetOptionPresencesAsync(int idOption, DateTime? date, DateTime? dateDebut, DateTime? dateFin, string? periode, string? groupBy)
        {
            throw new NotImplementedException("À implémenter prochainement");
        }

        public Task<object> GetSectionPresencesAsync(int idSection, DateTime? date, DateTime? dateDebut, DateTime? dateFin, string? periode, string? groupBy)
        {
            throw new NotImplementedException("À implémenter prochainement");
        }

        public Task<object> GetDirectionPresencesAsync(int idDirection, DateTime? date, DateTime? dateDebut, DateTime? dateFin, string? periode, string? groupBy)
        {
            throw new NotImplementedException("À implémenter prochainement");
        }

        public Task<object> GetEcolePresencesElevesAsync(int idEcole, DateTime? date, DateTime? dateDebut, DateTime? dateFin, string? periode, string? groupBy)
        {
            throw new NotImplementedException("À implémenter prochainement");
        }

        public Task<RetardsAnalyseDto> GetElevesRetardsAnalyseAsync(int idEcole, DateTime? dateDebut, DateTime? dateFin, string? periode, TimeSpan heureReference, string? groupBy)
        {
            throw new NotImplementedException("À implémenter prochainement");
        }

        public Task<object> GetAgentRetardsAsync(int idAgent, DateTime? dateDebut, DateTime? dateFin, string? periode, TimeSpan heureReference)
        {
            throw new NotImplementedException("À implémenter prochainement");
        }

        public Task<object> GetFonctionsComparatifAsync(int idEcole, DateTime? dateDebut, DateTime? dateFin, string? periode)
        {
            throw new NotImplementedException("À implémenter prochainement");
        }

        public Task<object> GetEcolePresencesAgentsAsync(int idEcole, DateTime? date, DateTime? dateDebut, DateTime? dateFin, string? periode, string? groupBy)
        {
            throw new NotImplementedException("À implémenter prochainement");
        }

        public Task<RetardsAnalyseDto> GetAgentsRetardsAnalyseAsync(int idEcole, DateTime? dateDebut, DateTime? dateFin, string? periode, TimeSpan heureReference, string? groupBy)
        {
            throw new NotImplementedException("À implémenter prochainement");
        }

        public Task<object> GetHierarchiePresencesAsync(int idEcole, DateTime? dateDebut, DateTime? dateFin, string? periode, string type)
        {
            throw new NotImplementedException("À implémenter prochainement");
        }
    }
}

