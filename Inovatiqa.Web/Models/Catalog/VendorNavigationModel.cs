using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Catalog
{
    public partial class VendorNavigationModel
    {
        public VendorNavigationModel()
        {
            Vendors = new List<VendorBriefInfoModel>();
        }

        public IList<VendorBriefInfoModel> Vendors { get; set; }

        public int TotalVendors { get; set; }
    }

    public partial class VendorBriefInfoModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string SeName { get; set; }
    }
}