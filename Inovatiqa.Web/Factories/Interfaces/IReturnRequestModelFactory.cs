using Inovatiqa.Database.Models;
using Inovatiqa.Web.Models.Order;
using System;

namespace Inovatiqa.Web.Factories.Interfaces
{
    public partial interface IReturnRequestModelFactory
    {
        SubmitReturnRequestModel.OrderItemModel PrepareSubmitReturnRequestOrderItemModel(OrderItem orderItem);

        SubmitReturnRequestModel PrepareSubmitReturnRequestModel(SubmitReturnRequestModel model, Order order);

        CustomerReturnRequestsModel PrepareCustomerReturnRequestsModel(DateTime? startDateValue = null, DateTime? endDateValue = null, int returnStatusId = -1, bool orderByDate = true, int returnRequestFindByKey = -1, string returnRequestFindByKeyValue = null);
        
        ReturnRequestItemsSelectionModel PrepareItemsSelectionModel(DateTime? startDateValue = null, DateTime? endDateValue = null);

        ReturnRequestItemsSelectionModel SelectItemsFromModel(ReturnRequestItemsSelectionModel model, int[] selectedItems = null, int[] selectedQuantity = null, int[] selectedReason = null, int[] selectedAction = null);

        ReturnRequestItemsSelectionModel SelectCurrentItem(ReturnRequestItemsSelectionModel model, int selected = 0, int quantity = 0, int reason = 0, int action = 0);

        ReturnRequestItemsSelectionModel PrepareShippingInfoModel(ReturnRequestItemsSelectionModel model, int[] items, int[] quantities, decimal totalPrice, int[] returnReasons, int[] selectedActions = null);

        ReturnRequestItemsSelectionModel PrepareCustomerReturnRequestReviewModel(ReturnRequestItemsSelectionModel model, Customer customer, int[] items, int[] quantities, int[] reasons, int[] actions, decimal totalPrice, int shippingLabel, string email1, string email2);

        ReturnRequestItemsSelectionModel PrepareCustomerReturnRequestCompletedModel(ReturnRequestItemsSelectionModel model, Customer customer, int[] selected = null, int[] quantity = null, decimal credit = 0, int shippingLabel = 0, string email1 = "", string email2 = "", int[] reasons = null, int[] actions = null);

    }
}
