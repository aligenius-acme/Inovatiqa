using System;

namespace Inovatiqa.Web.Models.ShoppingCart
{
    public partial class CustomerSuspendedCartModel
    {
        public int Id { get; set; }
        public string POName { get; set; }
        public string ShipToCompanyName { get; set; }
        public string ShipToFirstName { get; set; }
        public string ShipToLastName { get; set; }
        public int TotalLines { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public int ShipToAddressId { get; set; }
    }
}