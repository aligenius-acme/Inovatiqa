namespace Inovatiqa.Services.Common.Interfaces
{
    public partial interface IAddressAttributeFormatterService
    {
        string FormatAttributes(string attributesXml,
            string separator = "<br />", 
            bool htmlEncode = true);
    }
}
