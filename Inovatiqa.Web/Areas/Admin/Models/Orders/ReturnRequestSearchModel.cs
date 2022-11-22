using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Orders
{
    public class ReturnRequestSearchModel: BaseSearchModel
    {
        #region Ctor

        public ReturnRequestSearchModel()
        {
            ReturnRequestStatusList = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [Display(Name = "Start date")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End date")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "ID")]
        public string CustomNumber { get; set; }

        [Display(Name = "Return status")]
        public int ReturnRequestStatusId { get; set; }

        public IList<SelectListItem> ReturnRequestStatusList { get; set; }

        #endregion
    }
}
