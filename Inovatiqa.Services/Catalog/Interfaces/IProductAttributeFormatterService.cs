using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Catalog.Interfaces
{
    public partial interface IProductAttributeFormatterService
    {
        string FormatAttributes(Product product, string attributesXml);

        string FormatAttributes(Product product, string attributesXml,
            Customer customer, string separator = "<br />", bool htmlEncode = true, bool renderPrices = true,
            bool renderProductAttributes = true, bool renderGiftCardAttributes = true,
            bool allowHyperlinks = true);
    }
}
