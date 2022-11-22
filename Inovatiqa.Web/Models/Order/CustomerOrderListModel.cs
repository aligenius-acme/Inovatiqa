using System;
using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Order
{
    public partial class CustomerOrderListModel
    {
        public CustomerOrderListModel()
        {
            Orders = new List<OrderDetailsModel>();
            RecurringOrders = new List<RecurringOrderModel>();
            RecurringPaymentErrors = new List<string>();
        }

        public bool IsReturnView { get; set; }

        public IList<OrderDetailsModel> Orders { get; set; }
        public IList<RecurringOrderModel> RecurringOrders { get; set; }
        public IList<string> RecurringPaymentErrors { get; set; }
        public int AllOrdersCount { get; set; }
        public int OpenOrdersCount { get; set; }
        public int BackOrderedCount { get; set; }
        public int ShippedOrdersCount { get; set; }
        public int CurrentActive { get; set; }
         
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
            public string PaymentStatus { get; set; }
            public int ShipToId { get; set; }
            public int Bo { get; set; }
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