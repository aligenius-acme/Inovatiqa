using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Shipping
{
    public partial class DeliveryDateModel : BaseInovatiqaEntityModel, ILocalizedModel<DeliveryDateLocalizedModel>
    {
        #region Ctor

        public DeliveryDateModel()
        {
            Locales = new List<DeliveryDateLocalizedModel>();
        }

        #endregion

        #region Properties

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Display order")]
        public int DisplayOrder { get; set; }

        public IList<DeliveryDateLocalizedModel> Locales { get; set; }

        #endregion
    }

    public partial class DeliveryDateLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}
