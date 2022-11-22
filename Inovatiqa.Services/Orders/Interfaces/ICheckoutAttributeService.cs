using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Orders.Interfaces
{
    public partial interface ICheckoutAttributeService
    {
        #region Checkout attributes

        IList<CheckoutAttribute> GetAllCheckoutAttributes(int storeId = 0, bool excludeShippableAttributes = false);

        CheckoutAttribute GetCheckoutAttributeById(int checkoutAttributeId);

        IList<CheckoutAttributeValue> GetCheckoutAttributeValues(int checkoutAttributeId);

        #endregion

        #region Checkout attribute values

        CheckoutAttributeValue GetCheckoutAttributeValueById(int checkoutAttributeValueId);

        #endregion
    }
}
