using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Customers.Interfaces
{
    public partial interface ICustomerAttributeParserService
    {
        IList<CustomerAttributeValue> ParseCustomerAttributeValues(string attributesXml);

        IList<CustomerAttribute> ParseCustomerAttributes(string attributesXml);

        IList<string> ParseValues(string attributesXml, int customerAttributeId);

        string AddCustomerAttribute(string attributesXml, CustomerAttribute ca, string value);

        IList<string> GetAttributeWarnings(string attributesXml);
    }
}
