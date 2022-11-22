using Inovatiqa.Web.Framework.Models;
using Inovatiqa.Web.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Models.Customer
{
    public partial class PasswordRecoveryConfirmModel : BaseInovatiqaModel
    {
        [DataType(DataType.Password)]
        [NoTrim]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }
        
        [NoTrim]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        public string ConfirmNewPassword { get; set; }

        public bool DisablePasswordChanging { get; set; }
        public string Result { get; set; }
        public string Token { get; set; }
    }
}