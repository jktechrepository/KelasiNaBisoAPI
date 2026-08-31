using FluentAssertions;
using KelasiNaBiso.Models;
using KelasiNaBiso.Services.MokoAfrika;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class PaiementGatewayHelperTests
    {
        [Theory]
        [InlineData("Cash", false)]
        [InlineData("Chèque", false)]
        [InlineData("Virement", false)]
        [InlineData("Mobile Money", true)]
        [InlineData("mobilemoney", true)]
        [InlineData("Carte", true)]
        [InlineData("MoMo", true)]
        public void EstModeMokoInterdit_ShouldDetectForbiddenModes(string mode, bool expected)
        {
            PaiementGatewayHelper.EstModeMokoInterdit(mode).Should().Be(expected);
        }

        [Fact]
        public void ValiderCreationManuelle_ShouldThrow_ForMobileMoney()
        {
            var paiement = new Paiement { ModePaiement = "Mobile Money", Montant = 10 };

            var act = () => PaiementGatewayHelper.ValiderCreationManuelle(paiement);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*MokoAfrika/payin/frais-scolaire*");
        }

        [Fact]
        public void ValiderCreationManuelle_ShouldAllowCash()
        {
            var paiement = new Paiement { ModePaiement = "Cash", Montant = 10 };

            var act = () => PaiementGatewayHelper.ValiderCreationManuelle(paiement);

            act.Should().NotThrow();
        }

        [Fact]
        public void EstModeMokoInterdit_ShouldBeTrue_WhenOperateurMobileMoneySet()
        {
            var paiement = new Paiement { ModePaiement = "Cash", OperateurMobileMoney = "airtel" };

            PaiementGatewayHelper.EstModeMokoInterdit(paiement).Should().BeTrue();
        }
    }
}
