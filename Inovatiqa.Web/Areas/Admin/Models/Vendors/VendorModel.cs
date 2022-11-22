using Inovatiqa.Web.Areas.Admin.Models.Common;
using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Vendors
{
    public partial class VendorModel : BaseInovatiqaEntityModel, ILocalizedModel<VendorLocalizedModel>
    {
        #region Ctor

        public VendorModel()
        {
            if (PageSize < 1)
                PageSize = 5;

            Address = new AddressModel();
            VendorAttributes = new List<VendorAttributeModel>();
            Locales = new List<VendorLocalizedModel>();
            AssociatedCustomers = new List<VendorAssociatedCustomerModel>();
            VendorNoteSearchModel = new VendorNoteSearchModel();
        }

        #endregion

        #region Properties

        [Display(Name = "Name")]
        public string Name { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [UIHint("Picture")]
        [Display(Name = "Picture")]
        public int PictureId { get; set; }

        [Display(Name = "Admin comment")]
        public string AdminComment { get; set; }

        public AddressModel Address { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; }

        [Display(Name = "Display order")]
        public int DisplayOrder { get; set; }        

        [Display(Name = "Meta keywords")]
        public string MetaKeywords { get; set; }

        [Display(Name = "Meta description")]
        public string MetaDescription { get; set; }

        [Display(Name = "Meta title")]
        public string MetaTitle { get; set; }

        [Display(Name = "Search engine friendly page name")]
        public string SeName { get; set; }

        [Display(Name = "Page size")]
        public int PageSize { get; set; }

        [Display(Name = "Allow customers to select page size")]
        public bool AllowCustomersToSelectPageSize { get; set; }

        [Display(Name = "Page size options")]
        public string PageSizeOptions { get; set; }

        public List<VendorAttributeModel> VendorAttributes { get; set; }

        public IList<VendorLocalizedModel> Locales { get; set; }

        [Display(Name = "Customers")]
        public IList<VendorAssociatedCustomerModel> AssociatedCustomers { get; set; }

        [Display(Name = "Note")]
        public string AddVendorNoteMessage { get; set; }

        public VendorNoteSearchModel VendorNoteSearchModel { get; set; }

        #endregion

        #region Nested classes
        
        public partial class VendorAttributeModel : BaseInovatiqaEntityModel
        {
            public VendorAttributeModel()
            {
                Values = new List<VendorAttributeValueModel>();
            }

            public string Name { get; set; }

            public bool IsRequired { get; set; }

            public string DefaultValue { get; set; }

            public int AttributeControlTypeId { get; set; }

            public IList<VendorAttributeValueModel> Values { get; set; }
        }

        public partial class VendorAttributeValueModel : BaseInovatiqaEntityModel
        {
            public string Name { get; set; }

            public bool IsPreSelected { get; set; }
        }

        #endregion
    }

    public partial class VendorLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

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
