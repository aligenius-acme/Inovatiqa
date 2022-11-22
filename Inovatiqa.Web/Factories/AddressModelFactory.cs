using System;
using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Common;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Factories
{
    public partial class AddressModelFactory : IAddressModelFactory
    {
        #region Fields

        private readonly IAddressAttributeFormatterService _addressAttributeFormatter;
        private readonly IAddressAttributeParserService _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly ICountryService _countryService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStateProvinceService _stateProvinceService;

        #endregion

        #region Ctor

        public AddressModelFactory(IAddressAttributeFormatterService addressAttributeFormatterService,
            IAddressAttributeParserService addressAttributeParserService,
            IAddressAttributeService addressAttributeService,
            ICountryService countryService,
            IGenericAttributeService genericAttributeService,
            IStateProvinceService stateProvinceService)
        {
            _addressAttributeFormatter = addressAttributeFormatterService;
            _addressAttributeParser = addressAttributeParserService;
            _addressAttributeService = addressAttributeService;
            _countryService = countryService;
            _genericAttributeService = genericAttributeService;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Utilities

        protected virtual void PrepareCustomAddressAttributes(AddressModel model,
            Address address, string overrideAttributesXml = "")
        {
            var attributes = _addressAttributeService.GetAllAddressAttributes();
            foreach (var attribute in attributes)
            {
                var attributeModel = new AddressAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.Name,
                    IsRequired = attribute.IsRequired,
                    AttributeControlTypeId = attribute.AttributeControlTypeId,
                };

                if (attribute.ShouldHaveValues())
                {
                    var attributeValues = _addressAttributeService.GetAddressAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new AddressAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                var selectedAddressAttributes = !string.IsNullOrEmpty(overrideAttributesXml) ?
                    overrideAttributesXml :
                    address?.CustomAttributes;
                switch (attribute.AttributeControlTypeId)
                {
                    case (int)AttributeControlType.DropdownList:
                    case (int)AttributeControlType.RadioList:
                    case (int)AttributeControlType.Checkboxes:
                        {
                            if (!string.IsNullOrEmpty(selectedAddressAttributes))
                            {
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                var selectedValues = _addressAttributeParser.ParseAddressAttributeValues(selectedAddressAttributes);
                                foreach (var attributeValue in selectedValues)
                                    foreach (var item in attributeModel.Values)
                                        if (attributeValue.Id == item.Id)
                                            item.IsPreSelected = true;
                            }
                        }
                        break;
                    case (int)AttributeControlType.ReadonlyCheckboxes:
                        {
                        }
                        break;
                    case (int)AttributeControlType.TextBox:
                    case (int)AttributeControlType.MultilineTextbox:
                        {
                            if (!string.IsNullOrEmpty(selectedAddressAttributes))
                            {
                                var enteredText = _addressAttributeParser.ParseValues(selectedAddressAttributes, attribute.Id);
                                if (enteredText.Any())
                                    attributeModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                    case (int)AttributeControlType.ColorSquares:
                    case (int)AttributeControlType.ImageSquares:
                    case (int)AttributeControlType.Datepicker:
                    case (int)AttributeControlType.FileUpload:
                    default:
                        break;
                }

                model.CustomAddressAttributes.Add(attributeModel);
            }
        }

        #endregion

        #region Methods

        public virtual void PrepareAddressModel(AddressModel model,
            Address address, bool excludeProperties,
            Func<IList<Country>> loadCountries = null,
            bool prePopulateWithCustomerFields = false,
            Customer customer = null,
            string overrideAttributesXml = "")
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));


            if (!excludeProperties && address != null)
            {
                model.Id = address.Id;
                model.FirstName = address.FirstName;
                model.LastName = address.LastName;
                model.Email = address.Email;
                model.Company = address.Company;
                model.CountryId = address.CountryId;
                model.CountryName = _countryService.GetCountryByAddress(address) is Country country ? country.Name : null;
                model.StateProvinceId = address.StateProvinceId;
                model.StateProvinceName = _stateProvinceService.GetStateProvinceByAddress(address) is StateProvince stateProvince ? stateProvince.Name : null;
                model.County = address.County;
                model.City = address.City;
                model.Address1 = address.Address1;
                model.Address2 = address.Address2;
                model.ZipPostalCode = address.ZipPostalCode;
                model.PhoneNumber = address.PhoneNumber;
                model.FaxNumber = address.FaxNumber;
            }

            if (address == null && prePopulateWithCustomerFields)
            {
                if (customer == null)
                    throw new Exception("Customer cannot be null when prepopulating an address");
                model.Email = customer.Email;
                model.FirstName = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.FirstNameAttribute, customer.Id);
                model.LastName = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.LastNameAttribute, customer.Id);
                model.Company = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CompanyAttribute, customer.Id);
                model.Address1 = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.StreetAddressAttribute, customer.Id);
                model.Address2 = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.StreetAddress2Attribute, customer.Id);
                model.ZipPostalCode = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.ZipPostalCodeAttribute, customer.Id);
                model.City = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CityAttribute, customer.Id);
                model.County = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CountyAttribute, customer.Id);
                model.PhoneNumber = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.PhoneAttribute, customer.Id);
                model.FaxNumber = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.FaxAttribute, customer.Id);
            }

            if (InovatiqaDefaults.CountryEnabled && loadCountries != null)
            {
                var countries = loadCountries();

                if (countries.Count == 1)
                {
                    model.CountryId = countries[0].Id;
                }
                else
                {
                    model.AvailableCountries.Add(new SelectListItem { Text = "Select country", Value = "0" });
                }

                foreach (var c in countries)
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (InovatiqaDefaults.StateProvinceEnabled)
                {
                    var states = _stateProvinceService
                        .GetStateProvincesByCountryId(model.CountryId ?? 0, InovatiqaDefaults.LanguageId)
                        .ToList();
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = "Select state", Value = "0" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem
                            {
                                Text = s.Name,
                                Value = s.Id.ToString(),
                                Selected = (s.Id == model.StateProvinceId)
                            });
                        }
                    }
                    else
                    {
                        var anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);
                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = anyCountrySelected ? "Other" : "Select state",
                            Value = "0"
                        });
                    }
                }
            }

            model.CompanyEnabled = InovatiqaDefaults.CompanyEnabled;
            model.CompanyRequired = InovatiqaDefaults.CompanyRequired;
            model.StreetAddressEnabled = InovatiqaDefaults.StreetAddressEnabled;
            model.StreetAddressRequired = InovatiqaDefaults.StreetAddressRequired;
            model.StreetAddress2Enabled = InovatiqaDefaults.StreetAddress2Enabled;
            model.StreetAddress2Required = InovatiqaDefaults.StreetAddress2Required;
            model.ZipPostalCodeEnabled = InovatiqaDefaults.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = InovatiqaDefaults.ZipPostalCodeRequired;
            model.CityEnabled = InovatiqaDefaults.CityEnabled;
            model.CityRequired = InovatiqaDefaults.CityRequired;
            model.CountyEnabled = InovatiqaDefaults.CountyEnabled;
            model.CountyRequired = InovatiqaDefaults.CountyRequired;
            model.CountryEnabled = InovatiqaDefaults.CountryEnabled;
            model.StateProvinceEnabled = InovatiqaDefaults.StateProvinceEnabled;
            model.PhoneEnabled = InovatiqaDefaults.PhoneEnabled;
            model.PhoneRequired = InovatiqaDefaults.PhoneRequired;
            model.FaxEnabled = InovatiqaDefaults.FaxEnabled;
            model.FaxRequired = InovatiqaDefaults.FaxRequired;

            if (_addressAttributeService != null && _addressAttributeParser != null)
            {
                PrepareCustomAddressAttributes(model, address, overrideAttributesXml);
            }
            if (_addressAttributeFormatter != null && address != null)
            {
                model.FormattedCustomAddressAttributes = _addressAttributeFormatter.FormatAttributes(address.CustomAttributes);
            }
        }

        #endregion
    }
}