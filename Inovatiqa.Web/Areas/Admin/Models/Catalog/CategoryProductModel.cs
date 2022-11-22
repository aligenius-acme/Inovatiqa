using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class CategoryProductModel : BaseInovatiqaEntityModel
    {
        #region Properties

        public int CategoryId { get; set; }

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
