using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Directory.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Directory
{
    public partial class StateProvinceService : IStateProvinceService
    {
        #region Fields

        private readonly IRepository<StateProvince> _stateProvinceRepository;

        #endregion

        #region Ctor

        public StateProvinceService(IRepository<StateProvince> stateProvinceRepository)
        {
            _stateProvinceRepository = stateProvinceRepository;
        }

        #endregion

        #region Methods

        public virtual IList<StateProvince> GetStateProvincesByCountryId(int countryId, int languageId = 0, bool showHidden = false)
        {
            var query = from sp in _stateProvinceRepository.Query()
                        orderby sp.DisplayOrder, sp.Name
                        where sp.CountryId == countryId &&
                        (showHidden || sp.Published)
                        select sp;
            var stateProvinces = query.ToList();

            if (languageId > 0)
            {
                stateProvinces = stateProvinces
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .ToList();
            }

            return stateProvinces;
        }

        public virtual StateProvince GetStateProvinceByAddress(Address address)
        {
            return GetStateProvinceById(address?.StateProvinceId ?? 0);
        }

        public virtual StateProvince GetStateProvinceById(int stateProvinceId)
        {
            if (stateProvinceId == 0)
                return null;

            return _stateProvinceRepository.GetById(stateProvinceId);
        }

        public virtual StateProvince GetStateProvinceByAbbreviation(string abbreviation, int? countryId = null)
        {
            if (string.IsNullOrEmpty(abbreviation))
                return null;

            var query = _stateProvinceRepository.Query().Where(state => state.Abbreviation == abbreviation);

            if (countryId.HasValue)
                query = query.Where(state => state.CountryId == countryId);

            return query.FirstOrDefault();
        }

        #endregion
    }
}