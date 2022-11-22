using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Directory.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Directory
{
    public partial class CurrencyService : ICurrencyService
    {
        #region Fields

        private readonly IRepository<Currency> _currencyRepository;


        #endregion

        #region Ctor

        public CurrencyService(IRepository<Currency> currencyRepository)
        {
            _currencyRepository = currencyRepository;
        }

        #endregion

        #region Methods

        #region Currency

        public virtual void DeleteCurrency(Currency currency)
        {
            if (currency == null)
                throw new ArgumentNullException(nameof(currency));

            _currencyRepository.Delete(currency);

            //_eventPublisher.EntityDeleted(currency);
        }

        public virtual Currency GetCurrencyById(int currencyId)
        {
            if (currencyId == 0)
                return null;
            
            return _currencyRepository.GetById(currencyId);
        }

        public virtual Currency GetCurrencyByCode(string currencyCode)
        {
            if (string.IsNullOrEmpty(currencyCode))
                return null;
            return GetAllCurrencies(true)
                .FirstOrDefault(c => c.CurrencyCode.ToLower() == currencyCode.ToLower());
        }

        public virtual IList<Currency> GetAllCurrencies(bool showHidden = false, int storeId = 0)
        {
            var query = _currencyRepository.Query();

            if (!showHidden)
                query = query.Where(c => c.Published);

            query = query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id);

            var currencies = query.ToList();

            return currencies;
        }

        public virtual void InsertCurrency(Currency currency)
        {
            if (currency == null)
                throw new ArgumentNullException(nameof(currency));

            _currencyRepository.Insert(currency);

            //_eventPublisher.EntityInserted(currency);
        }

        public virtual void UpdateCurrency(Currency currency)
        {
            if (currency == null)
                throw new ArgumentNullException(nameof(currency));

            _currencyRepository.Update(currency);

            //_eventPublisher.EntityUpdated(currency);
        }

        #endregion

        #region Conversions


        #endregion

        #endregion
    }
}