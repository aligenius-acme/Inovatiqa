using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Logging
{
    public partial class ActivityLogSearchModel : BaseSearchModel
    {
        #region Ctor

        public ActivityLogSearchModel()
        {
            ActivityLogType = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [Display(Name = "Created from")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnFrom { get; set; }

        [Display(Name = "Created to")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnTo { get; set; }

        [Display(Name = "Activity log type")]
        public int ActivityLogTypeId { get; set; }

        [Display(Name = "Activity log type")]
        public IList<SelectListItem> ActivityLogType { get; set; }
        
        [Display(Name = "IP address")]
        public string IpAddress { get; set; }

        #endregion
    }
}
