using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Common.Interfaces
{
    public partial interface IAddressAttributeService
    {
        void DeleteAddressAttribute(AddressAttribute addressAttribute);

        IList<AddressAttribute> GetAllAddressAttributes();

        AddressAttribute GetAddressAttributeById(int addressAttributeId);

        void InsertAddressAttribute(AddressAttribute addressAttribute);

        void UpdateAddressAttribute(AddressAttribute addressAttribute);

        void DeleteAddressAttributeValue(AddressAttributeValue addressAttributeValue);

        IList<AddressAttributeValue> GetAddressAttributeValues(int addressAttributeId);

        AddressAttributeValue GetAddressAttributeValueById(int addressAttributeValueId);

        void InsertAddressAttributeValue(AddressAttributeValue addressAttributeValue);

        void UpdateAddressAttributeValue(AddressAttributeValue addressAttributeValue);
    }
}
