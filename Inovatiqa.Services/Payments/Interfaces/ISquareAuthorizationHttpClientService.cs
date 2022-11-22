using System.Threading.Tasks;

namespace Inovatiqa.Services.Payments.Interfaces
{
    public partial interface ISquareAuthorizationHttpClientService
    {
        #region Properties

        string BaseAddress =>string.Empty;

        #endregion

        #region Methods

        Task<(string AccessToken, string RefreshToken)> ObtainAccessTokenAsync(string authorizationCode, int storeId);

        Task<(string AccessToken, string RefreshToken)> RenewAccessTokenAsync(int storeId);

        Task<bool> RevokeAccessTokensAsync(int storeId);

        #endregion
    }
}
