using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Orders.Interfaces
{
    public partial interface IOrderTotalCalculationService
    {
        void UpdateOrderTotals(UpdateOrderParameters updateOrderParameters, IList<ShoppingCartItem> restoredCart);
        void GetShoppingCartSubTotal(IList<ShoppingCartItem> cart,
            bool includingTax,
            out decimal discountAmount, out List<Discount> appliedDiscounts,
            out decimal subTotalWithoutDiscount, out decimal subTotalWithDiscount);

        void GetShoppingCartSubTotal(IList<ShoppingCartItem> cart,
            bool includingTax,
            out decimal discountAmount, out List<Discount> appliedDiscounts,
            out decimal subTotalWithoutDiscount, out decimal subTotalWithDiscount,
            out SortedDictionary<decimal, decimal> taxRates);

        decimal? GetShoppingCartShippingTotal(IList<ShoppingCartItem> cart);

        decimal? GetShoppingCartShippingTotal(IList<ShoppingCartItem> cart, bool includingTax);

        decimal? GetShoppingCartShippingTotal(IList<ShoppingCartItem> cart, bool includingTax, out decimal taxRate);

        decimal? GetShoppingCartShippingTotal(IList<ShoppingCartItem> cart, bool includingTax,
            out decimal taxRate, out List<Discount> appliedDiscounts);

        bool IsFreeShipping(IList<ShoppingCartItem> cart, decimal? subTotal = null);

        decimal AdjustShippingRate(decimal shippingRate, IList<ShoppingCartItem> cart, out List<Discount> appliedDiscounts, bool applyToPickupInStore = false);

        decimal GetShoppingCartAdditionalShippingCharge(IList<ShoppingCartItem> cart);

        decimal GetTaxTotal(IList<ShoppingCartItem> cart, out SortedDictionary<decimal, decimal> taxRates,
            bool usePaymentMethodAdditionalFee = true);

        decimal GetTaxTotal(IList<ShoppingCartItem> cart, bool usePaymentMethodAdditionalFee = true);

        decimal? GetShoppingCartTotal(IList<ShoppingCartItem> cart,
            bool? useRewardPoints = null, bool usePaymentMethodAdditionalFee = true);

        decimal? GetShoppingCartTotal(IList<ShoppingCartItem> cart,
            out decimal discountAmount, out List<Discount> appliedDiscounts,
            out int redeemedRewardPoints, out decimal redeemedRewardPointsAmount,
            bool? useRewardPoints = null, bool usePaymentMethodAdditionalFee = true);

    }
}
