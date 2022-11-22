using System;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Inovatiqa.Web.Mvc.Filters
{
    public sealed class ValidatePasswordAttribute : TypeFilterAttribute
    {
        #region Ctor

        public ValidatePasswordAttribute() : base(typeof(ValidatePasswordFilter))
        {
        }

        #endregion

        #region Nested filter

        private class ValidatePasswordFilter : IActionFilter
        {
            #region Fields

            private readonly ICustomerService _customerService;
            private readonly IUrlHelperFactory _urlHelperFactory;
            private readonly IWorkContextService _workContextService;

            #endregion

            #region Ctor

            public ValidatePasswordFilter(ICustomerService customerService,
                IUrlHelperFactory urlHelperFactory,
                IWorkContextService workContext)
            {
                _customerService = customerService;
                _urlHelperFactory = urlHelperFactory;
                _workContextService = workContext;
            }

            #endregion

            #region Methods

            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (context == null)
                    throw new ArgumentNullException(nameof(context));

                if (context.HttpContext.Request == null)
                    return;

                var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var actionName = actionDescriptor?.ActionName;
                var controllerName = actionDescriptor?.ControllerName;

                if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
                    return;

                if (!(controllerName.Equals("Customer", StringComparison.InvariantCultureIgnoreCase) &&
                    actionName.Equals("ChangePassword", StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (_customerService.PasswordIsExpired(_workContextService.CurrentCustomer))
                    {
                        var changePasswordUrl = _urlHelperFactory.GetUrlHelper(context).RouteUrl("CustomerChangePassword");
                        context.Result = new RedirectResult(changePasswordUrl);
                    }
                }
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }

            #endregion
        }

        #endregion
    }
}