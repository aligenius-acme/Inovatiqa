using System;
using Inovatiqa.Core;
using Inovatiqa.Web.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Inovatiqa.Web.Framework.TagHelpers.Admin
{
    [HtmlTargetElement("nop-nested-setting", Attributes = ForAttributeName)]
    public class InovatiqaNestedSettingTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        protected IHtmlGenerator Generator { get; set; }

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public InovatiqaNestedSettingTagHelper(IHtmlGenerator generator)
        {
            Generator = generator;
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

            var parentSettingName = For.Name;

            var random = CommonHelper.GenerateRandomInteger();
            var nestedSettingId = $"nestedSetting{random}";
            var parentSettingId = $"parentSetting{random}";

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("class", "nested-setting");

            if (context.AllAttributes.ContainsName("id"))
                nestedSettingId = context.AllAttributes["id"].Value.ToString();
            output.Attributes.Add("id", nestedSettingId);

            var script = new TagBuilder("script");
            script.InnerHtml.AppendHtml("$(document).ready(function () {" +
                                            $"initNestedSetting('{parentSettingName}', '{parentSettingId}', '{nestedSettingId}');" +
                                        "});");
            output.PreContent.SetHtmlContent(script.RenderHtmlContent());
        }
    }
}