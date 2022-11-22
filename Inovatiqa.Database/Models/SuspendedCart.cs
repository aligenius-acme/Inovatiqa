using System;

namespace Inovatiqa.Database.Models
{
    public partial class SuspendedCart
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Poname { get; set; }
        public string ShipToCompanyName { get; set; }
        public string ShipToFirstName { get; set; }
        public string ShipToLastName { get; set; }
        public int? Lines { get; set; }
        public DateTime LastModifiedDateUtc { get; set; }
        public int? SuspendedCartTypeId { get; set; }
        public string Comment { get; set; }
        public string EmailAddress { get; set; }
    }
}
