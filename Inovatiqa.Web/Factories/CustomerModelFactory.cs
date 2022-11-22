using System;
using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Common;
using Inovatiqa.Web.Models.Customer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Inovatiqa.Services.Security.Interfaces;

namespace Inovatiqa.Web.Factories
{
    public partial class CustomerModelFactory : ICustomerModelFactory
    {
        #region Fields

        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IWorkContextService _workContextService;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerAttributeParserService _customerAttributeParserService;
        private readonly IDateTimeHelperService _dateTimeHelperService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ICustomerService _customerService;
        private readonly IAddressModelFactory _addressModelFactory;
		private readonly IEncryptionService _encryptionService;
        private readonly IAddressService _addressService;
        private readonly ICustomerRegistrationService _customerRegistrationService;

        #endregion

        #region Ctor

        public CustomerModelFactory(ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IWorkContextService workContextService,
            ICustomerAttributeService customerAttributeService,
            IGenericAttributeService genericAttributeService,
            ICustomerAttributeParserService customerAttributeParserService,
            IDateTimeHelperService dateTimeHelperService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IEncryptionService encryptionService)
        {
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _workContextService = workContextService;
            _customerAttributeService = customerAttributeService;
            _genericAttributeService = genericAttributeService;
            _customerAttributeParserService = customerAttributeParserService;
            _dateTimeHelperService = dateTimeHelperService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _addressModelFactory = addressModelFactory;
            _customerService = customerService;
            _encryptionService = encryptionService;
            _addressService = addressService;
            _customerRegistrationService = customerRegistrationService;
        }

        #endregion

        #region Utilities

        protected virtual GdprConsentModel PrepareGdprConsentModel(GdprConsent consent, bool accepted)
        {
            if (consent == null)
                throw new ArgumentNullException(nameof(consent));

            var requiredMessage = consent.RequiredMessage;
            return new GdprConsentModel
            {
                Id = consent.Id,
                Message = consent.Message,
                IsRequired = consent.IsRequired,
                RequiredMessage = !string.IsNullOrEmpty(requiredMessage) ? requiredMessage : $"'{consent.Message}' is required",
                Accepted = accepted
            };
        }

        #endregion

        #region Methods

        public virtual LoginModel PrepareLoginModel(bool? checkoutAsGuest)
        {
            var model = new LoginModel
            {
                UsernamesEnabled = InovatiqaDefaults.UsernamesEnabled,
                RegistrationTypeId = InovatiqaDefaults.Standard,
                CheckoutAsGuest = checkoutAsGuest.GetValueOrDefault(),
                DisplayCaptcha = InovatiqaDefaults.ShowOnLoginPage
            };
            return model;
        }

        public virtual RegisterModel PrepareRegisterModel(RegisterModel model, bool excludeProperties,
            string overrideCustomCustomerAttributesXml = "", bool setDefaultValues = false)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AllowCustomersToSetTimeZone = InovatiqaDefaults.AllowCustomersToSetTimeZone;

            model.DisplayVatNumber = InovatiqaDefaults.EuVatEnabled;
            model.FirstNameEnabled = InovatiqaDefaults.FirstNameEnabled;
            model.LastNameEnabled = InovatiqaDefaults.LastNameEnabled;
            model.FirstNameRequired = InovatiqaDefaults.FirstNameRequired;
            model.LastNameRequired = InovatiqaDefaults.LastNameRequired;
            model.GenderEnabled = InovatiqaDefaults.GenderEnabled;
            model.DateOfBirthEnabled = InovatiqaDefaults.DateOfBirthEnabled;
            model.DateOfBirthRequired = InovatiqaDefaults.DateOfBirthRequired;
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
            model.CountryRequired = InovatiqaDefaults.CountryRequired;
            model.StateProvinceEnabled = InovatiqaDefaults.StateProvinceEnabled;
            model.StateProvinceRequired = InovatiqaDefaults.StateProvinceRequired;
            model.PhoneEnabled = InovatiqaDefaults.PhoneEnabled;
            model.PhoneRequired = InovatiqaDefaults.PhoneRequired;
            model.FaxEnabled = InovatiqaDefaults.FaxEnabled;
            model.FaxRequired = InovatiqaDefaults.FaxRequired;
            model.NewsletterEnabled = InovatiqaDefaults.NewsletterEnabled;
            model.AcceptPrivacyPolicyEnabled = InovatiqaDefaults.AcceptPrivacyPolicyEnabled;
            model.AcceptPrivacyPolicyPopup = InovatiqaDefaults.PopupForTermsOfServiceLinks;
            model.UsernamesEnabled = InovatiqaDefaults.UsernamesEnabled;
            model.CheckUsernameAvailabilityEnabled = InovatiqaDefaults.CheckUsernameAvailabilityEnabled;
            model.HoneypotEnabled = InovatiqaDefaults.HoneypotEnabled;
            model.DisplayCaptcha = InovatiqaDefaults.Enabled && InovatiqaDefaults.ShowOnRegistrationPage;
            model.EnteringEmailTwice = InovatiqaDefaults.EnteringEmailTwice;
            model.Newsletter = InovatiqaDefaults.NewsletterTickedByDefault;

            if (InovatiqaDefaults.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = "Select country", Value = "0" });

                foreach (var c in _countryService.GetAllCountries(InovatiqaDefaults.LanguageId))
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
                    var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId, InovatiqaDefaults.LanguageId).ToList();
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = "Select state", Value = "0" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
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

            var customAttributes = PrepareCustomCustomerAttributes(_workContextService.CurrentCustomer, overrideCustomCustomerAttributesXml);
            foreach (var attribute in customAttributes)
                model.CustomerAttributes.Add(attribute);

            return model;
        }

        public virtual IList<CustomerAttributeModel> PrepareCustomCustomerAttributes(Customer customer, string overrideAttributesXml = "")
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var result = new List<CustomerAttributeModel>();

            var customerAttributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in customerAttributes)
            {
                var attributeModel = new CustomerAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.Name,
                    IsRequired = attribute.IsRequired,
                    AttributeControlTypeId = attribute.AttributeControlTypeId,
                };

                if (attribute.ShouldHaveValues())
                {
                    var attributeValues = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new CustomerAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(valueModel);
                    }
                }

                var selectedAttributesXml = !string.IsNullOrEmpty(overrideAttributesXml) ?
                    overrideAttributesXml :
                    _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CustomCustomerAttributes, customer.Id);
                switch (attribute.AttributeControlTypeId)
                {
                    case (int)AttributeControlType.DropdownList:
                    case (int)AttributeControlType.RadioList:
                    case (int)AttributeControlType.Checkboxes:
                        {
                            if (!string.IsNullOrEmpty(selectedAttributesXml))
                            {
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                var selectedValues = _customerAttributeParserService.ParseCustomerAttributeValues(selectedAttributesXml);
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
                            if (!string.IsNullOrEmpty(selectedAttributesXml))
                            {
                                var enteredText = _customerAttributeParserService.ParseValues(selectedAttributesXml, attribute.Id);
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

                result.Add(attributeModel);
            }

            return result;
        }

        public virtual RegisterResultModel PrepareRegisterResultModel(int resultId)
        {
            var resultText = "";
            switch (resultId)
            {
                case InovatiqaDefaults.Disabled:
                    resultText = "Registration not allowed. You can edit this in the admin area.";
                    break;
                case InovatiqaDefaults.Standard:
                    resultText = "Your registration completed";
                    break;
                case InovatiqaDefaults.AdminApproval:
                    resultText = "Your account will be activated after approving by administrator.";
                    break;
                case InovatiqaDefaults.EmailValidation:
                    resultText = "Your registration has been successfully completed. You have just been sent an email containing activation instructions.";
                    break;
                default:
                    break;
            }
            var model = new RegisterResultModel
            {
                Result = resultText
            };
            return model;
        }

        public virtual CustomerInfoModel PrepareCustomerInfoModel(CustomerInfoModel model, Customer customer,
            bool excludeProperties, string overrideCustomCustomerAttributesXml = "")
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            model.AllowCustomersToSetTimeZone = InovatiqaDefaults.AllowCustomersToSetTimeZone;
            foreach (var tzi in _dateTimeHelperService.GetSystemTimeZones())
                model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (excludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == _dateTimeHelperService.CurrentTimeZone.Id) });

            if (!excludeProperties)
            {
                model.VatNumber = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.VatNumberAttribute, customer.Id);
                model.FirstName = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.FirstNameAttribute, customer.Id);
                model.LastName = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.LastNameAttribute, customer.Id);
                model.Gender = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.GenderAttribute, customer.Id);
                var dateOfBirth = _genericAttributeService.GetAttribute<DateTime?>(customer, InovatiqaDefaults.DateOfBirthAttribute, customer.Id);
                if (dateOfBirth.HasValue)
                {
                    model.DateOfBirthDay = dateOfBirth.Value.Day;
                    model.DateOfBirthMonth = dateOfBirth.Value.Month;
                    model.DateOfBirthYear = dateOfBirth.Value.Year;
                }
                model.Company = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CompanyAttribute, customer.Id);
                model.StreetAddress = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.StreetAddressAttribute, customer.Id);
                model.StreetAddress2 = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.StreetAddress2Attribute, customer.Id);
                model.ZipPostalCode = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.ZipPostalCodeAttribute, customer.Id);
                model.City = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CityAttribute, customer.Id);
                model.County = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CountyAttribute, customer.Id);
                model.CountryId = _genericAttributeService.GetAttribute<int>(customer, InovatiqaDefaults.CountryIdAttribute, customer.Id);
                model.StateProvinceId = _genericAttributeService.GetAttribute<int>(customer, InovatiqaDefaults.StateProvinceIdAttribute, customer.Id);
                model.Phone = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.PhoneAttribute, customer.Id);
                model.Fax = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.FaxAttribute, customer.Id);

                var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, InovatiqaDefaults.StoreId);
                model.Newsletter = newsletter != null && newsletter.Active;

                model.Signature = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.SignatureAttribute, customer.Id);

                model.Email = customer.Email;
                model.Username = customer.Username;
            }
            else
            {
                if (InovatiqaDefaults.UsernamesEnabled && !InovatiqaDefaults.AllowUsersToChangeUsernames)
                    model.Username = customer.Username;
            }

            model.EmailToRevalidate = customer.EmailToRevalidate;

            if (InovatiqaDefaults.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = "Select country", Value = "0" });
                foreach (var c in _countryService.GetAllCountries(InovatiqaDefaults.LanguageId))
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
                    var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId, InovatiqaDefaults.LanguageId).ToList();
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = "Select state", Value = "0" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
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

            model.DisplayVatNumber = InovatiqaDefaults.EuVatEnabled;
            model.VatNumberStatusNote = _genericAttributeService
                .GetAttribute<string>(customer, InovatiqaDefaults.VatNumberStatusIdAttribute, customer.Id);
            model.FirstNameEnabled = InovatiqaDefaults.FirstNameEnabled;
            model.LastNameEnabled = InovatiqaDefaults.LastNameEnabled;
            model.FirstNameRequired = InovatiqaDefaults.FirstNameRequired;
            model.LastNameRequired = InovatiqaDefaults.LastNameRequired;
            model.GenderEnabled = InovatiqaDefaults.GenderEnabled;
            model.DateOfBirthEnabled = InovatiqaDefaults.DateOfBirthEnabled;
            model.DateOfBirthRequired = InovatiqaDefaults.DateOfBirthRequired;
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
            model.CountryRequired = InovatiqaDefaults.CountryRequired;
            model.StateProvinceEnabled = InovatiqaDefaults.StateProvinceEnabled;
            model.StateProvinceRequired = InovatiqaDefaults.StateProvinceRequired;
            model.PhoneEnabled = InovatiqaDefaults.PhoneEnabled;
            model.PhoneRequired = InovatiqaDefaults.PhoneRequired;
            model.FaxEnabled = InovatiqaDefaults.FaxEnabled;
            model.FaxRequired = InovatiqaDefaults.FaxRequired;
            model.NewsletterEnabled = InovatiqaDefaults.NewsletterEnabled;
            model.UsernamesEnabled = InovatiqaDefaults.UsernamesEnabled;
            model.AllowUsersToChangeUsernames = InovatiqaDefaults.AllowUsersToChangeUsernames;
            model.CheckUsernameAvailabilityEnabled = InovatiqaDefaults.CheckUsernameAvailabilityEnabled;

            var customAttributes = PrepareCustomCustomerAttributes(customer, overrideCustomCustomerAttributesXml);
            foreach (var attribute in customAttributes)
                model.CustomerAttributes.Add(attribute);

            return model;
        }

        public virtual CustomerAddressListModel PrepareCustomerAddressListModel()
        {
            var customer = _workContextService.CurrentCustomer;
            List<int> ChildIDs = new List<int>();
            ChildIDs = _customerService.getAllChildAccounts(customer);  // if current customer is a child, then returns all siblings otherwise returns all childs
            var model = new CustomerAddressListModel();
            foreach(var id in ChildIDs)
            {
                var addresses = _customerService.GetAddressesByCustomerId(id);
                foreach (var address in addresses)
                {
                    var addressModel = new AddressModel();
                    _addressModelFactory.PrepareAddressModel(addressModel,
                        address: address,
                        excludeProperties: false);
                    addressModel.IsShippingAddress = (addressModel.Id == customer.ShippingAddressId);
                    addressModel.IsBillingAddress = (addressModel.Id == customer.BillingAddressId);
                    addressModel.CustomerId = id;
                    model.Addresses.Add(addressModel);
                }
            }
            
            return model;
        }

        public virtual PasswordRecoveryModel PreparePasswordRecoveryModel(PasswordRecoveryModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.DisplayCaptcha = InovatiqaDefaults.Enabled && InovatiqaDefaults.ShowOnForgotPasswordPage;

            return model;
        }

        public virtual PasswordRecoveryConfirmModel PreparePasswordRecoveryConfirmModel()
        {
            var model = new PasswordRecoveryConfirmModel();
            return model;
        }

        public virtual ChangePasswordModel PrepareChangePasswordModel()
        {
            var model = new ChangePasswordModel();
            return model;
        }
        public virtual Customer PrepareChildCustomerModel(IFormCollection collection)
        {
            var customer = _workContextService.CurrentCustomer;
            var model = new Customer
            {
                Email = collection["email"],
                Username = collection["email"],
                CustomerGuid = Guid.NewGuid(),
                Active = collection["isActive"] == "on",
                BillingAddressId = customer.BillingAddressId,
                ShippingAddressId = customer.ShippingAddressId,
                CreatedOnUtc = DateTime.UtcNow,
                ParentId = customer.Id,
            };
            if(collection["Subaccount_CAO"] == "on")
            {
                model.MaxOrderApprovalValue = collection["OrderApprovalValue"] != "" ? Convert.ToDecimal(collection["OrderApprovalValue"]) : 0;
            }
            if(collection["Subaccount_RABCO"] == "on")
            {
                model.MaxOrderWithoutApproval = collection["OrderApprovalValue"] != "" ? Convert.ToDecimal(collection["OrderPlacementValue"]) : 0;
            }
            return model;
        }
        public virtual Address PrepareChildAddressModel(IFormCollection collection)
        {
            var company = "";
            if(collection["Subaccount_FUPCN"] == "on")
            {
                var customer = _workContextService.CurrentCustomer;
                var address = _addressService.GetAddressById(Convert.ToInt32(customer.BillingAddressId));
                company = address.Company;
            }
            else
            {
                company = collection["company"];
            }
            //added by hamza
            var model = new Address
            {
                City = collection["city"],
                Address1 = collection["address1"],
                Address2 = collection["address2"],
                ZipPostalCode = collection["zip"],
                PhoneNumber = collection["phone"],
                FirstName = collection["firstName"],
                LastName = collection["lastName"],
                Email = collection["email"],
                StateProvinceId = int.Parse(collection["stateProvinceId"]),
                CountryId = int.Parse(collection["countryId"]),
                Company = company

            };
            return model;
        }
        public virtual ChildAccountModel GetAllChildAccounts()
        {
            var customer = _workContextService.CurrentCustomer;
            var model = new ChildAccountModel();
            var AllCustomers = _customerService.GetAllCustomers();
            if(customer.ParentId != null && customer.ParentId != 0) // customer is parent or child
            {
                foreach (var Customer in AllCustomers)
                {
                    if (Customer.ParentId == customer.ParentId)
                    {
                        model.Accounts.Add(new ChildAccountModel
                        {
                            Id = Customer.Id,
                            Name = Customer.Username
                        });
                    }
                }
            }
            else
            {
                foreach (var Customer in AllCustomers)
                {
                    if (Customer.ParentId == _workContextService.CurrentCustomer.Id)
                    {
                        model.Accounts.Add(new ChildAccountModel
                        {
                            Id = Customer.Id,
                            Name = Customer.Username
                        });
                    }
                }
            }
            
            return model;
        }
        public virtual ChildAccountModel PrepareChildDetailsModel(Customer model)
        {
            var address = _customerService.GetAddressesByCustomerId(model.Id).FirstOrDefault();
            var roles = _customerService.GetCustomerRoleMappingByCustomerId(model.Id);
            //changes by hamza
            var country = _countryService.GetCountryById(Convert.ToInt32(address.CountryId)).Name;
            var state = _stateProvinceService.GetStateProvinceById(Convert.ToInt32(address.StateProvinceId))?.Name ?? "";

            var childModel = new ChildAccountModel
            {
                Id = model.Id,
                email = model.Email,
                isActive = model.Active,
                firstName = address.FirstName,
                lastName = address.LastName,
                City = address.City,
                Address1 = address.Address1,
                Address2 = address.Address2,
                Zip = address.ZipPostalCode,
                Phone = address.PhoneNumber,
                Company = address.Company,
                Country = country,
                State = state,
                CountryId =Convert.ToInt32(address.CountryId),
                StateProvinceId = Convert.ToInt32(address.StateProvinceId),
                MaxOrderApprovalValue = model.MaxOrderApprovalValue.ToString(),
                MinValueToRequestApproval = model.MaxOrderWithoutApproval.ToString()
            };
            foreach(var role in roles)
            {
                if (role == "Subaccount_MAD")
                    childModel.Subaccount_MAD = true;
                else if (role == "Subaccount_MAB")
                    childModel.Subaccount_MAB = true;
                else if (role == "Subaccount_DAOH")
                    childModel.Subaccount_DAOH = true;
                else if (role == "Subaccount_CO")
                    childModel.Subaccount_CO = true;
                else if (role == "Subaccount_RABCO")
                    childModel.Subaccount_RABCO = true;
                else if (role == "Subaccount_GTCC")
                    childModel.Subaccount_GTCC = true;
                else if (role == "Subaccount_GTC")
                    childModel.Subaccount_GTC = true;
                else if (role == "Subaccount_OPN")
                    childModel.Subaccount_OPN = true;
                else if (role == "Subaccount_FUPCN")
                    childModel.Subaccount_FUPCN = true;
                else if (role == "Subaccount_FUPCVAT")
                    childModel.Subaccount_FUPCVAT = true;
                else if (role == "Subaccount_FUPA")
                    childModel.Subaccount_FUPA = true;
                else if (role == "Subaccount_CAO")
                    childModel.Subaccount_CAO = true;
                else if (role == "Subaccount_CMS")
                    childModel.Subaccount_CMS = true;
            }
            return childModel;
        }

        public virtual CustomerAccountInfoUpdateResultModel UpdateCustomerAccountInformationResultModel(CustomerAccountInfoUpdateResultModel model, IFormCollection form)
        {
            var customer = _workContextService.CurrentCustomer;
            var address = _addressService.GetAddressById(Convert.ToInt32(customer.BillingAddressId));
            if(form.ContainsKey("FirstName"))
            {
                address.FirstName = form["FirstName"];
                address.LastName = form["LastName"];
                _addressService.UpdateAddress(address);
            }
            if (!string.IsNullOrEmpty(form["email"]) && form["email"] != customer.Email)
            {
                customer.Email = form["email"];
                _customerService.UpdateCustomer(customer);
                model.Message = "Email updated successfully.";
                model.MessageClass = "Success";
            }
            else if (!string.IsNullOrEmpty(form["password"]))
            {
                if (form["password"] != form["confirmPassword"])
                {
                    model.Message = "Password and confirm password do not match.";
                    model.MessageClass = "Danger";
                }
                else
                {
                    var response = _customerRegistrationService.ChangePassword(new ChangePasswordRequest(customer.Email,
                    false, InovatiqaDefaults.Hashed, form["password"]));
                    if (response.Success)
                    {
                        model.Message = "Password updated successfully. Please use new password to login.";
                        model.MessageClass = "success";
                    }
                }
            }
            if (!string.IsNullOrEmpty(form["password"]) && !string.IsNullOrEmpty(form["email"]) && form["password"] == form["confirmPassword"])
            {
                model.Message = "Email and password updated successfully.";
                model.MessageClass = "success";
            }

            model.Email = customer.Email;
            model.FirstName = address.FirstName;
            model.LastName = address.LastName;
            return model;
        }

        #endregion
    }
}