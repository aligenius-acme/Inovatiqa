namespace Inovatiqa.Services.Payments
{
    public class PaymentSettings
    {
        public string ApplicationId { get; set; }

        public string ApplicationSecret { get; set; }

        public string AccessToken { get; set; }

        public bool UseSandbox { get; set; }

        public bool Use3ds { get; set; }

        public string AccessTokenVerificationString { get; set; }

        public int TransactionMode { get; set; }

        public string LocationId { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public string RefreshToken { get; set; }
    }
}