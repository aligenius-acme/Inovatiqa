using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Customers.Interfaces
{
    public partial interface ICustomerAttributeService
    {
        IList<CustomerAttribute> GetAllCustomerAttributes();

        IList<CustomerAttributeValue> GetCustomerAttributeValues(int customerAttributeId);

        CustomerAttributeValue GetCustomerAttributeValueById(int customerAttributeValueId);

        CustomerAttribute GetCustomerAttributeById(int customerAttributeId);
    }
}
