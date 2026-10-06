using KelasiNaBiso.Attributes;
using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace KelasiNaBiso.Controllers;

[ApiController]
[Route("api/[controller]")]
[Permission("HoraireDeCours.Read")]
public sealed class HoraireDeCoursController : ControllerBase
{
    private readonly IHoraireDeCoursRepository repository;

    public HoraireDeCoursController(IHoraireDeCoursRepository repository)
    {
        this.repository = repository;
    }
    /// <summary>
    /// Returns the active weekly schedule. Regular users are scoped to their school;
    /// tutors are scoped to the classes of their children across all their schools.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int? idClasse,
        [FromQuery] int? idAgent,
        [FromQuery] int? idAnneeScolaire,
        [FromQuery] int? idEcole,
        CancellationToken cancellationToken)
    {
        var scope = await ResolveScheduleScopeAsync(idEcole, cancellationToken);
        if (scope.Error is not null)
            return scope.Error;

        return Ok(await repository.GetAsync(
            idClasse, idAgent, idAnneeScolaire, scope.Value!.SchoolId, scope.Value.TutorId, cancellationToken));
    }

    /// <summary>Returns a paginated active weekly schedule using the same filters and access scope.</summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int? idClasse,
        [FromQuery] int? idAgent,
        [FromQuery] int? idAnneeScolaire,
        [FromQuery] int? idEcole,
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var scope = await ResolveScheduleScopeAsync(idEcole, cancellationToken);
        if (scope.Error is not null)
            return scope.Error;

        return Ok(await repository.GetPagedAsync(
            idClasse, idAgent, idAnneeScolaire, scope.Value!.SchoolId, scope.Value.TutorId,
            request, cancellationToken));
    }

    private async Task<(ScheduleScope? Value, IActionResult? Error)> ResolveScheduleScopeAsync(
        int? requestedSchoolId,
        CancellationToken cancellationToken)
    {
        var hasAdministrativeScope =
            User.IsInRole(UserRoles.ADMIN)
            || User.IsInRole(UserRoles.DIRECTEUR)
            || User.IsInRole(UserRoles.SOUS_DIRECTEUR)
            || User.IsInRole(UserRoles.SUPER_ADMIN)
            || User.IsInRole(UserRoles.IT_SUPPORT);
        var isTutor = User.IsInRole(UserRoles.PARENT) && !hasAdministrativeScope;
        var tutorId = isTutor ? this.GetCurrentTuteurId() : null;

        if (isTutor)
        {
            if (!tutorId.HasValue)
                return (null, StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Accès refusé : identité tuteur absente du token (IdTuteur)." }));

            if (requestedSchoolId.HasValue)
            {
                var schoolAccess = await this.ForbidIfWrongSchoolAsync(
                    requestedSchoolId, cancellationToken: cancellationToken);
                if (schoolAccess is not null)
                    return (null, schoolAccess);
            }

            return (new ScheduleScope(requestedSchoolId, tutorId), null);
        }

        var tokenSchoolId = this.GetCurrentUserSchoolId();
        var isGlobalAdministrator = User.IsInRole(UserRoles.SUPER_ADMIN)
            || User.IsInRole(UserRoles.IT_SUPPORT);
        var effectiveSchoolId = requestedSchoolId ?? tokenSchoolId;

        if (!effectiveSchoolId.HasValue && !isGlobalAdministrator)
            return (null, StatusCode(StatusCodes.Status403Forbidden,
                new { message = "Accès refusé : école absente du token." }));

        if (requestedSchoolId.HasValue && !isGlobalAdministrator)
        {
            var schoolAccess = this.ForbidIfWrongSchool(requestedSchoolId);
            if (schoolAccess is not null)
                return (null, schoolAccess);
        }

        return (new ScheduleScope(effectiveSchoolId, null), null);
    }

    private sealed record ScheduleScope(int? SchoolId, int? TutorId);

    /// <summary>Returns one active weekly schedule entry.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<HoraireDeCoursResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await repository.GetByIdAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    /// <summary>Creates a weekly schedule entry.</summary>
    [HttpPost]
    [Permission("HoraireDeCours.Create")]
    public async Task<ActionResult<HoraireDeCoursResponse>> Create(
        CreateHoraireDeCoursRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await repository.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = response.IdHoraireDeCours }, response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    /// <summary>Updates a weekly schedule entry.</summary>
    [HttpPut("{id:int}")]
    [Permission("HoraireDeCours.Update")]
    public async Task<ActionResult<HoraireDeCoursResponse>> Update(
        int id,
        UpdateHoraireDeCoursRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await repository.UpdateAsync(id, request, cancellationToken);
            return response is null ? NotFound() : Ok(response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    /// <summary>Deactivates a weekly schedule entry.</summary>
    [HttpDelete("{id:int}")]
    [Permission("HoraireDeCours.Delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => await repository.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
