using System.Text;
using KelasiNaBiso.Models.DTOs.Reporting;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace KelasiNaBiso.Services.Reporting
{
    public class FeuilleAppelAgentsExcelExporter : IFeuilleAppelAgentsExcelExporter
    {
        public string ContentType =>
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public FeuilleAppelAgentsExcelExporter()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public byte[] Export(FeuilleAppelAgentsDto feuille)
        {
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Feuille d'appel agents");

            ws.Cells[1, 1].Value = "Feuille d'appel agents";
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.Font.Size = 14;

            ws.Cells[2, 1].Value = "École";
            ws.Cells[2, 2].Value = string.IsNullOrWhiteSpace(feuille.NomEcole)
                ? $"#{feuille.IdEcole}"
                : feuille.NomEcole;
            ws.Cells[3, 1].Value = "Fonction (filtre)";
            ws.Cells[3, 2].Value = feuille.FonctionFiltre ?? "(toutes)";
            ws.Cells[4, 1].Value = "Date";
            ws.Cells[4, 2].Value = feuille.Date;
            ws.Cells[4, 2].Style.Numberformat.Format = "yyyy-mm-dd";

            ws.Cells[6, 1].Value = "Effectif";
            ws.Cells[6, 2].Value = feuille.Effectif;
            ws.Cells[7, 1].Value = "Présents";
            ws.Cells[7, 2].Value = feuille.NbPresents;
            ws.Cells[8, 1].Value = "Absents";
            ws.Cells[8, 2].Value = feuille.NbAbsents;
            ws.Cells[9, 1].Value = "Retards";
            ws.Cells[9, 2].Value = feuille.NbRetards;

            for (var r = 2; r <= 9; r++)
                ws.Cells[r, 1].Style.Font.Bold = true;

            const int headerRow = 11;
            var headers = new[]
            {
                "#", "Matricule", "NomComplet", "Fonction", "Genre", "StatutJour",
                "HeureArrivee", "HeureDepart", "Observation"
            };

            for (var i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cells[headerRow, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            var row = headerRow + 1;
            var index = 1;
            foreach (var ligne in feuille.Lignes)
            {
                ws.Cells[row, 1].Value = index++;
                ws.Cells[row, 2].Value = ligne.Matricule;
                ws.Cells[row, 3].Value = ligne.NomComplet;
                ws.Cells[row, 4].Value = ligne.Fonction;
                ws.Cells[row, 5].Value = ligne.Genre;
                ws.Cells[row, 6].Value = ligne.StatutJour;
                ws.Cells[row, 7].Value = FormatTime(ligne.HeureArrivee);
                ws.Cells[row, 8].Value = FormatTime(ligne.HeureDepart);
                ws.Cells[row, 9].Value = ligne.Observation;
                row++;
            }

            if (ws.Dimension != null)
                ws.Cells[ws.Dimension.Address].AutoFitColumns();

            return package.GetAsByteArray();
        }

        public string GetFileName(FeuilleAppelAgentsDto feuille)
        {
            var ecole = SanitizeFilePart(feuille.NomEcole);
            if (string.IsNullOrWhiteSpace(ecole))
                ecole = $"Ecole{feuille.IdEcole}";

            var suffix = string.IsNullOrWhiteSpace(feuille.FonctionFiltre)
                ? string.Empty
                : $"_{SanitizeFilePart(feuille.FonctionFiltre)}";

            return $"FeuilleAppel_Agents_{ecole}{suffix}_{feuille.Date:yyyyMMdd}.xlsx";
        }

        private static string? FormatTime(TimeSpan? time) =>
            time.HasValue ? time.Value.ToString(@"hh\:mm") : null;

        private static string SanitizeFilePart(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(value.Length);
            foreach (var c in value.Trim())
            {
                if (invalid.Contains(c) || c == ' ')
                    sb.Append('_');
                else
                    sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
