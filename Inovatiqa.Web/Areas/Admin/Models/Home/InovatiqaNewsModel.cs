using System.Collections.Generic;
using Inovatiqa.Web.Framework.Models;

namespace Inovatiqa.Web.Areas.Admin.Models.Home
{
    public partial class InovatiqaNewsModel : BaseInovatiqaModel
    {
        #region Ctor

        public InovatiqaNewsModel()
        {
            Items = new List<InovatiqaNewsDetailsModel>();
        }

        #endregion

        #region Properties

        public List<InovatiqaNewsDetailsModel> Items { get; set; }

        public bool HasNewItems { get; set; }

        public bool HideAdvertisements { get; set; }

        #endregion
    }
}