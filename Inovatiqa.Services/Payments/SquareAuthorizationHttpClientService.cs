using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Inovatiqa.Core;
using Inovatiqa.Services.Payments;
using Inovatiqa.Services.Payments.Interfaces;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Inovatiqa.Payments.Services
{
    public partial class SquareAuthorizationHttpClientService: ISquareAuthorizationHttpClientService
    {
        #region Fields

        private readonly HttpClient _httpClient;

        #endregion

        #region Ctor

        public SquareAuthorizationHttpClientService(HttpClient client)
        {
            client.BaseAddress = new Uri("https://connect.squareup.com/oauth2/");
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, InovatiqaDefaults.UserAgent);
            client.DefaultRequestHeaders.Add(HeaderNames.Accept, MimeTypes.ApplicationJson);

            _httpClient = client;
        }

        #endregion

        #region Properties

        public virtual string BaseAddress => _httpClient.BaseAddress.ToString();

        #endregion

        #region Methods

        public virtual async Task<(string AccessToken, string RefreshToken)> ObtainAccessTokenAsync(string authorizationCode, int storeId)
        {
            try
            {
                var request = new ObtainAccessTokenRequest
                {
                    ApplicationId = InovatiqaDefaults.ApplicationId,
                    ApplicationSecret = InovatiqaDefaults.ApplicationSecret,
                    GrantType = GrantType.New,
                    AuthorizationCode = authorizationCode
                };
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "token");
                httpRequest.Headers.Add(HeaderNames.Authorization, $"Client {InovatiqaDefaults.ApplicationSecret}");
                httpRequest.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, MimeTypes.ApplicationJson);

                var response = await _httpClient.SendAsync(httpRequest);

                var responseContent = await response.Content.ReadAsStringAsync();
                var accessTokenResponse = JsonConvert.DeserializeObject<ObtainAccessTokenResponse>(responseContent);
                return (accessTokenResponse?.AccessToken, accessTokenResponse?.RefreshToken);
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        public virtual async Task<(string AccessToken, string RefreshToken)> RenewAccessTokenAsync(int storeId)
        {
            try
            {
                var request = new ObtainAccessTokenRequest
                {
                    ApplicationId = InovatiqaDefaults.ApplicationId,
                    ApplicationSecret = InovatiqaDefaults.ApplicationSecret,
                    GrantType = GrantType.Refresh,
                    RefreshToken = InovatiqaDefaults.RefreshToken
                };
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "token");
                httpRequest.Headers.Add(HeaderNames.Authorization, $"Client {InovatiqaDefaults.ApplicationSecret}");
                httpRequest.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, MimeTypes.ApplicationJson);

                var response = await _httpClient.SendAsync(httpRequest);

                var responseContent = await response.Content.ReadAsStringAsync();
                var accessTokenResponse = JsonConvert.DeserializeObject<ObtainAccessTokenResponse>(responseContent);
                return (accessTokenResponse?.AccessToken, accessTokenResponse?.RefreshToken);
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        public virtual async Task<bool> RevokeAccessTokensAsync(int storeId)
        {
            try
            {
                var request = new RevokeAccessTokenRequest
                {
                    ApplicationId = InovatiqaDefaults.ApplicationId,
                    AccessToken = InovatiqaDefaults.AccessToken
                };
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "revoke");
                httpRequest.Headers.Add(HeaderNames.Authorization, $"Client {InovatiqaDefaults.ApplicationSecret}");
                httpRequest.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, MimeTypes.ApplicationJson);

                var response = await _httpClient.SendAsync(httpRequest);

                var responseContent = await response.Content.ReadAsStringAsync();
                var accessTokenResponse = JsonConvert.DeserializeObject<RevokeAccessTokenResponse>(responseContent);
                return accessTokenResponse?.SuccessfullyRevoked ?? false;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        #endregion
    }
}