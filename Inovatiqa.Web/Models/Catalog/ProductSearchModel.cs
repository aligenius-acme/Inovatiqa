using Inovatiqa.Web.Models.Media;
using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Catalog
{
    public class ProductSearchModel
    {
        public ProductSearchModel()
        {
            Products = new List<ProductOverviewModel>();
            PagingFilteringContext = new CatalogPagingFilteringModel();
            Categories = new List<ParentCategoryModel>();
            CategoriesCount = new List<KeyValuePair<string, int>>();
            Manufacturers = new List<ManufacturerBriefInfoModel>();
            searchedCategories = new List<string>();
            searchedCategoriesName = new List<string>();
            ChildCategories = new List<ChildCategoryModel>();
            ChildCategoryCount = new List<KeyValuePair<string, string>>();
            searchedAttributes = new List<string>();
            searchedAttributesName = new List<string>();
        }
        public IEnumerable<ProductOverviewModel> Products { get; set; }
        public CatalogPagingFilteringModel PagingFilteringContext { get; set; }
        public IList<ManufacturerBriefInfoModel> Manufacturers { get; set; }
        public int ManCount { get; set; }
        public int CatCount { get; set; }
        public int AttCount { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }   

        public List<ParentCategoryModel> Categories { get; set; }

        public List<KeyValuePair<string, int>> CategoriesCount = new List<KeyValuePair<string, int>>();
        public string Term { get; set; }
        public string MinPrice { get; set; }
        public string MaxPrice { get; set; }
        public int IsCatAvailable { get; set; }
        public int categoryId { get; set; }
        public int IsAttAvailable { get; set; }
        public List<KeyValuePair<string, string>> ChildCategoryCount = new List<KeyValuePair<string, string>>();
        public List<ChildCategoryModel> ChildCategories { get; set; }
        public List<string> searchedCategories { get; set; }
        public List<string> searchedAttributes { get; set; }
        public List<string> searchedCategoriesName { get; set; }
        public List<string> searchedAttributesName { get; set; }
    }
    
    public class ParentCategoryModel
    {
        public ParentCategoryModel()
        {
            ChildCategories = new List<ChildCategoryModel>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentCategoryId { get; set; }
        public string CategoryCount { get; set; }
        public List<ChildCategoryModel> ChildCategories { get; set; }
    }
    public class ChildCategoryModel
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public string ChildCategoriesCount { get; set; }
        public string Name { get; set; }
        public PictureModel PictureModel { get; set; }
    }
}