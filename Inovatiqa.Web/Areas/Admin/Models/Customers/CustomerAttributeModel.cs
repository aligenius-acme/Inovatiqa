using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerAttributeModel : BaseInovatiqaEntityModel, ILocalizedModel<CustomerAttributeLocalizedModel>
    {
        #region Ctor

        public CustomerAttributeModel()
        {
            Locales = new List<CustomerAttributeLocalizedModel>();
            CustomerAttributeValueSearchModel = new CustomerAttributeValueSearchModel();
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

        public IList<CustomerAttributeLocalizedModel> Locales { get; set; }

        public CustomerAttributeValueSearchModel CustomerAttributeValueSearchModel { get; set; }

        #endregion
    }

    public partial class CustomerAttributeLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}
