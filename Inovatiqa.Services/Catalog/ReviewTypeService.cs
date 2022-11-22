using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Catalog
{
    public partial class ReviewTypeService : IReviewTypeService
    {
        #region Fields

        private readonly IRepository<ProductReviewReviewTypeMapping> _productReviewReviewTypeMappingRepository;
        private readonly IRepository<ReviewType> _reviewTypeRepository;

        #endregion

        #region Ctor

        public ReviewTypeService(IRepository<ProductReviewReviewTypeMapping> productReviewReviewTypeMappingRepository,
            IRepository<ReviewType> reviewTypeRepository)
        {
            _productReviewReviewTypeMappingRepository = productReviewReviewTypeMappingRepository;
            _reviewTypeRepository = reviewTypeRepository;
        }

        #endregion

        #region Methods

        #region Review type

        public virtual ReviewType GetReviewTypeById(int reviewTypeId)
        {
            if (reviewTypeId == 0)
                return null;

            return _reviewTypeRepository.GetById(reviewTypeId);
        }

        public virtual IList<ReviewType> GetAllReviewTypes()
        {
            return _reviewTypeRepository.Query()
                .OrderBy(reviewType => reviewType.DisplayOrder).ThenBy(reviewType => reviewType.Id)
                .ToList();
        }

        #endregion

        #region Product review type mapping

        public IList<ProductReviewReviewTypeMapping> GetProductReviewReviewTypeMappingsByProductReviewId(
            int productReviewId)
        {
            var query = from pam in _productReviewReviewTypeMappingRepository.Query()
                        orderby pam.Id
                        where pam.ProductReviewId == productReviewId
                        select pam;
            var productReviewReviewTypeMappings = query.ToList();

            return productReviewReviewTypeMappings;
        }

        public virtual void InsertProductReviewReviewTypeMappings(ProductReviewReviewTypeMapping productReviewReviewType)
        {
            if (productReviewReviewType == null)
                throw new ArgumentNullException(nameof(productReviewReviewType));

            _productReviewReviewTypeMappingRepository.Insert(productReviewReviewType);

            //_eventPublisher.EntityInserted(productReviewReviewType);
        }

        #endregion

        #endregion
    }
}