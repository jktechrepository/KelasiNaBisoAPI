using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using KelasiNaBiso.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Vue Pointage/Présence - Token JWT requis
    public class VuePointagePresenceParEcoleController : ControllerBase
    {
        private readonly IVuePointagePresenceParEcoleRepository _vuePointageRepository;

        public VuePointagePresenceParEcoleController(IVuePointagePresenceParEcoleRepository vuePointageRepository)
        {
            _vuePointageRepository = vuePointageRepository;
        }

        // GET: api/VuePointagePresenceParEcole
        [HttpGet]
        [RequireGlobalAccess]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetAll()
        {
            var pointages = await _vuePointageRepository.GetAllAsync();
            return Ok(pointages);
        }

        // GET: api/VuePointagePresenceParEcole/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VuePointagePresenceParEcoleDTO>> GetById(int id)
        {
            var pointage = await _vuePointageRepository.GetByIdAsync(id);
            if (pointage == null)
            {
                return NotFound($"Aucun pointage trouv� avec l'ID {id}");
            }
            return Ok(pointage);
        }

        // Filtres par �cole
        [HttpGet("ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByEcole(int idEcole)
        {
            var pointages = await _vuePointageRepository.GetByEcoleAsync(idEcole);
            return Ok(pointages);
        }

        [HttpGet("ecole-name/{nomEcole}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByEcoleName(string nomEcole)
        {
            var pointages = await _vuePointageRepository.GetByEcoleNameAsync(nomEcole);
            return Ok(pointages);
        }

        [HttpGet("type-ecole/{typeEcole}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByTypeEcole(string typeEcole)
        {
            var pointages = await _vuePointageRepository.GetByTypeEcoleAsync(typeEcole);
            return Ok(pointages);
        }

        // Filtres par �l�ve
        [HttpGet("eleve/{idEleve}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByEleve(int idEleve)
        {
            var pointages = await _vuePointageRepository.GetByEleveAsync(idEleve);
            return Ok(pointages);
        }

        [HttpGet("eleve-reference/{referenceEleve}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByEleveReference(Guid referenceEleve)
        {
            var pointages = await _vuePointageRepository.GetByEleveReferenceAsync(referenceEleve);
            return Ok(pointages);
        }

        [HttpGet("eleve-matricule/{matricule}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByEleveMatricule(string matricule)
        {
            var pointages = await _vuePointageRepository.GetByEleveMatriculeAsync(matricule);
            return Ok(pointages);
        }

        [HttpGet("eleve-name/{nomEleve}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByEleveName(string nomEleve)
        {
            var pointages = await _vuePointageRepository.GetByEleveNameAsync(nomEleve);
            return Ok(pointages);
        }

        [HttpGet("eleve-genre/{genre}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByEleveGenre(string genre)
        {
            var pointages = await _vuePointageRepository.GetByEleveGenreAsync(genre);
            return Ok(pointages);
        }

        [HttpGet("eleve-statut/{statut}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByEleveStatut(string statut)
        {
            if (!bool.TryParse(statut, out bool statutBool))
            {
                return BadRequest("Le param�tre statut doit �tre 'true' ou 'false'");
            }
            
            var pointages = await _vuePointageRepository.GetByEleveStatutAsync(statutBool);
            return Ok(pointages);
        }

        // Filtres par classe
        [HttpGet("classe/{idClasse}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByClasse(int idClasse)
        {
            var pointages = await _vuePointageRepository.GetByClasseAsync(idClasse);
            return Ok(pointages);
        }

        [HttpGet("classe-name/{nomClasse}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByClasseName(string nomClasse)
        {
            var pointages = await _vuePointageRepository.GetByClasseNameAsync(nomClasse);
            return Ok(pointages);
        }

        // Filtres par section
        [HttpGet("section/{idSection}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetBySection(int idSection)
        {
            var pointages = await _vuePointageRepository.GetBySectionAsync(idSection);
            return Ok(pointages);
        }

        [HttpGet("section-name/{nomSection}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetBySectionName(string nomSection)
        {
            var pointages = await _vuePointageRepository.GetBySectionNameAsync(nomSection);
            return Ok(pointages);
        }

        // Filtres par direction
        [HttpGet("direction/{idDirection}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByDirection(int idDirection)
        {
            var pointages = await _vuePointageRepository.GetByDirectionAsync(idDirection);
            return Ok(pointages);
        }

        [HttpGet("direction-name/{nomDirection}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByDirectionName(string nomDirection)
        {
            var pointages = await _vuePointageRepository.GetByDirectionNameAsync(nomDirection);
            return Ok(pointages);
        }

        // Filtres par option
        [HttpGet("option/{idOption}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByOption(int idOption)
        {
            var pointages = await _vuePointageRepository.GetByOptionAsync(idOption);
            return Ok(pointages);
        }

        [HttpGet("option-name/{nomOption}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByOptionName(string nomOption)
        {
            var pointages = await _vuePointageRepository.GetByOptionNameAsync(nomOption);
            return Ok(pointages);
        }

        // Filtres par tuteur
        [HttpGet("tuteur/{idTuteur}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByTuteur(int idTuteur)
        {
            var pointages = await _vuePointageRepository.GetByTuteurAsync(idTuteur);
            return Ok(pointages);
        }

        [HttpGet("tuteur-name/{nomTuteur}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByTuteurName(string nomTuteur)
        {
            var pointages = await _vuePointageRepository.GetByTuteurNameAsync(nomTuteur);
            return Ok(pointages);
        }

        [HttpGet("tuteur-contact/{contact}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByTuteurContact(string contact)
        {
            var pointages = await _vuePointageRepository.GetByTuteurContactAsync(contact);
            return Ok(pointages);
        }

        [HttpGet("tuteur-statut/{statut}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByTuteurStatut(bool statut)
        {
            var pointages = await _vuePointageRepository.GetByTuteurStatutAsync(statut);
            return Ok(pointages);
        }

        // Filtres par horaire/vacation
        [HttpGet("vacation/{IdHoraire}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByVacation(int IdHoraire)
        {
            var pointages = await _vuePointageRepository.GetByVacationAsync(IdHoraire);
            return Ok(pointages);
        }

        [HttpGet("vacation-name/{nomVacation}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByVacationName(string nomVacation)
        {
            var pointages = await _vuePointageRepository.GetByVacationNameAsync(nomVacation);
            return Ok(pointages);
        }

        // Filtres par pr�sence
        [HttpGet("presence-statut/{statut}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByPresenceStatut(string statut)
        {
            var pointages = await _vuePointageRepository.GetByPresenceStatutAsync(statut);
            return Ok(pointages);
        }

        [HttpGet("date-du-jour/{dateDuJour:datetime}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByDateDuJour(DateTime dateDuJour)
        {
            var pointages = await _vuePointageRepository.GetByDateDuJourAsync(dateDuJour);
            return Ok(pointages);
        }

        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByDateRange(
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var pointages = await _vuePointageRepository.GetByDateRangeAsync(dateDebut, dateFin);
            return Ok(pointages);
        }

        [HttpGet("heure-arrivee-range")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByHeureArriveeRange(
            [FromQuery] TimeSpan heureDebut,
            [FromQuery] TimeSpan heureFin)
        {
            var pointages = await _vuePointageRepository.GetByHeureArriveeRangeAsync(heureDebut, heureFin);
            return Ok(pointages);
        }

        [HttpGet("heure-depart-range")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByHeureDepartRange(
            [FromQuery] TimeSpan heureDebut,
            [FromQuery] TimeSpan heureFin)
        {
            var pointages = await _vuePointageRepository.GetByHeureDepartRangeAsync(heureDebut, heureFin);
            return Ok(pointages);
        }

        [HttpGet("retard")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByRetard(
            [FromQuery] TimeSpan heureLimite)
        {
            var pointages = await _vuePointageRepository.GetByRetardAsync(heureLimite);
            return Ok(pointages);
        }

        [HttpGet("absence")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByAbsence()
        {
            var pointages = await _vuePointageRepository.GetByAbsenceAsync();
            return Ok(pointages);
        }

        [HttpGet("presence")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByPresence()
        {
            var pointages = await _vuePointageRepository.GetByPresenceAsync();
            return Ok(pointages);
        }

        // Filtres par localisation
        [HttpGet("province/{province}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByProvince(string province)
        {
            var pointages = await _vuePointageRepository.GetByProvinceAsync(province);
            return Ok(pointages);
        }

        [HttpGet("ville/{ville}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByVille(string ville)
        {
            var pointages = await _vuePointageRepository.GetByVilleAsync(ville);
            return Ok(pointages);
        }

        [HttpGet("commune/{commune}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByCommune(string commune)
        {
            var pointages = await _vuePointageRepository.GetByCommuneAsync(commune);
            return Ok(pointages);
        }

        [HttpGet("localisation-range")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetByLocalisationRange(
            [FromQuery] string latMin,
            [FromQuery] string latMax,
            [FromQuery] string longMin,
            [FromQuery] string longMax)
        {
            var pointages = await _vuePointageRepository.GetByLocalisationRangeAsync(latMin, latMax, longMin, longMax);
            return Ok(pointages);
        }

        // Recherche g�n�rale
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> Search([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest("Le terme de recherche ne peut pas �tre vide");
            }

            var pointages = await _vuePointageRepository.SearchAsync(term);
            return Ok(pointages);
        }

        // Comptages
        [HttpGet("count/ecole/{idEcole}")]
        public async Task<ActionResult<int>> GetCountByEcole(int idEcole)
        {
            var count = await _vuePointageRepository.GetCountByEcoleAsync(idEcole);
            return Ok(count);
        }

        [HttpGet("count/classe/{idClasse}")]
        public async Task<ActionResult<int>> GetCountByClasse(int idClasse)
        {
            var count = await _vuePointageRepository.GetCountByClasseAsync(idClasse);
            return Ok(count);
        }

        [HttpGet("count/section/{idSection}")]
        public async Task<ActionResult<int>> GetCountBySection(int idSection)
        {
            var count = await _vuePointageRepository.GetCountBySectionAsync(idSection);
            return Ok(count);
        }

        [HttpGet("count/direction/{idDirection}")]
        public async Task<ActionResult<int>> GetCountByDirection(int idDirection)
        {
            var count = await _vuePointageRepository.GetCountByDirectionAsync(idDirection);
            return Ok(count);
        }

        [HttpGet("count/tuteur/{idTuteur}")]
        public async Task<ActionResult<int>> GetCountByTuteur(int idTuteur)
        {
            var count = await _vuePointageRepository.GetCountByTuteurAsync(idTuteur);
            return Ok(count);
        }

        [HttpGet("count/vacation/{IdHoraire}")]
        public async Task<ActionResult<int>> GetCountByVacation(int IdHoraire)
        {
            var count = await _vuePointageRepository.GetCountByVacationAsync(IdHoraire);
            return Ok(count);
        }

        [HttpGet("count/presence-statut/{statut}")]
        public async Task<ActionResult<int>> GetCountByPresenceStatut(string statut)
        {
            var count = await _vuePointageRepository.GetCountByPresenceStatutAsync(statut);
            return Ok(count);
        }

        [HttpGet("count/date-range")]
        public async Task<ActionResult<int>> GetCountByDateRange(
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var count = await _vuePointageRepository.GetCountByDateRangeAsync(dateDebut, dateFin);
            return Ok(count);
        }

        [HttpGet("count/date-du-jour/{dateDuJour:datetime}")]
        public async Task<ActionResult<int>> GetCountByDateDuJour(DateTime dateDuJour)
        {
            var count = await _vuePointageRepository.GetCountByDateDuJourAsync(dateDuJour);
            return Ok(count);
        }

        // Statistiques de pr�sence
        [HttpGet("stats/count-presence/ecole/{idEcole}")]
        public async Task<ActionResult<int>> GetCountPresenceByEcole(
            int idEcole,
            [FromQuery] DateTime dateDuJour)
        {
            var count = await _vuePointageRepository.GetCountPresenceByEcoleAsync(idEcole, dateDuJour);
            return Ok(count);
        }

        [HttpGet("stats/count-absence/ecole/{idEcole}")]
        public async Task<ActionResult<int>> GetCountAbsenceByEcole(
            int idEcole,
            [FromQuery] DateTime dateDuJour)
        {
            var count = await _vuePointageRepository.GetCountAbsenceByEcoleAsync(idEcole, dateDuJour);
            return Ok(count);
        }

        [HttpGet("stats/count-retard/ecole/{idEcole}")]
        public async Task<ActionResult<int>> GetCountRetardByEcole(
            int idEcole,
            [FromQuery] DateTime dateDuJour)
        {
            var count = await _vuePointageRepository.GetCountRetardByEcoleAsync(idEcole, dateDuJour);
            return Ok(count);
        }

        [HttpGet("stats/taux-presence/ecole/{idEcole}")]
        public async Task<ActionResult<double>> GetTauxPresenceByEcole(
            int idEcole,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var taux = await _vuePointageRepository.GetTauxPresenceByEcoleAsync(idEcole, dateDebut, dateFin);
            return Ok(taux);
        }

        [HttpGet("stats/taux-presence/classe/{idClasse}")]
        public async Task<ActionResult<double>> GetTauxPresenceByClasse(
            int idClasse,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var taux = await _vuePointageRepository.GetTauxPresenceByClasseAsync(idClasse, dateDebut, dateFin);
            return Ok(taux);
        }

        [HttpGet("stats/taux-presence/eleve/{idEleve}")]
        public async Task<ActionResult<double>> GetTauxPresenceByEleve(
            int idEleve,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var taux = await _vuePointageRepository.GetTauxPresenceByEleveAsync(idEleve, dateDebut, dateFin);
            return Ok(taux);
        }

        // Statistiques temporelles
        [HttpGet("stats/moyenne-heure-arrivee/ecole/{idEcole}")]
        public async Task<ActionResult<TimeSpan>> GetMoyenneHeureArriveeByEcole(
            int idEcole,
            [FromQuery] DateTime dateDuJour)
        {
            var moyenne = await _vuePointageRepository.GetMoyenneHeureArriveeByEcoleAsync(idEcole, dateDuJour);
            return Ok(moyenne);
        }

        [HttpGet("stats/moyenne-heure-depart/ecole/{idEcole}")]
        public async Task<ActionResult<TimeSpan>> GetMoyenneHeureDepartByEcole(
            int idEcole,
            [FromQuery] DateTime dateDuJour)
        {
            var moyenne = await _vuePointageRepository.GetMoyenneHeureDepartByEcoleAsync(idEcole, dateDuJour);
            return Ok(moyenne);
        }

        [HttpGet("stats/moyenne-heure-arrivee/classe/{idClasse}")]
        public async Task<ActionResult<TimeSpan>> GetMoyenneHeureArriveeByClasse(
            int idClasse,
            [FromQuery] DateTime dateDuJour)
        {
            var moyenne = await _vuePointageRepository.GetMoyenneHeureArriveeByClasseAsync(idClasse, dateDuJour);
            return Ok(moyenne);
        }

        [HttpGet("stats/moyenne-heure-depart/classe/{idClasse}")]
        public async Task<ActionResult<TimeSpan>> GetMoyenneHeureDepartByClasse(
            int idClasse,
            [FromQuery] DateTime dateDuJour)
        {
            var moyenne = await _vuePointageRepository.GetMoyenneHeureDepartByClasseAsync(idClasse, dateDuJour);
            return Ok(moyenne);
        }

        // === NOUVEAUX ENDPOINTS POUR LE SUIVI PARENTAL ===

        // Suivi d�taill� par parent
        [HttpGet("parent/suivi/{contactParent}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetSuiviEnfantByParent(string contactParent)
        {
            var suivi = await _vuePointageRepository.GetSuiviEnfantByParentAsync(contactParent);
            return Ok(suivi);
        }

        [HttpGet("parent/suivi/{contactParent}/date/{date:datetime}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetSuiviEnfantByParentAndDate(string contactParent, DateTime date)
        {
            var suivi = await _vuePointageRepository.GetSuiviEnfantByParentAndDateAsync(contactParent, date);
            return Ok(suivi);
        }

        [HttpGet("parent/suivi/{contactParent}/periode")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetSuiviEnfantByParentAndDateRange(
            string contactParent,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var suivi = await _vuePointageRepository.GetSuiviEnfantByParentAndDateRangeAsync(contactParent, dateDebut, dateFin);
            return Ok(suivi);
        }

        [HttpGet("parent/retards/{contactParent}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetRetardsEnfantByParent(
            string contactParent,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var retards = await _vuePointageRepository.GetRetardsEnfantByParentAsync(contactParent, dateDebut, dateFin);
            return Ok(retards);
        }

        [HttpGet("parent/absences/{contactParent}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetAbsencesEnfantByParent(
            string contactParent,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var absences = await _vuePointageRepository.GetAbsencesEnfantByParentAsync(contactParent, dateDebut, dateFin);
            return Ok(absences);
        }

        [HttpGet("parent/presences/{contactParent}")]
        public async Task<ActionResult<IEnumerable<VuePointagePresenceParEcoleDTO>>> GetPresencesEnfantByParent(
            string contactParent,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var presences = await _vuePointageRepository.GetPresencesEnfantByParentAsync(contactParent, dateDebut, dateFin);
            return Ok(presences);
        }

        // Statistiques hebdomadaires
        [HttpGet("parent/stats/hebdomadaire/{contactParent}")]
        public async Task<ActionResult<object>> GetStatistiquesHebdomadairesEnfant(
            string contactParent,
            [FromQuery] DateTime dateDebutSemaine)
        {
            var stats = await _vuePointageRepository.GetStatistiquesHebdomadairesEnfantAsync(contactParent, dateDebutSemaine);
            return Ok(stats);
        }

        [HttpGet("parent/stats/hebdomadaire/{contactParent}/semaine/{semaine}/annee/{annee}")]
        public async Task<ActionResult<object>> GetStatistiquesHebdomadairesEnfant(
            string contactParent,
            int semaine,
            int annee)
        {
            var stats = await _vuePointageRepository.GetStatistiquesHebdomadairesEnfantAsync(contactParent, semaine, annee);
            return Ok(stats);
        }

        // Statistiques mensuelles
        [HttpGet("parent/stats/mensuel/{contactParent}/mois/{mois}/annee/{annee}")]
        public async Task<ActionResult<object>> GetStatistiquesMensuellesEnfant(
            string contactParent,
            int mois,
            int annee)
        {
            var stats = await _vuePointageRepository.GetStatistiquesMensuellesEnfantAsync(contactParent, mois, annee);
            return Ok(stats);
        }

        [HttpGet("parent/stats/mensuel/{contactParent}/date/{date:datetime}")]
        public async Task<ActionResult<object>> GetStatistiquesMensuellesEnfant(
            string contactParent,
            DateTime date)
        {
            var stats = await _vuePointageRepository.GetStatistiquesMensuellesEnfantAsync(contactParent, date);
            return Ok(stats);
        }

        // Statistiques trimestrielles
        [HttpGet("parent/stats/trimestriel/{contactParent}/trimestre/{trimestre}/annee/{annee}")]
        public async Task<ActionResult<object>> GetStatistiquesTrimestriellesEnfant(
            string contactParent,
            int trimestre,
            int annee)
        {
            var stats = await _vuePointageRepository.GetStatistiquesTrimestriellesEnfantAsync(contactParent, trimestre, annee);
            return Ok(stats);
        }

        [HttpGet("parent/stats/trimestriel/{contactParent}/periode")]
        public async Task<ActionResult<object>> GetStatistiquesTrimestriellesEnfant(
            string contactParent,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var stats = await _vuePointageRepository.GetStatistiquesTrimestriellesEnfantAsync(contactParent, dateDebut, dateFin);
            return Ok(stats);
        }

        // Statistiques annuelles
        [HttpGet("parent/stats/annuel/{contactParent}/annee/{annee}")]
        public async Task<ActionResult<object>> GetStatistiquesAnnuellesEnfant(
            string contactParent,
            int annee)
        {
            var stats = await _vuePointageRepository.GetStatistiquesAnnuellesEnfantAsync(contactParent, annee);
            return Ok(stats);
        }

        // Moyennes et tendances
        [HttpGet("parent/moyenne/presence/{contactParent}")]
        public async Task<ActionResult<object>> GetMoyennePresenceEnfant(
            string contactParent,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var moyenne = await _vuePointageRepository.GetMoyennePresenceEnfantAsync(contactParent, dateDebut, dateFin);
            return Ok(moyenne);
        }

        [HttpGet("parent/moyenne/heures-arrivee/{contactParent}")]
        public async Task<ActionResult<object>> GetMoyenneHeuresArriveeEnfant(
            string contactParent,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var moyenne = await _vuePointageRepository.GetMoyenneHeuresArriveeEnfantAsync(contactParent, dateDebut, dateFin);
            return Ok(moyenne);
        }

        [HttpGet("parent/moyenne/heures-depart/{contactParent}")]
        public async Task<ActionResult<object>> GetMoyenneHeuresDepartEnfant(
            string contactParent,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var moyenne = await _vuePointageRepository.GetMoyenneHeuresDepartEnfantAsync(contactParent, dateDebut, dateFin);
            return Ok(moyenne);
        }

        [HttpGet("parent/tendance/{contactParent}")]
        public async Task<ActionResult<object>> GetTendancePresenceEnfant(
            string contactParent,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var tendance = await _vuePointageRepository.GetTendancePresenceEnfantAsync(contactParent, dateDebut, dateFin);
            return Ok(tendance);
        }

        // Alertes et notifications
        [HttpGet("parent/alertes/retards/{contactParent}")]
        public async Task<ActionResult<object>> GetAlertesRetardEnfant(
            string contactParent,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var alertes = await _vuePointageRepository.GetAlertesRetardEnfantAsync(contactParent, dateDebut, dateFin);
            return Ok(alertes);
        }

        [HttpGet("parent/alertes/absences/{contactParent}")]
        public async Task<ActionResult<object>> GetAlertesAbsenceEnfant(
            string contactParent,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var alertes = await _vuePointageRepository.GetAlertesAbsenceEnfantAsync(contactParent, dateDebut, dateFin);
            return Ok(alertes);
        }

        [HttpGet("parent/resume-jour/{contactParent}/date/{date:datetime}")]
        public async Task<ActionResult<object>> GetResumeJourEnfant(
            string contactParent,
            DateTime date)
        {
            var resume = await _vuePointageRepository.GetResumeJourEnfantAsync(contactParent, date);
            return Ok(resume);
        }

        // Comparaisons
        [HttpGet("parent/comparaison/{contactParent}")]
        public async Task<ActionResult<object>> GetComparaisonPresenceEnfant(
            string contactParent,
            [FromQuery] DateTime periode1Debut,
            [FromQuery] DateTime periode1Fin,
            [FromQuery] DateTime periode2Debut,
            [FromQuery] DateTime periode2Fin)
        {
            var comparaison = await _vuePointageRepository.GetComparaisonPresenceEnfantAsync(contactParent, periode1Debut, periode1Fin, periode2Debut, periode2Fin);
            return Ok(comparaison);
        }

        [HttpGet("parent/comparaison-classe/{contactParent}")]
        public async Task<ActionResult<object>> GetComparaisonAvecClasseEnfant(
            string contactParent,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var comparaison = await _vuePointageRepository.GetComparaisonAvecClasseEnfantAsync(contactParent, dateDebut, dateFin);
            return Ok(comparaison);
        }

        // Rapports d�taill�s
        [HttpGet("parent/rapport/complet/{contactParent}")]
        public async Task<ActionResult<object>> GetRapportCompletEnfant(
            string contactParent,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var rapport = await _vuePointageRepository.GetRapportCompletEnfantAsync(contactParent, dateDebut, dateFin);
            return Ok(rapport);
        }

        [HttpGet("parent/rapport/mensuel/{contactParent}/mois/{mois}/annee/{annee}")]
        public async Task<ActionResult<object>> GetRapportMensuelEnfant(
            string contactParent,
            int mois,
            int annee)
        {
            var rapport = await _vuePointageRepository.GetRapportMensuelEnfantAsync(contactParent, mois, annee);
            return Ok(rapport);
        }

        // Rapports détaillés
        [HttpGet("parent/rapport/trimestriel/{contactParent}/trimestre/{trimestre}/annee/{annee}")]
        public async Task<ActionResult<object>> GetRapportTrimestrielEnfant(
            string contactParent,
            int trimestre,
            int annee)
        {
            var rapport = await _vuePointageRepository.GetRapportTrimestrielEnfantAsync(contactParent, trimestre, annee);
            return Ok(rapport);
        }
    }
}
