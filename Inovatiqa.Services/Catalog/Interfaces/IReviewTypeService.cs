using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Catalog.Interfaces
{
    public partial interface IReviewTypeService
    {
        #region ReviewType

        ReviewType GetReviewTypeById(int reviewTypeId);

        IList<ReviewType> GetAllReviewTypes();

        #endregion

        #region ProductReviewReviewTypeMapping

        IList<ProductReviewReviewTypeMapping> GetProductReviewReviewTypeMappingsByProductReviewId(int productReviewId);

        void InsertProductReviewReviewTypeMappings(ProductReviewReviewTypeMapping productReviewReviewType);

        #endregion
    }
}
