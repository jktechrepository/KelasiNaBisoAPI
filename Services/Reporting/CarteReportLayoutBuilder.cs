using FastReport;
using FastReport.Barcode;
using FastReport.Utils;
using System.Drawing;

namespace KelasiNaBiso.Services.Reporting
{
    /// <summary>
    /// Construit la mise en page CR80 (85,6 x 54 mm) si le template .frx est absent ou invalide.
    /// </summary>
    internal static class CarteReportLayoutBuilder
    {
        private const float CardWidthMm = 85.6f;
        private const float CardHeightMm = 54f;

        public static void ApplyEleveLayout(Report report, string dataSourceName = "Carte")
        {
            ConfigureReport(report, dataSourceName, isAgent: false);
        }

        public static void ApplyAgentLayout(Report report, string dataSourceName = "Carte")
        {
            ConfigureReport(report, dataSourceName, isAgent: true);
        }

        private static void ConfigureReport(Report report, string dataSourceName, bool isAgent)
        {
            report.Clear();
            report.ScriptLanguage = Language.CSharp;

            var page = new ReportPage
            {
                Name = "CartePage",
                PaperWidth = CardWidthMm,
                PaperHeight = CardHeightMm,
                LeftMargin = 2f,
                RightMargin = 2f,
                TopMargin = 2f,
                BottomMargin = 2f
            };
            report.Pages.Add(page);

            var band = new DataBand
            {
                Name = "CarteBand",
                Height = Units.Millimeters * (CardHeightMm - 4f),
                DataSource = report.GetDataSource(dataSourceName)
            };
            page.Bands.Add(band);

            var logo = new PictureObject
            {
                Name = "LogoEcole",
                Left = Units.Millimeters * 2f,
                Top = Units.Millimeters * 2f,
                Width = Units.Millimeters * 12f,
                Height = Units.Millimeters * 12f,
                DataColumn = $"{dataSourceName}.LogoBytes",
            };
            band.Objects.Add(logo);

            var titreEcole = CreateText(logo.Right + Units.Millimeters * 2f, Units.Millimeters * 2f,
                Units.Millimeters * 55f, Units.Millimeters * 6f, $"[{dataSourceName}.NomEcole]", 10, true);
            band.Objects.Add(titreEcole);

            var slogan = CreateText(titreEcole.Left, titreEcole.Bottom, titreEcole.Width, Units.Millimeters * 4f,
                $"[{dataSourceName}.SloganEcole]", 7, false);
            band.Objects.Add(slogan);

            var typeCarte = CreateText(titreEcole.Right + Units.Millimeters * 2f, Units.Millimeters * 2f,
                Units.Millimeters * 18f, Units.Millimeters * 5f, $"[{dataSourceName}.TypeCarte]", 8, true);
            typeCarte.HorzAlign = HorzAlign.Right;
            band.Objects.Add(typeCarte);

            var photo = new PictureObject
            {
                Name = "Photo",
                Left = Units.Millimeters * 2f,
                Top = Units.Millimeters * 18f,
                Width = Units.Millimeters * 22f,
                Height = Units.Millimeters * 28f,
                DataColumn = $"{dataSourceName}.PhotoBytes",
            };
            band.Objects.Add(photo);

            var nom = CreateText(photo.Right + Units.Millimeters * 3f, photo.Top,
                Units.Millimeters * 55f, Units.Millimeters * 8f, $"[{dataSourceName}.NomComplet]", 11, true);
            band.Objects.Add(nom);

            var matricule = CreateText(nom.Left, nom.Bottom + Units.Millimeters * 1f,
                nom.Width, Units.Millimeters * 5f, $"\"Matricule : \" + [{dataSourceName}.Matricule]", 9, false);
            band.Objects.Add(matricule);

            var detailLabel = isAgent ? "Fonction" : "Classe";
            var detailColumn = isAgent ? "Fonction" : "NomClasse";
            var detail = CreateText(nom.Left, matricule.Bottom + Units.Millimeters * 1f,
                nom.Width, Units.Millimeters * 5f,
                $"\"{detailLabel} : \" + [{dataSourceName}.{detailColumn}]", 9, false);
            band.Objects.Add(detail);

            if (isAgent)
            {
                var role = CreateText(nom.Left, detail.Bottom + Units.Millimeters * 1f,
                    nom.Width, Units.Millimeters * 5f,
                    $"\"Role : \" + [{dataSourceName}.RoleAgent]", 8, false);
                band.Objects.Add(role);
            }

            var annee = CreateText(nom.Left, Units.Millimeters * 44f,
                nom.Width, Units.Millimeters * 4f, $"[{dataSourceName}.AnneeScolaire]", 8, false);
            band.Objects.Add(annee);

            var barcode = new BarcodeObject
            {
                Name = "BarcodeMatricule",
                Left = Units.Millimeters * 2f,
                Top = Units.Millimeters * 47f,
                Width = Units.Millimeters * 50f,
                Height = Units.Millimeters * 8f,
                Barcode = new Barcode128(),
                Text = $"[{dataSourceName}.Matricule]",
                ShowText = false
            };
            band.Objects.Add(barcode);

            // Verso (2ᵉ page) — logo école centré
            var verso = new ReportPage
            {
                Name = "VersoPage",
                PaperWidth = CardWidthMm,
                PaperHeight = CardHeightMm,
                LeftMargin = 2f,
                RightMargin = 2f,
                TopMargin = 2f,
                BottomMargin = 2f
            };
            report.Pages.Add(verso);

            var versoBand = new DataBand
            {
                Name = "Data2",
                Height = Units.Millimeters * (CardHeightMm - 4f),
                DataSource = report.GetDataSource(dataSourceName),
                StartNewPage = false,
                PrintIfDatasourceEmpty = true
            };
            verso.Bands.Add(versoBand);

            var logoVerso = new PictureObject
            {
                Name = "LogoVerso",
                Left = Units.Millimeters * 30f,
                Top = Units.Millimeters * 15f,
                Width = Units.Millimeters * 25f,
                Height = Units.Millimeters * 25f,
                DataColumn = $"{dataSourceName}.LogoBytes",
            };
            versoBand.Objects.Add(logoVerso);
        }

        private static TextObject CreateText(float left, float top, float width, float height, string text,
            float fontSize, bool bold)
        {
            return new TextObject
            {
                Left = left,
                Top = top,
                Width = width,
                Height = height,
                Text = text,
                Font = new Font("Arial", fontSize, bold ? FontStyle.Bold : FontStyle.Regular),
                VertAlign = VertAlign.Center
            };
        }
    }
}
