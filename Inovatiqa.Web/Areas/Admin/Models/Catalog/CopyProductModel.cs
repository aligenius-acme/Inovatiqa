using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class CopyProductModel : BaseInovatiqaEntityModel
    {
        #region Properties

        [Display(Name = "New product name")]
        public string Name { get; set; }

        [Display(Name = "Copy images")]
        public bool CopyImages { get; set; }

        [Display(Name = "Published")]
        public bool Published { get; set; }

        #endregion
    }
}