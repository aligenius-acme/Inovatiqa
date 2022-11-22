using System;

namespace Inovatiqa.Core
{
    [Serializable]
    public partial class BestsellersReportLine
    {
        public int ProductId { get; set; }

        public decimal TotalAmount { get; set; }

        public int TotalQuantity { get; set; }
    }
}
