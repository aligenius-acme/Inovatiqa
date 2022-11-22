using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Microsoft.AspNetCore.Http;

namespace Inovatiqa.Services.Customers.Interfaces
{
    public partial interface ICustomerRegistrationService
    {
        CustomerLoginResults ValidateCustomer(string usernameOrEmail, string password);

        CustomerRegistrationResult RegisterCustomer(CustomerRegistrationRequest request);

        ChangePasswordResult ChangePassword(ChangePasswordRequest request);

        void SetEmail(Customer customer, string newEmail, bool requireValidation);
		void SetUsername(Customer customer, string newUsername);
        CustomerPassword PrepareChildPasswordModel(IFormCollection collection, int CustomerId);
    }
}