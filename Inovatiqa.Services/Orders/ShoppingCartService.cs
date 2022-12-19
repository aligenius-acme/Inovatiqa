using System;
using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Net;
using Inovatiqa.Services.Seo.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Inovatiqa.Database.Extensions;
using Inovatiqa.Services.Catalog.Extensions;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;

namespace Inovatiqa.Services.Orders
{
    public partial class ShoppingCartService : IShoppingCartService
    {
        #region Fields

        private readonly IRepository<ShoppingCartItem> _sciRepository;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IProductAttributeParserService _productAttributeParserService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IShippingService _shippingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IDateRangeService _dateRangeService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICheckoutAttributeParserService _checkoutAttributeParserService;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IRepository<WishList> _wishListRepository;
        private readonly IRepository<ShoppingCartItem> _shoppingCartItemRepository;
        private readonly IRepository<SuspendedCart> _suspendedCartRepository;
        private readonly IAddressService _addressService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEmailAccountService _emailAccountService;

        #endregion

        #region Ctor

        public ShoppingCartService(IRepository<ShoppingCartItem> sciRepository,
            ICustomerService customerService,
            IProductService productService,
            IProductAttributeParserService productAttributeParserService,
            IPriceCalculationService priceCalculationService,
            IShippingService shippingService,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IUrlRecordService urlRecordService,
            IDateRangeService dateRangeService,
            IProductAttributeService productAttributeService,
            IGenericAttributeService genericAttributeService,
            ICheckoutAttributeParserService checkoutAttributeParserService,
            ICheckoutAttributeService checkoutAttributeService,
            IRepository<WishList> wishListRepository,
            IRepository<ShoppingCartItem> shoppingCartItemRepository,
            IRepository<SuspendedCart> suspendedCartRepository,
            IAddressService addressService,
            IQueuedEmailService queuedEmailService,
            IEmailAccountService emailAccountService)
        {
            _sciRepository = sciRepository;
            _customerService = customerService;
            _productService = productService;
            _productAttributeParserService = productAttributeParserService;
            _priceCalculationService = priceCalculationService;
            _shippingService = shippingService;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _urlRecordService = urlRecordService;
            _dateRangeService = dateRangeService;
            _productAttributeService = productAttributeService;
            _genericAttributeService = genericAttributeService;
            _checkoutAttributeParserService = checkoutAttributeParserService;
            _checkoutAttributeService = checkoutAttributeService;
            _wishListRepository = wishListRepository;
            _shoppingCartItemRepository = shoppingCartItemRepository;
            _suspendedCartRepository = suspendedCartRepository;
            _addressService = addressService;
            _queuedEmailService = queuedEmailService;
            _emailAccountService = emailAccountService;
        }

        #endregion

        #region Utilities

        protected virtual bool ShoppingCartItemIsEqual(ShoppingCartItem shoppingCartItem,
     Product product,
     string attributesXml,
     decimal customerEnteredPrice,
     DateTime? rentalStartDate,
     DateTime? rentalEndDate)
        {
            if (shoppingCartItem.ProductId != product.Id)
                return false;

            var attributesEqual = _productAttributeParserService.AreProductAttributesEqual(shoppingCartItem.AttributesXml, attributesXml, false, false);
            if (!attributesEqual)
                return false;

            if (product.CustomerEntersPrice)
            {
                var customerEnteredPricesEqual = Math.Round(shoppingCartItem.CustomerEnteredPrice, 2) == Math.Round(customerEnteredPrice, 2);
                if (!customerEnteredPricesEqual)
                    return false;
            }

            if (!product.IsRental)
                return true;

            var rentalInfoEqual = shoppingCartItem.RentalStartDateUtc == rentalStartDate && shoppingCartItem.RentalEndDateUtc == rentalEndDate;

            return rentalInfoEqual;
        }

        protected virtual bool IsCustomerShoppingCartEmpty(Customer customer)
        {
            return !_sciRepository.Query().Any(sci => sci.CustomerId == customer.Id);
        }

        #endregion

        #region Methods

        public virtual IList<ShoppingCartItem> GetShoppingCart(Customer customer, int shoppingCartTypeId = 0,
            int storeId = 0, int? productId = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int? wishListId = 0, bool getallShoppingCarts = false)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var items = _sciRepository.Query().Where(sci => sci.CustomerId == customer.Id);
            if (!getallShoppingCarts)
            {
                if (shoppingCartTypeId > 0 && wishListId == 0)
                    items = items.Where(item => item.ShoppingCartTypeId == shoppingCartTypeId);
                else
                    items = items.Where(item => item.ShoppingCartTypeId == shoppingCartTypeId && item.WishListId == wishListId);
            }
            if (productId > 0)
                items = items.Where(item => item.ProductId == productId);

            if (createdFromUtc.HasValue)
                items = items.Where(item => createdFromUtc.Value <= item.CreatedOnUtc);
            if (createdToUtc.HasValue)
                items = items.Where(item => createdToUtc.Value >= item.CreatedOnUtc);

            return items.ToList();
        }

        public virtual IList<SuspendedCart> GetCustomerSuspendedCarts(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));
            if(customer.ParentId != null && customer.ParentId != 0)
            {
                // customer is child account
                // get parent account
                customer = _customerService.GetCustomerById(Convert.ToInt32(customer.ParentId));
            }
            var AllChilds = _customerService.getAllChildAccounts(customer);
            var items = new List<SuspendedCart>();
            foreach(var child in AllChilds)
            {
                foreach(var item in _suspendedCartRepository.Query().Where(scr => scr.CustomerId == child).ToList())
                {
                    items.Add(item);
                }
            }
            //var items = _suspendedCartRepository.Query().Where(scr => scr.CustomerId == customer.Id);

            return items;
        }

        public virtual decimal GetSubTotal(ShoppingCartItem shoppingCartItem,
            bool includeDiscounts = true)
        {
            return GetSubTotal(shoppingCartItem, includeDiscounts, out var _, out var _, out var _);
        }

        public virtual decimal GetSubTotal(ShoppingCartItem shoppingCartItem,
            bool includeDiscounts,
            out decimal discountAmount,
            out List<Discount> appliedDiscounts,
            out int? maximumDiscountQty)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException(nameof(shoppingCartItem));

            decimal subTotal;
            maximumDiscountQty = null;

            var unitPrice = GetUnitPrice(shoppingCartItem, includeDiscounts,
                out discountAmount, out appliedDiscounts);

            if (appliedDiscounts.Any())
            {
                Discount oneAndOnlyDiscount = null;
                if (appliedDiscounts.Count == 1)
                    oneAndOnlyDiscount = appliedDiscounts.First();

                if ((oneAndOnlyDiscount?.MaximumDiscountedQuantity.HasValue ?? false) &&
                    shoppingCartItem.Quantity > oneAndOnlyDiscount.MaximumDiscountedQuantity.Value)
                {
                    maximumDiscountQty = oneAndOnlyDiscount.MaximumDiscountedQuantity.Value;
                    var discountedQuantity = oneAndOnlyDiscount.MaximumDiscountedQuantity.Value;
                    var discountedSubTotal = unitPrice * discountedQuantity;
                    discountAmount *= discountedQuantity;

                    var notDiscountedQuantity = shoppingCartItem.Quantity - discountedQuantity;
                    var notDiscountedUnitPrice = GetUnitPrice(shoppingCartItem, false);
                    var notDiscountedSubTotal = notDiscountedUnitPrice * notDiscountedQuantity;

                    subTotal = discountedSubTotal + notDiscountedSubTotal;
                }
                else
                {
                    discountAmount *= shoppingCartItem.Quantity;

                    subTotal = unitPrice * shoppingCartItem.Quantity;
                }
            }
            else
            {
                subTotal = unitPrice * shoppingCartItem.Quantity;
            }

            return subTotal;
        }

        public virtual decimal GetUnitPrice(ShoppingCartItem shoppingCartItem,
           bool includeDiscounts = true)
        {
            return GetUnitPrice(shoppingCartItem, includeDiscounts, out _, out _);
        }

        public virtual decimal GetUnitPrice(ShoppingCartItem shoppingCartItem,
            bool includeDiscounts,
            out decimal discountAmount,
            out List<Discount> appliedDiscounts)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException(nameof(shoppingCartItem));

            var customer = _customerService.GetCustomerById(shoppingCartItem.CustomerId);
            var product = _productService.GetProductById(shoppingCartItem.ProductId);

            return GetUnitPrice(product,
                customer,
                shoppingCartItem.ShoppingCartTypeId,
                shoppingCartItem.Quantity,
                shoppingCartItem.AttributesXml,
                shoppingCartItem.CustomerEnteredPrice,
                shoppingCartItem.RentalStartDateUtc,
                shoppingCartItem.RentalEndDateUtc,
                includeDiscounts,
                out discountAmount,
                out appliedDiscounts);
        }

        public virtual decimal GetUnitPrice(Product product,
            Customer customer,
            int shoppingCartTypeId,
            int quantity,
            string attributesXml,
            decimal customerEnteredPrice,
            DateTime? rentalStartDate, DateTime? rentalEndDate,
            bool includeDiscounts,
            out decimal discountAmount,
            out List<Discount> appliedDiscounts)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            discountAmount = decimal.Zero;
            appliedDiscounts = new List<Discount>();

            decimal finalPrice;

            var combination = _productAttributeParserService.FindProductAttributeCombination(product, attributesXml);
            if (combination?.OverriddenPrice.HasValue ?? false)
            {
                finalPrice = _priceCalculationService.GetFinalPrice(product,
                        customer,
                        combination.OverriddenPrice.Value,
                        decimal.Zero,
                        includeDiscounts,
                        quantity,
                        product.IsRental ? rentalStartDate : null,
                        product.IsRental ? rentalEndDate : null,
                        out discountAmount, out appliedDiscounts);
            }
            else
            {
                var attributesTotalPrice = decimal.Zero;
                var attributeValues = _productAttributeParserService.ParseProductAttributeValues(attributesXml);
                if (attributeValues != null)
                {
                    foreach (var attributeValue in attributeValues)
                    {
                        attributesTotalPrice += _priceCalculationService.GetProductAttributeValuePriceAdjustment(product, attributeValue, customer, product.CustomerEntersPrice ? (decimal?)customerEnteredPrice : null);
                    }
                }

                int qty = quantity;

                finalPrice = _priceCalculationService.GetFinalPrice(product,
                    customer,
                    attributesTotalPrice,
                    includeDiscounts,
                    qty,
                    product.IsRental ? rentalStartDate : null,
                    product.IsRental ? rentalEndDate : null,
                    out discountAmount, out appliedDiscounts);
            }
            
            return finalPrice;
        }

        public virtual bool ShoppingCartRequiresShipping(IList<ShoppingCartItem> shoppingCart)
        {
            return shoppingCart.Any(shoppingCartItem => _shippingService.IsShipEnabled(shoppingCartItem));
        }

        public virtual ShoppingCartItem FindShoppingCartItemInTheCart(IList<ShoppingCartItem> shoppingCart,
            int shoppingCartTypeId,
            Product product,
            string attributesXml = "",
            decimal customerEnteredPrice = decimal.Zero,
            DateTime? rentalStartDate = null,
            DateTime? rentalEndDate = null,
            int wishListId = 0)
        {
            if (shoppingCart == null)
                throw new ArgumentNullException(nameof(shoppingCart));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (wishListId == 0)
            {
                return shoppingCart.Where(sci => sci.ShoppingCartTypeId == shoppingCartTypeId)
                    .FirstOrDefault(sci => ShoppingCartItemIsEqual(sci, product, attributesXml, customerEnteredPrice, rentalStartDate, rentalEndDate));
            }
            else
                return shoppingCart.Where(sci => sci.ShoppingCartTypeId == shoppingCartTypeId && sci.WishListId == wishListId)
                    .FirstOrDefault(sci => ShoppingCartItemIsEqual(sci, product, attributesXml, customerEnteredPrice, rentalStartDate, rentalEndDate));
        }

        public virtual IList<string> GetShoppingCartItemWarnings(Customer customer, int shoppingCartTypeId,
            Product product, int storeId,
            string attributesXml, decimal customerEnteredPrice,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool addRequiredProducts = true, int shoppingCartItemId = 0,
            bool getStandardWarnings = true, bool getAttributesWarnings = true,
            bool getGiftCardWarnings = true, bool getRequiredProductWarnings = true,
            bool getRentalWarnings = true)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();

            if (getStandardWarnings)
                warnings.AddRange(GetStandardWarnings(customer, shoppingCartTypeId, product, attributesXml, customerEnteredPrice, quantity));

            if (getAttributesWarnings)
                warnings.AddRange(GetShoppingCartItemAttributeWarnings(customer, shoppingCartTypeId, product, quantity, attributesXml));

            if (getRequiredProductWarnings)
                warnings.AddRange(GetRequiredProductWarnings(customer, shoppingCartTypeId, product, storeId, quantity, addRequiredProducts, shoppingCartItemId));

            return warnings;
        }

        public virtual IList<string> GetRequiredProductWarnings(Customer customer, int shoppingCartTypeId, Product product,
            int storeId, int quantity, bool addRequiredProducts, int shoppingCartItemId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();

            var requiredProductQuantity = 1;

            var cart = GetShoppingCart(customer, shoppingCartTypeId, storeId);

            var productsRequiringProduct = GetProductsRequiringProduct(cart, product);

            var passedProductRequiredQuantity = cart.Where(ci => productsRequiringProduct.Any(p => p.Id == ci.ProductId))
                .Sum(item => item.Quantity * requiredProductQuantity);

            if (passedProductRequiredQuantity > quantity)
                warnings.Add(string.Format("This product is required in the quantity of {0}", passedProductRequiredQuantity));

            if (!product.RequireOtherProducts)
                return warnings;

            var requiredProducts = _productService.GetProductsByIds(_productService.ParseRequiredProductIds(product));
            if (!requiredProducts.Any())
                return warnings;

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var warningLocale = "This product requires the following product is added to the cart in the quantity of {1}: {0}";
            foreach (var requiredProduct in requiredProducts)
            {
                var productsRequiringRequiredProduct = GetProductsRequiringProduct(cart, requiredProduct);

                var requiredProductRequiredQuantity = quantity * requiredProductQuantity +

                    cart.Where(ci => productsRequiringRequiredProduct.Any(p => p.Id == ci.ProductId))
                        .Where(item => item.Id != shoppingCartItemId)
                        .Sum(item => item.Quantity * requiredProductQuantity);

                var quantityToAdd = requiredProductRequiredQuantity - (cart.FirstOrDefault(item => item.ProductId == requiredProduct.Id)?.Quantity ?? 0);
                if (quantityToAdd <= 0)
                    continue;

                var requiredProductName = WebUtility.HtmlEncode(requiredProduct.Name);
                var requiredProductWarning = InovatiqaDefaults.UseLinksInRequiredProductWarnings
                    ? string.Format(warningLocale, $"<a href=\"{urlHelper.RouteUrl(nameof(Product), new { SeName = _urlRecordService.GetActiveSlug(requiredProduct.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId) })}\">{requiredProductName}</a>", requiredProductRequiredQuantity)
                    : string.Format(warningLocale, requiredProductName, requiredProductRequiredQuantity);

                if (addRequiredProducts && product.AutomaticallyAddRequiredProducts)
                {
                    var addToCartWarnings = AddToCart(customer, requiredProduct, shoppingCartTypeId, storeId,
                        quantity: quantityToAdd, addRequiredProducts: false);

                    if (addToCartWarnings.Any())
                        warnings.Add(requiredProductWarning);
                }
                else
                    warnings.Add(requiredProductWarning);
            }

            return warnings;
        }

        public virtual IEnumerable<Product> GetProductsRequiringProduct(IList<ShoppingCartItem> cart, Product product)
        {
            if (cart is null)
                throw new ArgumentNullException(nameof(cart));

            if (product is null)
                throw new ArgumentNullException(nameof(product));

            if (cart.Count == 0)
                yield break;

            var productIds = cart.Select(ci => ci.ProductId).ToArray();

            var cartProducts = _productService.GetProductsByIds(productIds);

            foreach (var cartProduct in cartProducts)
            {
                if (!cartProduct.RequireOtherProducts && _productService.ParseRequiredProductIds(cartProduct).Contains(product.Id))
                    yield return cartProduct;
            }
        }

        public virtual IList<string> AddToCart(Customer customer, Product product,
            int shoppingCartTypeId, int storeId, string attributesXml = null,
            decimal customerEnteredPrice = decimal.Zero,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool addRequiredProducts = true, int wishListId = 0, bool reordered = false)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();


            if (customer.IsSearchEngineAccount())
            {
                warnings.Add("Search engine can't add to cart");
                return warnings;
            }

            if (quantity <= 0)
            {
                warnings.Add("Quantity should be positive");
                return warnings;
            }

            _customerService.ResetCheckoutData(customer, storeId);

            var cart = GetShoppingCart(customer, shoppingCartTypeId, storeId);

            var shoppingCartItem = FindShoppingCartItemInTheCart(cart,
                shoppingCartTypeId, product, attributesXml, customerEnteredPrice,
                rentalStartDate, rentalEndDate, wishListId);

            if (shoppingCartItem != null)
            {
                var newQuantity = shoppingCartItem.Quantity + quantity;
                warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartTypeId, product,
                    storeId, attributesXml,
                    customerEnteredPrice, rentalStartDate, rentalEndDate,
                    newQuantity, addRequiredProducts, shoppingCartItem.Id));

                if (warnings.Any())
                    return warnings;

                shoppingCartItem.AttributesXml = attributesXml;
                shoppingCartItem.Quantity = newQuantity;
                shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;

                shoppingCartItem.Reordered = reordered;

                if (wishListId > 0)
                    shoppingCartItem.WishListId = wishListId;

                _sciRepository.Update(shoppingCartItem);
                if(shoppingCartTypeId == (int)ShoppingCartType.ShoppingCart)
                    UpdatePurchaseStatus(customer);
                //_eventPublisher.EntityUpdated(shoppingCartItem);
            }
            else
            {
                warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartTypeId, product,
                    storeId, attributesXml, customerEnteredPrice,
                    rentalStartDate, rentalEndDate,
                    quantity, addRequiredProducts));

                if (warnings.Any())
                    return warnings;

                switch (shoppingCartTypeId)
                {
                    case (int)ShoppingCartType.ShoppingCart:
                        if (cart.Count >= InovatiqaDefaults.MaximumShoppingCartItems)
                        {
                            warnings.Add(string.Format("Maximum shopping cart items", InovatiqaDefaults.MaximumShoppingCartItems));
                            return warnings;
                        }

                        break;
                    case (int)ShoppingCartType.Wishlist:
                        if (cart.Count >= InovatiqaDefaults.MaximumWishlistItems)
                        {
                            warnings.Add(string.Format("Maximum wishlist items", InovatiqaDefaults.MaximumWishlistItems));
                            return warnings;
                        }

                        break;
                    default:
                        break;
                }

                var now = DateTime.UtcNow;
                shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartTypeId = shoppingCartTypeId,
                    StoreId = storeId,
                    ProductId = product.Id,
                    AttributesXml = attributesXml,
                    CustomerEnteredPrice = customerEnteredPrice,
                    Quantity = quantity,
                    RentalStartDateUtc = rentalStartDate,
                    RentalEndDateUtc = rentalEndDate,
                    CreatedOnUtc = now,
                    UpdatedOnUtc = now,
                    CustomerId = customer.Id,
                    WishListId = wishListId,
                    Reordered = reordered
                };

                _sciRepository.Insert(shoppingCartItem);
                UpdatePurchaseStatus(customer);
                customer.HasShoppingCartItems = !IsCustomerShoppingCartEmpty(customer);
                _customerService.UpdateCustomer(customer);

                //_eventPublisher.EntityInserted(shoppingCartItem);
            }

            return warnings;
        }

        public virtual IList<string> GetStandardWarnings(Customer customer, int shoppingCartTypeId,
            Product product, string attributesXml, decimal customerEnteredPrice,
            int quantity)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();

            if (product.Deleted)
            {
                warnings.Add("Product is deleted");
                return warnings;
            }

            if (product.ProductTypeId != InovatiqaDefaults.SimpleProduct)
            {
                warnings.Add("This is not simple product");
            }

            if (shoppingCartTypeId == (int)ShoppingCartType.ShoppingCart && product.DisableBuyButton)
            {
                warnings.Add("Buying is disabled for this product");
            }

            if (shoppingCartTypeId == (int)ShoppingCartType.Wishlist && product.DisableWishlistButton)
            {
                warnings.Add("Wishlist is disabled for this product");
            }

            var hasQtyWarnings = false;
            if (quantity < product.OrderMinimumQuantity)
            {
                warnings.Add(string.Format("The minimum quantity allowed for purchase is {0}.", product.OrderMinimumQuantity));
                hasQtyWarnings = true;
            }

            if (quantity > product.OrderMaximumQuantity)
            {
                warnings.Add(string.Format("The maximum quantity allowed for purchase is {0}.", product.OrderMaximumQuantity));
                hasQtyWarnings = true;
            }

            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            if (allowedQuantities.Length > 0 && !allowedQuantities.Contains(quantity))
            {
                warnings.Add(string.Format("Allowed quantities for this product: {0}", string.Join(", ", allowedQuantities)));
            }

            var validateOutOfStock = shoppingCartTypeId == (int)ShoppingCartType.ShoppingCart || !InovatiqaDefaults.AllowOutOfStockItemsToBeAddedToWishlist;
            if (validateOutOfStock && !hasQtyWarnings)
            {
                switch (product.ManageInventoryMethodId)
                {
                    case InovatiqaDefaults.DontManageStock:
                        break;
                    case InovatiqaDefaults.ManageStock:
                        if (product.BackorderModeId == (int)BackorderMode.NoBackorders)
                        {
                            var maximumQuantityCanBeAdded = _productService.GetTotalStockQuantity(product);
                            if (maximumQuantityCanBeAdded < quantity)
                            {
                                if (maximumQuantityCanBeAdded <= 0)
                                {
                                    var productAvailabilityRange = _dateRangeService.GetProductAvailabilityRangeById(product.ProductAvailabilityRangeId);
                                    var warning = productAvailabilityRange == null ? "Out of stock"
                                        : string.Format("Available in {0}",
                                            productAvailabilityRange.Name);
                                    warnings.Add(warning);
                                }
                                else
                                    warnings.Add(string.Format("Your quantity exceeds stock on hand. The maximum quantity that can be added is {0}.", maximumQuantityCanBeAdded));
                            }
                        }

                        break;
                    case InovatiqaDefaults.ManageStockByAttributes:
                        var combination = _productAttributeParserService.FindProductAttributeCombination(product, attributesXml);
                        if (combination != null)
                        {
                            if (!combination.AllowOutOfStockOrders && combination.StockQuantity < quantity)
                            {
                                var maximumQuantityCanBeAdded = combination.StockQuantity;
                                if (maximumQuantityCanBeAdded <= 0)
                                {
                                    var productAvailabilityRange = _dateRangeService.GetProductAvailabilityRangeById(product.ProductAvailabilityRangeId);
                                    var warning = productAvailabilityRange == null ? "Out of stock"
                                        : string.Format("Available in {0}",
                                            productAvailabilityRange.Name);
                                    warnings.Add(warning);
                                }
                                else
                                {
                                    warnings.Add(string.Format("Your quantity exceeds stock on hand. The maximum quantity that can be added is {0}.", maximumQuantityCanBeAdded));
                                }
                            }
                        }
                        else
                        {
                            if (product.AllowAddingOnlyExistingAttributeCombinations)
                            {
                                var productAvailabilityRange = _dateRangeService.GetProductAvailabilityRangeById(product.ProductAvailabilityRangeId);
                                var warning = productAvailabilityRange == null ? "Out of stock"
                                    : string.Format("Available in {0}",
                                        productAvailabilityRange.Name);
                                warnings.Add(warning);
                            }
                        }

                        break;
                    default:
                        break;
                }
            }

            var availableStartDateError = false;
            if (product.AvailableStartDateTimeUtc.HasValue)
            {
                var availableStartDateTime = DateTime.SpecifyKind(product.AvailableStartDateTimeUtc.Value, DateTimeKind.Utc);
                if (availableStartDateTime.CompareTo(DateTime.UtcNow) > 0)
                {
                    warnings.Add("Product is not available");
                    availableStartDateError = true;
                }
            }

            if (!product.AvailableEndDateTimeUtc.HasValue || availableStartDateError)
                return warnings;

            var availableEndDateTime = DateTime.SpecifyKind(product.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc);
            if (availableEndDateTime.CompareTo(DateTime.UtcNow) < 0)
            {
                warnings.Add("Product is not available");
            }

            return warnings;
        }

        public virtual IList<string> GetShoppingCartItemAttributeWarnings(Customer customer,
            int shoppingCartTypeId,
            Product product,
            int quantity = 1,
            string attributesXml = "",
            bool ignoreNonCombinableAttributes = false,
            bool ignoreConditionMet = false)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();

            var attributes1 = _productAttributeParserService.ParseProductAttributeMappings(attributesXml);
            if (ignoreNonCombinableAttributes)
            {
                attributes1 = attributes1.Where(x => !x.IsNonCombinable()).ToList();
            }

            foreach (var attribute in attributes1)
            {
                if (attribute.ProductId == 0)
                {
                    warnings.Add("Attribute error");
                    return warnings;
                }

                if (attribute.ProductId != product.Id)
                {
                    warnings.Add("Attribute error");
                }
            }

            var attributes2 = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            if (ignoreNonCombinableAttributes)
            {
                attributes2 = attributes2.Where(x => !x.IsNonCombinable()).ToList();
            }

            if (!ignoreConditionMet)
            {
                attributes2 = attributes2.Where(x =>
                {
                    var conditionMet = _productAttributeParserService.IsConditionMet(x, attributesXml);
                    return !conditionMet.HasValue || conditionMet.Value;
                }).ToList();
            }

            foreach (var a2 in attributes2)
            {
                if (a2.IsRequired)
                {
                    var found = false;
                    foreach (var a1 in attributes1)
                    {
                        if (a1.Id != a2.Id)
                            continue;

                        var attributeValuesStr = _productAttributeParserService.ParseValues(attributesXml, a1.Id);

                        foreach (var str1 in attributeValuesStr)
                        {
                            if (string.IsNullOrEmpty(str1.Trim()))
                                continue;

                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        var productAttribute = _productAttributeService.GetProductAttributeById(a2.ProductAttributeId);

                        var textPrompt = a2.TextPrompt;
                        var notFoundWarning = !string.IsNullOrEmpty(textPrompt) ?
                            textPrompt :
                            string.Format("Please select {0}", productAttribute.Name);

                        warnings.Add(notFoundWarning);
                    }
                }

                if (a2.AttributeControlTypeId != (int)AttributeControlType.ReadonlyCheckboxes)
                    continue;

                var allowedReadOnlyValueIds = _productAttributeService.GetProductAttributeValues(a2.Id)
                    .Where(x => x.IsPreSelected)
                    .Select(x => x.Id)
                    .ToArray();

                var selectedReadOnlyValueIds = _productAttributeParserService.ParseProductAttributeValues(attributesXml)
                    .Where(x => x.ProductAttributeMappingId == a2.Id)
                    .Select(x => x.Id)
                    .ToArray();

                if (!CommonHelper.ArraysEqual(allowedReadOnlyValueIds, selectedReadOnlyValueIds))
                {
                    warnings.Add("You cannot change read-only values");
                }
            }

            foreach (var pam in attributes2)
            {
                if (!pam.ValidationRulesAllowed())
                    continue;

                string enteredText;
                int enteredTextLength;

                var productAttribute = _productAttributeService.GetProductAttributeById(pam.ProductAttributeId);

                if (pam.ValidationMinLength.HasValue)
                {
                    if (pam.AttributeControlTypeId == (int)AttributeControlType.TextBox ||
                        pam.AttributeControlTypeId == (int)AttributeControlType.MultilineTextbox)
                    {
                        enteredText = _productAttributeParserService.ParseValues(attributesXml, pam.Id).FirstOrDefault();
                        enteredTextLength = string.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                        if (pam.ValidationMinLength.Value > enteredTextLength)
                        {
                            warnings.Add(string.Format("{0} : minimum length is {1} chars", productAttribute.Name, pam.ValidationMinLength.Value));
                        }
                    }
                }

                if (!pam.ValidationMaxLength.HasValue)
                    continue;

                if (pam.AttributeControlTypeId != (int)AttributeControlType.TextBox && pam.AttributeControlTypeId != (int)AttributeControlType.MultilineTextbox)
                    continue;

                enteredText = _productAttributeParserService.ParseValues(attributesXml, pam.Id).FirstOrDefault();
                enteredTextLength = string.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                if (pam.ValidationMaxLength.Value < enteredTextLength)
                {
                    warnings.Add(string.Format("{0} : maximum length is {1} chars", productAttribute.Name, pam.ValidationMaxLength.Value));
                }
            }

            if (warnings.Any())
                return warnings;

            var attributeValues = _productAttributeParserService.ParseProductAttributeValues(attributesXml);
            foreach (var attributeValue in attributeValues)
            {
                if (attributeValue.AttributeValueTypeId != InovatiqaDefaults.AssociatedToProduct)
                    continue;

                var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(attributeValue.ProductAttributeMappingId);

                if (ignoreNonCombinableAttributes && productAttributeMapping != null && productAttributeMapping.IsNonCombinable())
                    continue;

                var associatedProduct = _productService.GetProductById(attributeValue.AssociatedProductId);
                if (associatedProduct != null)
                {
                    var totalQty = quantity * attributeValue.Quantity;
                    var associatedProductWarnings = GetShoppingCartItemWarnings(customer,
                        shoppingCartTypeId, associatedProduct, InovatiqaDefaults.StoreId,
                        string.Empty, decimal.Zero, null, null, totalQty, false);

                    var productAttribute = _productAttributeService.GetProductAttributeById(productAttributeMapping.ProductAttributeId);

                    foreach (var associatedProductWarning in associatedProductWarnings)
                    {
                        var attributeName = productAttribute.Name;
                        var attributeValueName = attributeValue.Name;
                        warnings.Add(string.Format(
                            "{0}. {1}. {2}",
                            attributeName, attributeValueName, associatedProductWarning));
                    }
                }
                else
                {
                    warnings.Add($"Associated product cannot be loaded - {attributeValue.AssociatedProductId}");
                }
            }

            return warnings;
        }

        public virtual void MigrateShoppingCart(Customer fromCustomer, Customer toCustomer, bool includeCouponCodes)
        {
            if (fromCustomer == null)
                throw new ArgumentNullException(nameof(fromCustomer));
            if (toCustomer == null)
                throw new ArgumentNullException(nameof(toCustomer));

            if (fromCustomer.Id == toCustomer.Id)
                return;

            var fromCart = GetShoppingCart(fromCustomer);

            for (var i = 0; i < fromCart.Count; i++)
            {
                var sci = fromCart[i];
                var product = _productService.GetProductById(sci.ProductId);

                AddToCart(toCustomer, product, sci.ShoppingCartTypeId, sci.StoreId,
                    sci.AttributesXml, sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, false);
            }

            for (var i = 0; i < fromCart.Count; i++)
            {
                var sci = fromCart[i];
                DeleteShoppingCartItem(sci);
            }

            var checkoutAttributesXml = _genericAttributeService.GetAttribute<string>(fromCustomer, InovatiqaDefaults.CheckoutAttributes, fromCustomer.Id, InovatiqaDefaults.StoreId);
            _genericAttributeService.SaveAttribute<string>(toCustomer.GetType().Name, toCustomer.Id, InovatiqaDefaults.CheckoutAttributes, checkoutAttributesXml, InovatiqaDefaults.StoreId);

        }

        public virtual void DeleteShoppingCartItem(ShoppingCartItem shoppingCartItem, bool resetCheckoutData = true,
            bool ensureOnlyActiveCheckoutAttributes = false)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException(nameof(shoppingCartItem));

            var customer = _customerService.GetCustomerById(shoppingCartItem.CustomerId);
            var storeId = shoppingCartItem.StoreId;

            if (resetCheckoutData)
            {
                _customerService.ResetCheckoutData(customer, shoppingCartItem.StoreId);
            }

            _sciRepository.Delete(shoppingCartItem);

            customer.HasShoppingCartItems = !IsCustomerShoppingCartEmpty(customer);
            _customerService.UpdateCustomer(customer);

            if (ensureOnlyActiveCheckoutAttributes &&
                shoppingCartItem.ShoppingCartTypeId == (int)ShoppingCartType.ShoppingCart)
            {
                var cart = GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, storeId);

                var checkoutAttributesXml =
                    _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CheckoutAttributes, customer.Id,
                        storeId);
                checkoutAttributesXml =
                    _checkoutAttributeParserService.EnsureOnlyActiveAttributes(checkoutAttributesXml, cart);

                _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.CheckoutAttributes, checkoutAttributesXml, InovatiqaDefaults.StoreId);


            }

            //_eventPublisher.EntityDeleted(shoppingCartItem);

            if (!InovatiqaDefaults.RemoveRequiredProducts)
                return;

            var product = _productService.GetProductById(shoppingCartItem.ProductId);
            if (!product?.RequireOtherProducts ?? true)
                return;

            var requiredProductIds = _productService.ParseRequiredProductIds(product);
            var requiredShoppingCartItems =
                GetShoppingCart(customer, shoppingCartTypeId: shoppingCartItem.ShoppingCartTypeId)
                    .Where(item => requiredProductIds.Any(id => id == item.ProductId))
                    .ToList();

            foreach (var cartItem in requiredShoppingCartItems)
            {
                var requiredProductQuantity = 1;

                UpdateShoppingCartItem(customer, cartItem.Id, cartItem.AttributesXml, cartItem.CustomerEnteredPrice,
                    quantity: cartItem.Quantity - shoppingCartItem.Quantity * requiredProductQuantity,
                    resetCheckoutData: false);
            }
        }

        public virtual void DeleteShoppingCartItem(int shoppingCartItemId, bool resetCheckoutData = true,
            bool ensureOnlyActiveCheckoutAttributes = false)
        {
            var shoppingCartItem = _sciRepository.Query().FirstOrDefault(sci => sci.Id == shoppingCartItemId);
            if (shoppingCartItem != null)
            {
                DeleteShoppingCartItem(shoppingCartItem, resetCheckoutData, ensureOnlyActiveCheckoutAttributes);
            }
        }

        public virtual IList<string> UpdateShoppingCartItem(Customer customer,
            int shoppingCartItemId, string attributesXml,
            decimal customerEnteredPrice,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool resetCheckoutData = true)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var warnings = new List<string>();

            var shoppingCartItem = _sciRepository.GetById(shoppingCartItemId);

            if (shoppingCartItem == null || shoppingCartItem.CustomerId != customer.Id)
                return warnings;

            if (resetCheckoutData)
            {
                _customerService.ResetCheckoutData(customer, shoppingCartItem.StoreId);
            }

            var product = _productService.GetProductById(shoppingCartItem.ProductId);

            if (quantity > 0)
            {
                warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartItem.ShoppingCartTypeId,
                    product, shoppingCartItem.StoreId,
                    attributesXml, customerEnteredPrice,
                    rentalStartDate, rentalEndDate, quantity, false, shoppingCartItemId));
                if (warnings.Any())
                    return warnings;

                shoppingCartItem.Quantity = quantity;
                shoppingCartItem.AttributesXml = attributesXml;
                shoppingCartItem.CustomerEnteredPrice = customerEnteredPrice;
                shoppingCartItem.RentalStartDateUtc = rentalStartDate;
                shoppingCartItem.RentalEndDateUtc = rentalEndDate;
                shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;

                _sciRepository.Update(shoppingCartItem);
                _customerService.UpdateCustomer(customer);

                //_eventPublisher.EntityUpdated(shoppingCartItem);
            }
            else
            {
                warnings.AddRange(GetRequiredProductWarnings(customer, shoppingCartItem.ShoppingCartTypeId,
                    product, shoppingCartItem.StoreId, quantity, false, shoppingCartItemId));
                if (warnings.Any())
                    return warnings;

                DeleteShoppingCartItem(shoppingCartItem, resetCheckoutData, true);
            }
            //if (shoppingCartTypeId == (int)ShoppingCartType.ShoppingCart)
            //    UpdatePurchaseStatus(customer);
            return warnings;
        }

        public virtual IList<string> GetShoppingCartWarnings(IList<ShoppingCartItem> shoppingCart,
            string checkoutAttributesXml, bool validateCheckoutAttributes)
        {
            var warnings = new List<string>();

            var hasStandartProducts = false;
            var hasRecurringProducts = false;

            foreach (var sci in shoppingCart)
            {
                var product = _productService.GetProductById(sci.ProductId);
                if (product == null)
                {
                    warnings.Add(string.Format("Product (Id={0}) cannot be loaded", sci.ProductId));
                    return warnings;
                }

                if (product.IsRecurring)
                    hasRecurringProducts = true;
                else
                    hasStandartProducts = true;
            }

            if (hasStandartProducts && hasRecurringProducts)
                warnings.Add("Your cart has standard and auto-ship (recurring) items. Only one product type is allowed per order.");

            if (hasRecurringProducts)
            {
                var cyclesError = GetRecurringCycleInfo(shoppingCart, out var _, out var _, out var _);
                if (!string.IsNullOrEmpty(cyclesError))
                {
                    warnings.Add(cyclesError);
                    return warnings;
                }
            }

            if (!validateCheckoutAttributes)
                return warnings;

            var attributes1 = _checkoutAttributeParserService.ParseCheckoutAttributes(checkoutAttributesXml);

            var excludeShippableAttributes = !ShoppingCartRequiresShipping(shoppingCart);
            var attributes2 = _checkoutAttributeService.GetAllCheckoutAttributes(InovatiqaDefaults.StoreId, excludeShippableAttributes);

            attributes2 = attributes2.Where(x =>
            {
                var conditionMet = _checkoutAttributeParserService.IsConditionMet(x, checkoutAttributesXml);
                return !conditionMet.HasValue || conditionMet.Value;
            }).ToList();

            foreach (var a2 in attributes2)
            {
                if (!a2.IsRequired)
                    continue;

                var found = false;
                foreach (var a1 in attributes1)
                {
                    if (a1.Id != a2.Id)
                        continue;

                    var attributeValuesStr = _checkoutAttributeParserService.ParseValues(checkoutAttributesXml, a1.Id);
                    foreach (var str1 in attributeValuesStr)
                        if (!string.IsNullOrEmpty(str1.Trim()))
                        {
                            found = true;
                            break;
                        }
                }

                if (found)
                    continue;

                warnings.Add(!string.IsNullOrEmpty(a2.TextPrompt)
                    ? a2.TextPrompt
                    : string.Format("Please select {0}",
                        a2.Name));
            }

            foreach (var ca in attributes2)
            {
                string enteredText;
                int enteredTextLength;

                if (ca.ValidationMinLength.HasValue)
                {
                    if (ca.AttributeControlTypeId == (int)AttributeControlType.TextBox ||
                        ca.AttributeControlTypeId == (int)AttributeControlType.MultilineTextbox)
                    {
                        enteredText = _checkoutAttributeParserService.ParseValues(checkoutAttributesXml, ca.Id).FirstOrDefault();
                        enteredTextLength = string.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                        if (ca.ValidationMinLength.Value > enteredTextLength)
                        {
                            warnings.Add(string.Format("{0} : minimum length is {1} chars", ca.Name, ca.ValidationMinLength.Value));
                        }
                    }
                }

                if (!ca.ValidationMaxLength.HasValue)
                    continue;

                if (ca.AttributeControlTypeId != (int)AttributeControlType.TextBox && ca.AttributeControlTypeId != (int)AttributeControlType.MultilineTextbox)
                    continue;

                enteredText = _checkoutAttributeParserService.ParseValues(checkoutAttributesXml, ca.Id).FirstOrDefault();
                enteredTextLength = string.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                if (ca.ValidationMaxLength.Value < enteredTextLength)
                {
                    warnings.Add(string.Format("{0} : maximum length is {1} chars", ca.Name, ca.ValidationMaxLength.Value));
                }
            }

            return warnings;
        }

        public virtual string GetRecurringCycleInfo(IList<ShoppingCartItem> shoppingCart,
            out int cycleLength, out int cyclePeriodId, out int totalCycles)
        {
            cycleLength = 0;
            cyclePeriodId = 0;
            totalCycles = 0;

            int? _cycleLength = null;
            int? _cyclePeriodId = null;
            int? _totalCycles = null;

            foreach (var sci in shoppingCart)
            {
                var product = _productService.GetProductById(sci.ProductId);
                if (product == null)
                {
                    throw new InovatiqaException($"Product (Id={sci.ProductId}) cannot be loaded");
                }

                if (!product.IsRecurring)
                    continue;

                var conflictError = "Your cart has auto-ship (recurring) items with conflicting shipment schedules. Only one auto-ship schedule is allowed per order.";

                if (_cycleLength.HasValue && _cycleLength.Value != product.RecurringCycleLength)
                    return conflictError;
                _cycleLength = product.RecurringCycleLength;

                if (_cyclePeriodId.HasValue && _cyclePeriodId.Value != product.RecurringCyclePeriodId)
                    return conflictError;
                _cyclePeriodId = product.RecurringCyclePeriodId;

                if (_totalCycles.HasValue && _totalCycles.Value != product.RecurringTotalCycles)
                    return conflictError;
                _totalCycles = product.RecurringTotalCycles;
            }

            if (!_cycleLength.HasValue)
                return string.Empty;

            cycleLength = _cycleLength.Value;
            cyclePeriodId = _cyclePeriodId.Value;
            totalCycles = _totalCycles.Value;

            return string.Empty;
        }

        public virtual IList<WishList> PopulateWishList(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));
            if(customer.ParentId != null && customer.ParentId != 0)
            {
                //customer is a child
                // get parent
                customer = _customerService.GetCustomerById(Convert.ToInt32(customer.ParentId));
            }
            var AllCustomers = _customerService.getAllChildAccounts(customer);

            var items = new List<WishList>();
            foreach(var Customer in AllCustomers)
            {
                foreach(var item in _wishListRepository.Query().Where(wl => wl.CustomerId == Customer).ToList())
                {
                    items.Add(item);
                }
            }
            //var items = _wishListRepository.Query().Where(wl => wl.CustomerId == customer.Id);

            return items.ToList();
        }
        public virtual WishList GetWishList(int? wishListId)
        {
            var items = _wishListRepository.Query().Where(wl => wl.Id == wishListId);

            return items.FirstOrDefault();
        }

        public virtual WishList SaveWishListName(Customer customer, string wishListName, bool isPublic)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var wishList = new WishList();

            wishList.CustomerId = customer.Id;
            wishList.ListName = wishListName;
            wishList.IsSharedList = isPublic;

            _wishListRepository.Insert(wishList);

            return wishList;
        }

        public virtual WishList RemoveWishList(Customer customer, int wishListId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var itemList = _shoppingCartItemRepository.Query().Where(x => x.WishListId == wishListId && x.CustomerId == customer.Id).ToList();

            if (itemList != null)
            {
                if (itemList.Count() > 0)
                {
                    foreach (var item in itemList)
                    {
                        _shoppingCartItemRepository.Delete(item);
                    }
                }
            }

            var wishList = new WishList();

            wishList.CustomerId = customer.Id;
            wishList.Id = wishListId;

            _wishListRepository.Delete(wishList);

            return wishList;
        }

        public virtual bool IsWishListShared(Customer customer, int? wishListId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var item = _wishListRepository.Query().Where(x => x.Id == wishListId).FirstOrDefault();

            if (item != null)
            {
                if (item.IsSharedList)
                    return true;
                else
                    return false;
            }
            return false;
        }

        public virtual SuspendedCart SaveSuspendedShoppingCart(Customer customer, int[] suspendedItemIds, string suspendedCartName,string suspendedCartEmail, string suspendedCartComment)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            Address shippingAddress = null;
            string shipToCompanyName = string.Empty;
            string shipToFirstName = string.Empty;
            string shipToLastName = string.Empty;

            if (customer.ShippingAddressId != null)
            {
                shippingAddress = _addressService.GetAddressById(int.Parse(customer.ShippingAddressId.ToString()));
                if (shippingAddress != null)
                {
                    if (!string.IsNullOrEmpty(shippingAddress.Company))
                        shipToCompanyName = shippingAddress.Company;
                    if (!string.IsNullOrEmpty(shippingAddress.FirstName))
                        shipToFirstName = shippingAddress.FirstName;
                    if (!string.IsNullOrEmpty(shippingAddress.LastName))
                        shipToLastName = shippingAddress.LastName;
                }
            }

            var totalLines = suspendedItemIds != null ? suspendedItemIds.Count() : 0;

            var suspendedCart = new SuspendedCart
            {
                CustomerId = customer.Id,
                LastModifiedDateUtc = DateTime.Now,
                Lines = totalLines,
                ShipToCompanyName = shipToCompanyName,
                ShipToFirstName = shipToFirstName,
                ShipToLastName = shipToLastName,
                Poname = suspendedCartName,
                EmailAddress = suspendedCartEmail,
                Comment = suspendedCartComment
            };

            _suspendedCartRepository.Insert(suspendedCart);

            List<ShoppingCartItem> shoppingCartItems = new List<ShoppingCartItem>();

            foreach (var sii in suspendedItemIds)
            {
                var sci = _shoppingCartItemRepository.GetById(sii);
                if (sci != null)
                    shoppingCartItems.Add(sci);
            }

            if (shoppingCartItems != null)
            {
                if (shoppingCartItems.Count > 0)
                {
                    foreach (var item in shoppingCartItems)
                    {
                        if (item.ParentSuspendedItemId == null)
                        {
                            item.SuspendedCartId = suspendedCart.Id;
                            item.ShoppingCartTypeId = (int)ShoppingCartType.SuspendedCart;
                            _shoppingCartItemRepository.Update(item);
                        }
                        else
                        {
                            var newItem = new ShoppingCartItem
                            {
                                AttributesXml = item.AttributesXml,
                                CreatedOnUtc = item.CreatedOnUtc,
                                Customer = item.Customer,
                                CustomerEnteredPrice = item.CustomerEnteredPrice,
                                CustomerId = item.CustomerId,
                                ParentSuspendedItemId = null,
                                Product = item.Product,
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                RentalEndDateUtc = item.RentalEndDateUtc,
                                RentalStartDateUtc = item.RentalStartDateUtc,
                                Reordered = item.Reordered,
                                ShoppingCartTypeId = (int)ShoppingCartType.SuspendedCart,
                                StoreId = item.StoreId,
                                SuspendedCartId = suspendedCart.Id,
                                UpdatedOnUtc = DateTime.Now,
                                WishListId = item.WishListId
                            };
                            _shoppingCartItemRepository.Insert(newItem);
                        }
                    }
                }
            }
            var items = GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart);

            for (var i = 0; i < items.Count; i++)
            {
                var sci = items[i];
                DeleteShoppingCartItem(sci);
            }

            return suspendedCart;
        }

        public virtual SuspendedCart DeleteSuspendedShoppingCart(Customer customer, int suspendedCartId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));
            customer = _customerService.GetCustomerById(_suspendedCartRepository.Query().Where(item => item.Id == suspendedCartId).ToList().FirstOrDefault().CustomerId);
            var shoppingCartItems = GetSuspendedShoppingCart(customer, suspendedCartId);
            var suspendedCart = _suspendedCartRepository.GetById(suspendedCartId);
            if (suspendedCart != null)
            {
                if (shoppingCartItems != null)
                {
                    if (shoppingCartItems.Count > 0)
                    {
                        _shoppingCartItemRepository.BulkDelete(shoppingCartItems.ToList());
                        _suspendedCartRepository.Delete(suspendedCart);
                    }
                }
            }
            return suspendedCart;
        }

        public virtual IList<ShoppingCartItem> GetSuspendedShoppingCart(Customer customer, int suspendedCartId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //var items = _sciRepository.Query().Where(sci => sci.CustomerId == customer.Id && sci.ShoppingCartTypeId == (int)ShoppingCartType.SuspendedCart && sci.SuspendedCartId == suspendedCartId);
            var items = _sciRepository.Query().Where(sci => sci.ShoppingCartTypeId == (int)ShoppingCartType.SuspendedCart && sci.SuspendedCartId == suspendedCartId);

            return items.ToList();
        }

        public virtual void CopySuspendedItemsToShoppingCartItems(Customer customer, int suspendedCartId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //var shoppingCartItems = GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart);

            //for (var i = 0; i < shoppingCartItems.Count; i++)
            //{
            //    var sci = shoppingCartItems[i];
            //    DeleteShoppingCartItem(sci);
            //}

            var items = GetSuspendedShoppingCart(customer, suspendedCartId);

            foreach (var item in items)
            {
                var newItem = new ShoppingCartItem
                {
                    AttributesXml = item.AttributesXml,
                    CreatedOnUtc = item.CreatedOnUtc,
                    Customer = item.Customer,
                    CustomerEnteredPrice = item.CustomerEnteredPrice,
                    CustomerId = customer.Id,
                    ParentSuspendedItemId = item.Id,
                    Product = item.Product,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    RentalEndDateUtc = item.RentalEndDateUtc,
                    RentalStartDateUtc = item.RentalStartDateUtc,
                    Reordered = item.Reordered,
                    ShoppingCartTypeId = (int)ShoppingCartType.ShoppingCart,
                    StoreId = item.StoreId,
                    SuspendedCartId = null,
                    UpdatedOnUtc = DateTime.Now,
                    WishListId = item.WishListId
                };
                _shoppingCartItemRepository.Insert(newItem);
            }
        }

        public virtual void DeleteSuspendedShoppingCartItemById(int id)
        {
            var sci = _shoppingCartItemRepository.GetById(id);

            if (sci != null)
                _shoppingCartItemRepository.Delete(sci);
        }

        public virtual void UpdateSuspendedShoppingCart(SuspendedCart cart)
        {
            _suspendedCartRepository.Update(cart);
        }

        public virtual SuspendedCart GetSuspendedShoppingCartById(Customer customer, int id)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var item = _suspendedCartRepository.Query().Where(scr => scr.CustomerId == customer.Id && scr.Id == id).FirstOrDefault();

            return item;
        }

        public virtual int SaveOrderApprovalRequest(IList<ShoppingCartItem> cart, Customer customer)
        {
            var approvalCart = new SuspendedCart
            {
                CustomerId = customer.Id,
                LastModifiedDateUtc = DateTime.Now,
                SuspendedCartTypeId = (int)ShoppingCartType.OrderApprovalCart
            };
            _suspendedCartRepository.Insert(approvalCart);
            foreach(var item in cart)
            {
                var newItem = new ShoppingCartItem
                {
                    AttributesXml = item.AttributesXml,
                    CreatedOnUtc = item.CreatedOnUtc,
                    Customer = item.Customer,
                    CustomerEnteredPrice = item.CustomerEnteredPrice,
                    CustomerId = item.CustomerId,
                    ParentSuspendedItemId = null,
                    Product = item.Product,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    RentalEndDateUtc = item.RentalEndDateUtc,
                    RentalStartDateUtc = item.RentalStartDateUtc,
                    Reordered = item.Reordered,
                    ShoppingCartTypeId = (int)ShoppingCartType.SuspendedCart,
                    StoreId = item.StoreId,
                    SuspendedCartId = approvalCart.Id,
                    UpdatedOnUtc = DateTime.Now,
                    WishListId = item.WishListId
                };
                _shoppingCartItemRepository.Insert(newItem);
            }
            // remove items from shopping cart
            var items = GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart);

            for (var i = 0; i < items.Count; i++)
            {
                var sci = items[i];
                DeleteShoppingCartItem(sci);
            }
            var parent = _customerService.GetCustomerById(Convert.ToInt32(customer.ParentId));
            var address = _customerService.GetAddressesByCustomerId(parent.Id).FirstOrDefault();
            //var defaultMail = _emailAccountService.GetEmailAccountById(customer.Id);
            var defaultMail = _emailAccountService.GetEmailAccountById(InovatiqaDefaults.DefaultEmailAccountId);
            var email = new QueuedEmail
            {
                From = defaultMail.Email,
                FromName = defaultMail.DisplayName,
                To = parent.Email,
                ToName = address.FirstName + address.LastName,
                Subject = "New Order Waiting For Approval",
                EmailAccountId = InovatiqaDefaults.DefaultEmailAccountId,
                PriorityId = InovatiqaDefaults.QueuedEmailPrioritHigh,
                Body = "Hello, Your Child Account "+customer.Username+" have applied an Order for Approval. Please Have a look at that an make appropriate decession",
                CreatedOnUtc = DateTime.Now,
                SentTries = 0,
                AttachedDownloadId = 0
            };
            _queuedEmailService.InsertQueuedEmail(email);

            return approvalCart.Id;
        }
        public virtual IList<ShoppingCartItem> GetAllOrderApprovalItems(int ID)
        {
            var model = new List<ShoppingCartItem>();
            var items = _shoppingCartItemRepository.Query().Where(sci => sci.SuspendedCartId != null && sci.SuspendedCartId == ID);
            foreach(var item in items)
            {
                model.Add(item);
            }
            return model;
        }
        public virtual SuspendedCart GetOrderWaitingForApproval(int ID)
        {
            return _suspendedCartRepository.Query().Where(c => c.Id == ID).FirstOrDefault();
        }
        public virtual bool DeleteOrderFromWaitingList(int ID)
        {
           var cart = _suspendedCartRepository.GetById(ID);
            _suspendedCartRepository.Delete(cart);
            return true;
        }
        public virtual bool ResetChildShoppingCart(IList<ShoppingCartItem> cart)
        {
            try
            {
                foreach (var item in cart)
                {
                    _sciRepository.Delete(item);
                }
            }
            catch(Exception ex)
            {
                return false;
            }
            return true;
        }
        public virtual void MarkOrderApproved(int OrderId)
        {
            var cart = _suspendedCartRepository.Query().Where(sc => sc.Id == OrderId).FirstOrDefault();
            cart.SuspendedCartTypeId = (int)ShoppingCartType.ApprovedOrder;
            _suspendedCartRepository.Update(cart);
        }
        public virtual void UpdatePurchaseStatus(Customer customer)
        {
            if (customer.ParentId != null && customer.ParentId > 0 && _customerService.IsInCustomerRole(customer, "Subaccount_RABCO"))
            {
                customer.CanPurchaseCart = false;
            }
            _customerService.UpdateCustomer(customer);
        }

        public virtual void MoveItemsToCurrentUser(List<ShoppingCartItem> items, Customer customer)
        {
            foreach(var sci in items)
            {
                var item = _shoppingCartItemRepository.GetById(sci.Id);
                item.CustomerId = customer.Id;
                _shoppingCartItemRepository.Update(item);
            }
        }
        #endregion
    }
}