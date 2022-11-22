using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Authentication.Interfaces
{
    public partial interface IAuthenticationService 
    {
        void SignIn(Customer customer, bool isPersistent);

        void SignOut();

        Customer GetAuthenticatedCustomer();
    }
}