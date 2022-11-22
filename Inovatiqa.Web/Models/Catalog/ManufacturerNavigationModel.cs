using System.Collections.Generic;
namespace Inovatiqa.Web.Models.Catalog
{
    public partial class ManufacturerNavigationModel
    {
        public ManufacturerNavigationModel()
        {
            Manufacturers = new List<ManufacturerBriefInfoModel>();
        }

        public IList<ManufacturerBriefInfoModel> Manufacturers { get; set; }

        public int TotalManufacturers { get; set; }
    }

    public partial class ManufacturerBriefInfoModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string SeName { get; set; }
        
        public bool IsActive { get; set; }
        public string Count { get; set; }

        public int? NumberOfProducts { get; set; }
        public bool IsSelected { get; set; }
    }
}