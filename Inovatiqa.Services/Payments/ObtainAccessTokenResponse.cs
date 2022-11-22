using System;
using Newtonsoft.Json;

namespace Inovatiqa.Services.Payments
{
    public class ObtainAccessTokenResponse
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        [JsonProperty(PropertyName = "expires_at")]
        public DateTime ExpirationDate { get; set; }

        [JsonProperty(PropertyName = "merchant_id")]
        public string MerchantId { get; set; }

        [JsonProperty(PropertyName = "plan_id")]
        public string PlanId { get; set; }

        [JsonProperty(PropertyName = "id_token")]
        public string OpenIdToken { get; set; }

        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshToken { get; set; }
    }
}