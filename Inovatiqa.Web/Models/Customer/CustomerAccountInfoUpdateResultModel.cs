

using Inovatiqa.Database.Models;

namespace Inovatiqa.Web.Models.Customer
{
    public partial class CustomerAccountInfoUpdateResultModel
    {
        public CustomerAccountInfoUpdateResultModel()
        {
            BillingAddress = new Address();
            ShippingAddress = new Address();
        }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Message { get; set; }

        public string MessageClass { get; set; }
        public Address BillingAddress { get; set; }
        public Address ShippingAddress { get; set; }
    }
}
