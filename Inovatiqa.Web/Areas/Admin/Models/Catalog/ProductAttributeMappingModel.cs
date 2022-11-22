using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class ProductAttributeMappingModel : BaseInovatiqaEntityModel, ILocalizedModel<ProductAttributeMappingLocalizedModel>
    {
        #region Ctor

        public ProductAttributeMappingModel()
        {
            AvailableProductAttributes = new List<SelectListItem>();
            Locales = new List<ProductAttributeMappingLocalizedModel>();
            ConditionModel = new ProductAttributeConditionModel();
            ProductAttributeValueSearchModel = new ProductAttributeValueSearchModel();
        }

        #endregion

        #region Properties

        public int ProductId { get; set; }

        [Display(Name = "Attribute")]
        public int ProductAttributeId { get; set; }

        [Display(Name = "Attribute")]
        public string ProductAttribute { get; set; }

        public IList<SelectListItem> AvailableProductAttributes { get; set; }

        [Display(Name = "Text prompt")]
        public string TextPrompt { get; set; }

        [Display(Name = "Is Required")]
        public bool IsRequired { get; set; }

        [Display(Name = "Control type")]
        public int AttributeControlTypeId { get; set; }

        [Display(Name = "Control type")]
        public string AttributeControlType { get; set; }

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

        public string ValidationRulesString { get; set; }

        [Display(Name = "Condition")]
        public bool ConditionAllowed { get; set; }

        public string ConditionString { get; set; }

        public ProductAttributeConditionModel ConditionModel { get; set; }

        public IList<ProductAttributeMappingLocalizedModel> Locales { get; set; }

        public ProductAttributeValueSearchModel ProductAttributeValueSearchModel { get; set; }

        #endregion
    }

    public partial class ProductAttributeMappingLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [Display(Name = "Text prompt")]
        public string TextPrompt { get; set; }

        [Display(Name = "Default value")]
        public string DefaultValue { get; set; }
    }
}
