using System;
using System.Linq;
using System.Net;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Inovatiqa.Web.Framework.Mvc.Filters
{
    public sealed class SaveSelectedTabAttribute : TypeFilterAttribute
    {
        #region Fields

        private readonly bool _ignoreFilter;
        private readonly bool _persistForTheNextRequest;

        #endregion

        #region Ctor

        public SaveSelectedTabAttribute(bool ignore = false, bool persistForTheNextRequest = true) : base(typeof(SaveSelectedTabFilter))
        {
            _persistForTheNextRequest = persistForTheNextRequest;
            _ignoreFilter = ignore;
            Arguments = new object[] { ignore, persistForTheNextRequest };
        }

        #endregion

        #region Properties

        public bool IgnoreFilter => _ignoreFilter;

        public bool PersistForTheNextRequest => _persistForTheNextRequest;

        #endregion

        #region Nested filter

        private class SaveSelectedTabFilter : IActionFilter
        {
            #region Fields

            private readonly bool _ignoreFilter;
            private bool _persistForTheNextRequest;
            private readonly IWebHelper _webHelper;

            #endregion

            #region Ctor

            public SaveSelectedTabFilter(bool ignoreFilter, bool persistForTheNextRequest,
                IWebHelper webHelper)
            {
                _ignoreFilter = ignoreFilter;
                _persistForTheNextRequest = persistForTheNextRequest;
                _webHelper = webHelper;
            }

            #endregion

            #region Methods

            public void OnActionExecuting(ActionExecutingContext context)
            {
            }
            public void OnActionExecuted(ActionExecutedContext filterContext)
            {
                if (filterContext == null)
                    throw new ArgumentNullException(nameof(filterContext));

                if (filterContext.HttpContext.Request == null)
                    return;

                if (!filterContext.HttpContext.Request.Method.Equals(WebRequestMethods.Http.Post, StringComparison.InvariantCultureIgnoreCase))
                    return;
                if (_webHelper.IsAjaxRequest(filterContext.HttpContext.Request))
                    return;

                var actionFilter = filterContext.ActionDescriptor.FilterDescriptors
                    .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                    .Select(filterDescriptor => filterDescriptor.Filter).OfType<SaveSelectedTabAttribute>().FirstOrDefault();

                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                    return;

                var persistForTheNextRequest = actionFilter?.PersistForTheNextRequest ?? _persistForTheNextRequest;

                var controller = filterContext.Controller as BaseController;
                if (controller != null)
                    controller.SaveSelectedTabName(persistForTheNextRequest: persistForTheNextRequest);
            }
            
            #endregion
        }

        #endregion
    }
}