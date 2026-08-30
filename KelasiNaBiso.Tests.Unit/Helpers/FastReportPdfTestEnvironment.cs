using FastReport;

namespace KelasiNaBiso.Tests.Unit.Helpers
{
    /// <summary>
    /// FastReport PDF nécessite GDI+ (Windows ou libgdiplus sur Linux).
    /// </summary>
    internal static class FastReportPdfTestEnvironment
    {
        private static readonly Lazy<bool> IsAvailableLazy = new(() =>
        {
            try
            {
                using var report = new Report();
                return true;
            }
            catch
            {
                return false;
            }
        });

        public static bool IsAvailable => IsAvailableLazy.Value;
    }
}
