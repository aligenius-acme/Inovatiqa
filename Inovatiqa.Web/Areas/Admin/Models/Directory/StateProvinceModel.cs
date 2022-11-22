using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Directory
{
    public partial class StateProvinceModel : BaseInovatiqaEntityModel, ILocalizedModel<StateProvinceLocalizedModel>
    {
        #region Ctor

        public StateProvinceModel()
        {
            Locales = new List<StateProvinceLocalizedModel>();
        }

        #endregion

        #region Properties

        public int CountryId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Abbreviation")]
        public string Abbreviation { get; set; }

        [Display(Name = "Published")]
        public bool Published { get; set; }

        [Display(Name = "Display order")]
        public int DisplayOrder { get; set; }

        public IList<StateProvinceLocalizedModel> Locales { get; set; }

        #endregion
    }

    public partial class StateProvinceLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }
        
        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}
