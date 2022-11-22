using Inovatiqa.Database.Models;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Services.Catalog.Interfaces
{
    public partial interface IPriceCalculationService
    {
        decimal GetFinalPrice(Product product,
            Customer customer,
            decimal additionalCharge = decimal.Zero,
            bool includeDiscounts = true,
            int quantity = 1);
       

        decimal GetFinalPrice(Product product,
            Customer customer,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
            out decimal discountAmount,
            out List<Discount> appliedDiscounts);

        decimal GetFinalPrice(Product product,
            Customer customer,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
            DateTime? rentalStartDate,
            DateTime? rentalEndDate,
            out decimal discountAmount,
            out List<Discount> appliedDiscounts);

        decimal GetFinalPrice(Product product,
            Customer customer,
            decimal? overriddenProductPrice,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
            DateTime? rentalStartDate,
            DateTime? rentalEndDate,
            out decimal discountAmount,
            out List<Discount> appliedDiscounts);

        decimal GetProductAttributeValuePriceAdjustment(Product product, ProductAttributeValue value, Customer customer, decimal? productPrice = null);

        decimal RoundPrice(decimal value, Currency currency = null);

        decimal GetProductCost(Product product, string attributesXml);

    }
}