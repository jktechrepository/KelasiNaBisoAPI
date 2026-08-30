using KelasiNaBiso.Helpers;
using KelasiNaBiso.Models;
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
    public class V_EleveController : ControllerBase
    {
        private readonly IV_EleveRepository _vEleveRepository;
        private readonly EleveAnneeScopeHelper _scope;

        public V_EleveController(IV_EleveRepository vEleveRepository, EleveAnneeScopeHelper scope)
        {
            _vEleveRepository = vEleveRepository;
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

        private async Task<IActionResult?> ForbidSectionSchoolAsync(int idSection)
        {
            int idEcole;
            try
            {
                idEcole = await _scope.ResolveIdEcoleForSectionAsync(idSection);
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
        public async Task<IActionResult> GetV_Eleves(
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetAllAsync(resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<V_Eleve>> GetV_Eleve(int id)
        {
            var vEleve = await _vEleveRepository.GetByIdAsync(id);
            if (vEleve == null)
                return NotFound();
            return Ok(vEleve);
        }

        [HttpGet("reference/{reference}")]
        public async Task<ActionResult<V_Eleve>> GetV_EleveByReference(Guid reference)
        {
            var vEleve = await _vEleveRepository.GetByReferenceAsync(reference);
            if (vEleve == null)
                return NotFound();
            return Ok(vEleve);
        }

        [HttpGet("matricule/{matricule}")]
        public async Task<ActionResult<V_Eleve>> GetV_EleveByMatricule(string matricule)
        {
            var vEleve = await _vEleveRepository.GetByMatriculeAsync(matricule);
            if (vEleve == null)
                return NotFound();
            return Ok(vEleve);
        }

        [HttpGet("ecole/{idEcole}")]
        public async Task<IActionResult> GetV_ElevesByEcole(int idEcole, [FromQuery] int? idAnneeScolaire = null)
        {
            var deny = this.ForbidIfWrongSchool(idEcole);
            if (deny != null)
                return deny;

            try
            {
                return Ok(await _vEleveRepository.GetByEcoleAsync(idEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("classe/{idClasse}")]
        public async Task<IActionResult> GetV_ElevesByClasse(int idClasse, [FromQuery] int? idAnneeScolaire = null)
        {
            var deny = await ForbidClasseSchoolAsync(idClasse);
            if (deny != null)
                return deny;

            try
            {
                return Ok(await _vEleveRepository.GetByClasseAsync(idClasse, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("tuteur/{idTuteur}")]
        public async Task<IActionResult> GetV_ElevesByTuteur(
            int idTuteur,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByTuteurAsync(idTuteur, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("statut/{statut}")]
        public async Task<IActionResult> GetV_ElevesByStatut(
            bool statut,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByStatutAsync(statut, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("genre/{genre}")]
        public async Task<IActionResult> GetV_ElevesByGenre(
            string genre,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByGenreAsync(genre, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("nationalite/{nationalite}")]
        public async Task<IActionResult> GetV_ElevesByNationalite(
            string nationalite,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByNationaliteAsync(nationalite, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("province/{province}")]
        public async Task<IActionResult> GetV_ElevesByProvince(
            string province,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByProvinceAsync(province, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("ville/{ville}")]
        public async Task<IActionResult> GetV_ElevesByVille(
            string ville,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByVilleAsync(ville, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("commune/{commune}")]
        public async Task<IActionResult> GetV_ElevesByCommune(
            string commune,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByCommuneAsync(commune, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("date-creation/{date}")]
        public async Task<IActionResult> GetV_ElevesByDateCreation(
            DateTime date,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByDateCreationAsync(date, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("date-creation-range")]
        public async Task<IActionResult> GetV_ElevesByDateCreationRange(
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByDateCreationRangeAsync(
                    dateDebut, dateFin, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("date-naissance/{dateNaissance}")]
        public async Task<IActionResult> GetV_ElevesByDateNaissance(
            DateTime dateNaissance,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByDateNaissanceAsync(dateNaissance, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("date-naissance-range")]
        public async Task<IActionResult> GetV_ElevesByDateNaissanceRange(
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByDateNaissanceRangeAsync(
                    dateDebut, dateFin, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("nom-complet/{nomComplet}")]
        public async Task<IActionResult> GetV_ElevesByNomComplet(
            string nomComplet,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByNomCompletAsync(nomComplet, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("nom/{nom}")]
        public async Task<IActionResult> GetV_ElevesByNom(
            string nom,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByNomAsync(nom, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("prenom/{prenom}")]
        public async Task<IActionResult> GetV_ElevesByPrenom(
            string prenom,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByPrenomAsync(prenom, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("lieu-naissance/{lieuNaissance}")]
        public async Task<IActionResult> GetV_ElevesByLieuNaissance(
            string lieuNaissance,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByLieuNaissanceAsync(lieuNaissance, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("section/{idSection}")]
        public async Task<IActionResult> GetV_ElevesBySection(int idSection, [FromQuery] int? idAnneeScolaire = null)
        {
            var deny = await ForbidSectionSchoolAsync(idSection);
            if (deny != null)
                return deny;

            try
            {
                return Ok(await _vEleveRepository.GetBySectionAsync(idSection, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("option/{idOption}")]
        public async Task<IActionResult> GetV_ElevesByOption(int idOption, [FromQuery] int? idAnneeScolaire = null)
        {
            var deny = await ForbidOptionSchoolAsync(idOption);
            if (deny != null)
                return deny;

            try
            {
                return Ok(await _vEleveRepository.GetByOptionAsync(idOption, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("statut-tuteur/{statutTuteur}")]
        public async Task<IActionResult> GetV_ElevesByStatutTuteur(
            bool statutTuteur,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByStatutTuteurAsync(statutTuteur, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("email-tuteur/{emailTuteur}")]
        public async Task<IActionResult> GetV_ElevesByEmailTuteur(
            string emailTuteur,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByEmailTuteurAsync(emailTuteur, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("telephone-tuteur/{telephoneTuteur}")]
        public async Task<IActionResult> GetV_ElevesByTelephoneTuteur(
            string telephoneTuteur,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetByTelephoneTuteurAsync(telephoneTuteur, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetV_ElevesCount(
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetCountAsync(resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("count/ecole/{idEcole}")]
        public async Task<IActionResult> GetV_ElevesCountByEcole(int idEcole, [FromQuery] int? idAnneeScolaire = null)
        {
            var deny = this.ForbidIfWrongSchool(idEcole);
            if (deny != null)
                return deny;

            try
            {
                return Ok(await _vEleveRepository.GetCountByEcoleAsync(idEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("count/classe/{idClasse}")]
        public async Task<IActionResult> GetV_ElevesCountByClasse(int idClasse, [FromQuery] int? idAnneeScolaire = null)
        {
            var deny = await ForbidClasseSchoolAsync(idClasse);
            if (deny != null)
                return deny;

            try
            {
                return Ok(await _vEleveRepository.GetCountByClasseAsync(idClasse, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("count/tuteur/{idTuteur}")]
        public async Task<IActionResult> GetV_ElevesCountByTuteur(
            int idTuteur,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetCountByTuteurAsync(idTuteur, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("count/statut/{statut}")]
        public async Task<IActionResult> GetV_ElevesCountByStatut(
            bool statut,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetCountByStatutAsync(statut, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("count/genre/{genre}")]
        public async Task<IActionResult> GetV_ElevesCountByGenre(
            string genre,
            [FromQuery] int? idEcole = null,
            [FromQuery] int? idAnneeScolaire = null)
        {
            var resolveError = this.TryResolveListIdEcole(idEcole, out var resolvedEcole);
            if (resolveError != null)
                return resolveError;

            try
            {
                return Ok(await _vEleveRepository.GetCountByGenreAsync(genre, resolvedEcole, idAnneeScolaire));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> V_EleveExists(int id) =>
            Ok(await _vEleveRepository.ExistsAsync(id));

        [HttpGet("exists/reference/{reference}")]
        public async Task<ActionResult<bool>> V_EleveExistsByReference(Guid reference) =>
            Ok(await _vEleveRepository.ExistsByReferenceAsync(reference));

        [HttpGet("exists/matricule/{matricule}")]
        public async Task<ActionResult<bool>> V_EleveExistsByMatricule(string matricule) =>
            Ok(await _vEleveRepository.ExistsByMatriculeAsync(matricule));
    }
}
