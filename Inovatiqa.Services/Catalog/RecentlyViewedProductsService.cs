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
    public partial class RecentlyViewedProductsService : IRecentlyViewedProductsService
    {
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public RecentlyViewedProductsService(IHttpContextAccessor httpContextAccessor,
            IProductService productService,
            IWebHelper webHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _productService = productService;
            _webHelper = webHelper;
        }

        #endregion

        #region Utilities

        protected List<int> GetRecentlyViewedProductsIds()
        {
            return GetRecentlyViewedProductsIds(int.MaxValue);
        }

        protected List<int> GetRecentlyViewedProductsIds(int number)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request == null)
                return new List<int>();

            var cookieName = $"{InovatiqaDefaults.Prefix}{InovatiqaDefaults.RecentlyViewedProductsCookie}";
            if (!httpContext.Request.Cookies.TryGetValue(cookieName, out var productIdsCookie) || string.IsNullOrEmpty(productIdsCookie))
                return new List<int>();

            var productIds = productIdsCookie.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            return productIds.Select(int.Parse).Distinct().Take(number).ToList();
        }

        protected virtual void AddRecentlyViewedProductsCookie(IEnumerable<int> recentlyViewedProductIds)
        {
            var cookieName = $"{InovatiqaDefaults.Prefix}{InovatiqaDefaults.RecentlyViewedProductsCookie}";
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(cookieName);

            var productIdsCookie = string.Join(",", recentlyViewedProductIds);

            var cookieExpires = InovatiqaDefaults.RecentlyViewedProductsCookieExpires;
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddHours(cookieExpires),
                HttpOnly = true,
                Secure = _webHelper.IsCurrentConnectionSecured()
            };

            _httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName, productIdsCookie, cookieOptions);
        }

        #endregion

        #region Methods

        public virtual IList<Product> GetRecentlyViewedProducts(int number)
        {
            var productIds = GetRecentlyViewedProductsIds(number);

            return _productService.GetProductsByIds(productIds.ToArray())
                .Where(product => product.Published && !product.Deleted).ToList();
        }

        public virtual void AddProductToRecentlyViewedList(int productId)
        {
            if (_httpContextAccessor.HttpContext?.Response == null)
                return;

            if (!InovatiqaDefaults.RecentlyViewedProductsEnabled)
                return;

            var productIds = GetRecentlyViewedProductsIds();

            if (!productIds.Contains(productId))
                productIds.Insert(0, productId);

            productIds = productIds.Take(InovatiqaDefaults.RecentlyViewedProductsNumber).ToList();

            AddRecentlyViewedProductsCookie(productIds);
        }

        #endregion
    }
}