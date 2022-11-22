using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Orders
{
    public partial class CheckoutAttributeValueModel : BaseInovatiqaEntityModel, ILocalizedModel<CheckoutAttributeValueLocalizedModel>
    {
        #region Ctor

        public CheckoutAttributeValueModel()
        {
            Locales = new List<CheckoutAttributeValueLocalizedModel>();
        }

        #endregion

        #region Properties

        public int CheckoutAttributeId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "RGB color")]
        public string ColorSquaresRgb { get; set; }
        public bool DisplayColorSquaresRgb { get; set; }

        [Display(Name = "Price adjustment")]
        public decimal PriceAdjustment { get; set; }
        public string PrimaryStoreCurrencyCode { get; set; }

        [Display(Name = "Weight adjustment")]
        public decimal WeightAdjustment { get; set; }
        public string BaseWeightIn { get; set; }

        [Display(Name = "Pre-selected")]
        public bool IsPreSelected { get; set; }

        [Display(Name = "Display order")]
        public int DisplayOrder {get;set;}

        public IList<CheckoutAttributeValueLocalizedModel> Locales { get; set; }

        #endregion
    }

    public partial class CheckoutAttributeValueLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}
