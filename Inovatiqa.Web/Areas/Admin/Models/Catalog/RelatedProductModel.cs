using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class RelatedProductModel : BaseInovatiqaEntityModel
    {
        #region Properties

        public int ProductId2 { get; set; }

        [Display(Name = "Product")]
        public string Product2Name { get; set; }

        [Display(Name = "Display order")]
        public int DisplayOrder { get; set; }

        #endregion
    }
}