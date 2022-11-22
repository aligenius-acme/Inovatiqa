using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Common
{
    public partial class AddressAttributeService : IAddressAttributeService
    {
        #region Fields

        private readonly IRepository<AddressAttribute> _addressAttributeRepository;
        private readonly IRepository<AddressAttributeValue> _addressAttributeValueRepository;

        #endregion

        #region Ctor

        public AddressAttributeService(IRepository<AddressAttribute> addressAttributeRepository,
            IRepository<AddressAttributeValue> addressAttributeValueRepository)
        {
            _addressAttributeRepository = addressAttributeRepository;
            _addressAttributeValueRepository = addressAttributeValueRepository;
        }

        #endregion

        #region Methods

        public virtual void DeleteAddressAttribute(AddressAttribute addressAttribute)
        {
            if (addressAttribute == null)
                throw new ArgumentNullException(nameof(addressAttribute));

            _addressAttributeRepository.Delete(addressAttribute);

            //_eventPublisher.EntityDeleted(addressAttribute);
        }

        public virtual IList<AddressAttribute> GetAllAddressAttributes()
        {
            var query = from aa in _addressAttributeRepository.Query()
                orderby aa.DisplayOrder, aa.Id
                select aa;

            return query.ToList();
        }

        public virtual AddressAttribute GetAddressAttributeById(int addressAttributeId)
        {
            if (addressAttributeId == 0)
                return null;

            return _addressAttributeRepository.GetById(addressAttributeId);
        }

        public virtual void InsertAddressAttribute(AddressAttribute addressAttribute)
        {
            if (addressAttribute == null)
                throw new ArgumentNullException(nameof(addressAttribute));

            _addressAttributeRepository.Insert(addressAttribute);
            
            //_eventPublisher.EntityInserted(addressAttribute);
        }

        public virtual void UpdateAddressAttribute(AddressAttribute addressAttribute)
        {
            if (addressAttribute == null)
                throw new ArgumentNullException(nameof(addressAttribute));

            _addressAttributeRepository.Update(addressAttribute);
            
            //_eventPublisher.EntityUpdated(addressAttribute);
        }

        public virtual void DeleteAddressAttributeValue(AddressAttributeValue addressAttributeValue)
        {
            if (addressAttributeValue == null)
                throw new ArgumentNullException(nameof(addressAttributeValue));

            _addressAttributeValueRepository.Delete(addressAttributeValue);
            
            //_eventPublisher.EntityDeleted(addressAttributeValue);
        }

        public virtual IList<AddressAttributeValue> GetAddressAttributeValues(int addressAttributeId)
        {

            var query = from aav in _addressAttributeValueRepository.Query()
                orderby aav.DisplayOrder, aav.Id
                where aav.AddressAttributeId == addressAttributeId
                select aav;
            var addressAttributeValues = query.ToList();

            return addressAttributeValues;
        }

        public virtual AddressAttributeValue GetAddressAttributeValueById(int addressAttributeValueId)
        {
            if (addressAttributeValueId == 0)
                return null;

            return _addressAttributeValueRepository.GetById(addressAttributeValueId);
        }

        public virtual void InsertAddressAttributeValue(AddressAttributeValue addressAttributeValue)
        {
            if (addressAttributeValue == null)
                throw new ArgumentNullException(nameof(addressAttributeValue));

            _addressAttributeValueRepository.Insert(addressAttributeValue);
            
            //_eventPublisher.EntityInserted(addressAttributeValue);
        }

        public virtual void UpdateAddressAttributeValue(AddressAttributeValue addressAttributeValue)
        {
            if (addressAttributeValue == null)
                throw new ArgumentNullException(nameof(addressAttributeValue));

            _addressAttributeValueRepository.Update(addressAttributeValue);
            
            //._eventPublisher.EntityUpdated(addressAttributeValue);
        }

        #endregion
    }
}