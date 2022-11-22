using Inovatiqa.Core;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Logging.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.Shipping.Pickup;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Shipping
{
    public partial class ShippingService : IShippingService
    {
        #region Fields

        private readonly IProductAttributeParserService _productAttributeParser;
        private readonly IProductService _productService;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IAddressService _addressService;
        private readonly ICustomerService _customerService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly Lazy<IShippingRateComputationMethodService> _shippingRateComputationMethodService;
        private readonly ILoggerService _loggerService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICheckoutAttributeParserService _checkoutAttributeParserService;
        private readonly IRepository<ShippingMethod> _shippingMethodRepository;

        #endregion

        #region Ctor

        public ShippingService(IProductAttributeParserService productAttributeParser,
            IProductService productService,
            IRepository<Warehouse> warehouseRepository,
            IAddressService addressService,
            ICustomerService customerService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            Lazy<IShippingRateComputationMethodService> shippingRateComputationMethodService,
            ILoggerService loggerService,
            IPriceCalculationService priceCalculationService,
            IGenericAttributeService genericAttributeService,
            ICheckoutAttributeParserService checkoutAttributeParserService,
            IRepository<ShippingMethod> shippingMethodRepository)
        {
            _productAttributeParser = productAttributeParser;
            _productService = productService;
            _warehouseRepository = warehouseRepository;
            _addressService = addressService;
            _customerService = customerService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _shippingRateComputationMethodService = shippingRateComputationMethodService;
            _loggerService = loggerService;
            _priceCalculationService = priceCalculationService;
            _genericAttributeService = genericAttributeService;
            _checkoutAttributeParserService = checkoutAttributeParserService;
            _shippingMethodRepository = shippingMethodRepository;
        }

        #endregion

        #region Utilities

        protected bool AreMultipleItems(IList<GetShippingOptionRequest.PackageItem> items)
        {
            if (!items.Any())
                return false;

            if (items.Count > 1)
                return true;

            var singleItem = items.First();

            if (singleItem.GetQuantity() > 1)
                return true;

            if (string.IsNullOrEmpty(singleItem.ShoppingCartItem.AttributesXml))
                return false;

            var associatedAttributeValues = _productAttributeParser.ParseProductAttributeValues(singleItem.ShoppingCartItem.AttributesXml)
                .Where(attributeValue => attributeValue.AttributeValueTypeId == InovatiqaDefaults.AssociatedToProduct);

            return associatedAttributeValues.Any(attributeValue =>
                _productService.GetProductById(attributeValue.AssociatedProductId)?.IsShipEnabled ?? false);
        }

        #endregion

        #region Methods

        #region Shipping methods

        public virtual IList<ShippingMethod> GetAllShippingMethods(int? filterByCountryId = null)
        {
            if (filterByCountryId.HasValue && filterByCountryId.Value > 0)
            {
                var query1 = from sm in _shippingMethodRepository.Query()
                             select sm.Id;

                query1 = query1.Distinct();

                var query2 = from sm in _shippingMethodRepository.Query()
                             where !query1.Contains(sm.Id)
                             orderby sm.DisplayOrder, sm.Id
                             select sm;

                return query2.ToList();
            }

            var query = from sm in _shippingMethodRepository.Query()
                        orderby sm.DisplayOrder, sm.Id
                        select sm;

            return query.ToList();
        }

        #endregion

        #region Warehouses

        public virtual Warehouse GetWarehouseById(int warehouseId)
        {
            if (warehouseId == 0)
                return null;

            return _warehouseRepository.GetById(warehouseId);
        }

        public virtual IList<Warehouse> GetAllWarehouses(string name = null)
        {
            var query = from wh in _warehouseRepository.Query()
                        orderby wh.Name
                        select wh;

            var warehouses = query.ToList();

            if (!string.IsNullOrEmpty(name))
            {
                warehouses = warehouses.Where(wh => wh.Name.Contains(name)).ToList();
            }

            return warehouses;
        }

        public virtual Warehouse GetNearestWarehouse(Address address, IList<Warehouse> warehouses = null)
        {
            warehouses ??= GetAllWarehouses();

            if (address == null)
                return warehouses.FirstOrDefault();

            var matchedByCountry = new List<Warehouse>();
            foreach (var warehouse in warehouses)
            {
                var warehouseAddress = _addressService.GetAddressById(warehouse.AddressId);
                if (warehouseAddress == null)
                    continue;

                if (warehouseAddress.CountryId == address.CountryId)
                    matchedByCountry.Add(warehouse);
            }
            //no country matches. return any
            if (!matchedByCountry.Any())
                return warehouses.FirstOrDefault();

            //find by state
            var matchedByState = new List<Warehouse>();
            foreach (var warehouse in matchedByCountry)
            {
                var warehouseAddress = _addressService.GetAddressById(warehouse.AddressId);
                if (warehouseAddress == null)
                    continue;

                if (warehouseAddress.StateProvinceId == address.StateProvinceId)
                    matchedByState.Add(warehouse);
            }

            if (matchedByState.Any())
                return matchedByState.FirstOrDefault();

            //no state matches. return any
            return matchedByCountry.FirstOrDefault();
        }

        #endregion

        #region Workflow

        public virtual bool IsShipEnabled(ShoppingCartItem shoppingCartItem)
        {
            if (shoppingCartItem.ProductId != 0 && _productService.GetProductById(shoppingCartItem.ProductId)?.IsShipEnabled == true)
                return true;

            if (string.IsNullOrEmpty(shoppingCartItem.AttributesXml))
                return false;

            return _productAttributeParser.ParseProductAttributeValues(shoppingCartItem.AttributesXml)
                .Where(attributeValue => attributeValue.AttributeValueTypeId == InovatiqaDefaults.AssociatedToProduct)
                .Any(attributeValue => _productService.GetProductById(attributeValue.AssociatedProductId)?.IsShipEnabled ?? false);
        }

        public virtual bool IsFreeShipping(ShoppingCartItem shoppingCartItem)
        {
            if (!IsShipEnabled(shoppingCartItem))
                return true;

            if (shoppingCartItem.ProductId != 0 && !_productService.GetProductById(shoppingCartItem.ProductId).IsFreeShipping)
                return false;

            if (string.IsNullOrEmpty(shoppingCartItem.AttributesXml))
                return true;

            return _productAttributeParser.ParseProductAttributeValues(shoppingCartItem.AttributesXml)
                .Where(attributeValue => attributeValue.AttributeValueTypeId == InovatiqaDefaults.AssociatedToProduct)
                .All(attributeValue => _productService.GetProductById(attributeValue.AssociatedProductId)?.IsFreeShipping ?? true);
        }

        public virtual decimal GetAdditionalShippingCharge(ShoppingCartItem shoppingCartItem)
        {
            if (IsFreeShipping(shoppingCartItem))
                return decimal.Zero;

            var additionalShippingCharge = (_productService.GetProductById(shoppingCartItem.ProductId)?.AdditionalShippingCharge ?? decimal.Zero) * shoppingCartItem.Quantity;

            if (string.IsNullOrEmpty(shoppingCartItem.AttributesXml))
                return additionalShippingCharge;

            additionalShippingCharge += _productAttributeParser.ParseProductAttributeValues(shoppingCartItem.AttributesXml)
                .Where(attributeValue => attributeValue.AttributeValueTypeId == InovatiqaDefaults.AssociatedToProduct)
                .Sum(attributeValue => _productService.GetProductById(attributeValue.AssociatedProductId)?.AdditionalShippingCharge ?? decimal.Zero);

            return additionalShippingCharge;
        }

        public virtual IList<GetShippingOptionRequest> CreateShippingOptionRequests(IList<ShoppingCartItem> cart,
            Address shippingAddress, int storeId, out bool shippingFromMultipleLocations)
        {
            var requests = new Dictionary<int, GetShippingOptionRequest>();

            var separateRequests = new List<GetShippingOptionRequest>();

            foreach (var sci in cart)
            {
                if (!IsShipEnabled(sci))
                    continue;

                var product = _productService.GetProductById(sci.ProductId);

                if (product == null || !product.IsShipEnabled)
                {
                    var associatedProducts = _productAttributeParser.ParseProductAttributeValues(sci.AttributesXml)
                        .Where(attributeValue => attributeValue.AttributeValueTypeId == InovatiqaDefaults.AssociatedToProduct)
                        .Select(attributeValue => _productService.GetProductById(attributeValue.AssociatedProductId));
                    product = associatedProducts.FirstOrDefault(associatedProduct => associatedProduct != null && associatedProduct.IsShipEnabled);
                }

                if (product == null)
                    continue;

                Warehouse warehouse = null;
                if (InovatiqaDefaults.UseWarehouseLocation)
                {
                    if (product.ManageInventoryMethodId == InovatiqaDefaults.ManageStock &&
                        product.UseMultipleWarehouses)
                    {
                        var allWarehouses = new List<Warehouse>();

                        foreach (var pwi in _productService.GetAllProductWarehouseInventoryRecords(product.Id))
                        {
                            var tmpWarehouse = GetWarehouseById(pwi.WarehouseId);
                            if (tmpWarehouse != null)
                                allWarehouses.Add(tmpWarehouse);
                        }

                        warehouse = GetNearestWarehouse(shippingAddress, allWarehouses);
                    }
                    else
                    {
                        warehouse = GetWarehouseById(product.WarehouseId);
                    }
                }

                var warehouseId = warehouse?.Id ?? 0;

                if (requests.ContainsKey(warehouseId) && !product.ShipSeparately)
                {
                    requests[warehouseId].Items.Add(new GetShippingOptionRequest.PackageItem(sci, product));
                }
                else
                {
                    var request = new GetShippingOptionRequest
                    {
                        StoreId = storeId
                    };
                    request.Customer = _customerService.GetShoppingCartCustomer(cart);

                    request.ShippingAddress = shippingAddress;
                    Address originAddress = null;
                    if (warehouse != null)
                    {
                        originAddress = _addressService.GetAddressById(warehouse.AddressId);
                        request.WarehouseFrom = warehouse;
                    }

                    if (originAddress == null)
                    {
                        originAddress = _addressService.GetAddressById(InovatiqaDefaults.ShippingOriginAddressId);
                    }

                    if (originAddress != null)
                    {
                        request.CountryFrom = _countryService.GetCountryByAddress(originAddress);
                        request.StateProvinceFrom = _stateProvinceService.GetStateProvinceByAddress(originAddress);
                        request.ZipPostalCodeFrom = originAddress.ZipPostalCode;
                        request.CountyFrom = originAddress.County;
                        request.CityFrom = originAddress.City;
                        request.AddressFrom = originAddress.Address1;
                    }

                    if (product.ShipSeparately)
                    {
                        if (InovatiqaDefaults.ShipSeparatelyOneItemEach)
                        {
                            request.Items.Add(new GetShippingOptionRequest.PackageItem(sci, product, 1));

                            for (var i = 0; i < sci.Quantity; i++)
                            {
                                separateRequests.Add(request);
                            }
                        }
                        else
                        {
                            request.Items.Add(new GetShippingOptionRequest.PackageItem(sci, product));
                            separateRequests.Add(request);
                        }
                    }
                    else
                    {
                        request.Items.Add(new GetShippingOptionRequest.PackageItem(sci, product));
                        requests.Add(warehouseId, request);
                    }
                }
            }

            shippingFromMultipleLocations = requests.Select(x => x.Key).Distinct().Count() > 1;

            var result = requests.Values.ToList();
            result.AddRange(separateRequests);

            return result;
        }

        public virtual GetShippingOptionResponse GetShippingOptions(IList<ShoppingCartItem> cart,
            Address shippingAddress, Customer customer = null, string allowedShippingRateComputationMethodSystemName = "",
            int storeId = 0)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            var result = new GetShippingOptionResponse();

            var shippingOptionRequests = CreateShippingOptionRequests(cart, shippingAddress, storeId, out var shippingFromMultipleLocations);
            result.ShippingFromMultipleLocations = shippingFromMultipleLocations;

            IList<ShippingOption> srcmShippingOptions = null;
            foreach (var shippingOptionRequest in shippingOptionRequests)
            {
                var getShippingOptionResponse = _shippingRateComputationMethodService.Value.GetShippingOptions(shippingOptionRequest);

                if (getShippingOptionResponse.Success)
                {
                    if (srcmShippingOptions == null)
                    {
                        srcmShippingOptions = getShippingOptionResponse.ShippingOptions;
                    }
                    else
                    {
                        srcmShippingOptions = srcmShippingOptions
                            .Where(existingso => getShippingOptionResponse.ShippingOptions.Any(newso => newso.Name == existingso.Name))
                            .ToList();

                        foreach (var existingso in srcmShippingOptions)
                        {
                            existingso.Rate += getShippingOptionResponse
                                .ShippingOptions
                                .First(newso => newso.Name == existingso.Name)
                                .Rate;
                        }
                    }
                }
                else
                {
                    foreach (var error in getShippingOptionResponse.Errors)
                    {
                        result.AddError(error);
                        _loggerService.Warning($"Shipping. {error}");
                    }
                    srcmShippingOptions = new List<ShippingOption>();
                    break;
                }
            }

            if (srcmShippingOptions == null)
                return null;

            foreach (var so in srcmShippingOptions)
            {
                if (InovatiqaDefaults.RoundPricesDuringCalculation)
                    so.Rate = _priceCalculationService.RoundPrice(so.Rate);

                so.ShippingRateComputationMethodSystemName = InovatiqaDefaults.FedExShippingMethodName;

                result.ShippingOptions.Add(so);
            }

            if (InovatiqaDefaults.ReturnValidOptionsIfThereAreAny)
            {
                if (result.ShippingOptions.Any() && result.Errors.Any())
                    result.Errors.Clear();
            }

            if (!result.ShippingOptions.Any() && !result.Errors.Any())
                result.Errors.Add("Shipping options could not be loaded");

            return result;
        }

        public virtual void GetDimensions(IList<GetShippingOptionRequest.PackageItem> packageItems,
            out decimal width, out decimal length, out decimal height, bool ignoreFreeShippedItems = false)
        {
            if (packageItems == null)
                throw new ArgumentNullException(nameof(packageItems));

            if (InovatiqaDefaults.UseCubeRootMethod && AreMultipleItems(packageItems))
            {
                var maxWidth = packageItems.Max(item => !item.Product.IsFreeShipping || !ignoreFreeShippedItems
                    ? item.Product.Width : decimal.Zero);
                var maxLength = packageItems.Max(item => !item.Product.IsFreeShipping || !ignoreFreeShippedItems
                    ? item.Product.Length : decimal.Zero);
                var maxHeight = packageItems.Max(item => !item.Product.IsFreeShipping || !ignoreFreeShippedItems
                    ? item.Product.Height : decimal.Zero);

                var totalVolume = packageItems.Sum(packageItem =>
                {
                    var productVolume = !packageItem.Product.IsFreeShipping || !ignoreFreeShippedItems ?
                        packageItem.Product.Width * packageItem.Product.Length * packageItem.Product.Height : decimal.Zero;

                    if (InovatiqaDefaults.ConsiderAssociatedProductsDimensions && !string.IsNullOrEmpty(packageItem.ShoppingCartItem.AttributesXml))
                    {
                        productVolume += _productAttributeParser.ParseProductAttributeValues(packageItem.ShoppingCartItem.AttributesXml)
                            .Where(attributeValue => attributeValue.AttributeValueTypeId == InovatiqaDefaults.AssociatedToProduct).Sum(attributeValue =>
                            {
                                var associatedProduct = _productService.GetProductById(attributeValue.AssociatedProductId);
                                if (associatedProduct == null || !associatedProduct.IsShipEnabled || (associatedProduct.IsFreeShipping && ignoreFreeShippedItems))
                                    return 0;

                                maxWidth = Math.Max(maxWidth, associatedProduct.Width);
                                maxLength = Math.Max(maxLength, associatedProduct.Length);
                                maxHeight = Math.Max(maxHeight, associatedProduct.Height);

                                return attributeValue.Quantity * associatedProduct.Width * associatedProduct.Length * associatedProduct.Height;
                            });
                    }

                    return productVolume * packageItem.GetQuantity();
                });

                width = length = height = Convert.ToDecimal(Math.Pow(Convert.ToDouble(totalVolume), 1.0 / 3.0));
                width = Math.Max(width, maxWidth);
                length = Math.Max(length, maxLength);
                height = Math.Max(height, maxHeight);
            }
            else
            {
                width = length = height = decimal.Zero;
                foreach (var packageItem in packageItems)
                {
                    var productWidth = decimal.Zero;
                    var productLength = decimal.Zero;
                    var productHeight = decimal.Zero;
                    if (!packageItem.Product.IsFreeShipping || !ignoreFreeShippedItems)
                    {
                        productWidth = packageItem.Product.Width;
                        productLength = packageItem.Product.Length;
                        productHeight = packageItem.Product.Height;
                    }

                    GetAssociatedProductDimensions(packageItem.ShoppingCartItem, out var associatedProductsWidth, out var associatedProductsLength, out var associatedProductsHeight);

                    var quantity = packageItem.GetQuantity();
                    width += (productWidth + associatedProductsWidth) * quantity;
                    length += (productLength + associatedProductsLength) * quantity;
                    height += (productHeight + associatedProductsHeight) * quantity;
                }
            }
        }

        public virtual void GetAssociatedProductDimensions(ShoppingCartItem shoppingCartItem,
            out decimal width, out decimal length, out decimal height, bool ignoreFreeShippedItems = false)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException(nameof(shoppingCartItem));

            width = length = height = decimal.Zero;

            if (!InovatiqaDefaults.ConsiderAssociatedProductsDimensions)
                return;

            if (string.IsNullOrEmpty(shoppingCartItem.AttributesXml))
                return;

            var attributeValues = _productAttributeParser.ParseProductAttributeValues(shoppingCartItem.AttributesXml)
                .Where(x => x.AttributeValueTypeId == InovatiqaDefaults.AssociatedToProduct).ToList();
            foreach (var attributeValue in attributeValues)
            {
                var associatedProduct = _productService.GetProductById(attributeValue.AssociatedProductId);
                if (associatedProduct == null || !associatedProduct.IsShipEnabled || (associatedProduct.IsFreeShipping && ignoreFreeShippedItems))
                    continue;

                width += associatedProduct.Width * attributeValue.Quantity;
                length += associatedProduct.Length * attributeValue.Quantity;
                height += associatedProduct.Height * attributeValue.Quantity;
            }
        }

        public virtual decimal GetTotalWeight(GetShippingOptionRequest request,
            bool includeCheckoutAttributes = true, bool ignoreFreeShippedItems = false)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var totalWeight = decimal.Zero;

            foreach (var packageItem in request.Items)
                totalWeight += GetShoppingCartItemWeight(packageItem.ShoppingCartItem, ignoreFreeShippedItems) * packageItem.GetQuantity();

            if (request.Customer is null || !includeCheckoutAttributes)
                return totalWeight;
            var checkoutAttributesXml = _genericAttributeService.GetAttribute<string>(request.Customer, InovatiqaDefaults.CheckoutAttributes, request.Customer.Id, InovatiqaDefaults.StoreId);
            if (string.IsNullOrEmpty(checkoutAttributesXml))
                return totalWeight;
            var attributeValues = _checkoutAttributeParserService.ParseCheckoutAttributeValues(checkoutAttributesXml);
            foreach (var attributeValue in attributeValues.SelectMany(x => x.values))
                totalWeight += attributeValue.WeightAdjustment;

            return totalWeight;
        }

        public virtual decimal GetShoppingCartItemWeight(ShoppingCartItem shoppingCartItem, bool ignoreFreeShippedItems = false)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException(nameof(shoppingCartItem));

            var product = _productService.GetProductById(shoppingCartItem.ProductId);

            return GetShoppingCartItemWeight(product, shoppingCartItem.AttributesXml, ignoreFreeShippedItems);
        }

        public virtual decimal GetShoppingCartItemWeight(Product product, string attributesXml, bool ignoreFreeShippedItems = false)
        {
            if (product == null)
                return decimal.Zero;

            var productWeight = !product.IsFreeShipping || !ignoreFreeShippedItems ? product.Weight : decimal.Zero;

            var attributesTotalWeight = decimal.Zero;

            if (!InovatiqaDefaults.ConsiderAssociatedProductsDimensions || string.IsNullOrEmpty(attributesXml))
                return productWeight + attributesTotalWeight;

            var attributeValues = _productAttributeParser.ParseProductAttributeValues(attributesXml);
            foreach (var attributeValue in attributeValues)
            {
                switch (attributeValue.AttributeValueTypeId)
                {
                    case InovatiqaDefaults.Simple:
                        attributesTotalWeight += attributeValue.WeightAdjustment;
                        break;
                    case InovatiqaDefaults.AssociatedToProduct:
                        var associatedProduct = _productService.GetProductById(attributeValue.AssociatedProductId);
                        if (associatedProduct != null && associatedProduct.IsShipEnabled && (!associatedProduct.IsFreeShipping || !ignoreFreeShippedItems))
                            attributesTotalWeight += associatedProduct.Weight * attributeValue.Quantity;
                        break;
                }
            }

            return productWeight + attributesTotalWeight;
        }

        public virtual GetPickupPointsResponse GetPickupPoints(int addressId, Customer customer = null,
            string providerSystemName = null, int storeId = 0)
        {
            var result = new GetPickupPointsResponse();

            //var pickupPointsProviders = _pickupPluginManager.LoadActivePlugins(customer, storeId, providerSystemName);
            //if (!pickupPointsProviders.Any())
            //    return result;

            //var allPickupPoints = new List<PickupPoint>();
            //foreach (var provider in pickupPointsProviders)
            //{
            //    var pickPointsResponse = provider.GetPickupPoints(_addressService.GetAddressById(addressId));
            //    if (pickPointsResponse.Success)
            //        allPickupPoints.AddRange(pickPointsResponse.PickupPoints);
            //    else
            //    {
            //        foreach (var error in pickPointsResponse.Errors)
            //        {
            //            result.AddError(error);
            //            _loggerService.Warning($"PickupPoints ({provider.PluginDescriptor.FriendlyName}). {error}");
            //        }
            //    }
            //}

            //if (allPickupPoints.Count <= 0)
            //    return result;

            //result.Errors.Clear();
            //result.PickupPoints = allPickupPoints.OrderBy(point => point.DisplayOrder).ThenBy(point => point.Name).ToList();

            return result;
        }

        #endregion

        #endregion
    }
}