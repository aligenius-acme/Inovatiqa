using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Vendors.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Vendors
{
    public partial class VendorService : IVendorService
    {
        #region Fields

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<VendorNote> _vendorNoteRepository;

        #endregion

        #region Ctor

        public VendorService(IRepository<Product> productRepository,
            IRepository<Vendor> vendorRepository,
            IRepository<VendorNote> vendorNoteRepository)
        {
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;
            _vendorNoteRepository = vendorNoteRepository;
        }

        #endregion

        #region Methods

        public virtual Vendor GetVendorById(int vendorId)
        {
            if (vendorId == 0)
                return null;

            return _vendorRepository.GetById(vendorId);
        }

        public virtual Vendor GetVendorByProductId(int productId)
        {
            if (productId == 0)
                return null;

            return (from v in _vendorRepository.Query()
                    join p in _productRepository.Query() on v.Id equals p.VendorId
                    select v).FirstOrDefault();
        }

        public virtual IPagedList<Vendor> GetAllVendors(string name = "", string email = "", int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = _vendorRepository.Query();
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(v => v.Name.Contains(name));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(v => v.Email.Contains(email));

            if (!showHidden)
                query = query.Where(v => v.Active);

            query = query.Where(v => !v.Deleted);
            query = query.OrderBy(v => v.DisplayOrder).ThenBy(v => v.Name).ThenBy(v => v.Email);

            var vendors = new PagedList<Vendor>(query, pageIndex, pageSize);
            return vendors;
        }

        public virtual IList<Vendor> GetVendorsByProductIds(int[] productIds)
        {
            if (productIds is null)
                throw new ArgumentNullException(nameof(productIds));

            //return (from v in _vendorRepository.Query()
            //        join p in _productRepository.Query() on v.Id equals p.VendorId
            //        where productIds.Contains(p.Id) && !v.Deleted && v.Active
            //        group v by p.Id into v
            //        select v.First()).ToList();

            return (from v in _vendorRepository.Query()
                    join p in _productRepository.Query() on v.Id equals p.VendorId
                    where productIds.Contains(p.Id) && !v.Deleted && v.Active
                    select v).ToList();
        }

        public virtual void InsertVendor(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            _vendorRepository.Insert(vendor);

            //event notification
            //_eventPublisher.EntityInserted(vendor);
        }

        public virtual void UpdateVendor(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            _vendorRepository.Update(vendor);

            //event notification
            //_eventPublisher.EntityUpdated(vendor);
        }

        public virtual void DeleteVendor(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            vendor.Deleted = true;
            UpdateVendor(vendor);

            //event notification
            //_eventPublisher.EntityDeleted(vendor);
        }

        #endregion
    }
}