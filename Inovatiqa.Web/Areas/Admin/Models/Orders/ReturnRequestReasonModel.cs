using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Orders
{
    public partial class ReturnRequestReasonModel : BaseInovatiqaEntityModel, ILocalizedModel<ReturnRequestReasonLocalizedModel>
    {
        #region Ctor

        public ReturnRequestReasonModel()
        {
            Locales = new List<ReturnRequestReasonLocalizedModel>();
        }

        #endregion

        #region Properties

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Display order")]
        public int DisplayOrder { get; set; }

        public IList<ReturnRequestReasonLocalizedModel> Locales { get; set; }

        #endregion
    }

    public partial class ReturnRequestReasonLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}
