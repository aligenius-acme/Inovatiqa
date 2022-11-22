using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Catalog
{
    public partial class ProductAttributeService : IProductAttributeService
    {
        #region Fields

        private readonly IRepository<PredefinedProductAttributeValue> _predefinedProductAttributeValueRepository;
        private readonly IRepository<ProductAttribute> _productAttributeRepository;
        private readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
        private readonly IRepository<ProductProductAttributeMapping> _productAttributeMappingRepository;
        private readonly IRepository<ProductAttributeValue> _productAttributeValueRepository;

        #endregion

        #region Ctor

        public ProductAttributeService(IRepository<PredefinedProductAttributeValue> predefinedProductAttributeValueRepository,
            IRepository<ProductAttribute> productAttributeRepository,
            IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
            IRepository<ProductProductAttributeMapping> productAttributeMappingRepository,
            IRepository<ProductAttributeValue> productAttributeValueRepository)
        {
            _predefinedProductAttributeValueRepository = predefinedProductAttributeValueRepository;
            _productAttributeRepository = productAttributeRepository;
            _productAttributeCombinationRepository = productAttributeCombinationRepository;
            _productAttributeMappingRepository = productAttributeMappingRepository;
            _productAttributeValueRepository = productAttributeValueRepository;
        }

        #endregion

        #region Methods

        #region Product attributes

        public virtual IPagedList<ProductAttribute> GetAllProductAttributes(int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = from pa in _productAttributeRepository.Query()
                        orderby pa.Name
                        select pa;

            var productAttributes = new PagedList<ProductAttribute>(query, pageIndex, pageSize);

            return productAttributes;
        }

        public virtual ProductAttribute GetProductAttributeById(int productAttributeId)
        {
            if (productAttributeId == 0)
                return null;

            return _productAttributeRepository.GetById(productAttributeId);
        }

        #endregion

        #region Product attributes mappings

        public virtual ProductProductAttributeMapping GetProductAttributeMappingById(int productAttributeMappingId)
        {
            if (productAttributeMappingId == 0)
                return null;

            return _productAttributeMappingRepository.GetById(productAttributeMappingId);
        }

        public virtual IList<ProductProductAttributeMapping> GetProductAttributeMappingsByProductId(int productId)
        {
            var query = from pam in _productAttributeMappingRepository.Query()
                        orderby pam.DisplayOrder, pam.Id
                        where pam.ProductId == productId
                        select pam;

            var attributes = query.ToList() ?? new List<ProductProductAttributeMapping>();

            return attributes;
        }

        #endregion

        #region Product attribute values

        public virtual IList<ProductAttributeValue> GetProductAttributeValues(int productAttributeMappingId)
        {
            var query = from pav in _productAttributeValueRepository.Query()
                        orderby pav.DisplayOrder, pav.Id
                        where pav.ProductAttributeMappingId == productAttributeMappingId
                        select pav;
            var productAttributeValues = query.ToList();

            return productAttributeValues;
        }

        public virtual ProductAttributeValue GetProductAttributeValueById(int productAttributeValueId)
        {
            if (productAttributeValueId == 0)
                return null;

            return _productAttributeValueRepository.GetById(productAttributeValueId);
        }

        #endregion

        #region Predefined product attribute values


        #endregion

        #region Product attribute combinations

        public virtual void DeleteProductAttributeCombination(ProductAttributeCombination combination)
        {
            if (combination == null)
                throw new ArgumentNullException(nameof(combination));

            _productAttributeCombinationRepository.Delete(combination);

            //event notification
            //_eventPublisher.EntityDeleted(combination);
        }

        public virtual ProductAttributeCombination GetProductAttributeCombinationById(int productAttributeCombinationId)
        {
            if (productAttributeCombinationId == 0)
                return null;

            return _productAttributeCombinationRepository.GetById(productAttributeCombinationId);
        }

        public virtual IList<ProductAttributeCombination> GetAllProductAttributeCombinations(int productId)
        {
            if (productId == 0)
                return new List<ProductAttributeCombination>();

            var query = from c in _productAttributeCombinationRepository.Query()
                        orderby c.Id
                        where c.ProductId == productId
                        select c;
            var combinations = query.ToList();

            return combinations;

        }

        public virtual void UpdateProductAttributeCombination(ProductAttributeCombination combination)
        {
            if (combination == null)
                throw new ArgumentNullException(nameof(combination));

            _productAttributeCombinationRepository.Update(combination);

            //////event notification
            ////_eventPublisher.EntityUpdated(combination);
        }

        public virtual ProductAttributeCombination GetProductAttributeCombinationBySku(string sku)
        {
            if (string.IsNullOrEmpty(sku))
                return null;

            sku = sku.Trim();

            var query = from pac in _productAttributeCombinationRepository.Query()
                        orderby pac.Id
                        where pac.Sku == sku
                        select pac;
            var combination = query.FirstOrDefault();
            return combination;
        }

        #endregion

        #endregion
    }
}