using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Orders.Interfaces
{
    public partial interface ICheckoutAttributeFormatterService
    {
        string FormatAttributes(string attributesXml);

        string FormatAttributes(string attributesXml,
            Customer customer, 
            string separator = "<br />", 
            bool htmlEncode = true,
            bool renderPrices = true,
            bool allowHyperlinks = true);
    }
}
