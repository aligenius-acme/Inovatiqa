using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class ProductPictureModel : BaseInovatiqaEntityModel
    {
        #region Properties

        public int ProductId { get; set; }

        [Display(Name = "Picture")]
        [UIHint("Picture")]
        public int PictureId { get; set; }

        [Display(Name = "Picture")]
        public string PictureUrl { get; set; }

        [Display(Name = "Display order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Alt")]
        public string OverrideAltAttribute { get; set; }

        [Display(Name = "Title")]
        public string OverrideTitleAttribute { get; set; }

        #endregion
    }
}