using FastReport;
using FastReport.Utils;
using KelasiNaBiso.Models.DTOs.Reporting;
using DataBand = FastReport.DataBand;
using DataFooterBand = FastReport.DataFooterBand;
using DataHeaderBand = FastReport.DataHeaderBand;
using ReportPage = FastReport.ReportPage;
using TextObject = FastReport.TextObject;

namespace KelasiNaBiso.Services.Reporting
{
    /// <summary>
    /// Mise en page A4 feuille d'appel agents.
    /// En-tête via DataHeaderBand (PDFSimpleExport ignore souvent ReportTitleBand).
    /// </summary>
    internal static class FeuilleAppelAgentsReportLayoutBuilder
    {
        private const float PageWidthMm = 210f;
        private const float PageHeightMm = 297f;
        private const float MarginMm = 12f;

        public static void ApplyLayout(
            Report report,
            FeuilleAppelEnteteReportDto entete,
            List<FeuilleAppelLigneReportDto> lignes)
        {
            report.Clear();
            report.ScriptLanguage = Language.CSharp;

            report.RegisterData(new List<FeuilleAppelLigneReportDto>(lignes), "FeuilleAppelLignes");
            var lignesDs = report.GetDataSource("FeuilleAppelLignes");
            if (lignesDs != null)
                lignesDs.Enabled = true;

            var page = new ReportPage
            {
                Name = "FeuilleAppelAgentsPage",
                PaperWidth = PageWidthMm,
                PaperHeight = PageHeightMm,
                LeftMargin = MarginMm,
                RightMargin = MarginMm,
                TopMargin = MarginMm,
                BottomMargin = MarginMm
            };
            report.Pages.Add(page);

            float w = Units.Millimeters * (PageWidthMm - 2 * MarginMm);

            var dataBand = new DataBand
            {
                Name = "LignesBand",
                Height = Units.Millimeters * 6f,
                DataSource = lignesDs,
                PrintIfDatasourceEmpty = true
            };
            page.Bands.Add(dataBand);

            var header = new DataHeaderBand
            {
                Name = "FeuilleHeader",
                Height = Units.Millimeters * 52f,
                RepeatOnEveryPage = true
            };
            dataBand.Header = header;
            FillHeader(header, entete, w);

            AddColumnBindings(dataBand, w);

            var footer = new DataFooterBand
            {
                Name = "FeuilleFooter",
                Height = Units.Millimeters * 8f
            };
            dataBand.Footer = footer;
            footer.Objects.Add(CreateText(0, Units.Millimeters * 1f, w, Units.Millimeters * 5f,
                "Document généré par KelasiNaBiso", 8, false, HorzAlign.Right));
        }

        private static void FillHeader(
            DataHeaderBand header,
            FeuilleAppelEnteteReportDto entete,
            float w)
        {
            float y = Units.Millimeters * 1f;

            header.Objects.Add(CreateText(0, y, w, Units.Millimeters * 7f, entete.Titre, 13, true, HorzAlign.Left));
            y += Units.Millimeters * 8f;
            header.Objects.Add(CreateText(0, y, w, Units.Millimeters * 5f, $"École : {entete.NomEcole}", 9, false, HorzAlign.Left));
            y += Units.Millimeters * 5.5f;
            header.Objects.Add(CreateText(0, y, w * 0.55f, Units.Millimeters * 5f, entete.LigneContexte, 9, false, HorzAlign.Left));
            header.Objects.Add(CreateText(w * 0.55f, y, w * 0.45f, Units.Millimeters * 5f, $"Date : {entete.DateTexte}", 9, false, HorzAlign.Left));
            y += Units.Millimeters * 6f;
            header.Objects.Add(CreateText(0, y, w * 0.25f, Units.Millimeters * 5f, $"Effectif : {entete.EffectifTexte}", 9, true, HorzAlign.Left));
            header.Objects.Add(CreateText(w * 0.25f, y, w * 0.25f, Units.Millimeters * 5f, $"Présents : {entete.NbPresentsTexte}", 9, true, HorzAlign.Left));
            header.Objects.Add(CreateText(w * 0.50f, y, w * 0.25f, Units.Millimeters * 5f, $"Absents : {entete.NbAbsentsTexte}", 9, true, HorzAlign.Left));
            header.Objects.Add(CreateText(w * 0.75f, y, w * 0.25f, Units.Millimeters * 5f, $"Retards : {entete.NbRetardsTexte}", 9, true, HorzAlign.Left));
            y += Units.Millimeters * 8f;

            header.Objects.Add(CreateText(0, y, w * 0.04f, Units.Millimeters * 5f, "#", 8, true, HorzAlign.Center));
            header.Objects.Add(CreateText(w * 0.04f, y, w * 0.12f, Units.Millimeters * 5f, "Matricule", 8, true, HorzAlign.Left));
            header.Objects.Add(CreateText(w * 0.16f, y, w * 0.22f, Units.Millimeters * 5f, "NomComplet", 8, true, HorzAlign.Left));
            header.Objects.Add(CreateText(w * 0.38f, y, w * 0.14f, Units.Millimeters * 5f, "Fonction", 8, true, HorzAlign.Left));
            header.Objects.Add(CreateText(w * 0.52f, y, w * 0.12f, Units.Millimeters * 5f, "Genre", 8, true, HorzAlign.Center));
            header.Objects.Add(CreateText(w * 0.64f, y, w * 0.10f, Units.Millimeters * 5f, "StatutJour", 8, true, HorzAlign.Center));
            header.Objects.Add(CreateText(w * 0.74f, y, w * 0.08f, Units.Millimeters * 5f, "HeureArrivee", 7, true, HorzAlign.Center));
            header.Objects.Add(CreateText(w * 0.82f, y, w * 0.08f, Units.Millimeters * 5f, "HeureDepart", 7, true, HorzAlign.Center));
            header.Objects.Add(CreateText(w * 0.90f, y, w * 0.10f, Units.Millimeters * 5f, "Observation", 7, true, HorzAlign.Left));
        }

        private static void AddColumnBindings(DataBand band, float w)
        {
            band.Objects.Add(CreateText(0, 0, w * 0.04f, Units.Millimeters * 5f, "[FeuilleAppelLignes.Index]", 8, false, HorzAlign.Center));
            band.Objects.Add(CreateText(w * 0.04f, 0, w * 0.12f, Units.Millimeters * 5f, "[FeuilleAppelLignes.Matricule]", 8, false, HorzAlign.Left));
            band.Objects.Add(CreateText(w * 0.16f, 0, w * 0.22f, Units.Millimeters * 5f, "[FeuilleAppelLignes.NomComplet]", 8, false, HorzAlign.Left));
            band.Objects.Add(CreateText(w * 0.38f, 0, w * 0.14f, Units.Millimeters * 5f, "[FeuilleAppelLignes.Fonction]", 8, false, HorzAlign.Left));
            band.Objects.Add(CreateText(w * 0.52f, 0, w * 0.12f, Units.Millimeters * 5f, "[FeuilleAppelLignes.Genre]", 8, false, HorzAlign.Center));
            band.Objects.Add(CreateText(w * 0.64f, 0, w * 0.10f, Units.Millimeters * 5f, "[FeuilleAppelLignes.StatutJour]", 8, false, HorzAlign.Center));
            band.Objects.Add(CreateText(w * 0.74f, 0, w * 0.08f, Units.Millimeters * 5f, "[FeuilleAppelLignes.HeureArrivee]", 8, false, HorzAlign.Center));
            band.Objects.Add(CreateText(w * 0.82f, 0, w * 0.08f, Units.Millimeters * 5f, "[FeuilleAppelLignes.HeureDepart]", 8, false, HorzAlign.Center));
            band.Objects.Add(CreateText(w * 0.90f, 0, w * 0.10f, Units.Millimeters * 5f, "[FeuilleAppelLignes.Observation]", 7, false, HorzAlign.Left));
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
                WordWrap = false,
                CanGrow = false
            };
        }
    }
}
