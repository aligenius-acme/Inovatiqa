using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Catalog
{
    public partial class SearchBoxModel
    {
        public SearchBoxModel()
        {
            Categories = new List<KeyValuePair<int, string>>();
        }
        public bool AutoCompleteEnabled { get; set; }
        public bool ShowProductImagesInSearchAutoComplete { get; set; }
        public int SearchTermMinimumLength { get; set; }
        public bool ShowSearchBox { get; set; }
        public List<KeyValuePair<int, string>> Categories { get; set; }
    }
}