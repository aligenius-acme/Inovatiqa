using System;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    public partial class PreferencesController : BaseAdminController
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContextService _workContextService;

        #endregion

        #region Ctor

        public PreferencesController(IGenericAttributeService genericAttributeService,
            IWorkContextService workContextService,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _genericAttributeService = genericAttributeService;
            _workContextService = workContextService;
        }

        #endregion

        #region Methods

        [HttpPost]
        public virtual IActionResult SavePreference(string name, bool value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            var customer = _workContextService.CurrentCustomer;

            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, name, value);

            return Json(new
            {
                Result = true
            });
        }

        #endregion
    }
}