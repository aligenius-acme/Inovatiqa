using System.Collections.Generic;

namespace Inovatiqa.Web.Models
{
    public partial class ElasticProduct
    {
        public ElasticProduct()
        {
            Categories = new List<int>();
            Manufacturers = new List<int>();
            CategoriesNames = new List<string>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string AdminComment { get; set; }
        public string MetaKeywords { get; set; }
        public string Sku { get; set; }
        public string ManufacturerPartNumber { get; set;  }
        public string Gtin { get; set; }
        public List<string> CategoriesNames { get; set; }

        public List<int> Categories { get; set; }
        public List<int> Manufacturers { get; set; }
    }
}
