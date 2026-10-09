using FastReport;
using FluentAssertions;
using KelasiNaBiso.Services.Reporting;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class CarteSchoolNameFitTests
    {
        [Fact]
        public void FitSchoolName_reduit_la_police_sur_une_ligne()
        {
            var text = new TextObject
            {
                Name = "ecole",
                Width = 249.48f,
                Height = 18.9f,
                WordWrap = true,
                Font = new System.Drawing.Font("Arial Black", 8f)
            };

            CarteReportLayoutBuilder.FitSchoolName(text);

            text.WordWrap.Should().BeFalse();
            text.AutoShrink.Should().Be(AutoShrinkMode.FontSize);
            text.AutoShrinkMinSize.Should().Be(5f);
            text.CanGrow.Should().BeFalse();
        }

        [Fact]
        public void ApplyEleveLayout_adapte_le_nom_ecole_long()
        {
            var report = new Report();
            CarteReportLayoutBuilder.ApplyEleveLayout(report);

            var nomEcole = report.FindObject("NomEcole").Should().BeOfType<TextObject>().Subject;
            nomEcole.AutoShrink.Should().Be(AutoShrinkMode.FontSize);
            nomEcole.AutoShrinkMinSize.Should().Be(5f);
            nomEcole.WordWrap.Should().BeFalse();
            nomEcole.Text.Should().Contain("NomEcole");
        }

        [Fact]
        public void FitSchoolNameObjects_cible_le_champ_ecole_du_template_recto()
        {
            var report = new Report();
            var page = new ReportPage();
            report.Pages.Add(page);
            var band = new DataBand();
            page.Bands.Add(band);
            band.Objects.Add(new TextObject
            {
                Name = "ecole",
                Width = 249.48f,
                Height = 18.9f,
                WordWrap = true,
                Font = new System.Drawing.Font("Arial Black", 8f)
            });

            CarteReportLayoutBuilder.FitSchoolNameObjects(report);

            var ecole = report.FindObject("ecole").Should().BeOfType<TextObject>().Subject;
            ecole.AutoShrink.Should().Be(AutoShrinkMode.FontSize);
            ecole.AutoShrinkMinSize.Should().Be(5f);
        }
    }
}
