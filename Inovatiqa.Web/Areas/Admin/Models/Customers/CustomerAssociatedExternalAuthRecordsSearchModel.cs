using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public class CustomerAssociatedExternalAuthRecordsSearchModel : BaseSearchModel
    {
        #region Properties

        public int CustomerId { get; set; }

        [Display(Name = "External authentication")]
        public IList<CustomerAssociatedExternalAuthModel> AssociatedExternalAuthRecords { get; set; } = new List<CustomerAssociatedExternalAuthModel>();
        
        #endregion
    }
}
