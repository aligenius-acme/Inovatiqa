using Inovatiqa.Web.Framework.Models;
using Inovatiqa.Web.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Models.Customer
{
    public partial class ChangePasswordModel : BaseInovatiqaModel
    {
        [NoTrim]
        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string OldPassword { get; set; }

        [NoTrim]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [NoTrim]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmNewPassword { get; set; }

        public string Result { get; set; }
    }
}