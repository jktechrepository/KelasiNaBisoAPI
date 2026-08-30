namespace KelasiNaBiso.Services.Reporting
{
    public interface IBulletinReportService
    {
        Task<byte[]> GetElevePdfAsync(
            int idEleve,
            int idAnneeScolaire,
            string periode,
            CancellationToken cancellationToken = default);
    }
}
