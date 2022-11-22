using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Order
{
    public class OrderApprovalModel
    {
        public OrderApprovalModel()
        {
            Items = new List<ItemModel>();
        }
        public IList<ItemModel> Items { get; set; }
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }
        public int TotalItemsQuantity { get; set; }
        public string TotalPrice { get; set; }

    }
    public class ItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string TotalPrice { get; set; }
    }
}
