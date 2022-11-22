using System;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Inovatiqa.Web.TagHelpers.Shared
{
    [HtmlTargetElement("nop-required", TagStructure = TagStructure.WithoutEndTag)]
    public class NopRequiredTagHelper : TagHelper
    {
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

            output.SuppressOutput();

            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", "required");
            output.Content.SetContent("*");
        }
    }
}