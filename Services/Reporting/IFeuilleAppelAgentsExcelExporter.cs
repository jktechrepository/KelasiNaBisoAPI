using KelasiNaBiso.Models.DTOs.Reporting;

namespace KelasiNaBiso.Services.Reporting
{
    public interface IFeuilleAppelAgentsExcelExporter
    {
        string ContentType { get; }
        byte[] Export(FeuilleAppelAgentsDto feuille);
        string GetFileName(FeuilleAppelAgentsDto feuille);
    }
}
