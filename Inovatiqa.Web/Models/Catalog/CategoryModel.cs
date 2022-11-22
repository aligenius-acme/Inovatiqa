using Inovatiqa.Database.Models;
using Inovatiqa.Web.Models.Media;
using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Catalog
{
    public partial class CategoryModel
    {
        public CategoryModel()
        {
            PictureModel = new PictureModel();
            FeaturedProducts = new List<ProductOverviewModel>();
            Products = new List<ProductOverviewModel>();
            PagingFilteringContext = new CatalogPagingFilteringModel();
            CategoryBreadcrumb = new List<CategoryModel>();
            ChildCategories = new List<Category>();
            childCategoriesLinks = new List<CategoryModel>();
            childCategory = new List<KeyValuePair<string, int>>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }
        public int ParentCategoriesId { get; set; }
        //public List<int> ChildCategoryId { get; set; }
        //public List<string> ChildCategoryName { get; set; }
        public List<KeyValuePair<string,int>> childCategory { get; set; }
        public List<Category> ChildCategories { get; set; }
        public List<CategoryModel> childCategoriesLinks { get; set; }
        public string MinPrice { get; set; }
        public string MaxPrice { get; set; }
        public PictureModel PictureModel { get; set; }

        public CatalogPagingFilteringModel PagingFilteringContext { get; set; }

        public bool DisplayCategoryBreadcrumb { get; set; }
        public IList<CategoryModel> CategoryBreadcrumb { get; set; }
       
        public IList<ProductOverviewModel> FeaturedProducts { get; set; }
        public IList<ProductOverviewModel> Products { get; set; }

    }
}