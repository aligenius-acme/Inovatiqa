using Inovatiqa.Core;
using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Customers
{
    public static class CustomerAttributeExtensions
    {
        public static bool ShouldHaveValues(this CustomerAttribute customerAttribute)
        {
            if (customerAttribute == null)
                return false;

            if (customerAttribute.AttributeControlTypeId == (int)AttributeControlType.TextBox ||
                customerAttribute.AttributeControlTypeId == (int)AttributeControlType.MultilineTextbox ||
                customerAttribute.AttributeControlTypeId == (int)AttributeControlType.Datepicker ||
                customerAttribute.AttributeControlTypeId == (int)AttributeControlType.FileUpload)
                return false;

            return true;
        }
    }
}
