using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Models.Customer
{
    public partial class RegisterModel
    {
        public RegisterModel()
        {
            AvailableTimeZones = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            CustomerAttributes = new List<CustomerAttributeModel>();
            GdprConsents = new List<GdprConsentModel>();
        }
        public string CustomerType { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public bool EnteringEmailTwice { get; set; }
        [DataType(DataType.EmailAddress)]
        public string ConfirmEmail { get; set; }

        public bool UsernamesEnabled { get; set; }
        public string Username { get; set; }

        public bool CheckUsernameAvailabilityEnabled { get; set; }
        [Required(ErrorMessage ="Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Confirm password is required.")]
        public string ConfirmPassword { get; set; }
        
        [Required(ErrorMessage ="Accept terms and conditions.")]
        public string termsCheckbox { get; set; }
        [Required]
        public string CaptchaCode { get; set; }

        public bool GenderEnabled { get; set; }
        public string Gender { get; set; }

        public bool FirstNameEnabled { get; set; }
        public string FirstName { get; set; }
        public bool FirstNameRequired { get; set; }
        public bool LastNameEnabled { get; set; }
        public string LastName { get; set; }
        public bool LastNameRequired { get; set; }

        public bool DateOfBirthEnabled { get; set; }
        public int? DateOfBirthDay { get; set; }
        public int? DateOfBirthMonth { get; set; }
        public int? DateOfBirthYear { get; set; }
        public bool DateOfBirthRequired { get; set; }
        public DateTime? ParseDateOfBirth()
        {
            if (!DateOfBirthYear.HasValue || !DateOfBirthMonth.HasValue || !DateOfBirthDay.HasValue)
                return null;

            DateTime? dateOfBirth = null;
            try
            {
                dateOfBirth = new DateTime(DateOfBirthYear.Value, DateOfBirthMonth.Value, DateOfBirthDay.Value);
            }
            catch { }
            return dateOfBirth;
        }

        public bool CompanyEnabled { get; set; }
        public bool CompanyRequired { get; set; }
        public string Company { get; set; }

        public bool StreetAddressEnabled { get; set; }
        public bool StreetAddressRequired { get; set; }
        public string StreetAddress { get; set; }

        public bool StreetAddress2Enabled { get; set; }
        public bool StreetAddress2Required { get; set; }
        public string StreetAddress2 { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }
        public bool ZipPostalCodeRequired { get; set; }
        public string ZipPostalCode { get; set; }

        public bool CityEnabled { get; set; }
        public bool CityRequired { get; set; }
        public string City { get; set; }

        public bool CountyEnabled { get; set; }
        public bool CountyRequired { get; set; }
        public string County { get; set; }

        public bool CountryEnabled { get; set; }
        public bool CountryRequired { get; set; }
        public int CountryId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }

        public bool StateProvinceEnabled { get; set; }
        public bool StateProvinceRequired { get; set; }
        public int StateProvinceId { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

        public bool PhoneEnabled { get; set; }
        public bool PhoneRequired { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }
        public bool ShipToCountryEnabled { get; set; }
        public bool ShipToCountryRequired { get; set; }
        //[Required]
        public int ShipToCountryId { get; set; }
        public bool ShipToStateProvinceEnabled { get; set; }
        public bool ShipToStateProvinceRequired { get; set; }
        //[Required]
        public int ShipToStateProvinceId { get; set; }
        public bool ShipToPhoneEnabled { get; set; }
        public bool ShipToPhoneRequired { get; set; }
        [RegularExpression(@"(\({0,1}[\d+]{3}\){0,1}(\-){0,1}[\d+]{3}(\-){0,1}[\d+]{4}){0,1}$", ErrorMessage = "XXX-XXX-XXXX OR (XXX)-XXX-XXXX Expected.")]
        [DataType(DataType.PhoneNumber)]
        public string ShipToPhone { get; set; }
        public bool ShipToZipPostalCodeEnabled { get; set; }
        public bool ShipToZipPostalCodeRequired { get; set; }
        //[Required]
        [RegularExpression(@"((^\d{5}$)|(^\d{9}$)|(^\d{5}-\d{4})){0,1}$", ErrorMessage = "XXXXX OR XXXXX-XXXX Expected.")]
        [DataType(DataType.PhoneNumber)]
        public string ShipToZipPostalCode { get; set; }
        public bool ShipToStreetAddressEnabled { get; set; }
        public bool ShipToStreetAddressRequired { get; set; }
        public string ShipToStreetAddress { get; set; }

        public bool ShipToCityEnabled { get; set; }
        public bool ShipToCityRequired { get; set; }
        //[Required]
        public string ShipToCity { get; set; }

        public bool BillToCountryEnabled { get; set; }
        public bool BillToCountryRequired { get; set; }
        
        public int BillToCountryId { get; set; }

        public int IsTaxExempt { get; set; }
        public bool BillToStateProvinceEnabled { get; set; }
        public bool BillToStateProvinceRequired { get; set; }
        public int BillToStateProvinceId { get; set; }

        public bool BillToPhoneEnabled { get; set; }
        public bool BillToPhoneRequired { get; set; }
        [RegularExpression(@"\({0,1}[\d+]{3}\){0,1}(\-){0,1}[\d+]{3}(\-){0,1}[\d+]{4}$", ErrorMessage = "XXX-XXX-XXXX OR (XXX)-XXX-XXXX Expected.")]
        [DataType(DataType.PhoneNumber)]
        public string BillToPhone { get; set; }

        public bool BillToZipPostalCodeEnabled { get; set; }
        public bool BillToZipPostalCodeRequired { get; set; }
        [RegularExpression(@"(^\d{5}$)|(^\d{9}$)|(^\d{5}-\d{4}$)", ErrorMessage = "XXXXX OR XXXXX-XXXX Expected.")]
        [DataType(DataType.PhoneNumber)]
        public string BillToZipPostalCode { get; set; }
        public bool BillToStreetAddressEnabled { get; set; }
        public bool BillToStreetAddressRequired { get; set; }
        public string BillToStreetAddress { get; set; }

        public bool BillToCityEnabled { get; set; }
        public bool BillToCityRequired { get; set; }
        public string BillToCity { get; set; }

        public bool FaxEnabled { get; set; }
        public bool FaxRequired { get; set; }
        
        [RegularExpression(@"\({0,1}[\d+]{3}\){0,1}(\-){0,1}[\d+]{3}(\-){0,1}[\d+]{4}$", ErrorMessage = "XXX-XXX-XXXX OR (XXX)-XXX-XXXX Expected.")]
        [DataType(DataType.PhoneNumber)]
        public string Fax { get; set; }
        
        public bool NewsletterEnabled { get; set; }
        public bool Newsletter { get; set; }
        
        public bool AcceptPrivacyPolicyEnabled { get; set; }
        public bool AcceptPrivacyPolicyPopup { get; set; }

        public string TimeZoneId { get; set; }
        public bool AllowCustomersToSetTimeZone { get; set; }
        public IList<SelectListItem> AvailableTimeZones { get; set; }

        public string VatNumber { get; set; }
        public bool DisplayVatNumber { get; set; }

        public bool HoneypotEnabled { get; set; }
        public bool DisplayCaptcha { get; set; }

        public IList<CustomerAttributeModel> CustomerAttributes { get; set; }

        public IList<GdprConsentModel> GdprConsents { get; set; }

        public bool IsCustomerRegistration { get; set; }
        public bool IsSameShipToAddress { get; set; }
    }
}