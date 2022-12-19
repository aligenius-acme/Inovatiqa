using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Catalog
{
    public partial class ProductService : IProductService
    {
        #region Fields

        protected readonly Database.Interfaces.IRepository<Product> _productRepository;
        protected readonly Database.Interfaces.IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        protected readonly IProductAttributeParserService _productAttributeParserService;
        protected readonly IDateRangeService _dateRangeService;
        protected readonly IProductAttributeService _productAttributeService;
        protected readonly Database.Interfaces.IRepository<ProductCategoryMapping> _productCategoryMappingRepository;
        protected readonly Database.Interfaces.IRepository<CrossSellProduct> _crossSellProductRepository;
        protected readonly Database.Interfaces.IRepository<ProductReview> _productReviewRepository;
        protected readonly Database.Interfaces.IRepository<ProductReviewHelpfulness> _productReviewHelpfulnessRepository;
        protected readonly Database.Interfaces.IRepository<StockQuantityHistory> _stockQuantityHistoryRepository;
        protected readonly IWorkContextService _workContextService;
        protected readonly Database.Interfaces.IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
        protected readonly Database.Interfaces.IRepository<Warehouse> _warehouseRepository;
        protected readonly Database.Interfaces.IRepository<Shipment> _shipmentRepository;
        protected readonly Database.Interfaces.IRepository<TierPrice> _tierPriceRepository;
        protected readonly Database.Interfaces.IRepository<EntityTierPrice> _entityTierPriceRepository;
        protected readonly Database.Interfaces.IRepository<ProductPictureMapping> _productPictureRepository;
        protected readonly ICustomerService _customerService;
        //protected readonly IElasticClient _elasticClient;
        protected readonly ICategoryService _categoryService;
        protected readonly IManufacturerService _manufacturerService;
        #endregion

        #region Ctor

        public ProductService(
            Database.Interfaces.IRepository<Product> productRepository,
            Database.Interfaces.IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
            IProductAttributeParserService productAttributeParserService,
            IDateRangeService dateRangeService,
            IProductAttributeService productAttributeService,
            Database.Interfaces.IRepository<ProductCategoryMapping> productCategoryMappingRepository,
            Database.Interfaces.IRepository<CrossSellProduct> crossSellProductRepository,
            Database.Interfaces.IRepository<ProductReview> productReviewRepository,
            Database.Interfaces.IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
            IWorkContextService workContextService,
            Database.Interfaces.IRepository<StockQuantityHistory> stockQuantityHistoryRepository,
            Database.Interfaces.IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
            Database.Interfaces.IRepository<Warehouse> warehouseRepository,
            Database.Interfaces.IRepository<Shipment> shipmentRepository,
            Database.Interfaces.IRepository<TierPrice> tierPriceRepository,
            Database.Interfaces.IRepository<EntityTierPrice> entityTierPriceRepository,
            Database.Interfaces.IRepository<ProductPictureMapping> productPictureRepository,
            ICustomerService customerService,
            //IElasticClient elasticClient,
            ICategoryService categoryService,
            IManufacturerService manufacturerService)
        {
            _productRepository = productRepository;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _productAttributeParserService = productAttributeParserService;
            _dateRangeService = dateRangeService;
            _productAttributeService = productAttributeService;
            _productCategoryMappingRepository = productCategoryMappingRepository;
            _crossSellProductRepository = crossSellProductRepository;
            _productReviewRepository = productReviewRepository;
            _productReviewHelpfulnessRepository = productReviewHelpfulnessRepository;
            _workContextService = workContextService;
            _stockQuantityHistoryRepository = stockQuantityHistoryRepository;
            _productAttributeCombinationRepository = productAttributeCombinationRepository;
            _warehouseRepository = warehouseRepository;
            _shipmentRepository = shipmentRepository;
            _tierPriceRepository = tierPriceRepository;
            _entityTierPriceRepository = entityTierPriceRepository;
            _productPictureRepository = productPictureRepository;
            _customerService = customerService;
            //_elasticClient = elasticClient;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
        }

        #endregion

        #region Utilities

        protected IList<TierPrice> CalculateProductPriceRoleBased(Product product, string role, int percentage)
        {
            List<TierPrice> tierPrices = new List<TierPrice>();
            var tierPrice = new TierPrice();

            if (role.Contains("_"))
            {
                var roleCategoryId = role.Split('_').LastOrDefault();
                if (roleCategoryId == product.RootCategoryId.ToString())
                {
                    var totalAmountToBeAdded = ((decimal)percentage / 100) * product.Price;
                    tierPrice.Price = product.Price + totalAmountToBeAdded;
                    tierPrices.Add(tierPrice);
                    return tierPrices;
                }
                else
                {
                    tierPrice.Price = product.Price;
                    tierPrices.Add(tierPrice);
                    return tierPrices;
                }
            }
            else
            {
                var totalAmountToBeAdded = ((decimal)percentage / 100) * product.Price;
                tierPrice.Price = product.Price + totalAmountToBeAdded;
                tierPrices.Add(tierPrice);
                return tierPrices;
            }
        }

        protected virtual string GetStockMessage(Product product, string stockMessage)
        {
            if (!product.DisplayStockAvailability)
                return string.Empty;

            var stockQuantity = GetTotalStockQuantity(product);
            if (stockQuantity > 0)
            {
                stockMessage = product.DisplayStockQuantity
                    ?
                    string.Format("{0} in stock", stockQuantity)
                    :
                    "In stock";
            }
            else
            {
                stockMessage = "Out of stock";
            }

            return stockMessage;
        }

        protected virtual string GeStockMessage(Product product, string attributesXml)
        {
            if (!product.DisplayStockAvailability)
                return string.Empty;

            string stockMessage;

            var combination = _productAttributeParserService.FindProductAttributeCombination(product, attributesXml);
            if (combination != null)
            {
                var stockQuantity = combination.StockQuantity;
                if (stockQuantity > 0)
                {
                    stockMessage = product.DisplayStockQuantity
                        ?
                        string.Format("{0} in stock", stockQuantity)
                        :
                        "In stock";
                }
                else if (combination.AllowOutOfStockOrders)
                {
                    stockMessage = "{0} in stock";
                }
                else
                {
                    var productAvailabilityRange =
                        _dateRangeService.GetProductAvailabilityRangeById(product.ProductAvailabilityRangeId);
                    stockMessage = productAvailabilityRange == null
                        ? "Out of stock"
                        : string.Format("Available in {0}",
                            productAvailabilityRange.Name);
                }
            }
            else
            {
                if (product.AllowAddingOnlyExistingAttributeCombinations)
                {
                    var productAvailabilityRange =
                        _dateRangeService.GetProductAvailabilityRangeById(product.ProductAvailabilityRangeId);
                    stockMessage = productAvailabilityRange == null
                        ? "Out of stock"
                        : string.Format("Available in {0}",
                            productAvailabilityRange.Name);
                }
                else
                {
                    stockMessage = !_productAttributeService.GetProductAttributeMappingsByProductId(product.Id)
                        .Any(pam => pam.IsRequired) ? "In stock" : "Please select required attribute(s)";
                }
            }

            return stockMessage;
        }

        protected virtual void GetSkuMpnGtin(Product product, string attributesXml,
            out string sku, out string manufacturerPartNumber, out string gtin)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            sku = null;
            manufacturerPartNumber = null;
            gtin = null;

            if (!string.IsNullOrEmpty(attributesXml) &&
                product.ManageInventoryMethodId == InovatiqaDefaults.ManageStockByAttributes)
            {
                var combination = _productAttributeParserService.FindProductAttributeCombination(product, attributesXml);
                if (combination != null)
                {
                    sku = combination.Sku;
                    manufacturerPartNumber = combination.ManufacturerPartNumber;
                    gtin = combination.Gtin;
                }
            }

            if (string.IsNullOrEmpty(sku))
                sku = product.Sku;
            if (string.IsNullOrEmpty(manufacturerPartNumber))
                manufacturerPartNumber = product.ManufacturerPartNumber;
            if (string.IsNullOrEmpty(gtin))
                gtin = product.Gtin;
        }

        #endregion

        #region Methods

        #region Products

        public virtual IList<Product> GetAssociatedProducts(int parentGroupedProductId,
            int storeId = 0, int vendorId = 0, bool showHidden = false)
        {
            var query = _productRepository.Query();
            query = query.Where(x => x.ParentGroupedProductId == parentGroupedProductId);
            if (!showHidden)
            {
                query = query.Where(x => x.Published);

                query = query.Where(p =>
                    (!p.AvailableStartDateTimeUtc.HasValue || p.AvailableStartDateTimeUtc.Value < DateTime.UtcNow) &&
                    (!p.AvailableEndDateTimeUtc.HasValue || p.AvailableEndDateTimeUtc.Value > DateTime.UtcNow));
            }
            if (vendorId > 0)
            {
                query = query.Where(p => p.VendorId == vendorId);
            }

            query = query.Where(x => !x.Deleted);
            query = query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id);

            var products = query.ToList();

            return products;
        }

        public virtual Product GetProductBySku(string sku)
        {
            if (string.IsNullOrEmpty(sku))
                return null;

            sku = sku.Trim();

            var query = from p in _productRepository.Query()
                        orderby p.Id
                        where !p.Deleted &&
                        p.Sku == sku
                        select p;
            var product = query.FirstOrDefault();

            return product;
        }

        public virtual void ReserveInventory(Product product, int quantity)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (quantity >= 0)
                throw new ArgumentException("Value must be negative.", nameof(quantity));

            var qty = -quantity;

            var productInventory = _productWarehouseInventoryRepository.Query().Where(pwi => pwi.ProductId == product.Id)
                .OrderByDescending(pwi => pwi.StockQuantity - pwi.ReservedQuantity)
                .ToList();

            if (productInventory.Count <= 0)
                return;

            foreach (var item in productInventory)
            {
                var selectQty = Math.Min(item.StockQuantity - item.ReservedQuantity, qty);
                item.ReservedQuantity += selectQty;
                qty -= selectQty;

                if (qty <= 0)
                    break;
            }

            if (qty > 0)
            {
                var pwi = productInventory[0];
                pwi.ReservedQuantity += qty;
            }

            UpdateProduct(product);
        }

        public virtual void UpdateProductWarehouseInventory(ProductWarehouseInventory pwi)
        {
            if (pwi == null)
                throw new ArgumentNullException(nameof(pwi));

            _productWarehouseInventoryRepository.Update(pwi);

            ////event notification
            //_eventPublisher.EntityUpdated(pwi);
        }

        public virtual void UpdateProductWarehouseInventory(IList<ProductWarehouseInventory> pwis)
        {
            if (pwis == null)
                throw new ArgumentNullException(nameof(pwis));

            if (!pwis.Any())
                return;

            _productWarehouseInventoryRepository.BulkUpdate(pwis.ToList());

            //////event notification
            ////foreach (var pwi in pwis)
            ////{
            ////    _eventPublisher.EntityUpdated(pwi);
            ////}
        }

        public virtual void UnblockReservedInventory(Product product, int quantity)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (quantity < 0)
                throw new ArgumentException("Value must be positive.", nameof(quantity));

            var productInventory = _productWarehouseInventoryRepository.Query().Where(pwi => pwi.ProductId == product.Id)
                .OrderByDescending(pwi => pwi.ReservedQuantity)
                .ThenByDescending(pwi => pwi.StockQuantity)
                .ToList();

            if (!productInventory.Any())
                return;

            var qty = quantity;

            foreach (var item in productInventory)
            {
                var selectQty = Math.Min(item.ReservedQuantity, qty);
                item.ReservedQuantity -= selectQty;
                qty -= selectQty;

                if (qty <= 0)
                    break;
            }

            if (qty > 0)
            {
                var pwi = productInventory[0];
                pwi.StockQuantity += qty;
            }

            UpdateProductWarehouseInventory(productInventory);
        }

        public virtual void AdjustInventory(Product product, int quantityToChange, string attributesXml = "", string message = "")
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (quantityToChange == 0)
                return;

            if (product.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock)
            {
                var prevStockQuantity = GetTotalStockQuantity(product);

                if (product.UseMultipleWarehouses)
                {
                    if (quantityToChange < 0)
                        ReserveInventory(product, quantityToChange);
                    else
                        UnblockReservedInventory(product, quantityToChange);
                }
                else
                {
                    product.StockQuantity += quantityToChange;
                    UpdateProduct(product);

                    AddStockQuantityHistoryEntry(product, quantityToChange, product.StockQuantity, product.WarehouseId, message);
                }

                if (quantityToChange < 0 && product.MinStockQuantity >= GetTotalStockQuantity(product))
                {
                    switch (product.LowStockActivityId)
                    {
                        case (int)LowStockActivity.DisableBuyButton:
                            product.DisableBuyButton = true;
                            product.DisableWishlistButton = true;
                            UpdateProduct(product);
                            break;
                        case(int) LowStockActivity.Unpublish:
                            product.Published = false;
                            UpdateProduct(product);
                            break;
                        default:
                            break;
                    }
                }
                if (InovatiqaDefaults.PublishBackProductWhenCancellingOrders)
                {
                    if (quantityToChange > 0 && prevStockQuantity <= product.MinStockQuantity && product.MinStockQuantity < GetTotalStockQuantity(product))
                    {
                        switch (product.LowStockActivityId)
                        {
                            case (int)LowStockActivity.DisableBuyButton:
                                product.DisableBuyButton = false;
                                product.DisableWishlistButton = false;
                                UpdateProduct(product);
                                break;
                            case (int)LowStockActivity.Unpublish:
                                product.Published = true;
                                UpdateProduct(product);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            if (product.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStockByAttributes)
            {
                var combination = _productAttributeParserService.FindProductAttributeCombination(product, attributesXml);
                if (combination != null)
                {
                    combination.StockQuantity += quantityToChange;
                    _productAttributeService.UpdateProductAttributeCombination(combination);

                    AddStockQuantityHistoryEntry(product, quantityToChange, combination.StockQuantity, message: message, combinationId: combination.Id);
                }
            }

            var attributeValues = _productAttributeParserService.ParseProductAttributeValues(attributesXml);
            foreach (var attributeValue in attributeValues)
            {
                if (attributeValue.AttributeValueTypeId != InovatiqaDefaults.AssociatedToProduct)
                    continue;

                var associatedProduct = GetProductById(attributeValue.AssociatedProductId);
                if (associatedProduct != null)
                {
                    AdjustInventory(associatedProduct, quantityToChange * attributeValue.Quantity, message);
                }
            }
        }

        public virtual IPagedList<Product> SearchProducts(
             out IList<int> filterableSpecificationAttributeOptionIds,
             bool loadFilterableSpecificationAttributeOptionIds = false,
             int pageIndex = 0,
             int pageSize = int.MaxValue,
             IList<int> categoryIds = null,
             int manufacturerId = 0,
             int storeId = 0,
             int vendorId = 0,
             int warehouseId = 0,
             ProductType? productType = null,
             bool visibleIndividuallyOnly = false,
             bool markedAsNewOnly = false,
             bool? featuredProducts = null,
             decimal? priceMin = null,
             decimal? priceMax = null,
             int productTagId = 0,
             string keywords = null,
             bool searchDescriptions = false,
             bool searchManufacturerPartNumber = true,
             bool searchSku = true,
             bool searchProductTags = false,
             int languageId = 0,
             IList<int> filteredSpecs = null,
             ProductSortingEnum orderBy = ProductSortingEnum.Position,
             bool showHidden = false,
             bool? overridePublished = null)
        {
            filterableSpecificationAttributeOptionIds = new List<int>();

            if (categoryIds != null && categoryIds.Contains(0))
                categoryIds.Remove(0);

            var commaSeparatedCategoryIds = categoryIds == null ? string.Empty : string.Join(",", categoryIds);

            var commaSeparatedSpecIds = string.Empty;

            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var pCategoryIds = SqlParameterHelper.GetStringParameter("CategoryIds", commaSeparatedCategoryIds);
            var pManufacturerId = SqlParameterHelper.GetInt32Parameter("ManufacturerId", manufacturerId);
            var pStoreId = SqlParameterHelper.GetInt32Parameter("StoreId", storeId);
            var pVendorId = SqlParameterHelper.GetInt32Parameter("VendorId", vendorId);
            var pWarehouseId = SqlParameterHelper.GetInt32Parameter("WarehouseId", warehouseId);
            var pProductTypeId = SqlParameterHelper.GetInt32Parameter("ProductTypeId", (int?)productType);
            var pVisibleIndividuallyOnly = SqlParameterHelper.GetBooleanParameter("VisibleIndividuallyOnly", visibleIndividuallyOnly);
            var pMarkedAsNewOnly = SqlParameterHelper.GetBooleanParameter("MarkedAsNewOnly", markedAsNewOnly);
            var pProductTagId = SqlParameterHelper.GetInt32Parameter("ProductTagId", productTagId);
            var pFeaturedProducts = SqlParameterHelper.GetBooleanParameter("FeaturedProducts", featuredProducts);
            var pPriceMin = SqlParameterHelper.GetDecimalParameter("PriceMin", priceMin);
            var pPriceMax = SqlParameterHelper.GetDecimalParameter("PriceMax", priceMax);
            var pKeywords = SqlParameterHelper.GetStringParameter("Keywords", keywords);
            var pSearchDescriptions = SqlParameterHelper.GetBooleanParameter("SearchDescriptions", searchDescriptions);
            var pSearchManufacturerPartNumber = SqlParameterHelper.GetBooleanParameter("SearchManufacturerPartNumber", searchManufacturerPartNumber);
            var pSearchSku = SqlParameterHelper.GetBooleanParameter("SearchSku", searchSku);
            var pSearchProductTags = SqlParameterHelper.GetBooleanParameter("SearchProductTags", searchProductTags);
            var pUseFullTextSearch = SqlParameterHelper.GetBooleanParameter("UseFullTextSearch", InovatiqaDefaults.UseFullTextSearch);
            var pFullTextMode = SqlParameterHelper.GetInt32Parameter("FullTextMode", (int)FulltextSearchMode.ExactMatch);
            var pFilteredSpecs = SqlParameterHelper.GetStringParameter("FilteredSpecs", commaSeparatedSpecIds);
            var pLanguageId = SqlParameterHelper.GetInt32Parameter("LanguageId", languageId);
            var pOrderBy = SqlParameterHelper.GetInt32Parameter("OrderBy", (int)orderBy);
            var pAllowedCustomerRoleIds = SqlParameterHelper.GetStringParameter("AllowedCustomerRoleIds", string.Empty);
            var pPageIndex = SqlParameterHelper.GetInt32Parameter("PageIndex", pageIndex);
            var pPageSize = SqlParameterHelper.GetInt32Parameter("PageSize", pageSize);
            var pShowHidden = SqlParameterHelper.GetBooleanParameter("ShowHidden", showHidden);
            var pOverridePublished = SqlParameterHelper.GetBooleanParameter("OverridePublished", overridePublished);
            var pLoadFilterableSpecificationAttributeOptionIds = SqlParameterHelper.GetBooleanParameter("LoadFilterableSpecificationAttributeOptionIds", loadFilterableSpecificationAttributeOptionIds);

            var pFilterableSpecificationAttributeOptionIds = SqlParameterHelper.GetOutputStringParameter("FilterableSpecificationAttributeOptionIds");
            pFilterableSpecificationAttributeOptionIds.Size = int.MaxValue - 1;
            var pTotalRecords = SqlParameterHelper.GetOutputInt32Parameter("TotalRecords");

            var products = _productRepository.EntityFromSql("ProductLoadAllPaged",
                pCategoryIds,
                pManufacturerId,
                pStoreId,
                pVendorId,
                pWarehouseId,
                pProductTypeId,
                pVisibleIndividuallyOnly,
                pMarkedAsNewOnly,
                pProductTagId,
                pFeaturedProducts,
                pPriceMin,
                pPriceMax,
                pKeywords,
                pSearchDescriptions,
                pSearchManufacturerPartNumber,
                pSearchSku,
                pSearchProductTags,
                pUseFullTextSearch,
                pFullTextMode,
                pFilteredSpecs,
                pLanguageId,
                pOrderBy,
                pAllowedCustomerRoleIds,
                pPageIndex,
                pPageSize,
                pShowHidden,
                pOverridePublished,
                pLoadFilterableSpecificationAttributeOptionIds,
                pFilterableSpecificationAttributeOptionIds,
                pTotalRecords).ToList();

            var filterableSpecificationAttributeOptionIdsStr =
                pFilterableSpecificationAttributeOptionIds.Value != DBNull.Value
                    ? (string)pFilterableSpecificationAttributeOptionIds.Value
                    : string.Empty;

            if (loadFilterableSpecificationAttributeOptionIds &&
                !string.IsNullOrWhiteSpace(filterableSpecificationAttributeOptionIdsStr))
            {
                filterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIdsStr
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x.Trim()))
                    .ToList();
            }
            var totalRecords = pTotalRecords.Value != DBNull.Value ? Convert.ToInt32(pTotalRecords.Value) : 0;

            return new PagedList<Product>(products, pageIndex, pageSize, totalRecords);
        }

        public virtual Product GetProductById(int productId)
        {
            if (productId == 0)
                return null;

            return _productRepository.GetById(productId);
        }

        public virtual string FormatStockMessage(Product product, string attributesXml)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var stockMessage = string.Empty;

            switch (product.ManageInventoryMethodId)
            {
                case 1:
                    stockMessage = GetStockMessage(product, stockMessage);
                    break;
                case 2:
                    stockMessage = GeStockMessage(product, attributesXml);
                    break;
            }

            return stockMessage;
        }

        public virtual int GetTotalStockQuantity(Product product, bool useReservedQuantity = true, int warehouseId = 0)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (product.ManageInventoryMethodId != 1)
            {
                return 0;
            }

            if (!product.UseMultipleWarehouses)
                return product.StockQuantity;

            var pwi = _productWarehouseInventoryRepository.Query().Where(wi => wi.ProductId == product.Id);

            if (warehouseId > 0)
            {
                pwi = pwi.Where(x => x.WarehouseId == warehouseId);
            }

            var result = pwi.Sum(x => x.StockQuantity);
            if (useReservedQuantity)
            {
                result -= pwi.Sum(x => x.ReservedQuantity);
            }

            return result;
        }

        public virtual int[] ParseAllowedQuantities(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var result = new List<int>();
            if (!string.IsNullOrWhiteSpace(product.AllowedQuantities))
            {
                product.AllowedQuantities
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList()
                    .ForEach(qtyStr =>
                    {
                        if (int.TryParse(qtyStr.Trim(), out var qty))
                        {
                            result.Add(qty);
                        }
                    });
            }

            return result.ToArray();
        }

        public virtual IList<Product> GetAllProductsDisplayedOnHomepage()
        {
            var query = from p in _productRepository.Query()
                        orderby p.DisplayOrder, p.Id
                        where p.Published &&
                        !p.Deleted &&
                        p.ShowOnHomepage
                        select p;

            var products = query.ToList();

            return products;
        }

        public virtual bool ProductIsAvailable(Product product, DateTime? dateTime = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (product.AvailableStartDateTimeUtc.HasValue && product.AvailableStartDateTimeUtc.Value > dateTime)
                return false;

            if (product.AvailableEndDateTimeUtc.HasValue && product.AvailableEndDateTimeUtc.Value < dateTime)
                return false;

            return true;
        }

        public virtual int GetNumberOfProductsInCategory(IList<int> categoryIds = null, int storeId = 0)
        {
            if (categoryIds != null && categoryIds.Contains(0))
                categoryIds.Remove(0);

            var query = _productRepository.Query();
            query = query.Where(p => !p.Deleted && p.Published && p.VisibleIndividually);

            if (categoryIds != null && categoryIds.Any())
            {
                query = from p in query
                        join pc in _productCategoryMappingRepository.Query() on p.Id equals pc.ProductId
                        where categoryIds.Contains(pc.CategoryId)
                        select p;
            }

            var result = query.Select(p => p.Id).Distinct().Count();

            return result;
        }

        public virtual IPagedList<Product> SearchProducts(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            int manufacturerId = 0,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool markedAsNewOnly = false,
            bool? featuredProducts = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            IList<int> filteredSpecs = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null)
        {
            return SearchProducts(out var _, false,
                pageIndex, pageSize, categoryIds, manufacturerId,
                storeId, vendorId, warehouseId,
                productType, visibleIndividuallyOnly, markedAsNewOnly, featuredProducts,
                priceMin, priceMax, productTagId, keywords, searchDescriptions, searchManufacturerPartNumber, searchSku,
                searchProductTags, languageId, filteredSpecs,
                orderBy, showHidden, overridePublished);
        }

        public virtual IList<Product> GetProductsByIds(int[] productIds)
        {
            if (productIds == null || productIds.Length == 0)
                return new List<Product>();

            var query = from p in _productRepository.Query()
                        where productIds.Contains(p.Id) && !p.Deleted
                        select p;

            var products = query.ToList();

            var sortedProducts = new List<Product>();
            foreach (var id in productIds)
            {
                var product = products.FirstOrDefault(x => x.Id == id);
                if (product != null)
                    sortedProducts.Add(product);
            }

            return sortedProducts;
        }

        public virtual int[] ParseRequiredProductIds(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (string.IsNullOrEmpty(product.RequiredProductIds))
                return Array.Empty<int>();

            var ids = new List<int>();

            foreach (var idStr in product.RequiredProductIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()))
            {
                if (int.TryParse(idStr, out var id))
                    ids.Add(id);
            }

            return ids.ToArray();
        }

        public virtual string FormatSku(Product product, string attributesXml = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            GetSkuMpnGtin(product, attributesXml, out var sku, out var _, out var _);

            return sku;
        }

        public virtual void UpdateProductViews(Product product)
        {
            product.TotalViews = product.TotalViews + 1;

            _productRepository.Update(product);
        }

        public virtual void UpdateProductReviewTotals(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var approvedRatingSum = 0;
            var notApprovedRatingSum = 0;
            var approvedTotalReviews = 0;
            var notApprovedTotalReviews = 0;

            var reviews = _productReviewRepository.Query().Where(r => r.ProductId == product.Id);
            foreach (var pr in reviews)
            {
                if (pr.IsApproved)
                {
                    approvedRatingSum += pr.Rating;
                    approvedTotalReviews++;
                }
                else
                {
                    notApprovedRatingSum += pr.Rating;
                    notApprovedTotalReviews++;
                }
            }

            product.ApprovedRatingSum = approvedRatingSum;
            product.NotApprovedRatingSum = notApprovedRatingSum;
            product.ApprovedTotalReviews = approvedTotalReviews;
            product.NotApprovedTotalReviews = notApprovedTotalReviews;
            UpdateProduct(product);
        }

        public virtual void UpdateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            _productRepository.Update(product);

            //_eventPublisher.EntityUpdated(product);
        }

        public virtual Product GetProductByMSku(int mSku)
        {
            if (mSku == 0)
                return null;

            return _productRepository.GetProductByMSku(mSku);
        }

        public virtual IPagedList<Product> GetLowStockProducts(int? vendorId = null, bool? loadPublishedOnly = true,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var query = _productRepository.Query();

            query = query.Where(product => product.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock);

            query = query.Where(product =>
                (product.UseMultipleWarehouses ? _productWarehouseInventoryRepository.Query().Where(pwi => pwi.ProductId == product.Id).Sum(pwi => pwi.StockQuantity - pwi.ReservedQuantity)
                    : product.StockQuantity) <= product.MinStockQuantity);

            query = query.Where(product => !product.Deleted);

            query = query.Where(product => product.ProductTypeId != (int)ProductType.GroupedProduct);

            if (vendorId.HasValue && vendorId.Value > 0)
                query = query.Where(product => product.VendorId == vendorId.Value);

            if (loadPublishedOnly.HasValue)
                query = loadPublishedOnly.Value ? query.Where(product => product.Published) : query.Where(product => !product.Published);

            query = query.OrderBy(product => product.MinStockQuantity).ThenBy(product => product.DisplayOrder).ThenBy(product => product.Id);

            return new PagedList<Product>(query, pageIndex, pageSize, getOnlyTotalCount);
        }

        public virtual IPagedList<ProductAttributeCombination> GetLowStockProductCombinations(int? vendorId = null, bool? loadPublishedOnly = true,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var combinations = from pac in _productAttributeCombinationRepository.Query()
                               join p in _productRepository.Query() on pac.ProductId equals p.Id
                               where
                                   pac.StockQuantity < pac.NotifyAdminForQuantityBelow &&
                                   p.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStockByAttributes &&
                                   !p.Deleted &&
                                   p.ProductTypeId != (int)ProductType.GroupedProduct &&
                                   (vendorId ?? 0) == 0 || p.VendorId == vendorId &&
                                   loadPublishedOnly == null || p.Published == loadPublishedOnly
                               orderby pac.ProductId, pac.Id
                               select pac;

            return new PagedList<ProductAttributeCombination>(combinations, pageIndex, pageSize, getOnlyTotalCount);
        }

        //public virtual void IndexProducts()
        //{
        //    var model = GetAllProducts();
        //    var allCats = _categoryService.GetAllCategories();
        //    foreach (var product in model)
        //    {
        //        try
        //        {
        //            var cats = _categoryService.GetProductCategoriesByProductId(product.Id);
        //            var CategoryList = cats.Select(product => product.CategoryId).ToList();
        //            var categoryNames = allCats.Where(c => CategoryList.Contains(c.Id)).ToList().Select(c => c.Name);
        //            var ManufacturerList = _manufacturerService.GetProductManufacturersByProductId(product.Id).Select(man => man.ManufacturerId).ToList();
        //            var elasticProduct = new ElasticProduct
        //            {
        //                Id = product.Id,
        //                Name = product.Name,
        //                ShortDescription = product.ShortDescription,
        //                FullDescription = product.FullDescription,
        //                AdminComment = product.AdminComment,
        //                MetaKeywords = product.MetaKeywords,
        //                Sku = product.Sku,
        //                ManufacturerPartNumber = product.ManufacturerPartNumber,
        //                Gtin = product.Gtin,
        //                Categories = CategoryList,
        //                Manufacturers = ManufacturerList,
        //                CategoriesNames = categoryNames.ToList() ?? new List<string>()
        //            };
        //            var resp = _elasticClient.IndexDocument(elasticProduct);
        //            if (!resp.ApiCall.Success)
        //            {
        //                throw new Exception(resp.ApiCall.DebugInformation);
        //            }
        //        }
        //        catch(Exception ex)
        //        {
        //            throw ex;
        //        }
        //    }
        //}

        #endregion

        #region Cross-sell products
        public virtual IList<Product> GetCrosssellProductsByShoppingCart(IList<ShoppingCartItem> cart, int numberOfProducts)
        {
            var result = new List<Product>();

            if (numberOfProducts == 0)
                return result;

            if (cart == null || !cart.Any())
                return result;

            var cartProductIds = new List<int>();
            foreach (var sci in cart)
            {
                var prodId = sci.ProductId;
                if (!cartProductIds.Contains(prodId))
                    cartProductIds.Add(prodId);
            }

            var productIds = cart.Select(sci => sci.ProductId).ToArray();
            var crossSells = GetCrossSellProductsByProductIds(productIds);
            foreach (var crossSell in crossSells)
            {
                if (result.Find(p => p.Id == crossSell.ProductId2) != null || cartProductIds.Contains(crossSell.ProductId2))
                    continue;

                var productToAdd = GetProductById(crossSell.ProductId2);
                if (productToAdd == null || productToAdd.Deleted || !productToAdd.Published)
                    continue;

                result.Add(productToAdd);
                if (result.Count >= numberOfProducts)
                    return result;
            }

            return result;
        }

        public virtual IList<CrossSellProduct> GetCrossSellProductsByProductIds(int[] productIds, bool showHidden = false)
        {
            if (productIds == null || productIds.Length == 0)
                return new List<CrossSellProduct>();

            var query = from csp in _crossSellProductRepository.Query()
                        join p in _productRepository.Query() on csp.ProductId2 equals p.Id
                        where productIds.Contains(csp.ProductId1) &&
                              !p.Deleted &&
                              (showHidden || p.Published)
                        orderby csp.Id
                        select csp;
            var crossSellProducts = query.ToList();
            return crossSellProducts;
        }
        #endregion

        #region Most viewed products

        public virtual IList<Product> GetMostViewedProducts(int number)
        {
            var list = (from p in _productRepository.Query()
                         where p.Published && !p.Deleted
                         orderby p.TotalViews descending
                         select p).Take(number).ToList();

            return list;
        }

        #endregion

        #region Product pictures

        public virtual void DeleteProductPicture(ProductPictureMapping productPicture)
        {
            if (productPicture == null)
                throw new ArgumentNullException(nameof(productPicture));

            _productPictureRepository.Delete(productPicture);

            //event notification
            //_eventPublisher.EntityDeleted(productPicture);
        }

        public virtual void UpdateProductPicture(ProductPictureMapping productPicture)
        {
            if (productPicture == null)
                throw new ArgumentNullException(nameof(productPicture));

            _productPictureRepository.Update(productPicture);

            //event notification
            //_eventPublisher.EntityUpdated(productPicture);
        }

        public virtual ProductPictureMapping GetProductPictureById(int productPictureId)
        {
            if (productPictureId == 0)
                return null;

            return _productPictureRepository.GetById(productPictureId);
        }

        public virtual IList<ProductPictureMapping> GetProductPicturesByProductId(int productId)
        {
            var query = from pp in _productPictureRepository.Query()
                        where pp.ProductId == productId
                        orderby pp.DisplayOrder, pp.Id
                        select pp;

            var productPictures = query.ToList();

            return productPictures;
        }

        #endregion


        #region Product reviews

        public virtual IPagedList<ProductReview> GetAllProductReviews(int customerId = 0, bool? approved = null,
            DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = null, int storeId = 0, int productId = 0, int vendorId = 0, bool showHidden = false,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _productReviewRepository.Query();

            if (approved.HasValue)
                query = query.Where(pr => pr.IsApproved == approved);
            if (customerId > 0)
                query = query.Where(pr => pr.CustomerId == customerId);
            if (fromUtc.HasValue)
                query = query.Where(pr => fromUtc.Value <= pr.CreatedOnUtc);
            if (toUtc.HasValue)
                query = query.Where(pr => toUtc.Value >= pr.CreatedOnUtc);
            if (!string.IsNullOrEmpty(message))
                query = query.Where(pr => pr.Title.Contains(message) || pr.ReviewText.Contains(message));
            if (productId > 0)
                query = query.Where(pr => pr.ProductId == productId);

            query = from productReview in query
                    join product in _productRepository.Query() on productReview.ProductId equals product.Id
                    where
                        (vendorId == 0 || product.VendorId == vendorId) &&
                        !product.Deleted
                    select productReview;

            query = InovatiqaDefaults.ProductReviewsSortByCreatedDateAscending
                ? query.OrderBy(pr => pr.CreatedOnUtc).ThenBy(pr => pr.Id)
                : query.OrderByDescending(pr => pr.CreatedOnUtc).ThenBy(pr => pr.Id);

            var productReviews = new PagedList<ProductReview>(query, pageIndex, pageSize);

            return productReviews;
        }

        public virtual void InsertProductReview(ProductReview productReview)
        {
            if (productReview == null)
                throw new ArgumentNullException(nameof(productReview));

            _productReviewRepository.Insert(productReview);

            //_eventPublisher.EntityInserted(productReview);
        }

        public virtual ProductReview GetProductReviewById(int productReviewId)
        {
            if (productReviewId == 0)
                return null;

            return _productReviewRepository.GetById(productReviewId);
        }

        public virtual void SetProductReviewHelpfulness(ProductReview productReview, bool helpfulness)
        {
            if (productReview is null)
                throw new ArgumentNullException(nameof(productReview));

            var prh = _productReviewHelpfulnessRepository.Query().SingleOrDefault(h => h.ProductReviewId == productReview.Id && h.CustomerId == _workContextService.CurrentCustomer.Id);

            if (prh is null)
            {
                prh = new ProductReviewHelpfulness
                {
                    ProductReviewId = productReview.Id,
                    CustomerId = _workContextService.CurrentCustomer.Id,
                    WasHelpful = helpfulness,
                };

                InsertProductReviewHelpfulness(prh);

                //_eventPublisher.EntityInserted(prh);
            }
            else
            {
                prh.WasHelpful = helpfulness;
                _productReviewHelpfulnessRepository.Update(prh);

                //_eventPublisher.EntityUpdated(prh);
            }
            
        }

        public virtual void InsertProductReviewHelpfulness(ProductReviewHelpfulness productReviewHelpfulness)
        {
            if (productReviewHelpfulness == null)
                throw new ArgumentNullException(nameof(productReviewHelpfulness));

            _productReviewHelpfulnessRepository.Insert(productReviewHelpfulness);

            //_eventPublisher.EntityInserted(productReviewHelpfulness);
        }

        public virtual void UpdateProductReviewHelpfulnessTotals(ProductReview productReview)
        {
            if (productReview is null)
                throw new ArgumentNullException(nameof(productReview));

            (productReview.HelpfulYesTotal, productReview.HelpfulNoTotal) = GetHelpfulnessCounts(productReview);

            _productReviewRepository.Update(productReview);

            //_eventPublisher.EntityUpdated(productReview);
        }

        public virtual (int usefulCount, int notUsefulCount) GetHelpfulnessCounts(ProductReview productReview)
        {
            if (productReview is null)
                throw new ArgumentNullException(nameof(productReview));

            var productReviewHelpfulness = _productReviewHelpfulnessRepository.Query().Where(prh => prh.ProductReviewId == productReview.Id);

            return (productReviewHelpfulness.Count(prh => prh.WasHelpful), productReviewHelpfulness.Count(prh => !prh.WasHelpful));
        }

        #endregion

        #region Product Warehouses

        public virtual Warehouse GetWarehousesById(int warehouseId)
        {
            if (warehouseId == 0)
                return null;

            return _warehouseRepository.GetById(warehouseId);
        }

        public virtual IList<ProductWarehouseInventory> GetAllProductWarehouseInventoryRecords(int productId)
        {
            return _productWarehouseInventoryRepository.Query().Where(pwi => pwi.ProductId == productId).ToList();
        }

        #endregion

        #region COVID-19 products

        public virtual IList<Product> GetAllCovid19ProductsDisplayedOnHomepage()
        {
            var query = from p in _productRepository.Query()
                        orderby p.DisplayOrder, p.Id
                        where p.Published &&
                        !p.Deleted &&
                        p.ShowCovid19OnHomepage && p.IsCovid19Product
                        select p;

            var products = query.ToList();

            return products;
        }

        #endregion

        #region Stock quantity history

        public virtual void AddStockQuantityHistoryEntry(Product product, int quantityAdjustment, int stockQuantity,
            int warehouseId = 0, string message = "", int? combinationId = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (quantityAdjustment == 0)
                return;

            var historyEntry = new StockQuantityHistory
            {
                ProductId = product.Id,
                CombinationId = combinationId,
                WarehouseId = warehouseId > 0 ? (int?)warehouseId : null,
                QuantityAdjustment = quantityAdjustment,
                StockQuantity = stockQuantity,
                Message = message,
                CreatedOnUtc = DateTime.UtcNow
            };

            _stockQuantityHistoryRepository.Insert(historyEntry);

            ////event notification
            //_eventPublisher.EntityInserted(historyEntry);
        }

        public virtual List<Product> GetAllProducts()
        {
            return _productRepository.Query().ToList();
        }

        #endregion

        #region Inventory management methods

        public virtual void BookReservedInventory(Product product, int warehouseId, int quantity, string message = "")
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (quantity >= 0)
                throw new ArgumentException("Value must be negative.", nameof(quantity));

            if (product.ManageInventoryMethodId != (int)ManageInventoryMethod.ManageStock || !product.UseMultipleWarehouses)
                return;

            var pwi = _productWarehouseInventoryRepository.Query().FirstOrDefault(wi => wi.ProductId == product.Id && wi.WarehouseId == warehouseId);
            if (pwi == null)
                return;

            pwi.ReservedQuantity = Math.Max(pwi.ReservedQuantity + quantity, 0);
            pwi.StockQuantity += quantity;

            UpdateProductWarehouseInventory(pwi);

            AddStockQuantityHistoryEntry(product, quantity, pwi.StockQuantity, warehouseId, message);
        }

        public virtual void BalanceInventory(Product product, int warehouseId, int quantity)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productInventory = _productWarehouseInventoryRepository.Query()
                .Where(pwi => pwi.ProductId == product.Id && pwi.WarehouseId == warehouseId)
                .ToList()
                .FirstOrDefault();

            if (productInventory == null)
                return;

            var selectQty = Math.Min(productInventory.StockQuantity - productInventory.ReservedQuantity, quantity);
            productInventory.ReservedQuantity += selectQty;

            var productAnotherInventories = _productWarehouseInventoryRepository.Query()
                .Where(pwi => pwi.ProductId == product.Id && pwi.WarehouseId != warehouseId)
                .OrderByDescending(ob => ob.ReservedQuantity)
                .ToList();

            var qty = selectQty;

            foreach (var productAnotherInventory in productAnotherInventories)
            {
                if (qty > 0)
                {
                    if (productAnotherInventory.ReservedQuantity >= qty)
                    {
                        productAnotherInventory.ReservedQuantity -= qty;
                    }
                    else
                    {
                        qty = selectQty - productAnotherInventory.ReservedQuantity;
                        productAnotherInventory.ReservedQuantity = 0;
                    }
                }
            }

            UpdateProduct(product);
        }

        public virtual int ReverseBookedInventory(Product product, ShipmentItem shipmentItem, string message = "")
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (shipmentItem == null)
                throw new ArgumentNullException(nameof(shipmentItem));

            if (product.ManageInventoryMethodId != (int)ManageInventoryMethod.ManageStock || !product.UseMultipleWarehouses)
                return 0;

            var pwi = _productWarehouseInventoryRepository.Query().FirstOrDefault(wi => wi.ProductId == product.Id && wi.WarehouseId == shipmentItem.WarehouseId);
            if (pwi == null)
                return 0;

            var shipment = _shipmentRepository.GetById(shipmentItem.ShipmentId);

            if (!shipment.ShippedDateUtc.HasValue)
                return 0;

            var qty = shipmentItem.Quantity;

            pwi.StockQuantity += qty;
            pwi.ReservedQuantity += qty;

            UpdateProductWarehouseInventory(pwi);

            AddStockQuantityHistoryEntry(product, qty, pwi.StockQuantity, shipmentItem.WarehouseId, message);

            return qty;
        }

        #endregion

        #region Tier prices
        //tier price change
        public virtual EntityTierPrice GetTierPriceById(int tierPriceId)
        {
            if (tierPriceId == 0)
                return null;

            return _entityTierPriceRepository.GetById(tierPriceId);
        }

        public virtual void UpdateHasTierPricesProperty(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            product.HasTierPrices = GetTierPricesByProduct(product.Id, "Product").Any();
            UpdateProduct(product);
        }
        //tier price change
        public virtual void InsertTierPrice(EntityTierPrice tierPrice)
        {
            if (tierPrice == null)
                throw new ArgumentNullException(nameof(tierPrice));

            //_tierPriceRepository.Insert(tierPrice);
            _entityTierPriceRepository.Insert(tierPrice);

            //event notification
            //_eventPublisher.EntityInserted(tierPrice);
        }
        //tier price change
        public virtual void DeleteTierPrice(EntityTierPrice tierPrice)
        {
            if (tierPrice == null)
                throw new ArgumentNullException(nameof(tierPrice));

            _entityTierPriceRepository.Delete(tierPrice);

            //event notification
            //_eventPublisher.EntityDeleted(tierPrice);
        }
        //tier price change
        public virtual void UpdateTierPrice(EntityTierPrice tierPrice)
        {
            if (tierPrice == null)
                throw new ArgumentNullException(nameof(tierPrice));

            _entityTierPriceRepository.Update(tierPrice);

            //event notification
            //_eventPublisher.EntityUpdated(tierPrice);
        }
        //tier price change
        public virtual IList<EntityTierPrice> GetTierPricesByProduct(int EntityId, String EntityName)
        {
            return _entityTierPriceRepository.Query().Where(tp => tp.EntityId == EntityId && tp.EntityName == EntityName)
                .ToList();
        }
        //tier price change
        public virtual EntityTierPrice GetPreferredTierPrice(Product product, Customer customer, int storeId, int quantity)
        {
            if (product is null)
                throw new ArgumentNullException(nameof(product));

            if (customer is null)
                throw new ArgumentNullException(nameof(customer));

            //if (!product.HasTierPrices)
            //    return null;
            //return null;

            return GetTierPrices(product, customer, storeId)?.LastOrDefault();
        }
        //tier price change
        public virtual IList<EntityTierPrice> GetTierPrices(Product product, Customer customer, int storeId)
        {
            if (product is null)
                throw new ArgumentNullException(nameof(product));

            if (customer is null)
                throw new ArgumentNullException(nameof(customer));

            //if (!product.HasTierPrices)
            //    return null;

            //var customerRoleIds = _customerService.GetCustomerRoleIds(customer);
            DateTime now = DateTime.Now;

            var tierPrices = GetTierPricesByProduct(product.Id, "Product")
                .OrderBy(price => price.Rate)
                .ToList();
            tierPrices = tierPrices.Where(tp => tp.StartDateTimeUtc <= now && tp.EndDateTimeUtc >= now).ToList();
            if (tierPrices.Count == 0)
            {
                tierPrices = GetTierPricesByProduct(Convert.ToInt32(product.RootCategoryId), "Category")
                .OrderBy(price => price.Rate)
                .ToList();
                tierPrices = tierPrices.Where(tp => tp.StartDateTimeUtc <= now && tp.EndDateTimeUtc >= now).ToList();
                if (tierPrices.Count == 0)
                {
                    tierPrices = GetTierPricesByProduct(0, "ALL")
                    .OrderBy(price => price.Rate)
                    .ToList();
                    tierPrices = tierPrices.Where(tp => tp.StartDateTimeUtc <= now && tp.EndDateTimeUtc >= now).ToList();
                }
            }

            if (tierPrices.Count > 0)
            {
                return tierPrices;
            }
            //else if (tierPrices.Count == 0)
            //{
            //    var allRoles = _customerService.GetAllCustomerRoles(false).Where(x => x.IsSystemRole == true);
            //    var customerRoleNames = allRoles.Where(item => customerRoleIds.Contains(item.Id))
            //         .Select(x => x.Name).ToList();

            //    IList<EntityTierPrice> prices = null;
            //    var priceHandled = false;
            //    if (customerRoleNames.Any(str => str.Contains("Guest")))
            //    {
            //        prices = CalculateProductPriceRoleBased(product, InovatiqaDefaults.RetailRoleName, 60);
            //    }
            //    else if (customerRoleNames.Any(str => str.Contains("_" + product.RootCategoryId)))
            //    {
            //        foreach (var customerRoleName in customerRoleNames)
            //        {
            //            if (priceHandled == false && customerRoleName.Contains("_"))
            //            {
            //                var currentCustomerRole = customerRoleName.Replace("\r\n", "");
            //                var split = currentCustomerRole.Split('_');
            //                var currentRoleName = split[1];
            //                switch (currentRoleName)
            //                {
            //                    case InovatiqaDefaults.RetailRoleName:
            //                        prices = CalculateProductPriceRoleBased(product, "_" + split[2], 60);
            //                        priceHandled = true;
            //                        break;
            //                    case InovatiqaDefaults.BronzeRoleName:
            //                        prices = CalculateProductPriceRoleBased(product, "_" + split[2], 50);
            //                        priceHandled = true;
            //                        break;
            //                    case InovatiqaDefaults.BronzePremierRoleName:
            //                        prices = CalculateProductPriceRoleBased(product, "_" + split[2], 45);
            //                        priceHandled = true;
            //                        break;
            //                    case InovatiqaDefaults.GoldRoleName:
            //                        prices = CalculateProductPriceRoleBased(product, "_" + split[2], 40);
            //                        priceHandled = true;
            //                        break;
            //                    case InovatiqaDefaults.GoldPremierRoleName:
            //                        prices = CalculateProductPriceRoleBased(product, "_" + split[2], 35);
            //                        priceHandled = true;
            //                        break;
            //                    case InovatiqaDefaults.OnyxRoleName:
            //                        prices = CalculateProductPriceRoleBased(product, "_" + split[2], 30);
            //                        priceHandled = true;
            //                        break;
            //                    case InovatiqaDefaults.OnyxPremierRoleName:
            //                        prices = CalculateProductPriceRoleBased(product, "_" + split[2], 25);
            //                        priceHandled = true;
            //                        break;
            //                    case InovatiqaDefaults.DiamondRoleName:
            //                        prices = CalculateProductPriceRoleBased(product, "_" + split[2], 20);
            //                        priceHandled = true;
            //                        break;
            //                    case InovatiqaDefaults.DiamondPremierRoleName:
            //                        prices = CalculateProductPriceRoleBased(product, "_" + split[2], 15);
            //                        priceHandled = true;
            //                        break;
            //                    case InovatiqaDefaults.DistributorRoleName:
            //                        prices = CalculateProductPriceRoleBased(product, "_" + split[2], 12);
            //                        priceHandled = true;
            //                        break;
            //                    case InovatiqaDefaults.DistributorPremierRoleName:
            //                        prices = CalculateProductPriceRoleBased(product, "_" + split[2], 10);
            //                        priceHandled = true;
            //                        break;
            //                    default:
            //                        break;
            //                }
            //            }
            //        }
            //    }
            //    priceHandled = false;
            //    foreach (var customerRoleName in customerRoleNames)
            //    {
            //        if (priceHandled == false)
            //        {
            //            switch (customerRoleName)
            //            {
            //                case InovatiqaDefaults.RetailRoleName:
            //                    prices = CalculateProductPriceRoleBased(product, customerRoleName, 60);
            //                    priceHandled = true;
            //                    break;
            //                case InovatiqaDefaults.BronzeRoleName:
            //                    prices = CalculateProductPriceRoleBased(product, customerRoleName, 50);
            //                    priceHandled = true;
            //                    break;
            //                case InovatiqaDefaults.BronzePremierRoleName:
            //                    prices = CalculateProductPriceRoleBased(product, customerRoleName, 45);
            //                    priceHandled = true;
            //                    break;
            //                case InovatiqaDefaults.GoldRoleName:
            //                    prices = CalculateProductPriceRoleBased(product, customerRoleName, 40);
            //                    priceHandled = true;
            //                    break;
            //                case InovatiqaDefaults.GoldPremierRoleName:
            //                    prices = CalculateProductPriceRoleBased(product, customerRoleName, 35);
            //                    priceHandled = true;
            //                    break;
            //                case InovatiqaDefaults.OnyxRoleName:
            //                    prices = CalculateProductPriceRoleBased(product, customerRoleName, 30);
            //                    priceHandled = true;
            //                    break;
            //                case InovatiqaDefaults.OnyxPremierRoleName:
            //                    prices = CalculateProductPriceRoleBased(product, customerRoleName, 25);
            //                    priceHandled = true;
            //                    break;
            //                case InovatiqaDefaults.DiamondRoleName:
            //                    prices = CalculateProductPriceRoleBased(product, customerRoleName, 20);
            //                    priceHandled = true;
            //                    break;
            //                case InovatiqaDefaults.DiamondPremierRoleName:
            //                    prices = CalculateProductPriceRoleBased(product, customerRoleName, 15);
            //                    priceHandled = true;
            //                    break;
            //                case InovatiqaDefaults.DistributorRoleName:
            //                    prices = CalculateProductPriceRoleBased(product, customerRoleName, 12);
            //                    priceHandled = true;
            //                    break;
            //                case InovatiqaDefaults.DistributorPremierRoleName:
            //                    prices = CalculateProductPriceRoleBased(product, customerRoleName, 10);
            //                    priceHandled = true;
            //                    break;
            //                default:
            //                    break;
            //            }
            //        }
            //    }
            //    return prices;
            //}
            else
                return null;
        }

        private class ElasticProduct
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string ShortDescription { get; set; }
            public string FullDescription { get; set; }
            public string AdminComment { get; set; }
            public string MetaKeywords { get; set; }
            public string Sku { get; set; }
            public string ManufacturerPartNumber { get; set; }
            public string Gtin { get; set; }
            public List<int> Categories { get; set; }
            public List<string> CategoriesNames { get; set; }
            public List<int> Manufacturers { get; set; }
        }

        #endregion

        #endregion
    }
}
