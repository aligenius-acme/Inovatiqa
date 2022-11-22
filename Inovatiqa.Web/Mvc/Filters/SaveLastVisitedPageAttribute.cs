using System;
using System.Net;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Inovatiqa.Web.Mvc.Filters
{
    public sealed class SaveLastVisitedPageAttribute : TypeFilterAttribute
    {
        #region Ctor

        public SaveLastVisitedPageAttribute() : base(typeof(SaveLastVisitedPageFilter))
        {
        }

        #endregion

        #region Nested filter

        private class SaveLastVisitedPageFilter : IActionFilter
        {
            #region Fields
            private readonly IGenericAttributeService _genericAttributeService;
            private readonly IWebHelper _webHelper;
            private readonly IWorkContextService _workContextService;

            #endregion

            #region Ctor

            public SaveLastVisitedPageFilter(IGenericAttributeService genericAttributeService,
                IWebHelper webHelper,
                IWorkContextService workContextService)
            {
                _genericAttributeService = genericAttributeService;
                _webHelper = webHelper;
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

                if (!InovatiqaDefaults.StoreLastVisitedPage)
                    return;

                var pageUrl = _webHelper.GetThisPageUrl(true);
                if (string.IsNullOrEmpty(pageUrl))
                    return;

                var customer = _workContextService.CurrentCustomer;

                var previousPageUrl = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.LastVisitedPageAttribute, customer.Id);

                if (!pageUrl.Equals(previousPageUrl, StringComparison.InvariantCultureIgnoreCase))
                    _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.LastVisitedPageAttribute, pageUrl);

            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }

            #endregion
        }

        #endregion
    }
}