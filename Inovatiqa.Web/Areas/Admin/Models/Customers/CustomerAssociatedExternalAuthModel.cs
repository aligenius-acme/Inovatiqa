using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerAssociatedExternalAuthModel : BaseInovatiqaEntityModel
    {
        #region Properties

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "External identifier")]
        public string ExternalIdentifier { get; set; }
        
        [Display(Name = "Authentication method")]
        public string AuthMethodName { get; set; }

        #endregion
    }
}
