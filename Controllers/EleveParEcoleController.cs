using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Élèves par école - Token JWT requis
    public class EleveParEcoleController : ControllerBase
    {
        private readonly IEleveParEcoleRepository _eleveParEcoleRepository;

        public EleveParEcoleController(IEleveParEcoleRepository eleveParEcoleRepository)
        {
            _eleveParEcoleRepository = eleveParEcoleRepository;
        }

        // GET: api/EleveParEcole
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EleveParEcoleDTO>>> GetAll()
        {
            var eleves = await _eleveParEcoleRepository.GetAllAsync();
            return Ok(eleves);
        }

        // GET: api/EleveParEcole/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EleveParEcoleDTO>> GetById(int id)
        {
            var eleve = await _eleveParEcoleRepository.GetByIdAsync(id);
            if (eleve == null)
            {
                return NotFound($"Aucun �l�ve trouv� avec l'ID {id}");
            }
            return Ok(eleve);
        }

        // GET: api/EleveParEcole/ecole/5
        [HttpGet("ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<EleveParEcoleDTO>>> GetByEcole(int idEcole)
        {
            var eleves = await _eleveParEcoleRepository.GetByEcoleAsync(idEcole);
            return Ok(eleves);
        }

        // GET: api/EleveParEcole/classe/5
        [HttpGet("classe/{idClasse}")]
        public async Task<ActionResult<IEnumerable<EleveParEcoleDTO>>> GetByClasse(int idClasse)
        {
            var eleves = await _eleveParEcoleRepository.GetByClasseAsync(idClasse);
            return Ok(eleves);
        }

        // GET: api/EleveParEcole/direction/5
        [HttpGet("direction/{idDirection}")]
        public async Task<ActionResult<IEnumerable<EleveParEcoleDTO>>> GetByDirection(int idDirection)
        {
            var eleves = await _eleveParEcoleRepository.GetByDirectionAsync(idDirection);
            return Ok(eleves);
        }

        // GET: api/EleveParEcole/option/5
        [HttpGet("option/{idOption}")]
        public async Task<ActionResult<IEnumerable<EleveParEcoleDTO>>> GetByOption(int idOption)
        {
            var eleves = await _eleveParEcoleRepository.GetByOptionAsync(idOption);
            return Ok(eleves);
        }

        // GET: api/EleveParEcole/tuteur/5
        [HttpGet("tuteur/{idTuteur}")]
        public async Task<ActionResult<IEnumerable<EleveParEcoleDTO>>> GetByTuteur(int idTuteur)
        {
            var eleves = await _eleveParEcoleRepository.GetByTuteurAsync(idTuteur);
            return Ok(eleves);
        }

        // GET: api/EleveParEcole/statut/actif
        [HttpGet("statut/{statut}")]
        public async Task<ActionResult<IEnumerable<EleveParEcoleDTO>>> GetByStatut(string statut)
        {
            if (!bool.TryParse(statut, out bool statutBool))
            {
                return BadRequest("Le param�tre statut doit �tre 'true' ou 'false'");
            }
            
            var eleves = await _eleveParEcoleRepository.GetByStatutAsync(statutBool);
            return Ok(eleves);
        }

        // GET: api/EleveParEcole/genre/M
        [HttpGet("genre/{genre}")]
        public async Task<ActionResult<IEnumerable<EleveParEcoleDTO>>> GetByGenre(string genre)
        {
            var eleves = await _eleveParEcoleRepository.GetByGenreAsync(genre);
            return Ok(eleves);
        }

        // GET: api/EleveParEcole/age-range?minAge=10&maxAge=15
        [HttpGet("age-range")]
        public async Task<ActionResult<IEnumerable<EleveParEcoleDTO>>> GetByAgeRange(
            [FromQuery] int minAge,
            [FromQuery] int maxAge)
        {
            var eleves = await _eleveParEcoleRepository.GetByAgeRangeAsync(minAge, maxAge);
            return Ok(eleves);
        }

        // GET: api/EleveParEcole/province/Kinshasa
        [HttpGet("province/{province}")]
        public async Task<ActionResult<IEnumerable<EleveParEcoleDTO>>> GetByProvince(string province)
        {
            var eleves = await _eleveParEcoleRepository.GetByProvinceAsync(province);
            return Ok(eleves);
        }

        // GET: api/EleveParEcole/ville/Kinshasa
        [HttpGet("ville/{ville}")]
        public async Task<ActionResult<IEnumerable<EleveParEcoleDTO>>> GetByVille(string ville)
        {
            var eleves = await _eleveParEcoleRepository.GetByVilleAsync(ville);
            return Ok(eleves);
        }

        // GET: api/EleveParEcole/commune/Gombe
        [HttpGet("commune/{commune}")]
        public async Task<ActionResult<IEnumerable<EleveParEcoleDTO>>> GetByCommune(string commune)
        {
            var eleves = await _eleveParEcoleRepository.GetByCommuneAsync(commune);
            return Ok(eleves);
        }

        // GET: api/EleveParEcole/tuteur-contact/+243123456789
        [HttpGet("tuteur-contact/{contact}")]
        public async Task<ActionResult<IEnumerable<EleveParEcoleDTO>>> GetByTuteurContact(string contact)
        {
            if (string.IsNullOrWhiteSpace(contact))
            {
                return BadRequest("Le contact ne peut pas �tre vide");
            }

            var eleves = await _eleveParEcoleRepository.GetByTuteurContactAsync(contact);
            return Ok(eleves);
        }

        // GET: api/EleveParEcole/search?term=John
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<EleveParEcoleDTO>>> Search([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest("Le terme de recherche ne peut pas �tre vide");
            }

            var eleves = await _eleveParEcoleRepository.SearchAsync(term);
            return Ok(eleves);
        }

        // GET: api/EleveParEcole/count/ecole/5
        [HttpGet("count/ecole/{idEcole}")]
        public async Task<ActionResult<int>> GetCountByEcole(int idEcole)
        {
            var count = await _eleveParEcoleRepository.GetCountByEcoleAsync(idEcole);
            return Ok(count);
        }

        // GET: api/EleveParEcole/count/classe/5
        [HttpGet("count/classe/{idClasse}")]
        public async Task<ActionResult<int>> GetCountByClasse(int idClasse)
        {
            var count = await _eleveParEcoleRepository.GetCountByClasseAsync(idClasse);
            return Ok(count);
        }

        // GET: api/EleveParEcole/count/direction/5
        [HttpGet("count/direction/{idDirection}")]
        public async Task<ActionResult<int>> GetCountByDirection(int idDirection)
        {
            var count = await _eleveParEcoleRepository.GetCountByDirectionAsync(idDirection);
            return Ok(count);
        }
    }
}
