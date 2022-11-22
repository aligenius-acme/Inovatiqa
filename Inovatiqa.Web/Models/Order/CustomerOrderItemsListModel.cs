using System;
using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Order
{
    public partial class CustomerOrderItemsListModel
    {
        public CustomerOrderItemsListModel()
        {
            Orders = new List<OrderDetailsModel>();
            RecurringOrders = new List<RecurringOrderModel>();
            RecurringPaymentErrors = new List<string>();
            AvailableCatagories = new List<KeyValuePair<int, string>>();
        }

        public int SelectedCategory { get; set; }
        public bool IsReturnView { get; set; }
        public IList<OrderDetailsModel> Orders { get; set; }
        public IList<RecurringOrderModel> RecurringOrders { get; set; }
        public IList<string> RecurringPaymentErrors { get; set; }
        public int ActiveStatusId { get; set; }
        public int ShippedItemsCount { get; set; }
        public int BackorderItemsCount { get; set; }
        public List<KeyValuePair<int,string>> AvailableCatagories { get; set; }

        #region Nested classes

        public partial class OrderDetailsModel
        {
            public int Id { get; set; }
            public string CustomOrderNumber { get; set; }
            public string OrderTotal { get; set; }
            public bool IsReturnRequestAllowed { get; set; }
            public int OrderStatusId { get; set; }
            public string OrderStatus { get; set; }
            public int PaymentStatusId { get; set; }
            public int ShippingStatusId { get; set; }
            public DateTime CreatedOn { get; set; }
            public string ShipTo { get; set; }
            public string ShipToCity { get; set; }
            public string ShipToState { get; set; }
            public string PONumber { get; set; }
            public int TotalItems { get; set; }
            public IList<OrderItemModel> Items { get; set; }
        }

        public partial class OrderItemModel
        {
            public int Id { get; set; }
            public Guid OrderItemGuid { get; set; }
            public string Sku { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductSeName { get; set; }
            public string UnitPrice { get; set; }
            public string SubTotal { get; set; }
            public int Quantity { get; set; }
            public string AttributeInfo { get; set; }
            public string RentalInfo { get; set; }
            public string VendorName { get; set; }
            public int DownloadId { get; set; }
            public int LicenseId { get; set; }
            public string ManufacturerPartNumber { get; set; }
            public IList<Catalog.ManufacturerBriefInfoModel> ProductManufacturers { get; set; }
            public int Bo { get; set; }
            public int Shipped { get; set; }
        }

        public partial class RecurringOrderModel
        {
            public int Id { get; set; }
            public string StartDate { get; set; }
            public string CycleInfo { get; set; }
            public string NextPayment { get; set; }
            public int TotalCycles { get; set; }
            public int CyclesRemaining { get; set; }
            public int InitialOrderId { get; set; }
            public bool CanRetryLastPayment { get; set; }
            public string InitialOrderNumber { get; set; }
            public bool CanCancel { get; set; }
        }

        #endregion
    }
}