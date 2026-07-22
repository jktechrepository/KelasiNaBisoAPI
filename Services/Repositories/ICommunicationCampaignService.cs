using KelasiNaBiso.Models.DTOs.Communication;
using KelasiNaBiso.Models.DTOs.Pagination;

namespace KelasiNaBiso.Services.Repositories
{
    public interface ICommunicationCampaignService
    {
        Task<PagedResult<CommunicationCampaignSummaryDto>> GetCampaignsAsync(int currentUserId, string currentUserRole, int? currentUserEcoleId, PagedRequest request, CancellationToken cancellationToken = default);

        Task<CommunicationCampaignDetailDto?> GetCampaignByIdAsync(int idCampaign, int currentUserId, string currentUserRole, int? currentUserEcoleId, CancellationToken cancellationToken = default);

        Task<PagedResult<CommunicationCampaignDetailDto>> GetCampaignsByEcoleAsync(int ecoleId, int currentUserId, string currentUserRole, int? currentUserEcoleId, PagedRequest request, CancellationToken cancellationToken = default);

        Task<CommunicationCampaignDetailDto> CreateCampaignAsync(CreateCommunicationCampaignDto dto, int currentUserId, string currentUserRole, int? currentUserEcoleId, CancellationToken cancellationToken = default);

        Task<CommunicationCampaignDetailDto> UpdateCampaignAsync(int idCampaign, UpdateCommunicationCampaignDto dto, int currentUserId, string currentUserRole, int? currentUserEcoleId, CancellationToken cancellationToken = default);

        Task<bool> CancelCampaignAsync(int idCampaign, CancelCommunicationRequest? request, int currentUserId, string currentUserRole, int? currentUserEcoleId, CancellationToken cancellationToken = default);

        Task<int> RefreshRecipientsAsync(int idCampaign, int currentUserId, string currentUserRole, int? currentUserEcoleId, CancellationToken cancellationToken = default);

        Task<PagedResult<CommunicationRecipientDto>> GetRecipientsAsync(int idCampaign, PagedRequest request, int currentUserId, string currentUserRole, int? currentUserEcoleId, CancellationToken cancellationToken = default);

        Task<PagedResult<CommunicationHistoryDto>> GetHistoryAsync(int idCampaign, PagedRequest request, int currentUserId, string currentUserRole, int? currentUserEcoleId, CancellationToken cancellationToken = default);

        Task<bool> DispatchCampaignAsync(int idCampaign, int currentUserId, string currentUserRole, int? currentUserEcoleId, CancellationToken cancellationToken = default);
    }
}

