using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Customers.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Customers
{
    public partial class CustomerAttributeService : ICustomerAttributeService
    {
        #region Fields

        private readonly IRepository<CustomerAttribute> _customerAttributeRepository;
        private readonly IRepository<CustomerAttributeValue> _customerAttributeValueRepository;

        #endregion

        #region Ctor

        public CustomerAttributeService(IRepository<CustomerAttribute> customerAttributeRepository,
            IRepository<CustomerAttributeValue> customerAttributeValueRepository)
        {
            _customerAttributeRepository = customerAttributeRepository;
            _customerAttributeValueRepository = customerAttributeValueRepository;
        }

        #endregion

        #region Methods

        public virtual IList<CustomerAttribute> GetAllCustomerAttributes()
        {
            var query = from ca in _customerAttributeRepository.Query()
                orderby ca.DisplayOrder, ca.Id
                select ca;

            return query.ToList();
        }

        public virtual IList<CustomerAttributeValue> GetCustomerAttributeValues(int customerAttributeId)
        {
            var query = from cav in _customerAttributeValueRepository.Query()
                        orderby cav.DisplayOrder, cav.Id
                        where cav.CustomerAttributeId == customerAttributeId
                        select cav;
            var customerAttributeValues = query.ToList();

            return customerAttributeValues;
        }

        public virtual CustomerAttributeValue GetCustomerAttributeValueById(int customerAttributeValueId)
        {
            if (customerAttributeValueId == 0)
                return null;

            return _customerAttributeValueRepository.GetById(customerAttributeValueId);
        }

        public virtual CustomerAttribute GetCustomerAttributeById(int customerAttributeId)
        {
            if (customerAttributeId == 0)
                return null;

            return _customerAttributeRepository.GetById(customerAttributeId);
        }

        #endregion
    }
}