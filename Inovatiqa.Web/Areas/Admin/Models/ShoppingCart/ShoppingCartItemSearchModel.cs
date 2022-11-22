using Inovatiqa.Web.Framework.Models;
using System;

namespace Inovatiqa.Web.Areas.Admin.Models.ShoppingCart
{
    public partial class ShoppingCartItemSearchModel : BaseSearchModel
    {
        #region Properties

        public int CustomerId { get; set; }

        public int ShoppingCartTypeId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int ProductId { get; set; }

        public int BillingCountryId { get; set; }

        public int StoreId { get; set; }

        #endregion
    }
}