using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Vue Élèves - Token JWT requis
    public class V_EleveController : ControllerBase
    {
        private readonly IV_EleveRepository _vEleveRepository;

        public V_EleveController(IV_EleveRepository vEleveRepository)
        {
            _vEleveRepository = vEleveRepository;
        }

        // GET: api/V_Eleve
        [HttpGet]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_Eleves()
        {
            var vEleves = await _vEleveRepository.GetAllAsync();
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/5
        [HttpGet("{id}")]
        public async Task<ActionResult<V_Eleve>> GetV_Eleve(int id)
        {
            var vEleve = await _vEleveRepository.GetByIdAsync(id);
            if (vEleve == null)
            {
                return NotFound();
            }
            return Ok(vEleve);
        }

        // GET: api/V_Eleve/reference/{reference}
        [HttpGet("reference/{reference}")]
        public async Task<ActionResult<V_Eleve>> GetV_EleveByReference(Guid reference)
        {
            var vEleve = await _vEleveRepository.GetByReferenceAsync(reference);
            if (vEleve == null)
            {
                return NotFound();
            }
            return Ok(vEleve);
        }

        // GET: api/V_Eleve/matricule/{matricule}
        [HttpGet("matricule/{matricule}")]
        public async Task<ActionResult<V_Eleve>> GetV_EleveByMatricule(string matricule)
        {
            var vEleve = await _vEleveRepository.GetByMatriculeAsync(matricule);
            if (vEleve == null)
            {
                return NotFound();
            }
            return Ok(vEleve);
        }

        // GET: api/V_Eleve/ecole/{idEcole}
        [HttpGet("ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByEcole(int idEcole)
        {
            var vEleves = await _vEleveRepository.GetByEcoleAsync(idEcole);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/classe/{idClasse}
        [HttpGet("classe/{idClasse}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByClasse(int idClasse)
        {
            var vEleves = await _vEleveRepository.GetByClasseAsync(idClasse);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/tuteur/{idTuteur}
        [HttpGet("tuteur/{idTuteur}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByTuteur(int idTuteur)
        {
            var vEleves = await _vEleveRepository.GetByTuteurAsync(idTuteur);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/statut/{statut}
        [HttpGet("statut/{statut}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByStatut(bool statut)
        {
            var vEleves = await _vEleveRepository.GetByStatutAsync(statut);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/genre/{genre}
        [HttpGet("genre/{genre}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByGenre(string genre)
        {
            var vEleves = await _vEleveRepository.GetByGenreAsync(genre);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/nationalite/{nationalite}
        [HttpGet("nationalite/{nationalite}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByNationalite(string nationalite)
        {
            var vEleves = await _vEleveRepository.GetByNationaliteAsync(nationalite);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/province/{province}
        [HttpGet("province/{province}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByProvince(string province)
        {
            var vEleves = await _vEleveRepository.GetByProvinceAsync(province);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/ville/{ville}
        [HttpGet("ville/{ville}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByVille(string ville)
        {
            var vEleves = await _vEleveRepository.GetByVilleAsync(ville);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/commune/{commune}
        [HttpGet("commune/{commune}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByCommune(string commune)
        {
            var vEleves = await _vEleveRepository.GetByCommuneAsync(commune);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/date-creation/{date}
        [HttpGet("date-creation/{date}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByDateCreation(DateTime date)
        {
            var vEleves = await _vEleveRepository.GetByDateCreationAsync(date);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/date-creation-range
        [HttpGet("date-creation-range")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByDateCreationRange(
            [FromQuery] DateTime dateDebut, [FromQuery] DateTime dateFin)
        {
            var vEleves = await _vEleveRepository.GetByDateCreationRangeAsync(dateDebut, dateFin);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/date-naissance/{dateNaissance}
        [HttpGet("date-naissance/{dateNaissance}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByDateNaissance(DateTime dateNaissance)
        {
            var vEleves = await _vEleveRepository.GetByDateNaissanceAsync(dateNaissance);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/date-naissance-range
        [HttpGet("date-naissance-range")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByDateNaissanceRange(
            [FromQuery] DateTime dateDebut, [FromQuery] DateTime dateFin)
        {
            var vEleves = await _vEleveRepository.GetByDateNaissanceRangeAsync(dateDebut, dateFin);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/nom-complet/{nomComplet}
        [HttpGet("nom-complet/{nomComplet}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByNomComplet(string nomComplet)
        {
            var vEleves = await _vEleveRepository.GetByNomCompletAsync(nomComplet);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/nom/{nom}
        [HttpGet("nom/{nom}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByNom(string nom)
        {
            var vEleves = await _vEleveRepository.GetByNomAsync(nom);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/prenom/{prenom}
        [HttpGet("prenom/{prenom}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByPrenom(string prenom)
        {
            var vEleves = await _vEleveRepository.GetByPrenomAsync(prenom);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/lieu-naissance/{lieuNaissance}
        [HttpGet("lieu-naissance/{lieuNaissance}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByLieuNaissance(string lieuNaissance)
        {
            var vEleves = await _vEleveRepository.GetByLieuNaissanceAsync(lieuNaissance);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/section/{idSection}
        [HttpGet("section/{idSection}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesBySection(int idSection)
        {
            var vEleves = await _vEleveRepository.GetBySectionAsync(idSection);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/option/{idOption}
        [HttpGet("option/{idOption}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByOption(int idOption)
        {
            var vEleves = await _vEleveRepository.GetByOptionAsync(idOption);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/statut-tuteur/{statutTuteur}
        [HttpGet("statut-tuteur/{statutTuteur}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByStatutTuteur(bool statutTuteur)
        {
            var vEleves = await _vEleveRepository.GetByStatutTuteurAsync(statutTuteur);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/email-tuteur/{emailTuteur}
        [HttpGet("email-tuteur/{emailTuteur}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByEmailTuteur(string emailTuteur)
        {
            var vEleves = await _vEleveRepository.GetByEmailTuteurAsync(emailTuteur);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/telephone-tuteur/{telephoneTuteur}
        [HttpGet("telephone-tuteur/{telephoneTuteur}")]
        public async Task<ActionResult<IEnumerable<V_Eleve>>> GetV_ElevesByTelephoneTuteur(string telephoneTuteur)
        {
            var vEleves = await _vEleveRepository.GetByTelephoneTuteurAsync(telephoneTuteur);
            return Ok(vEleves);
        }

        // GET: api/V_Eleve/count
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetV_ElevesCount()
        {
            var count = await _vEleveRepository.GetCountAsync();
            return Ok(count);
        }

        // GET: api/V_Eleve/count/ecole/{idEcole}
        [HttpGet("count/ecole/{idEcole}")]
        public async Task<ActionResult<int>> GetV_ElevesCountByEcole(int idEcole)
        {
            var count = await _vEleveRepository.GetCountByEcoleAsync(idEcole);
            return Ok(count);
        }

        // GET: api/V_Eleve/count/classe/{idClasse}
        [HttpGet("count/classe/{idClasse}")]
        public async Task<ActionResult<int>> GetV_ElevesCountByClasse(int idClasse)
        {
            var count = await _vEleveRepository.GetCountByClasseAsync(idClasse);
            return Ok(count);
        }

        // GET: api/V_Eleve/count/tuteur/{idTuteur}
        [HttpGet("count/tuteur/{idTuteur}")]
        public async Task<ActionResult<int>> GetV_ElevesCountByTuteur(int idTuteur)
        {
            var count = await _vEleveRepository.GetCountByTuteurAsync(idTuteur);
            return Ok(count);
        }

        // GET: api/V_Eleve/count/statut/{statut}
        [HttpGet("count/statut/{statut}")]
        public async Task<ActionResult<int>> GetV_ElevesCountByStatut(bool statut)
        {
            var count = await _vEleveRepository.GetCountByStatutAsync(statut);
            return Ok(count);
        }

        // GET: api/V_Eleve/count/genre/{genre}
        [HttpGet("count/genre/{genre}")]
        public async Task<ActionResult<int>> GetV_ElevesCountByGenre(string genre)
        {
            var count = await _vEleveRepository.GetCountByGenreAsync(genre);
            return Ok(count);
        }

        // GET: api/V_Eleve/exists/{id}
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> V_EleveExists(int id)
        {
            var exists = await _vEleveRepository.ExistsAsync(id);
            return Ok(exists);
        }

        // GET: api/V_Eleve/exists/reference/{reference}
        [HttpGet("exists/reference/{reference}")]
        public async Task<ActionResult<bool>> V_EleveExistsByReference(Guid reference)
        {
            var exists = await _vEleveRepository.ExistsByReferenceAsync(reference);
            return Ok(exists);
        }

        // GET: api/V_Eleve/exists/matricule/{matricule}
        [HttpGet("exists/matricule/{matricule}")]
        public async Task<ActionResult<bool>> V_EleveExistsByMatricule(string matricule)
        {
            var exists = await _vEleveRepository.ExistsByMatriculeAsync(matricule);
            return Ok(exists);
        }
    }
}
