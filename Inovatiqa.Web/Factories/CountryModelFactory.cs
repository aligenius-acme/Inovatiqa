using Inovatiqa.Core;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Directory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Web.Factories
{
    public partial class CountryModelFactory : ICountryModelFactory
    {
        #region Fields

        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IWorkContextService _workContextService;

        #endregion

        #region Ctor

        public CountryModelFactory(ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IWorkContextService workContextService)
        {
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _workContextService = workContextService;
        }

        #endregion

        #region Methods

        public virtual IList<StateProvinceModel> GetStatesByCountryId(string countryId, bool addSelectStateItem)
        {
            if (string.IsNullOrEmpty(countryId))
                throw new ArgumentNullException(nameof(countryId));

            var country = _countryService.GetCountryById(Convert.ToInt32(countryId));
            var states = _stateProvinceService
                .GetStateProvincesByCountryId(country?.Id ?? 0, InovatiqaDefaults.LanguageId)
                .ToList();
            var result = new List<StateProvinceModel>();
            foreach (var state in states)
                result.Add(new StateProvinceModel
                {
                    id = state.Id,
                    name = state.Name
                });

            if (country == null)
            {
                if (addSelectStateItem)
                {
                    result.Insert(0, new StateProvinceModel
                    {
                        id = 0,
                        name = "Select state"
                    });
                }
                else
                {
                    result.Insert(0, new StateProvinceModel
                    {
                        id = 0,
                        name = "Other"
                    }); ;
                }
            }
            else
            {
                if (!result.Any())
                {
                    result.Insert(0, new StateProvinceModel
                    {
                        id = 0,
                        name = "Other"
                    });
                }
                else
                {
                    if (addSelectStateItem)
                    {
                        result.Insert(0, new StateProvinceModel
                        {
                            id = 0,
                            name = "Select state"
                        });
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
