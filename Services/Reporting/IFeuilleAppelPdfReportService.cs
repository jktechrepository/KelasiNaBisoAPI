using KelasiNaBiso.Models.DTOs.Reporting;

namespace KelasiNaBiso.Services.Reporting
{
    public interface IFeuilleAppelPdfReportService
    {
        string ContentType { get; }

        byte[] ExportEleves(FeuilleAppelClasseDto feuille);

        byte[] ExportAgents(FeuilleAppelAgentsDto feuille);

        string GetFileNameEleves(FeuilleAppelClasseDto feuille);

        string GetFileNameAgents(FeuilleAppelAgentsDto feuille);
    }
}
