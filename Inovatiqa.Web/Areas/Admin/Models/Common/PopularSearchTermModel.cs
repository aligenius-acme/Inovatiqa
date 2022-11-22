using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Common
{
    public partial class PopularSearchTermModel : BaseInovatiqaModel
    {
        #region Properties

        [Display(Name = "Keyword")]
        public string Keyword { get; set; }

        [Display(Name = "Count")]
        public int Count { get; set; }

        #endregion
    }
}
