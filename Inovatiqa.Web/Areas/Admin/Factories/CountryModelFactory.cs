using Inovatiqa.Database.Models;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Web.Areas.Admin.Models.Directory;
using Inovatiqa.Web.Extensions;
using Inovatiqa.Web.Framework.Factories.Interfaces;
using System;
using System.Linq;

namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial class CountryModelFactory : ICountryModelFactory
    {
        #region Fields

        private readonly ICountryService _countryService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IStateProvinceService _stateProvinceService;

        #endregion

        #region Ctor

        public CountryModelFactory(ICountryService countryService,
            ILocalizedModelFactory localizedModelFactory,
            IStateProvinceService stateProvinceService)
        {
            _countryService = countryService;
            _localizedModelFactory = localizedModelFactory;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Utilities

        protected virtual StateProvinceSearchModel PrepareStateProvinceSearchModel(StateProvinceSearchModel searchModel, Country country)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (country == null)
                throw new ArgumentNullException(nameof(country));

            searchModel.CountryId = country.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }
        
        #endregion

        #region Methods

        public virtual CountrySearchModel PrepareCountrySearchModel(CountrySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual CountryListModel PrepareCountryListModel(CountrySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var countries = _countryService.GetAllCountries(showHidden: true).ToPagedList(searchModel);

            var model = new CountryListModel().PrepareToGrid(searchModel, countries, () =>
            {
                return countries.Select(country =>
                {
                    var countryModel = country.ToCountryModel<CountryModel>();
                    countryModel.NumberOfStates = _stateProvinceService.GetStateProvincesByCountryId(country.Id)?.Count ?? 0;

                    return countryModel;
                });
            });

            return model;
        }

        public virtual CountryModel PrepareCountryModel(CountryModel model, Country country, bool excludeProperties = false)
        {
            Action<CountryLocalizedModel, int> localizedModelConfiguration = null;

            if (country != null)
            {
                if (model == null)
                {
                    model = country.ToCountryModel<CountryModel>();
                    model.NumberOfStates = _stateProvinceService.GetStateProvincesByCountryId(country.Id)?.Count ?? 0;
                }

                PrepareStateProvinceSearchModel(model.StateProvinceSearchModel, country);

                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = country.Name;
                };
            }

            if (country == null)
            {
                model.Published = true;
                model.AllowsBilling = true;
                model.AllowsShipping = true;
            }

            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            return model;
        }

        public virtual StateProvinceListModel PrepareStateProvinceListModel(StateProvinceSearchModel searchModel, Country country)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (country == null)
                throw new ArgumentNullException(nameof(country));

            var states = _stateProvinceService.GetStateProvincesByCountryId(country.Id, showHidden: true).ToPagedList(searchModel);

            var model = new StateProvinceListModel().PrepareToGrid(searchModel, states, ()=>
            {
                return states.Select(state => state.ToStateProvinceModel<StateProvinceModel>());
            });

            return model;
        }

        public virtual StateProvinceModel PrepareStateProvinceModel(StateProvinceModel model,
            Country country, StateProvince state, bool excludeProperties = false)
        {
            Action<StateProvinceLocalizedModel, int> localizedModelConfiguration = null;

            if (state != null)
            {
                model ??= state.ToStateProvinceModel<StateProvinceModel>();

                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = state.Name;
                };
            }

            model.CountryId = country.Id;

            if (state == null)
                model.Published = true;

            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            return model;
        }

        #endregion
    }
}