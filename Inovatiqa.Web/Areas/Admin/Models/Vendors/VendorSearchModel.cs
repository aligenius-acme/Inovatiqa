using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Vendors
{
    public partial class VendorSearchModel : BaseSearchModel
    {
        #region Properties

        [Display(Name = "Vendor name")]
        public string SearchName { get; set; }

        [Display(Name = "Vendor Email")]
        public string SearchEmail { get; set; }

        #endregion
    }
}
