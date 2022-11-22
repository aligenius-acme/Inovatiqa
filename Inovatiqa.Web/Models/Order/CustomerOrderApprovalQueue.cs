using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Order
{
    public class CustomerOrderApprovalQueue
    {
        public CustomerOrderApprovalQueue()
        {
            Items = new List<OrderItems>();
        }
        public int Id { get; set; }
        public string TotalPrice { get; set; }
        public bool IsApproved { get; set; }
        public int TotalItems { get; set; }
        public int TotalQunaity { get; set; }
        public IList<OrderItems> Items { get; set; }
    }
    public class OrderItems
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string TotalPrice { get; set; }
    }
}
