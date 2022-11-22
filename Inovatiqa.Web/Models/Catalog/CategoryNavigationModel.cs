using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Catalog
{
    public partial class CategoryNavigationModel
    {
        public CategoryNavigationModel()
        {
            Categories = new List<CategorySimpleModel>();
            CategoryCount = new List<KeyValuePair<string, int>>();
        }

        public int CurrentCategoryId { get; set; }
        public string CurrentCategoryName { get; set; }
        public List<CategorySimpleModel> Categories { get; set; }
        public List<KeyValuePair<string, int>> CategoryCount { get; set; }

        #region Nested classes

        public class CategoryLineModel
        {
            public int CurrentCategoryId { get; set; }
            public CategorySimpleModel Category { get; set; }
        }

        #endregion
    }
}