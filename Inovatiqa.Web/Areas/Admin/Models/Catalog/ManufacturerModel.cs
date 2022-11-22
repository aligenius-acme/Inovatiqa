using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Inovatiqa.Web.Framework.Models;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class ManufacturerModel : BaseInovatiqaEntityModel, IAclSupportedModel, IDiscountSupportedModel,
        ILocalizedModel<ManufacturerLocalizedModel>, IStoreMappingSupportedModel
    {
        #region Ctor

        public ManufacturerModel()
        {
            if (PageSize < 1)
            {
                PageSize = 5;
            }
            Locales = new List<ManufacturerLocalizedModel>();
            AvailableManufacturerTemplates = new List<SelectListItem>();

            AvailableDiscounts = new List<SelectListItem>();
            SelectedDiscountIds = new List<int>();

            SelectedCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();

            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();

            ManufacturerProductSearchModel = new ManufacturerProductSearchModel();
        }

        #endregion

        #region Properties

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Manufacturer template")]
        public int ManufacturerTemplateId { get; set; }

        public IList<SelectListItem> AvailableManufacturerTemplates { get; set; }

        [Display(Name = "Meta keywords")]
        public string MetaKeywords { get; set; }

        [Display(Name = "Meta description")]
        public string MetaDescription { get; set; }

        [Display(Name = "Meta title")]
        public string MetaTitle { get; set; }

        [Display(Name = "Search engine friendly page name")]
        public string SeName { get; set; }

        [UIHint("Picture")]
        [Display(Name = "Picture")]
        public int PictureId { get; set; }

        [Display(Name = "Page size")]
        public int PageSize { get; set; }

        [Display(Name = "Allow customers to select page size")]
        public bool AllowCustomersToSelectPageSize { get; set; }

        [Display(Name = "Page size options")]
        public string PageSizeOptions { get; set; }

        [Display(Name = "Price ranges")]
        public string PriceRanges { get; set; }

        [Display(Name = "Published")]
        public bool Published { get; set; }
        //added by hamza
        [Display(Name = "ShowOnHomepage")]
        public bool ShowOnHomepage { get; set; }

        [Display(Name = "Deleted")]
        public bool Deleted { get; set; }

        [Display(Name = "Display order")]
        public int DisplayOrder { get; set; }
        
        public IList<ManufacturerLocalizedModel> Locales { get; set; }

        [Display(Name = "Limited to customer roles")]
        public IList<int> SelectedCustomerRoleIds { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }
        
        [Display(Name = "Limited to stores")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        [Display(Name = "Discounts")]
        public IList<int> SelectedDiscountIds { get; set; }
        public IList<SelectListItem> AvailableDiscounts { get; set; }

        public ManufacturerProductSearchModel ManufacturerProductSearchModel { get; set; }

        #endregion
    }

    public partial class ManufacturerLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description {get;set;}

        [Display(Name = "Meta keywords")]
        public string MetaKeywords { get; set; }

        [Display(Name = "Meta description")]
        public string MetaDescription { get; set; }

        [Display(Name = "Meta title")]
        public string MetaTitle { get; set; }

        [Display(Name = "Search engine friendly page name")]
        public string SeName { get; set; }
    }
}
