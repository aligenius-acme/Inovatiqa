using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Models.Customer
{
    public partial class PasswordRecoveryModel : BaseInovatiqaModel
    {
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Password recovery rmail")]
        public string Email { get; set; }

        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}