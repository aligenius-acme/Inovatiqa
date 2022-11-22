using System.Collections.Generic;
using Inovatiqa.Database.Models;
using Microsoft.AspNetCore.Http;

namespace Inovatiqa.Services.Catalog.Interfaces
{
    public partial interface IProductAttributeParserService
    {
        #region Product attributes

        ProductAttributeCombination FindProductAttributeCombination(Product product,
            string attributesXml, bool ignoreNonCombinableAttributes = true);

        bool AreProductAttributesEqual(string attributesXml1, string attributesXml2, bool ignoreNonCombinableAttributes, bool ignoreQuantity = true);

        IList<ProductProductAttributeMapping> ParseProductAttributeMappings(string attributesXml);

        IList<ProductAttributeValue> ParseProductAttributeValues(string attributesXml, int productAttributeMappingId = 0);

        IList<string> ParseValues(string attributesXml, int productAttributeMappingId);

        string AddProductAttribute(string attributesXml, ProductProductAttributeMapping productAttributeMapping, string value, int? quantity = null);

        bool? IsConditionMet(ProductProductAttributeMapping pam, string selectedAttributesXml);

        string ParseProductAttributes(Product product, IFormCollection form, List<string> errors);

        string RemoveProductAttribute(string attributesXml, ProductProductAttributeMapping productAttributeMapping);

        int ParseEnteredQuantity(Product product, IFormCollection form);
        
        #endregion

        #region Gift card attributes


        #endregion
    }
}
