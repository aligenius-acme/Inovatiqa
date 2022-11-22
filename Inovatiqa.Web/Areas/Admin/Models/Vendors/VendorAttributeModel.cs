using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Vendors
{
    public partial class VendorAttributeModel : BaseInovatiqaEntityModel, ILocalizedModel<VendorAttributeLocalizedModel>
    {
        #region Ctor

        public VendorAttributeModel()
        {
            Locales = new List<VendorAttributeLocalizedModel>();
            VendorAttributeValueSearchModel = new VendorAttributeValueSearchModel();
        }

        #endregion

        #region Properties

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Required")]
        public bool IsRequired { get; set; }

        [Display(Name = "Control type")]
        public int AttributeControlTypeId { get; set; }

        [Display(Name = "Control type")]
        public string AttributeControlTypeName { get; set; }

        [Display(Name = "Display order")]
        public int DisplayOrder { get; set; }

        public IList<VendorAttributeLocalizedModel> Locales { get; set; }

        public VendorAttributeValueSearchModel VendorAttributeValueSearchModel { get; set; }

        #endregion
    }

    public partial class VendorAttributeLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}
