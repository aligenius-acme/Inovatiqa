using Inovatiqa.Core;
using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Orders
{
    public static class CheckoutAttributeExtensions
    {
        public static bool ShouldHaveValues(this CheckoutAttribute checkoutAttribute)
        {
            if (checkoutAttribute == null)
                return false;

            if (checkoutAttribute.AttributeControlTypeId == (int)AttributeControlType.TextBox ||
                checkoutAttribute.AttributeControlTypeId == (int)AttributeControlType.MultilineTextbox ||
                checkoutAttribute.AttributeControlTypeId == (int)AttributeControlType.Datepicker ||
                checkoutAttribute.AttributeControlTypeId == (int)AttributeControlType.FileUpload)
                return false;

            return true;
        }

        public static bool CanBeUsedAsCondition(this CheckoutAttribute checkoutAttribute)
        {
            if (checkoutAttribute == null)
                return false;

            if (checkoutAttribute.AttributeControlTypeId == (int)AttributeControlType.ReadonlyCheckboxes ||
                checkoutAttribute.AttributeControlTypeId == (int)AttributeControlType.TextBox ||
                checkoutAttribute.AttributeControlTypeId == (int)AttributeControlType.MultilineTextbox ||
                checkoutAttribute.AttributeControlTypeId == (int)AttributeControlType.Datepicker ||
                checkoutAttribute.AttributeControlTypeId == (int)AttributeControlType.FileUpload)
                return false;

            return true;
        }
    }
}