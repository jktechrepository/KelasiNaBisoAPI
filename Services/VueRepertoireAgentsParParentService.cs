using KelasiNaBiso.Data;
using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public class VueRepertoireAgentsParParentService : IVueRepertoireAgentsParParentRepository
    {
        private readonly KelasiNaBisoDbContext _context;

        public VueRepertoireAgentsParParentService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetAllAsync()
        {
            return await _context.VueRepertoireAgentsParParent
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomCours)
                .ToListAsync();
        }

        public async Task<VueRepertoireAgentsParParentDTO?> GetByIdAsync(int idAgent)
        {
            return await _context.VueRepertoireAgentsParParent
                .FirstOrDefaultAsync(r => r.IdAgent == idAgent);
        }

        // Filtres par parent/tuteur
        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByParentContactAsync(string contactParent)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.TelephoneTuteur == contactParent ||
                           r.EmailTuteur == contactParent ||
                           r.TelephoneRepresentant == contactParent)
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomCours)
                .ToListAsync();
        }

        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByParentNameAsync(string nomParent)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.NomCompletTuteur.Contains(nomParent) ||
                           r.NomCompletRepresentant.Contains(nomParent))
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomCours)
                .ToListAsync();
        }

        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByParentIdAsync(int idTuteur)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.IdTuteur == idTuteur)
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomCours)
                .ToListAsync();
        }

        // Filtres par élève
        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByEleveAsync(int idEleve)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.IdEleve == idEleve)
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomCours)
                .ToListAsync();
        }

        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByEleveNameAsync(string nomEleve)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.NomCompletEleve.Contains(nomEleve))
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomCours)
                .ToListAsync();
        }

        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByEleveMatriculeAsync(string matricule)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.Matricule.Contains(matricule))
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomCours)
                .ToListAsync();
        }

        // Filtres par agent
        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByAgentAsync(int idAgent)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.IdAgent == idAgent)
                .OrderBy(r => r.NomCours)
                .ThenBy(r => r.NomClasse)
                .ToListAsync();
        }

        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByAgentNameAsync(string nomAgent)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.NomCompletAgent.Contains(nomAgent))
                .OrderBy(r => r.NomCours)
                .ThenBy(r => r.NomClasse)
                .ToListAsync();
        }

        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByAgentContactAsync(string contactAgent)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.TelephoneAgent == contactAgent ||
                           r.EmailAgent == contactAgent)
                .OrderBy(r => r.NomCours)
                .ThenBy(r => r.NomClasse)
                .ToListAsync();
        }

        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByAgentGenreAsync(string genre)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.GenreAgent == genre)
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomCours)
                .ToListAsync();
        }

        // Filtres par cours
        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByCoursAsync(int idCours)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.IdCours == idCours)
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomClasse)
                .ToListAsync();
        }

        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByCoursNameAsync(string nomCours)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.NomCours.Contains(nomCours))
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomClasse)
                .ToListAsync();
        }

        // Filtres par classe
        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByClasseAsync(int idClasse)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.IdClasse == idClasse)
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomCours)
                .ToListAsync();
        }

        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByClasseNameAsync(string nomClasse)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.NomClasse.Contains(nomClasse))
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomCours)
                .ToListAsync();
        }

        // Filtres par école
        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByEcoleAsync(int idEcole)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.IdEcole == idEcole)
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomCours)
                .ToListAsync();
        }

        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByEcoleNameAsync(string nomEcole)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.NomEcole.Contains(nomEcole))
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomCours)
                .ToListAsync();
        }

        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByTypeEcoleAsync(string typeEcole)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.TypeEcole == typeEcole)
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomCours)
                .ToListAsync();
        }

        // Filtres par année scolaire
        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByAnneeScolaireAsync(int idAnneeScolaire)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.IdAnneeScolaire == idAnneeScolaire)
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomCours)
                .ToListAsync();
        }

        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> GetByAnneeScolaireLibelleAsync(string libelleAnnee)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.LibelleAnneeScolaire.Contains(libelleAnnee))
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomCours)
                .ToListAsync();
        }

        // Recherche générale
        public async Task<IEnumerable<VueRepertoireAgentsParParentDTO>> SearchAsync(string searchTerm)
        {
            return await _context.VueRepertoireAgentsParParent
                .Where(r => r.NomCompletAgent.Contains(searchTerm) ||
                           r.NomCours.Contains(searchTerm) ||
                           r.NomClasse.Contains(searchTerm) ||
                           r.NomCompletEleve.Contains(searchTerm) ||
                           r.NomCompletTuteur.Contains(searchTerm) ||
                           r.NomEcole.Contains(searchTerm))
                .OrderBy(r => r.NomCompletAgent)
                .ThenBy(r => r.NomCours)
                .ToListAsync();
        }

        // Comptages
        public async Task<int> GetCountByParentAsync(int idTuteur)
        {
            return await _context.VueRepertoireAgentsParParent
                .CountAsync(r => r.IdTuteur == idTuteur);
        }

        public async Task<int> GetCountByAgentAsync(int idAgent)
        {
            return await _context.VueRepertoireAgentsParParent
                .CountAsync(r => r.IdAgent == idAgent);
        }

        public async Task<int> GetCountByCoursAsync(int idCours)
        {
            return await _context.VueRepertoireAgentsParParent
                .CountAsync(r => r.IdCours == idCours);
        }

        public async Task<int> GetCountByClasseAsync(int idClasse)
        {
            return await _context.VueRepertoireAgentsParParent
                .CountAsync(r => r.IdClasse == idClasse);
        }

        public async Task<int> GetCountByEcoleAsync(int idEcole)
        {
            return await _context.VueRepertoireAgentsParParent
                .CountAsync(r => r.IdEcole == idEcole);
        }

        public async Task<int> GetCountByAnneeScolaireAsync(int idAnneeScolaire)
        {
            return await _context.VueRepertoireAgentsParParent
                .CountAsync(r => r.IdAnneeScolaire == idAnneeScolaire);
        }

        // Statistiques
        public async Task<object> GetStatistiquesAgentsByParentAsync(string contactParent)
        {
            var repertoire = await GetByParentContactAsync(contactParent);

            if (!repertoire.Any()) return new { Message = "Aucun agent trouvé pour ce parent" };

            var agents = repertoire
                .GroupBy(r => new { r.IdAgent, r.NomCompletAgent, r.GenreAgent })
                .Select(g => new
                {
                    IdAgent = g.Key.IdAgent,
                    NomComplet = g.Key.NomCompletAgent,
                    Genre = g.Key.GenreAgent,
                    NombreCours = g.Count(),
                    Cours = g.Select(r => r.NomCours).Distinct().ToList(),
                    Classes = g.Select(r => r.NomClasse).Distinct().ToList()
                })
                .OrderBy(x => x.NomComplet)
                .ToList();

            return new
            {
                ContactParent = contactParent,
                NombreAgents = agents.Count,
                Agents = agents
            };
        }

        public async Task<object> GetStatistiquesCoursByParentAsync(string contactParent)
        {
            var repertoire = await GetByParentContactAsync(contactParent);

            if (!repertoire.Any()) return new { Message = "Aucun cours trouvé pour ce parent" };

            var cours = repertoire
                .GroupBy(r => new { r.IdCours, r.NomCours, r.DescriptionCours })
                .Select(g => new
                {
                    IdCours = g.Key.IdCours,
                    NomCours = g.Key.NomCours,
                    Description = g.Key.DescriptionCours,
                    NombreAgents = g.Select(r => r.IdAgent).Distinct().Count(),
                    Agents = g.Select(r => r.NomCompletAgent).Distinct().ToList(),
                    Classes = g.Select(r => r.NomClasse).Distinct().ToList()
                })
                .OrderBy(x => x.NomCours)
                .ToList();

            return new
            {
                ContactParent = contactParent,
                NombreCours = cours.Count,
                Cours = cours
            };
        }

        public async Task<object> GetRepertoireCompletByParentAsync(string contactParent)
        {
            var repertoire = await GetByParentContactAsync(contactParent);

            if (!repertoire.Any()) return new { Message = "Aucun répertoire trouvé pour ce parent" };

            var parent = repertoire.First();
            var statsAgents = await GetStatistiquesAgentsByParentAsync(contactParent);
            var statsCours = await GetStatistiquesCoursByParentAsync(contactParent);

            return new
            {
                InformationsParent = new
                {
                    NomComplet = parent.NomCompletTuteur,
                    Telephone = parent.TelephoneTuteur,
                    Email = parent.EmailTuteur,
                    Representant = parent.NomCompletRepresentant,
                    TelephoneRepresentant = parent.TelephoneRepresentant
                },
                InformationsEleve = new
                {
                    NomComplet = parent.NomCompletEleve,
                    Matricule = parent.Matricule,
                    Classe = parent.NomClasse,
                    Ecole = parent.NomEcole
                },
                StatistiquesAgents = statsAgents,
                StatistiquesCours = statsCours,
                RepertoireComplet = repertoire.Select(r => new
                {
                    Agent = r.NomCompletAgent,
                    TelephoneAgent = r.TelephoneAgent,
                    EmailAgent = r.EmailAgent,
                    Cours = r.NomCours,
                    Classe = r.NomClasse,
                    DescriptionCours = r.DescriptionCours
                })
            };
        }
    }
}

