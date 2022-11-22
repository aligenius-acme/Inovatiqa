using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Inovatiqa.Web.Framework.TagHelpers.Admin
{
    [HtmlTargetElement("nop-editor", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public class InovatiqaEditorTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string DisabledAttributeName = "asp-disabled";
        private const string RequiredAttributeName = "asp-required";
        private const string RenderFormControlClassAttributeName = "asp-render-form-control-class";
        private const string TemplateAttributeName = "asp-template";
        private const string PostfixAttributeName = "asp-postfix";
        private const string ValueAttributeName = "asp-value";
        private const string PlaceholderAttributeName = "placeholder";

        private readonly IHtmlHelper _htmlHelper;

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName(DisabledAttributeName)]
        public string IsDisabled { set; get; }

        [HtmlAttributeName(RequiredAttributeName)]
        public string IsRequired { set; get; }

        [HtmlAttributeName(PlaceholderAttributeName)]
        public string Placeholder { set; get; }

        [HtmlAttributeName(RenderFormControlClassAttributeName)]
        public string RenderFormControlClass { set; get; }

        [HtmlAttributeName(TemplateAttributeName)]
        public string Template { set; get; }

        [HtmlAttributeName(PostfixAttributeName)]
        public string Postfix { set; get; }

        [HtmlAttributeName(ValueAttributeName)]
        public string Value { set; get; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public InovatiqaEditorTagHelper(IHtmlHelper htmlHelper)
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

            output.SuppressOutput();

            var htmlAttributes = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(Placeholder))
                htmlAttributes.Add("placeholder", Placeholder);

            if (!string.IsNullOrEmpty(Value))
                htmlAttributes.Add("value", Value);

            bool.TryParse(IsDisabled, out var disabled);
            if (disabled)
            {
                htmlAttributes.Add("disabled", "disabled");
            }

            bool.TryParse(IsRequired, out var required);
            if (required)
            {
                output.PreElement.SetHtmlContent("<div class='input-group input-group-required'>");
                output.PostElement.SetHtmlContent("<div class=\"input-group-btn\"><span class=\"required\">*</span></div></div>");
            }

            var viewContextAware = _htmlHelper as IViewContextAware;
            viewContextAware?.Contextualize(ViewContext);

            bool.TryParse(RenderFormControlClass, out var renderFormControlClass);
            if (string.IsNullOrEmpty(RenderFormControlClass) && For.Metadata.ModelType.Name.Equals("String") || renderFormControlClass)
                htmlAttributes.Add("class", "form-control");

            var pattern = $"{nameof(ILocalizedModel<object>.Locales)}" + @"(?=\[\w+\]\.)";
            if (!_htmlHelper.ViewData.ContainsKey(For.Name) && Regex.IsMatch(For.Name, pattern))
                _htmlHelper.ViewData.Add(For.Name, For.Model);

            var htmlOutput = _htmlHelper.Editor(For.Name, Template, new { htmlAttributes, postfix = Postfix });
            output.Content.SetHtmlContent(htmlOutput);
        }
    }
}