using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Services.Catalog
{
    public partial class PriceCalculationService : IPriceCalculationService
    {
        #region Fields
        private readonly ICategoryService _categoryService;
        private readonly ICustomerService _customerService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductAttributeParserService _productAttributeParserService;
        private readonly IProductService _productService;
        private readonly IWorkContextService _workContextService;
        private readonly IProductAttributeService _productAttributeService;

        #endregion

        #region Ctor

        public PriceCalculationService(ICategoryService categoryService,
            ICustomerService customerService,
            IManufacturerService manufacturerService,
            IProductAttributeParserService productAttributeParserService,
            IProductService productService,
            IWorkContextService workContextService,
            IProductAttributeService productAttributeService)
        {
            _categoryService = categoryService;
            _customerService = customerService;
            _manufacturerService = manufacturerService;
            _productAttributeParserService = productAttributeParserService;
            _productService = productService;
            _workContextService = workContextService;
            _productAttributeService = productAttributeService;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        public virtual decimal GetFinalPrice(Product product,
            Customer customer,
            decimal additionalCharge = decimal.Zero,
            bool includeDiscounts = true,
            int quantity = 1)
        {
            return GetFinalPrice(product, customer, additionalCharge, includeDiscounts,
              quantity, out _, out _);
            //return 80;
        }

        public virtual decimal GetFinalPrice(Product product,
            Customer customer,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
            out decimal discountAmount,
            out List<Discount> appliedDiscounts)
        {
            return GetFinalPrice(product, customer,
                additionalCharge, includeDiscounts, quantity,
                null, null,
                out discountAmount, out appliedDiscounts);
        }

        public virtual decimal GetFinalPrice(Product product,
            Customer customer,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
            DateTime? rentalStartDate,
            DateTime? rentalEndDate,
            out decimal discountAmount,
            out List<Discount> appliedDiscounts)
        {
            return GetFinalPrice(product, customer, null, additionalCharge, includeDiscounts, quantity,
                rentalStartDate, rentalEndDate, out discountAmount, out appliedDiscounts);
        }

        public virtual decimal GetFinalPrice(Product product,
            Customer customer,
            decimal? overriddenProductPrice,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
            DateTime? rentalStartDate,
            DateTime? rentalEndDate,
            out decimal discountAmount,
            out List<Discount> appliedDiscounts)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            decimal rezPrice;
            discountAmount = 0;
            appliedDiscounts = new List<Discount>();

            var price = overriddenProductPrice ?? product.Price;
            price += additionalCharge;
            
            var tierPrice = _productService.GetPreferredTierPrice(product, customer, InovatiqaDefaults.StoreId, quantity);
            if (tierPrice != null)
            {
                if (tierPrice.EntityName == "Product" && tierPrice.CustomerId == customer.Id && product.HasTierPrices)
                {
                    //price = tierPrice.Price * quantity;
                    // 07-06-2022 ALI AHMAD - DUE TO ISSUE IN OVERAL PRODUCT PRICE WAS MULTIPLIED BY ITS QUANTITY
                    if(additionalCharge == 0)
                    {
                        price = Convert.ToDecimal(tierPrice.Rate);
                    }
                }
                else if (tierPrice.EntityName == "Category")
                {
                        price = price - (Convert.ToDecimal(tierPrice.Rate) / 100 * price);
                }
                else if (tierPrice.EntityName == "ALL")
                {
                    price = price - (Convert.ToDecimal(tierPrice.Rate) / 100 * price);
                }

            }

            //price += additionalCharge;

            if (price < decimal.Zero)
                price = decimal.Zero;

            rezPrice = price;

            return rezPrice;
        }

        public virtual decimal GetProductAttributeValuePriceAdjustment(Product product, ProductAttributeValue value, Customer customer, decimal? productPrice = null)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var adjustment = decimal.Zero;
            switch (value.AttributeValueTypeId)
            {
                case 0:
                    if (value.PriceAdjustmentUsePercentage)
                    {
                        if (!productPrice.HasValue)
                            productPrice = GetFinalPrice(product, customer);

                        adjustment = (decimal)((float)productPrice * (float)value.PriceAdjustment / 100f);
                    }
                    else
                    {
                        adjustment = value.PriceAdjustment;
                        if (adjustment > 0)
                        {
                            var prod = new Product
                            {
                                RootCategoryId = product.RootCategoryId,
                                Price = adjustment,
                                Id = product.Id,
                                HasTierPrices = true
                            };
                            //var tierPrice = _productService.GetTierPrices(prod, customer, InovatiqaDefaults.StoreId);
                            //if (tierPrice != null)
                            //    adjustment = Convert.ToInt32(tierPrice[0].Rate);
                            //if (tierPrice != null)
                            //{
                            //    if (tierPrice[0].EntityName == "Product" && tierPrice[0].CustomerId == customer.Id)
                            //    {
                            //        if (prod.HasTierPrices)
                            //        {
                            //            //price = tierPrice.Price * quantity;
                            //            // 07-06-2022 ALI AHMAD - DUE TO ISSUE IN OVERAL PRODUCT PRICE WAS MULTIPLIED BY ITS QUANTITY
                            //            adjustment = Convert.ToDecimal(tierPrice[0].Rate);
                            //        }
                            //    }
                            //    else if (tierPrice[0].EntityName == "Category")
                            //    {
                            //        adjustment = prod.Price - (Convert.ToDecimal(tierPrice[0].Rate) / 100 * prod.Price);
                            //    }
                            //    else if (tierPrice[0].EntityName == "ALL")
                            //    {
                            //        adjustment = prod.Price - (Convert.ToDecimal(tierPrice[0].Rate) / 100 * prod.Price);
                            //    }

                            //}
                        }
                    }

                    break;
                case 10:
                    var associatedProduct = _productService.GetProductById(value.AssociatedProductId);
                    if (associatedProduct != null)
                    {
                        adjustment = GetFinalPrice(associatedProduct, _workContextService.CurrentCustomer) * value.Quantity;
                    }

                    break;
                default:
                    break;
            }

            return adjustment;
        }

        public virtual decimal RoundPrice(decimal value, Currency currency = null)
        {
            return Round(value, InovatiqaDefaults.Rounding001);
        }

        public virtual decimal Round(decimal value, int roundingTypeId)
        {
            var rez = Math.Round(value, 2);
            var fractionPart = (rez - Math.Truncate(rez)) * 10;

            if (fractionPart == 0)
                return rez;

            switch (roundingTypeId)
            {
                case InovatiqaDefaults.Rounding005Up:
                case InovatiqaDefaults.Rounding005Down:
                    fractionPart = (fractionPart - Math.Truncate(fractionPart)) * 10;

                    fractionPart %= 5;
                    if (fractionPart == 0)
                        break;

                    if (roundingTypeId == InovatiqaDefaults.Rounding005Up)
                        fractionPart = 5 - fractionPart;
                    else
                        fractionPart *= -1;

                    rez += fractionPart / 100;
                    break;
                case InovatiqaDefaults.Rounding01Up:
                case InovatiqaDefaults.Rounding01Down:
                    fractionPart = (fractionPart - Math.Truncate(fractionPart)) * 10;

                    if (roundingTypeId == InovatiqaDefaults.Rounding01Down && fractionPart == 5)
                        fractionPart = -5;
                    else
                        fractionPart = fractionPart < 5 ? fractionPart * -1 : 10 - fractionPart;

                    rez += fractionPart / 100;
                    break;
                case InovatiqaDefaults.Rounding05:
                    fractionPart *= 10;
                    fractionPart = fractionPart < 25 ? fractionPart * -1 : fractionPart < 50 || fractionPart < 75 ? 50 - fractionPart : 100 - fractionPart;

                    rez += fractionPart / 100;
                    break;
                case InovatiqaDefaults.Rounding1:
                case InovatiqaDefaults.Rounding1Up:
                    fractionPart *= 10;

                    if (roundingTypeId == InovatiqaDefaults.Rounding1Up && fractionPart > 0)
                        rez = Math.Truncate(rez) + 1;
                    else
                        rez = fractionPart < 50 ? Math.Truncate(rez) : Math.Truncate(rez) + 1;

                    break;
                case InovatiqaDefaults.Rounding001:
                default:
                    break;
            }

            return rez;
        }

        public virtual decimal GetProductCost(Product product, string attributesXml)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var cost = product.ProductCost;
            var attributeValues = _productAttributeParserService.ParseProductAttributeValues(attributesXml);
            foreach (var attributeValue in attributeValues)
            {
                switch (attributeValue.AttributeValueTypeId)
                {
                    case InovatiqaDefaults.Simple:
                        cost += attributeValue.Cost;
                        break;
                    case InovatiqaDefaults.AssociatedToProduct:
                        var associatedProduct = _productService.GetProductById(attributeValue.AssociatedProductId);
                        if (associatedProduct != null)
                            cost += associatedProduct.ProductCost * attributeValue.Quantity;
                        break;
                    default:
                        break;
                }
            }

            return cost;
        }

        #endregion
    }
}