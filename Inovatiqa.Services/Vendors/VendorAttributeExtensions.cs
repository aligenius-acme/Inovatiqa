using Inovatiqa.Core;
using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Vendors
{
    public static class VendorAttributeExtensions
    {
        public static bool ShouldHaveValues(this VendorAttribute vendorAttribute)
        {
            if (vendorAttribute == null)
                return false;

            if (vendorAttribute.AttributeControlTypeId == (int)AttributeControlType.TextBox ||
                vendorAttribute.AttributeControlTypeId == (int)AttributeControlType.MultilineTextbox ||
                vendorAttribute.AttributeControlTypeId == (int)AttributeControlType.Datepicker ||
                vendorAttribute.AttributeControlTypeId == (int)AttributeControlType.FileUpload)
                return false;

            return true;
        }
    }
}