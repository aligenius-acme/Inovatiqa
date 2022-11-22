using System;
using System.Collections.Generic;
using Inovatiqa.Web.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Inovatiqa.Web.Framework.TagHelpers.Admin
{
    [HtmlTargetElement("nop-select", TagStructure = TagStructure.WithoutEndTag)]
    public class InovatiqaSelectTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string NameAttributeName = "asp-for-name";
        private const string ItemsAttributeName = "asp-items";
        private const string DisabledAttributeName = "asp-multiple";
        private const string RequiredAttributeName = "asp-required";

        private readonly IHtmlHelper _htmlHelper;

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName(NameAttributeName)]
        public string Name { get; set; }

        [HtmlAttributeName(ItemsAttributeName)]
        public IEnumerable<SelectListItem> Items { set; get; } = new List<SelectListItem>();

        [HtmlAttributeName(RequiredAttributeName)]
        public string IsRequired { set; get; }

        [HtmlAttributeName(DisabledAttributeName)]
        public string IsMultiple { set; get; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public InovatiqaSelectTagHelper(IHtmlHelper htmlHelper)
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

            bool.TryParse(IsRequired, out bool required);
            if (required)
            {
                output.PreElement.SetHtmlContent("<div class='input-group input-group-required'>");
                output.PostElement.SetHtmlContent("<div class=\"input-group-btn\"><span class=\"required\">*</span></div></div>");
            }

            var viewContextAware = _htmlHelper as IViewContextAware;
            viewContextAware?.Contextualize(ViewContext);

            var htmlAttributes = new Dictionary<string, object>();
            var attributes = context.AllAttributes;
            foreach (var attribute in attributes)
            {
                if (!attribute.Name.Equals(ForAttributeName) &&
                    !attribute.Name.Equals(NameAttributeName) &&
                    !attribute.Name.Equals(ItemsAttributeName) &&
                    !attribute.Name.Equals(DisabledAttributeName) &&
                    !attribute.Name.Equals(RequiredAttributeName))
                {
                    htmlAttributes.Add(attribute.Name, attribute.Value);
                }
            }

            var tagName = For != null ? For.Name : Name;
            bool.TryParse(IsMultiple, out bool multiple);
            if (!string.IsNullOrEmpty(tagName))
            {
                IHtmlContent selectList;
                if (multiple)
                {
                    selectList = _htmlHelper.Editor(tagName, "MultiSelect", new {htmlAttributes, SelectList = Items});
                }
                else
                {
                    if (htmlAttributes.ContainsKey("class"))
                        htmlAttributes["class"] += " form-control";
                    else
                        htmlAttributes.Add("class", "form-control");

                    selectList = _htmlHelper.DropDownList(tagName, Items, htmlAttributes);
                }
                output.Content.SetHtmlContent(selectList.RenderHtmlContent());
            }
        }
    }
}