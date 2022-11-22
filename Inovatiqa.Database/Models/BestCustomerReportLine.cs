namespace Inovatiqa.Domain.Models
{
    public partial class BestCustomerReportLine
    {
        public int CustomerId { get; set; }

        public decimal OrderTotal { get; set; }

        public int OrderCount { get; set; }
    }
}
