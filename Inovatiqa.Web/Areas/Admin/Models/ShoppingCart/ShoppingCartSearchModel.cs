using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.ShoppingCart
{
    public partial class ShoppingCartSearchModel : BaseSearchModel
    {
        #region Ctor

        public ShoppingCartSearchModel()
        {
            AvailableShoppingCartTypes = new List<SelectListItem>();
            ShoppingCartItemSearchModel = new ShoppingCartItemSearchModel();
            AvailableStores = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [Display(Name = "Shopping cart type")]
        public int ShoppingCartTypeId { get; set; }

        [Display(Name = "Start date")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End date")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Billing country")]
        public int BillingCountryId { get; set; }

        [Display(Name = "Store")]
        public int StoreId { get; set; }

        public IList<SelectListItem> AvailableShoppingCartTypes { get; set; }

        public ShoppingCartItemSearchModel ShoppingCartItemSearchModel { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }

        public bool HideStoresList { get; set; }

        #endregion
    }
}
