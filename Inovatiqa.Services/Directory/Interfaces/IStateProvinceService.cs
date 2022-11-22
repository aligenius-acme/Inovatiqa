using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Directory.Interfaces
{
    public partial interface IStateProvinceService
    {
        IList<StateProvince> GetStateProvincesByCountryId(int countryId, int languageId = 0, bool showHidden = false);

        StateProvince GetStateProvinceByAddress(Address address);

        StateProvince GetStateProvinceById(int stateProvinceId);

        StateProvince GetStateProvinceByAbbreviation(string abbreviation, int? countryId = null);
    }
}
