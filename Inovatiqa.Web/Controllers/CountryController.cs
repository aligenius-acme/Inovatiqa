using Inovatiqa.Web.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Controllers
{
    public partial class CountryController : BasePublicController
	{
        #region Fields

        private readonly ICountryModelFactory _countryModelFactory;
        
        #endregion
        
        #region Ctor

        public CountryController(ICountryModelFactory countryModelFactory,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _countryModelFactory = countryModelFactory;
		}
        
        #endregion
        
        #region States / provinces

        public virtual IActionResult GetStatesByCountryId(string countryId, bool addSelectStateItem)
        {
            var model = _countryModelFactory.GetStatesByCountryId(countryId, addSelectStateItem);
            return Json(model);
        }
        
        #endregion
    }
}