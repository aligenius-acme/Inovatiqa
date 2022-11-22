using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Vendors
{
    public partial interface IVendorAttributeParserService
    {
        IList<VendorAttribute> ParseVendorAttributes(string attributesXml);

        IList<VendorAttributeValue> ParseVendorAttributeValues(string attributesXml);

        IList<string> ParseValues(string attributesXml, int vendorAttributeId);

        string AddVendorAttribute(string attributesXml, VendorAttribute vendorAttribute, string value);

        IList<string> GetAttributeWarnings(string attributesXml);
    }
}