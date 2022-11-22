using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Common
{
    public partial class AddressAttributeModel : BaseInovatiqaEntityModel, ILocalizedModel<AddressAttributeLocalizedModel>
    {
        #region Ctor

        public AddressAttributeModel()
        {
            Locales = new List<AddressAttributeLocalizedModel>();
            AddressAttributeValueSearchModel = new AddressAttributeValueSearchModel();
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

        public IList<AddressAttributeLocalizedModel> Locales { get; set; }

        public AddressAttributeValueSearchModel AddressAttributeValueSearchModel { get; set; }

        #endregion
    }

    public partial class AddressAttributeLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}
