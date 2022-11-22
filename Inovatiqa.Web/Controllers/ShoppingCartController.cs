using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dapper;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Media.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Media;
using Inovatiqa.Web.Models.ShoppingCart;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Primitives;

namespace Inovatiqa.Web.Controllers
{
    [AutoValidateAntiforgeryToken]
    public partial class ShoppingCartController : BasePublicController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParserService _productAttributeParserService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContextService _workContextService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPictureService _pictureService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;
        private readonly INotificationService _notificationService;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICheckoutAttributeParserService _checkoutAttributeParserService;
        private readonly IShippingService _shippingService;
        private readonly ICommonModelFactory _commonModelFactory;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IOrderService _orderService;
        private readonly IAddressService _addressService;
        private readonly IStateProvinceService _stateProvinceService;

        #endregion

        #region Ctor

        public ShoppingCartController(IProductService productService,
            IUrlRecordService urlRecordService,
            IProductAttributeService productAttributeService,
            IProductAttributeParserService productAttributeParserService,
            IShoppingCartService shoppingCartService,
            IWorkContextService workContextService,
            ICustomerActivityService customerActivityService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            ICheckoutAttributeService checkoutAttributeService,
            IPriceFormatter priceFormatter,
            IPictureService pictureService,
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService,
            INotificationService notificationService,
            ICheckoutAttributeParserService checkoutAttributeParserService,
            IShippingService shippingService,
            ICommonModelFactory commonModelFactory,
            IPriceCalculationService priceCalculationService,
            IRazorViewEngine viewEngine,
            IOrderService orderService, 
            IProductModelFactory productModelFactory,
            IAddressService addressService,
            IStateProvinceService stateProvinceService) : base(viewEngine)
            
        {
            _productService = productService;
            _urlRecordService = urlRecordService;
            _productAttributeService = productAttributeService;
            _productAttributeParserService = productAttributeParserService;
            _shoppingCartService = shoppingCartService;
            _workContextService = workContextService;
            _customerActivityService = customerActivityService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _priceFormatter = priceFormatter;
            _pictureService = pictureService;
            _genericAttributeService = genericAttributeService;
            _customerService = customerService;
            _notificationService = notificationService;
            _checkoutAttributeService = checkoutAttributeService;
            _checkoutAttributeParserService = checkoutAttributeParserService;
            _shippingService = shippingService;
            _commonModelFactory = commonModelFactory;
            _priceCalculationService = priceCalculationService;
            _orderService = orderService;
            _productModelFactory = productModelFactory;
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Utilities

        protected virtual void SaveItem(ShoppingCartItem updatecartitem, List<string> addToCartWarnings, Product product,
           int cartTypeId, string attributes, decimal customerEnteredPriceConverted, DateTime? rentalStartDate,
           DateTime? rentalEndDate, int quantity, int wishListId)
        {
            if (updatecartitem == null)
            {
                addToCartWarnings.AddRange(_shoppingCartService.AddToCart(_workContextService.CurrentCustomer,
                    product, cartTypeId, InovatiqaDefaults.StoreId,
                    attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity, true, wishListId));
            }
            else
            {
                var cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, updatecartitem.ShoppingCartTypeId, InovatiqaDefaults.StoreId);

                var otherCartItemWithSameParameters = _shoppingCartService.FindShoppingCartItemInTheCart(
                    cart, updatecartitem.ShoppingCartTypeId, product, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate);
                if (otherCartItemWithSameParameters != null &&
                    otherCartItemWithSameParameters.Id == updatecartitem.Id)
                {
                    otherCartItemWithSameParameters = null;
                }
                addToCartWarnings.AddRange(_shoppingCartService.UpdateShoppingCartItem(_workContextService.CurrentCustomer,
                    updatecartitem.Id, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity + (otherCartItemWithSameParameters?.Quantity ?? 0), true));
                if (otherCartItemWithSameParameters != null && !addToCartWarnings.Any())
                {
                    _shoppingCartService.DeleteShoppingCartItem(otherCartItemWithSameParameters);
                }
            }
        }

        protected virtual IActionResult GetProductToCartDetails(List<string> addToCartWarnings, int cartTypeId,
            Product product, bool displayInFlyout = true, string attributes = "")
        {
            if (addToCartWarnings.Any())
            {
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            var currentCustomer = _workContextService.CurrentCustomer;

            switch (cartTypeId)
            {
                case (int)ShoppingCartType.Wishlist:
                    {
                        _customerActivityService.InsertActivity("PublicStore.AddToWishlist",
                            string.Format("Added a product to wishlist ('{0}')", product.Name), product.Id, product.GetType().Name);

                        if (InovatiqaDefaults.DisplayWishlistAfterAddingProduct)
                        {
                            return Json(new
                            {
                                redirect = Url.RouteUrl("Wishlist")
                            });
                        }

                        var shoppingCarts = _shoppingCartService.GetShoppingCart(currentCustomer, (int)ShoppingCartType.Wishlist, InovatiqaDefaults.StoreId);

                        var updatetopwishlistsectionhtml = string.Format(
                            "{0}",
                            shoppingCarts.Sum(item => item.Quantity));

                        return Json(new
                        {
                            success = true,
                            message = string.Format(
                                "The product has been added to your <a href=\"{0}\">wishlist</a>",
                                Url.RouteUrl("Wishlist")),
                            updatetopwishlistsectionhtml
                        });
                    }

                case (int)ShoppingCartType.ShoppingCart:
                default:
                    {
                        _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart",
                            string.Format("Added a product to shopping cart ('{0}')", product.Name), product.Id, product.GetType().Name);

                        if (InovatiqaDefaults.DisplayCartAfterAddingProduct)
                        {
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart")
                            });
                        }

                        var shoppingCarts = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);
                        string totalprice = _priceFormatter.FormatPrice(shoppingCarts.Sum(item => _shoppingCartService.GetUnitPrice(item) * item.Quantity));

                        /*decimal price = 0; //Need to check                
                        for (int i = 0; i < shoppingCarts.Count; i++)
                        {
                            //////var UnitPrice = shoppingCarts[i].Product.Price;

                            var unitPrice = _priceCalculationService.GetFinalPrice(product, currentCustomer);

                            var Quantity = shoppingCarts[i].Quantity;
                            price += Convert.ToDecimal(unitPrice * Quantity);
                        }
                        string totalprice = _priceFormatter.FormatPrice(price);
                        */
                        var updatetopcartsectionhtml = string.Format(
                            "{0}",
                            shoppingCarts.Count());

                        var updateflyoutcartsectionhtml = InovatiqaDefaults.MiniShoppingCartEnabled
                            ? RenderViewComponentToString("FlyoutShoppingCart")
                            : string.Empty;
                        
                        return Json(new
                        {
                            flyout = GetFlyoutCartData(product, attributes, displayInFlyout),
                            success = true,
                            message = string.Format("The product has been added to your <a href=\"{0}\">shopping cart</a>",
                                 _customerService.IsRegistered(currentCustomer) ? Url.RouteUrl("ShoppingCart") : Url.RouteUrl("GuestCart")),
                            updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml,
                            totalprice
                        });
                    }
            }
        }

        protected virtual void ParseAndSaveCheckoutAttributes(IList<ShoppingCartItem> cart, IFormCollection form)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = string.Empty;
            var excludeShippableAttributes = !_shoppingCartService.ShoppingCartRequiresShipping(cart);
            var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes(InovatiqaDefaults.StoreId, excludeShippableAttributes);
            foreach (var attribute in checkoutAttributes)
            {
                var controlId = $"checkout_attribute_{attribute.Id}";
                switch (attribute.AttributeControlTypeId)
                {
                    case (int)AttributeControlType.DropdownList:
                    case (int)AttributeControlType.RadioList:
                    case (int)AttributeControlType.ColorSquares:
                    case (int)AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _checkoutAttributeParserService.AddCheckoutAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }

                        break;
                    case (int)AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _checkoutAttributeParserService.AddCheckoutAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }

                        break;
                    case (int)AttributeControlType.ReadonlyCheckboxes:
                        {
                            var attributeValues = _checkoutAttributeService.GetCheckoutAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _checkoutAttributeParserService.AddCheckoutAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                            }
                        }

                        break;
                    case (int)AttributeControlType.TextBox:
                    case (int)AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _checkoutAttributeParserService.AddCheckoutAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }

                        break;
                    case (int)AttributeControlType.Datepicker:
                        {
                            var date = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(date));
                            }
                            catch
                            {
                            }

                            if (selectedDate.HasValue)
                                attributesXml = _checkoutAttributeParserService.AddCheckoutAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                        }

                        break;
                    default:
                        break;
                }
            }

            foreach (var attribute in checkoutAttributes)
            {
                var conditionMet = _checkoutAttributeParserService.IsConditionMet(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                    attributesXml = _checkoutAttributeParserService.RemoveCheckoutAttribute(attributesXml, attribute);
            }
            var customer = _workContextService.CurrentCustomer;

            _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id, InovatiqaDefaults.GenderAttribute, attributesXml, InovatiqaDefaults.StoreId);
        }


        #endregion

        #region Shopping cart

        [HttpPost]
        public virtual IActionResult SelectShippingOption([FromQuery] string name, [FromQuery] EstimateShippingModel model, IFormCollection form)
        {
            if (model == null)
                model = new EstimateShippingModel();

            var errors = new List<string>();
            if (string.IsNullOrEmpty(model.ZipPostalCode))
                errors.Add("Zip / postal code is required");

            if (model.CountryId == null || model.CountryId == 0)
                errors.Add("Country is required");

            if (errors.Count > 0)
                return Json(new
                {
                    success = false,
                    errors = errors
                });

            var currentCustomer = _workContextService.CurrentCustomer;

            var cart = _shoppingCartService.GetShoppingCart(currentCustomer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            ParseAndSaveCheckoutAttributes(cart, form);

            var shippingOptions = new List<ShippingOption>();
            ShippingOption selectedShippingOption = null;

            if (!string.IsNullOrWhiteSpace(name))
            {
                shippingOptions = _genericAttributeService.GetAttribute<List<ShippingOption>>(currentCustomer,
                    InovatiqaDefaults.OfferedShippingOptionsAttribute, InovatiqaDefaults.StoreId);

                if (shippingOptions == null || !shippingOptions.Any())
                {
                    var address = new Address
                    {
                        CountryId = model.CountryId,
                        StateProvinceId = model.StateProvinceId,
                        ZipPostalCode = model.ZipPostalCode,
                    };

                    var getShippingOptionResponse = _shippingService.GetShippingOptions(cart, address,
                        currentCustomer, storeId: InovatiqaDefaults.StoreId);

                    if (getShippingOptionResponse.Success)
                        shippingOptions = getShippingOptionResponse.ShippingOptions.ToList();
                    else
                        foreach (var error in getShippingOptionResponse.Errors)
                            errors.Add(error);
                }
            }

            selectedShippingOption = shippingOptions.Find(so => !string.IsNullOrEmpty(so.Name) && so.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (selectedShippingOption == null)
                errors.Add("Selected shipping option is not found");

            if (errors.Count > 0)
                return Json(new
                {
                    success = false,
                    errors = errors
                });

            _genericAttributeService.SaveAttribute<PickupPoint>(currentCustomer.GetType().Name,
                currentCustomer.Id, InovatiqaDefaults.SelectedPickupPointAttribute, null, InovatiqaDefaults.StoreId);

            _genericAttributeService.SaveAttribute(currentCustomer.GetType().Name, currentCustomer.Id,
                InovatiqaDefaults.SelectedShippingOptionAttribute, selectedShippingOption, InovatiqaDefaults.StoreId);

            var ordertotalssectionhtml = RenderViewComponentToString("OrderTotals", new { isEditable = true });

            return Json(new
            {
                success = true,
                ordertotalssectionhtml
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult ProductDetails_AttributeChange(int productId, bool validateAttributeConditions,
            bool loadPicture, IFormCollection form, int selectedAttributeVaueId = 0, int mSKU = 0)
        {
            if (mSKU != 0)
            {
                var parentProduct = _productService.GetProductByMSku(mSKU);

                if (parentProduct != null)
                {
                    var parentProductURL = _urlRecordService.GetActiveSlug(parentProduct.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId);
                    return Json(new
                    {
                        parentProductURL
                    });
                }
            }

            var product = _productService.GetProductById(productId);
            if (product == null)
                return new JsonResult(null);

            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;

            var errors = new List<string>();
            var attributeXml = _productAttributeParserService.ParseProductAttributes(product, form, errors);

            var sku = _productService.FormatSku(product, attributeXml);

            var attributeValues = _productAttributeParserService.ParseProductAttributeValues(attributeXml);
            var totalWeight = product.BasepriceAmount;

            foreach (var attributeValue in attributeValues)
            {
                switch (attributeValue.AttributeValueTypeId)
                {
                    case InovatiqaDefaults.Simple:
                        totalWeight += attributeValue.WeightAdjustment;
                        break;
                    case InovatiqaDefaults.AssociatedToProduct:
                        var associatedProduct = _productService.GetProductById(attributeValue.AssociatedProductId);
                        if (associatedProduct != null)
                            totalWeight += associatedProduct.BasepriceAmount * attributeValue.Quantity;
                        break;
                }
            }

            var price = string.Empty;
            var basepricepangv = string.Empty;
            if (InovatiqaDefaults.DisplayPrices)
            {
                var finalPrice = _shoppingCartService.GetUnitPrice(product,
                    _workContextService.CurrentCustomer,
                    (int)ShoppingCartType.ShoppingCart,
                    1, attributeXml, 0,
                    rentalStartDate, rentalEndDate,
                    true, out var _, out _);
                var finalPriceWithDiscountBase = finalPrice;
                var finalPriceWithDiscount = finalPriceWithDiscountBase;
                price = _priceFormatter.FormatPrice(finalPriceWithDiscount);
                //basepricepangv = _priceFormatter.FormatBasePrice(product, finalPriceWithDiscountBase, totalWeight);
            }

            var stockAvailability = _productService.FormatStockMessage(product, attributeXml);

            var enabledAttributeMappingIds = new List<int>();
            var disabledAttributeMappingIds = new List<int>();
            if (validateAttributeConditions)
            {
                var attributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
                foreach (var attribute in attributes)
                {
                    var conditionMet = _productAttributeParserService.IsConditionMet(attribute, attributeXml);
                    if (conditionMet.HasValue)
                    {
                        if (conditionMet.Value)
                            enabledAttributeMappingIds.Add(attribute.Id);
                        else
                            disabledAttributeMappingIds.Add(attribute.Id);
                    }
                }
            }

            var pictureFullSizeUrl = string.Empty;
            var pictureDefaultSizeUrl = string.Empty;
            if (loadPicture)
            {
                var pictureId = _productAttributeParserService.FindProductAttributeCombination(product, attributeXml)?.PictureId ?? 0;

                if (pictureId == 0)
                {
                    pictureId = _productAttributeParserService.ParseProductAttributeValues(attributeXml)
                        .FirstOrDefault(attributeValue => attributeValue.PictureId > 0)?.PictureId ?? 0;
                }

                if (pictureId > 0)
                {
                    var picture = _pictureService.GetPictureById(pictureId);
                    var pictureModel = new PictureModel
                    {
                        FullSizeImageUrl = _pictureService.GetPictureUrl(ref picture),
                        ImageUrl = _pictureService.GetPictureUrl(ref picture, InovatiqaDefaults.ProductDetailsPictureSize)
                    };

                    if (pictureModel != null)
                    {
                        pictureFullSizeUrl = pictureModel.FullSizeImageUrl;
                        pictureDefaultSizeUrl = pictureModel.ImageUrl;
                    }
                }
            }

            var isFreeShipping = product.IsFreeShipping;
            if (isFreeShipping && !string.IsNullOrEmpty(attributeXml))
            {
                isFreeShipping = _productAttributeParserService.ParseProductAttributeValues(attributeXml)
                    .Where(attributeValue => attributeValue.AttributeValueTypeId == InovatiqaDefaults.AssociatedToProduct)
                    .Select(attributeValue => _productService.GetProductById(attributeValue.AssociatedProductId))
                    .All(associatedProduct => associatedProduct == null || !associatedProduct.IsShipEnabled || associatedProduct.IsFreeShipping);
            }
            string gtin = "";
            string mpn = "";
            return Json(new
            {
                productId,
                gtin,
                mpn,
                sku,
                price,
                basepricepangv,
                stockAvailability,
                enabledattributemappingids = enabledAttributeMappingIds.ToArray(),
                disabledattributemappingids = disabledAttributeMappingIds.ToArray(),
                pictureFullSizeUrl,
                pictureDefaultSizeUrl,
                isFreeShipping,
                message = errors.Any() ? errors.ToArray() : null
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult AddProductToCart_Catalog(int productId, int shoppingCartTypeId,
            int quantity, bool forceredirection = false, int wishListId = 0)
        {
            var cartTypeId = shoppingCartTypeId;
            var customer = _workContextService.CurrentCustomer;
            var product = _productService.GetProductById(productId);
            if (product == null)
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            if (product.ProductTypeId != InovatiqaDefaults.SimpleProduct)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId) })
                });
            }

            if (product.OrderMinimumQuantity > quantity)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId) })
                });
            }

            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            if (allowedQuantities.Length > 0)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId) })
                });
            }

            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            if (productAttributes.Any(pam => pam.AttributeControlTypeId != (int)AttributeControlType.ReadonlyCheckboxes))
            {
                var RedirectUrl = Url.RouteUrl("Product", new { SeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId) });
                return Json(new
                {

                    redirect = RedirectUrl
                });
            }

            var attXml = productAttributes.Aggregate(string.Empty, (attributesXml, attribute) =>
            {
                var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                foreach (var selectedAttributeId in attributeValues
                    .Where(v => v.IsPreSelected)
                    .Select(v => v.Id)
                    .ToList())
                {
                    attributesXml = _productAttributeParserService.AddProductAttribute(attributesXml,
                        attribute, selectedAttributeId.ToString());
                }

                return attributesXml;
            });

            var cart = _shoppingCartService.GetShoppingCart(customer, cartTypeId, InovatiqaDefaults.StoreId);
            var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, cartTypeId, product);
            var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + quantity : quantity;
            var addToCartWarnings = _shoppingCartService
                .GetShoppingCartItemWarnings(customer, cartTypeId,
                product, InovatiqaDefaults.StoreId, string.Empty,
                decimal.Zero, null, null, quantityToValidate, false, shoppingCartItem?.Id ?? 0, true, false, false, false);
            if (addToCartWarnings.Any())
            {
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            addToCartWarnings = _shoppingCartService.AddToCart(customer: customer,
                product: product,
                shoppingCartTypeId: cartTypeId,
                storeId: InovatiqaDefaults.StoreId,
                attributesXml: attXml,
                quantity: quantity, wishListId: wishListId);
            if (addToCartWarnings.Any())
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId) })
                });
            }

            switch (cartTypeId)
            {
                case (int)ShoppingCartType.Wishlist:
                    {
                        _customerActivityService.InsertActivity("PublicStore.AddToWishlist",
                            string.Format("Added a product to wishlist ('{0}')", product.Name), product.Id, product.GetType().Name);

                        if (InovatiqaDefaults.DisplayWishlistAfterAddingProduct || forceredirection)
                        {
                            return Json(new
                            {
                                redirect = Url.RouteUrl("Wishlist")
                            });
                        }

                        var shoppingCarts = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.Wishlist, InovatiqaDefaults.StoreId);

                        var updatetopwishlistsectionhtml = string.Format("{0}",
                            shoppingCarts.Sum(item => item.Quantity));
                        return Json(new
                        {
                            success = true,
                            message = string.Format("The product has been added to your <a href='{0}'>wishlist</a>", Url.RouteUrl("Wishlist")),
                            updatetopwishlistsectionhtml
                        });
                    }

                case (int)ShoppingCartType.ShoppingCart:
                default:
                    {
                        _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart",
                            string.Format("Added a product to shopping cart ('{0}')", product.Name), product.Id, product.GetType().Name);

                        if (InovatiqaDefaults.DisplayCartAfterAddingProduct || forceredirection)
                        {
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart")
                            });
                        }

                        var shoppingCarts = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

                        //var updatetopcartsectionhtml = string.Format("({0})",
                        //    shoppingCarts.Sum(item => item.Quantity));

                        //var updateflyoutcartsectionhtml = InovatiqaDefaults.MiniShoppingCartEnabled
                        //    ? RenderViewComponentToString("FlyoutShoppingCart")
                        //    : string.Empty;

                        //return Json(new
                        //{
                        //    success = true,
                        //    message = string.Format("The product has been added to your <a href='{0}'>shopping cart</a>", Url.RouteUrl("ShoppingCart")),
                        //    updatetopcartsectionhtml,
                        //    updateflyoutcartsectionhtml
                        //});
                        var updatetopcartsectionhtml = string.Format(
                            "{0}",
                            shoppingCarts.Sum(item => item.Quantity));

                        var updateHeadersectionhtml = InovatiqaDefaults.MiniShoppingCartEnabled
                                                    ? RenderViewComponentToString("HeaderLinks")
                                                    : string.Empty;

                        return Json(new
                        {
                            success = true,
                            message = string.Format("The product has been added to your <a href=\"{0}\">shopping cart</a>",
                                _customerService.IsRegistered(customer) ? Url.RouteUrl("ShoppingCart") : Url.RouteUrl("GuestCart")),
                            updatetopcartsectionhtml,
                            updateHeadersectionhtml
                            // updateflyoutcartsectionhtml
                        });

                    }
            }
        }

        [RequireHttps]
        public virtual IActionResult Cart()
        {
            var customer = _workContextService.CurrentCustomer;
            // customer is not registered, then move to guest checkout otherwise proceed as normal
            if (!_customerService.IsRegistered(customer))
                return RedirectToAction("GuestCart");

            // Implementation of Role - Can Modify Account Data
            if (!InovatiqaDefaults.EnableShoppingCart)
                return RedirectToRoute("Homepage");
            /*if (customer.ParentId != null && customer.ParentId != 0)
            {
                if (!_customerService.IsInCustomerRole(customer, "Subaccount_GTCC"))
                {
                    return RedirectToAction("NotAllowed", "Common");
                }
            }*/
            // Implementatiuon of Role Done
            var cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);
            var model = new ShoppingCartModel();
            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);
            ViewBag.Id = _workContextService.CurrentCustomer.Id;
            ViewBag.ShippingAddress = _customerService.GetCustomerShippingAddress(_workContextService.CurrentCustomer);
            ViewBag.BillingAddress = _customerService.GetCustomerBillingAddress(_workContextService.CurrentCustomer);
            //comment by hamza for subaccount check

            ViewBag.state = _stateProvinceService.GetStateProvinceById(ViewBag.ShippingAddress?.StateProvinceId ?? 0)?.Abbreviation;
            ViewBag.LoggedUser = _commonModelFactory.PrepareAdminHeaderLinksModel().ImpersonatedCustomerName;
            return View(model);
        }

        [RequireHttps]
        public virtual IActionResult SuspendedCarts()
        {
            if (!_customerService.IsRegistered(_workContextService.CurrentCustomer))
            {
                return Challenge();
            }
            var customer = _workContextService.CurrentCustomer;
            var carts = _shoppingCartService.GetCustomerSuspendedCarts(customer);
            var model = new List<CustomerSuspendedCartModel>();
            model = _shoppingCartModelFactory.PrepareCustomerSuspendedCartModel(model, carts);
            return View(model);
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired("updatecart")]
        public virtual IActionResult UpdateCart(IFormCollection form)
        {
            if (!InovatiqaDefaults.EnableShoppingCart)
                return RedirectToRoute("Homepage");

            var cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            var itemIdsToRemove = new List<int>();
            itemIdsToRemove = form["removefromcart"]
                .SelectMany(value => value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(idString => int.TryParse(idString, out var id) ? id : 0)
                .Distinct().ToList();
            if (itemIdsToRemove.Count() > 1)
            {
                itemIdsToRemove = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart).Select(sc => sc.Id).ToList();
            }

            var products = _productService.GetProductsByIds(cart.Select(item => item.ProductId).Distinct().ToArray())
                .ToDictionary(item => item.Id, item => item);

            var itemsWithNewQuantity = cart.Select(item => new
            {
                NewQuantity = itemIdsToRemove.Contains(item.Id) ? 0 : int.TryParse(form[$"itemquantity{item.Id}"], out var quantity) ? quantity : item.Quantity,
                Item = item,
                Product = products.ContainsKey(item.ProductId) ? products[item.ProductId] : null
            }).Where(item => item.NewQuantity != item.Item.Quantity);

            var orderedCart = itemsWithNewQuantity
                .OrderByDescending(cartItem =>
                    (cartItem.NewQuantity < cartItem.Item.Quantity &&
                     (cartItem.Product?.RequireOtherProducts ?? false)) ||
                    (cartItem.NewQuantity > cartItem.Item.Quantity && cartItem.Product != null && _shoppingCartService
                         .GetProductsRequiringProduct(cart, cartItem.Product).Any()))
                .ToList();

            var warnings = orderedCart.Select(cartItem => new
            {
                ItemId = cartItem.Item.Id,
                Warnings = _shoppingCartService.UpdateShoppingCartItem(_workContextService.CurrentCustomer,
                    cartItem.Item.Id, cartItem.Item.AttributesXml, cartItem.Item.CustomerEnteredPrice,
                    cartItem.Item.RentalStartDateUtc, cartItem.Item.RentalEndDateUtc, cartItem.NewQuantity, true)
            }).ToList();

            cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            var model = new ShoppingCartModel();
            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);

            foreach (var warningItem in warnings.Where(warningItem => warningItem.Warnings.Any()))
            {
                var itemModel = model.Items.FirstOrDefault(item => item.Id == warningItem.ItemId);
                if (itemModel != null)
                    itemModel.Warnings = warningItem.Warnings.Concat(itemModel.Warnings).Distinct().ToList();
            }
            ViewBag.BillingAddress = _customerService.GetCustomerBillingAddress(_workContextService.CurrentCustomer);
            ViewBag.ShippingAddress = _customerService.GetCustomerShippingAddress(_workContextService.CurrentCustomer);
            ViewBag.LoggedUser = _commonModelFactory.PrepareAdminHeaderLinksModel().ImpersonatedCustomerName;
            return View(model);
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired("continueshopping")]
        public virtual IActionResult ContinueShopping()
        {
            var currentCustomer = _workContextService.CurrentCustomer;
            var returnUrl = _genericAttributeService.GetAttribute<string>(currentCustomer, InovatiqaDefaults.LastContinueShoppingPageAttribute, currentCustomer.Id, InovatiqaDefaults.StoreId);

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToRoute("Homepage");
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired("checkout")]
        public virtual IActionResult StartCheckout(IFormCollection form)
        {
            var cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            ParseAndSaveCheckoutAttributes(cart, form);

            var checkoutAttributes = _genericAttributeService.GetAttribute<string>(_workContextService.CurrentCustomer,
                InovatiqaDefaults.CheckoutAttributes, InovatiqaDefaults.StoreId);
            var checkoutAttributeWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributes, true);
            if (checkoutAttributeWarnings.Any())
            {
                var model = new ShoppingCartModel();
                model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart, validateCheckoutAttributes: true);
                return View(model);
            }

            //var anonymousPermissed = _orderSettings.AnonymousCheckoutAllowed
            //                         && _customerSettings.UserRegistrationType == UserRegistrationType.Disabled;

            if (InovatiqaDefaults.AnonymousCheckoutAllowed || !_customerService.IsGuest(_workContextService.CurrentCustomer))
                return RedirectToRoute("Checkout");

            //var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();
            //var downloadableProductsRequireRegistration =
            //    _customerSettings.RequireRegistrationForDownloadableProducts && _productService.HasAnyDownloadableProduct(cartProductIds);

            //if (!_orderSettings.AnonymousCheckoutAllowed || downloadableProductsRequireRegistration)
            //{
            //    //verify user identity (it may be facebook login page, or google, or local)
            //    return Challenge();
            //}

            //return RedirectToRoute("LoginCheckoutAsGuest", new { returnUrl = Url.RouteUrl("ShoppingCart") });
            return RedirectToRoute("Login", new { returnUrl = Url.RouteUrl("ShoppingCart") });
        }
        //[HttpPost]
        //[IgnoreAntiforgeryToken]
        //public virtual IActionResult UUpdateProductToCart_Details(int productId, int shoppingCartTypeId, IFormCollection form, int wishListId = 0, bool editing = false)
        //{
        //    var product = _productService.GetProductById(productId);
        //    if (product == null)
        //    {
        //        return Json(new
        //        {
        //            redirect = Url.RouteUrl("Homepage")
        //        });
        //    }

        //    if (product.ProductTypeId != InovatiqaDefaults.SimpleProduct)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = "Only simple products could be added to the cart"
        //        });
        //    }

        //    var updatecartitemid = 0;
        //    foreach (var formKey in form.Keys)
        //        if (formKey.Equals($"addtocart_{productId}.UpdatedShoppingCartItemId", StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            int.TryParse(form[formKey], out updatecartitemid);
        //            break;
        //        }

        //    ShoppingCartItem updatecartitem = null;
        //    if (InovatiqaDefaults.AllowCartItemEditing && updatecartitemid > 0)
        //    {
        //        var cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)shoppingCartTypeId, InovatiqaDefaults.StoreId);

        //        updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
        //        if (updatecartitem == null)
        //        {
        //            return Json(new
        //            {
        //                success = false,
        //                message = "No shopping cart item found to update"
        //            });
        //        }

        //        if (updatecartitem != null && product.Id != updatecartitem.ProductId)
        //        {
        //            return Json(new
        //            {
        //                success = false,
        //                message = "This product does not match a passed shopping cart item identifier"
        //            });
        //        }
        //    }

        //    var addToCartWarnings = new List<string>();
        //    var quantity = _productAttributeParserService.ParseEnteredQuantity(product, form);

        //    var attributes = _productAttributeParserService.ParseProductAttributes(product, form, addToCartWarnings);
        //    var cartType = updatecartitem == null ? (int)shoppingCartTypeId :
        //        updatecartitem.ShoppingCartTypeId;
        //    bool displayinFlyout = false;
        //    UpdateCart(form);
        //    return GetProductToCartDetails(addToCartWarnings, cartType, product, displayinFlyout, attributes);
        //}

            [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult AddProductToCart_Details(int productId, int shoppingCartTypeId, IFormCollection form, int wishListId = 0)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("Homepage")
                });
            }
            
            if (product.ProductTypeId != InovatiqaDefaults.SimpleProduct)
            {
                return Json(new
                {
                    success = false,
                    message = "Only simple products could be added to the cart"
                });
            }

            var updatecartitemid = 0;
            foreach (var formKey in form.Keys)
                if (formKey.Equals($"addtocart_{productId}.UpdatedShoppingCartItemId", StringComparison.InvariantCultureIgnoreCase))
                {
                    int.TryParse(form[formKey], out updatecartitemid);
                    break;
                }

            ShoppingCartItem updatecartitem = null;
            if (InovatiqaDefaults.AllowCartItemEditing && updatecartitemid > 0)
            {
                var cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)shoppingCartTypeId, InovatiqaDefaults.StoreId);

                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                if (updatecartitem == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No shopping cart item found to update"
                    });
                }

                if (updatecartitem != null && product.Id != updatecartitem.ProductId)
                {
                    return Json(new
                    {
                        success = false,
                        message = "This product does not match a passed shopping cart item identifier"
                    });
                }
            }

            var addToCartWarnings = new List<string>();

            var quantity = _productAttributeParserService.ParseEnteredQuantity(product, form);

            var attributes = _productAttributeParserService.ParseProductAttributes(product, form, addToCartWarnings);

            var cartType = updatecartitem == null ? (int)shoppingCartTypeId :
                updatecartitem.ShoppingCartTypeId;

            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            // if product is not in shopping cart, then display the product in flyout cart, set flag to true or false
            bool displayinFlyout = false;
            if ((int)ShoppingCartType.ShoppingCart == shoppingCartTypeId)
            {
                // comment by hamza because pproduct is not added to cart
                displayinFlyout = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart).Where(sci => sci.ProductId == productId && sci.AttributesXml == attributes  && sci.AttributesXml == (updatecartitem != null ? updatecartitem.AttributesXml : sci.AttributesXml)).FirstOrDefault() == null;
            }
            SaveItem(updatecartitem, addToCartWarnings, product, cartType, attributes, 0, rentalStartDate, rentalEndDate, quantity, wishListId);
            return GetProductToCartDetails(addToCartWarnings, cartType, product, displayinFlyout, attributes);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult AddProductToCartSKU_Details(string skuId, int shoppingCartTypeId, IFormCollection form, int wishListId = 0)
        {
            var product = _productService.GetProductBySku(skuId);
            var model = _productModelFactory.PrepareProductDetailsModel(product, null, false);
            if (product == null)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("Homepage")
                });
            }

            if (product.ProductTypeId != InovatiqaDefaults.SimpleProduct)
            {
                return Json(new
                {
                    success = false,
                    message = "Only simple products could be added to the cart"
                });
            }

            ShoppingCartItem updatecartitem = null;

            var addToCartWarnings = new List<string>();

            var quantity = 1;
            foreach (var formKey in form.Keys)
            {
                if (formKey.Equals($"addtocart_Id.EnteredQuantity", StringComparison.InvariantCultureIgnoreCase))
                {
                    int.TryParse(form[formKey], out quantity);
                    break;
                }
            }
            var attributes = _productAttributeParserService.ParseProductAttributes(product, form, addToCartWarnings);

            var cartType = updatecartitem == null ? (int)shoppingCartTypeId :
                updatecartitem.ShoppingCartTypeId;

            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
          
            SaveItem(updatecartitem, addToCartWarnings, product, cartType, attributes, 0, rentalStartDate, rentalEndDate, quantity, wishListId);
            bool displayInFlyout = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart).Where(sci => sci.ProductId == product.Id && sci.AttributesXml == attributes).FirstOrDefault() == null;

            return GetProductToCartDetails(addToCartWarnings, cartType, product, displayInFlyout, attributes);
        }


        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult UpdateProductToCart_Details(int productId, int shoppingCartTypeId, IFormCollection form, int wishListId = 0)
        {
            var product = _productService.GetProductById(productId);
            var updatecartitemid = 0;
            foreach (var formKey in form.Keys)
                if (formKey.Equals($"addtocart_Id.UpdatedShoppingCartItemId", StringComparison.InvariantCultureIgnoreCase))
                {
                    int.TryParse(form[formKey], out updatecartitemid);
                    break;
                }
            ShoppingCartItem updatecartitem = null;
            if (InovatiqaDefaults.AllowCartItemEditing && updatecartitemid > 0)
            {
                var cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)shoppingCartTypeId, InovatiqaDefaults.StoreId);
                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                if (updatecartitem == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No shopping cart item found to update"
                    });
                }

                if (updatecartitem != null && product.Id != updatecartitem.ProductId)
                {
                    return Json(new
                    {
                        success = false,
                        message = "This product does not match a passed shopping cart item identifier"
                    });
                }
            }


            var addToCartWarnings = new List<string>();

            var quantity = _productAttributeParserService.ParseEnteredQuantity(product, form);

            var attributes = _productAttributeParserService.ParseProductAttributes(product, form, addToCartWarnings);

            var scId = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart).Where(sci => sci.ProductId == product.Id && sci.AttributesXml == attributes).FirstOrDefault()?.Id;
            if (updatecartitem != null  && updatecartitem.Id != scId && scId != null)
            {
                _shoppingCartService.DeleteShoppingCartItem(updatecartitem);
                updatecartitem = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)shoppingCartTypeId, InovatiqaDefaults.StoreId).FirstOrDefault(x => x.Id == scId);
                quantity = quantity + updatecartitem.Quantity;
            }
            
            var cartType = updatecartitem == null ? (int)shoppingCartTypeId :
                updatecartitem.ShoppingCartTypeId;

            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            bool displayinFlyout = false;
            if ((int)ShoppingCartType.ShoppingCart == shoppingCartTypeId)
            {
                // comment by hamza because pproduct is not added to cart
                displayinFlyout = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart).Where(sci => sci.ProductId == updatecartitem.ProductId && sci.AttributesXml == updatecartitem.AttributesXml).FirstOrDefault() == null;
            }

            addToCartWarnings.AddRange(_shoppingCartService.UpdateShoppingCartItem(_workContextService.CurrentCustomer,
                    updatecartitem == null ? 0 : updatecartitem.Id, attributes, 0,
                    rentalStartDate, rentalEndDate, quantity, true));
            if (shoppingCartTypeId == (int)ShoppingCartType.ShoppingCart)
                _shoppingCartService.UpdatePurchaseStatus(_workContextService.CurrentCustomer);

            return GetProductToCartDetails(addToCartWarnings, cartType, product, displayinFlyout, attributes);
        }

        [HttpPost]
        public virtual IActionResult GetEstimateShipping(EstimateShippingModel model, IFormCollection form)
        {
            if (model == null)
                model = new EstimateShippingModel();

            var errors = new List<string>();
            if (string.IsNullOrEmpty(model.ZipPostalCode))
                errors.Add("Zip / postal code is required");

            if (model.CountryId == null || model.CountryId == 0)
                errors.Add("Country is required");

            if (errors.Count > 0)
                return Json(new
                {
                    success = false,
                    errors = errors
                });

            var cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            ParseAndSaveCheckoutAttributes(cart, form);

            var result = _shoppingCartModelFactory.PrepareEstimateShippingResultModel(cart, model.CountryId, model.StateProvinceId, model.ZipPostalCode, true);

            return Json(new
            {
                success = true,
                result = result
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult AddProductToCart_ProductListing(int productId, int quantity, IFormCollection form)
        {
            // check if product is already in cart, then set show flag to false, otherwise true.
            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("Homepage")
                });
            }

            if (product.ProductTypeId != InovatiqaDefaults.SimpleProduct)
            {
                return Json(new
                {
                    success = false,
                    message = "Only simple products could be added to the cart"
                });
            }
            //if(product.StockQuantity == 0 || product.StockQuantity < quantity)
            //{
            //    return Json(new
            //    {
            //        success = false,
            //        message = "Product stock quantity is not enough. Please contact admin."
            //    });
            //}

            var addToCartWarnings = new List<string>();
            var attributes = _productAttributeParserService.ParseProductAttributes(product, form, addToCartWarnings);
            ShoppingCartItem updatecartitem = null;

            var cartType = updatecartitem == null ? (int)ShoppingCartType.ShoppingCart :
                updatecartitem.ShoppingCartTypeId;

            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;


            bool showInFlyoutCart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart).Where(sci => sci.ProductId == productId && sci.AttributesXml == attributes).FirstOrDefault() == null;
            SaveItem(updatecartitem, addToCartWarnings, product, cartType, attributes, 0, rentalStartDate, rentalEndDate, quantity, 0);

            return GetProductToCartDetails(addToCartWarnings, cartType, product, showInFlyoutCart, attributes);
        }
        public virtual IActionResult GuestCart()
        {
            if(_customerService.IsRegistered(_workContextService.CurrentCustomer)){
                return Redirect("~/cart");
            }
            var cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);
            var model = new ShoppingCartModel();
            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        public virtual IActionResult DeleteGuestCart(int id)
        {
            if(id == -1)
            {
                var sci = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart);
                foreach (var item in sci)
                {
                    _shoppingCartService.DeleteShoppingCartItem(item.Id); 
                }
            }
            else
            {
                _shoppingCartService.DeleteShoppingCartItem(id);
            }
            
            return Json(new
            {
                success = true,
                message = "Item deleted successfully."
            });
        }
       



        public virtual IActionResult GuestCheckout()
        {
            if (_customerService.IsRegistered(_workContextService.CurrentCustomer))
            {
                return Redirect("~/onepagecheckout");
            }
            var cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);
            var model = new ShoppingCartModel();
            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);
            return View(model);
        }
        
        public virtual IActionResult ChangeQuantities(string ProductIds, string Quantities)
        {
            var customer = _workContextService.CurrentCustomer;
            string[] Ids = ProductIds.Split(",");
            string[] Qty = Quantities.Split(",");
            if (Ids.Length != Qty.Length)
                return new StatusCodeResult(500);
            var cart = _shoppingCartService.GetShoppingCart(customer, Convert.ToInt32(ShoppingCartType.ShoppingCart));

            try
            {
                for(var i = 0; i<Ids.Length; i++)
                {
                    var sci = cart.Where(x => x.Id == Convert.ToInt32(Ids[i])).FirstOrDefault();
                    var product = _productService.GetProductById(sci.ProductId);
                    sci.Quantity = Convert.ToInt32(Qty[i]) > product.OrderMaximumQuantity ? product.OrderMaximumQuantity : Convert.ToInt32(Qty[i]);
                    _shoppingCartService.UpdateShoppingCartItem(customer, sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice, quantity: sci.Quantity);
                }
                return Json(true);
            }
            catch (Exception)
            {
                return new StatusCodeResult(500); 
            }
        }

        public virtual IActionResult GetFlyoutCartData(Product product, string attributes = "", bool shouldShow = true)
        {
            var item = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart).Where(sci => sci.ProductId == product.Id && sci.AttributesXml == attributes).FirstOrDefault();
            var customer = _workContextService.CurrentCustomer;
            List<Product> products = new List<Product> { product };
            var productOverview = _productModelFactory.PrepareProductOverviewModels(products, true, true).FirstOrDefault();
            var shoppingCartItem = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart);
            var price = item != null ? _priceFormatter.FormatPrice(_shoppingCartService.GetUnitPrice(item)) : null;
            return Json(new
            {
                productId = shoppingCartItem.Where(sci => sci.ProductId == product.Id && sci.AttributesXml == attributes).FirstOrDefault().Id,
                shouldDisplay = shouldShow,
                pictureURL = productOverview.DefaultPictureModel.FullSizeImageUrl,
                quantity = shoppingCartItem.Where(sci => sci.ProductId == product.Id && sci.AttributesXml == attributes).FirstOrDefault().Quantity,
                price = price != null ? price : productOverview.ProductPrice.Price,
                name = product.Name,
                totalItems = shoppingCartItem.Count(),
            }) ;
        }
        #endregion

        #region Suspended Cart

        public virtual IActionResult DeleteSuspendedShoppingCart(string suspendedCartIds)
        {
            if (suspendedCartIds == null || suspendedCartIds == "")
            {
                return Json(new
                {
                    success = false,
                    message = "No carts selected to delete"
                });
            }
            var ids = suspendedCartIds.Split(',').Select(int.Parse).ToList();
            var customer = _workContextService.CurrentCustomer;
            foreach (var item in ids)
            {
                var suspendedShoppingCart = _shoppingCartService.DeleteSuspendedShoppingCart(customer, item);
            }
            return Json(new
            {
                success = true,
                message = ""
            });
        }

        public virtual IActionResult SaveSuspendedShoppingCart(string suspendedItemIds, string suspendedCartName,string suspendedCartEmail,string suspendedCartComment)
        {
            //var array = suspendedItemIds.Split(',').Select(x => int.Parse(x)).ToArray();
            var array = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart).Select(sc => sc.Id).ToArray();
            var customer = _workContextService.CurrentCustomer;
            var suspendedShoppingCart = _shoppingCartService.SaveSuspendedShoppingCart(customer, array, suspendedCartName, suspendedCartEmail, suspendedCartComment);

            return Json(suspendedShoppingCart);
        }

        #endregion

        #region Wishlist

        [RequireHttps]
        public virtual IActionResult Wishlist( Guid? customerGuid, int? wishListId = 0, int CategoryFilter = -1)
        {
            if (!InovatiqaDefaults.EnableWishlist)
                return RedirectToRoute("Homepage");

            var customer = customerGuid.HasValue && customerGuid != Guid.Empty ?
                _customerService.GetCustomerByGuid(customerGuid.Value)
                : _workContextService.CurrentCustomer;
            if (customer == null)
                return RedirectToRoute("Homepage");

            var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.Wishlist, InovatiqaDefaults.StoreId, null, null, null, wishListId);

            var model = new WishlistModel();
            model = _shoppingCartModelFactory.PrepareWishlistModel(model, cart, !customerGuid.HasValue, wishListId, CategoryFilter);
            model.Items = model.Items.GroupBy(x => x.Sku).Select(x => x.FirstOrDefault()).ToList();
            model.SelectedCategory = CategoryFilter;
            return View(model);
        }

        [RequireHttps]
        public virtual IActionResult WishlistByName(int? wishListId = 0)
        {
            // return RedirectToAction("Wishlist", new { wishListId = wishListId });

            if (!InovatiqaDefaults.EnableWishlist)
                return RedirectToRoute("Homepage");

            var customer = _workContextService.CurrentCustomer;
            var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.Wishlist, InovatiqaDefaults.StoreId, null, null, null, wishListId);

            var model = new WishlistModel();
            model = _shoppingCartModelFactory.PrepareWishlistModel(model: model, cart: cart, wishListId: wishListId);
            model.Items = model.Items.GroupBy(x => x.Sku).Select(x => x.FirstOrDefault()).ToList();
            return View("Wishlist" ,model);
        }

        [HttpPost, ActionName("Wishlist")]
        [FormValueRequired("updatecart")]
        public virtual IActionResult UpdateWishlist(IFormCollection form)
        {
            if (!InovatiqaDefaults.EnableWishlist)
                return RedirectToRoute("Homepage");

            var cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.Wishlist, InovatiqaDefaults.StoreId);

            var allIdsToRemove = form.ContainsKey("removefromcart")
                ? form["removefromcart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList()
                : new List<int>();

            var innerWarnings = new Dictionary<int, IList<string>>();
            foreach (var sci in cart)
            {
                var remove = allIdsToRemove.Contains(sci.Id);
                if (remove)
                    _shoppingCartService.DeleteShoppingCartItem(sci);
                else
                {
                    foreach (var formKey in form.Keys)
                        if (formKey.Equals($"itemquantity{sci.Id}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (int.TryParse(form[formKey], out var newQuantity))
                            {
                                var currSciWarnings = _shoppingCartService.UpdateShoppingCartItem(_workContextService.CurrentCustomer,
                                    sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                    sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                    newQuantity, true);
                                innerWarnings.Add(sci.Id, currSciWarnings);
                            }

                            break;
                        }
                }
            }

            cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.Wishlist, InovatiqaDefaults.StoreId);
            var model = new WishlistModel();
            model = _shoppingCartModelFactory.PrepareWishlistModel(model, cart);

            foreach (var kvp in innerWarnings)
            {
                var sciId = kvp.Key;
                var warnings = kvp.Value;
                var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
                if (sciModel != null)
                    foreach (var w in warnings)
                        if (!sciModel.Warnings.Contains(w))
                            sciModel.Warnings.Add(w);
            }

            return View(model);
        }

        //[HttpPost, ActionName("Wishlist")]
        //[FormValueRequired("addtocartbutton")]
        //public virtual IActionResult AddItemsToCartFromWishlist(Guid? customerGuid, IFormCollection form)
        //{
        //    if (!InovatiqaDefaults.EnableShoppingCart)
        //        return RedirectToRoute("Homepage");

        //    if (!InovatiqaDefaults.EnableWishlist)
        //        return RedirectToRoute("Homepage");

        //    var pageCustomer = customerGuid.HasValue
        //        ? _customerService.GetCustomerByGuid(customerGuid.Value)
        //        : _workContextService.CurrentCustomer;
        //    if (pageCustomer == null)
        //        return RedirectToRoute("Homepage");

        //    var pageCart = _shoppingCartService.GetShoppingCart(pageCustomer, (int)ShoppingCartType.Wishlist, InovatiqaDefaults.StoreId);

        //    var allWarnings = new List<string>();
        //    var countOfAddedItems = 0;
        //    var allIdsToAdd = form.ContainsKey("addtocart")
        //        ? form["addtocart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
        //        : new List<int>();
        //    foreach (var sci in pageCart)
        //    {
        //        if (allIdsToAdd.Contains(sci.Id))
        //        {
        //            var product = _productService.GetProductById(sci.ProductId);

        //            var warnings = _shoppingCartService.AddToCart(_workContextService.CurrentCustomer,
        //                product, (int)ShoppingCartType.ShoppingCart,
        //                InovatiqaDefaults.StoreId,
        //                sci.AttributesXml, sci.CustomerEnteredPrice,
        //                sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, true);
        //            if (!warnings.Any())
        //                countOfAddedItems++;
        //            if (InovatiqaDefaults.MoveItemsFromWishlistToCart &&
        //                !customerGuid.HasValue &&
        //                !warnings.Any())
        //            {
        //                _shoppingCartService.DeleteShoppingCartItem(sci);
        //            }

        //            allWarnings.AddRange(warnings);
        //        }
        //    }

        //    if (countOfAddedItems > 0)
        //    {
        //        if (allWarnings.Any())
        //        {
        //            _notificationService.ErrorNotification("Some product(s) from wishlist could not be moved to the cart for some reasons.");
        //        }

        //        return RedirectToRoute("ShoppingCart");
        //    }
        //    else
        //    {
        //        _notificationService.WarningNotification("No products selected to add to cart.");
        //    }

        //    if (allWarnings.Any())
        //    {
        //        _notificationService.ErrorNotification("Some product(s) from wishlist could not be moved to the cart for some reasons.");
        //    }

        //    var cart = _shoppingCartService.GetShoppingCart(pageCustomer, (int)ShoppingCartType.Wishlist, InovatiqaDefaults.StoreId);

        //    var model = new WishlistModel();
        //    model = _shoppingCartModelFactory.PrepareWishlistModel(model, cart, !customerGuid.HasValue);
        //    return View(model);
        //}
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult AddItemsToCartFromWishlist(Guid? customerGuid, int qty, Microsoft.AspNetCore.Http.IFormCollection form)
        {
            if (!InovatiqaDefaults.EnableShoppingCart)
                return RedirectToRoute("Homepage");

            if (!InovatiqaDefaults.EnableWishlist)
                return RedirectToRoute("Homepage");

            var pageCustomer = _workContextService.CurrentCustomer;
            if (pageCustomer == null)
                return RedirectToRoute("Homepage");

            var pageCart = _shoppingCartService.GetShoppingCart(pageCustomer, (int)ShoppingCartType.Wishlist, InovatiqaDefaults.StoreId);

            var allWarnings = new List<string>();
            var countOfAddedItems = 0;
            var allIdsToAdd = form.ContainsKey("addtocart")
                ? form["addtocart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
                : new List<int>();
            foreach (var sci in pageCart)
            {
                if (allIdsToAdd.Contains(sci.Id))
                {
                    var product = _productService.GetProductById(sci.ProductId);
                    var addToCartWarnings = new List<string>();
                    var attributes = _productAttributeParserService.ParseProductAttributes(product, form, addToCartWarnings);

                    var warnings = _shoppingCartService.AddToCart(_workContextService.CurrentCustomer,
                        product, (int)ShoppingCartType.ShoppingCart,
                        InovatiqaDefaults.StoreId,
                        attributes, sci.CustomerEnteredPrice,
                        sci.RentalStartDateUtc, sci.RentalEndDateUtc, qty, true);
                    if (!warnings.Any())
                        countOfAddedItems++;
                    if (InovatiqaDefaults.MoveItemsFromWishlistToCart &&
                        !customerGuid.HasValue &&
                        !warnings.Any())
                    {
                        //_shoppingCartService.DeleteShoppingCartItem(sci); Prevent Iterm Deletion from List
                    } 

                    allWarnings.AddRange(warnings);
                }
            }

            if (countOfAddedItems > 0)
            {
                if (allWarnings.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Some product(s) from wishlist could not be moved to the cart for some reasons."
                    });
                }
                return Json(new
                {
                    success = true,
                    samePageRedirect = true
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "No products selected to add to cart."
                });
            }
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult AddbulkProductToCart_WishList(Guid? customerGuid, IFormCollection form)
        {
            List<string> quantities = form["quantities"].FirstOrDefault().Split(',').ToList();
            if (!InovatiqaDefaults.EnableShoppingCart)
                return RedirectToRoute("Homepage");

            if (!InovatiqaDefaults.EnableWishlist)
                return RedirectToRoute("Homepage");

            var pageCustomer = _workContextService.CurrentCustomer;
            if (pageCustomer == null)
                return RedirectToRoute("Homepage");

            var pageCart = _shoppingCartService.GetShoppingCart(pageCustomer, (int)ShoppingCartType.Wishlist, InovatiqaDefaults.StoreId);

            var allWarnings = new List<string>();
            var countOfAddedItems = 0;
           // var allIdsToAdd = form.ContainsKey("addtocart")
           //     ? form["addtocart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
           //     : new List<int>();
           var allIdsToAdd = form.ContainsKey("addtocart")
                ? form["addtocart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
                : form["orderItemIds"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
            foreach (var sci in pageCart)
            {
                if (allIdsToAdd.Contains(sci.Id))
                {
                    var product = _productService.GetProductById(sci.ProductId);
                    var addToCartWarnings = new List<string>();
                    var attributes = _productAttributeParserService.ParseProductAttributes(product, form, addToCartWarnings);

                    var warnings = _shoppingCartService.AddToCart(_workContextService.CurrentCustomer,
                        product, (int)ShoppingCartType.ShoppingCart,
                        InovatiqaDefaults.StoreId,
                        attributes, sci.CustomerEnteredPrice,
                        sci.RentalStartDateUtc, sci.RentalEndDateUtc, Convert.ToInt32(quantities[allIdsToAdd.IndexOf(sci.Id)]), true);
                    if (!warnings.Any())
                        countOfAddedItems++;
                    if (InovatiqaDefaults.MoveItemsFromWishlistToCart &&
                        !customerGuid.HasValue &&
                        !warnings.Any())
                    {
                        _shoppingCartService.DeleteShoppingCartItem(sci);
                    }

                    allWarnings.AddRange(warnings);
                }
            }

            if (!form.ContainsKey("addtocart"))
            {
                var customer = _workContextService.CurrentCustomer;
                var cart = _shoppingCartService.GetShoppingCart(customer, Convert.ToInt32(ShoppingCartType.ShoppingCart));
                for(int i = 0; i < allIdsToAdd.Count; i++)
                {
                  
                    var product = _productService.GetProductById(_orderService.GetOrderItemById(allIdsToAdd[i]).ProductId);
                    var addToCartWarnings = new List<string>();
                    var attributes = _productAttributeParserService.ParseProductAttributes(product, form, addToCartWarnings);

                    var warnings = _shoppingCartService.AddToCart(customer: _workContextService.CurrentCustomer,
                    product: product,
                    shoppingCartTypeId: Convert.ToInt32(ShoppingCartType.ShoppingCart),
                    storeId: InovatiqaDefaults.StoreId,
                    attributesXml: attributes,
                    quantity: Convert.ToInt32(quantities[i]), 
                    wishListId: 0,
                    reordered: true);
                    if(!warnings.Any())
                        countOfAddedItems++;
                }
            }


            if (countOfAddedItems > 0)
            {
                if (allWarnings.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Some product(s) from wishlist could not be moved to the cart for some reasons."
                    });
                }
                return Json(new
                {
                    success = true,
                    message = "Redirect to cart"
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "No products selected to add to cart."
                });
            }
        }

        public virtual IActionResult IsProductHasAttributes(int productId)
        {
            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(productId);
            if (productAttributes.Any(pam => pam.AttributeControlTypeId != (int)AttributeControlType.ReadonlyCheckboxes))
            {
                return Json(true);
            }
            else
                return Json(false);
        }

        public virtual IActionResult PopulateWishList()
        {
            if (!InovatiqaDefaults.EnableWishlist)
                return RedirectToRoute("Homepage");

            var wishLists = _shoppingCartService.PopulateWishList(_workContextService.CurrentCustomer);

            return Json(wishLists);
        }
        
        public virtual IActionResult SaveWishlistName(string wishListName, bool isPublic)
        {
            if (!(_customerService.IsRegistered(_workContextService.CurrentCustomer)))
            {
                return Json(new { success = false, message = "Please login to create a wishlist." });
            }

            if (!InovatiqaDefaults.EnableWishlist)
                return Json(new { success = false, message="Wishlists are not allowed." });

            var wishList = _shoppingCartService.SaveWishListName(_workContextService.CurrentCustomer, wishListName, isPublic);

            return Json(wishList);
        }

        public virtual IActionResult RemoveWishList(int wishListId = 0)
        {
            if (wishListId == 0)
                return Json(false);
            if (!InovatiqaDefaults.EnableWishlist)
                return RedirectToRoute("Homepage");

            var wishList = _shoppingCartService.RemoveWishList(_workContextService.CurrentCustomer, wishListId);

            return Json(wishList);
        }

        public virtual IActionResult ViewAllWishlists()
        {
            if (!_customerService.IsRegistered(_workContextService.CurrentCustomer))
            {
                return Redirect("~/Wishlist");
            }
            IList<WishList> model = _shoppingCartService.PopulateWishList(_workContextService.CurrentCustomer);
            return View(model);
        }

        public virtual IActionResult CreateWishList()
        {
            //IList<WishList> model = _shoppingCartService.PopulateWishList(_workContextService.CurrentCustomer);
            return View("CreateWishList");
        }
        public virtual IActionResult ConvertSuspendedCartToShoppingCart(List<string> suspendedShoppingCartIds)
        {
            //var customer = _workContextService.CurrentCustomer;
            //foreach (var item in suspendedShoppingCartIds)
            //{
            //    _shoppingCartService.CopySuspendedItemsToShoppingCartItems(customer, item);
            //}
            //return Json(new
            //{
            //    success = true,
            //    message = ""
            //});
            string[] subs = suspendedShoppingCartIds[0].Split(',');
            var customer = _workContextService.CurrentCustomer;
            foreach (var item in subs)
            {
                _shoppingCartService.CopySuspendedItemsToShoppingCartItems(customer, Convert.ToInt32(item));
            }
            return Json(new
            {
                success = true,
                message = ""
            });
        }
        public virtual IActionResult SuspendedCartItems(int SuspendedCartId)
        {
            var customer = _workContextService.CurrentCustomer;
            var cart = _shoppingCartService.GetSuspendedShoppingCart(customer, SuspendedCartId);
            var model = new ShoppingCartModel();
            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);
            var billingAddress = _addressService.GetAddressById(Convert.ToInt32(customer.BillingAddressId));
            var shippingAddress = _addressService.GetAddressById(Convert.ToInt32(customer.ShippingAddressId));
            model.OrderReviewData.BillingAddress = new Models.Common.AddressModel{
                Id = billingAddress.Id,
                Company = billingAddress.Company ?? "COMPANY N/A",
                Address1 = billingAddress.Address1,
                City = billingAddress.City ?? "CITY N/A",
                StateProvinceName = billingAddress.StateProvince?.Name ?? "STATE N/A",
                ZipPostalCode = billingAddress.ZipPostalCode ?? "ZIP N/A"
            };
            model.OrderReviewData.ShippingAddress = new Models.Common.AddressModel
            {
                Id = shippingAddress.Id,
                Company = shippingAddress.Company ?? "COMPANY N/A",
                Address1 = shippingAddress.Address1,
                City = shippingAddress.City ?? "CITY N/A",
                StateProvinceName = shippingAddress.StateProvince?.Name ?? "STATE N/A",
                ZipPostalCode = shippingAddress.ZipPostalCode ?? "ZIP N/A"
            };
            //model.Items = _shoppingCartModelFactory.PrepareSuspendedCartItemsModel(SuspendedCartId);

            return View(model);
        }
        public virtual IActionResult RemoveFromShoppingCart(int Id)
        {
            _shoppingCartService.DeleteShoppingCartItem(Id);
            return Json(true);
        }
        #endregion

    }
}