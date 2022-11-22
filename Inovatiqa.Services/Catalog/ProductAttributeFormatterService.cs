using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Extensions;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using System.Net;
using System.Text;

namespace Inovatiqa.Services.Catalog
{
    public partial class ProductAttributeFormatterService : IProductAttributeFormatterService
    {
        #region Fields

        private readonly IWorkContextService _workContextService;
        private readonly IProductAttributeParserService _productAttributeParserService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;

        #endregion

        #region Ctor

        public ProductAttributeFormatterService(IWorkContextService workContextService,
            IProductAttributeParserService productAttributeParserService,
            IProductAttributeService productAttributeService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter)
        {
            _workContextService = workContextService;
            _productAttributeParserService = productAttributeParserService;
            _productAttributeService = productAttributeService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
        }

        #endregion

        #region Methods

        public virtual string FormatAttributes(Product product, string attributesXml)
        {
            var customer = _workContextService.CurrentCustomer;
            return FormatAttributes(product, attributesXml, customer);
        }

        public virtual string FormatAttributes(Product product, string attributesXml,
            Customer customer, string separator = "<br />", bool htmlEncode = true, bool renderPrices = true,
            bool renderProductAttributes = true, bool renderGiftCardAttributes = true,
            bool allowHyperlinks = true)
        {
            var result = new StringBuilder();

            if (renderProductAttributes)
            {
                foreach (var attribute in _productAttributeParserService.ParseProductAttributeMappings(attributesXml))
                {
                    var productAttrubute = _productAttributeService.GetProductAttributeById(attribute.ProductAttributeId);
                    var attributeName = productAttrubute.Name;

                    if (!attribute.ShouldHaveValues())
                    {
                        foreach (var value in _productAttributeParserService.ParseValues(attributesXml, attribute.Id))
                        {
                            var formattedAttribute = string.Empty;
                            if (attribute.AttributeControlTypeId == (int)AttributeControlType.MultilineTextbox)
                            {
                                if (htmlEncode)
                                    attributeName = WebUtility.HtmlEncode(attributeName);

                                formattedAttribute = $"{attributeName}: {HtmlHelper.FormatText(value, false, true, false, false, false, false)}";
                            }
                            else
                            {
                                formattedAttribute = $"{attributeName}: {value}";

                                if (htmlEncode)
                                    formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);
                            }

                            if (string.IsNullOrEmpty(formattedAttribute))
                                continue;

                            if (result.Length > 0)
                                result.Append(separator);
                            result.Append(formattedAttribute);
                        }
                    }
                    else
                    {
                        foreach (var attributeValue in _productAttributeParserService.ParseProductAttributeValues(attributesXml, attribute.Id))
                        {
                            var formattedAttribute = $"{attributeName}: {attributeValue.Name}";

                            if (renderPrices)
                            {
                                if (attributeValue.PriceAdjustmentUsePercentage)
                                {
                                    if (attributeValue.PriceAdjustment > decimal.Zero)
                                    {
                                        formattedAttribute += string.Format(
                                                " [{0}{1}{2}]",
                                                "+", attributeValue.PriceAdjustment.ToString("G29"), "%");
                                    }
                                    else if (attributeValue.PriceAdjustment < decimal.Zero)
                                    {
                                        formattedAttribute += string.Format(
                                                " [{0}{1}{2}]",
                                                string.Empty, attributeValue.PriceAdjustment.ToString("G29"), "%");
                                    }
                                }
                                else
                                {
                                    var attributeValuePriceAdjustment = _priceCalculationService.GetProductAttributeValuePriceAdjustment(product, attributeValue, customer);
                                    var priceAdjustmentBase = attributeValuePriceAdjustment;
                                    var priceAdjustment = priceAdjustmentBase;

                                    if (priceAdjustmentBase > decimal.Zero)
                                    {
                                        formattedAttribute += string.Format(
                                                " [{0}{1}{2}]",
                                                "+", _priceFormatter.FormatPrice(priceAdjustment), string.Empty);
                                    }
                                    else if (priceAdjustmentBase < decimal.Zero)
                                    {
                                        formattedAttribute += string.Format(
                                                " [{0}{1}{2}]",
                                                "-", _priceFormatter.FormatPrice(-priceAdjustment), string.Empty);
                                    }
                                }
                            }

                            if (InovatiqaDefaults.RenderAssociatedAttributeValueQuantity && attributeValue.AttributeValueTypeId == InovatiqaDefaults.AssociatedToProduct)
                            {
                                if (attributeValue.Quantity > 1)
                                    formattedAttribute += string.Format(" - quantity {0}", attributeValue.Quantity);
                            }

                            if (htmlEncode)
                                formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);

                            if (string.IsNullOrEmpty(formattedAttribute))
                                continue;

                            if (result.Length > 0)
                                result.Append(separator);
                            result.Append(formattedAttribute);
                        }
                    }
                }
            }


            return result.ToString();
        }

        #endregion
    }
}