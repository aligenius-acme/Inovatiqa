using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Order
{
    public class OrderApprovalCompositeModel
    {
        public OrderApprovalCompositeModel()
        {
            customerOrdersInQueue = new List<CustomerOrderApprovalQueue>();
            OrdersWaitingForApproval = new List<OrderApprovalModel>();
        }
        public List<CustomerOrderApprovalQueue> customerOrdersInQueue { get; set; }
        public List<OrderApprovalModel> OrdersWaitingForApproval { get; set; }
        public bool showApprovalTable { get; set; }
        public bool showQueueTable { get; set; }
    }
}
