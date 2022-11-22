using System;
using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Authentication.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Vendors.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Inovatiqa.Services
{
    public partial class WorkContextService : IWorkContextService
    {
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICustomerService _customerService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IVendorService _vendorService;

        private Customer _originalCustomerIfImpersonated;
        private Vendor _cachedVendor;

        #endregion

        #region Ctor

        public WorkContextService(IHttpContextAccessor httpContextAccessor,
            ICustomerService customerService,
            IAuthenticationService authenticationService,
            IGenericAttributeService genericAttributeService,
            IVendorService vendorService)
        {
            _httpContextAccessor = httpContextAccessor;
            _customerService = customerService;
            _authenticationService = authenticationService;
            _genericAttributeService = genericAttributeService;
            _vendorService = vendorService;
        }

        #endregion

        #region Utilities

        protected virtual string GetCustomerCookie()
        {
            var cookieName = $"{InovatiqaDefaults.Prefix}{InovatiqaDefaults.CustomerCookie}";
            return _httpContextAccessor.HttpContext?.Request?.Cookies[cookieName];
        }

        protected virtual void SetCustomerCookie(Guid customerGuid)
        {
            if (_httpContextAccessor.HttpContext?.Response == null)
                return;

            var cookieName = $"{InovatiqaDefaults.Prefix}{InovatiqaDefaults.CustomerCookie}";
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(cookieName);

            var cookieExpires = InovatiqaDefaults.CustomerCookieExpires;
            var cookieExpiresDate = DateTime.Now.AddHours(cookieExpires);

            if (customerGuid == Guid.Empty)
                cookieExpiresDate = DateTime.Now.AddMonths(-1);

            var options = new CookieOptions
            {
                HttpOnly = true,
                Expires = cookieExpiresDate,
                Secure = InovatiqaDefaults.IsCurrentConnectionSecured
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName, customerGuid.ToString(), options);
        }

        #endregion

        #region Properties

        public virtual Vendor CurrentVendor
        {
            get
            {
                if (_cachedVendor != null)
                    return _cachedVendor;

                if (CurrentCustomer == null)
                    return null;

                var vendor = _vendorService.GetVendorById(CurrentCustomer.VendorId);

                if (vendor == null || vendor.Deleted || !vendor.Active)
                    return null;

                _cachedVendor = vendor;

                return _cachedVendor;
            }
        }

        public virtual Customer CurrentCustomer
        {
            get
            {
                var sessionName = $"{InovatiqaDefaults.Prefix}{InovatiqaDefaults.CustomerCookie}";
                var httpContext = _httpContextAccessor.HttpContext;

                Customer customer = null;

                customer = _authenticationService.GetAuthenticatedCustomer();

                if (customer != null && !customer.Deleted && customer.Active && !customer.RequireReLogin)
                {
                    var impersonatedCustomerId = _genericAttributeService
                        .GetAttribute<int?>(customer, InovatiqaDefaults.ImpersonatedCustomerIdAttribute, customer.Id);
                    if (impersonatedCustomerId.HasValue && impersonatedCustomerId.Value > 0)
                    {
                        var impersonatedCustomer = _customerService.GetCustomerById(impersonatedCustomerId.Value);
                        if (impersonatedCustomer != null && !impersonatedCustomer.Deleted && impersonatedCustomer.Active && !impersonatedCustomer.RequireReLogin)
                        {
                            _originalCustomerIfImpersonated = customer;
                            customer = impersonatedCustomer;
                        }
                    }
                }

                if (customer == null || customer.Deleted || !customer.Active || customer.RequireReLogin)
                {
                    var customerCookie = GetCustomerCookie();
                    if (!string.IsNullOrEmpty(customerCookie))
                    {
                        if (Guid.TryParse(customerCookie, out var customerGuid))
                        {
                            var customerByCookie = _customerService.GetCustomerByGuid(customerGuid);
                            if (customerByCookie != null && !_customerService.IsRegistered(customerByCookie))
                                customer = customerByCookie;

                            if(customer != null && httpContext.Session.GetString(sessionName) != null)
                                httpContext.Session.Remove(sessionName);
                        }
                    }
                }

                if (customer == null || customer.Deleted || !customer.Active || customer.RequireReLogin)
                {
                    if (httpContext.Session.GetString(sessionName) == null)
                    {
                        customer = _customerService.InsertGuestCustomer();
                        httpContext.Session.SetString(sessionName, customer.CustomerGuid.ToString());
                        SetCustomerCookie(customer.CustomerGuid);
                    }
                }

                if (customer == null || customer.Deleted || !customer.Active || customer.RequireReLogin)
                {
                    if (httpContext.Session.GetString(sessionName) != null)
                        customer = _customerService.GetCustomerByGuid(new Guid(httpContext.Session.GetString(sessionName).ToString()));
                }

                return customer;
            }
            set
            {
                SetCustomerCookie(value.CustomerGuid);
            }
        }

        public virtual Customer OriginalCustomerIfImpersonated => _originalCustomerIfImpersonated;

        #endregion
    }
}