using Inovatiqa.Core;
using Inovatiqa.Services.Common.Interfaces;
using System.Net;
using System.Text;

namespace Inovatiqa.Services.Common
{
    public partial class AddressAttributeFormatterService : IAddressAttributeFormatterService
    {
        #region Fields

        private readonly IAddressAttributeParserService _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;

        #endregion

        #region Ctor

        public AddressAttributeFormatterService(IAddressAttributeParserService addressAttributeParser,
            IAddressAttributeService addressAttributeService)
        {
            _addressAttributeParser = addressAttributeParser;
            _addressAttributeService = addressAttributeService;
        }

        #endregion

        #region Methods

        public virtual string FormatAttributes(string attributesXml,
            string separator = "<br />",
            bool htmlEncode = true)
        {
            var result = new StringBuilder();

            var attributes = _addressAttributeParser.ParseAddressAttributes(attributesXml);
            for (var i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];
                var valuesStr = _addressAttributeParser.ParseValues(attributesXml, attribute.Id);
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
                    else
                    {
                        if (int.TryParse(valueStr, out var attributeValueId))
                        {
                            var attributeValue = _addressAttributeService.GetAddressAttributeValueById(attributeValueId);
                            if (attributeValue != null)
                            {
                                formattedAttribute = $"{attribute.Name}: {attributeValue.Name}";
                            }
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