using System;
using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Order
{
    public partial class CustomerReturnRequestsModel
    {
        public CustomerReturnRequestsModel()
        {
            Items = new List<ReturnRequestModel>();
            ProductManufacturers = new List<ManufacturerBriefInfoModel>();
        }
        public IList<ReturnRequestModel> Items { get; set; }
        public IList<ManufacturerBriefInfoModel> ProductManufacturers { get; set; }

        #region Nested classes

        public partial class ReturnRequestModel
        {
            public int Id { get; set; }
            public int OrderId { get; set; }
            public string CustomNumber { get; set; }
            public string ReturnRequestStatus { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductSeName { get; set; }
            public int Quantity { get; set; }
            public string ReturnReason { get; set; }
            public string ReturnAction { get; set; }
            public string Comments { get; set; }
            public Guid UploadedFileGuid { get; set; }
            public DateTime CreatedOn { get; set; }
            public string PendingCredit { get; set; }
            public string ManufacturerPartNumber { get; set; }
            public string AttributeInfo { get; set; }
            public string PoNumber { get; set; }
            public IList<ManufacturerBriefInfoModel> ProductManufacturers { get; set; }
            public string Freight { get; set; }
            public string TotalProductCredit { get; set; }
            public BillingAddressModel BillingAddress { get; set; }
        }

        public partial class ManufacturerBriefInfoModel
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public string SeName { get; set; }

            public bool IsActive { get; set; }
        }
        public partial class BillingAddressModel
        {
            public string CompanyName { get; set; }
            public string AddressLine { get; set; }
            public string Country { get; set; }
            public string ZipCode { get; set; }
        }

        #endregion
    }
}