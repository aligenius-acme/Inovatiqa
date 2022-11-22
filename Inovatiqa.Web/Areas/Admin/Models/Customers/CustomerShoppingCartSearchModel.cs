using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerShoppingCartSearchModel : BaseSearchModel
    {
        #region Ctor

        public CustomerShoppingCartSearchModel()
        {
            AvailableShoppingCartTypes = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        public int CustomerId { get; set; }

        [Display(Name = "Shopping cart type")]
        public int ShoppingCartTypeId { get; set; }

        public IList<SelectListItem> AvailableShoppingCartTypes { get; set; }

        #endregion
    }
}
