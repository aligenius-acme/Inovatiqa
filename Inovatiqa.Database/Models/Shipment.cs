using System;
using System.Collections.Generic;

namespace Inovatiqa.Database.Models
{
    public partial class Shipment
    {
        public Shipment()
        {
            ShipmentItem = new HashSet<ShipmentItem>();
        }

        public int Id { get; set; }
        public int OrderId { get; set; }
        public string TrackingNumber { get; set; }
        public decimal? TotalWeight { get; set; }
        public DateTime? ShippedDateUtc { get; set; }
        public DateTime? DeliveryDateUtc { get; set; }
        public string AdminComment { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public int? PaymentStatusId { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? AmountPaid { get; set; }
        public DateTime? InvoiceDueDateUtc { get; set; }
        public DateTime? InvoicePayedDateUtc { get; set; }

        public virtual Order Order { get; set; }
        public virtual ICollection<ShipmentItem> ShipmentItem { get; set; }
    }
}
