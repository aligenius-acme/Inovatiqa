using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class AddSpecificationAttributeModel : BaseInovatiqaEntityModel, ILocalizedModel<AddSpecificationAttributeLocalizedModel>
    {
        #region Ctor

        public AddSpecificationAttributeModel()
        {
            AvailableOptions = new List<SelectListItem>();
            AvailableAttributes = new List<SelectListItem>();
            ShowOnProductPage = true;
            AttributeName = string.Empty;
            AttributeTypeName = string.Empty;
            Value = string.Empty;
            ValueRaw = string.Empty;
            Locales = new List<AddSpecificationAttributeLocalizedModel>();
        }

        #endregion

        #region Properties

        public int SpecificationId { get; set; }

        public int AttributeTypeId { get; set; }

        [Display(Name = "Attribute type")]
        public string AttributeTypeName { get; set; }

        public int AttributeId { get; set; }

        public int ProductId { get; set; }

        public IList<SelectListItem> AvailableAttributes { get; set; }

        [Display(Name = "Attribute")]
        public string AttributeName { get; set; }

        [Display(Name = "Value")]
        public string ValueRaw { get; set; }

        [Display(Name = "Value")]
        public string Value { get; set; }

        [Display(Name = "Allow filtering")]
        public bool AllowFiltering { get; set; }

        [Display(Name = "Show on product page")]
        public bool ShowOnProductPage { get; set; }

        [Display(Name = "Display order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Attribute option")]
        public int SpecificationAttributeOptionId { get; set; }

        public IList<SelectListItem> AvailableOptions { get; set; }

        public IList<AddSpecificationAttributeLocalizedModel> Locales { get; set; }

        #endregion
    }
}
