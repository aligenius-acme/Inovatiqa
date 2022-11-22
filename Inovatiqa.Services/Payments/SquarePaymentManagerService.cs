using System;
using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Services.Logging.Interfaces;
using Inovatiqa.Services.Payments.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Square;
using Square.Exceptions;
using Square.Models;
using SquareSdk = Square;

namespace Inovatiqa.Payments.Services
{
    public partial class SquarePaymentManagerService: ISquarePaymentManagerService
    {
        #region Fields

        private readonly ILoggerService _loggerService;
        private readonly IWorkContextService _workContextService;
        private readonly ISquareAuthorizationHttpClientService _squareAuthorizationHttpClientService;

        #endregion

        #region Ctor

        public SquarePaymentManagerService(ILoggerService loggerService,
            IWorkContextService workContextService,
            ISquareAuthorizationHttpClientService squareAuthorizationHttpClientService)
        {
            _loggerService = loggerService;
            _workContextService = workContextService;
            _squareAuthorizationHttpClientService = squareAuthorizationHttpClientService;
        }

        #endregion

        #region Utilities

        private ISquareClient CreateSquareClient(int storeId)
        {
            if (InovatiqaDefaults.UseSandbox && string.IsNullOrEmpty(InovatiqaDefaults.AccessToken))
                throw new InovatiqaException("Sandbox access token should not be empty");

            var client = new SquareClient.Builder()
                .AccessToken(InovatiqaDefaults.AccessToken)
                .AddAdditionalHeader("user-agent", InovatiqaDefaults.UserAgent);
            
            if (InovatiqaDefaults.UseSandbox)
                client.Environment(SquareSdk.Environment.Sandbox);
            else
                client.Environment(SquareSdk.Environment.Production);

            return client.Build();
        }

        private void ThrowErrorsIfExists(IList<Error> errors)
        {
            if (errors?.Any() ?? false)
            {
                var errorsMessage = string.Join(";", errors.Select(error => error.Detail));
                throw new InovatiqaException($"There are errors in the service response. {errorsMessage}");
            }
        }

        private string CatchException(Exception exception)
        {
            var errorMessage = exception.Message;

            var customer = _workContextService.CurrentCustomer;

            _loggerService.Error($"Square payment error: {errorMessage}.", exception, customer);

            if (exception is ApiException apiException)
            {
                if (apiException?.Errors?.Any() ?? false)
                    errorMessage = string.Join(";", apiException.Errors.Select(error => error.Detail));
            }

            return errorMessage;
        }

        #endregion

        #region Methods

        #region Common

        public virtual Location GetSelectedActiveLocation(int storeId)
        {
            var client = CreateSquareClient(storeId);

            if (string.IsNullOrEmpty(InovatiqaDefaults.LocationId))
                return null;

            try
            {
                var locationResponse = client.LocationsApi.RetrieveLocation(InovatiqaDefaults.LocationId);
                if (locationResponse == null)
                    throw new InovatiqaException("No service response");

                ThrowErrorsIfExists(locationResponse.Errors);

                var location = locationResponse.Location;
                if (location == null
                      || location.Status != InovatiqaDefaults.LOCATION_STATUS_ACTIVE
                         || (!location.Capabilities?.Contains(InovatiqaDefaults.LOCATION_CAPABILITIES_PROCESSING) ?? true))
                {
                    throw new InovatiqaException("There are no selected active location for the account");
                }

                return location;
            }
            catch (Exception exception)
            {
                CatchException(exception);

                return null;
            }
        }

        public virtual IList<Location> GetActiveLocations(int storeId)
        {
            var client = CreateSquareClient(storeId);

            try
            {
                var listLocationsResponse = client.LocationsApi.ListLocations();
                if (listLocationsResponse == null)
                    throw new InovatiqaException("No service response");

                ThrowErrorsIfExists(listLocationsResponse.Errors);

                var activeLocations = listLocationsResponse.Locations?.Where(location => location?.Status == InovatiqaDefaults.LOCATION_STATUS_ACTIVE
                    && (location.Capabilities?.Contains(InovatiqaDefaults.LOCATION_CAPABILITIES_PROCESSING) ?? false)).ToList();
                if (!activeLocations?.Any() ?? true)
                    throw new InovatiqaException("There are no active locations for the account");

                return activeLocations;
            }
            catch (Exception exception)
            {
                CatchException(exception);

                return new List<Location>();
            }
        }

        public virtual Customer GetCustomer(string customerId, int storeId)
        {
            if (string.IsNullOrEmpty(customerId))
                return null;

            var client = CreateSquareClient(storeId);

            try
            {
                var retrieveCustomerResponse = client.CustomersApi.RetrieveCustomer(customerId);
                if (retrieveCustomerResponse == null)
                    throw new InovatiqaException("No service response");

                ThrowErrorsIfExists(retrieveCustomerResponse.Errors);

                return retrieveCustomerResponse.Customer;
            }
            catch (Exception exception)
            {
                CatchException(exception);

                return null;
            }
        }

        public virtual Customer CreateCustomer(CreateCustomerRequest customerRequest, int storeId)
        {
            if (customerRequest == null)
                throw new ArgumentNullException(nameof(customerRequest));

            var client = CreateSquareClient(storeId);

            try
            {
                var createCustomerResponse = client.CustomersApi.CreateCustomer(customerRequest);
                if (createCustomerResponse == null)
                    throw new InovatiqaException("No service response");

                ThrowErrorsIfExists(createCustomerResponse.Errors);

                return createCustomerResponse.Customer;
            }
            catch (Exception exception)
            {
                CatchException(exception);

                return null;
            }
        }

        public virtual Card CreateCustomerCard(string customerId, CreateCustomerCardRequest cardRequest, int storeId)
        {
            if (cardRequest == null)
                throw new ArgumentNullException(nameof(cardRequest));

            if (string.IsNullOrEmpty(customerId))
                return null;

            var client = CreateSquareClient(storeId);

            try
            {
                var createCustomerCardResponse = client.CustomersApi.CreateCustomerCard(customerId, cardRequest);
                if (createCustomerCardResponse == null)
                    throw new InovatiqaException("No service response");

                ThrowErrorsIfExists(createCustomerCardResponse.Errors);

                return createCustomerCardResponse.Card;
            }
            catch (Exception exception)
            {
                CatchException(exception);

                return null;
            }
        }

        #endregion

        #region Payment workflow

        public virtual (Payment, string) CreatePayment(CreatePaymentRequest paymentRequest, int storeId)
        {
            if (paymentRequest == null)
                throw new ArgumentNullException(nameof(paymentRequest));

            var client = CreateSquareClient(storeId);

            try
            {
                var paymentResponse = client.PaymentsApi.CreatePayment(paymentRequest);
                if (paymentResponse == null)
                    throw new InovatiqaException("No service response");

                ThrowErrorsIfExists(paymentResponse.Errors);

                return (paymentResponse.Payment, null);
            }
            catch (Exception exception)
            {
                return (null, CatchException(exception));
            }
        }

        public virtual (bool, string) CompletePayment(string paymentId, int storeId)
        {
            if (string.IsNullOrEmpty(paymentId))
                return (false, null);

            var client = CreateSquareClient(storeId);

            try
            {
                //var paymentResponse = client.PaymentsApi.CompletePayment(paymentId, null);
                var paymentResponse = client.PaymentsApi.CompletePayment(paymentId);
                if (paymentResponse == null)
                    throw new InovatiqaException("No service response");

                ThrowErrorsIfExists(paymentResponse.Errors);

                return (true, null);
            }
            catch (Exception exception)
            {
                return (false, CatchException(exception));
            }
        }

        public virtual (bool, string) CancelPayment(string paymentId, int storeId)
        {
            if (string.IsNullOrEmpty(paymentId))
                return (false, null);

            var client = CreateSquareClient(storeId);

            try
            {
                var paymentResponse = client.PaymentsApi.CancelPayment(paymentId);
                if (paymentResponse == null)
                    throw new InovatiqaException("No service response");

                ThrowErrorsIfExists(paymentResponse.Errors);

                return (true, null);
            }
            catch (Exception exception)
            {
                return (false, CatchException(exception));
            }
        }

        public virtual (PaymentRefund, string) RefundPayment(RefundPaymentRequest refundPaymentRequest, int storeId)
        {
            if (refundPaymentRequest == null)
                throw new ArgumentNullException(nameof(refundPaymentRequest));

            var client = CreateSquareClient(storeId);

            try
            {
                var refundPaymentResponse = client.RefundsApi.RefundPayment(refundPaymentRequest);
                if (refundPaymentResponse == null)
                    throw new InovatiqaException("No service response");

                ThrowErrorsIfExists(refundPaymentResponse.Errors);

                return (refundPaymentResponse.Refund, null);
            }
            catch (Exception exception)
            {
                return (null, CatchException(exception));
            }
        }

        public virtual (PaymentRefund, string) GetPaymentRefund(string Id, int storeId)
        {
            var client = CreateSquareClient(storeId);

            try
            {
                var refundPaymentResponse = client.RefundsApi.GetPaymentRefund(Id);
                if (refundPaymentResponse == null)
                    throw new InovatiqaException("No service response");

                ThrowErrorsIfExists(refundPaymentResponse.Errors);

                return (refundPaymentResponse.Refund, null);
            }
            catch (Exception exception)
            {
                return (null, CatchException(exception));
            }
        }

        #endregion

        #region OAuth2 authorization

        public virtual string GenerateAuthorizeUrl(int storeId)
        {
            var serviceUrl = $"{_squareAuthorizationHttpClientService.BaseAddress}authorize";

            var permissionScopes = new List<string>
            {
                "MERCHANT_PROFILE_READ",

                "PAYMENTS_READ",

                "PAYMENTS_WRITE",

                "CUSTOMERS_READ",

                "CUSTOMERS_WRITE",

                "SETTLEMENTS_READ",

                "BANK_ACCOUNTS_READ",

                "ITEMS_READ",

                "ITEMS_WRITE",

                "ORDERS_READ",

                "ORDERS_WRITE",

                "EMPLOYEES_READ",

                "EMPLOYEES_WRITE",

                "TIMECARDS_READ",

                "TIMECARDS_WRITE"
            };

            var requestingPermissions = string.Join(" ", permissionScopes);

            var queryParameters = new Dictionary<string, string>
            {
                ["client_id"] = InovatiqaDefaults.ApplicationId,

                ["response_type"] = "code",

                ["scope"] = requestingPermissions,

                ["session"] = "false",

                ["state"] = InovatiqaDefaults.AccessTokenVerificationString,

            };

            return QueryHelpers.AddQueryString(serviceUrl, queryParameters);
        }

        public virtual (string AccessToken, string RefreshToken) ObtainAccessToken(string authorizationCode, int storeId)
        {
            return _squareAuthorizationHttpClientService.ObtainAccessTokenAsync(authorizationCode, storeId).Result;
        }

        public virtual (string AccessToken, string RefreshToken) RenewAccessToken(int storeId)
        {
            return _squareAuthorizationHttpClientService.RenewAccessTokenAsync(storeId).Result;
        }

        public virtual bool RevokeAccessTokens(int storeId)
        {
            return _squareAuthorizationHttpClientService.RevokeAccessTokensAsync(storeId).Result;
        }

        #endregion

        #endregion
    }
}