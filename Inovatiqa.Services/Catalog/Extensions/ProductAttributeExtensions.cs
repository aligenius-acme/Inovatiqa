using Inovatiqa.Core;
using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Catalog.Extensions
{
    public static class ProductAttributeExtensions
    {
        public static bool ShouldHaveValues(this ProductProductAttributeMapping productAttributeMapping)
        {
            if (productAttributeMapping == null)
                return false;

            if (productAttributeMapping.AttributeControlTypeId == 4 ||
                productAttributeMapping.AttributeControlTypeId == 10 ||
                productAttributeMapping.AttributeControlTypeId == 20)
                return false;

            return true;
        }

        public static bool CanBeUsedAsCondition(this ProductProductAttributeMapping productAttributeMapping)
        {
            if (productAttributeMapping == null)
                return false;

            if (productAttributeMapping.AttributeControlTypeId == 50 || 
                productAttributeMapping.AttributeControlTypeId == 4 ||
                productAttributeMapping.AttributeControlTypeId == 10 ||
                productAttributeMapping.AttributeControlTypeId == 20)
                return false;

            return true;
        }

        public static bool ValidationRulesAllowed(this ProductProductAttributeMapping productAttributeMapping)
        {
            if (productAttributeMapping == null)
                return false;

            if (productAttributeMapping.AttributeControlTypeId == 4 ||
                productAttributeMapping.AttributeControlTypeId == 10)
                return true;

            return false;
        }

        public static bool IsNonCombinable(this ProductProductAttributeMapping productAttributeMapping)
        {
            if (productAttributeMapping == null)
                return false;

            var result = !ShouldHaveValues(productAttributeMapping);
            return result;
        }

        public static bool ProductAttributeCanBeUsedAsCondition(this ProductProductAttributeMapping productAttributeMapping)
        {
            if (productAttributeMapping == null)
                return false;

            if (productAttributeMapping.AttributeControlTypeId == (int)AttributeControlType.ReadonlyCheckboxes ||
                productAttributeMapping.AttributeControlTypeId == (int)AttributeControlType.TextBox ||
                productAttributeMapping.AttributeControlTypeId == (int)AttributeControlType.MultilineTextbox ||
                productAttributeMapping.AttributeControlTypeId == (int)AttributeControlType.Datepicker ||
                productAttributeMapping.AttributeControlTypeId == (int)AttributeControlType.FileUpload)
                return false;

            return true;
        }
    }
}
