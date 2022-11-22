using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Catalog
{
    public class CategorySimpleModel
    {
        public CategorySimpleModel()
        {
            SubCategories = new List<CategorySimpleModel>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public bool IncludeInTopMenu { get; set; }

        public string SeName { get; set; }

        public int? NumberOfProducts { get; set; }

        public List<CategorySimpleModel> SubCategories { get; set; }

        public string Route { get; set; }

        public bool HaveSubCategories { get; set; }
    }
}