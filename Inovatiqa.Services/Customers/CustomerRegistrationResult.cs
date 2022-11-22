using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Customers
{
    public class CustomerRegistrationResult
    {
        public CustomerRegistrationResult()
        {
            Errors = new List<string>();
        }

        public bool Success => !Errors.Any();

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public IList<string> Errors { get; set; }
    }
}