using System.Collections.Generic;
using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Common.Interfaces
{
    public partial interface IAddressService
    {
        void DeleteAddress(Address address);

        int GetAddressTotalByCountryId(int countryId);

        int GetAddressTotalByStateProvinceId(int stateProvinceId);

        Address GetAddressById(int addressId);

        void InsertAddress(Address address);

        void UpdateAddress(Address address);

        bool IsAddressValid(Address address);

        Address FindAddress(List<Address> source, string firstName, string lastName, string phoneNumber, string email,
            string faxNumber, string company, string address1, string address2, string city, string county, int? stateProvinceId,
            string zipPostalCode, int? countryId, string customAttributes);

        Address CloneAddress(Address address, Customer customer, bool isBilling);
    }
}