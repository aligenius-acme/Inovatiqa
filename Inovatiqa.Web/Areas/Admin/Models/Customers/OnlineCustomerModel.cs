using Inovatiqa.Web.Framework.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public partial class OnlineCustomerModel : BaseInovatiqaEntityModel
    {
        #region Properties

        [Display(Name = "Customer info")]
        public string CustomerInfo { get; set; }

        [Display(Name = "IP Address")]
        public string LastIpAddress { get; set; }

        [Display(Name = "Location")]
        public string Location { get; set; }

        [Display(Name = "Last activity")]
        public DateTime LastActivityDate { get; set; }
        
        [Display(Name = "Last visited page")]
        public string LastVisitedPage { get; set; }

        #endregion
    }
}
