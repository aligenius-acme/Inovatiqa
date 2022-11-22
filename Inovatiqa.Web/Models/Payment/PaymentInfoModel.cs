using System.Collections.Generic;
using Inovatiqa.Web.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Square;
using Inovatiqa.Core;
namespace Inovatiqa.Web.Models.Payment
{
    public class PaymentInfoModel
    {
        #region Ctor

        public PaymentInfoModel()
        {
            StoredCards = new List<SelectListItem>();

            var environment = InovatiqaDefaults.UseSandbox ? Square.Environment.Sandbox : Square.Environment.Production;
            ApplicationId = InovatiqaDefaults.ApplicationId;
            LocationId = InovatiqaDefaults.LocationId;

            WebPaymentsSdkUrl = environment == Square.Environment.Sandbox ?
                "https://sandbox.web.squarecdn.com/v1/square.js" : "https://web.squarecdn.com/v1/square.js";


            client = new SquareClient.Builder()
                .Environment(environment)
                .AccessToken(InovatiqaDefaults.AccessToken)
                .Build();
        }

        #endregion

        #region Properties

        public string PurchaseOrderNumber { get; set; }

        public bool IsGuest { get; set; }

        public string CardNonce { get; set; }

        public string Token { get; set; }

        public string Errors { get; set; }

        public decimal OrderTotal { get; set; }

        public string Currency { get; set; }

        public string BillingFirstName { get; set; }

        public string BillingLastName { get; set; }

        public string BillingEmail { get; set; }

        public string BillingPostalCode { get; set; }

        public string BillingCountry { get; set; }

        public string BillingState { get; set; }

        public string BillingCity { get; set; }

        public string PostalCode { get; set; }

        public bool SaveCard { get; set; }

        public string StoredCardId { get; set; }
        public string WebPaymentsSdkUrl { get; set; }
        public string ApplicationId { get; set; }
        public string LocationId { get; set; }
        public string Country { get; set; }
        public SquareClient client { get; set; }
        public IList<SelectListItem> StoredCards { get; set; }

        #region "Specific To Payment Portal Navigation"

        public string LoggedInUser { get; set; }

        public decimal TotalPayment { get; set; }

        public decimal AmountToPay { get; set; }

        public string StatusMessage { get; set; }

        public AddressModel BillingAddress { get; set; }

        public string OrderIds { get; set; }

        public string InvoiceIds { get; set; }
        public string invoiceIdsAmounts { get; set; }

        #endregion

        #endregion
    }
}