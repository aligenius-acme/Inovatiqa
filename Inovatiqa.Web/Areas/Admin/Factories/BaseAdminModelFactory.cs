using System;
using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Core.Caching;
using Inovatiqa.Services;
using Inovatiqa.Services.Caching.Interfaces;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.Vendors.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial class BaseAdminModelFactory : IBaseAdminModelFactory
    {
        #region Fields

        private readonly IShippingService _shippingService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IDateRangeService _dateRangeService;
        private readonly IVendorService _vendorService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICategoryTemplateService _categoryTemplateService;
        private readonly IManufacturerTemplateService _manufacturerTemplateService;
        private readonly IDateTimeHelperService _dateTimeHelperService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly ICacheKeyService _cacheKeyService;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public BaseAdminModelFactory(IShippingService shippingService,
            IProductTemplateService productTemplateService,
            IDateRangeService dateRangeService,
            IVendorService vendorService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            ICategoryTemplateService categoryTemplateService,
             IManufacturerTemplateService manufacturerTemplateService,
             IDateTimeHelperService dateTimeHelperService,
             ICustomerActivityService customerActivityService,
             ICustomerService customerService,
             ICacheKeyService cacheKeyService,
            IStaticCacheManager staticCacheManager)
        {
            _shippingService = shippingService;
            _productTemplateService = productTemplateService;
            _dateRangeService = dateRangeService;
            _vendorService = vendorService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _categoryTemplateService = categoryTemplateService;
            _manufacturerTemplateService = manufacturerTemplateService;
            _dateTimeHelperService = dateTimeHelperService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _cacheKeyService = cacheKeyService;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Utilities

        protected virtual List<SelectListItem> GetManufacturerList(bool showHidden = true)
        {
            var manufacturers = _manufacturerService.GetAllManufacturers(showHidden: showHidden);

            var result = new List<SelectListItem>();
            foreach (var m in manufacturers)
            {
                result.Add(new SelectListItem
                {
                    Text = m.Name,
                    Value = m.Id.ToString()
                });
            }

            return result;
        }

        protected virtual List<SelectListItem> GetCategoryList(bool showHidden = true)
        {
            //var categories = _categoryService.GetAllCategories(showHidden: showHidden);
            var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(InovatiqaDefaults.CategoriesListKey, showHidden);
            var listItems = _staticCacheManager.Get(cacheKey, () =>
            {
                var categories = _categoryService.GetAllCategories(showHidden: showHidden);
                return categories.Select(c => new SelectListItem
                {
                    Text = _categoryService.GetFormattedBreadCrumb(c, categories),
                    Value = c.Id.ToString()
                });
            });

            //var result = new List<SelectListItem>();
            //foreach (var c in categories)
            //{
            //    result.Add(new SelectListItem
            //    {
            //        Text = _categoryService.GetFormattedBreadCrumb(c, categories),
            //        Value = c.Id.ToString()
            //    });
            //}

            //return result;

            var result = new List<SelectListItem>();
            //clone the list to ensure that "selected" property is not set
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            return result;
        }

        protected virtual List<SelectListItem> GetVendorList(bool showHidden = true)
        {
            var vendors = _vendorService.GetAllVendors(showHidden: showHidden);
            var result = new List<SelectListItem>();
            foreach (var v in vendors)
            {
                result.Add(new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                });
            }

            return result;
        }

        protected virtual void PrepareDefaultItem(IList<SelectListItem> items, bool withSpecialDefaultItem, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (!withSpecialDefaultItem)
                return;

            const string value = "0";

            defaultItemText ??= "All";

            items.Insert(0, new SelectListItem { Text = defaultItemText, Value = value });
        }

        #endregion

        #region Methods

        public virtual void PrepareActivityLogTypes(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableActivityTypes = _customerActivityService.GetAllActivityTypes();
            foreach (var activityType in availableActivityTypes)
            {
                items.Add(new SelectListItem { Value = activityType.Id.ToString(), Text = activityType.Name });
            }

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PrepareShoppingCartTypes(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableShoppingCartTypeItems = ShoppingCartType.ShoppingCart.ToSelectList(false);
            foreach (var shoppingCartTypeItem in availableShoppingCartTypeItems)
            {
                items.Add(shoppingCartTypeItem);
            }

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PreparePaymentModes(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availablePaymentModes = PaymentModes.CreditCard.ToSelectList(false);

            foreach (var availablePaymentMode in availablePaymentModes)
            {
                items.Add(availablePaymentMode);
            }

            ////////foreach (var e in Enum.GetValues(typeof(PaymentModes)))
            ////////{
            ////////    var text = string.Empty;
            ////////    if ((int)e == (int)PaymentModes.CreditCard)
            ////////        text = "Credit Card";
            ////////    else if ((int)e == (int)PaymentModes.PaymentTerms)
            ////////        text = "Payment Terms";
            ////////    var statusItem = new SelectListItem
            ////////    {
            ////////        Disabled = false,
            ////////        Selected = false,
            ////////        Text = text,
            ////////        Value = ((int)e).ToString()
            ////////    };
            ////////    items.Add(statusItem);
            ////////}
            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }
        public virtual void PreparePaymentTerms(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availablePaymentTerms = PaymentTerms.Net30.ToSelectList(false);

            foreach (var availablePaymentTerm in availablePaymentTerms)
            {
                items.Add(availablePaymentTerm);
            }

            ////////foreach (var e in Enum.GetValues(typeof(PaymentTerms)))
            ////////{
            ////////    var statusItem = new SelectListItem
            ////////    {
            ////////        Disabled = false,
            ////////        Selected = false,
            ////////        Text = e.ToString(),
            ////////        Value = ((int)e).ToString()
            ////////    };
            ////////    items.Add(statusItem);
            ////////}

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PrepareOrderStatuses(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            foreach (var e in Enum.GetValues(typeof(OrderStatus)))
            {
                var statusItem = new SelectListItem
                {
                    Disabled = false,
                    Selected = false,
                    Text = e.ToString(),
                    Value = ((int)e).ToString()
                };
                items.Add(statusItem);
            }

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PreparePaymentStatuses(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            foreach (var e in Enum.GetValues(typeof(PaymentStatus)))
            {
                var statusItem = new SelectListItem
                {
                    Disabled = false,
                    Selected = false,
                    Text = e.ToString(),
                    Value = ((int)e).ToString()
                };
                items.Add(statusItem);
            }

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PrepareShippingStatuses(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            foreach (var e in Enum.GetValues(typeof(ShippingStatus)))
            {
                var statusItem = new SelectListItem
                {
                    Disabled = false,
                    Selected = false,
                    Text = e.ToString(),
                    Value = ((int)e).ToString()
                };
                items.Add(statusItem);
            }

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PrepareStores(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var store = new SelectListItem
            {
                Disabled = false,
                Selected = true,
                Text = InovatiqaDefaults.CurrentStoreName,
                Value = InovatiqaDefaults.StoreId.ToString()
            };
            items.Add(store);

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PrepareProductTypes(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var productType = new SelectListItem
            {
                Disabled = false,
                Selected = true,
                Text = Enum.GetName(typeof(ProductType), ProductType.SimpleProduct),
                Value = ProductType.SimpleProduct.ToString()
            };
            items.Add(productType);

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PreparePaymentMethods(IList<SelectListItem> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var paymenType = new SelectListItem
            {
                Disabled = false,
                Selected = true,
                Text = "All",
                Value= ""
            };
            items.Add(paymenType);

            paymenType = new SelectListItem
            {
                Disabled = false,
                Selected = false,
                Text = "Purchase Order",
                Value = InovatiqaDefaults.PurchaseOrderPaymentName
            };
            items.Add(paymenType);

            paymenType = new SelectListItem
            {
                Disabled = false,
                Selected = false,
                Text = "Credit Card",
                Value = InovatiqaDefaults.SystemName
            };
            items.Add(paymenType);

            PrepareDefaultItem(items, true, null);
        }

        public virtual void PrepareCategories(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableCategoryItems = GetCategoryList();
            foreach (var categoryItem in availableCategoryItems)
            {
                items.Add(categoryItem);
            }
            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PrepareProductTemplates(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableTemplates = _productTemplateService.GetAllProductTemplates();
            foreach (var template in availableTemplates)
            {
                items.Add(new SelectListItem { Value = template.Id.ToString(), Text = template.Name });
            }

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }
        public virtual void PrepareWarehouses(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableWarehouses = _shippingService.GetAllWarehouses();
            foreach (var warehouse in availableWarehouses)
            {
                items.Add(new SelectListItem { Value = warehouse.Id.ToString(), Text = warehouse.Name });
            }

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PrepareDeliveryDates(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableDeliveryDates = _dateRangeService.GetAllDeliveryDates();
            foreach (var date in availableDeliveryDates)
            {
                items.Add(new SelectListItem { Value = date.Id.ToString(), Text = date.Name });
            }

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PrepareProductAvailabilityRanges(IList<SelectListItem> items,
            bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableProductAvailabilityRanges = _dateRangeService.GetAllProductAvailabilityRanges();
            foreach (var range in availableProductAvailabilityRanges)
            {
                items.Add(new SelectListItem { Value = range.Id.ToString(), Text = range.Name });
            }

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PrepareVendors(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableVendorItems = GetVendorList();
            foreach (var vendorItem in availableVendorItems)
            {
                items.Add(vendorItem);
            }

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PrepareManufacturers(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableManufacturerItems = GetManufacturerList();
            foreach (var manufacturerItem in availableManufacturerItems)
            {
                items.Add(manufacturerItem);
            }

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PrepareCountries(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableCountries = _countryService.GetAllCountries(showHidden: true);
            foreach (var country in availableCountries)
            {
                items.Add(new SelectListItem { Value = country.Id.ToString(), Text = country.Name });
            }

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText ?? "Select country");
        }

        public virtual void PrepareStatesAndProvinces(IList<SelectListItem> items, int? countryId,
            bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (countryId.HasValue)
            {
                var availableStates = _stateProvinceService.GetStateProvincesByCountryId(countryId.Value, showHidden: true);
                foreach (var state in availableStates)
                {
                    items.Add(new SelectListItem { Value = state.Id.ToString(), Text = state.Name });
                }

                if (items.Any())
                    PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText ?? "Select state");
            }

            if (!items.Any())
                PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText ?? "Other");
        }

        public virtual void PrepareCategoryTemplates(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableTemplates = _categoryTemplateService.GetAllCategoryTemplates();
            foreach (var template in availableTemplates)
            {
                items.Add(new SelectListItem { Value = template.Id.ToString(), Text = template.Name });
            }
            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PrepareManufacturerTemplates(IList<SelectListItem> items,
            bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableTemplates = _manufacturerTemplateService.GetAllManufacturerTemplates();
            foreach (var template in availableTemplates)
            {
                items.Add(new SelectListItem { Value = template.Id.ToString(), Text = template.Name });
            }

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PrepareReturnRequestStatuses(IList<SelectListItem> items,
            bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableStatusItems = ReturnRequestStatus.Pending.ToSelectList(false);
            foreach (var statusItem in availableStatusItems)
            {
                items.Add(statusItem);
            }

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PrepareTimeZones(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableTimeZones = _dateTimeHelperService.GetSystemTimeZones();
            foreach (var timeZone in availableTimeZones)
            {
                items.Add(new SelectListItem { Value = timeZone.Id, Text = timeZone.DisplayName });
            }

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }

        public virtual void PrepareCustomerRoles(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableCustomerRoles = _customerService.GetAllCustomerRoles();
            foreach (var customerRole in availableCustomerRoles)
            {
                items.Add(new SelectListItem { Value = customerRole.Id.ToString(), Text = customerRole.Name });
            }

            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }
        #endregion
    }
}