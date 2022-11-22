using System;
using System.Collections.Generic;
using Inovatiqa.Web.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Inovatiqa.Web.Framework.TagHelpers.Admin
{
    [HtmlTargetElement("nop-panel", Attributes = NAME_ATTRIBUTE_NAME)]
    public class InovatiqaPanelTagHelper : TagHelper
    {
        private const string NAME_ATTRIBUTE_NAME = "asp-name";
        private const string TITLE_ATTRIBUTE_NAME = "asp-title";
        private const string HIDE_BLOCK_ATTRIBUTE_NAME_ATTRIBUTE_NAME = "asp-hide-block-attribute-name";
        private const string IS_HIDE_ATTRIBUTE_NAME = "asp-hide";
        private const string IS_ADVANCED_ATTRIBUTE_NAME = "asp-advanced";
        private const string PANEL_ICON_ATTRIBUTE_NAME = "asp-icon";

        private readonly IHtmlHelper _htmlHelper;

        [HtmlAttributeName(TITLE_ATTRIBUTE_NAME)]
        public string Title { get; set; }

        [HtmlAttributeName(NAME_ATTRIBUTE_NAME)]
        public string Name { get; set; }

        [HtmlAttributeName(HIDE_BLOCK_ATTRIBUTE_NAME_ATTRIBUTE_NAME)]
        public string HideBlockAttributeName { get; set; }

        [HtmlAttributeName(IS_HIDE_ATTRIBUTE_NAME)]
        public bool IsHide { get; set; }

        [HtmlAttributeName(IS_ADVANCED_ATTRIBUTE_NAME)]
        public bool IsAdvanced { get; set; }

        [HtmlAttributeName(PANEL_ICON_ATTRIBUTE_NAME)]
        public string PanelIconIsAdvanced { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public InovatiqaPanelTagHelper(IHtmlHelper htmlHelper)
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

            var panel = new TagBuilder("div")
            {
                Attributes =
                {
                    new KeyValuePair<string, string>("id", Name),
                    new KeyValuePair<string, string>("data-panel-name", Name),
                }
            };
            panel.AddCssClass("panel panel-default collapsible-panel");
            if (context.AllAttributes.ContainsName(IS_ADVANCED_ATTRIBUTE_NAME) && context.AllAttributes[IS_ADVANCED_ATTRIBUTE_NAME].Value.Equals(true))
            {
                panel.AddCssClass("advanced-setting");
            }

            var panelHeading = new TagBuilder("div");
            panelHeading.AddCssClass("panel-heading");
            panelHeading.Attributes.Add("data-hideAttribute", context.AllAttributes[HIDE_BLOCK_ATTRIBUTE_NAME_ATTRIBUTE_NAME].Value.ToString());

            if (context.AllAttributes[IS_HIDE_ATTRIBUTE_NAME].Value.Equals(false))
            {
                panelHeading.AddCssClass("opened");
            }

            if (context.AllAttributes.ContainsName(PANEL_ICON_ATTRIBUTE_NAME))
            {
                var panelIcon = new TagBuilder("i");
                panelIcon.AddCssClass("panel-icon");
                panelIcon.AddCssClass(context.AllAttributes[PANEL_ICON_ATTRIBUTE_NAME].Value.ToString());
                var iconContainer = new TagBuilder("div");
                iconContainer.AddCssClass("icon-container");
                iconContainer.InnerHtml.AppendHtml(panelIcon);
                panelHeading.InnerHtml.AppendHtml(iconContainer);
            }

            panelHeading.InnerHtml.AppendHtml($"<span>{context.AllAttributes[TITLE_ATTRIBUTE_NAME].Value}</span>");

            var collapseIcon = new TagBuilder("i");
            collapseIcon.AddCssClass("fa");
            collapseIcon.AddCssClass("toggle-icon");
            collapseIcon.AddCssClass(context.AllAttributes[IS_HIDE_ATTRIBUTE_NAME].Value.Equals(true) ? "fa-plus" : "fa-minus");
            panelHeading.InnerHtml.AppendHtml(collapseIcon);

            var panelContainer = new TagBuilder("div");
            panelContainer.AddCssClass("panel-container");
            if (context.AllAttributes[IS_HIDE_ATTRIBUTE_NAME].Value.Equals(true))
            {
                panelContainer.AddCssClass("collapsed");
            }

            panelContainer.InnerHtml.AppendHtml(output.GetChildContentAsync().Result.GetContent());

            panel.InnerHtml.AppendHtml(panelHeading);
            panel.InnerHtml.AppendHtml(panelContainer);

            output.Content.AppendHtml(panel.RenderHtmlContent());
        }
    }
}