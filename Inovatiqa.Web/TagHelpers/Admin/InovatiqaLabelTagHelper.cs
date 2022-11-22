using System;
using Inovatiqa.Services.WorkContext.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Inovatiqa.Web.Framework.TagHelpers.Admin
{
    [HtmlTargetElement("nop-label", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public class InovatiqaLabelTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string DisplayHintAttributeName = "asp-display-hint";

        protected IHtmlGenerator Generator { get; set; }

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName(DisplayHintAttributeName)]
        public bool DisplayHint { get; set; } = true;

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public InovatiqaLabelTagHelper(IHtmlGenerator generator, IWorkContextService workContext)
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

            var tagBuilder = Generator.GenerateLabel(ViewContext, For.ModelExplorer, For.Name, null, new { @class = "control-label" });
            if (tagBuilder != null)
            {
                output.TagName = "div";
                output.TagMode = TagMode.StartTagAndEndTag;
                var classValue = output.Attributes.ContainsName("class")
                                    ? $"{output.Attributes["class"].Value} label-wrapper"
                                    : "label-wrapper";
                output.Attributes.SetAttribute("class", classValue);

                output.Content.SetHtmlContent(tagBuilder);

                ////////if (For.Metadata.AdditionalValues.TryGetValue("NopResourceDisplayNameAttribute", out object value))
                ////////{
                ////////    var resourceDisplayName = value as NopResourceDisplayNameAttribute;
                ////////    if (resourceDisplayName != null && DisplayHint)
                ////////    {
                ////////        var langId = InovatiqaDefaults.LanguageId;
                ////////        var hintResource = _localizationService.GetResource(
                ////////            resourceDisplayName.ResourceKey + ".Hint", langId, returnEmptyIfNotFound: true,
                ////////            logIfNotFound: false);

                ////////        if (!string.IsNullOrEmpty(hintResource))
                ////////        {
                ////////            var hintContent = $"<div title='{WebUtility.HtmlEncode(hintResource)}' data-toggle='tooltip' class='ico-help'><i class='fa fa-question-circle'></i></div>";
                ////////            output.Content.AppendHtml(hintContent);
                ////////        }
                ////////    }
                ////////}
            }
        }
    }
}