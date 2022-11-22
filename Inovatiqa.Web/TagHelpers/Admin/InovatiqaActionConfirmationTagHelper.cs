using System;
using System.Threading.Tasks;
using Inovatiqa.Web.Extensions;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Inovatiqa.Web.TagHelpers.Admin
{
    [HtmlTargetElement("nop-action-confirmation", Attributes = ButtonIdAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public class InovatiqaActionConfirmationTagHelper : TagHelper
    {
        private const string ButtonIdAttributeName = "asp-button-id";
        private const string ActionAttributeName = "asp-action";
        private const string AdditionaConfirmText = "asp-additional-confirm";

        private readonly IHtmlHelper _htmlHelper;

        protected IHtmlGenerator Generator { get; set; }

        [HtmlAttributeName(ButtonIdAttributeName)]
        public string ButtonId { get; set; }

        [HtmlAttributeName(ActionAttributeName)]
        public string Action { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName(AdditionaConfirmText)]
        public string ConfirmText { get; set; }

        public InovatiqaActionConfirmationTagHelper(IHtmlGenerator generator, IHtmlHelper htmlHelper)
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

            if (string.IsNullOrEmpty(Action))
                Action = _htmlHelper.ViewContext.RouteData.Values["action"].ToString();

            var modalId = new HtmlString(ButtonId + "-action-confirmation").ToHtmlString();

            var actionConfirmationModel = new ActionConfirmationModel()
            {
                ControllerName = _htmlHelper.ViewContext.RouteData.Values["controller"].ToString(),
                ActionName = Action,
                WindowId = modalId,
                AdditonalConfirmText = ConfirmText
            };

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            output.Attributes.Add("id", modalId);
            output.Attributes.Add("class", "modal fade");
            output.Attributes.Add("tabindex", "-1");
            output.Attributes.Add("role", "dialog");
            output.Attributes.Add("aria-labelledby", $"{modalId}-title");
            output.Content.SetHtmlContent(await _htmlHelper.PartialAsync("Confirm", actionConfirmationModel));

            var script = new TagBuilder("script");
            script.InnerHtml.AppendHtml("$(document).ready(function () {" +
                                        $"$('#{ButtonId}').attr(\"data-toggle\", \"modal\").attr(\"data-target\", \"#{modalId}\");" +
                                        $"$('#{modalId}-submit-button').attr(\"name\", $(\"#{ButtonId}\").attr(\"name\"));" +
                                        $"$(\"#{ButtonId}\").attr(\"name\", \"\");" +
                                        $"if($(\"#{ButtonId}\").attr(\"type\") == \"submit\")$(\"#{ButtonId}\").attr(\"type\", \"button\");" +
                                        "});");
            output.PostContent.SetHtmlContent(script.RenderHtmlContent());
        }
    }
}