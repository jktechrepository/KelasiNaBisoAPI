using FastReport;
using FastReport.Utils;
using KelasiNaBiso.Models.DTOs.Reporting;
using System.Globalization;
using DataBand = FastReport.DataBand;
using ReportPage = FastReport.ReportPage;
using ReportSummaryBand = FastReport.ReportSummaryBand;
using ReportTitleBand = FastReport.ReportTitleBand;
using TextObject = FastReport.TextObject;

namespace KelasiNaBiso.Services.Reporting
{
    /// <summary>
    /// Mise en page A4 si BulletinEleve.frx est absent ou invalide.
    /// </summary>
    internal static class BulletinReportLayoutBuilder
    {
        private const float PageWidthMm = 210f;
        private const float PageHeightMm = 297f;
        private const float MarginMm = 15f;

        public static void ApplyLayout(
            Report report,
            BulletinEnteteReportDto entete,
            List<BulletinLigneReportDto> lignes)
        {
            report.Clear();
            report.ScriptLanguage = Language.CSharp;

            report.RegisterData(new List<BulletinLigneReportDto>(lignes), "BulletinLignes");
            var lignesDs = report.GetDataSource("BulletinLignes");
            if (lignesDs != null)
                lignesDs.Enabled = true;

            var page = new ReportPage
            {
                Name = "BulletinPage",
                PaperWidth = PageWidthMm,
                PaperHeight = PageHeightMm,
                LeftMargin = MarginMm,
                RightMargin = MarginMm,
                TopMargin = MarginMm,
                BottomMargin = MarginMm
            };
            report.Pages.Add(page);

            var title = new ReportTitleBand { Name = "TitleBand", Height = Units.Millimeters * 55f };
            page.Bands.Add(title);

            float y = Units.Millimeters * 2f;
            float w = Units.Millimeters * (PageWidthMm - 2 * MarginMm);

            title.Objects.Add(CreateText(0, y, w, Units.Millimeters * 8f, entete.NomEcole, 14, true, HorzAlign.Center));
            y += Units.Millimeters * 10f;
            title.Objects.Add(CreateText(0, y, w, Units.Millimeters * 6f, "BULLETIN SCOLAIRE", 12, true, HorzAlign.Center));
            y += Units.Millimeters * 8f;
            title.Objects.Add(CreateText(0, y, w / 2f, Units.Millimeters * 5f, $"Élève : {entete.NomEleve}", 10, false, HorzAlign.Left));
            title.Objects.Add(CreateText(w / 2f, y, w / 2f, Units.Millimeters * 5f, $"Classe : {entete.NomClasse}", 10, false, HorzAlign.Left));
            y += Units.Millimeters * 6f;
            title.Objects.Add(CreateText(0, y, w / 2f, Units.Millimeters * 5f, $"Année : {entete.LibelleAnnee}", 10, false, HorzAlign.Left));
            title.Objects.Add(CreateText(w / 2f, y, w / 2f, Units.Millimeters * 5f, $"Période : {entete.Periode}", 10, false, HorzAlign.Left));
            y += Units.Millimeters * 6f;
            title.Objects.Add(CreateText(0, y, w / 3f, Units.Millimeters * 5f, $"Moyenne : {entete.MoyenneGeneraleTexte}", 10, true, HorzAlign.Left));
            title.Objects.Add(CreateText(w / 3f, y, w / 3f, Units.Millimeters * 5f, $"Rang : {entete.RangTexte}", 10, true, HorzAlign.Left));
            title.Objects.Add(CreateText(2f * w / 3f, y, w / 3f, Units.Millimeters * 5f, $"Effectif : {entete.EffectifTexte}", 10, true, HorzAlign.Left));

            var colHeader = new ReportTitleBand { Name = "ColHeader", Height = Units.Millimeters * 8f };
            page.Bands.Add(colHeader);
            colHeader.Objects.Add(CreateText(0, 0, w * 0.35f, Units.Millimeters * 6f, "Cours", 9, true, HorzAlign.Left));
            colHeader.Objects.Add(CreateText(w * 0.35f, 0, w * 0.12f, Units.Millimeters * 6f, "Moy.", 9, true, HorzAlign.Center));
            colHeader.Objects.Add(CreateText(w * 0.47f, 0, w * 0.10f, Units.Millimeters * 6f, "Coeff.", 9, true, HorzAlign.Center));
            colHeader.Objects.Add(CreateText(w * 0.57f, 0, w * 0.43f, Units.Millimeters * 6f, "Détail notes", 9, true, HorzAlign.Left));

            var dataBand = new DataBand
            {
                Name = "LignesBand",
                Height = Units.Millimeters * 10f,
                DataSource = lignesDs
            };
            page.Bands.Add(dataBand);
            dataBand.Objects.Add(CreateText(0, 0, w * 0.35f, Units.Millimeters * 8f, "[BulletinLignes.NomCours]", 9, false, HorzAlign.Left));
            dataBand.Objects.Add(CreateText(w * 0.35f, 0, w * 0.12f, Units.Millimeters * 8f, "[BulletinLignes.MoyenneCoursTexte]", 9, false, HorzAlign.Center));
            dataBand.Objects.Add(CreateText(w * 0.47f, 0, w * 0.10f, Units.Millimeters * 8f, "[BulletinLignes.CoefficientTexte]", 9, false, HorzAlign.Center));
            dataBand.Objects.Add(CreateText(w * 0.57f, 0, w * 0.43f, Units.Millimeters * 8f, "[BulletinLignes.DetailNotes]", 8, false, HorzAlign.Left));

            var summary = new ReportSummaryBand { Name = "SummaryBand", Height = Units.Millimeters * 10f };
            page.Bands.Add(summary);
            summary.Objects.Add(CreateText(0, 0, w, Units.Millimeters * 6f,
                "Document généré par KelasiNaBiso", 8, false, HorzAlign.Right));
        }

        private static TextObject CreateText(
            float left, float top, float width, float height,
            string text, float fontSize, bool bold, HorzAlign align)
        {
            return new TextObject
            {
                Left = left,
                Top = top,
                Width = width,
                Height = height,
                Text = text,
                Font = new System.Drawing.Font("Arial", fontSize, bold ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular),
                HorzAlign = align,
                VertAlign = VertAlign.Center,
                CanGrow = true
            };
        }
    }
}
