using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Services.Media.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Web.Areas.Admin.Models.Common;
using Inovatiqa.Web.Areas.Admin.Models.Customers;
using Inovatiqa.Web.Areas.Admin.Models.ShoppingCart;
using Inovatiqa.Web.Extensions;
using Inovatiqa.Web.Framework.Factories.Interfaces;


namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial class CustomerModelFactory : ICustomerModelFactory
    {
        #region Fields
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IAddressAttributeFormatterService _addressAttributeFormatterService;
        private readonly IAddressAttributeModelFactory _addressAttributeModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICountryService _countryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerAttributeParserService _customerAttributeParserService;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelperService _dateTimeHelperService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeFormatterService _productAttributeFormatterService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IGeoLookupService _geoLookupService;


        #endregion

        #region Ctor

        public CustomerModelFactory(IAclSupportedModelFactory aclSupportedModelFactory,
            IAddressAttributeFormatterService addressAttributeFormatterService,
            IAddressAttributeModelFactory addressAttributeModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            ICustomerActivityService customerActivityService,
            ICustomerAttributeParserService customerAttributeParserService,
            ICustomerAttributeService customerAttributeService,
            ICustomerService customerService,
            IDateTimeHelperService dateTimeHelperService,
            IGenericAttributeService genericAttributeService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IOrderService orderService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductAttributeFormatterService productAttributeFormatterService,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            IGeoLookupService geoLookupService,
            IStateProvinceService stateProvinceService)
        {
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _addressAttributeFormatterService = addressAttributeFormatterService;
            _addressAttributeModelFactory = addressAttributeModelFactory;
            _baseAdminModelFactory = baseAdminModelFactory;
            _countryService = countryService;
            _customerActivityService = customerActivityService;
            _customerAttributeParserService = customerAttributeParserService;
            _customerAttributeService = customerAttributeService;
            _customerService = customerService;
            _dateTimeHelperService = dateTimeHelperService;
            _genericAttributeService = genericAttributeService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _orderService = orderService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productAttributeFormatterService = productAttributeFormatterService;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _stateProvinceService = stateProvinceService;
            _geoLookupService = geoLookupService;
        }

        #endregion

        #region Utilities

        protected virtual void PrepareCustomerAttributeModels(IList<CustomerModel.CustomerAttributeModel> models, Customer customer)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            var customerAttributes = _customerAttributeService.GetAllCustomerAttributes();
            var val = _customerAttributeService.GetCustomerAttributeValueById(21);
            foreach (var attribute in customerAttributes)
            {
                var attributeModel = new CustomerModel.CustomerAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.Name,
                    IsRequired = attribute.IsRequired,
                    AttributeControlTypeId = attribute.AttributeControlTypeId
                };

                if (attribute.ShouldHaveValues())
                {
                    var attributeValues = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new CustomerModel.CustomerAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                if (customer != null)
                {
                    var selectedCustomerAttributes = _genericAttributeService
                        .GetAttribute<string>(customer, InovatiqaDefaults.CustomCustomerAttributes, customer.Id);
                    switch (attribute.AttributeControlTypeId)
                    {
                        case (int)AttributeControlType.DropdownList:
                        case (int)AttributeControlType.RadioList:
                        case (int)AttributeControlType.Checkboxes:
                        {
                            if (!string.IsNullOrEmpty(selectedCustomerAttributes))
                            {
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                var selectedValues = _customerAttributeParserService.ParseCustomerAttributeValues(selectedCustomerAttributes);
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
                            if (!string.IsNullOrEmpty(selectedCustomerAttributes))
                            {
                                var enteredText = _customerAttributeParserService.ParseValues(selectedCustomerAttributes, attribute.Id);
                                if (enteredText.Any())
                                    attributeModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                        case (int)AttributeControlType.Datepicker:
                        case (int)AttributeControlType.ColorSquares:
                        case (int)AttributeControlType.ImageSquares:
                        case (int)AttributeControlType.FileUpload:
                        {
                            var enteredText = _customerAttributeParserService.ParseValues(selectedCustomerAttributes, attribute.Id);
                            if (enteredText.Any())
                            {
                                attributeModel.DefaultValue = enteredText[0].Replace("<![CDATA[" ,"").Replace("]]>", "");
                            }
                        }
                        break;
                        default:
                            break;
                    }
                }

                models.Add(attributeModel);
            }
        }

        protected virtual void PrepareAddressModel(AddressModel model, Address address)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.FirstNameEnabled = true;
            model.FirstNameRequired = true;
            model.LastNameEnabled = true;
            model.LastNameRequired = true;
            model.EmailEnabled = true;
            model.EmailRequired = true;
            model.CompanyEnabled = InovatiqaDefaults.CompanyEnabled;
            model.CompanyRequired = InovatiqaDefaults.CompanyRequired;
            model.CountryEnabled = InovatiqaDefaults.CountryEnabled;
            model.CountryRequired = InovatiqaDefaults.CountryEnabled;     
            model.StateProvinceEnabled = InovatiqaDefaults.StateProvinceEnabled;
            model.CityEnabled = InovatiqaDefaults.CityEnabled;
            model.CityRequired = InovatiqaDefaults.CityRequired;
            model.CountyEnabled = InovatiqaDefaults.CountyEnabled;
            model.CountyRequired = InovatiqaDefaults.CountyRequired;
            model.StreetAddressEnabled = InovatiqaDefaults.StreetAddressEnabled;
            model.StreetAddressRequired = InovatiqaDefaults.StreetAddressRequired;
            model.StreetAddress2Enabled = InovatiqaDefaults.StreetAddress2Enabled;
            model.StreetAddress2Required = InovatiqaDefaults.StreetAddress2Required;
            model.ZipPostalCodeEnabled = InovatiqaDefaults.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = InovatiqaDefaults.ZipPostalCodeRequired;
            model.PhoneEnabled = InovatiqaDefaults.PhoneEnabled;
            model.PhoneRequired = InovatiqaDefaults.PhoneRequired;
            model.FaxEnabled = InovatiqaDefaults.FaxEnabled;
            model.FaxRequired = InovatiqaDefaults.FaxRequired;

            _baseAdminModelFactory.PrepareCountries(model.AvailableCountries);

            _baseAdminModelFactory.PrepareStatesAndProvinces(model.AvailableStates, model.CountryId);

            _addressAttributeModelFactory.PrepareCustomAddressAttributes(model.CustomAddressAttributes, address);
        }

        protected virtual void PrepareModelAddressHtml(AddressModel model, Address address)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var addressHtmlSb = new StringBuilder("<div>");

            if (InovatiqaDefaults.CompanyEnabled && !string.IsNullOrEmpty(model.Company))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Company));

            if (InovatiqaDefaults.StreetAddressEnabled && !string.IsNullOrEmpty(model.Address1))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Address1));

            if (InovatiqaDefaults.StreetAddress2Enabled && !string.IsNullOrEmpty(model.Address2))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Address2));

            if (InovatiqaDefaults.CityEnabled && !string.IsNullOrEmpty(model.City))
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.City));

            if (InovatiqaDefaults.CountyEnabled && !string.IsNullOrEmpty(model.County))
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.County));

            if (InovatiqaDefaults.StateProvinceEnabled && !string.IsNullOrEmpty(model.StateProvinceName))
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.StateProvinceName));

            if (InovatiqaDefaults.ZipPostalCodeEnabled && !string.IsNullOrEmpty(model.ZipPostalCode))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.ZipPostalCode));

            if (InovatiqaDefaults.CountryEnabled && !string.IsNullOrEmpty(model.CountryName))
                addressHtmlSb.AppendFormat("{0}", WebUtility.HtmlEncode(model.CountryName));

            var customAttributesFormatted = _addressAttributeFormatterService.FormatAttributes(address?.CustomAttributes);
            if (!string.IsNullOrEmpty(customAttributesFormatted))
            {
                addressHtmlSb.AppendFormat("<br />{0}", customAttributesFormatted);
            }

            addressHtmlSb.Append("</div>");

            model.AddressHtml = addressHtmlSb.ToString();
        }

        protected virtual CustomerRewardPointsSearchModel PrepareRewardPointsSearchModel(CustomerRewardPointsSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual CustomerAddressSearchModel PrepareCustomerAddressSearchModel(CustomerAddressSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual CustomerOrderSearchModel PrepareCustomerOrderSearchModel(CustomerOrderSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual CustomerShoppingCartSearchModel PrepareCustomerShoppingCartSearchModel(CustomerShoppingCartSearchModel searchModel,
            Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            searchModel.ShoppingCartTypeId = (int)ShoppingCartType.ShoppingCart;
            _baseAdminModelFactory.PrepareShoppingCartTypes(searchModel.AvailableShoppingCartTypes, false);

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual CustomerActivityLogSearchModel PrepareCustomerActivityLogSearchModel(CustomerActivityLogSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual CustomerBackInStockSubscriptionSearchModel PrepareCustomerBackInStockSubscriptionSearchModel(
            CustomerBackInStockSubscriptionSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }
    
        #endregion

        #region Methods

        public virtual CustomerSearchModel PrepareCustomerSearchModel(CustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.UsernamesEnabled = InovatiqaDefaults.UsernamesEnabled;
            searchModel.AvatarEnabled = false;
            searchModel.FirstNameEnabled = InovatiqaDefaults.FirstNameEnabled;
            searchModel.LastNameEnabled = InovatiqaDefaults.LastNameEnabled;
            searchModel.DateOfBirthEnabled = InovatiqaDefaults.DateOfBirthEnabled;
            searchModel.CompanyEnabled = InovatiqaDefaults.CompanyEnabled;
            searchModel.PhoneEnabled = InovatiqaDefaults.PhoneEnabled;
            searchModel.ZipPostalCodeEnabled = InovatiqaDefaults.ZipPostalCodeEnabled;

            var registeredRole = _customerService.GetCustomerRoleBySystemName(InovatiqaDefaults.RegisteredRoleName);
            if (registeredRole != null)
                searchModel.SelectedCustomerRoleIds.Add(registeredRole.Id);

            _aclSupportedModelFactory.PrepareModelCustomerRoles(searchModel);

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual CustomerListModel PrepareCustomerListModel(CustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            int.TryParse(searchModel.SearchDayOfBirth, out var dayOfBirth);
            int.TryParse(searchModel.SearchMonthOfBirth, out var monthOfBirth);

            var customers = _customerService.GetAllCustomers(customerRoleIds: searchModel.SelectedCustomerRoleIds.ToArray(),
                email: searchModel.SearchEmail,
                username: searchModel.SearchUsername,
                firstName: searchModel.SearchFirstName,
                lastName: searchModel.SearchLastName,
                dayOfBirth: dayOfBirth,
                monthOfBirth: monthOfBirth,
                company: searchModel.SearchCompany,
                phone: searchModel.SearchPhone,
                zipPostalCode: searchModel.SearchZipPostalCode,
                ipAddress: searchModel.SearchIpAddress,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new CustomerListModel().PrepareToGrid(searchModel, customers, () =>
            {
                return customers.Select(customer =>
                {
                    var customerModel = customer.ToCustomerModel<CustomerModel>();

                    customerModel.Email = _customerService.IsRegistered(customer) ? customer.Email : "Guest";
                    customerModel.FullName = _customerService.GetCustomerFullName(customer);
                    customerModel.Company = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CompanyAttribute, customer.Id);
                    customerModel.Phone = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.PhoneAttribute, customer.Id);
                    customerModel.ZipPostalCode = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.ZipPostalCodeAttribute, customer.Id);

                    customerModel.CreatedOn = _dateTimeHelperService.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc);
                    customerModel.LastActivityDate = _dateTimeHelperService.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);

                    customerModel.CustomerRoleNames = string.Join(", ", _customerService.GetCustomerRoles(customer).Select(role => role.Name));
                    //if (InovatiqaDefaults.AllowCustomersToUploadAvatars)
                    //{
                    //    var avatarPictureId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute);
                    //    customerModel.AvatarUrl = _pictureService.GetPictureUrl(avatarPictureId, _mediaSettings.AvatarPictureSize,
                    //        _customerSettings.DefaultAvatarEnabled, defaultPictureType: PictureType.Avatar);
                    //}

                    return customerModel;
                });
            });

            return model;
        }

        public virtual CustomerModel PrepareCustomerModel(CustomerModel model, Customer customer, bool excludeProperties = false)
        {
            if (customer != null)
            {
                model ??= new CustomerModel();

                model.Id = customer.Id;
                model.DisplayVatNumber = InovatiqaDefaults.EuVatEnabled;
                model.AllowSendingOfPrivateMessage = _customerService.IsRegistered(customer);
                model.AllowSendingOfWelcomeMessage = _customerService.IsRegistered(customer);
                model.AllowReSendingOfActivationMessage = _customerService.IsRegistered(customer) && !customer.Active;
                model.GdprEnabled = false;

                if (customer.PaymentModeId != null)
                    model.PaymentModeId = int.Parse(customer.PaymentModeId.ToString());

                if (customer.PaymentTermsId != null)
                    model.PaymentTermsId = int.Parse(customer.PaymentTermsId.ToString());

                if (customer.CreditLimit != null)
                    model.CreditLimit = decimal.Parse(customer.CreditLimit.ToString());

                if (!excludeProperties)
                {
                    model.Email = customer.Email;
                    model.Username = customer.Username;
                    model.VendorId = customer.VendorId;
                    model.AdminComment = customer.AdminComment;
                    model.IsTaxExempt = customer.IsTaxExempt;
                    model.Active = customer.Active;
                    model.FirstName = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.FirstNameAttribute, customer.Id);
                    model.LastName = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.LastNameAttribute, customer.Id);
                    model.Gender = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.GenderAttribute, customer.Id);
                    model.DateOfBirth = _genericAttributeService.GetAttribute<DateTime?>(customer, InovatiqaDefaults.DateOfBirthAttribute, customer.Id);
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
                    model.TimeZoneId = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.TimeZoneIdAttribute, customer.Id);
                    model.VatNumber = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.VatNumberAttribute, customer.Id);
                    model.VatNumberStatusNote = "";
                    model.CreatedOn = _dateTimeHelperService.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc);
                    model.LastActivityDate = _dateTimeHelperService.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);
                    model.LastIpAddress = customer.LastIpAddress;
                    model.LastVisitedPage = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.LastVisitedPageAttribute, customer.Id);
                    model.SelectedCustomerRoleIds = _customerService.GetCustomerRoleIds(customer).ToList();

                    

                    model.RegisteredInStore = InovatiqaDefaults.CurrentStoreName;
                    model.DisplayRegisteredInStore = false;
                   
                    //var affiliate = _affiliateService.GetAffiliateById(customer.AffiliateId);
                    //if (affiliate != null)
                    //{
                    //    model.AffiliateId = affiliate.Id;
                    //    model.AffiliateName = _affiliateService.GetAffiliateFullName(affiliate);
                    //}

                    //if (!string.IsNullOrEmpty(customer.Email))
                    //{
                    //    model.SelectedNewsletterSubscriptionStoreIds = new List<int>();
                    //    model.SelectedNewsletterSubscriptionStoreIds.Add(InovatiqaDefaults.StoreId);
                    //}
                }
                model.DisplayRewardPointsHistory = false;
                //if (model.DisplayRewardPointsHistory)
                //    PrepareAddRewardPointsToCustomerModel(model.AddRewardPoints);

                PrepareRewardPointsSearchModel(model.CustomerRewardPointsSearchModel, customer);
                PrepareCustomerAddressSearchModel(model.CustomerAddressSearchModel, customer);
                PrepareCustomerOrderSearchModel(model.CustomerOrderSearchModel, customer);
                PrepareCustomerShoppingCartSearchModel(model.CustomerShoppingCartSearchModel, customer);
                PrepareCustomerActivityLogSearchModel(model.CustomerActivityLogSearchModel, customer);
                PrepareCustomerBackInStockSubscriptionSearchModel(model.CustomerBackInStockSubscriptionSearchModel, customer);
                //PrepareCustomerAssociatedExternalAuthRecordsSearchModel(model.CustomerAssociatedExternalAuthRecordsSearchModel, customer);
            }
            else
            {
                if (!excludeProperties)
                {
                    var registeredRole = _customerService.GetCustomerRoleBySystemName(InovatiqaDefaults.RegisteredRoleName);
                    if (registeredRole != null)
                        model.SelectedCustomerRoleIds.Add(registeredRole.Id);
                }
            }

            model.UsernamesEnabled = InovatiqaDefaults.UsernamesEnabled;
            model.AllowCustomersToSetTimeZone = InovatiqaDefaults.AllowCustomersToSetTimeZone;
            model.FirstNameEnabled = InovatiqaDefaults.FirstNameEnabled;
            model.LastNameEnabled = InovatiqaDefaults.LastNameEnabled;
            model.GenderEnabled = InovatiqaDefaults.GenderEnabled;
            model.DateOfBirthEnabled = InovatiqaDefaults.DateOfBirthEnabled;
            model.CompanyEnabled = InovatiqaDefaults.CompanyEnabled;
            model.StreetAddressEnabled = InovatiqaDefaults.StreetAddressEnabled;
            model.StreetAddress2Enabled = InovatiqaDefaults.StreetAddress2Enabled;
            model.ZipPostalCodeEnabled = InovatiqaDefaults.ZipPostalCodeEnabled;
            model.CityEnabled = InovatiqaDefaults.CityEnabled;
            model.CountyEnabled = InovatiqaDefaults.CountyEnabled;
            model.CountryEnabled = InovatiqaDefaults.CountryEnabled;
            model.StateProvinceEnabled = InovatiqaDefaults.StateProvinceEnabled;
            model.PhoneEnabled = InovatiqaDefaults.PhoneEnabled;
            model.FaxEnabled = InovatiqaDefaults.FaxEnabled;

            if (customer == null)
            {
                model.Active = true;
                model.DisplayVatNumber = false;
            }

            _baseAdminModelFactory.PrepareVendors(model.AvailableVendors,
                defaultItemText: "Not a vendor");

            _baseAdminModelFactory.PreparePaymentModes(model.AvailablePaymentModes, false);

            _baseAdminModelFactory.PreparePaymentTerms(model.AvailablePaymentTerms, false);

            PrepareCustomerAttributeModels(model.CustomerAttributes, customer);

            //model.AvailableNewsletterSubscriptionStores = _storeService.GetAllStores().Select(store => new SelectListItem
            //{
            //    Value = store.Id.ToString(),
            //    Text = store.Name,
            //    Selected = model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id)
            //}).ToList();

            _aclSupportedModelFactory.PrepareModelCustomerRoles(model);

            _baseAdminModelFactory.PrepareTimeZones(model.AvailableTimeZones, false);

            if (InovatiqaDefaults.CountryEnabled)
            {
                _baseAdminModelFactory.PrepareCountries(model.AvailableCountries);
                if (InovatiqaDefaults.StateProvinceEnabled)
                    _baseAdminModelFactory.PrepareStatesAndProvinces(model.AvailableStates, model.CountryId == 0 ? null : (int?)model.CountryId);
            }
            //changes by hamza for exception handling
            if(model.CustomerAttributes.Count > 0)
            {
                model.AttachmentUploaded = model.CustomerAttributes.Any(attribute => attribute.Name == "Attachment" && attribute.DefaultValue != null);
                model.AttachmentURL = model.CustomerAttributes.Where(attribute => attribute.Name == "Attachment").FirstOrDefault().DefaultValue;
            }
            return model;
        }

       
        public virtual CustomerAddressListModel PrepareCustomerAddressListModel(CustomerAddressSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var addresses = _customerService.GetAddressesByCustomerId(customer.Id)
                .OrderByDescending(address => address.CreatedOnUtc).ThenByDescending(address => address.Id).ToList()
                .ToPagedList(searchModel);

            var model = new CustomerAddressListModel().PrepareToGrid(searchModel, addresses, () =>
            {
                return addresses.Select(address =>
                {
                    var addressModel = address.ToAddressModel<AddressModel>();
                    addressModel.CountryName = _countryService.GetCountryByAddress(address)?.Name;
                    addressModel.StateProvinceName = _stateProvinceService.GetStateProvinceByAddress(address)?.Name;

                    PrepareModelAddressHtml(addressModel, address);

                    return addressModel;
                });
            });

            return model;
        }

        public virtual CustomerAddressModel PrepareCustomerAddressModel(CustomerAddressModel model,
            Customer customer, Address address, bool excludeProperties = false)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (address != null)
            {
                model ??= new CustomerAddressModel();

                if (!excludeProperties)
                    model.Address = address.ToAddressModel(model.Address);
            }

            model.CustomerId = customer.Id;

            PrepareAddressModel(model.Address, address);

            return model;
        }

        public virtual CustomerOrderListModel PrepareCustomerOrderListModel(CustomerOrderSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var orders = _orderService.SearchOrders(customerId: customer.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new CustomerOrderListModel().PrepareToGrid(searchModel, orders, () =>
            {
                return orders.Select(order =>
                {
                    var orderModel = order.ToCustomerOrderModel<CustomerOrderModel>();

                    orderModel.CreatedOn = _dateTimeHelperService.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);

                    orderModel.StoreName = InovatiqaDefaults.CurrentStoreName ?? "Unknown";
                    orderModel.OrderStatus = Enum.GetName(typeof(OrderStatus), order.OrderStatusId);
                    orderModel.PaymentStatus = Enum.GetName(typeof(PaymentStatus), order.PaymentStatusId);
                    orderModel.ShippingStatus = Enum.GetName(typeof(ShippingStatus), order.ShippingStatusId).Replace("NotYetShipped", "Not yet shipped").Replace("ShippingNotRequired", "Shipping not required").Replace("PartiallyShipped", "Partially shipped");
                    orderModel.OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal);

                    return orderModel;
                });
            });

            return model;
        }

        public virtual CustomerShoppingCartListModel PrepareCustomerShoppingCartListModel(CustomerShoppingCartSearchModel searchModel,
            Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var shoppingCart = _shoppingCartService.GetShoppingCart(customer, (int)searchModel.ShoppingCartTypeId)
                .ToPagedList(searchModel);

            var pageList = shoppingCart.Select(item =>
            {
                var shoppingCartItemModel = item.ToShoppingCartItemModel<ShoppingCartItemModel>();

                var product = _productService.GetProductById(item.ProductId);

                shoppingCartItemModel.ProductName = product.Name;
                shoppingCartItemModel.Store = InovatiqaDefaults .CurrentStoreName ?? "Unknown";
                shoppingCartItemModel.AttributeInfo =
                    _productAttributeFormatterService.FormatAttributes(product, item.AttributesXml);
                shoppingCartItemModel.UnitPrice = _priceFormatter.FormatPrice(_shoppingCartService.GetUnitPrice(item));
                shoppingCartItemModel.Total = _priceFormatter.FormatPrice(_shoppingCartService.GetSubTotal(item));
                shoppingCartItemModel.UpdatedOn =
                    _dateTimeHelperService.ConvertToUserTime(item.UpdatedOnUtc, DateTimeKind.Utc);

                return shoppingCartItemModel;
            }).ToList().ToPagedList(searchModel);

            var model = new CustomerShoppingCartListModel().PrepareToGrid(searchModel, pageList, () => pageList);

            return model;
        }

        public virtual CustomerActivityLogListModel PrepareCustomerActivityLogListModel(CustomerActivityLogSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var activityLog = _customerActivityService.GetAllActivities(customerId: customer.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var pageList = activityLog.Select(logItem =>
            {
                var customerActivityLogModel = logItem.ToCustomerActivityLogModel<CustomerActivityLogModel>();

                customerActivityLogModel.ActivityLogTypeName = _customerActivityService.GetActivityTypeById(logItem.ActivityLogTypeId)?.Name;

                customerActivityLogModel.CreatedOn =
                    _dateTimeHelperService.ConvertToUserTime(logItem.CreatedOnUtc, DateTimeKind.Utc);

                return customerActivityLogModel;
            }).ToList().ToPagedList(searchModel);

            var model = new CustomerActivityLogListModel().PrepareToGrid(searchModel, pageList, () => pageList);
            
            return model;
        }
  
        public virtual OnlineCustomerSearchModel PrepareOnlineCustomerSearchModel(OnlineCustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual OnlineCustomerListModel PrepareOnlineCustomerListModel(OnlineCustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var lastActivityFrom = DateTime.UtcNow.AddMinutes(-20);

            var customers = _customerService.GetOnlineCustomers(customerRoleIds: null,
                 lastActivityFromUtc: lastActivityFrom,
                 pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new OnlineCustomerListModel().PrepareToGrid(searchModel, customers, () =>
            {
                return customers.Select(customer =>
                {
                    var customerModel = customer.ToOnlineCustomerModel<OnlineCustomerModel>();

                    customerModel.LastActivityDate = _dateTimeHelperService.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);

                    customerModel.CustomerInfo = _customerService.IsRegistered(customer)
                        ? customer.Email : "Guest";
                    customerModel.LastIpAddress = true
                        ? customer.LastIpAddress : "Store IP addresses setting is disabled";
                    customerModel.Location = _geoLookupService.LookupCountryName(customer.LastIpAddress);
                    customerModel.LastVisitedPage = InovatiqaDefaults.StoreLastVisitedPage
                        ? _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.LastVisitedPageAttribute, customer.Id)
                        : "Store last visited page setting is disabled";

                    return customerModel;
                });
            });

            return model;
        }

        #endregion
    }
}