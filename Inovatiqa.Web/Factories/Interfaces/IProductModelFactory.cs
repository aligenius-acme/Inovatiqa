using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Web.Models.Catalog;
using System.Collections.Generic;

namespace Inovatiqa.Web.Factories.Interfaces
{
    public partial interface IProductModelFactory
    {
        IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false, bool prepareProductAttributes = false, bool prepareProductManufacturer = true, bool prepareProductCategories = false);

        IList<ProductDetailsModel.ProductAttributeModel> PrepareProductAttributeModels(Product product, ShoppingCartItem updatecartitem);

        IList<ProductSpecificationModel> PrepareProductSpecificationModel(Product product);

        ProductDetailsModel PrepareProductDetailsModel(Product product, ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false, decimal unitPrice = 0, bool editing = false);

        ProductReviewsModel PrepareProductReviewsModel(ProductReviewsModel model, Product product);

        CustomerProductReviewsModel PrepareCustomerProductReviewsModel(int? page);

        NewProductsModel PrepareNewProductsModel(CatalogPagingFilteringModel command);

        void PreparePageSizeOptions(CatalogPagingFilteringModel pagingFilteringModel, CatalogPagingFilteringModel command,
            bool allowCustomersToSelectPageSize, string pageSizeOptions, int fixedPageSize);

        IList<CategoryModel> PrepareProductCategoriesModel(Product product);
        IList<ManufacturerBriefInfoModel> PrepareManufacturersModels(List<Manufacturer> manufacturer);
        IList<CategoryModel> PrepareCategoryModel(IList<Category> categories);
        IList<ProductDetailsModel.ProductAttributeModel> PrepareAttributeModels(IList<ProductAttribute> attributes);
    }
}
