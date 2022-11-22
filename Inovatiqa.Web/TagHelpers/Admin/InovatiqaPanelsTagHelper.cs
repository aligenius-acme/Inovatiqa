using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Inovatiqa.Web.Framework.TagHelpers.Admin
{
    [HtmlTargetElement("nop-panels", Attributes = ID_ATTRIBUTE_NAME)]
    public class InovatiqaPanelsTagHelper : TagHelper
    {
        private const string ID_ATTRIBUTE_NAME = "id";

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }
    }
}