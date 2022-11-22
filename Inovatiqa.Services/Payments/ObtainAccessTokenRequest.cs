using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Inovatiqa.Services.Payments
{
    public class ObtainAccessTokenRequest
    {
        [JsonProperty(PropertyName = "client_id")]
        public string ApplicationId { get; set; }

        [JsonProperty(PropertyName = "client_secret")]
        public string ApplicationSecret { get; set; }

        [JsonProperty(PropertyName = "code")]
        public string AuthorizationCode { get; set; }

        [JsonProperty(PropertyName = "redirect_uri")]
        public string RedirectUrl { get; set; }

        [JsonProperty(PropertyName = "grant_type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public GrantType GrantType { get; set; }

        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty(PropertyName = "migration_token")]
        public string MigrationToken { get; set; }
    }
}