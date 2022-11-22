using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Services.Discounts.Interfaces
{
    public partial interface IDiscountService
    {
        #region Discounts

        Discount GetDiscountById(int discountId);

        IList<Discount> GetAllDiscounts(int? discountTypeId = null,
            string couponCode = null, string discountName = null, bool showHidden = false,
            DateTime? startDateUtc = null, DateTime? endDateUtc = null);

        bool ContainsDiscount(IList<Discount> discounts, Discount discount);

        List<Discount> GetPreferredDiscount(IList<Discount> discounts,
            decimal amount, out decimal discountAmount);

        decimal GetDiscountAmount(Discount discount, decimal amount);

        #endregion

        #region Discounts (caching)



        #endregion

        #region Discount requirements

        IList<DiscountRequirement> GetAllDiscountRequirements(int discountId = 0, bool topLevelOnly = false);

        IList<DiscountRequirement> GetDiscountRequirementsByParent(DiscountRequirement discountRequirement);

        #endregion

        #region Validation

        DiscountValidationResult ValidateDiscount(Discount discount, Customer customer);

        DiscountValidationResult ValidateDiscount(Discount discount, Customer customer, string[] couponCodesToValidate);

        #endregion

        #region Discount usage history

        IPagedList<DiscountUsageHistory> GetAllDiscountUsageHistory(int? discountId = null,
            int? customerId = null, int? orderId = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        void InsertDiscountUsageHistory(DiscountUsageHistory discountUsageHistory);

        #endregion
    }
}