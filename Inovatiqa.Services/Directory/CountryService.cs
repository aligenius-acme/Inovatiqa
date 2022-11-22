using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Directory.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Directory
{
    public partial class CountryService : ICountryService
    {
        #region Fields

        private readonly IRepository<Country> _countryRepository;

        #endregion

        #region Ctor

        public CountryService(IRepository<Country> countryRepository)
        {
            _countryRepository = countryRepository;
        }

        #endregion

        #region Methods

        public virtual IList<Country> GetAllCountriesForBilling(int languageId = 0, bool showHidden = false)
        {
            return GetAllCountries(languageId, showHidden).Where(c => c.AllowsBilling).ToList();
        }

        public virtual IList<Country> GetAllCountries(int languageId = 0, bool showHidden = false)
        {
            var query = _countryRepository.Query();
            if (!showHidden)
                query = query.Where(c => c.Published);
            query = query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name);

            var countries = query.ToList();

            if (languageId > 0)
            {
                countries = countries
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .ToList();
            }

            return countries;
        }

        public virtual Country GetCountryById(int countryId)
        {
            if (countryId == 0)
                return null;

            return _countryRepository.GetById(countryId);
        }

        public virtual Country GetCountryByAddress(Address address)
        {
            return GetCountryById(address?.CountryId ?? 0);
        }

        public virtual Country GetCountryByTwoLetterIsoCode(string twoLetterIsoCode)
        {
            if (string.IsNullOrEmpty(twoLetterIsoCode))
                return null;

            var query = from c in _countryRepository.Query()
                        where c.TwoLetterIsoCode == twoLetterIsoCode
                        select c;

            return query.FirstOrDefault();
        }

        public virtual IList<Country> GetAllCountriesForShipping(int languageId = 0, bool showHidden = false)
        {
            return GetAllCountries(languageId, showHidden).Where(c => c.AllowsShipping).ToList();
        }

        #endregion
    }
}