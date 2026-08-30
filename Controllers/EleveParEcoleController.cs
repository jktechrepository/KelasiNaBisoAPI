using KelasiNaBiso.Attributes;
using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EleveParEcoleController : ControllerBase
    {
        private readonly IEleveParEcoleRepository _eleveParEcoleRepository;
        private readonly EleveAnneeScopeHelper _scope;

        public EleveParEcoleController(
            IEleveParEcoleRepository eleveParEcoleRepository,
            EleveAnneeScopeHelper scope)
        {
            _eleveParEcoleRepository = eleveParEcoleRepository;
            _scope = scope;
        }

        private async Task<IActionResult?> ForbidClasseSchoolAsync(int idClasse)
        {
            int idEcole;
            try
            {
                idEcole = await _scope.ResolveIdEcoleForClasseAsync(idClasse);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            return this.ForbidIfWrongSchool(idEcole);
        }

        private async Task<IActionResult?> ForbidDirectionSchoolAsync(int idDirection)
        {
            int idEcole;
            try
            {
                idEcole = await _scope.ResolveIdEcoleForDirectionAsync(idDirection);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            return this.ForbidIfWrongSchool(idEcole);
        }

        private async Task<IActionResult?> ForbidOptionSchoolAsync(int idOption)
        {
            int idEcole;
            try
            {
                idEcole = await _scope.ResolveIdEcoleForOptionAsync(idOption);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            return this.ForbidIfWrongSchool(idEcole);
        }

        [HttpGet]
        [RequireGlobalAccess]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _eleveParEcoleRepository.GetAllAsync(resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EleveParEcoleDTO>> GetById(int id)
        {
            var eleve = await _eleveParEcoleRepository.GetByIdAsync(id);
            if (eleve == null)
                return NotFound($"Aucun élève trouvé avec l'ID {id}");
            return Ok(eleve);
        }

        [HttpGet("ecole/{idEcole}")]
        public async Task<IActionResult> GetByEcole(int idEcole, [FromQuery] int? idAnneeScolaire = null)
        {
            var deny = this.ForbidIfWrongSchool(idEcole);
            if (deny != null)
                return deny;

            try
            {
                return Ok(await _eleveParEcoleRepository.GetByEcoleAsync(idEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("classe/{idClasse}")]
        public async Task<IActionResult> GetByClasse(int idClasse, [FromQuery] int? idAnneeScolaire = null)
        {
            var deny = await ForbidClasseSchoolAsync(idClasse);
            if (deny != null)
                return deny;

            try
            {
                return Ok(await _eleveParEcoleRepository.GetByClasseAsync(idClasse, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("direction/{idDirection}")]
        public async Task<IActionResult> GetByDirection(int idDirection, [FromQuery] int? idAnneeScolaire = null)
        {
            var deny = await ForbidDirectionSchoolAsync(idDirection);
            if (deny != null)
                return deny;

            try
            {
                return Ok(await _eleveParEcoleRepository.GetByDirectionAsync(idDirection, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("option/{idOption}")]
        public async Task<IActionResult> GetByOption(int idOption, [FromQuery] int? idAnneeScolaire = null)
        {
            var deny = await ForbidOptionSchoolAsync(idOption);
            if (deny != null)
                return deny;

            try
            {
                return Ok(await _eleveParEcoleRepository.GetByOptionAsync(idOption, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("tuteur/{idTuteur}")]
        public async Task<IActionResult> GetByTuteur(
            int idTuteur,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _eleveParEcoleRepository.GetByTuteurAsync(idTuteur, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("statut/{statut}")]
        public async Task<IActionResult> GetByStatut(
            string statut,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            if (!bool.TryParse(statut, out bool statutBool))
                return BadRequest("Le paramètre statut doit être 'true' ou 'false'");

            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _eleveParEcoleRepository.GetByStatutAsync(statutBool, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("genre/{genre}")]
        public async Task<IActionResult> GetByGenre(
            string genre,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _eleveParEcoleRepository.GetByGenreAsync(genre, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("age-range")]
        public async Task<IActionResult> GetByAgeRange(
            [FromQuery] int minAge,
            [FromQuery] int maxAge,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _eleveParEcoleRepository.GetByAgeRangeAsync(minAge, maxAge, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("province/{province}")]
        public async Task<IActionResult> GetByProvince(
            string province,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _eleveParEcoleRepository.GetByProvinceAsync(province, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("ville/{ville}")]
        public async Task<IActionResult> GetByVille(
            string ville,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _eleveParEcoleRepository.GetByVilleAsync(ville, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("commune/{commune}")]
        public async Task<IActionResult> GetByCommune(
            string commune,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _eleveParEcoleRepository.GetByCommuneAsync(commune, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("tuteur-contact/{contact}")]
        public async Task<IActionResult> GetByTuteurContact(
            string contact,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            if (string.IsNullOrWhiteSpace(contact))
                return BadRequest("Le contact ne peut pas être vide");

            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _eleveParEcoleRepository.GetByTuteurContactAsync(contact, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string term,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest("Le terme de recherche ne peut pas être vide");

            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _eleveParEcoleRepository.SearchAsync(term, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("count/ecole/{idEcole}")]
        public async Task<IActionResult> GetCountByEcole(int idEcole, [FromQuery] int? idAnneeScolaire = null)
        {
            var deny = this.ForbidIfWrongSchool(idEcole);
            if (deny != null)
                return deny;

            try
            {
                return Ok(await _eleveParEcoleRepository.GetCountByEcoleAsync(idEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("count/classe/{idClasse}")]
        public async Task<IActionResult> GetCountByClasse(int idClasse, [FromQuery] int? idAnneeScolaire = null)
        {
            var deny = await ForbidClasseSchoolAsync(idClasse);
            if (deny != null)
                return deny;

            try
            {
                return Ok(await _eleveParEcoleRepository.GetCountByClasseAsync(idClasse, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("count/direction/{idDirection}")]
        public async Task<IActionResult> GetCountByDirection(int idDirection, [FromQuery] int? idAnneeScolaire = null)
        {
            var deny = await ForbidDirectionSchoolAsync(idDirection);
            if (deny != null)
                return deny;

            try
            {
                return Ok(await _eleveParEcoleRepository.GetCountByDirectionAsync(idDirection, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
