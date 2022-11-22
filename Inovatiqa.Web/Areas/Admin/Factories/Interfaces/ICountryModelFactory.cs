using Inovatiqa.Database.Models;
using Inovatiqa.Web.Areas.Admin.Models.Directory;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial interface ICountryModelFactory
    {
        CountrySearchModel PrepareCountrySearchModel(CountrySearchModel searchModel);

        CountryListModel PrepareCountryListModel(CountrySearchModel searchModel);

        CountryModel PrepareCountryModel(CountryModel model, Country country, bool excludeProperties = false);

        StateProvinceListModel PrepareStateProvinceListModel(StateProvinceSearchModel searchModel, Country country);

        StateProvinceModel PrepareStateProvinceModel(StateProvinceModel model,
            Country country, StateProvince state, bool excludeProperties = false);
    }
}