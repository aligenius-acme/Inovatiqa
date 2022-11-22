using System;
using System.Net;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Inovatiqa.Web.Mvc.Filters
{
    public sealed class HttpsRequirementAttribute : TypeFilterAttribute
    {
        #region Ctor

        public HttpsRequirementAttribute() : base(typeof(HttpsRequirementFilter))
        {
        }

        #endregion

        #region Nested filter

        private class HttpsRequirementFilter : IAuthorizationFilter
        {
            #region Fields

            private readonly IWebHelper _webHelper;

            #endregion

            #region Ctor

            public HttpsRequirementFilter(IWebHelper webHelper)
            {
                _webHelper = webHelper;
            }

            #endregion

            #region Utilities

            protected void RedirectRequest(AuthorizationFilterContext filterContext, bool useSsl)
            {
                var currentConnectionSecured = _webHelper.IsCurrentConnectionSecured();

                if (useSsl && !currentConnectionSecured)
                    filterContext.Result = new RedirectResult(_webHelper.GetThisPageUrl(true, true), true);

                if (!useSsl && currentConnectionSecured)
                    filterContext.Result = new RedirectResult(_webHelper.GetThisPageUrl(true, false), true);
            }

            #endregion

            #region Methods

            public void OnAuthorization(AuthorizationFilterContext filterContext)
            {
                if (filterContext == null)
                    throw new ArgumentNullException(nameof(filterContext));

                if (filterContext.HttpContext.Request == null)
                    return;

                if (!filterContext.HttpContext.Request.Method.Equals(WebRequestMethods.Http.Get, StringComparison.InvariantCultureIgnoreCase))
                    return;


                RedirectRequest(filterContext, InovatiqaDefaults.SslEnabled);
            }

            #endregion
        }

        #endregion
    }
}