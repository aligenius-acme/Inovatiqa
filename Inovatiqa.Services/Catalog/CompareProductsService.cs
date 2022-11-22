using System;
using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Inovatiqa.Services.Catalog
{
    public partial class CompareProductsService : ICompareProductsService
    {
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public CompareProductsService(IHttpContextAccessor httpContextAccessor,
            IProductService productService,
            IWebHelper webHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _productService = productService;
            _webHelper = webHelper;
        }

        #endregion

        #region Utilities

        protected virtual List<int> GetComparedProductIds()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request == null)
                return new List<int>();

            var cookieName = $"{InovatiqaDefaults.Prefix}{InovatiqaDefaults.ComparedProductsCookie}";
            if (!httpContext.Request.Cookies.TryGetValue(cookieName, out var productIdsCookie) || string.IsNullOrEmpty(productIdsCookie))
                return new List<int>();

            var productIds = productIdsCookie.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            return productIds.Select(int.Parse).Distinct().ToList();
        }

        protected virtual void AddCompareProductsCookie(IEnumerable<int> comparedProductIds)
        {
            var cookieName = $"{InovatiqaDefaults.Prefix}{InovatiqaDefaults.ComparedProductsCookie}";
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(cookieName);

            var comparedProductIdsCookie = string.Join(",", comparedProductIds);

            var cookieExpires = InovatiqaDefaults.CompareProductsCookieExpires;
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddHours(cookieExpires),
                HttpOnly = true,
                Secure =  _webHelper.IsCurrentConnectionSecured()
            };

            _httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName, comparedProductIdsCookie, cookieOptions);
        }

        #endregion

        #region Methods

        public virtual void ClearCompareProducts()
        {
            if (_httpContextAccessor.HttpContext?.Response == null)
                return;

            var cookieName = $"{InovatiqaDefaults.Prefix}{InovatiqaDefaults.ComparedProductsCookie}";
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(cookieName);
        }

        public virtual IList<Product> GetComparedProducts()
        {
            var productIds = GetComparedProductIds();

            return _productService.GetProductsByIds(productIds.ToArray())
                .Where(product => product.Published && !product.Deleted).ToList();
        }

        public virtual void RemoveProductFromCompareList(int productId)
        {
            if (_httpContextAccessor.HttpContext?.Response == null)
                return;

            var comparedProductIds = GetComparedProductIds();

            if (!comparedProductIds.Contains(productId))
                return;

            comparedProductIds.Remove(productId);

            AddCompareProductsCookie(comparedProductIds);
        }

        public virtual void AddProductToCompareList(int productId)
        {
            if (_httpContextAccessor.HttpContext?.Response == null)
                return;

            var comparedProductIds = GetComparedProductIds();

            if (!comparedProductIds.Contains(productId))
                comparedProductIds.Insert(0, productId);

            comparedProductIds = comparedProductIds.Take(InovatiqaDefaults.CompareProductsNumber).ToList();

            AddCompareProductsCookie(comparedProductIds);
        }

        #endregion
    }
}