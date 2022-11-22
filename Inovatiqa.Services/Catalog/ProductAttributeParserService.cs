using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Inovatiqa.Core;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Extensions;
using Inovatiqa.Services.Catalog.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Inovatiqa.Services.Catalog
{
    public partial class ProductAttributeParserService : IProductAttributeParserService
    {

        #region Fields

        private readonly IRepository<ProductAttributeValue> _productAttributeValueRepository;
        private readonly IProductAttributeService _productAttributeService;

        #endregion

        #region Ctor

        public ProductAttributeParserService(IRepository<ProductAttributeValue> productAttributeValueRepository,
            IProductAttributeService productAttributeService)
        {
            _productAttributeValueRepository = productAttributeValueRepository;
            _productAttributeService = productAttributeService;
        }

        #endregion

        #region Utilities

        protected virtual IList<int> ParseProductAttributeMappingIds(string attributesXml)
        {
            var ids = new List<int>();
            if (string.IsNullOrEmpty(attributesXml))
                return ids;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/ProductAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["ID"] == null)
                        continue;

                    var str1 = node1.Attributes["ID"].InnerText.Trim();
                    if (int.TryParse(str1, out var id))
                    {
                        ids.Add(id);
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return ids;
        }

        protected IList<Tuple<string, string>> ParseValuesWithQuantity(string attributesXml, int productAttributeMappingId)
        {
            var selectedValues = new List<Tuple<string, string>>();
            if (string.IsNullOrEmpty(attributesXml))
                return selectedValues;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                foreach (XmlNode attributeNode in xmlDoc.SelectNodes(@"//Attributes/ProductAttribute"))
                {
                    if (attributeNode.Attributes?["ID"] == null)
                        continue;

                    if (!int.TryParse(attributeNode.Attributes["ID"].InnerText.Trim(), out var attributeId) ||
                        attributeId != productAttributeMappingId)
                        continue;

                    foreach (XmlNode attributeValue in attributeNode.SelectNodes("ProductAttributeValue"))
                    {
                        var value = attributeValue.SelectSingleNode("Value").InnerText.Trim();
                        var quantityNode = attributeValue.SelectSingleNode("Quantity");
                        selectedValues.Add(new Tuple<string, string>(value, quantityNode != null ? quantityNode.InnerText.Trim() : string.Empty));
                    }
                }
            }
            catch
            {
            }

            return selectedValues;
        }

        protected virtual string GetProductAttributesXml(Product product, IFormCollection form, List<string> errors)
        {
            var attributesXml = string.Empty;
            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            foreach (var attribute in productAttributes)
            {
                var controlId = $"{InovatiqaDefaults.ProductAttributePrefix}{attribute.Id}";
                switch (attribute.AttributeControlTypeId)
                {
                    case (int)AttributeControlType.DropdownList:
                    case (int)AttributeControlType.RadioList:
                    case (int)AttributeControlType.ColorSquares:
                    case (int)AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (String.IsNullOrEmpty(ctrlAttributes))
                            {
                                var attrValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                                if (attrValues != null && attrValues.Count == 1)
                                    ctrlAttributes = attrValues.FirstOrDefault().Id.ToString();
                                else if (attrValues != null && attrValues.Where(x => x.Msku == product.Msku).FirstOrDefault() != null)
                                    ctrlAttributes = attrValues.Where(x => x.Msku == product.Msku).FirstOrDefault().Id.ToString();
                                // ADDED BY ALI AHMAD TO HANDLE UOM NOT FOUND ISSUE WHEN ADDING ITEM TO CART BY SKU
                                else if (attrValues != null && attrValues.Where(x => x.PriceAdjustment == 0).FirstOrDefault() != null)
                                    ctrlAttributes = attrValues.Where(x => x.PriceAdjustment == 0).FirstOrDefault().Id.ToString(); // GET THE DEFAULT ATTRIBUTE VALUE IF NOT SELECTED ANY IN FORM
                            }
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes) && ctrlAttributes.Count > 1)
                                ctrlAttributes = ctrlAttributes.FirstOrDefault();
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                {
                                    var quantity = 1;
                                    var quantityStr = form[$"{InovatiqaDefaults.ProductAttributePrefix}{attribute.Id}_{selectedAttributeId}_qty"];
                                    if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                        (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                        errors.Add("Quantity should be positive");

                                    attributesXml = AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                                }
                            }
                        }
                        break;
                    case (int)AttributeControlType.Checkboxes:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                foreach (var item in ctrlAttributes.ToString()
                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                    {
                                        var quantity = 1;
                                        var quantityStr = form[$"{InovatiqaDefaults.ProductAttributePrefix}{attribute.Id}_{item}_qty"];
                                        if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                            (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                            errors.Add("Quantity should be positive");

                                        attributesXml = AddProductAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                                    }
                                }
                            }
                        }
                        break;
                    case (int)AttributeControlType.ReadonlyCheckboxes:
                        {
                            var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                var quantity = 1;
                                var quantityStr = form[$"{InovatiqaDefaults.ProductAttributePrefix}{attribute.Id}_{selectedAttributeId}_qty"];
                                if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                    (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                    errors.Add("Quantity should be positive");

                                attributesXml = AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                            }
                        }
                        break;
                    case (int)AttributeControlType.TextBox:
                    case (int)AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = AddProductAttribute(attributesXml, attribute, enteredText);
                            }
                        }
                        break;
                    case (int)AttributeControlType.Datepicker:
                        {
                            var day = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                            }
                            catch
                            {
                            }
                            if (selectedDate.HasValue)
                                attributesXml = AddProductAttribute(attributesXml, attribute, selectedDate.Value.ToString("D"));
                        }
                        break;
                    default:
                        break;
                }
            }
            foreach (var attribute in productAttributes)
            {
                var conditionMet = IsConditionMet(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = RemoveProductAttribute(attributesXml, attribute);
                }
            }
            return attributesXml;
        }
        #endregion

        #region Product attributes

        public virtual ProductAttributeCombination FindProductAttributeCombination(Product product,
            string attributesXml, bool ignoreNonCombinableAttributes = true)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (string.IsNullOrEmpty(attributesXml))
                return null;

            var combinations = _productAttributeService.GetAllProductAttributeCombinations(product.Id);
            return combinations.FirstOrDefault(x =>
                AreProductAttributesEqual(x.AttributesXml, attributesXml, ignoreNonCombinableAttributes));
        }

        public virtual bool AreProductAttributesEqual(string attributesXml1, string attributesXml2, bool ignoreNonCombinableAttributes, bool ignoreQuantity = true)
        {
            var attributes1 = ParseProductAttributeMappings(attributesXml1);
            if (ignoreNonCombinableAttributes)
            {
                attributes1 = attributes1.Where(x => !x.IsNonCombinable()).ToList();
            }

            var attributes2 = ParseProductAttributeMappings(attributesXml2);
            if (ignoreNonCombinableAttributes)
            {
                attributes2 = attributes2.Where(x => !x.IsNonCombinable()).ToList();
            }

            if (attributes1.Count != attributes2.Count)
                return false;

            var attributesEqual = true;
            foreach (var a1 in attributes1)
            {
                var hasAttribute = false;
                foreach (var a2 in attributes2)
                {
                    if (a1.Id != a2.Id)
                        continue;

                    hasAttribute = true;
                    var values1Str = ParseValuesWithQuantity(attributesXml1, a1.Id);
                    var values2Str = ParseValuesWithQuantity(attributesXml2, a2.Id);
                    if (values1Str.Count == values2Str.Count)
                    {
                        foreach (var str1 in values1Str)
                        {
                            var hasValue = false;
                            foreach (var str2 in values2Str)
                            {
                                if (str1.Item1.Trim() != str2.Item1.Trim())
                                    continue;

                                hasValue = ignoreQuantity || str1.Item2.Trim() == str2.Item2.Trim();
                                break;
                            }

                            if (hasValue)
                                continue;

                            attributesEqual = false;
                            break;
                        }
                    }
                    else
                    {
                        attributesEqual = false;
                        break;
                    }
                }

                if (hasAttribute)
                    continue;

                attributesEqual = false;
                break;
            }

            return attributesEqual;
        }

        public virtual IList<ProductProductAttributeMapping> ParseProductAttributeMappings(string attributesXml)
        {
            var result = new List<ProductProductAttributeMapping>();
            if (string.IsNullOrEmpty(attributesXml))
                return result;

            var ids = ParseProductAttributeMappingIds(attributesXml);
            foreach (var id in ids)
            {
                var attribute = _productAttributeService.GetProductAttributeMappingById(id);
                if (attribute != null)
                {
                    result.Add(attribute);
                }
            }

            return result;
        }

        public virtual IList<ProductAttributeValue> ParseProductAttributeValues(string attributesXml, int productAttributeMappingId = 0)
        {
            var values = new List<ProductAttributeValue>();
            if (string.IsNullOrEmpty(attributesXml))
                return values;

            var attributes = ParseProductAttributeMappings(attributesXml);

            if (productAttributeMappingId > 0)
                attributes = attributes.Where(attribute => attribute.Id == productAttributeMappingId).ToList();

            foreach (var attribute in attributes)
            {
                if (!attribute.ShouldHaveValues())
                    continue;

                foreach (var attributeValue in ParseValuesWithQuantity(attributesXml, attribute.Id))
                {
                    if (string.IsNullOrEmpty(attributeValue.Item1) || !int.TryParse(attributeValue.Item1, out var attributeValueId))
                        continue;

                    var value = _productAttributeService.GetProductAttributeValueById(attributeValueId);
                    if (value == null)
                        continue;

                    if (!string.IsNullOrEmpty(attributeValue.Item2) && int.TryParse(attributeValue.Item2, out var quantity) && quantity != value.Quantity)
                    {
                        var oldValue = _productAttributeValueRepository.GetById(value.Id);

                        oldValue.ProductAttributeMappingId = attribute.Id;
                        oldValue.Quantity = quantity;
                        values.Add(oldValue);
                    }
                    else
                        values.Add(value);
                }
            }

            return values;
        }

        public virtual IList<string> ParseValues(string attributesXml, int productAttributeMappingId)
        {
            var selectedValues = new List<string>();
            if (string.IsNullOrEmpty(attributesXml))
                return selectedValues;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/ProductAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["ID"] == null)
                        continue;

                    var str1 = node1.Attributes["ID"].InnerText.Trim();
                    if (!int.TryParse(str1, out var id))
                        continue;

                    if (id != productAttributeMappingId)
                        continue;

                    var nodeList2 = node1.SelectNodes(@"ProductAttributeValue/Value");
                    foreach (XmlNode node2 in nodeList2)
                    {
                        var value = node2.InnerText.Trim();
                        selectedValues.Add(value);
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return selectedValues;
        }

        public virtual string AddProductAttribute(string attributesXml, ProductProductAttributeMapping productAttributeMapping, string value, int? quantity = null)
        {
            var result = string.Empty;
            try
            {
                var xmlDoc = new XmlDocument();
                if (string.IsNullOrEmpty(attributesXml))
                {
                    var element1 = xmlDoc.CreateElement("Attributes");
                    xmlDoc.AppendChild(element1);
                }
                else
                {
                    xmlDoc.LoadXml(attributesXml);
                }

                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes");

                XmlElement attributeElement = null;
                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/ProductAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["ID"] == null)
                        continue;

                    var str1 = node1.Attributes["ID"].InnerText.Trim();
                    if (!int.TryParse(str1, out var id))
                        continue;

                    if (id != productAttributeMapping.Id)
                        continue;

                    attributeElement = (XmlElement)node1;
                    break;
                }

                if (attributeElement == null)
                {
                    attributeElement = xmlDoc.CreateElement("ProductAttribute");
                    attributeElement.SetAttribute("ID", productAttributeMapping.Id.ToString());
                    rootElement.AppendChild(attributeElement);
                }

                var attributeValueElement = xmlDoc.CreateElement("ProductAttributeValue");
                attributeElement.AppendChild(attributeValueElement);

                var attributeValueValueElement = xmlDoc.CreateElement("Value");
                attributeValueValueElement.InnerText = value;
                attributeValueElement.AppendChild(attributeValueValueElement);

                if (quantity.HasValue)
                {
                    var attributeValueQuantity = xmlDoc.CreateElement("Quantity");
                    attributeValueQuantity.InnerText = quantity.ToString();
                    attributeValueElement.AppendChild(attributeValueQuantity);
                }

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return result;
        }

        public virtual bool? IsConditionMet(ProductProductAttributeMapping pam, string selectedAttributesXml)
        {
            if (pam == null)
                throw new ArgumentNullException(nameof(pam));

            var conditionAttributeXml = pam.ConditionAttributeXml;
            if (string.IsNullOrEmpty(conditionAttributeXml))
                return null;

            var dependOnAttribute = ParseProductAttributeMappings(conditionAttributeXml).FirstOrDefault();
            if (dependOnAttribute == null)
                return true;

            var valuesThatShouldBeSelected = ParseValues(conditionAttributeXml, dependOnAttribute.Id)
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            var selectedValues = ParseValues(selectedAttributesXml, dependOnAttribute.Id);
            if (valuesThatShouldBeSelected.Count != selectedValues.Count)
                return false;

            var allFound = true;
            foreach (var t1 in valuesThatShouldBeSelected)
            {
                var found = false;
                foreach (var t2 in selectedValues)
                    if (t1 == t2)
                        found = true;
                if (!found)
                    allFound = false;
            }

            return allFound;
        }

        public virtual string ParseProductAttributes(Product product, IFormCollection form, List<string> errors)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = GetProductAttributesXml(product, form, errors);


            return attributesXml;
        }

        public virtual string RemoveProductAttribute(string attributesXml, ProductProductAttributeMapping productAttributeMapping)
        {
            var result = string.Empty;
            try
            {
                var xmlDoc = new XmlDocument();
                if (string.IsNullOrEmpty(attributesXml))
                {
                    var element1 = xmlDoc.CreateElement("Attributes");
                    xmlDoc.AppendChild(element1);
                }
                else
                {
                    xmlDoc.LoadXml(attributesXml);
                }

                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes");

                XmlElement attributeElement = null;
                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/ProductAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["ID"] == null)
                        continue;

                    var str1 = node1.Attributes["ID"].InnerText.Trim();
                    if (!int.TryParse(str1, out var id))
                        continue;

                    if (id != productAttributeMapping.Id)
                        continue;

                    attributeElement = (XmlElement)node1;
                    break;
                }

                if (attributeElement != null)
                {
                    rootElement.RemoveChild(attributeElement);
                }

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return result;
        }

        public virtual int ParseEnteredQuantity(Product product, IFormCollection form)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var quantity = 1;
            foreach (var formKey in form.Keys)
            {
                if (formKey.Equals($"addtocart_{product.Id}.EnteredQuantity", StringComparison.InvariantCultureIgnoreCase))
                {
                    int.TryParse(form[formKey], out quantity);
                    break;
                }
            }

            return quantity;
        }

        #endregion

        #region Gift card attributes



        #endregion
    }
}