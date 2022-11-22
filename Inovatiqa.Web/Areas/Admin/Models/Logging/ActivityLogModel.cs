using Inovatiqa.Web.Framework.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Logging
{
    public partial class ActivityLogModel : BaseInovatiqaEntityModel
    {
        #region Properties

        [Display(Name = "Activity log type")]
        public string ActivityLogTypeName { get; set; }

        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Customer Email")]
        public string CustomerEmail { get; set; }

        [Display(Name = "Message")]
        public string Comment { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "IP address")]
        public string IpAddress { get; set; }

        #endregion
    }
}
