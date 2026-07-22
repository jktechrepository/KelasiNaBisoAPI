using KelasiNaBiso.Models.DTOs.Communication;
using KelasiNaBiso.Models.DTOs.Pagination;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Super-Admin,Admin,Directeur,Sous-Directeur")]
    public class CommunicationController : ControllerBase
    {
        private readonly ICommunicationCampaignService _campaignService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CommunicationController> _logger;

        public CommunicationController(
            ICommunicationCampaignService campaignService,
            ICurrentUserService currentUserService,
            ILogger<CommunicationController> logger)
        {
            _campaignService = campaignService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<CommunicationCampaignSummaryDto>>> GetCampaigns([FromQuery] PagedRequest request, CancellationToken cancellationToken)
        {
            var result = await _campaignService.GetCampaignsAsync(
                _currentUserService.UserId,
                _currentUserService.UserRole,
                _currentUserService.EcoleId,
                request,
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CommunicationCampaignDetailDto>> GetCampaign(int id, CancellationToken cancellationToken)
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(
                id,
                _currentUserService.UserId,
                _currentUserService.UserRole,
                _currentUserService.EcoleId,
                cancellationToken);

            if (campaign == null)
            {
                return NotFound(new { message = "Campagne introuvable." });
            }

            return Ok(campaign);
        }

        [HttpGet("ecole/{ecoleId:int}")]
        public async Task<ActionResult<PagedResult<CommunicationCampaignDetailDto>>> GetCampaignsByEcole(int ecoleId, [FromQuery] PagedRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _campaignService.GetCampaignsByEcoleAsync(
                    ecoleId,
                    _currentUserService.UserId,
                    _currentUserService.UserRole,
                    _currentUserService.EcoleId,
                    request,
                    cancellationToken);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<CommunicationCampaignDetailDto>> CreateCampaign([FromBody] CreateCommunicationCampaignDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var campaign = await _campaignService.CreateCampaignAsync(
                    dto,
                    _currentUserService.UserId,
                    _currentUserService.UserRole,
                    _currentUserService.EcoleId,
                    cancellationToken);

                return CreatedAtAction(nameof(GetCampaign), new { id = campaign.IdCampaign }, campaign);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création d'une campagne");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CommunicationCampaignDetailDto>> UpdateCampaign(int id, [FromBody] UpdateCommunicationCampaignDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var campaign = await _campaignService.UpdateCampaignAsync(
                    id,
                    dto,
                    _currentUserService.UserId,
                    _currentUserService.UserRole,
                    _currentUserService.EcoleId,
                    cancellationToken);

                return Ok(campaign);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id:int}/annuler")]
        public async Task<ActionResult> CancelCampaign(int id, [FromBody] CancelCommunicationRequest? request = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var success = await _campaignService.CancelCampaignAsync(
                    id,
                    request,
                    _currentUserService.UserId,
                    _currentUserService.UserRole,
                    _currentUserService.EcoleId,
                    cancellationToken);

                if (!success)
                {
                    return NotFound(new { message = "Campagne introuvable." });
                }

                return Ok(new { message = "Campagne annulée." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id:int}/destinataires/recharger")]
        public async Task<ActionResult> RefreshRecipients(int id, CancellationToken cancellationToken)
        {
            try
            {
                var total = await _campaignService.RefreshRecipientsAsync(
                    id,
                    _currentUserService.UserId,
                    _currentUserService.UserRole,
                    _currentUserService.EcoleId,
                    cancellationToken);

                return Ok(new { destinataires = total });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:int}/destinataires")]
        public async Task<ActionResult<PagedResult<CommunicationRecipientDto>>> GetRecipients(int id, [FromQuery] PagedRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _campaignService.GetRecipientsAsync(
                    id,
                    request,
                    _currentUserService.UserId,
                    _currentUserService.UserRole,
                    _currentUserService.EcoleId,
                    cancellationToken);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id:int}/historique")]
        public async Task<ActionResult<PagedResult<CommunicationHistoryDto>>> GetHistory(int id, [FromQuery] PagedRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _campaignService.GetHistoryAsync(
                    id,
                    request,
                    _currentUserService.UserId,
                    _currentUserService.UserRole,
                    _currentUserService.EcoleId,
                    cancellationToken);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id:int}/envoyer")]
        public async Task<ActionResult> Dispatch(int id, CancellationToken cancellationToken)
        {
            try
            {
                var success = await _campaignService.DispatchCampaignAsync(
                    id,
                    _currentUserService.UserId,
                    _currentUserService.UserRole,
                    _currentUserService.EcoleId,
                    cancellationToken);

                if (!success)
                {
                    return NotFound(new { message = "Campagne introuvable." });
                }

                return Accepted(new { message = "Campagne en cours d'envoi." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

