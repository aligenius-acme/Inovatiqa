using Inovatiqa.Web.Framework.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Logging
{
    public partial class LogModel : BaseInovatiqaEntityModel
    {
        #region Properties

        [Display(Name = "Log level")]
        public string LogLevel { get; set; }

        [Display(Name = "Short message")]
        public string ShortMessage { get; set; }

        [Display(Name = "Full message")]
        public string FullMessage { get; set; }

        [Display(Name = "IP address")]
        public string IpAddress { get; set; }

        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }

        [Display(Name = "Customer")]
        public string CustomerEmail { get; set; }

        [Display(Name = "Page URL")]
        public string PageUrl { get; set; }

        [Display(Name = "Referrer URL")]
        public string ReferrerUrl { get; set; }

        [Display(Name = "Created on")]
        public DateTime CreatedOn { get; set; }

        #endregion
    }
}
