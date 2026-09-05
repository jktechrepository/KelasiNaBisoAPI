namespace KelasiNaBiso.Services.MokoAfrika
{
    public class MokoSettings
    {
        public string MerchantId { get; set; } = string.Empty;
        public string MerchantCode { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string HmacKey { get; set; } = string.Empty;
        public string TestUrl { get; set; } = "https://api.gofreshpay.com/api/v1/gateway";
        public string ProductionUrl { get; set; } = "https://paydrc.gofreshbakery.net/api/v5/";
        public bool IsProduction { get; set; }
        public string CallbackUrl { get; set; } = string.Empty;
        public string StaticCustomerFirstName { get; set; } = "KANSA";
        public string StaticCustomerLastName { get; set; } = "BUSINESS";
        public string StaticCustomerEmail { get; set; } = string.Empty;
        public int PayoutSettlementDelayMinutes { get; set; } = 3;

        /// <summary>
        /// Fenêtre pendant laquelle un check PayIn MM ne doit pas tuer un pending
        /// sur un échec gateway ambigu (Status Error sans resultCodeError).
        /// Alignée sur le timeout polling front (120 s).
        /// </summary>
        public int PayInUssdWindowSeconds { get; set; } = 120;

        public string GatewayUrl => IsProduction ? ProductionUrl : TestUrl;
    }
}
