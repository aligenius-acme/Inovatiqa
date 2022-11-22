using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerAttributeValueModel : BaseInovatiqaEntityModel, ILocalizedModel<CustomerAttributeValueLocalizedModel>
    {
        #region Ctor

        public CustomerAttributeValueModel()
        {
            Locales = new List<CustomerAttributeValueLocalizedModel>();
        }

        #endregion

        #region Properties

        public int CustomerAttributeId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Pre-selected")]
        public bool IsPreSelected { get; set; }

        [Display(Name = "Display order")]
        public int DisplayOrder {get;set;}

        public IList<CustomerAttributeValueLocalizedModel> Locales { get; set; }

        #endregion
    }

    public partial class CustomerAttributeValueLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}
