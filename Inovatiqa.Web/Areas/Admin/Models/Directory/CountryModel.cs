using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Directory
{
    public partial class CountryModel : BaseInovatiqaEntityModel, ILocalizedModel<CountryLocalizedModel>, IStoreMappingSupportedModel
    {
        #region Ctor

        public CountryModel()
        {
            Locales = new List<CountryLocalizedModel>();
            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
            StateProvinceSearchModel = new StateProvinceSearchModel();
        }

        #endregion

        #region Properties

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Allows billing")]
        public bool AllowsBilling { get; set; }

        [Display(Name = "Allows shipping")]
        public bool AllowsShipping { get; set; }

        [Display(Name = "Two letter ISO code")]
        public string TwoLetterIsoCode { get; set; }

        [Display(Name = "Three letter ISO code")]
        public string ThreeLetterIsoCode { get; set; }

        [Display(Name = "Numeric ISO code")]
        public int NumericIsoCode { get; set; }

        [Display(Name = "Subject to VAT")]
        public bool SubjectToVat { get; set; }

        [Display(Name = "Published")]
        public bool Published { get; set; }

        [Display(Name = "Display order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Number of states")]
        public int NumberOfStates { get; set; }

        public IList<CountryLocalizedModel> Locales { get; set; }

        [Display(Name = "Limited to stores")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        public StateProvinceSearchModel StateProvinceSearchModel { get; set; }

        #endregion
    }

    public partial class CountryLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}
