namespace Inovatiqa.Core.Domain.Orders
{
    public partial class OrderAverageReportLine
    {
        public int CountOrders { get; set; }

        public decimal SumShippingExclTax { get; set; }

        public decimal OrderPaymentFeeExclTaxSum { get; set; }

        public decimal SumTax { get; set; }

        public decimal SumOrders { get; set; }

        public decimal SumRefundedAmount { get; set; }
    }
}
