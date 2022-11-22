using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Messages.Interfaces
{
    public partial interface IWorkflowMessageService
    {
        #region Customer workflow

        IList<int> SendCustomerWelcomeMessage(Customer customer, int languageId);

        IList<int> SendCustomerEmailRevalidationMessage(Customer customer, int languageId);

        IList<int> SendCustomerRegisteredNotificationMessage(Customer customer, int languageId);

        IList<int> SendCustomerEmailValidationMessage(Customer customer, int languageId);

        IList<int> SendCustomerPasswordRecoveryMessage(Customer customer, int languageId, string PasswordRecoveryToken);

        #endregion

        #region Order workflow

        IList<int> SendShipmentDeliveredCustomerNotification(Shipment shipment, int languageId);

        IList<int> SendShipmentSentCustomerNotification(Shipment shipment, int languageId);

        IList<int> SendOrderRefundedCustomerNotification(Order order, decimal refundedAmount, int languageId);

        IList<int> SendOrderRefundedStoreOwnerNotification(Order order, decimal refundedAmount, int languageId);

        IList<int> SendNewOrderNoteAddedCustomerNotification(OrderNote orderNote, int languageId);

        IList<int> SendOrderPaidVendorNotification(Order order, Vendor vendor, int languageId);

        IList<int> SendOrderPaidStoreOwnerNotification(Order order, int languageId);

        IList<int> SendOrderPlacedStoreOwnerNotification(Order order, int languageId);

        IList<int> SendOrderPlacedCustomerNotification(Order order, int languageId,
            string attachmentFilePath = null, string attachmentFileName = null);

        IList<int> SendOrderPlacedVendorNotification(Order order, Vendor vendor, int languageId);

        IList<int> SendOrderCompletedCustomerNotification(Order order, int languageId,
            string attachmentFilePath = null, string attachmentFileName = null);

        IList<int> SendOrderCancelledCustomerNotification(Order order, int languageId);

        IList<int> SendOrderPaidCustomerNotification(Order order, int languageId,
            string attachmentFilePath = null, string attachmentFileName = null);

        #endregion

        #region Newsletter workflow



        #endregion

        #region Send a message to a friend



        #endregion

        #region Return requests

        IList<int> SendNewReturnRequestStoreOwnerNotification(ReturnRequest returnRequest, OrderItem orderItem, Order order, int languageId);

        IList<int> SendNewReturnRequestCustomerNotification(ReturnRequest returnRequest, OrderItem orderItem, Order order);

        IList<int> SendReturnRequestStatusChangedCustomerNotification(ReturnRequest returnRequest, OrderItem orderItem, Order order);

        #endregion

        #region Forum Notifications



        #endregion

        #region Misc

        int SendNotification(MessageTemplate messageTemplate,
            EmailAccount emailAccount, int languageId, IEnumerable<Token> tokens,
            string toEmailAddress, string toName,
            string attachmentFilePath = null, string attachmentFileName = null,
            string replyToEmailAddress = null, string replyToName = null,
            string fromEmail = null, string fromName = null, string subject = null);

        IList<int> SendProductReviewNotificationMessage(ProductReview productReview, int languageId);

        #endregion
    }
}