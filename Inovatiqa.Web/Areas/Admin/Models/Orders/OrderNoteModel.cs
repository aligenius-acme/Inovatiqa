using Inovatiqa.Web.Framework.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Orders
{
    public partial class OrderNoteModel : BaseInovatiqaEntityModel
    {
        #region Properties

        public int OrderId { get; set; }

        [Display(Name = "Display to customer")]
        public bool DisplayToCustomer { get; set; }

        [Display(Name = "Note")]
        public string Note { get; set; }

        [Display(Name = "Attached file")]
        public int DownloadId { get; set; }

        [Display(Name = "Attached file")]
        public Guid DownloadGuid { get; set; }

        [Display(Name = "Created on")]
        public DateTime CreatedOn { get; set; }

        #endregion
    }
}
