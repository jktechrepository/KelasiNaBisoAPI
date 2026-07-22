using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Vue Répertoire Agents - Token JWT requis
    public class VueRepertoireAgentsParParentController : ControllerBase
    {
        private readonly IVueRepertoireAgentsParParentRepository _repertoireRepository;

        public VueRepertoireAgentsParParentController(IVueRepertoireAgentsParParentRepository repertoireRepository)
        {
            _repertoireRepository = repertoireRepository;
        }

        // Endpoints généraux
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetAll()
        {
            var repertoire = await _repertoireRepository.GetAllAsync();
            return Ok(repertoire);
        }

        [HttpGet("{idAgent}")]
        public async Task<ActionResult<VueRepertoireAgentsParParentDTO>> GetById(int idAgent)
        {
            var agent = await _repertoireRepository.GetByIdAsync(idAgent);
            if (agent == null)
                return NotFound($"Agent avec l'ID {idAgent} non trouvé");

            return Ok(agent);
        }

        // Filtres par parent/tuteur
        [HttpGet("parent/contact/{contactParent}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByParentContact(string contactParent)
        {
            var repertoire = await _repertoireRepository.GetByParentContactAsync(contactParent);
            return Ok(repertoire);
        }

        [HttpGet("parent/nom/{nomParent}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByParentName(string nomParent)
        {
            var repertoire = await _repertoireRepository.GetByParentNameAsync(nomParent);
            return Ok(repertoire);
        }

        [HttpGet("parent/id/{idTuteur}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByParentId(int idTuteur)
        {
            var repertoire = await _repertoireRepository.GetByParentIdAsync(idTuteur);
            return Ok(repertoire);
        }

        // Filtres par élève
        [HttpGet("eleve/{idEleve}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByEleve(int idEleve)
        {
            var repertoire = await _repertoireRepository.GetByEleveAsync(idEleve);
            return Ok(repertoire);
        }

        [HttpGet("eleve/nom/{nomEleve}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByEleveName(string nomEleve)
        {
            var repertoire = await _repertoireRepository.GetByEleveNameAsync(nomEleve);
            return Ok(repertoire);
        }

        [HttpGet("eleve/matricule/{matricule}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByEleveMatricule(string matricule)
        {
            var repertoire = await _repertoireRepository.GetByEleveMatriculeAsync(matricule);
            return Ok(repertoire);
        }

        // Filtres par agent
        [HttpGet("agent/{idAgent}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByAgent(int idAgent)
        {
            var repertoire = await _repertoireRepository.GetByAgentAsync(idAgent);
            return Ok(repertoire);
        }

        [HttpGet("agent/nom/{nomAgent}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByAgentName(string nomAgent)
        {
            var repertoire = await _repertoireRepository.GetByAgentNameAsync(nomAgent);
            return Ok(repertoire);
        }

        [HttpGet("agent/contact/{contactAgent}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByAgentContact(string contactAgent)
        {
            var repertoire = await _repertoireRepository.GetByAgentContactAsync(contactAgent);
            return Ok(repertoire);
        }

        [HttpGet("agent/genre/{genre}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByAgentGenre(string genre)
        {
            var repertoire = await _repertoireRepository.GetByAgentGenreAsync(genre);
            return Ok(repertoire);
        }

        // Filtres par cours
        [HttpGet("cours/{idCours}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByCours(int idCours)
        {
            var repertoire = await _repertoireRepository.GetByCoursAsync(idCours);
            return Ok(repertoire);
        }

        [HttpGet("cours/nom/{nomCours}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByCoursName(string nomCours)
        {
            var repertoire = await _repertoireRepository.GetByCoursNameAsync(nomCours);
            return Ok(repertoire);
        }

        // Filtres par classe
        [HttpGet("classe/{idClasse}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByClasse(int idClasse)
        {
            var repertoire = await _repertoireRepository.GetByClasseAsync(idClasse);
            return Ok(repertoire);
        }

        [HttpGet("classe/nom/{nomClasse}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByClasseName(string nomClasse)
        {
            var repertoire = await _repertoireRepository.GetByClasseNameAsync(nomClasse);
            return Ok(repertoire);
        }

        // Filtres par école
        [HttpGet("ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByEcole(int idEcole)
        {
            var repertoire = await _repertoireRepository.GetByEcoleAsync(idEcole);
            return Ok(repertoire);
        }

        [HttpGet("ecole/nom/{nomEcole}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByEcoleName(string nomEcole)
        {
            var repertoire = await _repertoireRepository.GetByEcoleNameAsync(nomEcole);
            return Ok(repertoire);
        }

        [HttpGet("ecole/type/{typeEcole}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByTypeEcole(string typeEcole)
        {
            var repertoire = await _repertoireRepository.GetByTypeEcoleAsync(typeEcole);
            return Ok(repertoire);
        }

        // Filtres par année scolaire
        [HttpGet("annee-scolaire/{idAnneeScolaire}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByAnneeScolaire(int idAnneeScolaire)
        {
            var repertoire = await _repertoireRepository.GetByAnneeScolaireAsync(idAnneeScolaire);
            return Ok(repertoire);
        }

        [HttpGet("annee-scolaire/libelle/{libelleAnnee}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> GetByAnneeScolaireLibelle(string libelleAnnee)
        {
            var repertoire = await _repertoireRepository.GetByAnneeScolaireLibelleAsync(libelleAnnee);
            return Ok(repertoire);
        }

        // Recherche générale
        [HttpGet("search/{searchTerm}")]
        public async Task<ActionResult<IEnumerable<VueRepertoireAgentsParParentDTO>>> Search(string searchTerm)
        {
            var repertoire = await _repertoireRepository.SearchAsync(searchTerm);
            return Ok(repertoire);
        }

        // Comptages
        [HttpGet("count/parent/{idTuteur}")]
        public async Task<ActionResult<int>> GetCountByParent(int idTuteur)
        {
            var count = await _repertoireRepository.GetCountByParentAsync(idTuteur);
            return Ok(count);
        }

        [HttpGet("count/agent/{idAgent}")]
        public async Task<ActionResult<int>> GetCountByAgent(int idAgent)
        {
            var count = await _repertoireRepository.GetCountByAgentAsync(idAgent);
            return Ok(count);
        }

        [HttpGet("count/cours/{idCours}")]
        public async Task<ActionResult<int>> GetCountByCours(int idCours)
        {
            var count = await _repertoireRepository.GetCountByCoursAsync(idCours);
            return Ok(count);
        }

        [HttpGet("count/classe/{idClasse}")]
        public async Task<ActionResult<int>> GetCountByClasse(int idClasse)
        {
            var count = await _repertoireRepository.GetCountByClasseAsync(idClasse);
            return Ok(count);
        }

        [HttpGet("count/ecole/{idEcole}")]
        public async Task<ActionResult<int>> GetCountByEcole(int idEcole)
        {
            var count = await _repertoireRepository.GetCountByEcoleAsync(idEcole);
            return Ok(count);
        }

        [HttpGet("count/annee-scolaire/{idAnneeScolaire}")]
        public async Task<ActionResult<int>> GetCountByAnneeScolaire(int idAnneeScolaire)
        {
            var count = await _repertoireRepository.GetCountByAnneeScolaireAsync(idAnneeScolaire);
            return Ok(count);
        }

        // Statistiques
        [HttpGet("stats/agents/parent/{contactParent}")]
        public async Task<ActionResult<object>> GetStatistiquesAgentsByParent(string contactParent)
        {
            var stats = await _repertoireRepository.GetStatistiquesAgentsByParentAsync(contactParent);
            return Ok(stats);
        }

        [HttpGet("stats/cours/parent/{contactParent}")]
        public async Task<ActionResult<object>> GetStatistiquesCoursByParent(string contactParent)
        {
            var stats = await _repertoireRepository.GetStatistiquesCoursByParentAsync(contactParent);
            return Ok(stats);
        }

        [HttpGet("repertoire-complet/parent/{contactParent}")]
        public async Task<ActionResult<object>> GetRepertoireCompletByParent(string contactParent)
        {
            var repertoire = await _repertoireRepository.GetRepertoireCompletByParentAsync(contactParent);
            return Ok(repertoire);
        }
    }
}

