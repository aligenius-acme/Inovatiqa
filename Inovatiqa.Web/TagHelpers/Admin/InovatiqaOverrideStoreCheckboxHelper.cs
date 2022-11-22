using System;
using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Web.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Inovatiqa.Web.Framework.TagHelpers.Admin
{
    [HtmlTargetElement("nop-override-store-checkbox", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public class InovatiqaOverrideStoreCheckboxHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string InputAttributeName = "asp-input";
        private const string Input2AttributeName = "asp-input2";
        private const string StoreScopeAttributeName = "asp-store-scope";
        private const string ParentContainerAttributeName = "asp-parent-container";

        private readonly IHtmlHelper _htmlHelper;

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName(InputAttributeName)]
        public ModelExpression Input { set; get; }

        [HtmlAttributeName(Input2AttributeName)]
        public ModelExpression Input2 { set; get; }

        [HtmlAttributeName(StoreScopeAttributeName)]
        public int StoreScope { set; get; }

        [HtmlAttributeName(ParentContainerAttributeName)]
        public string ParentContainer { set; get; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public InovatiqaOverrideStoreCheckboxHelper(IHtmlHelper htmlHelper)
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

            if (StoreScope > 0)
            {
                var viewContextAware = _htmlHelper as IViewContextAware;
                viewContextAware?.Contextualize(ViewContext);

                var dataInputIds = new List<string>();
                if (Input != null)
                    dataInputIds.Add(_htmlHelper.Id(Input.Name));
                if (Input2 != null)
                    dataInputIds.Add(_htmlHelper.Id(Input2.Name));

                const string cssClass = "multi-store-override-option";
                var dataInputSelector = "";
                if (!string.IsNullOrEmpty(ParentContainer))
                {
                    dataInputSelector = "#" + ParentContainer + " input, #" + ParentContainer + " textarea, #" + ParentContainer + " select";
                }
                if (dataInputIds.Any())
                {
                    dataInputSelector = "#" + string.Join(", #", dataInputIds);
                }
                var onClick = $"checkOverriddenStoreValue(this, '{dataInputSelector}')";

                var htmlAttributes = new
                {
                    @class = cssClass,
                    onclick = onClick,
                    data_for_input_selector = dataInputSelector
                };
                var htmlOutput = _htmlHelper.CheckBox(For.Name, null, htmlAttributes);
                output.Content.SetHtmlContent(htmlOutput.RenderHtmlContent());
            }
        }
    }
}