using System;
using System.Net.Http;
using System.Threading.Tasks;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Core.Rss;
using Inovatiqa.Services.WorkContext.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Inovatiqa.Services.Common
{
    public partial class InovatiqaHttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContextService _workContextService;

        #endregion

        #region Ctor

        public InovatiqaHttpClient(HttpClient client,
            IHttpContextAccessor httpContextAccessor,
            IWebHelper webHelper,
            IWorkContextService workContextService)
        {
            client.BaseAddress = new Uri(InovatiqaDefaults.StoreUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
            client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, $"inovatiqa-001");

            _httpClient = client;
            _httpContextAccessor = httpContextAccessor;
            _webHelper = webHelper;
            _workContextService = workContextService;
        }

        #endregion

        #region Methods

        public virtual async Task PingAsync()
        {
            await _httpClient.GetStringAsync("/");
        }

        public virtual async Task<string> GetCopyrightWarningAsync()
        {
            var url = InovatiqaDefaults.StoreUrl.ToLowerInvariant();

            return await _httpClient.GetStringAsync(url);
        }

        public virtual async Task<RssFeed> GetNewsRssAsync()
        {
            var url = InovatiqaDefaults.StoreUrl.ToLowerInvariant();

            using var stream = await _httpClient.GetStreamAsync(url);
            return await RssFeed.LoadAsync(stream);
        }

        public virtual async Task InstallationCompletedAsync(string email, string languageCode)
        {
            var url = InovatiqaDefaults.StoreUrl.ToLowerInvariant();

            await _httpClient.GetStringAsync(url);
        }

        public virtual async Task<string> GetExtensionsCategoriesAsync()
        {
            var url = InovatiqaDefaults.StoreUrl.ToLowerInvariant();

            return await _httpClient.GetStringAsync(url);
        }

        public virtual async Task<string> GetExtensionsVersionsAsync()
        {
            var url = InovatiqaDefaults.StoreUrl.ToLowerInvariant();

            return await _httpClient.GetStringAsync(url);
        }

        public virtual async Task<string> GetExtensionsAsync(int categoryId = 0,
            int versionId = 0, int price = 0, string searchTerm = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var url = InovatiqaDefaults.StoreUrl.ToLowerInvariant();

            return await _httpClient.GetStringAsync(url);
        }

        #endregion
    }
}