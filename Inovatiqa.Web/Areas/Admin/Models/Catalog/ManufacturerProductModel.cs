using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class ManufacturerProductModel : BaseInovatiqaEntityModel
    {
        #region Properties

        public int ManufacturerId { get; set; }

        public int ProductId { get; set; }

        [Display(Name = "Product")]
        public string ProductName { get; set; }

        [Display(Name = "Is featured product?")]
        public bool IsFeaturedProduct { get; set; }

        [Display(Name = "Display order")]
        public int DisplayOrder { get; set; }

        #endregion
    }
}
