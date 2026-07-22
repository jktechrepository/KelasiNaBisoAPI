using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models.DTOs.Vitrine;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KelasiNaBiso.Controllers
{
    /// <summary>Endpoints publics pour le site vitrine KelasiNaBiso.</summary>
    [ApiController]
    [Route("api/vitrine")]
    public class VitrineController : ControllerBase
    {
        private readonly IEcoleRepository _ecoleRepository;
        private readonly IVitrineService _vitrineService;
        private readonly IVitrineContactService _vitrineContactService;

        public VitrineController(
            IEcoleRepository ecoleRepository,
            IVitrineService vitrineService,
            IVitrineContactService vitrineContactService)
        {
            _ecoleRepository = ecoleRepository;
            _vitrineService = vitrineService;
            _vitrineContactService = vitrineContactService;
        }

        /// <summary>
        /// Logos des écoles partenaires (écoles actives avec logo configuré).
        /// Public — site vitrine section « Nos partenaires ».
        /// </summary>
        [HttpGet("partenaires")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<EcolePartenaireDto>), 200)]
        public async Task<ActionResult<IEnumerable<EcolePartenaireDto>>> GetPartenaires([FromQuery] int limit = 50)
        {
            var partenaires = await _ecoleRepository.GetPartenairesLogosAsync(limit);
            return Ok(partenaires);
        }

        /// <summary>
        /// Statistiques globales de la plateforme (écoles, élèves, personnel, parents connectés).
        /// Public — site vitrine section chiffres clés.
        /// </summary>
        [HttpGet("statistiques")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(VitrineStatistiquesDto), 200)]
        public async Task<ActionResult<VitrineStatistiquesDto>> GetStatistiques(CancellationToken cancellationToken)
        {
            var stats = await _vitrineService.GetStatistiquesAsync(cancellationToken);
            return Ok(stats);
        }

        /// <summary>
        /// Formulaire de contact — envoie un email à contact@kelasinabiso.com.
        /// Public — page kelasinabiso.com/contact.
        /// </summary>
        [HttpPost("contact")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ContactVitrineResponseDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ContactVitrineResponseDto), 503)]
        public async Task<ActionResult<ContactVitrineResponseDto>> EnvoyerContact(
            [FromBody] ContactVitrineRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _vitrineContactService.EnvoyerMessageAsync(
                request,
                this.GetClientIpAddress(),
                cancellationToken);

            if (!result.Success)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, result);

            return Ok(result);
        }
    }
}
