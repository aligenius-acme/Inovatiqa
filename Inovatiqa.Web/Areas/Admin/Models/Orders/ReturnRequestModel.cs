using Inovatiqa.Web.Framework.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Orders
{
    public partial class ReturnRequestModel : BaseInovatiqaEntityModel
    {
        #region Properties

        [Display(Name = "ID")]
        public string CustomNumber { get; set; }
        
        public int OrderId { get; set; }

        [Display(Name = "Order #")]
        public string CustomOrderNumber { get; set; }

        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Customer")]
        public string CustomerInfo { get; set; }

        public int ProductId { get; set; }

        [Display(Name = "Product")]
        public string ProductName { get; set; }

        public string AttributeInfo { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }
        
        [Display(Name = "Reason for return")]
        public string ReasonForReturn { get; set; }
        
        [Display(Name = "Requested action")]
        public string RequestedAction { get; set; }
        
        [Display(Name = "Customer comments")]
        public string CustomerComments { get; set; }

        [Display(Name = "Uploaded file")]
        public Guid UploadedFileGuid { get; set; }
        
        [Display(Name = "Staff notes")]
        public string StaffNotes { get; set; }

        [Display(Name = "Return request status")]
        public int ReturnRequestStatusId { get; set; }

        [Display(Name = "Return request status")]
        public string ReturnRequestStatusStr { get; set; }

        [Display(Name = "Date")]
        public DateTime CreatedOn { get; set; }

        #endregion
    }
}
