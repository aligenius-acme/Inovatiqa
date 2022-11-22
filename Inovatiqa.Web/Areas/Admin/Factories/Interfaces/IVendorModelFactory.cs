using Inovatiqa.Database.Models;
using Inovatiqa.Web.Areas.Admin.Models.Vendors;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial interface IVendorModelFactory
    {
        VendorSearchModel PrepareVendorSearchModel(VendorSearchModel searchModel);

        VendorListModel PrepareVendorListModel(VendorSearchModel searchModel);

        VendorModel PrepareVendorModel(VendorModel model, Vendor vendor, bool excludeProperties = false);
    }
}