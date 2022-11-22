using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class ManufacturerSearchModel : BaseSearchModel
    {
        #region Ctor

        public ManufacturerSearchModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailablePublishedOptions = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [Display(Name = "Manufacturer name")]
        public string SearchManufacturerName { get; set; }

        [Display(Name = "Store")]
        public int SearchStoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        [Display(Name = "Published")]
        public int SearchPublishedId { get; set; }

        public IList<SelectListItem> AvailablePublishedOptions { get; set; }

        public bool HideStoresList { get; set; }

        #endregion
    }
}
