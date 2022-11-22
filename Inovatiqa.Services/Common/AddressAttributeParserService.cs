using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Inovatiqa.Services.Common
{
    public partial class AddressAttributeParserService : IAddressAttributeParserService
    {
        #region Fields

        private readonly IAddressAttributeService _addressAttributeService;

        #endregion

        #region Ctor

        public AddressAttributeParserService(IAddressAttributeService addressAttributeService)
        {
            _addressAttributeService = addressAttributeService;
        }

        #endregion

        #region Utilities

        protected virtual IList<int> ParseAddressAttributeIds(string attributesXml)
        {
            var ids = new List<int>();
            if (string.IsNullOrEmpty(attributesXml))
                return ids;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                foreach (XmlNode node in xmlDoc.SelectNodes(@"//Attributes/AddressAttribute"))
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

        public virtual IList<AddressAttribute> ParseAddressAttributes(string attributesXml)
        {
            var result = new List<AddressAttribute>();
            if (string.IsNullOrEmpty(attributesXml))
                return result;

            var ids = ParseAddressAttributeIds(attributesXml);
            foreach (var id in ids)
            {
                var attribute = _addressAttributeService.GetAddressAttributeById(id);
                if (attribute != null)
                {
                    result.Add(attribute);
                }
            }

            return result;
        }

        public virtual IList<AddressAttributeValue> ParseAddressAttributeValues(string attributesXml)
        {
            var values = new List<AddressAttributeValue>();
            if (string.IsNullOrEmpty(attributesXml))
                return values;

            var attributes = ParseAddressAttributes(attributesXml);
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

                    var value = _addressAttributeService.GetAddressAttributeValueById(id);
                    if (value != null)
                        values.Add(value);
                }
            }

            return values;
        }

        public virtual IList<string> ParseValues(string attributesXml, int addressAttributeId)
        {
            var selectedAddressAttributeValues = new List<string>();
            if (string.IsNullOrEmpty(attributesXml))
                return selectedAddressAttributeValues;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/AddressAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["ID"] == null) 
                        continue;

                    var str1 = node1.Attributes["ID"].InnerText.Trim();
                    if (!int.TryParse(str1, out var id)) 
                        continue;

                    if (id != addressAttributeId) 
                        continue;

                    var nodeList2 = node1.SelectNodes(@"AddressAttributeValue/Value");
                    foreach (XmlNode node2 in nodeList2)
                    {
                        var value = node2.InnerText.Trim();
                        selectedAddressAttributeValues.Add(value);
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return selectedAddressAttributeValues;
        }

        public virtual string AddAddressAttribute(string attributesXml, AddressAttribute attribute, string value)
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
                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/AddressAttribute");
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

                if (attributeElement == null)
                {
                    attributeElement = xmlDoc.CreateElement("AddressAttribute");
                    attributeElement.SetAttribute("ID", attribute.Id.ToString());
                    rootElement.AppendChild(attributeElement);
                }

                var attributeValueElement = xmlDoc.CreateElement("AddressAttributeValue");
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

            var attributes1 = ParseAddressAttributes(attributesXml);

            var attributes2 = _addressAttributeService.GetAllAddressAttributes();
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

        public virtual string ParseCustomAddressAttributes(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = string.Empty;

            foreach (var attribute in _addressAttributeService.GetAllAddressAttributes())
            {
                var controlId = string.Format(InovatiqaDefaults.AddressAttributeControlName, attribute.Id);
                var attributeValues = form[controlId];
                switch (attribute.AttributeControlTypeId)
                {
                    case (int)AttributeControlType.DropdownList:
                    case (int)AttributeControlType.RadioList:
                        if (!StringValues.IsNullOrEmpty(attributeValues) && int.TryParse(attributeValues, out var value) && value > 0)
                            attributesXml = AddAddressAttribute(attributesXml, attribute, value.ToString());
                        break;

                    case (int)AttributeControlType.Checkboxes:
                        foreach (var attributeValue in attributeValues.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (int.TryParse(attributeValue, out value) && value > 0)
                                attributesXml = AddAddressAttribute(attributesXml, attribute, value.ToString());
                        }

                        break;

                    case (int)AttributeControlType.ReadonlyCheckboxes:
                        var addressAttributeValues = _addressAttributeService.GetAddressAttributeValues(attribute.Id);
                        foreach (var addressAttributeValue in addressAttributeValues)
                        {
                            if (addressAttributeValue.IsPreSelected)
                                attributesXml = AddAddressAttribute(attributesXml, attribute, addressAttributeValue.Id.ToString());
                        }

                        break;

                    case (int)AttributeControlType.TextBox:
                    case (int)AttributeControlType.MultilineTextbox:
                        if (!StringValues.IsNullOrEmpty(attributeValues))
                            attributesXml = AddAddressAttribute(attributesXml, attribute, attributeValues.ToString().Trim());
                        break;

                    case (int)AttributeControlType.Datepicker:
                    case (int)AttributeControlType.ColorSquares:
                    case (int)AttributeControlType.ImageSquares:
                    case (int)AttributeControlType.FileUpload:
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        #endregion
    }
}