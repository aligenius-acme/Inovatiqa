using System;
using System.Linq;
using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Inovatiqa.Web.Mvc.Filters
{
    public sealed class AuthorizeAdminAttribute : TypeFilterAttribute
    {
        #region Fields

        private readonly bool _ignoreFilter;

        #endregion

        #region Ctor

        public AuthorizeAdminAttribute(bool ignore = false) : base(typeof(AuthorizeAdminFilter))
        {
            _ignoreFilter = ignore;
            Arguments = new object[] { ignore };
        }

        #endregion

        #region Properties

        public bool IgnoreFilter => _ignoreFilter;

        #endregion


        #region Nested filter

        private class AuthorizeAdminFilter : IAuthorizationFilter
        {
            #region Fields

            private readonly bool _ignoreFilter;
            private readonly IPermissionService _permissionService;

            #endregion

            #region Ctor

            public AuthorizeAdminFilter(bool ignoreFilter, IPermissionService permissionService)
            {
                _ignoreFilter = ignoreFilter;
                _permissionService = permissionService;
            }

            #endregion

            #region Methods

            public void OnAuthorization(AuthorizationFilterContext filterContext)
            {
                if (filterContext == null)
                    throw new ArgumentNullException(nameof(filterContext));

                var actionFilter = filterContext.ActionDescriptor.FilterDescriptors
                    .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                    .Select(filterDescriptor => filterDescriptor.Filter).OfType<AuthorizeAdminAttribute>().FirstOrDefault();

                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                    return;


                if (filterContext.Filters.Any(filter => filter is AuthorizeAdminFilter))
                {
                    if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                        filterContext.Result = new ChallengeResult();
                }
            }

            #endregion
        }

        #endregion
    }
}