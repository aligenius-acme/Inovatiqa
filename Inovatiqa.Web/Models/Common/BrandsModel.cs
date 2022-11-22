using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Common
{
    public class BrandsModel
    {
        public BrandsModel()
        {
            Brands = new List<string>();
        }
        public List<string> Brands { get; set; }
    }
}
