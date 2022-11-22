using System.Globalization;
using Inovatiqa.Core.Interfaces;

namespace Inovatiqa.Core
{
    public partial class PriceFormatter : IPriceFormatter
    {
        #region Fields

        #endregion

        #region Ctor

        #endregion

        #region Utilities

        #endregion

        #region Methods

        public virtual string FormatPrice(decimal price)
        {
            return price.ToString("C", new CultureInfo("en-US"));
        }

        #endregion
    }
}