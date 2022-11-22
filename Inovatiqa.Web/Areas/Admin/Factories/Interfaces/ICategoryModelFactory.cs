using Inovatiqa.Database.Models;
using Inovatiqa.Web.Areas.Admin.Models.Catalog;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial interface ICategoryModelFactory
    {
        CategorySearchModel PrepareCategorySearchModel(CategorySearchModel searchModel);

        CategoryListModel PrepareCategoryListModel(CategorySearchModel searchModel);

        CategoryModel PrepareCategoryModel(CategoryModel model, Category category, bool excludeProperties = false);

        CategoryProductListModel PrepareCategoryProductListModel(CategoryProductSearchModel searchModel, Category category);
    }
}