namespace KelasiNaBiso.Services.Reporting
{
    public interface IReportImageResolver
    {
        Task<byte[]?> ResolveAsync(string? url, CancellationToken cancellationToken = default);
    }
}
