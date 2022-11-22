using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Vendors
{
    public partial class VendorAttributeService : IVendorAttributeService
    {
        #region Fields

        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<VendorAttribute> _vendorAttributeRepository;
        private readonly IRepository<VendorAttributeValue> _vendorAttributeValueRepository;

        #endregion

        #region Ctor

        public VendorAttributeService(IEventPublisher eventPublisher,
            IRepository<VendorAttribute> vendorAttributeRepository,
            IRepository<VendorAttributeValue> vendorAttributeValueRepository)
        {
            _eventPublisher = eventPublisher;
            _vendorAttributeRepository = vendorAttributeRepository;
            _vendorAttributeValueRepository = vendorAttributeValueRepository;
        }

        #endregion

        #region Methods

        #region Vendor attributes

        public virtual IList<VendorAttribute> GetAllVendorAttributes()
        {
            return _vendorAttributeRepository.Query()
                .OrderBy(vendorAttribute => vendorAttribute.DisplayOrder).ThenBy(vendorAttribute => vendorAttribute.Id)
                .ToList();
        }

        public virtual VendorAttribute GetVendorAttributeById(int vendorAttributeId)
        {
            if (vendorAttributeId == 0)
                return null;

            return _vendorAttributeRepository.GetById(vendorAttributeId);
        }

        public virtual void InsertVendorAttribute(VendorAttribute vendorAttribute)
        {
            if (vendorAttribute == null)
                throw new ArgumentNullException(nameof(vendorAttribute));

            _vendorAttributeRepository.Insert(vendorAttribute);

            //_eventPublisher.EntityInserted(vendorAttribute);
        }

        public virtual void UpdateVendorAttribute(VendorAttribute vendorAttribute)
        {
            if (vendorAttribute == null)
                throw new ArgumentNullException(nameof(vendorAttribute));

            _vendorAttributeRepository.Update(vendorAttribute);

            //_eventPublisher.EntityUpdated(vendorAttribute);
        }

        public virtual void DeleteVendorAttribute(VendorAttribute vendorAttribute)
        {
            if (vendorAttribute == null)
                throw new ArgumentNullException(nameof(vendorAttribute));

            _vendorAttributeRepository.Delete(vendorAttribute);

            //_eventPublisher.EntityDeleted(vendorAttribute);
        }

        #endregion

        #region Vendor attribute values

        public virtual IList<VendorAttributeValue> GetVendorAttributeValues(int vendorAttributeId)
        {
            return _vendorAttributeValueRepository.Query()
                .Where(vendorAttributeValue => vendorAttributeValue.VendorAttributeId == vendorAttributeId)
                .OrderBy(vendorAttributeValue => vendorAttributeValue.DisplayOrder)
                .ThenBy(vendorAttributeValue => vendorAttributeValue.Id)
                .ToList();
        }

        public virtual VendorAttributeValue GetVendorAttributeValueById(int vendorAttributeValueId)
        {
            if (vendorAttributeValueId == 0)
                return null;

            return _vendorAttributeValueRepository.GetById(vendorAttributeValueId);
        }

        public virtual void InsertVendorAttributeValue(VendorAttributeValue vendorAttributeValue)
        {
            if (vendorAttributeValue == null)
                throw new ArgumentNullException(nameof(vendorAttributeValue));

            _vendorAttributeValueRepository.Insert(vendorAttributeValue);

            //_eventPublisher.EntityInserted(vendorAttributeValue);
        }

        public virtual void UpdateVendorAttributeValue(VendorAttributeValue vendorAttributeValue)
        {
            if (vendorAttributeValue == null)
                throw new ArgumentNullException(nameof(vendorAttributeValue));

            _vendorAttributeValueRepository.Update(vendorAttributeValue);

            //_eventPublisher.EntityUpdated(vendorAttributeValue);
        }

        public virtual void DeleteVendorAttributeValue(VendorAttributeValue vendorAttributeValue)
        {
            if (vendorAttributeValue == null)
                throw new ArgumentNullException(nameof(vendorAttributeValue));

            _vendorAttributeValueRepository.Delete(vendorAttributeValue);

            //_eventPublisher.EntityDeleted(vendorAttributeValue);
        }

        #endregion

        #endregion
    }
}