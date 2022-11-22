using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerRoleModel : BaseInovatiqaEntityModel
    {
        #region Ctor

        public CustomerRoleModel()
        {
            TaxDisplayTypeValues = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Free shipping")]
        public bool FreeShipping { get; set; }

        [Display(Name = "Tax exempt")]
        public bool TaxExempt { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; }

        [Display(Name = "Is system role")]
        public bool IsSystemRole { get; set; }

        [Display(Name = "System name")]
        public string SystemName { get; set; }

        [Display(Name = "Enable password lifetime")]
        public bool EnablePasswordLifetime { get; set; }

        [Display(Name = "Override default tax display type")]
        public bool OverrideTaxDisplayType { get; set; }

        [Display(Name = "Default tax display type")]
        public int DefaultTaxDisplayTypeId { get; set; }

        public IList<SelectListItem> TaxDisplayTypeValues { get; set; }

        [Display(Name = "Purchased with product")]
        public int PurchasedWithProductId { get; set; }

        [Display(Name = "Purchased with product")]
        public string PurchasedWithProductName { get; set; }

        #endregion
    }
}
