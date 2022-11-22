using System;
using System.Threading.Tasks;
using Inovatiqa.Web.Extensions;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Inovatiqa.Web.Framework.TagHelpers.Admin
{
    [HtmlTargetElement("nop-alert", Attributes = AlertNameId, TagStructure = TagStructure.WithoutEndTag)]
    public class NopAlertTagHelper : TagHelper
    {
        private const string AlertNameId = "asp-alert-id";
        private const string AlertMessageName = "asp-alert-message";

        private readonly IHtmlHelper _htmlHelper;

        protected IHtmlGenerator Generator { get; set; }

        [HtmlAttributeName(AlertNameId)]
        public string AlertId { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName(AlertMessageName)]
        public string Message { get; set; }

        public NopAlertTagHelper(IHtmlGenerator generator, IHtmlHelper htmlHelper)
        {
            Generator = generator;
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

            var modalId = new HtmlString(AlertId + "-action-alert").ToHtmlString();

            var actionAlertModel = new ActionAlertModel()
            {
                AlertId = AlertId,
                WindowId = modalId,
                AlertMessage = Message
            };

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            output.Attributes.Add("id", modalId);
            output.Attributes.Add("class", "modal fade");
            output.Attributes.Add("tabindex", "-1");
            output.Attributes.Add("role", "dialog");
            output.Attributes.Add("aria-labelledby", $"{modalId}-title");
            output.Content.SetHtmlContent(await _htmlHelper.PartialAsync("Alert", actionAlertModel));

            var script = new TagBuilder("script");
            script.InnerHtml.AppendHtml("$(document).ready(function () {" +
                                            $"$('#{AlertId}').attr(\"data-toggle\", \"modal\").attr(\"data-target\", \"#{modalId}\")" + "});");

            output.PostContent.SetHtmlContent(script.RenderHtmlContent());
        }
    }
}
