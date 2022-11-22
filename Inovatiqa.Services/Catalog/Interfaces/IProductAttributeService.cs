using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Catalog.Interfaces
{
    public partial interface IProductAttributeService
    {
        #region Product attributes

        ProductAttribute GetProductAttributeById(int productAttributeId);

        IPagedList<ProductAttribute> GetAllProductAttributes(int pageIndex = 0, int pageSize = int.MaxValue);

        #endregion

        #region Product attributes mappings

        ProductProductAttributeMapping GetProductAttributeMappingById(int productAttributeMappingId);

        IList<ProductProductAttributeMapping> GetProductAttributeMappingsByProductId(int productId);

        #endregion

        #region Product attribute values

        IList<ProductAttributeValue> GetProductAttributeValues(int productAttributeMappingId);

        ProductAttributeValue GetProductAttributeValueById(int productAttributeValueId);

        #endregion

        #region Predefined product attribute values


        #endregion

        #region Product attribute combinations

        void DeleteProductAttributeCombination(ProductAttributeCombination combination);

        IList<ProductAttributeCombination> GetAllProductAttributeCombinations(int productId);

        void UpdateProductAttributeCombination(ProductAttributeCombination combination);

        ProductAttributeCombination GetProductAttributeCombinationById(int productAttributeCombinationId);

        ProductAttributeCombination GetProductAttributeCombinationBySku(string sku);

        #endregion
    }
}
