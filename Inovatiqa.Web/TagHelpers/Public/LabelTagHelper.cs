using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Inovatiqa.Web.TagHelpers.Public
{
    [HtmlTargetElement("label", Attributes = ForAttributeName)]
    public class LabelTagHelper : Microsoft.AspNetCore.Mvc.TagHelpers.LabelTagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string PostfixAttributeName = "asp-postfix";

        [HtmlAttributeName(PostfixAttributeName)]
        public string Postfix { get; set; }

        public LabelTagHelper(IHtmlGenerator generator) : base(generator)
        {
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.Content.Append(Postfix);

            return base.ProcessAsync(context, output);
        }
    }
}