using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using System.Net;
using System.Text;

namespace Inovatiqa.Services.Orders
{
    public partial class CheckoutAttributeFormatterService : ICheckoutAttributeFormatterService
    {
        #region Fields

        private readonly ICheckoutAttributeParserService _checkoutAttributeParser;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContextService _workContextService;

        #endregion

        #region Ctor

        public CheckoutAttributeFormatterService(ICheckoutAttributeParserService checkoutAttributeParser,
            ICheckoutAttributeService checkoutAttributeService,
            IPriceFormatter priceFormatter,
            IWebHelper webHelper,
            IWorkContextService workContextService)
        {
            _checkoutAttributeParser = checkoutAttributeParser;
            _checkoutAttributeService = checkoutAttributeService;
            _priceFormatter = priceFormatter;
            _webHelper = webHelper;
            _workContextService = workContextService;
        }

        #endregion

        #region Methods

        public virtual string FormatAttributes(string attributesXml)
        {
            var customer = _workContextService.CurrentCustomer;
            return FormatAttributes(attributesXml, customer);
        }

        public virtual string FormatAttributes(string attributesXml,
            Customer customer,
            string separator = "<br />",
            bool htmlEncode = true,
            bool renderPrices = true,
            bool allowHyperlinks = true)
        {
            var result = new StringBuilder();

            var attributes = _checkoutAttributeParser.ParseCheckoutAttributes(attributesXml);
            for (var i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];
                var valuesStr = _checkoutAttributeParser.ParseValues(attributesXml, attribute.Id);
                for (var j = 0; j < valuesStr.Count; j++)
                {
                    var valueStr = valuesStr[j];
                    var formattedAttribute = string.Empty;
                    if (!attribute.ShouldHaveValues())
                    {
                        if (attribute.AttributeControlTypeId == (int)AttributeControlType.MultilineTextbox)
                        {
                            var attributeName = attribute.Name;
                            if (htmlEncode)
                                attributeName = WebUtility.HtmlEncode(attributeName);
                            formattedAttribute = $"{attributeName}: {HtmlHelper.FormatText(valueStr, false, true, false, false, false, false)}";
                        }
                        else
                        {
                            formattedAttribute = $"{attribute.Name}: {valueStr}";
                            if (htmlEncode)
                                formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);
                        }
                    }
                    if (string.IsNullOrEmpty(formattedAttribute))
                        continue;

                    if (i != 0 || j != 0)
                        result.Append(separator);
                    result.Append(formattedAttribute);
                }
            }

            return result.ToString();
        }

        #endregion
    }
}