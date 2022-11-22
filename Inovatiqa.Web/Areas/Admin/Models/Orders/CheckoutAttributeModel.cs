using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Orders
{
    public partial class CheckoutAttributeModel : BaseInovatiqaEntityModel, 
        ILocalizedModel<CheckoutAttributeLocalizedModel>, IStoreMappingSupportedModel
    {
        #region Ctor

        public CheckoutAttributeModel()
        {
            Locales = new List<CheckoutAttributeLocalizedModel>();
            AvailableTaxCategories = new List<SelectListItem>();
            ConditionModel = new ConditionModel();
            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
            CheckoutAttributeValueSearchModel = new CheckoutAttributeValueSearchModel();
        }

        #endregion

        #region Properties

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Text prompt")]
        public string TextPrompt { get; set; }

        [Display(Name = "Required")]
        public bool IsRequired { get; set; }

        [Display(Name = "Shippable product required")]
        public bool ShippableProductRequired { get; set; }

        [Display(Name = "Tax exempt")]
        public bool IsTaxExempt { get; set; }

        [Display(Name = "Tax category")]
        public int TaxCategoryId { get; set; }
        public IList<SelectListItem> AvailableTaxCategories { get; set; }

        [Display(Name = "Control type")]
        public int AttributeControlTypeId { get; set; }
        [Display(Name = "Control type")]
        public string AttributeControlTypeName { get; set; }

        [Display(Name = "Display order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Minimum length")]
        [UIHint("Int32Nullable")]
        public int? ValidationMinLength { get; set; }

        [Display(Name = "Maximum length")]
        [UIHint("Int32Nullable")]
        public int? ValidationMaxLength { get; set; }

        [Display(Name = "Allowed file extensions")]
        public string ValidationFileAllowedExtensions { get; set; }

        [Display(Name = "Maximum file size (KB)")]
        [UIHint("Int32Nullable")]
        public int? ValidationFileMaximumSize { get; set; }

        [Display(Name = "Default value")]
        public string DefaultValue { get; set; }

        public IList<CheckoutAttributeLocalizedModel> Locales { get; set; }

        public bool ConditionAllowed { get; set; }
        public ConditionModel ConditionModel { get; set; }

        [Display(Name = "Limited to stores")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        public CheckoutAttributeValueSearchModel CheckoutAttributeValueSearchModel { get; set; }

        #endregion
    }

    public partial class ConditionModel : BaseInovatiqaEntityModel
    {
        public ConditionModel()
        {
            ConditionAttributes = new List<AttributeConditionModel>();
        }

        [Display(Name = "Enable condition")]
        public bool EnableCondition { get; set; }

        [Display(Name = "Attribute")]
        public int SelectedAttributeId { get; set; }

        public IList<AttributeConditionModel> ConditionAttributes { get; set; }
    }

    public partial class AttributeConditionModel : BaseInovatiqaEntityModel
    {
        public string Name { get; set; }

        public int AttributeControlTypeId { get; set; }

        public IList<SelectListItem> Values { get; set; }

        public string SelectedValueId { get; set; }
    }

    public partial class CheckoutAttributeLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Text prompt")]
        public string TextPrompt { get; set; }

        [Display(Name = "Default value")]
        public string DefaultValue { get; set; }
    }
}
