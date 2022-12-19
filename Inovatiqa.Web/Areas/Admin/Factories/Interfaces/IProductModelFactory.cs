using Inovatiqa.Database.Models;
using Inovatiqa.Web.Areas.Admin.Models.Catalog;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial interface IProductModelFactory
    {
        ProductModel PrepareProductModel(ProductModel model, Product product, bool excludeProperties = false);

        ProductSearchModel PrepareProductSearchModel(ProductSearchModel searchModel);

        ProductListModel PrepareProductListModel(ProductSearchModel searchModel);

        ProductAttributeCombinationListModel PrepareProductAttributeCombinationListModel(
            ProductAttributeCombinationSearchModel searchModel, Product product);

        ProductSpecificationAttributeListModel PrepareProductSpecificationAttributeListModel(
            ProductSpecificationAttributeSearchModel searchModel, Product product);

        ProductAttributeMappingModel PrepareProductAttributeMappingModel(ProductAttributeMappingModel model,
            Product product, ProductProductAttributeMapping productAttributeMapping, bool excludeProperties = false);
        //tier price change
        TierPriceListModel PrepareTierPriceListModel(TierPriceSearchModel searchModel, int EntityId, string EntityName);

        ProductPictureListModel PrepareProductPictureListModel(ProductPictureSearchModel searchModel, Product product);

        ProductOrderListModel PrepareProductOrderListModel(ProductOrderSearchModel searchModel, Product product);
        //tier price change
        TierPriceModel PrepareTierPriceModel(TierPriceModel model,
            int EntityId, string EntityName, EntityTierPrice tierPrice, bool excludeProperties = false);

    }
}