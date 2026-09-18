using System.Text.Json;
using FluentAssertions;
using KelasiNaBiso.Services.MokoAfrika;
using Xunit;

namespace KelasiNaBiso.Tests.Unit.Services
{
    public class MokoGatewayResponseParserTests
    {
        [Fact]
        public void IsPending_ShouldBeTrue_WhenResultCodeZeroAndStatusPending()
        {
            using var doc = JsonDocument.Parse("{\"resultCode\":\"0\",\"Status\":\"Pending\"}");
            var root = doc.RootElement;

            MokoGatewayResponseParser.IsPending(root).Should().BeTrue();
            MokoGatewayResponseParser.IsSuccess(root).Should().BeFalse();
            MokoGatewayResponseParser.IsDefinitiveFailure(root).Should().BeFalse();
        }

        [Fact]
        public void IsSuccess_ShouldBeTrue_WhenStatusSuccessAndNoError()
        {
            using var doc = JsonDocument.Parse("{\"resultCode\":\"0\",\"Status\":\"Success\",\"Transaction_id\":\"PD123\"}");
            var root = doc.RootElement;

            MokoGatewayResponseParser.IsSuccess(root).Should().BeTrue();
            MokoGatewayResponseParser.IsPending(root).Should().BeFalse();
            MokoGatewayResponseParser.IsDefinitiveFailure(root).Should().BeFalse();
        }

        [Fact]
        public void IsDefinitiveFailure_ShouldBeTrue_WhenStatusErrorWithResultCodeError()
        {
            using var doc = JsonDocument.Parse("{\"Status\":\"Error\",\"resultCodeError\":\"404\"}");
            var root = doc.RootElement;

            MokoGatewayResponseParser.IsDefinitiveFailure(root).Should().BeTrue();
            MokoGatewayResponseParser.IsSuccess(root).Should().BeFalse();
            MokoGatewayResponseParser.IsPending(root).Should().BeFalse();
            MokoGatewayResponseParser.IsSoftAmbiguousFailure(root).Should().BeFalse();
        }

        [Fact]
        public void IsPending_ShouldBeTrue_WhenStatusErrorWithoutResultCodeError()
        {
            using var doc = JsonDocument.Parse("{\"Status\":\"Error\",\"Comment\":\"Waiting\"}");
            var root = doc.RootElement;

            MokoGatewayResponseParser.IsDefinitiveFailure(root).Should().BeFalse();
            MokoGatewayResponseParser.IsSoftAmbiguousFailure(root).Should().BeTrue();
            MokoGatewayResponseParser.IsPending(root).Should().BeTrue();
            MokoGatewayResponseParser.IsSuccess(root).Should().BeFalse();
        }

        [Fact]
        public void IsPending_ShouldBeTrue_WhenResultCodeZeroWithoutStatus()
        {
            using var doc = JsonDocument.Parse("{\"resultCode\":\"0\"}");
            var root = doc.RootElement;

            MokoGatewayResponseParser.IsPending(root).Should().BeTrue();
            MokoGatewayResponseParser.IsSuccess(root).Should().BeFalse();
        }

        [Fact]
        public void IsSuccess_ShouldBeFalse_WhenResultCodeZeroAloneWithoutSuccessStatus()
        {
            using var doc = JsonDocument.Parse("{\"resultCode\":\"0\",\"Comment\":\"Transaction Received\"}");
            var root = doc.RootElement;

            MokoGatewayResponseParser.IsSuccess(root).Should().BeFalse();
            MokoGatewayResponseParser.IsPending(root).Should().BeTrue();
        }

        [Fact]
        public void IsDefinitiveFailure_ShouldBeTrue_WhenStatusCancelled()
        {
            using var doc = JsonDocument.Parse("{\"Status\":\"Cancelled\"}");
            var root = doc.RootElement;

            MokoGatewayResponseParser.IsDefinitiveFailure(root).Should().BeTrue();
            MokoGatewayResponseParser.IsTerminalFailureStatus(root).Should().BeTrue();
            MokoGatewayResponseParser.IsPending(root).Should().BeFalse();
        }

        [Fact]
        public void IsTerminalFailure_ShouldBeTrue_WhenTransStatusFailed()
        {
            using var doc = JsonDocument.Parse("{\"Trans_Status\":\"Failed\",\"Comment\":\"Customer cancelled\"}");
            var root = doc.RootElement;

            MokoGatewayResponseParser.IsTerminalFailureStatus(root).Should().BeTrue();
            MokoGatewayResponseParser.IsDefinitiveFailure(root).Should().BeTrue();
            MokoGatewayResponseParser.IsSoftAmbiguousFailure(root).Should().BeFalse();
            MokoGatewayResponseParser.IsPending(root).Should().BeFalse();
        }

        [Fact]
        public void IsSoftAmbiguous_ShouldBeTrue_WhenStatusFailedWithoutTransStatus()
        {
            using var doc = JsonDocument.Parse("{\"Status\":\"Failed\",\"Comment\":\"USSD pending\"}");
            var root = doc.RootElement;

            MokoGatewayResponseParser.IsSoftAmbiguousFailure(root).Should().BeTrue();
            MokoGatewayResponseParser.IsTerminalFailureStatus(root).Should().BeFalse();
            MokoGatewayResponseParser.IsPending(root).Should().BeTrue();
        }

        [Fact]
        public void IsDefinitiveFailure_ShouldBeTrue_WhenResultCodeErrorPresent()
        {
            using var doc = JsonDocument.Parse("{\"resultCode\":1,\"resultCodeError\":\"404\",\"Status\":\"Error\"}");
            var root = doc.RootElement;

            MokoGatewayResponseParser.IsDefinitiveFailure(root).Should().BeTrue();
            MokoGatewayResponseParser.IsSuccess(root).Should().BeFalse();
        }

        [Fact]
        public void GetTransactionStatus_ShouldReadTransStatusField()
        {
            using var doc = JsonDocument.Parse("{\"trans_status\":\"SUCCESS\"}");
            var root = doc.RootElement;

            MokoGatewayResponseParser.GetTransactionStatus(root).Should().Be("success");
            MokoGatewayResponseParser.IsSuccess(root).Should().BeTrue();
        }

        [Fact]
        public void IsSuccess_ShouldBeTrue_WhenTransStatusSuccessful()
        {
            using var doc = JsonDocument.Parse("{\"Comment\":\"Transaction Found\",\"Trans_Status\":\"Successful\"}");
            var root = doc.RootElement;

            MokoGatewayResponseParser.GetTransactionStatus(root).Should().Be("successful");
            MokoGatewayResponseParser.IsDefinitiveSuccess(root).Should().BeTrue();
            MokoGatewayResponseParser.IsPending(root).Should().BeFalse();
        }

        [Fact]
        public void IsSuccess_ShouldBeTrue_WhenStatusCompleted()
        {
            using var doc = JsonDocument.Parse("{\"Status\":\"Completed\"}");
            var root = doc.RootElement;

            MokoGatewayResponseParser.IsDefinitiveSuccess(root).Should().BeTrue();
            MokoGatewayResponseParser.IsPending(root).Should().BeFalse();
        }
    }
}
