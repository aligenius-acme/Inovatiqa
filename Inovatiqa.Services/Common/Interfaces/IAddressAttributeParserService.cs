using System.Collections.Generic;
using Inovatiqa.Database.Models;
using Microsoft.AspNetCore.Http;

namespace Inovatiqa.Services.Common.Interfaces
{
    public partial interface IAddressAttributeParserService
    {
        IList<AddressAttribute> ParseAddressAttributes(string attributesXml);

        IList<AddressAttributeValue> ParseAddressAttributeValues(string attributesXml);

        IList<string> ParseValues(string attributesXml, int addressAttributeId);

        string AddAddressAttribute(string attributesXml, AddressAttribute attribute, string value);

        IList<string> GetAttributeWarnings(string attributesXml);

        string ParseCustomAddressAttributes(IFormCollection form);
    }
}