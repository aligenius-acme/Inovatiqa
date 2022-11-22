using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class AssociatedProductModel : BaseInovatiqaEntityModel
    {
        #region Properties

        [Display(Name = "Product")]
        public string ProductName { get; set; }

        [Display(Name = "Display order")]
        public int DisplayOrder { get; set; }

        #endregion
    }
}
