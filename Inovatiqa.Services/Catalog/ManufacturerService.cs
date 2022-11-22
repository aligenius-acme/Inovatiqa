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
    public partial class ManufacturerService : IManufacturerService
    {
        #region Fields

        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<ProductManufacturerMapping> _productManufacturerRepository;
        private readonly IRepository<Product> _productRepository;

        #endregion

        #region Ctor

        public ManufacturerService(IRepository<Manufacturer> manufacturerRepository,
            IRepository<ProductManufacturerMapping> productManufacturerRepository,
            IRepository<Product> productRepository)
        {
            _manufacturerRepository = manufacturerRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _productRepository = productRepository;
        }

        #endregion

        #region Methods

        public virtual IList<ProductManufacturerMapping> GetProductManufacturersByProductId(int productId,
            bool showHidden = false)
        {
            if (productId == 0)
                return new List<ProductManufacturerMapping>();

            var query = from pm in _productManufacturerRepository.Query()
                        join m in _manufacturerRepository.Query() on pm.ManufacturerId equals m.Id
                        where pm.ProductId == productId &&
                              !m.Deleted &&
                              (showHidden || m.Published)
                        orderby pm.DisplayOrder, pm.Id
                        select pm;


            var productManufacturers = query.ToList();

            return productManufacturers;
        }

        public virtual Manufacturer GetManufacturerById(int manufacturerId)
        {
            if (manufacturerId == 0)
                return null;

            return _manufacturerRepository.GetById(manufacturerId);
        }

        public virtual IPagedList<Manufacturer> GetAllManufacturers(string manufacturerName = "",
            int storeId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false,
            bool? overridePublished = null)
        {
            var query = _manufacturerRepository.Query();
            if (!showHidden)
                query = query.Where(m => m.Published);
            if (!string.IsNullOrWhiteSpace(manufacturerName))
                query = query.Where(m => m.Name.Contains(manufacturerName));
            query = query.Where(m => !m.Deleted);
            if (overridePublished.HasValue)
                query = query.Where(m => m.Published == overridePublished.Value);
            query = query.OrderBy(m => m.DisplayOrder).ThenBy(m => m.Id);

            return new PagedList<Manufacturer>(query, pageIndex, pageSize);
        }
        //added by Hamza 
        public virtual List<Manufacturer> GetHomePageManufacturers()
        {
            var list = _manufacturerRepository.Query().Where(m => m.ShowOnHomepage).ToList();
            return list; 
            
        }

        public virtual List<Manufacturer> GetAllFeaturedManufacturers(int number)
        {
            var list = _manufacturerRepository.Query().Where(m => m.Published && !m.Deleted && m.ShowOnHomepage).Take(number).ToList();

            return list;
        }

        public virtual void UpdateManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
                throw new ArgumentNullException(nameof(manufacturer));

            _manufacturerRepository.Update(manufacturer);

            //event notification
            //_eventPublisher.EntityUpdated(manufacturer);
        }

        public virtual IPagedList<ProductManufacturerMapping> GetProductManufacturersByManufacturerId(int manufacturerId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            if (manufacturerId == 0)
                return new PagedList<ProductManufacturerMapping>(new List<ProductManufacturerMapping>(), pageIndex, pageSize);

            var query = from pm in _productManufacturerRepository.Query()
                        join p in _productRepository.Query() on pm.ProductId equals p.Id
                        where pm.ManufacturerId == manufacturerId &&
                              !p.Deleted &&
                              (showHidden || p.Published)
                        orderby pm.DisplayOrder, pm.Id
                        select pm;

            var productManufacturers = new PagedList<ProductManufacturerMapping>(query, pageIndex, pageSize);

            return productManufacturers;
        }

        public virtual ProductManufacturerMapping GetProductManufacturerById(int productManufacturerId)
        {
            if (productManufacturerId == 0)
                return null;

            return _productManufacturerRepository.GetById(productManufacturerId);
        }

        public virtual void UpdateProductManufacturer(ProductManufacturerMapping productManufacturer)
        {
            if (productManufacturer == null)
                throw new ArgumentNullException(nameof(productManufacturer));

            _productManufacturerRepository.Update(productManufacturer);

            //event notification
            //_eventPublisher.EntityUpdated(productManufacturer);
        }

        public virtual void DeleteProductManufacturer(ProductManufacturerMapping productManufacturer)
        {
            if (productManufacturer == null)
                throw new ArgumentNullException(nameof(productManufacturer));

            _productManufacturerRepository.Delete(productManufacturer);

            //event notification
            //_eventPublisher.EntityDeleted(productManufacturer);
        }

        public virtual ProductManufacturerMapping FindProductManufacturer(IList<ProductManufacturerMapping> source, int productId, int manufacturerId)
        {
            foreach (var productManufacturer in source)
                if (productManufacturer.ProductId == productId && productManufacturer.ManufacturerId == manufacturerId)
                    return productManufacturer;

            return null;
        }

        public virtual void InsertProductManufacturer(ProductManufacturerMapping productManufacturer)
        {
            if (productManufacturer == null)
                throw new ArgumentNullException(nameof(productManufacturer));

            _productManufacturerRepository.Insert(productManufacturer);

            //event notification
            //_eventPublisher.EntityInserted(productManufacturer);
        }

        #endregion
    }
}