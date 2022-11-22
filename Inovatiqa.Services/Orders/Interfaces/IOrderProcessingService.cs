using Inovatiqa.Database.Models;
using Inovatiqa.Services.Payments;
using System.Collections.Generic;

namespace Inovatiqa.Services.Orders.Interfaces
{
    public partial interface IOrderProcessingService
    {
        void UpdateOrderTotals(UpdateOrderParameters updateOrderParameters);
        void CheckOrderStatus(Order order);

        bool ValidateMinOrderSubtotalAmount(IList<ShoppingCartItem> cart);

        bool IsPaymentWorkflowRequired(IList<ShoppingCartItem> cart, bool? useRewardPoints = null);

        bool ValidateMinOrderTotalAmount(IList<ShoppingCartItem> cart);

        PlaceOrderResult PlaceOrder(ProcessPaymentRequest processPaymentRequest);

        void ReOrder(Order order);

        bool IsReturnRequestAllowed(Order order);

        void ReOrderOrderedItem(Order order, int orderLineId, int qty, Microsoft.AspNetCore.Http.IFormCollection form);

        bool CanCancelOrder(Order order);

        void CancelOrder(Order order, bool notifyCustomer);

        bool CanCapture(Order order);

        bool CanMarkOrderAsPaid(Order order);

        bool CanRefund(Order order);

        bool CanRefundOffline(Order order);

        bool CanPartiallyRefund(Order order, decimal amountToRefund);

        bool CanPartiallyRefundOffline(Order order, decimal amountToRefund);

        bool CanVoid(Order order);

        bool CanVoidOffline(Order order);

        void DeleteOrder(Order order);

        void MarkOrderAsPaid(Order order);

        IList<string> Refund(Order order);

        void RefundOffline(Order order);

        IList<string> PartiallyRefund(Order order, decimal amountToRefund);

        void PartiallyRefundOffline(Order order, decimal amountToRefund);

        void Ship(Shipment shipment, bool notifyCustomer);

        void Deliver(Shipment shipment, bool notifyCustomer);

        IList<string> Capture(Order order);

        IList<string> Void(Order order);

        CapturePaymentResult CaptureShipment(string shipmentAuthorizationId, Shipment shipment);
    }
}
