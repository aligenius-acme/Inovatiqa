using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.ShoppingCart
{
    public partial class ShoppingCartModel : BaseInovatiqaModel
    {
        #region Properties

        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Customer")]
        public string CustomerEmail { get; set; }

        [Display(Name = "Total items")]
        public int TotalItems { get; set; }

        #endregion
    }
}
