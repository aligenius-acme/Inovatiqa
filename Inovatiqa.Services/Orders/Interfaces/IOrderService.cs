using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Services.Orders.Interfaces
{
    public partial interface IOrderService
    {
        #region Orders

        Order GetOrderByCustomOrderNumber(string customOrderNumber);

        bool HasItemsToShip(Order order);

        void DeleteOrder(Order order);

        bool HasItemsToAddToShipment(Order order);
        
		IList<Order> GetAllShippedOrdersByCustomer(Customer customer);

        IList<Order> GetAllOrdersByCustomer(Customer customer);

        IPagedList<Order> SearchOrders(int storeId = 0,
            int vendorId = 0, int customerId = 0,
            int productId = 0, int affiliateId = 0, int warehouseId = 0,
            int billingCountryId = 0, string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "",
            string orderNotes = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);

        void InsertOrder(Order order);

        void UpdateOrder(Order order);

        void InsertOrderItem(OrderItem orderItem);

        Order GetOrderById(int orderId);

        bool HasItemsToDeliver(Order order);
        bool RejectOrder(int ID);

        #endregion

        #region Orders items

        void DeleteOrderItem(OrderItem orderItem);

        void UpdateOrderItem(OrderItem orderItem);

        int GetTotalNumberOfShippedItems(OrderItem orderItem);

        int GetTotalNumberOfDeliveredItems(OrderItem orderItem);

        int GetTotalNumberOfNotYetShippedItems(OrderItem orderItem);

        Product GetProductByOrderItemId(int orderItemId);

        int GetTotalNumberOfItemsInAllShipment(OrderItem orderItem);

        int GetTotalNumberOfItemsCanBeAddedToShipment(OrderItem orderItem);

        OrderItem GetOrderItemById(int orderItemId);

        IList<OrderItem> GetOrderItems(int orderId, bool? isNotReturnable = null, bool? isShipEnabled = null, int vendorId = 0);

        #endregion

        #region Order notes

        OrderNote GetOrderNoteById(int orderNoteId);

        void InsertOrderNote(OrderNote orderNote);

        IList<OrderNote> GetOrderNotesByOrderId(int orderId, bool? displayToCustomer = null);

        string FormatOrderNoteText(OrderNote orderNote);

        void DeleteOrderNote(OrderNote orderNote);

        #endregion

        #region Recurring payments


        #endregion

        #region Recurring payment history



        #endregion
    }
}