using Inovatiqa.Database.Models;
using Inovatiqa.Web.Models.Common;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Web.Factories.Interfaces
{
    public partial interface IAddressModelFactory
    {
        void PrepareAddressModel(AddressModel model,
            Address address, bool excludeProperties,
            Func<IList<Country>> loadCountries = null,
            bool prePopulateWithCustomerFields = false,
            Customer customer = null,
            string overrideAttributesXml = "");
    }
}
