using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Catalog
{
    public partial class SpecificationAttributeService : ISpecificationAttributeService
    {
        #region Fields

        private readonly IRepository<ProductSpecificationAttributeMapping> _productSpecificationAttributeRepository;
        private readonly IRepository<SpecificationAttributeOption> _specificationAttributeOptionRepository;
        private readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;

        #endregion

        #region Ctor

        public SpecificationAttributeService(
            IRepository<ProductSpecificationAttributeMapping> productSpecificationAttributeRepository,
            IRepository<SpecificationAttributeOption> specificationAttributeOptionRepository,
            IRepository<SpecificationAttribute> specificationAttributeRepository)
        {
            _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            _specificationAttributeOptionRepository = specificationAttributeOptionRepository;
            _specificationAttributeRepository = specificationAttributeRepository;
        }

        #endregion

        #region Methods

        #region Specification attribute

        public virtual SpecificationAttribute GetSpecificationAttributeById(int specificationAttributeId)
        {
            if (specificationAttributeId == 0)
                return null;

            return _specificationAttributeRepository.GetById(specificationAttributeId);
        }

        public virtual IList<SpecificationAttribute> GetSpecificationAttributesWithOptions()
        {
            var query = from sa in _specificationAttributeRepository.Query()
                        where _specificationAttributeOptionRepository.Query().Any(o => o.SpecificationAttributeId == sa.Id)
                        select sa;

            return query.ToList();
        }


        #endregion

        #region Specification attribute option

        public virtual SpecificationAttributeOption GetSpecificationAttributeOptionById(int specificationAttributeOptionId)
        {
            if (specificationAttributeOptionId == 0)
                return null;

            return _specificationAttributeOptionRepository.GetById(specificationAttributeOptionId);
        }

        #endregion

        #region Product specification attribute

        public virtual void DeleteProductSpecificationAttribute(ProductSpecificationAttributeMapping productSpecificationAttribute)
        {
            if (productSpecificationAttribute == null)
                throw new ArgumentNullException(nameof(productSpecificationAttribute));

            _productSpecificationAttributeRepository.Delete(productSpecificationAttribute);

            //event notification
            //_eventPublisher.EntityDeleted(productSpecificationAttribute);
        }

        public virtual ProductSpecificationAttributeMapping GetProductSpecificationAttributeById(int productSpecificationAttributeId)
        {
            if (productSpecificationAttributeId == 0)
                return null;

            return _productSpecificationAttributeRepository.GetById(productSpecificationAttributeId);
        }

        public virtual void InsertProductSpecificationAttribute(ProductSpecificationAttributeMapping productSpecificationAttribute)
        {
            if (productSpecificationAttribute == null)
                throw new ArgumentNullException(nameof(productSpecificationAttribute));

            _productSpecificationAttributeRepository.Insert(productSpecificationAttribute);

            //event notification
            //_eventPublisher.EntityInserted(productSpecificationAttribute);
        }


        public virtual IList<ProductSpecificationAttributeMapping> GetProductSpecificationAttributes(int productId = 0,
            int specificationAttributeOptionId = 0, bool? allowFiltering = null, bool? showOnProductPage = null)
        {
            var allowFilteringCacheStr = allowFiltering.HasValue ? allowFiltering.ToString() : "null";
            var showOnProductPageCacheStr = showOnProductPage.HasValue ? showOnProductPage.ToString() : "null";


            var query = _productSpecificationAttributeRepository.Query();
            if (productId > 0)
                query = query.Where(psa => psa.ProductId == productId);
            if (specificationAttributeOptionId > 0)
                query = query.Where(psa => psa.SpecificationAttributeOptionId == specificationAttributeOptionId);
            if (allowFiltering.HasValue)
                query = query.Where(psa => psa.AllowFiltering == allowFiltering.Value);
            if (showOnProductPage.HasValue)
                query = query.Where(psa => psa.ShowOnProductPage == showOnProductPage.Value);
            query = query.OrderBy(psa => psa.DisplayOrder).ThenBy(psa => psa.Id);

            var productSpecificationAttributes = query.ToList();

            return productSpecificationAttributes;
        }


        #endregion

        #endregion
    }
}