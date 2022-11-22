using Inovatiqa.Web.Framework.Models;
using System;
namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerRewardPointsModel : BaseInovatiqaEntityModel
    {
        #region Properties

        public string StoreName { get; set; }

        public int Points { get; set; }

        public string PointsBalance { get; set; }

        public string Message { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? EndDate { get; set; }

        #endregion
    }
}