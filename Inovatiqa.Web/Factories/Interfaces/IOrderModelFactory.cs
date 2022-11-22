using Inovatiqa.Database.Models;
using Inovatiqa.Web.Models.Order;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Web.Factories.Interfaces
{
    public partial interface IOrderModelFactory
    {
        CustomerOrderItemsListModel PrepareCustomerOrderItemsListModel(DateTime? startDateValue = null, DateTime? endDateValue = null, int shippedItems = 0, int filterCategoryId = -1, int customerId = 0);

         
        CustomerOrderListModel PrepareCustomerOrderListModel(DateTime? startDateValue = null, DateTime? endDateValue = null, int orderStatusId = 0, bool returnView = false, int customerId = 0);

        CustomerOrderListModel PrepareCustomerInvoicedOrderListModel(DateTime? startDateValue = null, DateTime? endDateValue = null, int invoiceFindByKey = -1, string invoiceFindByKeyValue = null, int customerId = 0);


        OrderDetailsModel PrepareOrderDetailsModel(Order order);

        ShipmentDetailsModel PrepareShipmentDetailsModel(Shipment shipment);

        List<CustomerReorderGuideModel> PrepareCustomerReOrderGuideModel(int filterCategories =-1);
        List<OrderApprovalModel> OrderWaitingForApproval();
        List<CustomerOrderApprovalQueue> CustomerOrderInQueue();
    }
}
