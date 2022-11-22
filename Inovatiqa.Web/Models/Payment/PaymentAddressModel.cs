using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Models.Payment
{
    public class PaymentAddressModel
    {

    public PaymentAddressModel()
    {
        AvailableCountries = new List<SelectListItem>();
        AvailableStates = new List<SelectListItem>();
        CustomAddressAttributes = new List<AddressAttributeModel>();
    }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    public string Company { get; set; }

    public int? CountryId { get; set; }

    public string CountryName { get; set; }

    public int? StateProvinceId { get; set; }

    public string StateProvinceName { get; set; }

    public string Address1 { get; set; }

    public string City { get; set; }

    public string County { get; set; }

    public string Address2 { get; set; }

    public string ZipPostalCode { get; set; }

    [DataType(DataType.PhoneNumber)]
    public string PhoneNumber { get; set; }

    public string FaxNumber { get; set; }

    public string AddressHtml { get; set; }

    public string FormattedCustomAddressAttributes { get; set; }

    public IList<AddressAttributeModel> CustomAddressAttributes { get; set; }

    public IList<SelectListItem> AvailableCountries { get; set; }

    public IList<SelectListItem> AvailableStates { get; set; }

    public bool FirstNameEnabled { get; set; }

    public bool FirstNameRequired { get; set; }

    public bool LastNameEnabled { get; set; }

    public bool LastNameRequired { get; set; }

    public bool EmailEnabled { get; set; }

    public bool EmailRequired { get; set; }

    public bool CompanyEnabled { get; set; }

    public bool CompanyRequired { get; set; }

    public bool CountryEnabled { get; set; }

    public bool CountryRequired { get; set; }

    public bool StateProvinceEnabled { get; set; }

    public bool CityEnabled { get; set; }

    public bool CityRequired { get; set; }

    public bool CountyEnabled { get; set; }

    public bool CountyRequired { get; set; }

    public bool StreetAddressEnabled { get; set; }

    public bool StreetAddressRequired { get; set; }

    public bool StreetAddress2Enabled { get; set; }

    public bool StreetAddress2Required { get; set; }

    public bool ZipPostalCodeEnabled { get; set; }

    public bool ZipPostalCodeRequired { get; set; }

    public bool PhoneEnabled { get; set; }

    public bool PhoneRequired { get; set; }

    public bool FaxEnabled { get; set; }

    public bool FaxRequired { get; set; }

    #region Nested classes

    public partial class AddressAttributeModel
    {
        public AddressAttributeModel()
        {
            Values = new List<AddressAttributeValueModel>();
        }

        public string Name { get; set; }

        public bool IsRequired { get; set; }

        public string DefaultValue { get; set; }

        public int AttributeControlTypeId { get; set; }

        public IList<AddressAttributeValueModel> Values { get; set; }
    }

    public partial class AddressAttributeValueModel
    {
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }
    }

    #endregion
}
}
