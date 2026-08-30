using FluentAssertions;
using KelasiNaBiso.Helpers;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Helpers
{
    public class TelephoneNormalizerTests
    {
        [Theory]
        [InlineData("+243 819 932 032", "+243819932032")]
        [InlineData(" +243815421689 ", "+243815421689")]
        [InlineData("+243-819-932-032", "+243819932032")]
        [InlineData("+243819932032", "+243819932032")]
        public void Normalize_RemovesSpacesAndDashes(string input, string expected)
        {
            TelephoneNormalizer.Normalize(input).Should().Be(expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Normalize_Blank_ReturnsSame(string? input)
        {
            TelephoneNormalizer.Normalize(input).Should().Be(input);
        }
    }
}
