using KelasiNaBiso.Data;
using KelasiNaBiso.Models.DTOs.Vitrine;
using KelasiNaBiso.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBiso.Services
{
    public interface IVitrineService
    {
        Task<VitrineStatistiquesDto> GetStatistiquesAsync(CancellationToken cancellationToken = default);
    }

    public class VitrineService : IVitrineService
    {
        private readonly KelasiNaBisoDbContext _context;

        public VitrineService(KelasiNaBisoDbContext context)
        {
            _context = context;
        }

        public async Task<VitrineStatistiquesDto> GetStatistiquesAsync(CancellationToken cancellationToken = default)
        {
            var nombreEcoles = await _context.Ecoles
                .AsNoTracking()
                .Where(e => e.Statut == true)
                .CountAsync(cancellationToken);

            var nombreEleves = await _context.Eleves
                .AsNoTracking()
                .Where(e => e.Statut == true
                    && e.Inscriptions.Any(i => i.Statut == true
                        && (i.StatutInscription == "Confirmé" || i.StatutInscription == "Confirme" || i.StatutInscription.StartsWith("Confirm"))))
                .CountAsync(cancellationToken);

            var nombrePersonnel = await _context.Agents
                .AsNoTracking()
                .Where(a => a.Statut == true)
                .CountAsync(cancellationToken);

            var nombreParentsConnectes = await _context.Utilisateurs
                .AsNoTracking()
                .Where(u => u.Statut == true)
                .Where(u =>
                    u.IdTuteur != null ||
                    u.UserRoles.Any(ur =>
                        ur.Statut == true &&
                        ur.Role.Nom == UserRoles.PARENT))
                .CountAsync(cancellationToken);

            return new VitrineStatistiquesDto
            {
                NombreEcoles = nombreEcoles,
                NombreEleves = nombreEleves,
                NombrePersonnel = nombrePersonnel,
                NombreParentsConnectes = nombreParentsConnectes,
                DateMiseAJour = DateTime.UtcNow
            };
        }
    }
}
