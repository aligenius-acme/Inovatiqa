using System;
using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Directory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    public partial class CountryController : BaseAdminController
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly ICountryModelFactory _countryModelFactory;
        private readonly ICountryService _countryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IStateProvinceService _stateProvinceService;

        #endregion

        #region Ctor

        public CountryController(IAddressService addressService,
            ICountryModelFactory countryModelFactory,
            ICountryService countryService,
            ICustomerActivityService customerActivityService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IStateProvinceService stateProvinceService,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _addressService = addressService;
            _countryModelFactory = countryModelFactory;
            _countryService = countryService;
            _customerActivityService = customerActivityService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Utilities

        #endregion

        #region Countries

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

            var model = _countryModelFactory.PrepareCountrySearchModel(new CountrySearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult CountryList(CountrySearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedDataTablesJson();

            var model = _countryModelFactory.PrepareCountryListModel(searchModel);

            return Json(model);
        }
        #endregion

        #region States / provinces

        [HttpPost]
        public virtual IActionResult States(StateProvinceSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedDataTablesJson();

            var country = _countryService.GetCountryById(searchModel.CountryId)
                ?? throw new ArgumentException("No country found with the specified id");

            var model = _countryModelFactory.PrepareStateProvinceListModel(searchModel, country);

            return Json(model);
        }

        public virtual IActionResult GetStatesByCountryId(string countryId, bool? addSelectStateItem, bool? addAsterisk)
        {
            if (string.IsNullOrEmpty(countryId))
                throw new ArgumentNullException(nameof(countryId));

            var country = _countryService.GetCountryById(Convert.ToInt32(countryId));
            var states = country != null ? _stateProvinceService.GetStateProvincesByCountryId(country.Id, showHidden: true).ToList() : new List<StateProvince>();
            var result = (from s in states
                          select new { id = s.Id, name = s.Name }).ToList();
            if (addAsterisk.HasValue && addAsterisk.Value)
            {
                result.Insert(0, new { id = 0, name = "*" });
            }
            else
            {
                if (country == null)
                {
                    if (addSelectStateItem.HasValue && addSelectStateItem.Value)
                    {
                        result.Insert(0, new { id = 0, name = "Select state" });
                    }
                    else
                    {
                        result.Insert(0, new { id = 0, name = "Other" });
                    }
                }
                else
                {
                    if (!result.Any())
                    {
                        result.Insert(0, new { id = 0, name = "Other" });
                    }
                    else
                    {
                        if (addSelectStateItem.HasValue && addSelectStateItem.Value)
                        {
                            result.Insert(0, new { id = 0, name = "Select state" });
                        }
                    }
                }
            }

            return Json(result);
        }

        #endregion
    }
}