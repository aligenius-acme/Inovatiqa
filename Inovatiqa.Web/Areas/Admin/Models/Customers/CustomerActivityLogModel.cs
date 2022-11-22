using Inovatiqa.Web.Framework.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerActivityLogModel : BaseInovatiqaEntityModel
    {
        #region Properties

        [Display(Name = "Activity Log Type")]
        public string ActivityLogTypeName { get; set; }

        [Display(Name = "Comment")]
        public string Comment { get; set; }

        [Display(Name = "Created on")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "IP address")]
        public string IpAddress { get; set; }

        #endregion
    }
}
