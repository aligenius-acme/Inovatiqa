using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Square;
using Inovatiqa.Core;

namespace Inovatiqa.Services.Payments
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
                InovatiqaDefaults.SandboxPaymentFormScriptPath : InovatiqaDefaults.ProdPaymentFormScriptPath;
            client = new SquareClient.Builder()
                .Environment(environment)
                .AccessToken(InovatiqaDefaults.AccessToken)
                .Build();
        }

        #endregion

        #region Properties

        public string PurchaseOrderNumber { get; set; }
        // added email by hamza in purchase order
        public string PurchaseOrderEmail { get; set; }

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
        public string ApplicationId { get; set; }
        public string LocationId { get; set; }
        public string Country { get; set; }
        public string WebPaymentsSdkUrl { get; set; }
        public SquareClient client { get; set; }
        public IList<SelectListItem> StoredCards { get; set; }

        #endregion
    }
}