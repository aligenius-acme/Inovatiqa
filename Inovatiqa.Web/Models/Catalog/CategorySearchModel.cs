using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Catalog
{
    public class CategorySearchModel
    {
        public CategorySearchModel()
        {
            ChildCategoryCount = new List<KeyValuePair<string, string>>();
            searchedCategories = new List<string>();
            searchedAttributes = new List<string>();
            searchedCategoriesName = new List<string>();
            Categories = new List<ParentCategoriesModel>();
            Attributes = new List<AttributesModel>();
            AttributesValueCount = new List<KeyValuePair<string, string>>();
            NoFurtherAttribut = 1;
            NoFurtherChild = 1;
        }
        public List<string> searchedCategories { get; set; }
        public int CategoriesCount { get; set; }
        public List<string> searchedAttributes { get; set; }
        public int NoFurtherChild { get; set; }
        public int NoFurtherAttribut { get; set; }
        public int IsCatAvailable { get; set; }
        public int IsAttAvailable { get; set; }
        public List<string> searchedCategoriesName { get; set; }
        public List<KeyValuePair<string,string>> ChildCategoryCount { get; set; }
        public List<KeyValuePair<string, string>> AttributesValueCount { get; set; }
        public List<ParentCategoriesModel> Categories { get; set; }
        public List<AttributesModel> Attributes { get; set; }
        public class AttributesModel
        {
            public AttributesModel()
            {
                AttributesValues = new List<ProductDetailsModel.ProductAttributeValueModel>();
            }
            public string Id { get; set; }
            public string Name { get; set; }    
            public List<ProductDetailsModel.ProductAttributeValueModel> AttributesValues { get; set; }
        }
        public class ParentCategoriesModel
        {
            public ParentCategoriesModel()
            {
                ChildCategories = new List<ChildCategoryModel>(); 
            }
            public string Id { get; set; }
            public string Name { get; set; }
            public int ParentCategoryId { get; set; }
            public string CategoryCount { get; set; }
            public List<ChildCategoryModel> ChildCategories { get; set; }
        }
    }
}
