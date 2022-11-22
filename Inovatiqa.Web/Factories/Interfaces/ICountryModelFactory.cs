using Inovatiqa.Web.Models.Directory;
using System.Collections.Generic;

namespace Inovatiqa.Web.Factories.Interfaces
{
    public partial interface ICountryModelFactory
    {
        IList<StateProvinceModel> GetStatesByCountryId(string countryId, bool addSelectStateItem);
    }
}
