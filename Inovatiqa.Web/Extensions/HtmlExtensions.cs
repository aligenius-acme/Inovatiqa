using System;
using System.IO;
using System.Text.Encodings.Web;
using Inovatiqa.Web.Framework.Models;
using Inovatiqa.Web.Paging;
using Inovatiqa.Web.Paging.Interfaces;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Extensions
{
    public static class HtmlExtensions
    {

        #region Common extensions

        public static string RenderHtmlContent(this IHtmlContent htmlContent)
        {
            using var writer = new StringWriter();
            htmlContent.WriteTo(writer, HtmlEncoder.Default);
            var htmlOutput = writer.ToString();
            return htmlOutput;
        }

        public static string ToHtmlString(this IHtmlContent tag)
        {
            using var writer = new StringWriter();
            tag.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }

        public static Pager Pager(this IHtmlHelper helper, IPageableModel pagination, HttpRequest request)
        {
            return new Pager(pagination, helper.ViewContext, request);
        }

        public static string GetSelectedPanelName(this IHtmlHelper helper)
        {
            var tabName = string.Empty;
            const string dataKey = "inovatiqa.selected-panel-name";

            if (helper.ViewData.ContainsKey(dataKey))
                tabName = helper.ViewData[dataKey].ToString();

            if (helper.ViewContext.TempData.ContainsKey(dataKey))
                tabName = helper.ViewContext.TempData[dataKey].ToString();

            return tabName;
        }

        public static string GetSelectedTabName(this IHtmlHelper helper, string dataKeyPrefix = null)
        {
            var tabName = string.Empty;
            var dataKey = "inovatiqa.selected-tab-name";
            if (!string.IsNullOrEmpty(dataKeyPrefix))
                dataKey += $"-{dataKeyPrefix}";

            if (helper.ViewData.ContainsKey(dataKey))
                tabName = helper.ViewData[dataKey].ToString();

            if (helper.ViewContext.TempData.ContainsKey(dataKey))
                tabName = helper.ViewContext.TempData[dataKey].ToString();

            return tabName;
        }

        public static IHtmlContent LocalizedEditor<T, TLocalizedModelLocal>(this IHtmlHelper<T> helper,
            string name,
            Func<int, HelperResult> localizedTemplate,
            Func<T, HelperResult> standardTemplate,
            bool ignoreIfSeveralStores = false, string cssClass = "")
            where T : ILocalizedModel<TLocalizedModelLocal>
            where TLocalizedModelLocal : ILocalizedLocaleModel
        {
            return new HtmlString(standardTemplate(helper.ViewData.Model).RenderHtmlContent());
        }

        #endregion
    }
}