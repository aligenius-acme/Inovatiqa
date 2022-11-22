using System;
using System.Linq;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Inovatiqa.Web.Mvc.Filters
{
    public sealed class ValidateVendorAttribute : TypeFilterAttribute
    {
        #region Fields

        private readonly bool _ignoreFilter;

        #endregion

        #region Ctor

        public ValidateVendorAttribute(bool ignore = false) : base(typeof(ValidateVendorFilter))
        {
            _ignoreFilter = ignore;
            Arguments = new object[] { ignore };
        }

        #endregion

        #region Properties

        public bool IgnoreFilter => _ignoreFilter;

        #endregion

        #region Nested filter

        private class ValidateVendorFilter : IAuthorizationFilter
        {
            #region Fields

            private readonly bool _ignoreFilter;
            private readonly ICustomerService _customerService;
            private readonly IWorkContextService _workContextService;

            #endregion

            #region Ctor

            public ValidateVendorFilter(bool ignoreFilter, IWorkContextService workContextService, ICustomerService customerService)
            {
                _ignoreFilter = ignoreFilter;
                _customerService = customerService;
                _workContextService = workContextService;
            }

            #endregion

            #region Methods

            public void OnAuthorization(AuthorizationFilterContext filterContext)
            {
                if (filterContext == null)
                    throw new ArgumentNullException(nameof(filterContext));

                var actionFilter = filterContext.ActionDescriptor.FilterDescriptors
                    .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                    .Select(filterDescriptor => filterDescriptor.Filter).OfType<ValidateVendorAttribute>().FirstOrDefault();

                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                    return;


                if (!_customerService.IsVendor(_workContextService.CurrentCustomer))
                    return;

                if (_workContextService.CurrentVendor == null)
                    filterContext.Result = new ChallengeResult();
            }

            #endregion
        }

        #endregion
    }
}