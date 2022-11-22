using Inovatiqa.Web.Models.Common;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Payment
{
    public class PaymentShipmentModel
    {
        public PaymentShipmentModel()
        {
            PaymentShipmentListModel = new List<PaymentShipmentListModel>();
            BillingAddress = new AddressModel();
        }
        public string LoggedInUser { get; set; }

        public AddressModel BillingAddress { get; set; }

        public IList<PaymentShipmentListModel> PaymentShipmentListModel { get; set; }

        public string CustomerTotalInvoicedAmount { get; set; }

        public string CustomerTotalOpenAmount { get; set; }

        public string CustomerTotalAmount { get; set; }

        public string PastDue { get; set; }

        public string DaysPastDue15 { get; set; }

        public string DaysPastDue30 { get; set; }

        public string DaysPastDue60 { get; set; }

        public string DaysPastDue90 { get; set; }

    }
    public class PaymentShipmentListModel
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public string CustomOrderNumber { get; set; }

        public string PurchaseOrderNumber { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ShippedDate { get; set; }

        public DateTime? InvoicePayedDate { get; set; }

        public DateTime? InvoiceDueDate { get; set; }

        public string TrackingNumber { get; set; }

        public int? PaymentStatusId { get; set; }

        public decimal? TotalWeight { get; set; }

        public string OrderTotal { get; set; }
       
        public string TotalShipmentAmount { get; set; }

        public string TotalShipmentAmountPaid { get; set; }

        public string TotalShipmentOpenAmount { get; set; }      
    }
}
