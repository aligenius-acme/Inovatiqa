using Newtonsoft.Json;

namespace Inovatiqa.Payments.Services
{
    public class RevokeAccessTokenRequest
    {
        [JsonProperty(PropertyName = "client_id")]
        public string ApplicationId { get; set; }

        [JsonProperty(PropertyName = "merchant_id")]
        public string MerchantId { get; set; }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }
    }
}