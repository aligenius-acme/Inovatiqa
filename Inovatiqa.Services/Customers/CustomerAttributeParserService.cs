using Inovatiqa.Database.Models;
using Inovatiqa.Services.Customers.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace Inovatiqa.Services.Customers
{
    public partial class CustomerAttributeParserService : ICustomerAttributeParserService
    {
        #region Fields

        private readonly ICustomerAttributeService _customerAttributeService;

        #endregion

        #region Ctor

        public CustomerAttributeParserService(ICustomerAttributeService customerAttributeService)
        {
            _customerAttributeService = customerAttributeService;
        }

        #endregion

        #region Utilities

        protected virtual IList<int> ParseCustomerAttributeIds(string attributesXml)
        {
            var ids = new List<int>();
            if (string.IsNullOrEmpty(attributesXml))
                return ids;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                foreach (XmlNode node in xmlDoc.SelectNodes(@"//Attributes/CustomerAttribute"))
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

        public virtual IList<CustomerAttributeValue> ParseCustomerAttributeValues(string attributesXml)
        {
            var values = new List<CustomerAttributeValue>();
            if (string.IsNullOrEmpty(attributesXml))
                return values;

            var attributes = ParseCustomerAttributes(attributesXml);
            foreach (var attribute in attributes)
            {
                if (!attribute.ShouldHaveValues())
                    continue;

                var valuesStr = ParseValues(attributesXml, attribute.Id);
                foreach (var valueStr in valuesStr)
                {
                    if (string.IsNullOrEmpty(valueStr))
                        continue;

                    if (!int.TryParse(valueStr, out var id))
                        continue;

                    var value = _customerAttributeService.GetCustomerAttributeValueById(id);
                    if (value != null)
                        values.Add(value);
                }
            }

            return values;
        }

        public virtual IList<CustomerAttribute> ParseCustomerAttributes(string attributesXml)
        {
            var result = new List<CustomerAttribute>();
            if (string.IsNullOrEmpty(attributesXml))
                return result;

            var ids = ParseCustomerAttributeIds(attributesXml);
            foreach (var id in ids)
            {
                var attribute = _customerAttributeService.GetCustomerAttributeById(id);
                if (attribute != null)
                {
                    result.Add(attribute);
                }
            }

            return result;
        }

        public virtual IList<string> ParseValues(string attributesXml, int customerAttributeId)
        {
            var selectedCustomerAttributeValues = new List<string>();
            if (string.IsNullOrEmpty(attributesXml))
                return selectedCustomerAttributeValues;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/CustomerAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["ID"] == null)
                        continue;

                    var str1 = node1.Attributes["ID"].InnerText.Trim();

                    if (!int.TryParse(str1, out var id))
                        continue;

                    if (id != customerAttributeId)
                        continue;

                    var nodeList2 = node1.SelectNodes(@"CustomerAttributeValue/Value");
                    foreach (XmlNode node2 in nodeList2)
                    {
                        var value = node2.InnerText.Trim();
                        selectedCustomerAttributeValues.Add(value);
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return selectedCustomerAttributeValues;
        }

        public virtual string AddCustomerAttribute(string attributesXml, CustomerAttribute ca, string value)
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

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/CustomerAttribute");
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
                    attributeElement = xmlDoc.CreateElement("CustomerAttribute");
                    attributeElement.SetAttribute("ID", ca.Id.ToString());
                    rootElement.AppendChild(attributeElement);
                }

                var attributeValueElement = xmlDoc.CreateElement("CustomerAttributeValue");
                attributeElement.AppendChild(attributeValueElement);

                var attributeValueValueElement = xmlDoc.CreateElement("Value");
                if (ca.Name == "Attachment")
                {
                    //byte[] bytes = new byte[value.Length * sizeof(char)];
                    //System.Buffer.BlockCopy(value.ToCharArray(), 0, bytes, 0, bytes.Length);
                    attributeValueValueElement.InnerText = "<![CDATA[" + value + "]]>";
                }
                else
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

        public virtual IList<string> GetAttributeWarnings(string attributesXml)
        {
            var warnings = new List<string>();

            var attributes1 = ParseCustomerAttributes(attributesXml);

            var attributes2 = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var a2 in attributes2)
            {
                if (!a2.IsRequired)
                    continue;

                var found = false;

                foreach (var a1 in attributes1)
                {
                    if (a1.Id != a2.Id)
                        continue;

                    var valuesStr = ParseValues(attributesXml, a1.Id);

                    found = valuesStr.Any(str1 => !string.IsNullOrEmpty(str1.Trim()));
                }

                if (found)
                    continue;

                var notFoundWarning = string.Format("Please select {0}", a2.Name);

                warnings.Add(notFoundWarning);
            }

            return warnings;
        }

        #endregion
    }
}