using System;
using System.Collections.Generic;
using System.Security.Claims;
using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Customers.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using IAuthenticationService = Inovatiqa.Services.Authentication.Interfaces.IAuthenticationService;

namespace Inovatiqa.Services.Authentication
{
    public partial class CookieAuthenticationService : IAuthenticationService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private Customer _customer;

        #endregion

        #region Ctor

        public CookieAuthenticationService(ICustomerService customerService,
            IHttpContextAccessor httpContextAccessor)
        {
            _customerService = customerService;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Methods

        public virtual async void SignIn(Customer customer, bool isPersistent)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var claims = new List<Claim>();

            if (!string.IsNullOrEmpty(customer.Username))
                claims.Add(new Claim(ClaimTypes.Name, customer.Username, ClaimValueTypes.String, InovatiqaDefaults.ClaimsIssuer));

            if (!string.IsNullOrEmpty(customer.Email))
                claims.Add(new Claim(ClaimTypes.Email, customer.Email, ClaimValueTypes.Email, InovatiqaDefaults.ClaimsIssuer));

            var userIdentity = new ClaimsIdentity(claims, InovatiqaDefaults.AuthenticationScheme);
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            var authenticationProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                IssuedUtc = DateTime.UtcNow
            };

            await _httpContextAccessor.HttpContext.SignInAsync(InovatiqaDefaults.AuthenticationScheme, userPrincipal, authenticationProperties);

            _customer = customer;
        }

        public virtual async void SignOut()
        {
            _customer = null;

            await _httpContextAccessor.HttpContext.SignOutAsync(InovatiqaDefaults.AuthenticationScheme);
        }

        public virtual Customer GetAuthenticatedCustomer()
        {
            if (_customer != null)
                return _customer;

            var authenticateResult = _httpContextAccessor.HttpContext.AuthenticateAsync(InovatiqaDefaults.AuthenticationScheme).Result;
            if (!authenticateResult.Succeeded)
                return null;

            Customer customer = null;

            var usernameClaim = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.Name
                   && claim.Issuer.Equals(InovatiqaDefaults.ClaimsIssuer, StringComparison.InvariantCultureIgnoreCase));
            if (usernameClaim != null)
                customer = _customerService.GetCustomerByUsername(usernameClaim.Value);

            if (customer == null || !customer.Active || customer.RequireReLogin || customer.Deleted || !_customerService.IsRegistered(customer))
                return null;

            _customer = customer;

            return _customer;
        }

        #endregion
    }
}