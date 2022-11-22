using Inovatiqa.Web.UI;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Web.Extensions
{
    public static class LayoutExtensions
    {
        public static IHtmlContent Resource(this IHtmlHelper HtmlHelper, Func<object, HelperResult> Template, string Type)
        {
            if (HtmlHelper.ViewContext.HttpContext.Items[Type] != null) ((List<Func<object, HelperResult>>)HtmlHelper.ViewContext.HttpContext.Items[Type]).Add(Template);
            else HtmlHelper.ViewContext.HttpContext.Items[Type] = new List<Func<object, HelperResult>>() { Template };

            return new HtmlString(String.Empty);
        }

        public static IHtmlContent RenderResources(this IHtmlHelper HtmlHelper, string Type)
        {
            if (HtmlHelper.ViewContext.HttpContext.Items[Type] != null)
            {
                List<Func<object, HelperResult>> Resources = (List<Func<object, HelperResult>>)HtmlHelper.ViewContext.HttpContext.Items[Type];

                foreach (var Resource in Resources)
                {
                    if (Resource != null) HtmlHelper.ViewContext.Writer.Write(Resource(null));
                }
            }

            return new HtmlString(String.Empty);
        }

        public static void AddScriptParts(this IHtmlHelper html, IPageHeadBuilder builder, ResourceLocation location,
            string src, string debugSrc = "", bool excludeFromBundle = false, bool isAsync = false)
        {
            var pageHeadBuilder = builder;
            pageHeadBuilder.AddScriptParts(location, src, debugSrc, excludeFromBundle, isAsync);
        }

        public static IHtmlContent InovatiqaInlineScripts(this IHtmlHelper html, IPageHeadBuilder builder, ResourceLocation location)
        {
            var pageHeadBuilder = builder;
            return new HtmlString(pageHeadBuilder.GenerateInlineScripts(location));
        }

        public static void AddInlineScriptParts(this IHtmlHelper html, IPageHeadBuilder builder, ResourceLocation location, string script)
        {
            var pageHeadBuilder = builder;
            pageHeadBuilder.AddInlineScriptParts(location, script);
        }

        public static string GetActiveMenuItemSystemName(this IHtmlHelper html, IPageHeadBuilder builder)
        {
            var pageHeadBuilder = builder;
            return pageHeadBuilder.GetActiveMenuItemSystemName();
        }
    }
}
