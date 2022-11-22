using Inovatiqa.Database.Models;
using Inovatiqa.Services.Orders.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace Inovatiqa.Services.Orders
{
    public partial class CheckoutAttributeParserService : ICheckoutAttributeParserService
    {
        #region Fields

        private readonly ICheckoutAttributeService _checkoutAttributeService;

        #endregion

        #region Ctor

        public CheckoutAttributeParserService(ICheckoutAttributeService checkoutAttributeService)
        {
            _checkoutAttributeService = checkoutAttributeService;
        }

        #endregion

        #region Utilities

        protected virtual IList<int> ParseCheckoutAttributeIds(string attributesXml)
        {
            var ids = new List<int>();
            if (string.IsNullOrEmpty(attributesXml))
                return ids;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                foreach (XmlNode node in xmlDoc.SelectNodes(@"//Attributes/CheckoutAttribute"))
                {
                    if (node.Attributes?["ID"] == null)
                        continue;

                    var str1 = node.Attributes["ID"].InnerText.Trim();
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

        #endregion

        #region Methods

        public virtual IList<CheckoutAttribute> ParseCheckoutAttributes(string attributesXml)
        {
            var result = new List<CheckoutAttribute>();
            if (string.IsNullOrEmpty(attributesXml))
                return result;

            var ids = ParseCheckoutAttributeIds(attributesXml);
            foreach (var id in ids)
            {
                var attribute = _checkoutAttributeService.GetCheckoutAttributeById(id);
                if (attribute != null)
                {
                    result.Add(attribute);
                }
            }

            return result;
        }

        public virtual IEnumerable<(CheckoutAttribute attribute, IEnumerable<CheckoutAttributeValue> values)> ParseCheckoutAttributeValues(string attributesXml)
        {
            if (string.IsNullOrEmpty(attributesXml))
                yield break;

            var attributes = ParseCheckoutAttributes(attributesXml);

            foreach (var attribute in attributes)
            {
                if (!attribute.ShouldHaveValues())
                    continue;

                var valuesStr = ParseValues(attributesXml, attribute.Id);

                yield return (attribute, getValues(valuesStr));
            }

            IEnumerable<CheckoutAttributeValue> getValues(IList<string> valuesStr)
            {
                foreach (var valueStr in valuesStr)
                {
                    if (string.IsNullOrEmpty(valueStr))
                        continue;

                    if (!int.TryParse(valueStr, out var id))
                        continue;

                    var value = _checkoutAttributeService.GetCheckoutAttributeValueById(id);
                    if (value != null)
                        yield return value;
                }
            }
        }

        public virtual IList<string> ParseValues(string attributesXml, int checkoutAttributeId)
        {
            var selectedCheckoutAttributeValues = new List<string>();
            if (string.IsNullOrEmpty(attributesXml))
                return selectedCheckoutAttributeValues;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/CheckoutAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["ID"] == null)
                        continue;

                    var str1 = node1.Attributes["ID"].InnerText.Trim();
                    if (!int.TryParse(str1, out var id))
                        continue;

                    if (id != checkoutAttributeId)
                        continue;

                    var nodeList2 = node1.SelectNodes(@"CheckoutAttributeValue/Value");
                    foreach (XmlNode node2 in nodeList2)
                    {
                        var value = node2.InnerText.Trim();
                        selectedCheckoutAttributeValues.Add(value);
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return selectedCheckoutAttributeValues;
        }

        public virtual string EnsureOnlyActiveAttributes(string attributesXml, IList<ShoppingCartItem> cart)
        {
            if (string.IsNullOrEmpty(attributesXml))
                return attributesXml;

            var result = attributesXml;

            var checkoutAttributeIdsToRemove = new List<int>();
            var attributes = ParseCheckoutAttributes(attributesXml);

            foreach (var ca in attributes)
                if (ca.ShippableProductRequired)
                    checkoutAttributeIdsToRemove.Add(ca.Id);

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var nodesToRemove = new List<XmlNode>();
                foreach (XmlNode node in xmlDoc.SelectNodes(@"//Attributes/CheckoutAttribute"))
                {
                    if (node.Attributes?["ID"] == null)
                        continue;

                    var str1 = node.Attributes["ID"].InnerText.Trim();

                    if (!int.TryParse(str1, out var id))
                        continue;

                    if (checkoutAttributeIdsToRemove.Contains(id))
                    {
                        nodesToRemove.Add(node);
                    }
                }

                foreach (var node in nodesToRemove)
                {
                    node.ParentNode.RemoveChild(node);
                }

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return result;
        }

        public virtual bool? IsConditionMet(CheckoutAttribute attribute, string selectedAttributesXml)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            var conditionAttributeXml = attribute.ConditionAttributeXml;
            if (string.IsNullOrEmpty(conditionAttributeXml))
                return null;

            var dependOnAttribute = ParseCheckoutAttributes(conditionAttributeXml).FirstOrDefault();
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

        public virtual string AddCheckoutAttribute(string attributesXml, CheckoutAttribute ca, string value)
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

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/CheckoutAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["ID"] == null)
                        continue;

                    var str1 = node1.Attributes["ID"].InnerText.Trim();

                    if (!int.TryParse(str1, out var id))
                        continue;

                    if (id != ca.Id)
                        continue;

                    attributeElement = (XmlElement)node1;
                    break;
                }

                if (attributeElement == null)
                {
                    attributeElement = xmlDoc.CreateElement("CheckoutAttribute");
                    attributeElement.SetAttribute("ID", ca.Id.ToString());
                    rootElement.AppendChild(attributeElement);
                }

                var attributeValueElement = xmlDoc.CreateElement("CheckoutAttributeValue");
                attributeElement.AppendChild(attributeValueElement);

                var attributeValueValueElement = xmlDoc.CreateElement("Value");
                attributeValueValueElement.InnerText = value;
                attributeValueElement.AppendChild(attributeValueValueElement);

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return result;
        }

        public virtual string RemoveCheckoutAttribute(string attributesXml, CheckoutAttribute attribute)
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

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/CheckoutAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["ID"] == null)
                        continue;

                    var str1 = node1.Attributes["ID"].InnerText.Trim();

                    if (!int.TryParse(str1, out var id))
                        continue;

                    if (id != attribute.Id)
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

        #endregion
    }
}