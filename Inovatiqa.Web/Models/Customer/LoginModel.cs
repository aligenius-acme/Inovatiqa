using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Models.Customer
{
    public partial class LoginModel
    {
        public bool CheckoutAsGuest { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public bool UsernamesEnabled { get; set; }

        public int RegistrationTypeId { get; set; }

        public string Username { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}