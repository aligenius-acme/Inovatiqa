using Inovatiqa.Core;
using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Common
{
    public static class AddressAttributeExtensions
    {
        public static bool ShouldHaveValues(this AddressAttribute addressAttribute)
        {
            if (addressAttribute == null)
                return false;

            if (addressAttribute.AttributeControlTypeId == (int)AttributeControlType.TextBox ||
                addressAttribute.AttributeControlTypeId == (int)AttributeControlType.MultilineTextbox ||
                addressAttribute.AttributeControlTypeId == (int)AttributeControlType.Datepicker ||
                addressAttribute.AttributeControlTypeId == (int)AttributeControlType.FileUpload)
                return false;

            return true;
        }
    }
}
