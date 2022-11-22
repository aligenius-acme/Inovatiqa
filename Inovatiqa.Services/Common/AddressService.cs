using Inovatiqa.Core;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Common
{
    public partial class AddressService : IAddressService
    {
        #region Fields
        private readonly ICountryService _countryService;
        private readonly IRepository<Address> _addressRepository;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressAttributeParserService _addressAttributeParserService;
        private readonly IAddressAttributeService _addressAttributeService;

        #endregion

        #region Ctor

        public AddressService(IAddressAttributeParserService addressAttributeParserService,
            IAddressAttributeService addressAttributeService,
            ICountryService countryService,
            IRepository<Address> addressRepository,
            IStateProvinceService stateProvinceService)
        {
            _addressAttributeParserService = addressAttributeParserService;
            _addressAttributeService = addressAttributeService;
            _countryService = countryService;
            _addressRepository = addressRepository;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Methods

        public virtual void DeleteAddress(Address address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            _addressRepository.Delete(address);
            
            //_eventPublisher.EntityDeleted(address);
        }

        public virtual int GetAddressTotalByCountryId(int countryId)
        {
            if (countryId == 0)
                return 0;

            var query = from a in _addressRepository.Query()
                        where a.CountryId == countryId
                        select a;

            return query.Count();
        }

        public virtual int GetAddressTotalByStateProvinceId(int stateProvinceId)
        {
            if (stateProvinceId == 0)
                return 0;

            var query = from a in _addressRepository.Query()
                        where a.StateProvinceId == stateProvinceId
                        select a;

            return query.Count();
        }

        public virtual Address GetAddressById(int addressId)
        {
            if (addressId == 0)
                return null;
            
            return _addressRepository.GetById(addressId);
        }

        public virtual void InsertAddress(Address address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            address.CreatedOnUtc = DateTime.UtcNow;

            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;

            _addressRepository.Insert(address);
            
            //_eventPublisher.EntityInserted(address);
        }

        public virtual void UpdateAddress(Address address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;

            _addressRepository.Update(address);
            
            //_eventPublisher.EntityUpdated(address);
        }

        public virtual bool IsAddressValid(Address address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            if (string.IsNullOrWhiteSpace(address.FirstName))
                return false;

            //if (string.IsNullOrWhiteSpace(address.LastName))
            //    return false;

            if (string.IsNullOrWhiteSpace(address.Email))
                return false;

            //if (InovatiqaDefaults.CompanyEnabled &&
            //    InovatiqaDefaults.CompanyRequired &&
            //    string.IsNullOrWhiteSpace(address.Company))
            //    return false;

            if (InovatiqaDefaults.StreetAddressEnabled &&
                InovatiqaDefaults.StreetAddressRequired &&
                string.IsNullOrWhiteSpace(address.Address1))
                return false;

            if (InovatiqaDefaults.StreetAddress2Enabled &&
                InovatiqaDefaults.StreetAddress2Required &&
                string.IsNullOrWhiteSpace(address.Address2))
                return false;

            if (InovatiqaDefaults.ZipPostalCodeEnabled &&
                InovatiqaDefaults.ZipPostalCodeRequired &&
                string.IsNullOrWhiteSpace(address.ZipPostalCode))
                return false;

            if (InovatiqaDefaults.CountryEnabled)
            {
                var country = _countryService.GetCountryByAddress(address);
                if (country == null)
                    return false;

                if (InovatiqaDefaults.StateProvinceEnabled)
                {
                    var states = _stateProvinceService.GetStateProvincesByCountryId(country.Id);
                    if (states.Any())
                    {
                        if (address.StateProvinceId == null || address.StateProvinceId.Value == 0)
                            return false;

                        var state = states.FirstOrDefault(x => x.Id == address.StateProvinceId.Value);
                        if (state == null)
                            return false;
                    }
                }
            }

            if (InovatiqaDefaults.CountyEnabled &&
                InovatiqaDefaults.CountyRequired &&
                string.IsNullOrWhiteSpace(address.County))
                return false;

            if (InovatiqaDefaults.CityEnabled &&
                InovatiqaDefaults.CityRequired &&
                string.IsNullOrWhiteSpace(address.City))
                return false;

            if (InovatiqaDefaults.PhoneEnabled &&
                InovatiqaDefaults.PhoneRequired &&
                string.IsNullOrWhiteSpace(address.PhoneNumber))
                return false;

            //if (InovatiqaDefaults.FaxEnabled &&
            //    InovatiqaDefaults.FaxRequired &&
            //    string.IsNullOrWhiteSpace(address.FaxNumber))
            //    return false;

            var requiredAttributes = _addressAttributeService.GetAllAddressAttributes().Where(x => x.IsRequired);

            foreach (var requiredAttribute in requiredAttributes)
            {
                var value = _addressAttributeParserService.ParseValues(address.CustomAttributes, requiredAttribute.Id);

                if (!value.Any() || string.IsNullOrEmpty(value[0]))
                    return false;
            }

            return true;
        }

        public virtual Address FindAddress(List<Address> source, string firstName, string lastName, string phoneNumber, string email,
            string faxNumber, string company, string address1, string address2, string city, string county, int? stateProvinceId,
            string zipPostalCode, int? countryId, string customAttributes)
        {
            return source.Find(a => ((string.IsNullOrEmpty(a.FirstName) && string.IsNullOrEmpty(firstName)) || a.FirstName == firstName) &&
            ((string.IsNullOrEmpty(a.LastName) && string.IsNullOrEmpty(lastName)) || a.LastName == lastName) &&
            ((string.IsNullOrEmpty(a.PhoneNumber) && string.IsNullOrEmpty(phoneNumber)) || a.PhoneNumber == phoneNumber) &&
            ((string.IsNullOrEmpty(a.Email) && string.IsNullOrEmpty(email)) || a.Email == email) &&
            ((string.IsNullOrEmpty(a.FaxNumber) && string.IsNullOrEmpty(faxNumber)) || a.FaxNumber == faxNumber) &&
            ((string.IsNullOrEmpty(a.Company) && string.IsNullOrEmpty(company)) || a.Company == company) &&
            ((string.IsNullOrEmpty(a.Address1) && string.IsNullOrEmpty(address1)) || a.Address1 == address1) &&
            ((string.IsNullOrEmpty(a.Address2) && string.IsNullOrEmpty(address2)) || a.Address2 == address2) &&
            ((string.IsNullOrEmpty(a.City) && string.IsNullOrEmpty(city)) || a.City == city) &&
            ((string.IsNullOrEmpty(a.County) && string.IsNullOrEmpty(county)) || a.County == county) &&
            ((a.StateProvinceId == null && (stateProvinceId == null || stateProvinceId == 0)) || (a.StateProvinceId != null && a.StateProvinceId == stateProvinceId)) &&
            ((string.IsNullOrEmpty(a.ZipPostalCode) && string.IsNullOrEmpty(zipPostalCode)) || a.ZipPostalCode == zipPostalCode) &&
            ((a.CountryId == null && countryId == null) || (a.CountryId != null && a.CountryId == countryId)) &&
            ((string.IsNullOrEmpty(a.CustomAttributes) && string.IsNullOrEmpty(customAttributes)) || a.CustomAttributes == customAttributes));
        }

        public virtual Address CloneAddress(Address address, Customer customer, bool isBilling)
        {
            var addr = new Address
            {
                FirstName = address.FirstName,
                LastName = address.LastName,
                Email = address.Email,
                Company = address.Company,
                CountryId = address.CountryId,
                StateProvinceId = address.StateProvinceId,
                County = address.County,
                City = address.City,
                Address1 = address.Address1,
                Address2 = address.Address2,
                ZipPostalCode = address.ZipPostalCode,
                PhoneNumber = address.PhoneNumber,
                FaxNumber = address.FaxNumber,
                CustomAttributes = address.CustomAttributes,
                CreatedOnUtc = address.CreatedOnUtc
            };
            if (isBilling)
                addr.ParentAddressId = customer.BillingAddressId;
            else
                addr.ParentAddressId = customer.ShippingAddressId;
            return addr;
        }

        #endregion
    }
}