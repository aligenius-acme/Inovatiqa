using System.Collections.Generic;

namespace Inovatiqa.Services.Discounts
{
    public partial class DiscountValidationResult
    {
        public DiscountValidationResult()
        {
            Errors = new List<string>();
        }

        public bool IsValid { get; set; }

        public IList<string> Errors { get; set; }
    }
}