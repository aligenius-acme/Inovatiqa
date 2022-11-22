using Inovatiqa.Web.Areas.Admin.Models.Directory;
using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;

namespace Inovatiqa.Web.Areas.Admin.Models.Shipping
{
    public partial class ShippingMethodRestrictionModel : BaseInovatiqaModel
    {
        #region Ctor

        public ShippingMethodRestrictionModel()
        {
            AvailableShippingMethods = new List<ShippingMethodModel>();
            AvailableCountries = new List<CountryModel>();
            Restricted = new Dictionary<int, IDictionary<int, bool>>();
        }

        #endregion

        #region Properties

        public IList<ShippingMethodModel> AvailableShippingMethods { get; set; }

        public IList<CountryModel> AvailableCountries { get; set; }

        public IDictionary<int, IDictionary<int, bool>> Restricted { get; set; }

        #endregion
    }
}