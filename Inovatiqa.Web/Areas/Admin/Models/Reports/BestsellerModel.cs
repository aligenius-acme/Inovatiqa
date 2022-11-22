using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Reports
{
    public partial class BestsellerModel : BaseInovatiqaModel
    {
        #region Properties

        public int ProductId { get; set; }

        [Display(Name = "Name")]
        public string ProductName { get; set; }

        [Display(Name = "Total amount (incl tax")]
        public string TotalAmount { get; set; }

        [Display(Name = "Total quantity")]
        public decimal TotalQuantity { get; set; }

        #endregion
    }
}