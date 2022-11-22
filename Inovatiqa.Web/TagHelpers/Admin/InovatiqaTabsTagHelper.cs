using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Inovatiqa.Web.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Inovatiqa.Web.Framework.TagHelpers.Admin
{
    [HtmlTargetElement("nop-tabs", Attributes = IdAttributeName)]
    public class InovatiqaTabsTagHelper : TagHelper
    {
        private const string IdAttributeName = "id";
        private const string TabNameToSelectAttributeName = "asp-tab-name-to-select";
        private const string RenderSelectedTabInputAttributeName = "asp-render-selected-tab-input";

        private readonly IHtmlHelper _htmlHelper;

        [HtmlAttributeName(TabNameToSelectAttributeName)]
        public string TabNameToSelect { set; get; }

        [HtmlAttributeName(RenderSelectedTabInputAttributeName)]
        public string RenderSelectedTabInput { set; get; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public InovatiqaTabsTagHelper(IHtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            var viewContextAware = _htmlHelper as IViewContextAware;
            viewContextAware?.Contextualize(ViewContext);

            var tabContext = new List<NopTabContextItem>();
            context.Items.Add(typeof(InovatiqaTabsTagHelper), tabContext);

            var tabNameToSelect = ViewContext.HttpContext.Request.Query["tabNameToSelect"];

            if (!string.IsNullOrEmpty(TabNameToSelect))
                tabNameToSelect = TabNameToSelect;

            if (!string.IsNullOrEmpty(tabNameToSelect))
                context.Items.Add("tabNameToSelect", tabNameToSelect);

            await output.GetChildContentAsync();

            var tabsTitle = new TagBuilder("ul");
            tabsTitle.AddCssClass("nav");
            tabsTitle.AddCssClass("nav-tabs");

            var tabsContent = new TagBuilder("div");
            tabsContent.AddCssClass("tab-content");

            foreach (var tabItem in tabContext)
            {
                tabsTitle.InnerHtml.AppendHtml(tabItem.Title);
                tabsContent.InnerHtml.AppendHtml(tabItem.Content);
            }

            output.Content.AppendHtml(tabsTitle.RenderHtmlContent());
            output.Content.AppendHtml(tabsContent.RenderHtmlContent());

            bool.TryParse(RenderSelectedTabInput, out bool renderSelectedTabInput);
            if (string.IsNullOrEmpty(RenderSelectedTabInput) || renderSelectedTabInput)
            {
                var selectedTabInput = new TagBuilder("input");
                selectedTabInput.Attributes.Add("type", "hidden");
                selectedTabInput.Attributes.Add("id", "selected-tab-name");
                selectedTabInput.Attributes.Add("name", "selected-tab-name");
                selectedTabInput.Attributes.Add("value", _htmlHelper.GetSelectedTabName());
                output.PreContent.SetHtmlContent(selectedTabInput.RenderHtmlContent());

                if (output.Attributes.ContainsName("id"))
                {
                    var script = new TagBuilder("script");
                    script.InnerHtml.AppendHtml("$(document).ready(function () {bindBootstrapTabSelectEvent('" + output.Attributes["id"].Value + "', 'selected-tab-name');});");
                    output.PostContent.SetHtmlContent(script.RenderHtmlContent());
                }
            }

            output.TagName = "div";

            var itemClass = "nav-tabs-custom";
            var classValue = output.Attributes.ContainsName("class")
                ? $"{output.Attributes["class"].Value} {itemClass}"
                : itemClass;
            output.Attributes.SetAttribute("class", classValue);
        }
    }

    [HtmlTargetElement("nop-tab", ParentTag = "nop-tabs", Attributes = NameAttributeName)]
    public class NopTabTagHelper : TagHelper
    {
        private const string NameAttributeName = "asp-name";
        private const string TitleAttributeName = "asp-title";
        private const string DefaultAttributeName = "asp-default";

        private readonly IHtmlHelper _htmlHelper;

        [HtmlAttributeName(TitleAttributeName)]
        public string Title { set; get; }

        [HtmlAttributeName(DefaultAttributeName)]
        public string IsDefault { set; get; }

        [HtmlAttributeName(NameAttributeName)]
        public string Name { set; get; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public NopTabTagHelper(IHtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            var viewContextAware = _htmlHelper as IViewContextAware;
            viewContextAware?.Contextualize(ViewContext);

            bool.TryParse(IsDefault, out bool isDefaultTab);

            var tabNameToSelect = context.Items.ContainsKey("tabNameToSelect")
                ? context.Items["tabNameToSelect"].ToString()
                : "";

            if (string.IsNullOrEmpty(tabNameToSelect))
                tabNameToSelect = _htmlHelper.GetSelectedTabName();

            if (string.IsNullOrEmpty(tabNameToSelect) && isDefaultTab)
                tabNameToSelect = Name;

            var tabTitle = new TagBuilder("li");
            var a = new TagBuilder("a")
            {
                Attributes =
                {
                    new KeyValuePair<string, string>("data-tab-name", Name),
                    new KeyValuePair<string, string>("href", $"#{Name}"),
                    new KeyValuePair<string, string>("data-toggle", "tab"),
                }
            };
            a.InnerHtml.AppendHtml(Title);

            if (context.AllAttributes.ContainsName("class"))
                tabTitle.Attributes.Add("class", context.AllAttributes["class"].Value.ToString());
            tabTitle.InnerHtml.AppendHtml(a.RenderHtmlContent());

            var tabContent = new TagBuilder("div");
            tabContent.AddCssClass("tab-pane");
            tabContent.Attributes.Add("id", Name);
            tabContent.InnerHtml.AppendHtml(output.GetChildContentAsync().Result.GetContent());

            if (tabNameToSelect == Name)
            {
                tabTitle.AddCssClass("active");
                tabContent.AddCssClass("active");
            }

            var tabContext = (List<NopTabContextItem>)context.Items[typeof(InovatiqaTabsTagHelper)];
            tabContext.Add(new NopTabContextItem()
            {
                Title = tabTitle.RenderHtmlContent(),
                Content = tabContent.RenderHtmlContent(),
                IsDefault = isDefaultTab
            });

            output.SuppressOutput();
        }
    }

    public class NopTabContextItem
    {
        public string Title { set; get; }

        public string Content { set; get; }

        public bool IsDefault { set; get; }
    }
}