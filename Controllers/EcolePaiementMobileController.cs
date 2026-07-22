using KelasiNaBiso.Models.DTOs.MokoAfrika;
using KelasiNaBiso.Services.MokoAfrika;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/Ecole/{idEcole}/paiement-mobile")]
    [Authorize(Roles = "Admin,Super-Admin,Directeur,Financier")]
    public class EcolePaiementMobileController : ControllerBase
    {
        private readonly IEcolePaiementMobileService _service;
        private readonly ILogger<EcolePaiementMobileController> _logger;

        public EcolePaiementMobileController(
            IEcolePaiementMobileService service,
            ILogger<EcolePaiementMobileController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Vue d'ensemble : configuration, wallet et statistiques transactions.
        /// Retourne 200 même si l'école n'est pas encore configurée (estConfigure=false).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(EcolePaiementMobileOverviewDto), 200)]
        [ProducesResponseType(503)]
        public async Task<ActionResult<EcolePaiementMobileOverviewDto>> GetOverview(int idEcole)
        {
            try
            {
                var overview = await _service.GetOverviewAsync(idEcole);
                return Ok(overview);
            }
            catch (Exception ex) when (IsDatabaseSchemaError(ex))
            {
                _logger.LogError(ex, "Tables MOKO absentes pour ecole {IdEcole}", idEcole);
                return StatusCode(503, new
                {
                    code = "MOKO_MIGRATION_REQUIRED",
                    message = "Les tables MOKO Afrika ne sont pas encore créées en base. Exécutez scripts/migration-moko-afrika-manual.sql",
                    detail = ex.Message
                });
            }
        }

        /// <summary>Configuration paiement Mobile Money / carte MOKO de l'école.</summary>
        [HttpGet("config")]
        [ProducesResponseType(typeof(EcoleInfoPaiementMobileDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<EcoleInfoPaiementMobileDto>> GetConfig(int idEcole)
        {
            try
            {
                var config = await _service.GetByEcoleAsync(idEcole);
                if (config == null)
                    return NotFound(new { message = "Configuration paiement mobile non initialisée pour cette école." });
                return Ok(config);
            }
            catch (Exception ex) when (IsDatabaseSchemaError(ex))
            {
                return MigrationRequired(ex);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Super-Admin,Directeur")]
        [ProducesResponseType(typeof(EcoleInfoPaiementMobileDto), 200)]
        public async Task<ActionResult<EcoleInfoPaiementMobileDto>> CreateOrUpdateConfig(
            int idEcole,
            [FromBody] CreateEcoleInfoPaiementMobileDto dto)
        {
            try
            {
                var result = await _service.CreateOrUpdateAsync(idEcole, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex) when (IsDatabaseSchemaError(ex))
            {
                return MigrationRequired(ex);
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin,Super-Admin,Directeur")]
        [ProducesResponseType(typeof(EcoleInfoPaiementMobileDto), 200)]
        public async Task<ActionResult<EcoleInfoPaiementMobileDto>> UpdateConfig(
            int idEcole,
            [FromBody] UpdateEcoleInfoPaiementMobileDto dto)
        {
            try
            {
                var result = await _service.UpdateAsync(idEcole, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex) when (IsDatabaseSchemaError(ex))
            {
                return MigrationRequired(ex);
            }
        }

        /// <summary>Transactions MOKO de l'école (PayIn / PayOut) avec filtres.</summary>
        [HttpGet("transactions")]
        [ProducesResponseType(typeof(List<TransactionMokoListItemDto>), 200)]
        public async Task<ActionResult<List<TransactionMokoListItemDto>>> GetTransactions(
            int idEcole,
            [FromQuery] string? status = null,
            [FromQuery] string? action = null,
            [FromQuery] int limit = 50,
            [FromQuery] int offset = 0)
        {
            try
            {
                var transactions = await _service.GetTransactionsAsync(idEcole, status, action, limit, offset);
                return Ok(transactions);
            }
            catch (Exception ex) when (IsDatabaseSchemaError(ex))
            {
                return MigrationRequired(ex);
            }
        }

        [HttpGet("wallet/mouvements")]
        [ProducesResponseType(typeof(List<EcoleWalletMouvementDto>), 200)]
        public async Task<ActionResult<List<EcoleWalletMouvementDto>>> GetWalletMouvements(
            int idEcole,
            [FromQuery] int limit = 50)
        {
            try
            {
                var mouvements = await _service.GetWalletMouvementsAsync(idEcole, limit);
                return Ok(mouvements);
            }
            catch (Exception ex) when (IsDatabaseSchemaError(ex))
            {
                return MigrationRequired(ex);
            }
        }

        /// <summary>File d'attente PayOut (reversements vers bénéficiaires école).</summary>
        [HttpGet("payouts")]
        [ProducesResponseType(typeof(List<FilePayoutMokoDto>), 200)]
        public async Task<ActionResult<List<FilePayoutMokoDto>>> GetPayouts(
            int idEcole,
            [FromQuery] string? status = null,
            [FromQuery] int limit = 50)
        {
            try
            {
                var payouts = await _service.GetPayoutsAsync(idEcole, status, limit);
                return Ok(payouts);
            }
            catch (Exception ex) when (IsDatabaseSchemaError(ex))
            {
                return MigrationRequired(ex);
            }
        }

        [HttpPost("beneficiaires")]
        [Authorize(Roles = "Admin,Super-Admin,Directeur")]
        [ProducesResponseType(typeof(EcoleBeneficiaireMomoDto), 201)]
        public async Task<ActionResult<EcoleBeneficiaireMomoDto>> AddBeneficiaire(
            int idEcole,
            [FromBody] CreateEcoleBeneficiaireMomoDto dto)
        {
            try
            {
                var result = await _service.AddBeneficiaireAsync(idEcole, dto);
                return CreatedAtAction(nameof(GetOverview), new { idEcole }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex) when (IsDatabaseSchemaError(ex))
            {
                return MigrationRequired(ex);
            }
        }

        [HttpPut("beneficiaires/{idBeneficiaire}")]
        [Authorize(Roles = "Admin,Super-Admin,Directeur")]
        [ProducesResponseType(typeof(EcoleBeneficiaireMomoDto), 200)]
        public async Task<ActionResult<EcoleBeneficiaireMomoDto>> UpdateBeneficiaire(
            int idEcole,
            int idBeneficiaire,
            [FromBody] UpdateEcoleBeneficiaireMomoDto dto)
        {
            try
            {
                var result = await _service.UpdateBeneficiaireAsync(idEcole, idBeneficiaire, dto);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex) when (IsDatabaseSchemaError(ex))
            {
                return MigrationRequired(ex);
            }
        }

        [HttpDelete("beneficiaires/{idBeneficiaire}")]
        [Authorize(Roles = "Admin,Super-Admin,Directeur")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> DeleteBeneficiaire(int idEcole, int idBeneficiaire)
        {
            try
            {
                var success = await _service.DeleteBeneficiaireAsync(idEcole, idBeneficiaire);
                if (!success)
                    return NotFound();
                return NoContent();
            }
            catch (Exception ex) when (IsDatabaseSchemaError(ex))
            {
                return MigrationRequired(ex);
            }
        }

        private static bool IsDatabaseSchemaError(Exception ex)
        {
            for (var e = ex; e != null; e = e.InnerException)
            {
                var msg = e.Message;
                if (msg.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase)
                    || msg.Contains("Unknown column", StringComparison.OrdinalIgnoreCase)
                    || msg.Contains("n'existe pas", StringComparison.OrdinalIgnoreCase)
                    || msg.Contains("Table", StringComparison.OrdinalIgnoreCase) && msg.Contains("exist", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return ex is DbUpdateException;
        }

        private ObjectResult MigrationRequired(Exception ex) =>
            StatusCode(503, new
            {
                code = "MOKO_MIGRATION_REQUIRED",
                message = "Les tables MOKO Afrika ne sont pas encore créées en base. Exécutez scripts/migration-moko-afrika-manual.sql",
                detail = ex.Message
            });
    }
}
