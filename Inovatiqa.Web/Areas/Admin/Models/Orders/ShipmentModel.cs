using Inovatiqa.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Orders
{
    public partial class ShipmentModel : BaseInovatiqaEntityModel
    {
        #region Ctor

        public ShipmentModel()
        {
            ShipmentStatusEvents = new List<ShipmentStatusEventModel>();
            Items = new List<ShipmentItemModel>();
        }

        #endregion

        #region Properties

        [Display(Name = "Shipment #")]
        public override int Id { get; set; }

        public int OrderId { get; set; }

        [Display(Name = "Order #")]
        public string CustomOrderNumber { get; set; }

        [Display(Name = "Total weight")]
        public string TotalWeight { get; set; }

        [Display(Name = "Tracking number")]
        public string TrackingNumber { get; set; }

        public string TrackingNumberUrl { get; set; }

        [Display(Name = "Date shipped")]
        public string ShippedDate { get; set; }

        [Display(Name = "Shipped")]
        public bool CanShip { get; set; }

        public DateTime? ShippedDateUtc { get; set; }

        [Display(Name = "Date delivered")]
        public string DeliveryDate { get; set; }

        [Display(Name = "Delivered")]
        public bool CanDeliver { get; set; }

        public DateTime? DeliveryDateUtc { get; set; }

        [Display(Name = "Admin comment")]
        public string AdminComment { get; set; }

        public List<ShipmentItemModel> Items { get; set; }

        public IList<ShipmentStatusEventModel> ShipmentStatusEvents { get; set; }

        #endregion
    }
}
