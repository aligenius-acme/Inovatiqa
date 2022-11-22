using Inovatiqa.Core;
using Inovatiqa.Core.Domain.Discounts;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Discounts.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Payments.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Orders
{
    public partial class OrderTotalCalculationService : IOrderTotalCalculationService
    {
        #region Fields

        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICheckoutAttributeParserService _checkoutAttributeParser;
        private readonly IShippingService _shippingService;
        private readonly IDiscountService _discountService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPaymentService _paymentService;
        private readonly Lazy<IOrderService> _orderService;
        private readonly IAddressService _addressService;
        private readonly IWorkContextService _workContextService;
        private readonly Lazy<IShippingRateComputationMethodService> _shippingRateComputationMethodService;

        #endregion

        #region Ctor

        public OrderTotalCalculationService(IShoppingCartService shoppingCartService,
            ICustomerService customerService,
            IProductService productService,
            IGenericAttributeService genericAttributeService,
            ICheckoutAttributeParserService checkoutAttributeParser,
            IShippingService shippingService,
            IAddressService addressService,
            IDiscountService discountService,
            IPriceCalculationService priceCalculationService,
            IPaymentService paymentService,
            Lazy<IShippingRateComputationMethodService> shippingRateComputationMethodService,
            Lazy<IOrderService> orderService,
            IWorkContextService workContextService)
        {
            _shoppingCartService = shoppingCartService;
            _customerService = customerService;
            _productService = productService;
            _genericAttributeService = genericAttributeService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _shippingService = shippingService;
            _discountService = discountService;
            _priceCalculationService = priceCalculationService;
            _paymentService = paymentService;
            _shippingRateComputationMethodService = shippingRateComputationMethodService;
            _orderService = orderService;
            _addressService = addressService;
            _workContextService = workContextService;
    }

        #endregion

        #region Utilities

        protected virtual void UpdateTotal(UpdateOrderParameters updateOrderParameters, decimal subTotalExclTax,
            decimal discountAmountExclTax, decimal shippingTotalExclTax, decimal taxTotal)
        {
            var updatedOrder = updateOrderParameters.UpdatedOrder;
            var customer = _customerService.GetCustomerById(updatedOrder.CustomerId);

            var total = subTotalExclTax - discountAmountExclTax + shippingTotalExclTax + updatedOrder.PaymentMethodAdditionalFeeExclTax + taxTotal;

            var discountAmountTotal =
                GetOrderTotalDiscount(customer, total, out var orderAppliedDiscounts);
            if (total < discountAmountTotal)
                discountAmountTotal = total;
            total -= discountAmountTotal;

            //foreach (var giftCard in _giftCardService.GetAllGiftCards(usedWithOrderId: updatedOrder.Id))
            //{
            //    if (total <= decimal.Zero)
            //        continue;

            //    var remainingAmount = _giftCardService.GetGiftCardUsageHistory(giftCard)
            //        .Where(history => history.UsedWithOrderId == updatedOrder.Id).Sum(history => history.UsedValue);
            //    var amountCanBeUsed = total > remainingAmount ? remainingAmount : total;
            //    total -= amountCanBeUsed;
            //}

            //var rewardPointsOfOrder = _rewardPointService.GetRewardPointsHistoryEntryById(updatedOrder.RedeemedRewardPointsEntryId ?? 0);
            //if (rewardPointsOfOrder != null)
            //{
            //    var rewardPoints = -rewardPointsOfOrder.Points;
            //    var rewardPointsAmount = ConvertRewardPointsToAmount(rewardPoints);
            //    if (total < rewardPointsAmount)
            //    {
            //        rewardPoints = ConvertAmountToRewardPoints(total);
            //        rewardPointsAmount = total;
            //    }

            //    if (total > decimal.Zero)
            //        total -= rewardPointsAmount;

            //    //uncomment here for the return unused reward points if new order total less redeemed reward points amount
            //    //if (rewardPoints < -rewardPointsOfOrder.Points)
            //    //    _rewardPointService.AddRewardPointsHistoryEntry(customer, -rewardPointsOfOrder.Points - rewardPoints, _storeContext.CurrentStore.Id, "Return unused reward points");

            //    if (rewardPointsAmount != rewardPointsOfOrder.UsedAmount)
            //    {
            //        rewardPointsOfOrder.UsedAmount = rewardPointsAmount;
            //        rewardPointsOfOrder.Points = -rewardPoints;
            //        _rewardPointService.UpdateRewardPointsHistoryEntry(rewardPointsOfOrder);
            //    }
            //}

            if (total < decimal.Zero)
                total = decimal.Zero;
            if (InovatiqaDefaults.RoundPricesDuringCalculation)
                total = _priceCalculationService.RoundPrice(total);

            updatedOrder.OrderDiscount = discountAmountTotal;
            updatedOrder.OrderTotal = total;

            foreach (var discount in orderAppliedDiscounts)
                if (!_discountService.ContainsDiscount(updateOrderParameters.AppliedDiscounts, discount))
                    updateOrderParameters.AppliedDiscounts.Add(discount);
        }

        protected virtual decimal UpdateShipping(UpdateOrderParameters updateOrderParameters, IList<ShoppingCartItem> restoredCart,
            decimal subTotalInclTax, decimal subTotalExclTax, out decimal shippingTotalInclTax, out decimal shippingTaxRate)
        {
            var shippingTotalExclTax = decimal.Zero;
            shippingTotalInclTax = decimal.Zero;
            shippingTaxRate = decimal.Zero;

            var updatedOrder = updateOrderParameters.UpdatedOrder;
            var customer = _customerService.GetCustomerById(updatedOrder.CustomerId);

            if (_shoppingCartService.ShoppingCartRequiresShipping(restoredCart))
            {
                if (!IsFreeShipping(restoredCart, subTotalInclTax))
                {
                    var shippingTotal = decimal.Zero;
                    if (!string.IsNullOrEmpty(updatedOrder.ShippingRateComputationMethodSystemName))
                    {
                        if (updatedOrder.PickupInStore)
                        {
                            if (InovatiqaDefaults.AllowPickupInStore)
                            {
                                var pickupPointsResponse = _shippingService.GetPickupPoints(updatedOrder.BillingAddressId, customer,
                                    updatedOrder.ShippingRateComputationMethodSystemName, InovatiqaDefaults.StoreId);
                                if (pickupPointsResponse.Success)
                                {
                                    var selectedPickupPoint =
                                        pickupPointsResponse.PickupPoints.FirstOrDefault(point =>
                                            updatedOrder.ShippingMethod.Contains(point.Name));
                                    if (selectedPickupPoint != null)
                                        shippingTotal = selectedPickupPoint.PickupFee;
                                    else
                                        updateOrderParameters.Warnings.Add(
                                            $"Shipping method {updatedOrder.ShippingMethod} could not be loaded");
                                }
                                else
                                    updateOrderParameters.Warnings.AddRange(pickupPointsResponse.Errors);
                            }
                            else
                                updateOrderParameters.Warnings.Add("Pick up in store is not available");
                        }
                        else
                        {
                            var shippingAddress = _addressService.GetAddressById(updatedOrder.ShippingAddressId ?? 0);
                            var shippingOptionsResponse = _shippingService.GetShippingOptions(restoredCart, shippingAddress, customer, updatedOrder.ShippingRateComputationMethodSystemName, InovatiqaDefaults.StoreId);
                            if (shippingOptionsResponse.Success)
                            {
                                var shippingOption = shippingOptionsResponse.ShippingOptions.FirstOrDefault(option =>
                                    updatedOrder.ShippingMethod.Contains(option.Name));
                                if (shippingOption != null)
                                    shippingTotal = shippingOption.Rate;
                                else
                                    updateOrderParameters.Warnings.Add(
                                        $"Shipping method {updatedOrder.ShippingMethod} could not be loaded");
                            }
                            else
                                updateOrderParameters.Warnings.AddRange(shippingOptionsResponse.Errors);
                        }
                    }
                    else
                    {
                        if (InovatiqaDefaults.AllowPickupInStore)
                        {
                            var pickupPointsResponse = _shippingService.GetPickupPoints(updatedOrder.BillingAddressId, _workContextService.CurrentCustomer, storeId: InovatiqaDefaults.StoreId);
                            if (pickupPointsResponse.Success)
                            {
                                updateOrderParameters.PickupPoint = pickupPointsResponse.PickupPoints
                                    .OrderBy(point => point.PickupFee).First();
                                shippingTotal = updateOrderParameters.PickupPoint.PickupFee;
                            }
                            else
                                updateOrderParameters.Warnings.AddRange(pickupPointsResponse.Errors);
                        }
                        else
                            updateOrderParameters.Warnings.Add("Pick up in store is not available");

                        if (updateOrderParameters.PickupPoint == null)
                        {
                            var customerShippingAddress = _customerService.GetCustomerShippingAddress(customer);

                            var shippingOptionsResponse = _shippingService.GetShippingOptions(restoredCart, customerShippingAddress, _workContextService.CurrentCustomer, storeId: InovatiqaDefaults.StoreId);
                            if (shippingOptionsResponse.Success)
                            {
                                var shippingOption = shippingOptionsResponse.ShippingOptions.OrderBy(option => option.Rate)
                                    .First();
                                updatedOrder.ShippingRateComputationMethodSystemName =
                                    shippingOption.ShippingRateComputationMethodSystemName;
                                updatedOrder.ShippingMethod = shippingOption.Name;

                                var updatedShippingAddress = _addressService.CloneAddress(customerShippingAddress, customer, false);
                                _addressService.InsertAddress(updatedShippingAddress);
                                updatedOrder.ShippingAddressId = updatedShippingAddress.Id;

                                shippingTotal = shippingOption.Rate;
                            }
                            else
                                updateOrderParameters.Warnings.AddRange(shippingOptionsResponse.Errors);
                        }
                    }

                    shippingTotal += GetShoppingCartAdditionalShippingCharge(restoredCart);

                    shippingTotal -= GetShippingDiscount(customer, shippingTotal, out var shippingTotalDiscounts);
                    if (shippingTotal < decimal.Zero)
                        shippingTotal = decimal.Zero;

                    shippingTotalExclTax = shippingTotal;
                    shippingTotalInclTax = shippingTotal;

                    if (InovatiqaDefaults.RoundPricesDuringCalculation)
                    {
                        shippingTotalExclTax = _priceCalculationService.RoundPrice(shippingTotalExclTax);
                        shippingTotalInclTax = _priceCalculationService.RoundPrice(shippingTotalInclTax);
                    }

                    if (updatedOrder.ShippingStatusId == (int)ShippingStatus.ShippingNotRequired ||
                        updatedOrder.ShippingStatusId == (int)ShippingStatus.NotYetShipped)
                        updatedOrder.ShippingStatusId = (int)ShippingStatus.NotYetShipped;
                    else
                        updatedOrder.ShippingStatusId = (int)ShippingStatus.PartiallyShipped;

                    foreach (var discount in shippingTotalDiscounts)
                        if (!_discountService.ContainsDiscount(updateOrderParameters.AppliedDiscounts, discount))
                            updateOrderParameters.AppliedDiscounts.Add(discount);
                }
            }
            else
                updatedOrder.ShippingStatusId = (int)ShippingStatus.ShippingNotRequired;

            updatedOrder.OrderShippingExclTax = shippingTotalExclTax;
            updatedOrder.OrderShippingInclTax = shippingTotalInclTax;
            return shippingTotalExclTax;
        }


        protected virtual decimal GetOrderSubtotalDiscount(Customer customer,
            decimal orderSubTotal, out List<Discount> appliedDiscounts)
        {
            appliedDiscounts = new List<Discount>();
            var discountAmount = decimal.Zero;
            if (InovatiqaDefaults.IgnoreDiscounts)
                return discountAmount;

            var allDiscounts = _discountService.GetAllDiscounts((int)DiscountType.AssignedToOrderSubTotal);
            var allowedDiscounts = new List<Discount>();
            if (allDiscounts != null)
            {
                foreach (var discount in allDiscounts)
                    if (!_discountService.ContainsDiscount(allowedDiscounts, discount) &&
                        _discountService.ValidateDiscount(discount, customer).IsValid)
                    {
                        allowedDiscounts.Add(discount);
                    }
            }

            appliedDiscounts = _discountService.GetPreferredDiscount(allowedDiscounts, orderSubTotal, out discountAmount);

            if (discountAmount < decimal.Zero)
                discountAmount = decimal.Zero;

            return discountAmount;
        }

        protected virtual decimal UpdateSubTotal(UpdateOrderParameters updateOrderParameters, IList<ShoppingCartItem> restoredCart,
            out decimal subTotalInclTax, out SortedDictionary<decimal, decimal> subTotalTaxRates, out decimal discountAmountExclTax)
        {
            var subTotalExclTax = decimal.Zero;
            subTotalInclTax = decimal.Zero;
            subTotalTaxRates = new SortedDictionary<decimal, decimal>();

            var updatedOrder = updateOrderParameters.UpdatedOrder;
            var updatedOrderItem = updateOrderParameters.UpdatedOrderItem;

            foreach (var shoppingCartItem in restoredCart)
            {
                decimal itemSubTotalExclTax;
                decimal itemSubTotalInclTax;
                decimal taxRate;

                if (shoppingCartItem.Id == updatedOrderItem.Id)
                {
                    updatedOrderItem.UnitPriceExclTax = updateOrderParameters.PriceExclTax;
                    updatedOrderItem.UnitPriceInclTax = updateOrderParameters.PriceInclTax;
                    updatedOrderItem.DiscountAmountExclTax = updateOrderParameters.DiscountAmountExclTax;
                    updatedOrderItem.DiscountAmountInclTax = updateOrderParameters.DiscountAmountInclTax;
                    updatedOrderItem.PriceExclTax = itemSubTotalExclTax = updateOrderParameters.SubTotalExclTax;
                    updatedOrderItem.PriceInclTax = itemSubTotalInclTax = updateOrderParameters.SubTotalInclTax;
                    updatedOrderItem.Quantity = shoppingCartItem.Quantity;

                    taxRate = itemSubTotalExclTax > 0 ? Math.Round(100 * (itemSubTotalInclTax - itemSubTotalExclTax) / itemSubTotalExclTax, 3) : 0M;
                }
                else
                {
                    var order = _orderService.Value.GetOrderItemById(shoppingCartItem.Id);
                    itemSubTotalExclTax = order.PriceExclTax;
                    itemSubTotalInclTax = order.PriceInclTax;

                    taxRate = itemSubTotalExclTax > 0 ? Math.Round(100 * (itemSubTotalInclTax - itemSubTotalExclTax) / itemSubTotalExclTax, 3) : 0M;
                }

                subTotalExclTax += itemSubTotalExclTax;
                subTotalInclTax += itemSubTotalInclTax;

                //tax rates
                var itemTaxValue = itemSubTotalInclTax - itemSubTotalExclTax;
                if (taxRate <= decimal.Zero || itemTaxValue <= decimal.Zero)
                    continue;

                if (!subTotalTaxRates.ContainsKey(taxRate))
                    subTotalTaxRates.Add(taxRate, itemTaxValue);
                else
                    subTotalTaxRates[taxRate] = subTotalTaxRates[taxRate] + itemTaxValue;
            }

            if (subTotalExclTax < decimal.Zero)
                subTotalExclTax = decimal.Zero;

            if (subTotalInclTax < decimal.Zero)
                subTotalInclTax = decimal.Zero;

            var customer = _customerService.GetCustomerById(updatedOrder.CustomerId);
            discountAmountExclTax = GetOrderSubtotalDiscount(customer, subTotalExclTax, out var subTotalDiscounts);
            if (subTotalExclTax < discountAmountExclTax)
                discountAmountExclTax = subTotalExclTax;
            var discountAmountInclTax = discountAmountExclTax;

            //add tax for shopping items
            var tempTaxRates = new Dictionary<decimal, decimal>(subTotalTaxRates);
            foreach (var kvp in tempTaxRates)
            {
                if (kvp.Value == decimal.Zero || subTotalExclTax <= decimal.Zero)
                    continue;

                var discountTaxValue = kvp.Value * (discountAmountExclTax / subTotalExclTax);
                discountAmountInclTax += discountTaxValue;
                subTotalTaxRates[kvp.Key] = kvp.Value - discountTaxValue;
            }

            if (InovatiqaDefaults.RoundPricesDuringCalculation)
            {
                subTotalExclTax = _priceCalculationService.RoundPrice(subTotalExclTax);
                subTotalInclTax = _priceCalculationService.RoundPrice(subTotalInclTax);
                discountAmountExclTax = _priceCalculationService.RoundPrice(discountAmountExclTax);
                discountAmountInclTax = _priceCalculationService.RoundPrice(discountAmountInclTax);
            }

            updatedOrder.OrderSubtotalExclTax = subTotalExclTax;
            updatedOrder.OrderSubtotalInclTax = subTotalInclTax;
            updatedOrder.OrderSubTotalDiscountExclTax = discountAmountExclTax;
            updatedOrder.OrderSubTotalDiscountInclTax = discountAmountInclTax;

            foreach (var discount in subTotalDiscounts)
                if (!_discountService.ContainsDiscount(updateOrderParameters.AppliedDiscounts, discount))
                    updateOrderParameters.AppliedDiscounts.Add(discount);
            return subTotalExclTax;
        }

        protected virtual decimal GetShippingDiscount(Customer customer, decimal shippingTotal, out List<Discount> appliedDiscounts)
        {
            appliedDiscounts = new List<Discount>();
            var shippingDiscountAmount = decimal.Zero;
            if (InovatiqaDefaults.IgnoreDiscounts)
                return shippingDiscountAmount;

            var allDiscounts = _discountService.GetAllDiscounts(InovatiqaDefaults.AssignedToShipping);
            var allowedDiscounts = new List<Discount>();
            if (allDiscounts != null)
                foreach (var discount in allDiscounts)
                    if (!_discountService.ContainsDiscount(allowedDiscounts, discount) &&
                        _discountService.ValidateDiscount(discount, customer).IsValid)
                    {
                        allowedDiscounts.Add(discount);
                    }

            appliedDiscounts = _discountService.GetPreferredDiscount(allowedDiscounts, shippingTotal, out shippingDiscountAmount);

            if (shippingDiscountAmount < decimal.Zero)
                shippingDiscountAmount = decimal.Zero;

            if (InovatiqaDefaults.RoundPricesDuringCalculation)
                shippingDiscountAmount = _priceCalculationService.RoundPrice(shippingDiscountAmount);

            return shippingDiscountAmount;
        }

        protected virtual decimal GetOrderTotalDiscount(Customer customer, decimal orderTotal, out List<Discount> appliedDiscounts)
        {
            appliedDiscounts = new List<Discount>();
            var discountAmount = decimal.Zero;
            if (InovatiqaDefaults.IgnoreDiscounts)
                return discountAmount;

            var allDiscounts = _discountService.GetAllDiscounts(InovatiqaDefaults.AssignedToOrderTotal);
            var allowedDiscounts = new List<Discount>();
            if (allDiscounts != null)
                foreach (var discount in allDiscounts)
                    if (!_discountService.ContainsDiscount(allowedDiscounts, discount) &&
                        _discountService.ValidateDiscount(discount, customer).IsValid)
                    {
                        allowedDiscounts.Add(discount);
                    }

            appliedDiscounts = _discountService.GetPreferredDiscount(allowedDiscounts, orderTotal, out discountAmount);

            if (discountAmount < decimal.Zero)
                discountAmount = decimal.Zero;

            if (InovatiqaDefaults.RoundPricesDuringCalculation)
                discountAmount = _priceCalculationService.RoundPrice(discountAmount);

            return discountAmount;
        }

        protected virtual void SetRewardPoints(ref int redeemedRewardPoints, ref decimal redeemedRewardPointsAmount,
            bool? useRewardPoints, Customer customer, decimal orderTotal)
        {
            if (!InovatiqaDefaults.RewardPointsEnabled)
                return;
        }

        #endregion

        #region Methods

        public virtual void UpdateOrderTotals(UpdateOrderParameters updateOrderParameters, IList<ShoppingCartItem> restoredCart)
        {
            var subTotalExclTax = UpdateSubTotal(updateOrderParameters, restoredCart, out var subTotalInclTax, out var subTotalTaxRates, out var discountAmountExclTax);

            var shippingTotalExclTax = UpdateShipping(updateOrderParameters, restoredCart, subTotalInclTax, subTotalExclTax, out var shippingTotalInclTax, out var shippingTaxRate);

            var taxTotal = 0.0m;

            UpdateTotal(updateOrderParameters, subTotalExclTax, discountAmountExclTax, shippingTotalExclTax, taxTotal);
        }

        public virtual void GetShoppingCartSubTotal(IList<ShoppingCartItem> cart, bool includingTax,
            out decimal discountAmount, out List<Discount> appliedDiscounts,
            out decimal subTotalWithoutDiscount, out decimal subTotalWithDiscount)
        {
            GetShoppingCartSubTotal(cart, includingTax,
                out discountAmount, out appliedDiscounts,
                out subTotalWithoutDiscount, out subTotalWithDiscount, out _);
        }

        public virtual void GetShoppingCartSubTotal(IList<ShoppingCartItem> cart,
            bool includingTax,
            out decimal discountAmount, out List<Discount> appliedDiscounts,
            out decimal subTotalWithoutDiscount, out decimal subTotalWithDiscount,
            out SortedDictionary<decimal, decimal> taxRates)
        {
            discountAmount = decimal.Zero;
            appliedDiscounts = new List<Discount>();
            subTotalWithoutDiscount = decimal.Zero;
            subTotalWithDiscount = decimal.Zero;
            taxRates = new SortedDictionary<decimal, decimal>();

            if (!cart.Any())
                return;

            var customer = _customerService.GetShoppingCartCustomer(cart);

            var subTotal = decimal.Zero;
            foreach (var shoppingCartItem in cart)
            {
                var sciSubTotal = _shoppingCartService.GetSubTotal(shoppingCartItem);

                subTotal += sciSubTotal;
            }

            if (customer != null)
            {
                var checkoutAttributesXml = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CheckoutAttributes, customer.Id, InovatiqaDefaults.StoreId);
                var attributeValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(checkoutAttributesXml);
                if (attributeValues != null)
                {
                    foreach (var (attribute, values) in attributeValues)
                    {
                        foreach (var attributeValue in values)
                        {
                            var attributePrice = attributeValue.PriceAdjustment;

                            subTotal += attributePrice;
                        }
                    }
                }
            }

            subTotalWithoutDiscount = subTotal;
            subTotalWithDiscount = subTotalWithoutDiscount;
        }

        public virtual decimal? GetShoppingCartShippingTotal(IList<ShoppingCartItem> cart)
        {
            var includingTax = InovatiqaDefaults.SubTotalIncludingTax;
            return GetShoppingCartShippingTotal(cart, includingTax);
        }

        public virtual decimal? GetShoppingCartShippingTotal(IList<ShoppingCartItem> cart, bool includingTax)
        {
            return GetShoppingCartShippingTotal(cart, includingTax, out _);
        }

        public virtual decimal? GetShoppingCartShippingTotal(IList<ShoppingCartItem> cart, bool includingTax, out decimal taxRate)
        {
            return GetShoppingCartShippingTotal(cart, includingTax, out taxRate, out _);
        }

        public virtual decimal? GetShoppingCartShippingTotal(IList<ShoppingCartItem> cart, bool includingTax,
            out decimal taxRate, out List<Discount> appliedDiscounts)
        {
            decimal? shippingTotal = null;
            appliedDiscounts = new List<Discount>();
            taxRate = decimal.Zero;

            var customer = _customerService.GetShoppingCartCustomer(cart);

            var isFreeShipping = IsFreeShipping(cart);
            if (isFreeShipping)
                return decimal.Zero;

            ShippingOption shippingOption = null;
            if (customer != null)
                shippingOption = _genericAttributeService.GetAttribute<ShippingOption>(customer, InovatiqaDefaults.SelectedShippingOptionAttribute, customer.Id, InovatiqaDefaults.StoreId);

            if (shippingOption != null)
            {
                shippingTotal = AdjustShippingRate(shippingOption.Rate, cart, out appliedDiscounts, shippingOption.IsPickupInStore);
            }
            else
            {
                Address shippingAddress = null;
                if (customer != null)
                    shippingAddress = _customerService.GetCustomerShippingAddress(customer);

                var shippingOptionRequests = _shippingService.CreateShippingOptionRequests(cart,
                    shippingAddress,
                    InovatiqaDefaults.StoreId,
                    out _);
                decimal? fixedRate = null;
                foreach (var shippingOptionRequest in shippingOptionRequests)
                {
                    var fixedRateTmp = _shippingRateComputationMethodService.Value.GetFixedRate(shippingOptionRequest);
                    if (!fixedRateTmp.HasValue)
                        continue;

                    if (!fixedRate.HasValue)
                        fixedRate = decimal.Zero;

                    fixedRate += fixedRateTmp.Value;
                }

                if (fixedRate.HasValue)
                {
                    shippingTotal = AdjustShippingRate(fixedRate.Value, cart, out appliedDiscounts);
                }
            }

            if (!shippingTotal.HasValue)
                return null;

            if (shippingTotal.Value < decimal.Zero)
                shippingTotal = decimal.Zero;

            if (InovatiqaDefaults.RoundPricesDuringCalculation)
                shippingTotal = _priceCalculationService.RoundPrice(shippingTotal.Value);

            decimal? shippingTotalTaxed = shippingTotal;

            if (InovatiqaDefaults.RoundPricesDuringCalculation)
                shippingTotalTaxed = _priceCalculationService.RoundPrice(shippingTotalTaxed.Value);

            return shippingTotalTaxed;
        }

        public virtual bool IsFreeShipping(IList<ShoppingCartItem> cart, decimal? subTotal = null)
        {
            var customer = _customerService.GetCustomerById(cart.FirstOrDefault()?.CustomerId ?? 0);

            if (customer != null && _customerService.GetCustomerRoles(customer).Any(role => role.FreeShipping))
                return true;

            if (cart.All(shoppingCartItem => _shippingService.IsFreeShipping(shoppingCartItem)))
                return true;

            if (!InovatiqaDefaults.FreeShippingOverXEnabled)
                return false;

            if (!subTotal.HasValue)
            {
                GetShoppingCartSubTotal(cart, InovatiqaDefaults.FreeShippingOverXIncludingTax, out _, out _, out _, out var subTotalWithDiscount);
                subTotal = subTotalWithDiscount;
            }

            if (subTotal.Value > InovatiqaDefaults.FreeShippingOverXValue)
                return true;

            return false;
        }

        public virtual decimal AdjustShippingRate(decimal shippingRate, IList<ShoppingCartItem> cart,
            out List<Discount> appliedDiscounts, bool applyToPickupInStore = false)
        {
            appliedDiscounts = new List<Discount>();

            if (IsFreeShipping(cart))
                return decimal.Zero;

            var customer = _customerService.GetShoppingCartCustomer(cart);

            var pickupPoint = _genericAttributeService.GetAttribute<PickupPoint>(customer,
                    InovatiqaDefaults.SelectedPickupPointAttribute, customer.Id, InovatiqaDefaults.StoreId);

            var adjustedRate = shippingRate;

            if (!(applyToPickupInStore && InovatiqaDefaults.AllowPickupInStore && InovatiqaDefaults.IgnoreAdditionalShippingChargeForPickupInStore))
            {
                adjustedRate += GetShoppingCartAdditionalShippingCharge(cart);
            }

            var discountAmount = GetShippingDiscount(customer, adjustedRate, out appliedDiscounts);
            adjustedRate -= discountAmount;

            adjustedRate = Math.Max(adjustedRate, decimal.Zero);
            if (InovatiqaDefaults.RoundPricesDuringCalculation)
                adjustedRate = _priceCalculationService.RoundPrice(adjustedRate);

            return adjustedRate;
        }

        public virtual decimal GetShoppingCartAdditionalShippingCharge(IList<ShoppingCartItem> cart)
        {
            return cart.Sum(shoppingCartItem => _shippingService.GetAdditionalShippingCharge(shoppingCartItem));
        }

        public virtual decimal GetTaxTotal(IList<ShoppingCartItem> cart, out SortedDictionary<decimal, decimal> taxRates, bool usePaymentMethodAdditionalFee = true)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            ////////var taxTotalResult = _taxService.GetTaxTotal(cart, usePaymentMethodAdditionalFee);
            taxRates = new SortedDictionary<decimal, decimal>();
            ////////var taxTotal = taxTotalResult?.TaxTotal ?? decimal.Zero;

            ////////if (_shoppingCartSettings.RoundPricesDuringCalculation)
            ////////    taxTotal = _priceCalculationService.RoundPrice(taxTotal);

            ////////return taxTotal;
            ///
            return 0;
        }

        public virtual decimal GetTaxTotal(IList<ShoppingCartItem> cart, bool usePaymentMethodAdditionalFee = true)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            return GetTaxTotal(cart, out _, usePaymentMethodAdditionalFee);
        }

        public virtual decimal? GetShoppingCartTotal(IList<ShoppingCartItem> cart,
            bool? useRewardPoints = null, bool usePaymentMethodAdditionalFee = true)
        {
            return GetShoppingCartTotal(cart, out _, out _, out _, out _, useRewardPoints, usePaymentMethodAdditionalFee);
        }

        public virtual decimal? GetShoppingCartTotal(IList<ShoppingCartItem> cart,
            out decimal discountAmount, out List<Discount> appliedDiscounts,
            out int redeemedRewardPoints, out decimal redeemedRewardPointsAmount,
            bool? useRewardPoints = null, bool usePaymentMethodAdditionalFee = true)
        {
            redeemedRewardPoints = 0;
            redeemedRewardPointsAmount = decimal.Zero;

            var customer = _customerService.GetShoppingCartCustomer(cart);

            var paymentMethodSystemName = string.Empty;
            if (customer != null)
            {
                paymentMethodSystemName = _genericAttributeService.GetAttribute<string>(customer,
                    InovatiqaDefaults.SelectedPaymentMethodAttribute, customer.Id, InovatiqaDefaults.StoreId);
            }

            GetShoppingCartSubTotal(cart, false, out _, out _, out _, out var subTotalWithDiscountBase);

            var subtotalBase = subTotalWithDiscountBase;


            var shoppingCartShipping = GetShoppingCartShippingTotal(cart, false);


            var paymentMethodAdditionalFeeWithoutTax = decimal.Zero;
            if (usePaymentMethodAdditionalFee && !string.IsNullOrEmpty(paymentMethodSystemName))
            {
                var paymentMethodAdditionalFee = _paymentService.GetAdditionalHandlingFee(cart,
                    paymentMethodSystemName);
                paymentMethodAdditionalFeeWithoutTax = paymentMethodAdditionalFee;
            }

            var shoppingCartTax = GetTaxTotal(cart, usePaymentMethodAdditionalFee);

            var resultTemp = decimal.Zero;
            resultTemp += subtotalBase;
            if (shoppingCartShipping.HasValue)
            {
                resultTemp += shoppingCartShipping.Value;
            }

            resultTemp += paymentMethodAdditionalFeeWithoutTax;
            resultTemp += shoppingCartTax;
            if (InovatiqaDefaults.RoundPricesDuringCalculation)
                resultTemp = _priceCalculationService.RoundPrice(resultTemp);

            discountAmount = GetOrderTotalDiscount(customer, resultTemp, out appliedDiscounts);
 
            if (resultTemp < discountAmount)
                discountAmount = resultTemp;

            resultTemp -= discountAmount;

            if (resultTemp < decimal.Zero)
                resultTemp = decimal.Zero;
            if (InovatiqaDefaults.RoundPricesDuringCalculation)
                resultTemp = _priceCalculationService.RoundPrice(resultTemp);


            if (resultTemp < decimal.Zero)
                resultTemp = decimal.Zero;
            if (InovatiqaDefaults.RoundPricesDuringCalculation)
                resultTemp = _priceCalculationService.RoundPrice(resultTemp);

            if (!shoppingCartShipping.HasValue)
            {
                return null;
            }

            var orderTotal = resultTemp;

            SetRewardPoints(ref redeemedRewardPoints, ref redeemedRewardPointsAmount, useRewardPoints, customer, orderTotal);

            orderTotal -= redeemedRewardPointsAmount;

            if (InovatiqaDefaults.RoundPricesDuringCalculation)
                orderTotal = _priceCalculationService.RoundPrice(orderTotal);
            return orderTotal;
        }

        #endregion
    }
}