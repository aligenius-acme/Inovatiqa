using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Common
{
    public partial class AddressAttributeValueModel : BaseInovatiqaEntityModel, ILocalizedModel<AddressAttributeValueLocalizedModel>
    {
        #region Ctor

        public AddressAttributeValueModel()
        {
            Locales = new List<AddressAttributeValueLocalizedModel>();
        }

        #endregion

        #region Properties

        public int AddressAttributeId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Pre-selected")]
        public bool IsPreSelected { get; set; }

        [Display(Name = "Display order")]
        public int DisplayOrder {get;set;}

        public IList<AddressAttributeValueLocalizedModel> Locales { get; set; }

        #endregion
    }

    public partial class AddressAttributeValueLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}
