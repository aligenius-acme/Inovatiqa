using System.Collections.Generic;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Catalog.Interfaces
{
    public partial interface ICategoryService
    {
        IList<ProductCategoryMapping> GetProductCategoriesByProductId(int productId, bool showHidden = false);

        IList<Category> GetAllCategories(int storeId = 0, bool showHidden = false);

        IPagedList<Category> GetAllCategories(string categoryName, int storeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        IList<Category> SortCategoriesForTree(IList<Category> source, int parentId = 0,
            bool ignoreCategoriesWithoutExistingParent = false);

        Category GetCategoryById(int categoryId);

        IList<Category> GetCategoryBreadCrumb(Category category, IList<Category> allCategories = null, bool showHidden = false);

        IList<int> GetChildCategoryIds(int parentCategoryId, int storeId = 0, bool showHidden = false);

        IList<Category> GetAllCategoriesDisplayedOnHomepage(bool showHidden = false);
        string GetFormattedBreadCrumb(Category category, IList<Category> allCategories = null,
            string separator = ">>", int languageId = 0);

        IPagedList<ProductCategoryMapping> GetProductCategoriesByCategoryId(int categoryId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        ProductCategoryMapping GetProductCategoryById(int productCategoryId);

        void UpdateProductCategory(ProductCategoryMapping productCategory);

        //update category by hamza
        //void UpdateCategory(Category category);

        void DeleteProductCategory(ProductCategoryMapping productCategory);

        void InsertProductCategory(ProductCategoryMapping productCategory);

        ProductCategoryMapping FindProductCategory(IList<ProductCategoryMapping> source, int productId, int categoryId);
        List<Category> GetParentCategories();
        List<Category> GetChildCategories(int CategoryId);

    }
}