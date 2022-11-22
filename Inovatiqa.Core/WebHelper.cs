using System;
using System.Linq;
using System.Net;
using Inovatiqa.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Inovatiqa.Core
{
    public partial class WebHelper : IWebHelper
    {
        #region Fields 

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;

        #endregion

        #region Ctor

        public WebHelper(IHttpContextAccessor httpContextAccessor,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }

        #endregion

        #region Utilities

        protected virtual bool IsRequestAvailable()
        {
            if (_httpContextAccessor?.HttpContext == null)
                return false;

            try
            {
                if (_httpContextAccessor.HttpContext.Request == null)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Methods

        public virtual string GetRawUrl(HttpRequest request)
        {
            var rawUrl = request.HttpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;

            if (string.IsNullOrEmpty(rawUrl))
                rawUrl = $"{request.PathBase}{request.Path}{request.QueryString}";

            return rawUrl;
        }

        public virtual string GetCurrentIpAddress()
        {
            if (!IsRequestAvailable())
                return string.Empty;

            var result = string.Empty;
            try
            {
                if (_httpContextAccessor.HttpContext.Request.Headers != null)
                {
                    var forwardedHttpHeaderKey = InovatiqaDefaults.XForwardedForHeader;

                    var forwardedHeader = _httpContextAccessor.HttpContext.Request.Headers[forwardedHttpHeaderKey];
                    if (!StringValues.IsNullOrEmpty(forwardedHeader))
                        result = forwardedHeader.FirstOrDefault();
                }

                if (string.IsNullOrEmpty(result) && _httpContextAccessor.HttpContext.Connection.RemoteIpAddress != null)
                    result = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            catch
            {
                return string.Empty;
            }

            if (result != null && result.Equals(IPAddress.IPv6Loopback.ToString(), StringComparison.InvariantCultureIgnoreCase))
                result = IPAddress.Loopback.ToString();

            if (IPAddress.TryParse(result ?? string.Empty, out var ip))
                result = ip.ToString();
            else if (!string.IsNullOrEmpty(result))
                result = result.Split(':').FirstOrDefault();

            return result;
        }

        public virtual string CurrentRequestProtocol => IsCurrentConnectionSecured() ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;

        public virtual bool IsCurrentConnectionSecured()
        {
            if (!IsRequestAvailable())
                return false;

            return _httpContextAccessor.HttpContext.Request.IsHttps;
        }

        public virtual string ModifyQueryString(string url, string key, params string[] values)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            if (string.IsNullOrEmpty(key))
                return url;

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var isLocalUrl = urlHelper.IsLocalUrl(url);

            var uriStr = url;
            if (isLocalUrl)
            {
                var pathBase = _httpContextAccessor.HttpContext.Request.PathBase;
                uriStr = $"{GetStoreLocation().TrimEnd('/')}{(url.StartsWith(pathBase) ? url.Replace(pathBase, "") : url)}";
            }

            var uri = new Uri(uriStr, UriKind.Absolute);

            var queryParameters = QueryHelpers.ParseQuery(uri.Query);

            queryParameters[key] = string.Join(",", values);

            var queryBuilder = new QueryBuilder(queryParameters
                .ToDictionary(parameter => parameter.Key, parameter => parameter.Value.FirstOrDefault()?.ToString() ?? string.Empty));

            url = $"{(isLocalUrl ? uri.LocalPath : uri.GetLeftPart(UriPartial.Path))}{queryBuilder.ToQueryString()}{uri.Fragment}";

            return url;
        }

        public virtual string GetStoreHost(bool useSsl)
        {
            if (!IsRequestAvailable())
                return string.Empty;

            var hostHeader = _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Host];
            if (StringValues.IsNullOrEmpty(hostHeader))
                return string.Empty;

            var storeHost = $"{(useSsl ? Uri.UriSchemeHttps : Uri.UriSchemeHttp)}{Uri.SchemeDelimiter}{hostHeader.FirstOrDefault()}";

            storeHost = $"{storeHost.TrimEnd('/')}/";

            return storeHost;
        }

        public virtual string GetStoreLocation(bool? useSsl = null)
        {
            var storeLocation = string.Empty;

            var storeHost = GetStoreHost(useSsl ?? IsCurrentConnectionSecured());
            if (!string.IsNullOrEmpty(storeHost))
            {
                storeLocation = IsRequestAvailable() ? $"{storeHost.TrimEnd('/')}{_httpContextAccessor.HttpContext.Request.PathBase}" : storeHost;
            }

            if (string.IsNullOrEmpty(storeHost))
            {
                storeLocation = InovatiqaDefaults.StoreUrl
                    ?? throw new Exception("Current store cannot be loaded");
            }

            storeLocation = $"{storeLocation.TrimEnd('/')}/";

            return storeLocation;
        }

        public virtual string GetThisPageUrl(bool includeQueryString, bool? useSsl = null, bool lowercaseUrl = false)
        {
            if (!IsRequestAvailable())
                return string.Empty;

            var storeLocation = GetStoreLocation(useSsl ?? IsCurrentConnectionSecured());

            var pageUrl = $"{storeLocation.TrimEnd('/')}{_httpContextAccessor.HttpContext.Request.Path}";

            if (includeQueryString)
                pageUrl = $"{pageUrl}{_httpContextAccessor.HttpContext.Request.QueryString}";

            if (lowercaseUrl)
                pageUrl = pageUrl.ToLowerInvariant();

            return pageUrl;
        }

        public virtual string GetUrlReferrer()
        {
            if (!IsRequestAvailable())
                return string.Empty;
            return _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Referer];
        }

        public virtual bool IsAjaxRequest(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Headers == null)
                return false;

            return request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        #endregion
    }
}