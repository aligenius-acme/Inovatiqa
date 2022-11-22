using Inovatiqa.Web.Framework.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Vendors
{
    public partial class VendorNoteModel : BaseInovatiqaEntityModel
    {
        #region Properties

        public int VendorId { get; set; }

        [Display(Name = "Note")]
        public string Note { get; set; }

        [Display(Name = "Created on")]
        public DateTime CreatedOn { get; set; }

        #endregion
    }
}
