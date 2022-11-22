using Newtonsoft.Json;

namespace Inovatiqa.Payments.Services
{
    public class RevokeAccessTokenResponse
    {
        [JsonProperty(PropertyName = "success")]
        public bool SuccessfullyRevoked { get; set; }
    }
}