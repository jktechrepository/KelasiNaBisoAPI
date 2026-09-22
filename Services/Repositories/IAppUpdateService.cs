using KelasiNaBiso.Models.DTOs;

namespace KelasiNaBiso.Services.Repositories
{
    public interface IAppUpdateService
    {
        Task<AppUpdateCheckResponseDto> CheckPolicyAsync(
            string platform,
            string appVersion,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AppUpdatePolicyDto>> GetAllPoliciesAsync(
            CancellationToken cancellationToken = default);

        Task<AppUpdatePolicyDto> UpsertPolicyAsync(
            string platform,
            UpsertAppUpdatePolicyDto dto,
            int? idAuteur,
            CancellationToken cancellationToken = default);

        Task<NotifyAppUpdateResultDto> NotifyAsync(
            NotifyAppUpdateDto dto,
            CancellationToken cancellationToken = default);
    }
}
