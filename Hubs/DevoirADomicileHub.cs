using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using KelasiNaBiso.Data;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBisoAPI.Hubs
{
    /// <summary>
    /// Hub SignalR pour les notifications en temps réel des devoirs à domicile
    /// </summary>
    [Authorize]
    public class DevoirADomicileHub : Hub
    {
        private readonly ILogger<DevoirADomicileHub> _logger;
        private readonly KelasiNaBiso.Data.KelasiNaBisoDbContext _context;

        public DevoirADomicileHub(
            ILogger<DevoirADomicileHub> logger,
            KelasiNaBiso.Data.KelasiNaBisoDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Appelé quand un client se connecte
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            var idEcole = Context.User?.FindFirst("IdEcole")?.Value;
            
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                // 1. Ajouter l'utilisateur à son groupe personnel
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                // 2. Ajouter l'utilisateur au groupe de son école si disponible
                if (!string.IsNullOrEmpty(idEcole) && int.TryParse(idEcole, out int ecoleId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"ecole_{ecoleId}");
                }
                
                // 3. Récupérer l'utilisateur depuis la base de données
                var utilisateur = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.IdUtilisateur == userId);
                
                if (utilisateur != null)
                {
                    // 4. Si c'est un parent (tuteur), ajouter aux groupes de ses enfants
                    if (utilisateur.IdTuteur.HasValue)
                    {
                        var classesEnfants = await _context.Eleves
                            .Where(e => e.IdTuteur == utilisateur.IdTuteur.Value && e.Statut == true)
                            .Select(e => e.IdClasse)
                            .Distinct()
                            .ToListAsync();
                        
                        var classesEnfantsList = classesEnfants.ToList();
                        foreach (var idClasse in classesEnfantsList)
                        {
                            // Groupe classe
                            await Groups.AddToGroupAsync(Context.ConnectionId, $"classe_{idClasse}");
                            // Groupe parents de la classe
                            await Groups.AddToGroupAsync(Context.ConnectionId, $"parents_classe_{idClasse}");
                        }
                        
                        if (classesEnfantsList.Count > 0)
                        {
                            _logger.LogInformation($"Parent {userName} (ID: {userId}) ajouté aux groupes de {classesEnfantsList.Count} classe(s)");
                        }
                    }
                    
                    // 5. Si c'est un agent (enseignant), récupérer ses classes via AffectationsCours -> Cours
                    if (utilisateur.IdAgent.HasValue)
                    {
                        var classesEnseignant = await _context.AffectationsCours
                            .Where(ac => ac.IdAgent == utilisateur.IdAgent.Value)
                            .Join(_context.Cours,
                                ac => ac.IdCours,
                                cours => cours.IdCours,
                                (ac, cours) => cours.IdClasse)
                            .Distinct()
                            .ToListAsync();
                        
                        var classesEnseignantList = classesEnseignant.ToList();
                        foreach (var idClasse in classesEnseignantList)
                        {
                            await Groups.AddToGroupAsync(Context.ConnectionId, $"classe_{idClasse}");
                        }
                        
                        if (classesEnseignantList.Count > 0)
                        {
                            _logger.LogInformation($"Enseignant {userName} (ID: {userId}) ajouté aux groupes de {classesEnseignantList.Count} classe(s)");
                        }
                    }
                }
                
                // 6. Ajouter au groupe général
                await Groups.AddToGroupAsync(Context.ConnectionId, "all_users");
                
                _logger.LogInformation($"User {userName} (ID: {userId}) connected to DevoirADomicileHub. ConnectionId: {Context.ConnectionId}");
            }
            
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Appelé quand un client se déconnecte
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation($"User {userName} (ID: {userId}) disconnected from DevoirADomicileHub. ConnectionId: {Context.ConnectionId}");
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Permet à un utilisateur de rejoindre le groupe d'une classe spécifique
        /// Utile pour les enseignants qui veulent suivre plusieurs classes
        /// </summary>
        public async Task JoinClasseGroup(int idClasse)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"classe_{idClasse}");
            _logger.LogInformation($"User {Context.User?.Identity?.Name} joined classe group: classe_{idClasse}");
            
            // Confirmer l'ajout au groupe
            await Clients.Caller.SendAsync("JoinedClasseGroup", idClasse);
        }

        /// <summary>
        /// Permet à un utilisateur de quitter le groupe d'une classe
        /// </summary>
        public async Task LeaveClasseGroup(int idClasse)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"classe_{idClasse}");
            _logger.LogInformation($"User {Context.User?.Identity?.Name} left classe group: classe_{idClasse}");
            
            // Confirmer la sortie du groupe
            await Clients.Caller.SendAsync("LeftClasseGroup", idClasse);
        }

        /// <summary>
        /// Obtenir le statut de connexion
        /// </summary>
        public async Task GetConnectionStatus()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            var idEcole = Context.User?.FindFirst("IdEcole")?.Value;
            var idClasse = Context.User?.FindFirst("IdClasse")?.Value;
            
            await Clients.Caller.SendAsync("ConnectionStatus", new
            {
                IsConnected = true,
                UserId = userId,
                UserName = userName,
                IdEcole = idEcole,
                IdClasse = idClasse,
                ConnectionId = Context.ConnectionId,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

