using Inovatiqa.Database.Models;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Common;
using Inovatiqa.Web.Areas.Admin.Models.Shipping;
using Inovatiqa.Web.Extensions;
using Inovatiqa.Web.Framework.Factories.Interfaces;
using System;
using System.Linq;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial class ShippingModelFactory : IShippingModelFactory
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICountryService _countryService;
        private readonly IDateRangeService _dateRangeService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IShippingService _shippingService;
        private readonly IStateProvinceService _stateProvinceService;

        #endregion

        #region Ctor

        public ShippingModelFactory(IAddressService addressService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            IDateRangeService dateRangeService,
            ILocalizedModelFactory localizedModelFactory,
            IShippingService shippingService,
            IStateProvinceService stateProvinceService)
        {
            _addressService = addressService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _countryService = countryService;
            _dateRangeService = dateRangeService;
            _localizedModelFactory = localizedModelFactory;
            _shippingService = shippingService;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Utilities

        protected virtual void PrepareAddressModel(AddressModel model, Address address)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.CountryEnabled = true;
            model.CountryRequired = true;
            model.StateProvinceEnabled = true;
            model.CountyEnabled = true;
            model.CityEnabled = true;
            model.StreetAddressEnabled = true;
            model.ZipPostalCodeEnabled = true;
            model.ZipPostalCodeRequired = true;
            model.PhoneEnabled = true;

            _baseAdminModelFactory.PrepareCountries(model.AvailableCountries);

            _baseAdminModelFactory.PrepareStatesAndProvinces(model.AvailableStates, model.CountryId);
        }

        protected virtual DeliveryDateSearchModel PrepareDeliveryDateSearchModel(DeliveryDateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual ProductAvailabilityRangeSearchModel PrepareProductAvailabilityRangeSearchModel(ProductAvailabilityRangeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }
        
        #endregion

        #region Methods

        public virtual ShippingProviderSearchModel PrepareShippingProviderSearchModel(ShippingProviderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual ShippingMethodSearchModel PrepareShippingMethodSearchModel(ShippingMethodSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual ShippingMethodListModel PrepareShippingMethodListModel(ShippingMethodSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var shippingMethods = _shippingService.GetAllShippingMethods().ToPagedList(searchModel);

            var model = new ShippingMethodListModel().PrepareToGrid(searchModel, shippingMethods, () =>
            {
                return shippingMethods.Select(method => method.ToShippingMethodModel<ShippingMethodModel>());
            });

            return model;
        }

        public virtual ShippingMethodModel PrepareShippingMethodModel(ShippingMethodModel model,
            ShippingMethod shippingMethod, bool excludeProperties = false)
        {
            Action<ShippingMethodLocalizedModel, int> localizedModelConfiguration = null;

            if (shippingMethod != null)
            {
                model ??= shippingMethod.ToShippingMethodModel<ShippingMethodModel>();

                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = shippingMethod.Name;
                    locale.Description = shippingMethod.Description;
                };
            }

            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            return model;
        }

        public virtual DatesRangesSearchModel PrepareDatesRangesSearchModel(DatesRangesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            PrepareDeliveryDateSearchModel(searchModel.DeliveryDateSearchModel);
            PrepareProductAvailabilityRangeSearchModel(searchModel.ProductAvailabilityRangeSearchModel);

            return searchModel;
        }

        public virtual DeliveryDateListModel PrepareDeliveryDateListModel(DeliveryDateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var deliveryDates = _dateRangeService.GetAllDeliveryDates().ToPagedList(searchModel);

            var model = new DeliveryDateListModel().PrepareToGrid(searchModel, deliveryDates, () =>
            {
                return deliveryDates.Select(date => date.ToDeliveryDateModel<DeliveryDateModel>());
            });

            return model;
        }

        public virtual DeliveryDateModel PrepareDeliveryDateModel(DeliveryDateModel model, DeliveryDate deliveryDate, bool excludeProperties = false)
        {
            Action<DeliveryDateLocalizedModel, int> localizedModelConfiguration = null;

            if (deliveryDate != null)
            {
                model ??= deliveryDate.ToDeliveryDateModel<DeliveryDateModel>();

                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = deliveryDate.Name;
                };
            }

            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            return model;
        }

        public virtual ProductAvailabilityRangeListModel PrepareProductAvailabilityRangeListModel(ProductAvailabilityRangeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var productAvailabilityRanges = _dateRangeService.GetAllProductAvailabilityRanges().ToPagedList(searchModel);

            var model = new ProductAvailabilityRangeListModel().PrepareToGrid(searchModel, productAvailabilityRanges, () =>
            {
                return productAvailabilityRanges.Select(range => range.ToProductAvailabilityRangeModel<ProductAvailabilityRangeModel>());
            });

            return model;
        }

        public virtual ProductAvailabilityRangeModel PrepareProductAvailabilityRangeModel(ProductAvailabilityRangeModel model,
            ProductAvailabilityRange productAvailabilityRange, bool excludeProperties = false)
        {
            Action<ProductAvailabilityRangeLocalizedModel, int> localizedModelConfiguration = null;

            if (productAvailabilityRange != null)
            {
                model ??= productAvailabilityRange.ToProductAvailabilityRangeModel<ProductAvailabilityRangeModel>();

                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = productAvailabilityRange.Name;
                };
            }

            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            return model;
        }
        #endregion
    }
}