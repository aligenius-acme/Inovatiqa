using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Areas.Admin.Models.Catalog;
using Inovatiqa.Web.Framework.Models;
using Inovatiqa.Web.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerModel : BaseInovatiqaEntityModel, IAclSupportedModel
    {
        #region Ctor

        public CustomerModel()
        {
            AvailableTimeZones = new List<SelectListItem>();
            SendEmail = new SendEmailModel() { SendImmediately = true };
            SendPm = new SendPmModel();

            SelectedCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();

            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            CustomerAttributes = new List<CustomerAttributeModel>();
            AvailableNewsletterSubscriptionStores = new List<SelectListItem>();
            SelectedNewsletterSubscriptionStoreIds = new List<int>();
            AddRewardPoints = new AddRewardPointsToCustomerModel();
            CustomerRewardPointsSearchModel = new CustomerRewardPointsSearchModel();
            CustomerAddressSearchModel = new CustomerAddressSearchModel();
            CustomerOrderSearchModel = new CustomerOrderSearchModel();
            CustomerShoppingCartSearchModel = new CustomerShoppingCartSearchModel();
            CustomerActivityLogSearchModel = new CustomerActivityLogSearchModel();
            CustomerBackInStockSubscriptionSearchModel = new CustomerBackInStockSubscriptionSearchModel();
            CustomerAssociatedExternalAuthRecordsSearchModel = new CustomerAssociatedExternalAuthRecordsSearchModel();
            AvailablePaymentModes = new List<SelectListItem>();
            AvailablePaymentTerms = new List<SelectListItem>();
            TierPriceSearchModel = new TierPriceSearchModel();
        }

        #endregion

        #region Properties
        public TierPriceSearchModel TierPriceSearchModel { get; set; }
        public bool UsernamesEnabled { get; set; }

        [Display(Name = "Username")]
        public string Username { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [NoTrim]
        public string Password { get; set; }

        [Display(Name = "Manager of vendor")]
        public int VendorId { get; set; }

        public IList<SelectListItem> AvailableVendors { get; set; }

        public bool GenderEnabled { get; set; }

        [Display(Name = "Gender")]
        public string Gender { get; set; }

        public bool FirstNameEnabled { get; set; }
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        public bool LastNameEnabled { get; set; }
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Name")]
        public string FullName { get; set; }

        public bool DateOfBirthEnabled { get; set; }

        [UIHint("DateNullable")]
        [Display(Name = "Date of birth")]
        public DateTime? DateOfBirth { get; set; }

        public bool CompanyEnabled { get; set; }

        [Display(Name = "Company name")]
        public string Company { get; set; }

        public bool StreetAddressEnabled { get; set; }

        [Display(Name = "Address")]
        public string StreetAddress { get; set; }

        public bool StreetAddress2Enabled { get; set; }

        [Display(Name = "Address 2")]
        public string StreetAddress2 { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }

        [Display(Name = "Zip / postal code")]
        public string ZipPostalCode { get; set; }

        public bool CityEnabled { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        public bool CountyEnabled { get; set; }

        [Display(Name = "County / region")]
        public string County { get; set; }

        public bool CountryEnabled { get; set; }

        [Display(Name = "Country")]
        public int CountryId { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }

        public bool StateProvinceEnabled { get; set; }

        [Display(Name = "State / province")]
        public int StateProvinceId { get; set; }

        public IList<SelectListItem> AvailableStates { get; set; }

        public bool PhoneEnabled { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        public bool FaxEnabled { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Fax")]
        public string Fax { get; set; }

        public List<CustomerAttributeModel> CustomerAttributes { get; set; }

        public IList<SelectListItem> AvailablePaymentModes { get; set; }

        public IList<SelectListItem> AvailablePaymentTerms { get; set; }

        [Display(Name = "Registered in the store")]
        public string RegisteredInStore { get; set; }

        [Display(Name = "Admin comment")]
        public string AdminComment { get; set; }

        [Display(Name = "Is tax exempt")]
        public bool IsTaxExempt { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; }

        //[Display(Name = "Payment Terms")]
        //public string PaymentTerms { get; set; }

        [Display(Name = "Payment Terms")]
        public int PaymentTermsId { get; set; }

        [Display(Name = "Available Credit")]
        public decimal CreditLimit { get; set; }

        [Display(Name = "Affiliate")]
        public int AffiliateId { get; set; }

        [Display(Name = "Payment Mode")]
        public int PaymentModeId { get; set; }

        //[Display(Name = "Payment Mode")]
        //public string PaymentMode { get; set; }

        [Display(Name = "Affiliate")]
        public string AffiliateName { get; set; }

        [Display(Name = "Time zone")]
        public string TimeZoneId { get; set; }

        public bool AllowCustomersToSetTimeZone { get; set; }

        public IList<SelectListItem> AvailableTimeZones { get; set; }

        [Display(Name = "VAT number")]
        public string VatNumber { get; set; }

        public string VatNumberStatusNote { get; set; }

        public bool DisplayVatNumber { get; set; }

        public bool DisplayRegisteredInStore { get; set; }

        [Display(Name = "Created on")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Last activity")]
        public DateTime LastActivityDate { get; set; }

        [Display(Name = "IP Address")]
        public string LastIpAddress { get; set; }

        [Display(Name = "Last visited page")]
        public string LastVisitedPage { get; set; }

        [Display(Name = "Customer roles")]
        public string CustomerRoleNames { get; set; }

        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        [Display(Name = "Customer roles")]
        public IList<int> SelectedCustomerRoleIds { get; set; }

        [Display(Name = "Newsletter")]
        public IList<SelectListItem> AvailableNewsletterSubscriptionStores { get; set; }

        [Display(Name = "Newsletter")]
        public IList<int> SelectedNewsletterSubscriptionStoreIds { get; set; }

        public bool DisplayRewardPointsHistory { get; set; }

        public AddRewardPointsToCustomerModel AddRewardPoints { get; set; }

        public CustomerRewardPointsSearchModel CustomerRewardPointsSearchModel { get; set; }

        public SendEmailModel SendEmail { get; set; }

        public SendPmModel SendPm { get; set; }

        public bool AllowSendingOfPrivateMessage { get; set; }

        public bool AllowSendingOfWelcomeMessage { get; set; }

        public bool AllowReSendingOfActivationMessage { get; set; }

        public bool GdprEnabled { get; set; }
        
        public string AvatarUrl { get; internal set; }
        public bool AttachmentUploaded { get; set; }
        public string AttachmentURL { get; set; }

        public CustomerAddressSearchModel CustomerAddressSearchModel { get; set; }

        public CustomerOrderSearchModel CustomerOrderSearchModel { get; set; }

        public CustomerShoppingCartSearchModel CustomerShoppingCartSearchModel { get; set; }

        public CustomerActivityLogSearchModel CustomerActivityLogSearchModel { get; set; }

        public CustomerBackInStockSubscriptionSearchModel CustomerBackInStockSubscriptionSearchModel { get; set; }

        public CustomerAssociatedExternalAuthRecordsSearchModel CustomerAssociatedExternalAuthRecordsSearchModel { get; set; }

        #endregion

        #region Nested classes

        public partial class SendEmailModel : BaseInovatiqaModel
        {
            [Display(Name = "The subject")]
            public string Subject { get; set; }

            [Display(Name = "The email body")]
            public string Body { get; set; }

            [Display(Name = "Send immediately")]
            public bool SendImmediately { get; set; }

            [Display(Name = "Planned date of sending")]
            [UIHint("DateTimeNullable")]
            public DateTime? DontSendBeforeDate { get; set; }
        }

        public partial class SendPmModel : BaseInovatiqaModel
        {
            [Display(Name = "The subject")]
            public string Subject { get; set; }

            [Display(Name = "The message")]
            public string Message { get; set; }
        }

        public partial class CustomerAttributeModel : BaseInovatiqaEntityModel
        {
            public CustomerAttributeModel()
            {
                Values = new List<CustomerAttributeValueModel>();
            }

            public string Name { get; set; }

            public bool IsRequired { get; set; }

            public string DefaultValue { get; set; }

            public int AttributeControlTypeId { get; set; }

            public IList<CustomerAttributeValueModel> Values { get; set; }
        }

        public partial class CustomerAttributeValueModel : BaseInovatiqaEntityModel
        {
            public string Name { get; set; }

            public bool IsPreSelected { get; set; }
        }

        #endregion
    }
}
