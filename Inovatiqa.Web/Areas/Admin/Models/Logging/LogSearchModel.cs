using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Logging
{
    public partial class LogSearchModel : BaseSearchModel
    {
        #region Ctor

        public LogSearchModel()
        {
            AvailableLogLevels = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [Display(Name = "Created from")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnFrom { get; set; }

        [Display(Name = "Created to")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnTo { get; set; }

        [Display(Name = "Message")]
        public string Message { get; set; }

        [Display(Name = "Log level")]
        public int LogLevelId { get; set; }

        public IList<SelectListItem> AvailableLogLevels { get; set; }

        #endregion
    }
}
