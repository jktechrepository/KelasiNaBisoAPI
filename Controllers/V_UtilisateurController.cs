using KelasiNaBiso.Models;
using KelasiNaBiso.Services.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace KelasiNaBiso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Vue Utilisateurs - Token JWT requis
    public class V_UtilisateurController : ControllerBase
    {
        private readonly IV_UtilisateurRepository _vUtilisateurRepository;

        public V_UtilisateurController(IV_UtilisateurRepository vUtilisateurRepository)
        {
            _vUtilisateurRepository = vUtilisateurRepository;
        }

        // GET: api/V_Utilisateur
        [HttpGet]
        public async Task<ActionResult<IEnumerable<V_Utilisateur>>> GetV_Utilisateurs()
        {
            var vUtilisateurs = await _vUtilisateurRepository.GetAllAsync();
            return Ok(vUtilisateurs);
        }

        // GET: api/V_Utilisateur/5
        [HttpGet("{id}")]
        public async Task<ActionResult<V_Utilisateur>> GetV_Utilisateur(int id)
        {
            var vUtilisateur = await _vUtilisateurRepository.GetByIdAsync(id);
            if (vUtilisateur == null)
            {
                return NotFound();
            }
            return Ok(vUtilisateur);
        }

        // GET: api/V_Utilisateur/reference/{reference}
        [HttpGet("reference/{reference}")]
        public async Task<ActionResult<V_Utilisateur>> GetV_UtilisateurByReference(Guid reference)
        {
            var vUtilisateur = await _vUtilisateurRepository.GetByReferenceAsync(reference);
            if (vUtilisateur == null)
            {
                return NotFound();
            }
            return Ok(vUtilisateur);
        }

        // GET: api/V_Utilisateur/email/{email}
        [HttpGet("email/{email}")]
        public async Task<ActionResult<V_Utilisateur>> GetV_UtilisateurByEmail(string email)
        {
            var vUtilisateur = await _vUtilisateurRepository.GetByEmailAsync(email);
            if (vUtilisateur == null)
            {
                return NotFound();
            }
            return Ok(vUtilisateur);
        }

        // GET: api/V_Utilisateur/ecole/5
        [HttpGet("ecole/{idEcole}")]
        public async Task<ActionResult<IEnumerable<V_Utilisateur>>> GetV_UtilisateursByEcole(int idEcole)
        {
            var vUtilisateurs = await _vUtilisateurRepository.GetByEcoleAsync(idEcole);
            return Ok(vUtilisateurs);
        }

        // GET: api/V_Utilisateur/role/5
        [HttpGet("role/{idRole}")]
        public async Task<ActionResult<IEnumerable<V_Utilisateur>>> GetV_UtilisateursByRole(int idRole)
        {
            var vUtilisateurs = await _vUtilisateurRepository.GetByRoleAsync(idRole);
            return Ok(vUtilisateurs);
        }

        // GET: api/V_Utilisateur/role-name/{nomRole}
        [HttpGet("role-name/{nomRole}")]
        public async Task<ActionResult<IEnumerable<V_Utilisateur>>> GetV_UtilisateursByRoleName(string nomRole)
        {
            var vUtilisateurs = await _vUtilisateurRepository.GetByRoleNameAsync(nomRole);
            return Ok(vUtilisateurs);
        }

        // GET: api/V_Utilisateur/statut/{statut}
        [HttpGet("statut/{statut}")]
        public async Task<ActionResult<IEnumerable<V_Utilisateur>>> GetV_UtilisateursByStatut(bool statut)
        {
            var vUtilisateurs = await _vUtilisateurRepository.GetByStatutAsync(statut);
            return Ok(vUtilisateurs);
        }

        // GET: api/V_Utilisateur/connecte/{isConnecte}
        [HttpGet("connecte/{isConnecte}")]
        public async Task<ActionResult<IEnumerable<V_Utilisateur>>> GetV_UtilisateursByConnecte(bool isConnecte)
        {
            var vUtilisateurs = await _vUtilisateurRepository.GetByConnecteAsync(isConnecte);
            return Ok(vUtilisateurs);
        }

        // GET: api/V_Utilisateur/province/{province}
        [HttpGet("province/{province}")]
        public async Task<ActionResult<IEnumerable<V_Utilisateur>>> GetV_UtilisateursByProvince(string province)
        {
            var vUtilisateurs = await _vUtilisateurRepository.GetByProvinceAsync(province);
            return Ok(vUtilisateurs);
        }

        // GET: api/V_Utilisateur/ville/{ville}
        [HttpGet("ville/{ville}")]
        public async Task<ActionResult<IEnumerable<V_Utilisateur>>> GetV_UtilisateursByVille(string ville)
        {
            var vUtilisateurs = await _vUtilisateurRepository.GetByVilleAsync(ville);
            return Ok(vUtilisateurs);
        }

        // GET: api/V_Utilisateur/commune/{commune}
        [HttpGet("commune/{commune}")]
        public async Task<ActionResult<IEnumerable<V_Utilisateur>>> GetV_UtilisateursByCommune(string commune)
        {
            var vUtilisateurs = await _vUtilisateurRepository.GetByCommuneAsync(commune);
            return Ok(vUtilisateurs);
        }

        // GET: api/V_Utilisateur/date/{date}
        [HttpGet("date/{date}")]
        public async Task<ActionResult<IEnumerable<V_Utilisateur>>> GetV_UtilisateursByDate(DateTime date)
        {
            var vUtilisateurs = await _vUtilisateurRepository.GetByDateCreationAsync(date);
            return Ok(vUtilisateurs);
        }

        // GET: api/V_Utilisateur/date-range
        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<V_Utilisateur>>> GetV_UtilisateursByDateRange(
            [FromQuery] DateTime dateDebut, 
            [FromQuery] DateTime dateFin)
        {
            var vUtilisateurs = await _vUtilisateurRepository.GetByDateCreationRangeAsync(dateDebut, dateFin);
            return Ok(vUtilisateurs);
        }

        // GET: api/V_Utilisateur/nom-complet/{nomComplet}
        [HttpGet("nom-complet/{nomComplet}")]
        public async Task<ActionResult<IEnumerable<V_Utilisateur>>> GetV_UtilisateursByNomComplet(string nomComplet)
        {
            var vUtilisateurs = await _vUtilisateurRepository.GetByNomCompletAsync(nomComplet);
            return Ok(vUtilisateurs);
        }

        // GET: api/V_Utilisateur/telephone/{telephone}
        [HttpGet("telephone/{telephone}")]
        public async Task<ActionResult<IEnumerable<V_Utilisateur>>> GetV_UtilisateursByTelephone(string telephone)
        {
            var vUtilisateurs = await _vUtilisateurRepository.GetByTelephoneAsync(telephone);
            return Ok(vUtilisateurs);
        }

        // GET: api/V_Utilisateur/count
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetV_UtilisateursCount()
        {
            var count = await _vUtilisateurRepository.GetCountAsync();
            return Ok(count);
        }

        // GET: api/V_Utilisateur/count/ecole/5
        [HttpGet("count/ecole/{idEcole}")]
        public async Task<ActionResult<int>> GetV_UtilisateursCountByEcole(int idEcole)
        {
            var count = await _vUtilisateurRepository.GetCountByEcoleAsync(idEcole);
            return Ok(count);
        }

        // GET: api/V_Utilisateur/count/role/5
        [HttpGet("count/role/{idRole}")]
        public async Task<ActionResult<int>> GetV_UtilisateursCountByRole(int idRole)
        {
            var count = await _vUtilisateurRepository.GetCountByRoleAsync(idRole);
            return Ok(count);
        }

        // GET: api/V_Utilisateur/count/statut/{statut}
        [HttpGet("count/statut/{statut}")]
        public async Task<ActionResult<int>> GetV_UtilisateursCountByStatut(bool statut)
        {
            var count = await _vUtilisateurRepository.GetCountByStatutAsync(statut);
            return Ok(count);
        }

        // GET: api/V_Utilisateur/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> V_UtilisateurExists(int id)
        {
            var exists = await _vUtilisateurRepository.ExistsAsync(id);
            return Ok(exists);
        }

        // GET: api/V_Utilisateur/exists/email/{email}
        [HttpGet("exists/email/{email}")]
        public async Task<ActionResult<bool>> V_UtilisateurExistsByEmail(string email)
        {
            var exists = await _vUtilisateurRepository.ExistsByEmailAsync(email);
            return Ok(exists);
        }

        // GET: api/V_Utilisateur/exists/reference/{reference}
        [HttpGet("exists/reference/{reference}")]
        public async Task<ActionResult<bool>> V_UtilisateurExistsByReference(Guid reference)
        {
            var exists = await _vUtilisateurRepository.ExistsByReferenceAsync(reference);
            return Ok(exists);
        }
    }
}
