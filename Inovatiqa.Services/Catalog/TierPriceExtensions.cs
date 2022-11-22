using Inovatiqa.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Catalog
{
    public static class TierPriceExtensions
    {
        public static IEnumerable<TierPrice> FilterByStore(this IEnumerable<TierPrice> source, int storeId)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Where(tierPrice => tierPrice.StoreId == 0 || tierPrice.StoreId == storeId);
        }

        public static IEnumerable<TierPrice> FilterByCustomerRole(this IEnumerable<TierPrice> source, int[] customerRoleIds)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (customerRoleIds == null)
                throw new ArgumentNullException(nameof(customerRoleIds));

            if (!customerRoleIds.Any())
                return source;

            return source.Where(tierPrice =>
                !tierPrice.CustomerRoleId.HasValue || tierPrice.CustomerRoleId == 0 || customerRoleIds.Contains(tierPrice.CustomerRoleId.Value));
        }

        public static IEnumerable<TierPrice> RemoveDuplicatedQuantities(this IEnumerable<TierPrice> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var tierPrices = source.ToList();

            var tierPricesWithDuplicates = tierPrices.GroupBy(tierPrice => tierPrice.Quantity).Where(group => group.Count() > 1);

            var duplicatedPrices = tierPricesWithDuplicates.SelectMany(group =>
            {
                var minTierPrice = group.Aggregate((currentMinTierPrice, nextTierPrice) =>
                    (currentMinTierPrice.Price < nextTierPrice.Price ? currentMinTierPrice : nextTierPrice));

                return group.Where(tierPrice => tierPrice.Id != minTierPrice.Id);
            });

            return tierPrices.Where(tierPrice => duplicatedPrices.All(duplicatedPrice => duplicatedPrice.Id != tierPrice.Id));
        }

        public static IEnumerable<TierPrice> FilterByDate(this IEnumerable<TierPrice> source, DateTime? date = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (!date.HasValue)
                date = DateTime.UtcNow;

            return source.Where(tierPrice =>
                (!tierPrice.StartDateTimeUtc.HasValue || tierPrice.StartDateTimeUtc.Value < date) &&
                (!tierPrice.EndDateTimeUtc.HasValue || tierPrice.EndDateTimeUtc.Value > date));
        }
    }
}
