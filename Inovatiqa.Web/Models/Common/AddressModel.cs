using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Models.Common
{
    public partial class AddressModel
    {
        public AddressModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            CustomAddressAttributes = new List<AddressAttributeModel>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last name")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public bool HasCustomerBillingAddress { get; set; }
        public bool HasCustomerShippingAddress { get; set; }
        public bool IsSiblingAddress { get; set; }
        public bool CompanyEnabled { get; set; }
        public bool CompanyRequired { get; set; }
        public string Company { get; set; }
        public int CustomerId { get; set; }
        public bool CountryEnabled { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [Display(Name = "Country")]
        public int? CountryId { get; set; }
        public string CountryName { get; set; }

        public bool StateProvinceEnabled { get; set; }
        public int? StateProvinceId { get; set; }
        [Display(Name = "State / province")]
        public string StateProvinceName { get; set; }

        public bool CountyEnabled { get; set; }
        public bool CountyRequired { get; set; }
        public string County { get; set; }

        public bool CityEnabled { get; set; }
        public bool CityRequired { get; set; }
        [Required(ErrorMessage = "City is required")]
        [Display(Name = "City")]
        public string City { get; set; }

        public bool StreetAddressEnabled { get; set; }
        public bool StreetAddressRequired { get; set; }
        [Required(ErrorMessage = "Address is required")]
        [Display(Name = "Address 1")]
        public string Address1 { get; set; }

        public bool StreetAddress2Enabled { get; set; }
        public bool StreetAddress2Required { get; set; }
        public string Address2 { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }
        public bool ZipPostalCodeRequired { get; set; }

        [Required(ErrorMessage = "Zip / postal code is required")]
        [Display(Name = "Zip / postal code")]
        public string ZipPostalCode { get; set; }

        public bool PhoneEnabled { get; set; }
        public bool PhoneRequired { get; set; }
        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        public bool FaxEnabled { get; set; }
        public bool FaxRequired { get; set; }
        public string FaxNumber { get; set; }
        public bool IsShippingAddress { get; set; }
        public bool IsBillingAddress { get; set; }
        
        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

        public string FormattedCustomAddressAttributes { get; set; }
        public IList<AddressAttributeModel> CustomAddressAttributes { get; set; }
    }
}