using System;
using System.Net;
using Inovatiqa.Core;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Inovatiqa.Web.Mvc.Filters
{
    public sealed class SaveLastActivityAttribute : TypeFilterAttribute
    {
        #region Ctor

        public SaveLastActivityAttribute() : base(typeof(SaveLastActivityFilter))
        {
        }
        
        #endregion

        #region Nested filter

        private class SaveLastActivityFilter : IActionFilter
        {
            #region Fields
            private readonly ICustomerService _customerService;
            private readonly IWorkContextService _workContextService;

            #endregion

            #region Ctor

            public SaveLastActivityFilter(ICustomerService customerService,
                IWorkContextService workContextService)
            {
                _customerService = customerService;
                _workContextService = workContextService;
            }

            #endregion

            #region Methods

            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (context == null)
                    throw new ArgumentNullException(nameof(context));

                if (context.HttpContext.Request == null)
                    return;

                if (!context.HttpContext.Request.Method.Equals(WebRequestMethods.Http.Get, StringComparison.InvariantCultureIgnoreCase))
                    return;

                var customer = _workContextService.CurrentCustomer;

                if (customer.LastActivityDateUtc.AddMinutes(InovatiqaDefaults.LastActivityMinutes) < DateTime.UtcNow)
                {
                    customer.LastActivityDateUtc = DateTime.UtcNow;
                    _customerService.UpdateCustomer(customer);
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