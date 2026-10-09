using KelasiNaBiso.Models.DTOs;
using KelasiNaBiso.Models.DTOs.Pagination;

namespace KelasiNaBiso.Services.Repositories;

public interface IHoraireDeCoursRepository
{
    Task<IReadOnlyList<HoraireDeCoursResponse>> GetAsync(
        int? idClasse,
        int? idAgent,
        int? idAnneeScolaire,
        int? idEcole,
        int? idTuteur,
        int? idAgentAffecte,
        CancellationToken cancellationToken);
    Task<PagedResult<HoraireDeCoursResponse>> GetPagedAsync(
        int? idClasse,
        int? idAgent,
        int? idAnneeScolaire,
        int? idEcole,
        int? idTuteur,
        int? idAgentAffecte,
        PagedRequest request,
        CancellationToken cancellationToken);

    Task<HoraireDeCoursResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        int? idEcole = null,
        int? idTuteur = null,
        int? idAgentAffecte = null);
    Task<HoraireDeCoursResponse> CreateAsync(CreateHoraireDeCoursRequest request, CancellationToken cancellationToken);
    Task<HoraireDeCoursResponse?> UpdateAsync(int id, UpdateHoraireDeCoursRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
