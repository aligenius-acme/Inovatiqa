namespace Inovatiqa.Core
{
    public partial class ShippingOption
    {
        public string ShippingRateComputationMethodSystemName { get; set; }

        public decimal Rate { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int? TransitDays { get; set; }

        public bool IsPickupInStore { get; set; }
    }
}
