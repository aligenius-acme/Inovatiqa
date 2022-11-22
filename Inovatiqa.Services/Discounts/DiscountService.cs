using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Discounts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Core.Interfaces;

namespace Inovatiqa.Services.Discounts
{
    public partial class DiscountService : IDiscountService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<DiscountRequirement> _discountRequirementRepository;
        private readonly IRepository<DiscountUsageHistory> _discountUsageHistoryRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IShoppingCartService _shoppingCartService;

        #endregion

        #region Ctor

        public DiscountService(ICustomerService customerService,
            IProductService productService,
            IRepository<Discount> discountRepository,
            IRepository<DiscountRequirement> discountRequirementRepository,
            IRepository<DiscountUsageHistory> discountUsageHistoryRepository,
            IRepository<Order> orderRepository,
            IShoppingCartService shoppingCartService)
        {
            _customerService = customerService;
            _productService = productService;
            _discountRepository = discountRepository;
            _discountRequirementRepository = discountRequirementRepository;
            _discountUsageHistoryRepository = discountUsageHistoryRepository;
            _orderRepository = orderRepository;
            _shoppingCartService = shoppingCartService;
        }

        #endregion

        #region Utilities

        protected bool GetValidationResult(IEnumerable<DiscountRequirement> requirements,
            int? groupInteractionTypeId, Customer customer, List<string> errors)
        {
            var result = false;

            foreach (var requirement in requirements)
            {
                if (requirement.IsGroup)
                {
                    var childRequirements = GetDiscountRequirementsByParent(requirement);

                    var interactionType = requirement.InteractionTypeId ?? InovatiqaDefaults.And;
                    result = GetValidationResult(childRequirements, interactionType, customer, errors);
                }
                else
                {

                    ////////var ruleResult = requirementRulePlugin.CheckRequirement(new DiscountRequirementValidationRequest
                    ////////{
                    ////////    DiscountRequirementId = requirement.Id,
                    ////////    Customer = customer,
                    ////////    Store = _storeContext.CurrentStore
                    ////////});

                    ////////if (!ruleResult.IsValid)
                    ////////{
                    ////////    var userError = !string.IsNullOrEmpty(ruleResult.UserError)
                    ////////        ? ruleResult.UserError
                    ////////        : _localizationService.GetResource("ShoppingCart.Discount.CannotBeUsed");
                    ////////    errors.Add(userError);
                    ////////}

                    ////////result = ruleResult.IsValid;
                }

                if (!result && groupInteractionTypeId == InovatiqaDefaults.And)
                    return false;
                if (result && groupInteractionTypeId == InovatiqaDefaults.Or)
                    return true;
            }

            return result;
        }

        #endregion

        #region Methods

        #region Discounts
        public virtual Discount GetDiscountById(int discountId)
        {
            if (discountId == 0)
                return null;

            return _discountRepository.GetById(discountId);
        }
        public virtual IList<Discount> GetAllDiscounts(int? discountTypeId = null,
            string couponCode = null, string discountName = null, bool showHidden = false,
            DateTime? startDateUtc = null, DateTime? endDateUtc = null)
        {

            var query = _discountRepository.Query();

            if (!showHidden)
            {
                query = query.Where(discount =>
                    (!discount.StartDateUtc.HasValue || discount.StartDateUtc <= DateTime.UtcNow) &&
                    (!discount.EndDateUtc.HasValue || discount.EndDateUtc >= DateTime.UtcNow));
            }

            if (!string.IsNullOrEmpty(couponCode))
                query = query.Where(discount => discount.CouponCode == couponCode);

            if (!string.IsNullOrEmpty(discountName))
                query = query.Where(discount => discount.Name.Contains(discountName));

            query = query.OrderBy(discount => discount.Name).ThenBy(discount => discount.Id);

            query = query.ToList().AsQueryable();

            if (discountTypeId.HasValue)
                query = query.Where(discount => discount.DiscountTypeId == discountTypeId.Value);

            if (startDateUtc.HasValue)
                query = query.Where(discount =>
                    !discount.StartDateUtc.HasValue || discount.StartDateUtc >= startDateUtc.Value);
            if (endDateUtc.HasValue)
                query = query.Where(discount =>
                    !discount.EndDateUtc.HasValue || discount.EndDateUtc <= endDateUtc.Value);

            return query.ToList();
        }

        public virtual bool ContainsDiscount(IList<Discount> discounts, Discount discount)
        {
            if (discounts == null)
                throw new ArgumentNullException(nameof(discounts));

            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            foreach (var dis1 in discounts)
                if (discount.Id == dis1.Id)
                    return true;

            return false;
        }

        public virtual List<Discount> GetPreferredDiscount(IList<Discount> discounts,
            decimal amount, out decimal discountAmount)
        {
            if (discounts == null)
                throw new ArgumentNullException(nameof(discounts));

            var result = new List<Discount>();
            discountAmount = decimal.Zero;
            if (!discounts.Any())
                return result;

            foreach (var discount in discounts)
            {
                var currentDiscountValue = GetDiscountAmount(discount, amount);
                if (currentDiscountValue <= discountAmount)
                    continue;

                discountAmount = currentDiscountValue;

                result.Clear();
                result.Add(discount);
            }
            var cumulativeDiscounts = discounts.Where(x => x.IsCumulative).OrderBy(x => x.Name).ToList();
            if (cumulativeDiscounts.Count <= 1)
                return result;

            var cumulativeDiscountAmount = cumulativeDiscounts.Sum(d => GetDiscountAmount(d, amount));
            if (cumulativeDiscountAmount <= discountAmount)
                return result;

            discountAmount = cumulativeDiscountAmount;

            result.Clear();
            result.AddRange(cumulativeDiscounts);

            return result;
        }

        public virtual decimal GetDiscountAmount(Discount discount, decimal amount)
        {
            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            decimal result;
            if (discount.UsePercentage)
                result = (decimal)((float)amount * (float)discount.DiscountPercentage / 100f);
            else
                result = discount.DiscountAmount;

            if (discount.UsePercentage &&
                discount.MaximumDiscountAmount.HasValue &&
                result > discount.MaximumDiscountAmount.Value)
                result = discount.MaximumDiscountAmount.Value;

            if (result < decimal.Zero)
                result = decimal.Zero;

            return result;
        }

        #endregion

        #region Discounts (caching)


        #endregion

        #region Discount requirements

        public virtual IList<DiscountRequirement> GetAllDiscountRequirements(int discountId = 0, bool topLevelOnly = false)
        {
            var query = _discountRequirementRepository.Query();

            if (discountId > 0)
                query = query.Where(requirement => requirement.DiscountId == discountId);

            if (topLevelOnly)
                query = query.Where(requirement => !requirement.ParentId.HasValue);

            query = query.OrderBy(requirement => requirement.Id);

            return query.ToList();
        }

        public virtual IList<DiscountRequirement> GetDiscountRequirementsByParent(DiscountRequirement discountRequirement)
        {
            if (discountRequirement is null)
                throw new ArgumentNullException(nameof(discountRequirement));

            return _discountRequirementRepository.Query().Where(dr => dr.ParentId == discountRequirement.Id).ToList();
        }

        #endregion

        #region Validation

        public virtual DiscountValidationResult ValidateDiscount(Discount discount, Customer customer)
        {
            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var couponCodesToValidate = _customerService.ParseAppliedDiscountCouponCodes(customer);
            return ValidateDiscount(discount, customer, couponCodesToValidate);
        }

        public virtual DiscountValidationResult ValidateDiscount(Discount discount, Customer customer, string[] couponCodesToValidate)
        {
            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var result = new DiscountValidationResult();

            if (discount.RequiresCouponCode)
            {
                if (string.IsNullOrEmpty(discount.CouponCode))
                    return result;

                if (couponCodesToValidate == null)
                    return result;

                if (!couponCodesToValidate.Any(x => x.Equals(discount.CouponCode, StringComparison.InvariantCultureIgnoreCase)))
                    return result;
            }

            if (discount.DiscountTypeId == InovatiqaDefaults.AssignedToOrderSubTotal ||
                discount.DiscountTypeId == InovatiqaDefaults.AssignedToOrderTotal)
            {
                var cart = _shoppingCartService.GetShoppingCart(customer,
                    (int)ShoppingCartType.ShoppingCart, storeId: InovatiqaDefaults.StoreId);

                var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();

            }

            var now = DateTime.UtcNow;
            if (discount.StartDateUtc.HasValue)
            {
                var startDate = DateTime.SpecifyKind(discount.StartDateUtc.Value, DateTimeKind.Utc);
                if (startDate.CompareTo(now) > 0)
                {
                    result.Errors = new List<string> { "Sorry, this offer is not started yet" };
                    return result;
                }
            }

            if (discount.EndDateUtc.HasValue)
            {
                var endDate = DateTime.SpecifyKind(discount.EndDateUtc.Value, DateTimeKind.Utc);
                if (endDate.CompareTo(now) < 0)
                {
                    result.Errors = new List<string> { "Sorry, this offer is expired" };
                    return result;
                }
            }

            switch (discount.DiscountLimitationId)
            {
                case InovatiqaDefaults.NTimesOnly:
                    {
                        var usedTimes = GetAllDiscountUsageHistory(discount.Id, null, null, 0, 1).TotalCount;
                        if (usedTimes >= discount.LimitationTimes)
                            return result;
                    }

                    break;
                case InovatiqaDefaults.NTimesPerCustomer:
                    {
                        if (_customerService.IsRegistered(customer))
                        {
                            var usedTimes = GetAllDiscountUsageHistory(discount.Id, customer.Id, null, 0, 1).TotalCount;
                            if (usedTimes >= discount.LimitationTimes)
                            {
                                result.Errors = new List<string> { "Sorry, you've used this discount already" };
                                return result;
                            }
                        }
                    }

                    break;
                case InovatiqaDefaults.Unlimited:
                default:
                    break;
            }


            var requirements = GetAllDiscountRequirements(discount.Id, true);

            var topLevelGroup = requirements.FirstOrDefault();
            if (topLevelGroup == null || (topLevelGroup.IsGroup && !GetDiscountRequirementsByParent(topLevelGroup).Any()) || !topLevelGroup.InteractionTypeId.HasValue)
            {
                result.IsValid = true;
                return result;
            }

            var errors = new List<string>();
            result.IsValid = GetValidationResult(requirements, topLevelGroup.InteractionTypeId, customer, errors);

            if (!result.IsValid)
                result.Errors = errors;

            return result;
        }

        #endregion

        #region Discount usage history

        public virtual IPagedList<DiscountUsageHistory> GetAllDiscountUsageHistory(int? discountId = null,
            int? customerId = null, int? orderId = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var discountUsageHistory = _discountUsageHistoryRepository.Query();

            if (discountId.HasValue && discountId.Value > 0)
                discountUsageHistory = discountUsageHistory.Where(historyRecord => historyRecord.DiscountId == discountId.Value);

            if (customerId.HasValue && customerId.Value > 0)
                discountUsageHistory = from duh in discountUsageHistory
                                       join order in _orderRepository.Query() on duh.OrderId equals order.Id
                                       where order.CustomerId == customerId
                                       select duh;

            if (orderId.HasValue && orderId.Value > 0)
                discountUsageHistory = discountUsageHistory.Where(historyRecord => historyRecord.OrderId == orderId.Value);

            discountUsageHistory = from duh in discountUsageHistory
                                   join order in _orderRepository.Query() on duh.OrderId equals order.Id
                                   where !order.Deleted
                                   select duh;

            discountUsageHistory = discountUsageHistory.OrderByDescending(historyRecord => historyRecord.CreatedOnUtc).ThenBy(historyRecord => historyRecord.Id);

            return new PagedList<DiscountUsageHistory>(discountUsageHistory, pageIndex, pageSize);
        }

        public virtual void InsertDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        {
            if (discountUsageHistory == null)
                throw new ArgumentNullException(nameof(discountUsageHistory));

            _discountUsageHistoryRepository.Insert(discountUsageHistory);

            //event notification
            //_eventPublisher.EntityInserted(discountUsageHistory);
        }

        #endregion

        #endregion
    }
}