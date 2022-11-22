using Inovatiqa.Web.Framework.Models;
using System;

namespace Inovatiqa.Web.Areas.Admin.Models.Orders
{
    public partial class ShipmentStatusEventModel : BaseInovatiqaModel
    {
        #region Properties

        public string EventName { get; set; }

        public string Location { get; set; }

        public string Country { get; set; }

        public DateTime? Date { get; set; }

        #endregion
    }
}