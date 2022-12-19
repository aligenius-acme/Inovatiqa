using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Catalog;
using Microsoft.AspNetCore.Mvc.Rendering;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Core;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Framework.Factories.Interfaces;
using Inovatiqa.Web.Extensions;
using Inovatiqa.Services.Media.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services;
using Inovatiqa.Services.Catalog.Extensions;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Orders;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;

namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial class ProductModelFactory : IProductModelFactory
    {
        #region Fields

        private readonly IUrlRecordService _urlRecordService;
        private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IMeasureService _measureService;
        private readonly IWorkContextService _workContextService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ISettingModelFactory _settingModelFactory;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IPictureService _pictureService;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IProductAttributeFormatterService _productAttributeFormatterService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductAttributeParserService _productAttributeParserService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IAddressService _addressService;
        private readonly IDateTimeHelperService _dateTimeHelperService;

        #endregion

        #region Ctor

        public ProductModelFactory(IUrlRecordService urlRecordService,
            IProductAttributeService productAttributeService,
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IMeasureService measureService,
            IWorkContextService workContextService,
            ISpecificationAttributeService specificationAttributeService,
            ILocalizedModelFactory localizedModelFactory,
            ISettingModelFactory settingModelFactory,
            IProductTemplateService productTemplateService,
            IPictureService pictureService,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IProductAttributeFormatterService productAttributeFormatterService,
            IProductAttributeParserService productAttributeParserService,
            IShoppingCartService shoppingCartService,
            ICustomerService customerService,
            IOrderService orderService,
            IAddressService addressService,
            IDateTimeHelperService dateTimeHelperService)
        { 
            _urlRecordService = urlRecordService;
            _productService = productService;
            _productAttributeService = productAttributeService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _measureService = measureService;
            _workContextService = workContextService;
            _specificationAttributeService = specificationAttributeService;
            _localizedModelFactory = localizedModelFactory;
            _settingModelFactory = settingModelFactory;
            _productTemplateService = productTemplateService;
            _pictureService = pictureService;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _productAttributeFormatterService = productAttributeFormatterService;
            _shoppingCartService = shoppingCartService;
            _productAttributeParserService = productAttributeParserService;
            _customerService = customerService;
            _orderService = orderService;
            _addressService = addressService;
            _dateTimeHelperService = dateTimeHelperService;
        }

        #endregion

        #region Utilities

        protected virtual void PrepareProductAttributeConditionModel(ProductAttributeConditionModel model,
            ProductProductAttributeMapping productAttributeMapping)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (productAttributeMapping == null)
                throw new ArgumentNullException(nameof(productAttributeMapping));

            model.ProductAttributeMappingId = productAttributeMapping.Id;
            model.EnableCondition = !string.IsNullOrEmpty(productAttributeMapping.ConditionAttributeXml);

            var selectedPva = _productAttributeParserService
                .ParseProductAttributeMappings(productAttributeMapping.ConditionAttributeXml)
                .FirstOrDefault();

            var attributes = _productAttributeService.GetProductAttributeMappingsByProductId(productAttributeMapping.ProductId)
                .Where(x => x.ProductAttributeCanBeUsedAsCondition())
                .Where(x => x.Id != productAttributeMapping.Id)
                .ToList();
            foreach (var attribute in attributes)
            {
                var attributeModel = new ProductAttributeConditionModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = _productAttributeService.GetProductAttributeById(attribute.ProductAttributeId).Name,
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlTypeId
                };

                if (attribute.ShouldHaveValues())
                {
                    var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new ProductAttributeConditionModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }

                    if (selectedPva != null && attribute.Id == selectedPva.Id)
                    {
                        model.SelectedProductAttributeId = selectedPva.Id;

                        switch (attribute.AttributeControlTypeId)
                        {
                            case (int)AttributeControlType.DropdownList:
                            case (int)AttributeControlType.RadioList:
                            case (int)AttributeControlType.Checkboxes:
                            case (int)AttributeControlType.ColorSquares:
                            case (int)AttributeControlType.ImageSquares:
                                if (!string.IsNullOrEmpty(productAttributeMapping.ConditionAttributeXml))
                                {
                                    foreach (var item in attributeModel.Values)
                                        item.IsPreSelected = false;

                                    var selectedValues =
                                        _productAttributeParserService.ParseProductAttributeValues(productAttributeMapping
                                            .ConditionAttributeXml);
                                    foreach (var attributeValue in selectedValues)
                                        foreach (var item in attributeModel.Values)
                                            if (attributeValue.Id == item.Id)
                                                item.IsPreSelected = true;
                                }

                                break;
                            case (int)AttributeControlType.ReadonlyCheckboxes:
                            case (int)AttributeControlType.TextBox:
                            case (int)AttributeControlType.MultilineTextbox:
                            case (int)AttributeControlType.Datepicker:
                            case (int)AttributeControlType.FileUpload:
                            default:
                                break;
                        }
                    }
                }

                model.ProductAttributes.Add(attributeModel);
            }
        }

        protected virtual ProductAttributeValueSearchModel PrepareProductAttributeValueSearchModel(ProductAttributeValueSearchModel searchModel,
            ProductProductAttributeMapping productAttributeMapping)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (productAttributeMapping == null)
                throw new ArgumentNullException(nameof(productAttributeMapping));

            searchModel.ProductAttributeMappingId = productAttributeMapping.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual CopyProductModel PrepareCopyProductModel(CopyProductModel model, Product product)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Id = product.Id;
            model.Name = string.Format("{0} - copy", product.Name);
            model.Published = true;
            model.CopyImages = true;

            return model;
        }

        protected virtual RelatedProductSearchModel PrepareRelatedProductSearchModel(RelatedProductSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            searchModel.ProductId = product.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual CrossSellProductSearchModel PrepareCrossSellProductSearchModel(CrossSellProductSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            searchModel.ProductId = product.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual AssociatedProductSearchModel PrepareAssociatedProductSearchModel(AssociatedProductSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            searchModel.ProductId = product.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual ProductPictureSearchModel PrepareProductPictureSearchModel(ProductPictureSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            searchModel.ProductId = product.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual ProductSpecificationAttributeSearchModel PrepareProductSpecificationAttributeSearchModel(
            ProductSpecificationAttributeSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            searchModel.ProductId = product.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual ProductOrderSearchModel PrepareProductOrderSearchModel(ProductOrderSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            searchModel.ProductId = product.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual TierPriceSearchModel PrepareTierPriceSearchModel(TierPriceSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            searchModel.EntityId = product.Id;
            searchModel.EntityName = "Product";
            

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual StockQuantityHistorySearchModel PrepareStockQuantityHistorySearchModel(StockQuantityHistorySearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            searchModel.ProductId = product.Id;

            _baseAdminModelFactory.PrepareWarehouses(searchModel.AvailableWarehouses);

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual ProductAttributeMappingSearchModel PrepareProductAttributeMappingSearchModel(ProductAttributeMappingSearchModel searchModel,
            Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            searchModel.ProductId = product.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual ProductAttributeCombinationSearchModel PrepareProductAttributeCombinationSearchModel(
            ProductAttributeCombinationSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            searchModel.ProductId = product.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        #endregion

        #region Methods
        //tier price change
        public virtual TierPriceModel PrepareTierPriceModel(TierPriceModel model,
            int EntityId, string EntityName, EntityTierPrice tierPrice, bool excludeProperties = false)
        {
            //if (product == null)
            //    throw new ArgumentNullException(nameof(product));

            if (tierPrice != null)
            {
                if (model == null)
                {
                    //model = tierPrice.ToTierPriceModel<TierPriceModel>();
                    model = new TierPriceModel();
                    model.EntityId = Convert.ToInt32(tierPrice.EntityId);
                    model.EntityName = tierPrice.EntityName;
                    model.Id = tierPrice.Id;
                    model.CustomerId = Convert.ToInt32(tierPrice.CustomerId > 0 ? tierPrice.CustomerId : (int?)null);
                    model.StartDateTimeUtc = tierPrice.StartDateTimeUtc;
                    model.EndDateTimeUtc = tierPrice.EndDateTimeUtc;
                    model.Rate = Convert.ToInt32(tierPrice.Rate);
                }
            }
            model.EntityName = EntityName;
            //_baseAdminModelFactory.PrepareCustomerRoles(model.AvailableCustomerRoles);

            return model;
        }

        public virtual ProductPictureListModel PrepareProductPictureListModel(ProductPictureSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productPictures = _productService.GetProductPicturesByProductId(product.Id).ToPagedList(searchModel);

            var model = new ProductPictureListModel().PrepareToGrid(searchModel, productPictures, () =>
            {
                return productPictures.Select(productPicture =>
                {
                    var productPictureModel = productPicture.ToProductPictureModel<ProductPictureModel>();
                    var picture = _pictureService.GetPictureById(productPicture.PictureId)
                                  ?? throw new Exception("Picture cannot be loaded");

                    productPictureModel.PictureUrl = _pictureService.GetPictureUrl(ref picture);
                    productPictureModel.OverrideAltAttribute = picture.AltAttribute;
                    productPictureModel.OverrideTitleAttribute = picture.TitleAttribute;

                    return productPictureModel;
                });
            });

            return model;
        }

        public virtual ProductListModel PrepareProductListModel(ProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var vendor = _workContextService.CurrentVendor;

            var overridePublished = searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1);
            if (vendor != null)
                searchModel.SearchVendorId = vendor.Id;
            var categoryIds = new List<int> { searchModel.SearchCategoryId };
            if (searchModel.SearchIncludeSubCategories && searchModel.SearchCategoryId > 0)
            {
                var childCategoryIds = _categoryService.GetChildCategoryIds(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
                categoryIds.AddRange(childCategoryIds);
            }

            var products = _productService.SearchProducts(showHidden: true,
                categoryIds: categoryIds,
                manufacturerId: searchModel.SearchManufacturerId,
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                warehouseId: searchModel.SearchWarehouseId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                overridePublished: overridePublished);

            var model = new ProductListModel().PrepareToGrid(searchModel, products, () =>
            {
                return products.Select(product =>
                {
                    var productModel = product.ToProductModel<ProductModel>();

                    productModel.FullDescription = string.Empty;

                    productModel.SeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId);
                    var defaultProductPicture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                    productModel.PictureThumbnailUrl = _pictureService.GetPictureUrl(ref defaultProductPicture, 75);
                    productModel.ProductTypeName = Enum.GetName(typeof(ProductType), product.ProductTypeId);
                    if (product.ProductTypeId == (int)ProductType.SimpleProduct && product.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock)
                        productModel.StockQuantityStr = _productService.GetTotalStockQuantity(product).ToString();

                    return productModel;
                });
            });

            return model;
        }

        public virtual ProductSearchModel PrepareProductSearchModel(ProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var vendor = _workContextService.CurrentVendor;

            searchModel.IsLoggedInAsVendor = vendor != null;
            searchModel.AllowVendorsToImportProducts = InovatiqaDefaults.AllowVendorsToImportProducts;

            //_baseAdminModelFactory.PrepareCategories(searchModel.AvailableCategories);

            _baseAdminModelFactory.PrepareManufacturers(searchModel.AvailableManufacturers);

            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            _baseAdminModelFactory.PrepareVendors(searchModel.AvailableVendors);

            _baseAdminModelFactory.PrepareProductTypes(searchModel.AvailableProductTypes);

            _baseAdminModelFactory.PrepareWarehouses(searchModel.AvailableWarehouses);

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

        public virtual ProductModel PrepareProductModel(ProductModel model, Product product, bool excludeProperties = false)
        {
            Action<ProductLocalizedModel, int> localizedModelConfiguration = null;

            if (product != null)
            {
                if (model == null)
                {
                    model = product.ToProductModel<ProductModel>();
                    model.SeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId);
                }

                var parentGroupedProduct = _productService.GetProductById(product.ParentGroupedProductId);
                if (parentGroupedProduct != null)
                {
                    model.AssociatedToProductId = product.ParentGroupedProductId;
                    model.AssociatedToProductName = parentGroupedProduct.Name;
                }

                model.LastStockQuantity = product.StockQuantity;
                model.ProductTags = "";
                model.ProductAttributesExist = _productAttributeService.GetAllProductAttributes().Any();

                model.CanCreateCombinations = _productAttributeService
                    .GetProductAttributeMappingsByProductId(product.Id).Any(pam => _productAttributeService.GetProductAttributeValues(pam.Id).Any());

                if (!excludeProperties)
                {
                    model.SelectedCategoryIds = _categoryService.GetProductCategoriesByProductId(product.Id, true)
                        .Select(productCategory => productCategory.CategoryId).ToList();
                    model.SelectedManufacturerIds = _manufacturerService.GetProductManufacturersByProductId(product.Id, true)
                        .Select(productManufacturer => productManufacturer.ManufacturerId).ToList();
                }

                PrepareCopyProductModel(model.CopyProductModel, product);

                PrepareRelatedProductSearchModel(model.RelatedProductSearchModel, product);
                PrepareCrossSellProductSearchModel(model.CrossSellProductSearchModel, product);
                PrepareAssociatedProductSearchModel(model.AssociatedProductSearchModel, product);
                PrepareProductPictureSearchModel(model.ProductPictureSearchModel, product);
                PrepareProductSpecificationAttributeSearchModel(model.ProductSpecificationAttributeSearchModel, product);
                PrepareProductOrderSearchModel(model.ProductOrderSearchModel, product);
                PrepareTierPriceSearchModel(model.TierPriceSearchModel, product);
                PrepareStockQuantityHistorySearchModel(model.StockQuantityHistorySearchModel, product);
                PrepareProductAttributeMappingSearchModel(model.ProductAttributeMappingSearchModel, product);
                PrepareProductAttributeCombinationSearchModel(model.ProductAttributeCombinationSearchModel, product);

                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = product.Name;
                    locale.FullDescription = product.FullDescription;
                    locale.ShortDescription = product.ShortDescription;
                    locale.MetaKeywords = product.MetaKeywords;
                    locale.MetaDescription = product.MetaDescription;
                    locale.MetaTitle = product.MetaTitle;
                    locale.SeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, languageId);
                };
            }

            if (product == null)
            {
                model.MaximumCustomerEnteredPrice = 1000;
                model.MaxNumberOfDownloads = 10;
                model.RecurringCycleLength = 100;
                model.RecurringTotalCycles = 10;
                model.RentalPriceLength = 1;
                model.StockQuantity = 10000;
                model.NotifyAdminForQuantityBelow = 1;
                model.OrderMinimumQuantity = 1;
                model.OrderMaximumQuantity = 10000;
                model.TaxCategoryId = InovatiqaDefaults.DefaultTaxCategoryId;
                model.UnlimitedDownloads = true;
                model.IsShipEnabled = true;
                model.AllowCustomerReviews = true;
                model.Published = true;
                model.VisibleIndividually = true;
            }

            var vendor = _workContextService.CurrentVendor;

            model.PrimaryStoreCurrencyCode = InovatiqaDefaults.CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(InovatiqaDefaults.BaseWeightId).Name;
            model.BaseDimensionIn = _measureService.GetMeasureDimensionById(InovatiqaDefaults.BaseDimensionId).Name;
            model.IsLoggedInAsVendor = vendor != null;
            model.HasAvailableSpecificationAttributes =
                _specificationAttributeService.GetSpecificationAttributesWithOptions().Any();

            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            model.ProductEditorSettingsModel = _settingModelFactory.PrepareProductEditorSettingsModel();

            _baseAdminModelFactory.PrepareProductTemplates(model.AvailableProductTemplates, false);

            var productTemplates = _productTemplateService.GetAllProductTemplates();
            //foreach (var productType in Enum.GetValues(typeof(ProductType)).OfType<ProductType>())
            //{
            //    model.ProductsTypesSupportedByProductTemplates.Add((int)productType, new List<SelectListItem>());
            //    foreach (var template in productTemplates)
            //    {
            //        var list = (IList<int>)TypeDescriptor.GetConverter(typeof(List<int>)).ConvertFrom(template.IgnoredProductTypes) ?? new List<int>();
            //        if (string.IsNullOrEmpty(template.IgnoredProductTypes) || !list.Contains((int)productType))
            //        {
            //            model.ProductsTypesSupportedByProductTemplates[(int)productType].Add(new SelectListItem
            //            {
            //                Text = template.Name,
            //                Value = template.Id.ToString()
            //            });
            //        }
            //    }
            //}

            model.ProductsTypesSupportedByProductTemplates.Add(5, new List<SelectListItem>());
            model.ProductsTypesSupportedByProductTemplates[5].Add(new SelectListItem
            {
                Text = "Simple product",
                Value = "1"
            });

            model.ProductsTypesSupportedByProductTemplates.Add(10, new List<SelectListItem>());
            model.ProductsTypesSupportedByProductTemplates[10].Add(new SelectListItem
            {
                Text = "Grouped product (with variants)",
                Value = "2"
            });

            _baseAdminModelFactory.PrepareDeliveryDates(model.AvailableDeliveryDates,
                defaultItemText: "None");

            _baseAdminModelFactory.PrepareProductAvailabilityRanges(model.AvailableProductAvailabilityRanges,
                defaultItemText: "None");

            _baseAdminModelFactory.PrepareVendors(model.AvailableVendors,
                defaultItemText: "No vendor");

            //_baseAdminModelFactory.PrepareTaxCategories(model.AvailableTaxCategories);

            //_baseAdminModelFactory.PrepareWarehouses(model.AvailableWarehouses,
            //    defaultItemText: _localizationService.GetResource("Admin.Catalog.Products.Fields.Warehouse.None"));
            //PrepareProductWarehouseInventoryModels(model.ProductWarehouseInventoryModels, product);

            var availableMeasureWeights = _measureService.GetAllMeasureWeights()
                .Select(weight => new SelectListItem { Text = weight.Name, Value = weight.Id.ToString() }).ToList();
            model.AvailableBasepriceUnits = availableMeasureWeights;
            model.AvailableBasepriceBaseUnits = availableMeasureWeights;

            ////////////////////////////////_baseAdminModelFactory.PrepareCategories(model.AvailableCategories, false);
            ////////////////////////////////foreach (var categoryItem in model.AvailableCategories)
            ////////////////////////////////{
            ////////////////////////////////    categoryItem.Selected = int.TryParse(categoryItem.Value, out var categoryId)
            ////////////////////////////////        && model.SelectedCategoryIds.Contains(categoryId);
            ////////////////////////////////}

            _baseAdminModelFactory.PrepareManufacturers(model.AvailableManufacturers, false);
            foreach (var manufacturerItem in model.AvailableManufacturers)
            {
                manufacturerItem.Selected = int.TryParse(manufacturerItem.Value, out var manufacturerId)
                    && model.SelectedManufacturerIds.Contains(manufacturerId);
            }


            //var availableDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, showHidden: true);
            //_discountSupportedModelFactory.PrepareModelDiscounts(model, product, availableDiscounts, excludeProperties);

            //prepare model customer roles
            ////////////////////////////////////////////////////////////////_aclSupportedModelFactory.PrepareModelCustomerRoles(model, product, excludeProperties, product.Id);

            //_storeMappingSupportedModelFactory.PrepareModelStores(model, product, excludeProperties);

            //var productTags = _productTagService.GetAllProductTags();
            //var productTagsSb = new StringBuilder();
            //productTagsSb.Append("var initialProductTags = [");
            //for (var i = 0; i < productTags.Count; i++)
            //{
            //    var tag = productTags[i];
            //    productTagsSb.Append("'");
            //    productTagsSb.Append(JavaScriptEncoder.Default.Encode(tag.Name));
            //    productTagsSb.Append("'");
            //    if (i != productTags.Count - 1)
            //        productTagsSb.Append(",");
            //}
            //productTagsSb.Append("]");

            //model.InitialProductTags = productTagsSb.ToString();

            return model;
        }

        public virtual ProductAttributeCombinationListModel PrepareProductAttributeCombinationListModel(
            ProductAttributeCombinationSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productAttributeCombinations = _productAttributeService
                .GetAllProductAttributeCombinations(product.Id).ToPagedList(searchModel);

            var model = new ProductAttributeCombinationListModel().PrepareToGrid(searchModel, productAttributeCombinations, () =>
            {
                return productAttributeCombinations.Select(combination =>
                {
                    var productAttributeCombinationModel = combination.ToProductAttributeCombinationModel<ProductAttributeCombinationModel>();

                    var customer = _workContextService.CurrentCustomer;

                    productAttributeCombinationModel.AttributesXml = _productAttributeFormatterService
                        .FormatAttributes(product, combination.AttributesXml, customer, "<br />", true, true, true, false);
                    var pictureThumbnailUrl = _pictureService.GetPictureUrl(combination.PictureId, 75, false);

                    if (string.IsNullOrEmpty(pictureThumbnailUrl))
                        pictureThumbnailUrl = _pictureService.GetDefaultPictureUrl(targetSize: 1);

                    productAttributeCombinationModel.PictureThumbnailUrl = pictureThumbnailUrl;
                    var warnings = _shoppingCartService.GetShoppingCartItemAttributeWarnings(customer,
                        (int)ShoppingCartType.ShoppingCart, product,
                        attributesXml: combination.AttributesXml,
                        ignoreNonCombinableAttributes: true).Aggregate(string.Empty, (message, warning) => $"{message}{warning}<br />");
                    productAttributeCombinationModel.Warnings = new List<string> { warnings };

                    return productAttributeCombinationModel;
                });
            });

            return model;
        }

        public virtual ProductSpecificationAttributeListModel PrepareProductSpecificationAttributeListModel(
            ProductSpecificationAttributeSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productSpecificationAttributes = _specificationAttributeService
                .GetProductSpecificationAttributes(product.Id).ToPagedList(searchModel);

            var model = new ProductSpecificationAttributeListModel().PrepareToGrid(searchModel, productSpecificationAttributes, () =>
            {
                return productSpecificationAttributes.Select(attribute =>
                {
                    var productSpecificationAttributeModel = attribute.ToProductSpecificationAttributeModel<ProductSpecificationAttributeModel>();

                    var specAttributeOption = _specificationAttributeService.GetSpecificationAttributeOptionById(attribute.SpecificationAttributeOptionId);
                    var specAttribute = _specificationAttributeService.GetSpecificationAttributeById(specAttributeOption.SpecificationAttributeId);

                    productSpecificationAttributeModel.AttributeTypeName = Enum.GetName(typeof(SpecificationAttributeType), attribute.AttributeTypeId);
                    productSpecificationAttributeModel.AttributeId = specAttribute.Id;
                    productSpecificationAttributeModel.AttributeName = specAttribute.Name;

                    switch (attribute.AttributeTypeId)
                    {
                        case (int)SpecificationAttributeType.Option:
                            productSpecificationAttributeModel.ValueRaw = WebUtility.HtmlEncode(specAttributeOption.Name);
                            productSpecificationAttributeModel.SpecificationAttributeOptionId = specAttributeOption.Id;
                            break;
                        case (int)SpecificationAttributeType.CustomText:
                            productSpecificationAttributeModel.ValueRaw = WebUtility.HtmlEncode(attribute.CustomValue);
                            break;
                        case (int)SpecificationAttributeType.CustomHtmlText:
                            productSpecificationAttributeModel.ValueRaw = attribute.CustomValue;
                            break;
                        case (int)SpecificationAttributeType.Hyperlink:
                            productSpecificationAttributeModel.ValueRaw = attribute.CustomValue;
                            break;
                    }

                    return productSpecificationAttributeModel;
                });
            });

            return model;
        }

        public virtual ProductAttributeMappingModel PrepareProductAttributeMappingModel(ProductAttributeMappingModel model,
            Product product, ProductProductAttributeMapping productAttributeMapping, bool excludeProperties = false)
        {
            Action<ProductAttributeMappingLocalizedModel, int> localizedModelConfiguration = null;

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (productAttributeMapping != null)
            {
                model ??= new ProductAttributeMappingModel
                {
                    Id = productAttributeMapping.Id
                };

                model.ProductAttribute = _productAttributeService.GetProductAttributeById(productAttributeMapping.ProductAttributeId).Name;
                model.AttributeControlType = Enum.GetName(typeof(AttributeControlType), productAttributeMapping.AttributeControlTypeId);

                if (!excludeProperties)
                {
                    model.ProductAttributeId = productAttributeMapping.ProductAttributeId;
                    model.TextPrompt = productAttributeMapping.TextPrompt;
                    model.IsRequired = productAttributeMapping.IsRequired;
                    model.AttributeControlTypeId = productAttributeMapping.AttributeControlTypeId;
                    model.DisplayOrder = productAttributeMapping.DisplayOrder;
                    model.ValidationMinLength = productAttributeMapping.ValidationMinLength;
                    model.ValidationMaxLength = productAttributeMapping.ValidationMaxLength;
                    model.ValidationFileAllowedExtensions = productAttributeMapping.ValidationFileAllowedExtensions;
                    model.ValidationFileMaximumSize = productAttributeMapping.ValidationFileMaximumSize;
                    model.DefaultValue = productAttributeMapping.DefaultValue;
                }

                model.ConditionAllowed = true;
                PrepareProductAttributeConditionModel(model.ConditionModel, productAttributeMapping);

                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.TextPrompt = productAttributeMapping.TextPrompt;
                    locale.DefaultValue = productAttributeMapping.DefaultValue;
                };

                PrepareProductAttributeValueSearchModel(model.ProductAttributeValueSearchModel, productAttributeMapping);
            }

            model.ProductId = product.Id;

            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            model.AvailableProductAttributes = _productAttributeService.GetAllProductAttributes().Select(productAttribute => new SelectListItem
            {
                Text = productAttribute.Name,
                Value = productAttribute.Id.ToString()
            }).ToList();

            return model;
        }
        //tier price change
        public virtual TierPriceListModel PrepareTierPriceListModel(TierPriceSearchModel searchModel, int EntityId, String EntityName)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //if (product == null)
            //    throw new ArgumentNullException(nameof(product));

            var tierPrices = _productService.GetTierPricesByProduct(EntityId, EntityName)
                .OrderBy(price => price.EntityId).ThenBy(price => price.CustomerId).ThenBy(price => price.Rate)
                .ToList().ToPagedList(searchModel);

            var model = new TierPriceListModel().PrepareToGrid(searchModel, tierPrices, () =>
            {
                return tierPrices.Select(price =>
                {
                    //var tierPriceModel = price.ToTierPriceModel<TierPriceModel>();
                    var tierPriceModel = new TierPriceModel();
                    tierPriceModel.Id = price.Id;
                    tierPriceModel.EntityId = Convert.ToInt32(price.EntityId);
                    tierPriceModel.EntityName = price.EntityName;
                    tierPriceModel.StartDateTimeUtc = price.StartDateTimeUtc;
                    tierPriceModel.EndDateTimeUtc = price.EndDateTimeUtc;
                    tierPriceModel.Rate = Convert.ToDecimal(price.Rate);
                    //tierPriceModel.Store = InovatiqaDefaults.CurrentStoreName;
                    tierPriceModel.CustomerId = price.CustomerId ?? 0;
                    var customer = _customerService.GetCustomerById(Convert.ToInt32(price.CustomerId));
                    var category = _categoryService.GetCategoryById(Convert.ToInt32(price.EntityId));
                    tierPriceModel.Customer = customer?.Email;
                    tierPriceModel.Category = category?.Name;
                    //    ? _customerService.GetCustomerRoleById(price.CustomerRoleId.Value).Name
                    //    : "All customer roles";

                    return tierPriceModel;
                });
            });

            return model;
        }

        public virtual ProductOrderListModel PrepareProductOrderListModel(ProductOrderSearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var orders = _orderService.SearchOrders(productId: searchModel.ProductId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new ProductOrderListModel().PrepareToGrid(searchModel, orders, () =>
            {
                return orders.Select(order =>
                {
                    var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

                    var orderModel = new OrderModel
                    {
                        Id = order.Id,
                        CustomerEmail = billingAddress.Email,
                        CustomOrderNumber = order.CustomOrderNumber
                    };

                    orderModel.CreatedOn = _dateTimeHelperService.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);

                    orderModel.StoreName = InovatiqaDefaults.CurrentStoreName;
                    orderModel.OrderStatus = Enum.GetName(typeof(OrderStatus), order.OrderStatusId);
                    orderModel.PaymentStatus = Enum.GetName(typeof(PaymentStatus), order.PaymentStatusId);
                    orderModel.ShippingStatus = Enum.GetName(typeof(ShippingStatus), order.ShippingStatusId).Replace("NotYetShipped", "Not yet shipped").Replace("ShippingNotRequired", "Shipping not required").Replace("PartiallyShipped", "Partially shipped");

                    return orderModel;
                });
            });

            return model;
        }

        #endregion
    }
}