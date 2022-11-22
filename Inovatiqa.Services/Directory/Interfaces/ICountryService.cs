using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Directory.Interfaces
{
    public partial interface ICountryService
    {
        IList<Country> GetAllCountries(int languageId = 0, bool showHidden = false);

        Country GetCountryById(int countryId);

        Country GetCountryByAddress(Address address);

        Country GetCountryByTwoLetterIsoCode(string twoLetterIsoCode);

        IList<Country> GetAllCountriesForShipping(int languageId = 0, bool showHidden = false);

        IList<Country> GetAllCountriesForBilling(int languageId = 0, bool showHidden = false);
    }
}