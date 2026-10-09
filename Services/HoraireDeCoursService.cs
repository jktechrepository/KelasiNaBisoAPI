using KelasiNaBiso.Data;
using KelasiNaBiso.Extensions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using KelasiNaBiso.Models.DTOs.Pagination;

namespace KelasiNaBiso.Services;

public sealed class HoraireDeCoursService : IHoraireDeCoursRepository
{
    private readonly KelasiNaBisoDbContext context;

    public HoraireDeCoursService(KelasiNaBisoDbContext context)
    {
        this.context = context;
    }
    public async Task<IReadOnlyList<HoraireDeCoursResponse>> GetAsync(
        int? idClasse,
        int? idAgent,
        int? idAnneeScolaire,
        int? idEcole,
        int? idTuteur,
        int? idAgentAffecte,
        CancellationToken cancellationToken)
    {
        var query = BuildScheduleQuery(idClasse, idAgent, idAnneeScolaire, idEcole, idTuteur, idAgentAffecte);

        return await query
            .OrderBy(e => e.Jour)
            .ThenBy(e => e.HeureDebut)
            .ThenBy(e => e.IdHoraireDeCours)
            .Select(ToResponseExpression())
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<HoraireDeCoursResponse>> GetPagedAsync(
        int? idClasse,
        int? idAgent,
        int? idAnneeScolaire,
        int? idEcole,
        int? idTuteur,
        int? idAgentAffecte,
        PagedRequest request,
        CancellationToken cancellationToken)
    {
        var query = BuildScheduleQuery(idClasse, idAgent, idAnneeScolaire, idEcole, idTuteur, idAgentAffecte)
            .OrderBy(e => e.Jour)
            .ThenBy(e => e.HeureDebut)
            .ThenBy(e => e.IdHoraireDeCours)
            .Select(ToResponseExpression());

        return await query.ToPagedAsync(request, cancellationToken);
    }

    private IQueryable<HoraireDeCours> BuildScheduleQuery(
        int? idClasse,
        int? idAgent,
        int? idAnneeScolaire,
        int? idEcole,
        int? idTuteur,
        int? idAgentAffecte)
    {
        var query = context.HorairesDeCours
            .AsNoTracking()
            .Where(e => e.Statut);

        if (idTuteur.HasValue)
        {
            query = query.Where(e => context.Inscriptions.Any(i =>
                i.IdClasse == e.IdClasse &&
                i.IdAnneeScolaire == e.IdAnneeScolaire &&
                i.Statut == true &&
                i.StatutInscription != null &&
                (i.StatutInscription == InscriptionActiveRules.StatutConfirme ||
                 i.StatutInscription == "Confirme" ||
                 i.StatutInscription.StartsWith("Confirm")) &&
                context.Eleves.Any(el =>
                    el.IdEleve == i.IdEleve &&
                    el.IdTuteur == idTuteur.Value &&
                    el.Statut == true)));
        }

        if (idAgentAffecte.HasValue)
        {
            var agentId = idAgentAffecte.Value;
            query = query.Where(e =>
                context.TitulairesClasses.Any(t =>
                    t.IdAgent == agentId &&
                    t.IdClasse == e.IdClasse &&
                    t.IdAnneeScolaire == e.IdAnneeScolaire &&
                    t.Statut == true &&
                    t.DateFin == null)
                || context.HorairesDeCours.Any(h =>
                    h.Statut &&
                    h.IdAgent == agentId &&
                    h.IdClasse == e.IdClasse &&
                    h.IdAnneeScolaire == e.IdAnneeScolaire));
        }

        if (idEcole.HasValue)
            query = query.Where(e => e.Classe!.Direction!.IdEcole == idEcole.Value);

        if (idClasse.HasValue)
            query = query.Where(e => e.IdClasse == idClasse.Value);
        if (idAgent.HasValue)
            query = query.Where(e => e.IdAgent == idAgent.Value);
        if (idAnneeScolaire.HasValue)
            query = query.Where(e => e.IdAnneeScolaire == idAnneeScolaire.Value);

        return query;
    }

    public async Task<HoraireDeCoursResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        int? idEcole = null,
        int? idTuteur = null,
        int? idAgentAffecte = null)
    {
        return await BuildScheduleQuery(null, null, null, idEcole, idTuteur, idAgentAffecte)
            .Where(e => e.IdHoraireDeCours == id)
            .Select(ToResponseExpression())
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<HoraireDeCoursResponse> CreateAsync(
        CreateHoraireDeCoursRequest request,
        CancellationToken cancellationToken)
    {
        ValidateTimeRange(request.HeureDebut, request.HeureFin);
        await ValidateReferencesAsync(request.IdCours, request.IdClasse, request.IdAgent, request.IdAnneeScolaire, cancellationToken);
        await EnsureNoConflictAsync(
            request.Jour, request.HeureDebut, request.HeureFin,
            request.IdClasse, request.IdAgent, request.IdAnneeScolaire,
            request.Salle, null, cancellationToken);

        var entity = new HoraireDeCours
        {
            Jour = request.Jour,
            HeureDebut = request.HeureDebut,
            HeureFin = request.HeureFin,
            IdCours = request.IdCours,
            IdClasse = request.IdClasse,
            IdAgent = request.IdAgent,
            IdAnneeScolaire = request.IdAnneeScolaire,
            Salle = NormalizeRoom(request.Salle)
        };

        context.HorairesDeCours.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return (await GetByIdAsync(entity.IdHoraireDeCours, cancellationToken))!;
    }

    public async Task<HoraireDeCoursResponse?> UpdateAsync(
        int id,
        UpdateHoraireDeCoursRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await context.HorairesDeCours
            .SingleOrDefaultAsync(e => e.IdHoraireDeCours == id, cancellationToken);
        if (entity is null)
            return null;

        ValidateTimeRange(request.HeureDebut, request.HeureFin);
        await ValidateReferencesAsync(request.IdCours, request.IdClasse, request.IdAgent, request.IdAnneeScolaire, cancellationToken);
        await EnsureNoConflictAsync(
            request.Jour, request.HeureDebut, request.HeureFin,
            request.IdClasse, request.IdAgent, request.IdAnneeScolaire,
            request.Salle, id, cancellationToken);

        entity.Jour = request.Jour;
        entity.HeureDebut = request.HeureDebut;
        entity.HeureFin = request.HeureFin;
        entity.IdCours = request.IdCours;
        entity.IdClasse = request.IdClasse;
        entity.IdAgent = request.IdAgent;
        entity.IdAnneeScolaire = request.IdAnneeScolaire;
        entity.Salle = NormalizeRoom(request.Salle);
        entity.Statut = request.Statut;

        await context.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await context.HorairesDeCours
            .SingleOrDefaultAsync(e => e.IdHoraireDeCours == id, cancellationToken);
        if (entity is null)
            return false;

        entity.Statut = false;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ValidateReferencesAsync(
        int idCours,
        int idClasse,
        int idAgent,
        int idAnneeScolaire,
        CancellationToken cancellationToken)
    {
        var data = await (
            from cours in context.Cours
            join classe in context.Classes on cours.IdClasse equals classe.IdClasse
            join agent in context.Agents on idAgent equals agent.IdAgent
            join annee in context.AnneeScolaires on idAnneeScolaire equals annee.IdAnneeScolaire
            where cours.IdCours == idCours && classe.IdClasse == idClasse
            select new
            {
                CoursClasse = cours.IdClasse,
                ClasseEcole = classe.Direction!.IdEcole,
                AgentEcole = agent.IdEcole,
                AnneeEcole = annee.IdEcole,
                AgentActuellementAffecte = context.AffectationsCours.Any(a =>
                    a.IdAgent == idAgent && a.IdCours == idCours &&
                    a.IdAnneeScolaire == idAnneeScolaire && a.Statut == true)
            }).SingleOrDefaultAsync(cancellationToken);

        if (data is null)
            throw new ArgumentException("Le cours, la classe, l'agent ou l'année scolaire est introuvable.");
        if (data.ClasseEcole != data.AgentEcole || data.ClasseEcole != data.AnneeEcole)
            throw new ArgumentException("Les entités de l'horaire doivent appartenir à la même école.");
        if (!data.AgentActuellementAffecte)
            throw new ArgumentException("L'agent n'est pas affecté à ce cours pour cette année scolaire.");
    }

    private static void ValidateTimeRange(TimeSpan start, TimeSpan end)
    {
        if (start >= end)
            throw new ArgumentException("L'heure de début doit être antérieure à l'heure de fin.");
    }

    private async Task EnsureNoConflictAsync(
        Models.Enums.JourSemaine jour,
        TimeSpan heureDebut,
        TimeSpan heureFin,
        int idClasse,
        int idAgent,
        int idAnneeScolaire,
        string? salle,
        int? excludedId,
        CancellationToken cancellationToken)
    {
        var conflicts = await context.HorairesDeCours
            .Where(e => e.Statut &&
                        e.Jour == jour &&
                        e.IdAnneeScolaire == idAnneeScolaire &&
                        (!excludedId.HasValue || e.IdHoraireDeCours != excludedId.Value) &&
                        heureDebut < e.HeureFin &&
                        heureFin > e.HeureDebut &&
                        (e.IdClasse == idClasse ||
                         e.IdAgent == idAgent ||
                         (salle != null && e.Salle == salle)))
            .Select(e => new { e.IdClasse, e.IdAgent, e.Salle })
            .ToListAsync(cancellationToken);

        if (conflicts.Count > 0)
            throw new InvalidOperationException("Le créneau chevauche déjà un horaire de la classe, de l'agent ou de la salle.");
    }

    private static string? NormalizeRoom(string? room) =>
        string.IsNullOrWhiteSpace(room) ? null : room.Trim();

    private static System.Linq.Expressions.Expression<Func<HoraireDeCours, HoraireDeCoursResponse>> ToResponseExpression() =>
        e => new HoraireDeCoursResponse(
            e.IdHoraireDeCours, e.Jour, e.HeureDebut, e.HeureFin,
            e.IdCours, e.Cours!.NomCours, e.IdClasse, e.Classe!.NomClasse,
            e.IdAgent, e.Agent!.Nom + " " + e.Agent.Prenom,
            e.IdAnneeScolaire, e.AnneeScolaire!.LibelleAnneeScolaire,
            e.Salle, e.Statut);
}
