using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Inovatiqa.Web.Framework.TagHelpers.Admin
{
    [HtmlTargetElement("nop-textarea", Attributes = ForAttributeName)]
    public class InovatiqaTextAreaTagHelper : TextAreaTagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string RequiredAttributeName = "asp-required";
        private const string DisabledAttributeName = "asp-disabled";

        [HtmlAttributeName(DisabledAttributeName)]
        public string IsDisabled { set; get; }

        [HtmlAttributeName(RequiredAttributeName)]
        public string IsRequired { set; get; }

        public InovatiqaTextAreaTagHelper(IHtmlGenerator generator) : base(generator)
        {
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "textarea";
            output.TagMode = TagMode.StartTagAndEndTag;

            var classValue = output.Attributes.ContainsName("class")
                ? $"{output.Attributes["class"].Value} form-control"
                : "form-control";
            output.Attributes.SetAttribute("class", classValue);

            bool.TryParse(IsDisabled, out bool disabled);
            if (disabled)
            {
                var d = new TagHelperAttribute("disabled", "disabled");
                output.Attributes.Add(d);
            }

            var rowsNumber = output.Attributes.ContainsName("rows") ? output.Attributes["rows"].Value : 4;
            output.Attributes.SetAttribute("rows", rowsNumber);
            var colsNumber = output.Attributes.ContainsName("cols") ? output.Attributes["cols"].Value : 20;
            output.Attributes.SetAttribute("cols", colsNumber);

            bool.TryParse(IsRequired, out bool required);
            if (required)
            {
                output.PreElement.SetHtmlContent("<div class='input-group input-group-required'>");
                output.PostElement.SetHtmlContent("<div class=\"input-group-btn\"><span class=\"required\">*</span></div></div>");
            }

            base.Process(context, output);
        }
    }
}