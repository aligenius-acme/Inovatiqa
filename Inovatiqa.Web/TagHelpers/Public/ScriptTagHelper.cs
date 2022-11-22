using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Inovatiqa.Web.UI;
using Inovatiqa.Web.Extensions;

namespace Inovatiqa.Web.TagHelpers.Public
{
    [HtmlTargetElement("script", Attributes = LocationAttributeName)]
    public class ScriptTagHelper : TagHelper
    {
        private const string LocationAttributeName = "asp-location";
        private readonly IHtmlHelper _htmlHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPageHeadBuilder _pageHeadBuilder;

        [HtmlAttributeName(LocationAttributeName)]
        public ResourceLocation Location { set; get; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public ScriptTagHelper(IHtmlHelper htmlHelper, IHttpContextAccessor httpContextAccessor,
            IPageHeadBuilder pageHeadBuilder)
        {
            _htmlHelper = htmlHelper;
            _httpContextAccessor = httpContextAccessor;
            _pageHeadBuilder = pageHeadBuilder;
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

            if (_httpContextAccessor.HttpContext.Items["inovatiqa.IgnoreScriptTagLocation"] != null &&
                Convert.ToBoolean(_httpContextAccessor.HttpContext.Items["inovatiqa.IgnoreScriptTagLocation"]))
                return;

            var viewContextAware = _htmlHelper as IViewContextAware;
            viewContextAware?.Contextualize(ViewContext);

            var script = output.GetChildContentAsync().Result.GetContent();

            var scriptTag = new TagBuilder("script");
            scriptTag.InnerHtml.SetHtmlContent(new HtmlString(script));

            foreach (var attribute in context.AllAttributes)
                if (!attribute.Name.StartsWith("asp-"))
                    scriptTag.Attributes.Add(attribute.Name, attribute.Value.ToString());

            _htmlHelper.AddInlineScriptParts(_pageHeadBuilder, Location, scriptTag.RenderHtmlContent());

            output.SuppressOutput();
        }
    }

}