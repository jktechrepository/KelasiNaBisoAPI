using KelasiNaBiso.Models.DTOs.Reporting;

namespace KelasiNaBiso.Services.Reporting
{
    public interface IFeuilleAppelExcelExporter
    {
        byte[] Export(FeuilleAppelClasseDto feuille);
        string GetFileName(FeuilleAppelClasseDto feuille);
        string ContentType { get; }
    }
}
