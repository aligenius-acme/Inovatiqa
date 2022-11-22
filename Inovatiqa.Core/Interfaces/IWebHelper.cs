using Microsoft.AspNetCore.Http;

namespace Inovatiqa.Core.Interfaces
{
    public partial interface IWebHelper
    {
        string GetUrlReferrer();
        string GetRawUrl(HttpRequest request);

        string GetCurrentIpAddress();

        string CurrentRequestProtocol { get; }

        string ModifyQueryString(string url, string key, params string[] values);

        string GetStoreHost(bool useSsl);

        string GetThisPageUrl(bool includeQueryString, bool? useSsl = null, bool lowercaseUrl = false);

        bool IsCurrentConnectionSecured();

        bool IsAjaxRequest(HttpRequest request);
    }
}
