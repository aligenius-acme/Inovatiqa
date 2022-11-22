using System.Collections.Generic;
using Inovatiqa.Database.Models;
using Inovatiqa.Web.Models.Catalog;

namespace Inovatiqa.Web.Factories.Interfaces
{
    public partial interface ICatalogModelFactory
    {
        #region Categories

        CategoryNavigationModel PrepareCategoryNavigationModel(int currentCategoryId,
            int currentProductId);

        List<CategorySimpleModel> PrepareCategorySimpleModels(int rootCategoryId, bool loadSubCategories = true);

        CategoryModel PrepareCategoryModel(Category category, CatalogPagingFilteringModel command, int minPrice = 0, int maxPrice = 0);

        void PreparePageSizeOptions(CatalogPagingFilteringModel pagingFilteringModel, CatalogPagingFilteringModel command,
            bool allowCustomersToSelectPageSize, string pageSizeOptions, int fixedPageSize);

        List<CategoryModel> PrepareHomepageCategoryModels();

        #endregion

        #region Searching

        SearchBoxModel PrepareSearchBoxModel();

        CategoryModel PrepareCategorySearchModel(Category category, CatalogPagingFilteringModel command, string term);

        ProductSearchModel PrepareProductSearchModel(CatalogPagingFilteringModel command, string term, int categoryId, List<string> catSearchFilter, List<string> attSearchFilter, List<string> manList, int pageSize, int minPrice, int maxPrice);
        CategorySearchModel PrepareProductCategorySearchModel(CatalogPagingFilteringModel command, string term, int categoryId, List<string> catSearchFilter, List<string> attSearchFilter, List<string> manList, int pageSize, int minPrice, int maxPrice);
        List<ManufacturerSearchModel> PrepareProductManufacturerSearchModel(CatalogPagingFilteringModel command, string term, int categoryId, List<string> catSearchFilter, List<string> attSearchFilter, List<string> manList, int pageSize, int minPrice, int maxPrice);
        void IndexProducts(int start, int end);
        #endregion

        #region Manufacturers

        List<ManufacturerModel> PrepareManufacturerAllModels();

        ManufacturerNavigationModel PrepareManufacturerNavigationModel(int currentManufacturerId, List<string> selectedManufacturers);

        ManufacturerModel PrepareManufacturerModel(Manufacturer manufacturer, CatalogPagingFilteringModel command, int minPrice = 0, int maxPrice = 0);

        List<ManufacturerModel> PrepareFeaturedManufacturerModel(int number);
        void IndexManufacturers(int start, int end);
        void IndexCategories(int start, int end);
        void IndexAttributes(int start, int end);
        //void IndexProductsCheck(int start, int end);

        #endregion
    }
}