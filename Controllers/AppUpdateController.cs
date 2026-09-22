using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.Enums;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppUpdateController : ControllerBase
    {
        private readonly IAppUpdateService _appUpdateService;

        public AppUpdateController(IAppUpdateService appUpdateService)
        {
            _appUpdateService = appUpdateService;
        }

        /// <summary>
        /// Vérifie la policy de mise à jour pour une plateforme / version client (anonyme — avant login).
        /// </summary>
        [HttpGet("policy")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AppUpdateCheckResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AppUpdateCheckResponseDto>> CheckPolicy(
            [FromQuery] string platform,
            [FromQuery] string appVersion,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _appUpdateService.CheckPolicyAsync(platform, appVersion, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Liste des policies (Android / iOS) — Super-Admin.</summary>
        [HttpGet("policies")]
        [Authorize(Roles = UserRoles.SUPER_ADMIN)]
        [ProducesResponseType(typeof(IReadOnlyList<AppUpdatePolicyDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<AppUpdatePolicyDto>>> GetPolicies(
            CancellationToken cancellationToken = default)
        {
            var items = await _appUpdateService.GetAllPoliciesAsync(cancellationToken);
            return Ok(items);
        }

        /// <summary>Création / mise à jour manuelle de la policy d'une plateforme — Super-Admin.</summary>
        [HttpPut("policies/{platform}")]
        [Authorize(Roles = UserRoles.SUPER_ADMIN)]
        [ProducesResponseType(typeof(AppUpdatePolicyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AppUpdatePolicyDto>> UpsertPolicy(
            string platform,
            [FromBody] UpsertAppUpdatePolicyDto dto,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var auteur = this.GetCurrentUserId();
                var result = await _appUpdateService.UpsertPolicyAsync(
                    platform,
                    dto,
                    auteur > 0 ? auteur : null,
                    cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Campagne FCM de mise à jour (complément au check pull) — Super-Admin.
        /// Data : type=app_update, updateLevel, storeUrl (si policy connue).
        /// </summary>
        [HttpPost("notify")]
        [Authorize(Roles = UserRoles.SUPER_ADMIN)]
        [ProducesResponseType(typeof(NotifyAppUpdateResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<NotifyAppUpdateResultDto>> Notify(
            [FromBody] NotifyAppUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _appUpdateService.NotifyAsync(dto, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
