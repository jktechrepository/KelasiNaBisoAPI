namespace KelasiNaBiso.Services.Reporting
{
    public interface IBulletinReportService
    {
        Task<byte[]> GetElevePdfAsync(
            int idEleve,
            int idAnneeScolaire,
            string? periode = null,
            int? idPeriode = null,
            CancellationToken cancellationToken = default);
    }
}
