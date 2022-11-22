using Square.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Payments.Interfaces
{
    public partial interface ISquarePaymentManagerService
    {
        #region Methods

        #region Common

        Location GetSelectedActiveLocation(int storeId);

        IList<Location> GetActiveLocations(int storeId);

        Customer GetCustomer(string customerId, int storeId);

        Customer CreateCustomer(CreateCustomerRequest customerRequest, int storeId);

        Card CreateCustomerCard(string customerId, CreateCustomerCardRequest cardRequest, int storeId);

        #endregion

        #region Payment workflow

        (Payment, string) CreatePayment(CreatePaymentRequest paymentRequest, int storeId);

        (bool, string) CompletePayment(string paymentId, int storeId);

        (bool, string) CancelPayment(string paymentId, int storeId);

        (PaymentRefund, string) RefundPayment(Square.Models.RefundPaymentRequest refundPaymentRequest, int storeId);

        (PaymentRefund, string) GetPaymentRefund(string id, int storeId);

        #endregion

        #region OAuth2 authorization

        string GenerateAuthorizeUrl(int storeId);

        (string AccessToken, string RefreshToken) ObtainAccessToken(string authorizationCode, int storeId);

        (string AccessToken, string RefreshToken) RenewAccessToken(int storeId);

        bool RevokeAccessTokens(int storeId);

        #endregion

        #endregion
    }
}
