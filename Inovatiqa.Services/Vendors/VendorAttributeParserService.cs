using Inovatiqa.Database.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace Inovatiqa.Services.Vendors
{
    public partial class VendorAttributeParserService : IVendorAttributeParserService
    {
        #region Fields

        private readonly IVendorAttributeService _vendorAttributeService;

        #endregion

        #region Ctor

        public VendorAttributeParserService(IVendorAttributeService vendorAttributeService)
        {
            _vendorAttributeService = vendorAttributeService;
        }

        #endregion

        #region Utilities

        protected virtual IList<int> ParseVendorAttributeIds(string attributesXml)
        {
            var ids = new List<int>();
            if (string.IsNullOrEmpty(attributesXml))
                return ids;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                foreach (XmlNode node in xmlDoc.SelectNodes(@"//Attributes/VendorAttribute"))
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

        public virtual IList<VendorAttribute> ParseVendorAttributes(string attributesXml)
        {
            var result = new List<VendorAttribute>();
            if (string.IsNullOrEmpty(attributesXml))
                return result;

            var ids = ParseVendorAttributeIds(attributesXml);
            foreach (var id in ids)
            {
                var attribute = _vendorAttributeService.GetVendorAttributeById(id);
                if (attribute != null)
                {
                    result.Add(attribute);
                }
            }

            return result;
        }

        public virtual IList<VendorAttributeValue> ParseVendorAttributeValues(string attributesXml)
        {
            var values = new List<VendorAttributeValue>();
            if (string.IsNullOrEmpty(attributesXml))
                return values;

            var attributes = ParseVendorAttributes(attributesXml);
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

                    var value = _vendorAttributeService.GetVendorAttributeValueById(id);
                    if (value != null)
                        values.Add(value);
                }
            }

            return values;
        }

        public virtual IList<string> ParseValues(string attributesXml, int vendorAttributeId)
        {
            var selectedVendorAttributeValues = new List<string>();
            if (string.IsNullOrEmpty(attributesXml))
                return selectedVendorAttributeValues;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/VendorAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["ID"] == null) 
                        continue;

                    var str1 = node1.Attributes["ID"].InnerText.Trim();
                    if (!int.TryParse(str1, out var id)) 
                        continue;

                    if (id != vendorAttributeId) 
                        continue;

                    var nodeList2 = node1.SelectNodes(@"VendorAttributeValue/Value");
                    foreach (XmlNode node2 in nodeList2)
                    {
                        var value = node2.InnerText.Trim();
                        selectedVendorAttributeValues.Add(value);
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return selectedVendorAttributeValues;
        }

        public virtual string AddVendorAttribute(string attributesXml, VendorAttribute vendorAttribute, string value)
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
                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/VendorAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["ID"] == null) 
                        continue;

                    var str1 = node1.Attributes["ID"].InnerText.Trim();
                    if (!int.TryParse(str1, out var id)) 
                        continue;

                    if (id != vendorAttribute.Id) 
                        continue;

                    attributeElement = (XmlElement)node1;
                    break;
                }

                if (attributeElement == null)
                {
                    attributeElement = xmlDoc.CreateElement("VendorAttribute");
                    attributeElement.SetAttribute("ID", vendorAttribute.Id.ToString());
                    rootElement.AppendChild(attributeElement);
                }

                var attributeValueElement = xmlDoc.CreateElement("VendorAttributeValue");
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

        public virtual IList<string> GetAttributeWarnings(string attributesXml)
        {
            var warnings = new List<string>();

            var attributes1 = ParseVendorAttributes(attributesXml);

            var attributes2 = _vendorAttributeService.GetAllVendorAttributes();
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
                    foreach (var str1 in valuesStr)
                    {
                        if (string.IsNullOrEmpty(str1.Trim())) 
                            continue;

                        found = true;
                        break;
                    }
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