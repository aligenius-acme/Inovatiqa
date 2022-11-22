using System;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Services
{
    public static class Extensions
    {
        public static SelectList ToSelectList<TEnum>(this TEnum enumObj,
           bool markCurrentAsSelected = true, int[] valuesToExclude = null, bool useLocalization = true) where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("An Enumeration type is required.", nameof(enumObj));

            var values = from TEnum enumValue in Enum.GetValues(typeof(TEnum))
                         where valuesToExclude == null || !valuesToExclude.Contains(Convert.ToInt32(enumValue))
                         select new { ID = Convert.ToInt32(enumValue), Name = CommonHelper.ConvertEnum(enumValue.ToString()) };
            object selectedValue = null;
            if (markCurrentAsSelected)
                selectedValue = Convert.ToInt32(enumObj);
            return new SelectList(values, "ID", "Name", selectedValue);
        }
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