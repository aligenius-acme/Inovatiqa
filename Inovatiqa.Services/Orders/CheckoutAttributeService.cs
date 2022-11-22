using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Orders.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Orders
{
    public partial class CheckoutAttributeService : ICheckoutAttributeService
    {
        #region Fields

        private readonly IRepository<CheckoutAttribute> _checkoutAttributeRepository;
        private readonly IRepository<CheckoutAttributeValue> _checkoutAttributeValueRepository;

        #endregion

        #region Ctor

        public CheckoutAttributeService(IRepository<CheckoutAttribute> checkoutAttributeRepository,
            IRepository<CheckoutAttributeValue> checkoutAttributeValueRepository)
        {
            _checkoutAttributeRepository = checkoutAttributeRepository;
            _checkoutAttributeValueRepository = checkoutAttributeValueRepository;
        }

        #endregion

        #region Methods

        #region Checkout attributes

        public virtual CheckoutAttribute GetCheckoutAttributeById(int checkoutAttributeId)
        {
            if (checkoutAttributeId == 0)
                return null;

            return _checkoutAttributeRepository.GetById(checkoutAttributeId);
        }

        public virtual IList<CheckoutAttribute> GetAllCheckoutAttributes(int storeId = 0, bool excludeShippableAttributes = false)
        {
            var query = from ca in _checkoutAttributeRepository.Query()
                        orderby ca.DisplayOrder, ca.Id
                        select ca;

            var checkoutAttributes = query.ToList();

            if (excludeShippableAttributes)
            {
                checkoutAttributes = checkoutAttributes.Where(x => !x.ShippableProductRequired).ToList();
            }

            return checkoutAttributes;
        }

        public virtual IList<CheckoutAttributeValue> GetCheckoutAttributeValues(int checkoutAttributeId)
        {
            var query = from cav in _checkoutAttributeValueRepository.Query()
                        orderby cav.DisplayOrder, cav.Id
                        where cav.CheckoutAttributeId == checkoutAttributeId
                        select cav;
            var checkoutAttributeValues = query.ToList();

            return checkoutAttributeValues;
        }

        #endregion

        #region Checkout attribute values

        public virtual CheckoutAttributeValue GetCheckoutAttributeValueById(int checkoutAttributeValueId)
        {
            if (checkoutAttributeValueId == 0)
                return null;

            return _checkoutAttributeValueRepository.GetById(checkoutAttributeValueId);
        }

        #endregion

        #endregion
    }
}