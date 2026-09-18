using FluentAssertions;
using KelasiNaBiso.Services;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class PeriodeCotationAliasesTests
    {
        [Theory]
        [InlineData("T1", "T1")]
        [InlineData("Trimestre 1", "T1")]
        [InlineData("1er Trimestre", "T1")]
        [InlineData("1ère Trimestre", "T1")]
        [InlineData("t2", "T2")]
        [InlineData("2ème Trimestre", "T2")]
        [InlineData("Trimestre 3", "T3")]
        [InlineData("3eme Trimestre", "T3")]
        public void ResolveCode_KnownAliases(string input, string expected)
        {
            PeriodeCotationAliases.ResolveCode(input).Should().Be(expected);
        }

        [Fact]
        public void ResolveCode_Unknown_ReturnsNull()
        {
            PeriodeCotationAliases.ResolveCode("Semestre A").Should().BeNull();
        }
    }
}
