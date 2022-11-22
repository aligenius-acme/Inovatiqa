using System;

namespace Inovatiqa.Database.Models
{
    public partial class CustomerReturnRequest
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public int? ShippingLabel { get; set; }
        public string PrimaryContantEmail { get; set; }
        public string SecondaryContactEmail { get; set; }
        public DateTime? CreatedDateUtc { get; set; }
        public DateTime? UpdatedDateUtc { get; set; }
    }
}
