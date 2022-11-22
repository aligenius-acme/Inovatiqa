using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Catalog.Interfaces
{
    public partial interface ISpecificationAttributeService
    {
        #region Specification attribute

        SpecificationAttribute GetSpecificationAttributeById(int specificationAttributeId);

        IList<SpecificationAttribute> GetSpecificationAttributesWithOptions();

        #endregion

        #region Specification attribute option

        SpecificationAttributeOption GetSpecificationAttributeOptionById(int specificationAttributeOption);

        #endregion

        #region Product specification attribute

        void DeleteProductSpecificationAttribute(ProductSpecificationAttributeMapping productSpecificationAttribute);

        ProductSpecificationAttributeMapping GetProductSpecificationAttributeById(int productSpecificationAttributeId);

        void InsertProductSpecificationAttribute(ProductSpecificationAttributeMapping productSpecificationAttribute);

        IList<ProductSpecificationAttributeMapping> GetProductSpecificationAttributes(int productId = 0,
            int specificationAttributeOptionId = 0, bool? allowFiltering = null, bool? showOnProductPage = null);

        #endregion
    }
}
