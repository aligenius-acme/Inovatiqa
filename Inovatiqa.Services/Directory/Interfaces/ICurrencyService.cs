using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Directory.Interfaces
{
    public partial interface ICurrencyService
    {
        #region Currency

        void DeleteCurrency(Currency currency);

        Currency GetCurrencyById(int currencyId);

        Currency GetCurrencyByCode(string currencyCode);

        IList<Currency> GetAllCurrencies(bool showHidden = false, int storeId = 0);

        void InsertCurrency(Currency currency);

        void UpdateCurrency(Currency currency);

        #endregion

        #region Conversions


        #endregion
    }
}