using System;
using System.Linq;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Discounts.Interfaces;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Catalog;
using Inovatiqa.Web.Framework.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Inovatiqa.Web.Extensions;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Core;

namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial class ManufacturerModelFactory : IManufacturerModelFactory
    {
        #region Fields

        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IManufacturerService _manufacturerService;
        private readonly IDiscountService _discountService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public ManufacturerModelFactory(IAclSupportedModelFactory aclSupportedModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            IManufacturerService manufacturerService,
            IDiscountService discountService,
            ILocalizedModelFactory localizedModelFactory,
            IProductService productService,
            IUrlRecordService urlRecordService)
        {
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _baseAdminModelFactory = baseAdminModelFactory;
            _manufacturerService = manufacturerService;
            _discountService = discountService;
            _localizedModelFactory = localizedModelFactory;
            _productService = productService;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Utilities

        protected virtual ManufacturerProductSearchModel PrepareManufacturerProductSearchModel(ManufacturerProductSearchModel searchModel,
            Manufacturer manufacturer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (manufacturer == null)
                throw new ArgumentNullException(nameof(manufacturer));

            searchModel.ManufacturerId = manufacturer.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }
        
        #endregion

        #region Methods

        public virtual ManufacturerProductListModel PrepareManufacturerProductListModel(ManufacturerProductSearchModel searchModel,
            Manufacturer manufacturer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (manufacturer == null)
                throw new ArgumentNullException(nameof(manufacturer));

            var productManufacturers = _manufacturerService.GetProductManufacturersByManufacturerId(showHidden: true,
                manufacturerId: manufacturer.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new ManufacturerProductListModel().PrepareToGrid(searchModel, productManufacturers, () =>
            {
                return productManufacturers.Select(productManufacturer =>
                {
                    var manufacturerProductModel = productManufacturer.ToManufacturerProductModel<ManufacturerProductModel>();

                    manufacturerProductModel.ProductName = _productService.GetProductById(productManufacturer.ProductId)?.Name;

                    return manufacturerProductModel;
                });
            });

            return model;
        }

        public virtual ManufacturerSearchModel PrepareManufacturerSearchModel(ManufacturerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            searchModel.HideStoresList = InovatiqaDefaults.HideStoresList;

            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "0",
                Text = "All"
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "1",
                Text = "Published only"
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "2",
                Text = "Unpublished only"
            });

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual ManufacturerListModel PrepareManufacturerListModel(ManufacturerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var manufacturers = _manufacturerService.GetAllManufacturers(showHidden: true,
                manufacturerName: searchModel.SearchManufacturerName,
                storeId: searchModel.SearchStoreId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                overridePublished: searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1));

            var model = new ManufacturerListModel().PrepareToGrid(searchModel, manufacturers, () =>
            {
                return manufacturers.Select(manufacturer =>
                {
                    var manufacturerModel = manufacturer.ToManufacturerModel<ManufacturerModel>();
                    manufacturerModel.SeName = _urlRecordService.GetActiveSlug(manufacturer.Id, InovatiqaDefaults.ManufacturerSlugName, InovatiqaDefaults.LanguageId);

                    return manufacturerModel;
                });
            });

            return model;
        }

        public virtual ManufacturerModel PrepareManufacturerModel(ManufacturerModel model,
            Manufacturer manufacturer, bool excludeProperties = false)
        {
            Action<ManufacturerLocalizedModel, int> localizedModelConfiguration = null;

            if (manufacturer != null)
            {
                if (model == null)
                {
                    model = manufacturer.ToManufacturerModel<ManufacturerModel>();
                    model.SeName = _urlRecordService.GetActiveSlug(manufacturer.Id, InovatiqaDefaults.ManufacturerSlugName, InovatiqaDefaults.LanguageId);
                }

                PrepareManufacturerProductSearchModel(model.ManufacturerProductSearchModel, manufacturer);

                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = manufacturer.Name;
                    locale.Description = manufacturer.Description;
                    locale.MetaKeywords = manufacturer.MetaKeywords;
                    locale.MetaDescription = manufacturer.MetaDescription;
                    locale.MetaTitle = manufacturer.MetaTitle;
                    locale.SeName = _urlRecordService.GetActiveSlug(manufacturer.Id, InovatiqaDefaults.ManufacturerSlugName, InovatiqaDefaults.LanguageId);
                };
            }

            if (manufacturer == null)
            {
                model.PageSize = InovatiqaDefaults.PageSize;
                model.PageSizeOptions = InovatiqaDefaults.PageSizeOptions;
                model.Published = true;
                //added by hamza
                model.ShowOnHomepage = true;
                model.AllowCustomersToSelectPageSize = true;
            }

            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            _baseAdminModelFactory.PrepareManufacturerTemplates(model.AvailableManufacturerTemplates, false);

            return model;
        }

        #endregion
    }
}