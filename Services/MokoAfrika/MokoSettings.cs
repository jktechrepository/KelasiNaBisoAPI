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

        public string GatewayUrl => IsProduction ? ProductionUrl : TestUrl;
    }
}
