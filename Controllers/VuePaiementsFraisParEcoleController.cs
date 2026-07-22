using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Vue Paiements/Frais - Token JWT requis
    public class VuePaiementsFraisParEcoleController : ControllerBase
    {
        private readonly IVuePaiementsFraisParEcoleRepository _vuePaiementsRepository;

        public VuePaiementsFraisParEcoleController(IVuePaiementsFraisParEcoleRepository vuePaiementsRepository)
        {
            _vuePaiementsRepository = vuePaiementsRepository;
        }

        // GET: api/VuePaiementsFraisParEcole
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetAll()
        {
            var paiements = await _vuePaiementsRepository.GetAllAsync();
            return Ok(paiements);
        }

        // GET: api/VuePaiementsFraisParEcole/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VuePaiementsFraisParEcoleDTO>> GetById(int id)
        {
            var paiement = await _vuePaiementsRepository.GetByIdAsync(id);
            if (paiement == null)
            {
                return NotFound($"Aucun paiement trouv� avec l'ID {id}");
            }
            return Ok(paiement);
        }

        // Filtres par �cole
        [HttpGet("ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByEcole(int idEcole)
        {
            var paiements = await _vuePaiementsRepository.GetByEcoleAsync(idEcole);
            return Ok(paiements);
        }

        [HttpGet("ecole-name/{nomEcole}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByEcoleName(string nomEcole)
        {
            var paiements = await _vuePaiementsRepository.GetByEcoleNameAsync(nomEcole);
            return Ok(paiements);
        }

        [HttpGet("type-ecole/{typeEcole}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByTypeEcole(string typeEcole)
        {
            var paiements = await _vuePaiementsRepository.GetByTypeEcoleAsync(typeEcole);
            return Ok(paiements);
        }

        // Filtres par �l�ve
        [HttpGet("eleve/{idEleve}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByEleve(int idEleve)
        {
            var paiements = await _vuePaiementsRepository.GetByEleveAsync(idEleve);
            return Ok(paiements);
        }

        [HttpGet("eleve-reference/{referenceEleve}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByEleveReference(Guid referenceEleve)
        {
            var paiements = await _vuePaiementsRepository.GetByEleveReferenceAsync(referenceEleve);
            return Ok(paiements);
        }

        [HttpGet("eleve-matricule/{matricule}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByEleveMatricule(string matricule)
        {
            var paiements = await _vuePaiementsRepository.GetByEleveMatriculeAsync(matricule);
            return Ok(paiements);
        }

        [HttpGet("eleve-name/{nomEleve}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByEleveName(string nomEleve)
        {
            var paiements = await _vuePaiementsRepository.GetByEleveNameAsync(nomEleve);
            return Ok(paiements);
        }

        [HttpGet("eleve-genre/{genre}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByEleveGenre(string genre)
        {
            var paiements = await _vuePaiementsRepository.GetByEleveGenreAsync(genre);
            return Ok(paiements);
        }

        [HttpGet("eleve-statut/{statut}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByEleveStatut(string statut)
        {
            if (!bool.TryParse(statut, out bool statutBool))
            {
                return BadRequest("Le param�tre statut doit �tre 'true' ou 'false'");
            }
            
            var paiements = await _vuePaiementsRepository.GetByEleveStatutAsync(statutBool);
            return Ok(paiements);
        }

        // Filtres par classe
        [HttpGet("classe/{idClasse}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByClasse(int idClasse)
        {
            var paiements = await _vuePaiementsRepository.GetByClasseAsync(idClasse);
            return Ok(paiements);
        }

        [HttpGet("classe-name/{nomClasse}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByClasseName(string nomClasse)
        {
            var paiements = await _vuePaiementsRepository.GetByClasseNameAsync(nomClasse);
            return Ok(paiements);
        }

        // Filtres par section
        [HttpGet("section/{idSection}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetBySection(int idSection)
        {
            var paiements = await _vuePaiementsRepository.GetBySectionAsync(idSection);
            return Ok(paiements);
        }

        [HttpGet("section-name/{nomSection}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetBySectionName(string nomSection)
        {
            var paiements = await _vuePaiementsRepository.GetBySectionNameAsync(nomSection);
            return Ok(paiements);
        }

        // Filtres par direction
        [HttpGet("direction/{idDirection}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByDirection(int idDirection)
        {
            var paiements = await _vuePaiementsRepository.GetByDirectionAsync(idDirection);
            return Ok(paiements);
        }

        [HttpGet("direction-name/{nomDirection}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByDirectionName(string nomDirection)
        {
            var paiements = await _vuePaiementsRepository.GetByDirectionNameAsync(nomDirection);
            return Ok(paiements);
        }

        // Filtres par option
        [HttpGet("option/{idOption}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByOption(int idOption)
        {
            var paiements = await _vuePaiementsRepository.GetByOptionAsync(idOption);
            return Ok(paiements);
        }

        [HttpGet("option-name/{nomOption}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByOptionName(string nomOption)
        {
            var paiements = await _vuePaiementsRepository.GetByOptionNameAsync(nomOption);
            return Ok(paiements);
        }

        // Filtres par tuteur
        [HttpGet("tuteur/{idTuteur}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByTuteur(int idTuteur)
        {
            var paiements = await _vuePaiementsRepository.GetByTuteurAsync(idTuteur);
            return Ok(paiements);
        }

        [HttpGet("tuteur-name/{nomTuteur}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByTuteurName(string nomTuteur)
        {
            var paiements = await _vuePaiementsRepository.GetByTuteurNameAsync(nomTuteur);
            return Ok(paiements);
        }

        [HttpGet("tuteur-contact/{contact}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByTuteurContact(string contact)
        {
            var paiements = await _vuePaiementsRepository.GetByTuteurContactAsync(contact);
            return Ok(paiements);
        }

        [HttpGet("tuteur-statut/{statut}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByTuteurStatut(bool statut)
        {
            var paiements = await _vuePaiementsRepository.GetByTuteurStatutAsync(statut);
            return Ok(paiements);
        }

        // Filtres par frais
        [HttpGet("frais/{idFrais}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByFrais(int idFrais)
        {
            var paiements = await _vuePaiementsRepository.GetByFraisAsync(idFrais);
            return Ok(paiements);
        }

        [HttpGet("frais-libelle/{libelleFrais}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByFraisLibelle(string libelleFrais)
        {
            var paiements = await _vuePaiementsRepository.GetByFraisLibelleAsync(libelleFrais);
            return Ok(paiements);
        }

        [HttpGet("frais-montant-range")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByFraisMontantRange(
            [FromQuery] double minMontant,
            [FromQuery] double maxMontant)
        {
            var paiements = await _vuePaiementsRepository.GetByFraisMontantRangeAsync(minMontant, maxMontant);
            return Ok(paiements);
        }

        // Filtres par paiement
        [HttpGet("paiement-statut/{statut}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByPaiementStatut(string statut)
        {
            var paiements = await _vuePaiementsRepository.GetByPaiementStatutAsync(statut);
            return Ok(paiements);
        }

        [HttpGet("mode-paiement/{modePaiement}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByModePaiement(string modePaiement)
        {
            var paiements = await _vuePaiementsRepository.GetByModePaiementAsync(modePaiement);
            return Ok(paiements);
        }

        [HttpGet("devise/{devise}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByDevise(string devise)
        {
            var paiements = await _vuePaiementsRepository.GetByDeviseAsync(devise);
            return Ok(paiements);
        }

        [HttpGet("montant-range")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByMontantRange(
            [FromQuery] double minMontant,
            [FromQuery] double maxMontant)
        {
            var paiements = await _vuePaiementsRepository.GetByMontantRangeAsync(minMontant, maxMontant);
            return Ok(paiements);
        }

        [HttpGet("date-paiement-range")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByDatePaiementRange(
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var paiements = await _vuePaiementsRepository.GetByDatePaiementRangeAsync(dateDebut, dateFin);
            return Ok(paiements);
        }

        [HttpGet("date-paiement/{datePaiement:datetime}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByDatePaiement(DateTime datePaiement)
        {
            var paiements = await _vuePaiementsRepository.GetByDatePaiementAsync(datePaiement);
            return Ok(paiements);
        }

        [HttpGet("reference-transaction/{referenceTransaction}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByReferenceTransaction(string referenceTransaction)
        {
            var paiements = await _vuePaiementsRepository.GetByReferenceTransactionAsync(referenceTransaction);
            return Ok(paiements);
        }

        // Filtres par localisation
        [HttpGet("province/{province}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByProvince(string province)
        {
            var paiements = await _vuePaiementsRepository.GetByProvinceAsync(province);
            return Ok(paiements);
        }

        [HttpGet("ville/{ville}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByVille(string ville)
        {
            var paiements = await _vuePaiementsRepository.GetByVilleAsync(ville);
            return Ok(paiements);
        }

        [HttpGet("commune/{commune}")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> GetByCommune(string commune)
        {
            var paiements = await _vuePaiementsRepository.GetByCommuneAsync(commune);
            return Ok(paiements);
        }

        // Recherche g�n�rale
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<VuePaiementsFraisParEcoleDTO>>> Search([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest("Le terme de recherche ne peut pas �tre vide");
            }

            var paiements = await _vuePaiementsRepository.SearchAsync(term);
            return Ok(paiements);
        }

        // Comptages
        [HttpGet("count/ecole/{idEcole}")]
        public async Task<ActionResult<int>> GetCountByEcole(int idEcole)
        {
            var count = await _vuePaiementsRepository.GetCountByEcoleAsync(idEcole);
            return Ok(count);
        }

        [HttpGet("count/classe/{idClasse}")]
        public async Task<ActionResult<int>> GetCountByClasse(int idClasse)
        {
            var count = await _vuePaiementsRepository.GetCountByClasseAsync(idClasse);
            return Ok(count);
        }

        [HttpGet("count/section/{idSection}")]
        public async Task<ActionResult<int>> GetCountBySection(int idSection)
        {
            var count = await _vuePaiementsRepository.GetCountBySectionAsync(idSection);
            return Ok(count);
        }

        [HttpGet("count/direction/{idDirection}")]
        public async Task<ActionResult<int>> GetCountByDirection(int idDirection)
        {
            var count = await _vuePaiementsRepository.GetCountByDirectionAsync(idDirection);
            return Ok(count);
        }

        [HttpGet("count/tuteur/{idTuteur}")]
        public async Task<ActionResult<int>> GetCountByTuteur(int idTuteur)
        {
            var count = await _vuePaiementsRepository.GetCountByTuteurAsync(idTuteur);
            return Ok(count);
        }

        [HttpGet("count/frais/{idFrais}")]
        public async Task<ActionResult<int>> GetCountByFrais(int idFrais)
        {
            var count = await _vuePaiementsRepository.GetCountByFraisAsync(idFrais);
            return Ok(count);
        }

        [HttpGet("count/paiement-statut/{statut}")]
        public async Task<ActionResult<int>> GetCountByPaiementStatut(string statut)
        {
            var count = await _vuePaiementsRepository.GetCountByPaiementStatutAsync(statut);
            return Ok(count);
        }

        [HttpGet("count/mode-paiement/{modePaiement}")]
        public async Task<ActionResult<int>> GetCountByModePaiement(string modePaiement)
        {
            var count = await _vuePaiementsRepository.GetCountByModePaiementAsync(modePaiement);
            return Ok(count);
        }

        [HttpGet("count/date-range")]
        public async Task<ActionResult<int>> GetCountByDateRange(
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var count = await _vuePaiementsRepository.GetCountByDatePaiementRangeAsync(dateDebut, dateFin);
            return Ok(count);
        }

        // Statistiques
        [HttpGet("stats/total-montant/ecole/{idEcole}")]
        public async Task<ActionResult<double>> GetTotalMontantByEcole(int idEcole)
        {
            var total = await _vuePaiementsRepository.GetTotalMontantByEcoleAsync(idEcole);
            return Ok(total);
        }

        [HttpGet("stats/total-montant/date-range")]
        public async Task<ActionResult<double>> GetTotalMontantByDateRange(
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var total = await _vuePaiementsRepository.GetTotalMontantByDateRangeAsync(dateDebut, dateFin);
            return Ok(total);
        }

        [HttpGet("stats/total-montant/paiement-statut/{statut}")]
        public async Task<ActionResult<double>> GetTotalMontantByPaiementStatut(string statut)
        {
            var total = await _vuePaiementsRepository.GetTotalMontantByPaiementStatutAsync(statut);
            return Ok(total);
        }

        [HttpGet("stats/total-montant/mode-paiement/{modePaiement}")]
        public async Task<ActionResult<double>> GetTotalMontantByModePaiement(string modePaiement)
        {
            var total = await _vuePaiementsRepository.GetTotalMontantByModePaiementAsync(modePaiement);
            return Ok(total);
        }
    }
}
