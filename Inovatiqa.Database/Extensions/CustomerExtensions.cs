using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using System;

namespace Inovatiqa.Database.Extensions
{
    public static class CustomerExtensions
    {
        public static bool IsSearchEngineAccount(this Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (!customer.IsSystemAccount || string.IsNullOrEmpty(customer.SystemName))
                return false;

            var result = customer.SystemName.Equals(InovatiqaDefaults.SearchEngineCustomerName, StringComparison.InvariantCultureIgnoreCase);

            return result;
        }
    }
}