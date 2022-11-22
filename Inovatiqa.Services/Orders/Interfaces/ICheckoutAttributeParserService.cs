using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Orders.Interfaces
{
    public partial interface ICheckoutAttributeParserService
    {
        IList<CheckoutAttribute> ParseCheckoutAttributes(string attributesXml);
        IEnumerable<(CheckoutAttribute attribute, IEnumerable<CheckoutAttributeValue> values)> ParseCheckoutAttributeValues(string attributesXml);

        IList<string> ParseValues(string attributesXml, int checkoutAttributeId);

        string EnsureOnlyActiveAttributes(string attributesXml, IList<ShoppingCartItem> cart);

        bool? IsConditionMet(CheckoutAttribute attribute, string selectedAttributesXml);

        string AddCheckoutAttribute(string attributesXml, CheckoutAttribute ca, string value);

        string RemoveCheckoutAttribute(string attributesXml, CheckoutAttribute attribute);

    }
}
