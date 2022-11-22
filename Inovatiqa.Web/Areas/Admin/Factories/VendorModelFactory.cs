using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Services.Vendors;
using Inovatiqa.Services.Vendors.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Web.Areas.Admin.Models.Common;
using Inovatiqa.Web.Areas.Admin.Models.Vendors;
using Inovatiqa.Web.Extensions;
using Inovatiqa.Web.Framework.Factories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial class VendorModelFactory : IVendorModelFactory
    {
        #region Fields

        private readonly IAddressAttributeModelFactory _addressAttributeModelFactory;
        private readonly IAddressService _addressService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelperService _dateTimeHelperService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorAttributeParserService _vendorAttributeParser;
        private readonly IVendorAttributeService _vendorAttributeService;
        private readonly IVendorService _vendorService;

        #endregion

        #region Ctor

        public VendorModelFactory(IAddressAttributeModelFactory addressAttributeModelFactory,
            IAddressService addressService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICustomerService customerService,
            IDateTimeHelperService dateTimeHelperService,
            IGenericAttributeService genericAttributeService,
            ILocalizedModelFactory localizedModelFactory,
            IUrlRecordService urlRecordService,
            IVendorAttributeParserService vendorAttributeParser,
            IVendorAttributeService vendorAttributeService,
            IVendorService vendorService)
        {
            _addressAttributeModelFactory = addressAttributeModelFactory;
            _addressService = addressService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _customerService = customerService;
            _dateTimeHelperService = dateTimeHelperService;
            _genericAttributeService = genericAttributeService;
            _localizedModelFactory = localizedModelFactory;
            _urlRecordService = urlRecordService;
            _vendorAttributeParser = vendorAttributeParser;
            _vendorAttributeService = vendorAttributeService;
            _vendorService = vendorService;
        }

        #endregion

        #region Utilities

        protected virtual void PrepareAssociatedCustomerModels(IList<VendorAssociatedCustomerModel> models, Vendor vendor)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            var associatedCustomers = _customerService.GetAllCustomers(vendorId: vendor.Id);
            foreach (var customer in associatedCustomers)
            {
                models.Add(new VendorAssociatedCustomerModel
                {
                    Id = customer.Id,
                    Email = customer.Email
                });
            }
        }

        protected virtual void PrepareVendorAttributeModels(IList<VendorModel.VendorAttributeModel> models, Vendor vendor)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            var vendorAttributes = _vendorAttributeService.GetAllVendorAttributes();
            foreach (var attribute in vendorAttributes)
            {
                var attributeModel = new VendorModel.VendorAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.Name,
                    IsRequired = attribute.IsRequired,
                    AttributeControlTypeId = attribute.AttributeControlTypeId
                };

                if (attribute.ShouldHaveValues())
                {
                    var attributeValues = _vendorAttributeService.GetVendorAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new VendorModel.VendorAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                if (vendor != null)
                {
                    var selectedVendorAttributes = _genericAttributeService.GetAttribute<string>(vendor, InovatiqaDefaults.VendorAttributes, vendor.Id);
                    switch (attribute.AttributeControlTypeId)
                    {
                        case (int)AttributeControlType.DropdownList:
                        case (int)AttributeControlType.RadioList:
                        case (int)AttributeControlType.Checkboxes:
                            {
                                if (!string.IsNullOrEmpty(selectedVendorAttributes))
                                {
                                    foreach (var item in attributeModel.Values)
                                        item.IsPreSelected = false;

                                    var selectedValues = _vendorAttributeParser.ParseVendorAttributeValues(selectedVendorAttributes);
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
                                if (!string.IsNullOrEmpty(selectedVendorAttributes))
                                {
                                    var enteredText = _vendorAttributeParser.ParseValues(selectedVendorAttributes, attribute.Id);
                                    if (enteredText.Any())
                                        attributeModel.DefaultValue = enteredText[0];
                                }
                            }
                            break;
                        case (int)AttributeControlType.Datepicker:
                        case (int)AttributeControlType.ColorSquares:
                        case (int)AttributeControlType.ImageSquares:
                        case (int)AttributeControlType.FileUpload:
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

            model.CountryEnabled = true;
            model.StateProvinceEnabled = true;
            model.CountyEnabled = true;
            model.CityEnabled = true;
            model.StreetAddressEnabled = true;
            model.StreetAddress2Enabled = true;
            model.ZipPostalCodeEnabled = true;
            model.PhoneEnabled = true;
            model.FaxEnabled = true;

            _baseAdminModelFactory.PrepareCountries(model.AvailableCountries);

            _baseAdminModelFactory.PrepareStatesAndProvinces(model.AvailableStates, model.CountryId);

            _addressAttributeModelFactory.PrepareCustomAddressAttributes(model.CustomAddressAttributes, address);
        }

        protected virtual VendorNoteSearchModel PrepareVendorNoteSearchModel(VendorNoteSearchModel searchModel, Vendor vendor)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            searchModel.VendorId = vendor.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        #endregion

        #region Methods

        public virtual VendorSearchModel PrepareVendorSearchModel(VendorSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual VendorListModel PrepareVendorListModel(VendorSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var vendors = _vendorService.GetAllVendors(showHidden: true,
                name: searchModel.SearchName,
                email: searchModel.SearchEmail,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new VendorListModel().PrepareToGrid(searchModel, vendors, () =>
            {
                return vendors.Select(vendor =>
                {
                    var vendorModel = vendor.ToVendorModel<VendorModel>();
                    vendorModel.SeName = _urlRecordService.GetActiveSlug(vendor.Id, InovatiqaDefaults.VendorSlugName, InovatiqaDefaults.LanguageId);

                    return vendorModel;
                });
            });

            return model;
        }

        public virtual VendorModel PrepareVendorModel(VendorModel model, Vendor vendor, bool excludeProperties = false)
        {
            Action<VendorLocalizedModel, int> localizedModelConfiguration = null;

            if (vendor != null)
            {
                if (model == null)
                {
                    model = vendor.ToVendorModel<VendorModel>();
                    model.SeName = _urlRecordService.GetActiveSlug(vendor.Id, InovatiqaDefaults.VendorSlugName, InovatiqaDefaults.LanguageId);
                }

                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = vendor.Name;
                    locale.Description = vendor.Description;
                    locale.MetaKeywords = vendor.MetaKeywords;
                    locale.MetaDescription = vendor.MetaDescription;
                    locale.MetaTitle = vendor.MetaTitle;
                    locale.SeName = _urlRecordService.GetActiveSlug(vendor.Id, InovatiqaDefaults.VendorSlugName, InovatiqaDefaults.LanguageId);
                };

                PrepareAssociatedCustomerModels(model.AssociatedCustomers, vendor);

                PrepareVendorNoteSearchModel(model.VendorNoteSearchModel, vendor);
            }

            if (vendor == null)
            {
                model.PageSize = 6;
                model.Active = true;
                model.AllowCustomersToSelectPageSize = true;
                model.PageSizeOptions = InovatiqaDefaults.PageSizeOptions;
            }

            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            PrepareVendorAttributeModels(model.VendorAttributes, vendor);

            var address = _addressService.GetAddressById(vendor?.AddressId ?? 0);
            if (!excludeProperties && address != null)
                model.Address = address.ToAddressModel(model.Address);
            PrepareAddressModel(model.Address, address);

            return model;
        }
        #endregion
    }
}