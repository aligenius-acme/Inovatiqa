using Inovatiqa.Database.Models;
using Inovatiqa.Web.Areas.Admin.Models.Shipping;

namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial interface IShippingModelFactory
    {
        ShippingProviderSearchModel PrepareShippingProviderSearchModel(ShippingProviderSearchModel searchModel);

        ShippingMethodSearchModel PrepareShippingMethodSearchModel(ShippingMethodSearchModel searchModel);

        ShippingMethodListModel PrepareShippingMethodListModel(ShippingMethodSearchModel searchModel);

        ShippingMethodModel PrepareShippingMethodModel(ShippingMethodModel model,
            ShippingMethod shippingMethod, bool excludeProperties = false);

        DatesRangesSearchModel PrepareDatesRangesSearchModel(DatesRangesSearchModel searchModel);

        DeliveryDateListModel PrepareDeliveryDateListModel(DeliveryDateSearchModel searchModel);

        DeliveryDateModel PrepareDeliveryDateModel(DeliveryDateModel model, DeliveryDate deliveryDate, bool excludeProperties = false);

        ProductAvailabilityRangeListModel PrepareProductAvailabilityRangeListModel(ProductAvailabilityRangeSearchModel searchModel);

        ProductAvailabilityRangeModel PrepareProductAvailabilityRangeModel(ProductAvailabilityRangeModel model,
            ProductAvailabilityRange productAvailabilityRange, bool excludeProperties = false);

    }
}