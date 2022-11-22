using System;
using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Order
{
    public partial class ReturnRequestItemsSelectionModel
        {
            public ReturnRequestItemsSelectionModel()
            {
                Items = new List<ReturnRequestItemsSelectionModel>();
                SelectedItems = new List<int>();
                SelectedItemsQuantity = new List<int>();
                SelectedItemsReturnReason = new List<int>();
                ReturnReasons = new List<KeyValuePair<int, string>>();
                ReturnActions = new List<KeyValuePair<int, string>>();
                SelectedItemsReturnAction = new List<int>();
            }
            public string PO { get; set; }
            public string Invoice { get; set; }
            public int OrderNo { get; set; }
            public string InvoiceDate { get; set; }
            public int Itemno { get; set; }
            public string Description { get; set; }
            public string UOM { get; set; }
            public decimal Price { get; set; }
            public string Credit { get; set; }
            public int Quantity { get; set; }
            public int ReturnQuantity { get; set; }
            public string ReturnReason { get; set; }
            public IList<ReturnRequestItemsSelectionModel> Items { get; set; }
            public string User { get; set; }
            public DateTime CreatedOn { get; set; }
            public int TotalItems { get; set; }
            public decimal SelectedItemsPrice { get; set; }
            public bool IsSelected { get; set; }
            public int ShippingLabel { get; set; }
            public string Email1 { get; set; }
            public string Email2 { get; set; }
            public string Company { get; set; }
            public string AddressLine1 { get; set; }
            public string Zip { get; set; }
            public string City { get; set; }
            public int ReturnNo { get; set; }
            public int ReturnActionId { get; set; }
            public List<int> SelectedItems { get; set; }
            public List<int> SelectedItemsQuantity { get; set; }
            public List<int> SelectedItemsReturnReason { get; set; } 
            public List<int> SelectedItemsReturnAction { get; set; }
            public List<KeyValuePair<int, string>> ReturnReasons { get; set; }
            public List<KeyValuePair<int, string>> ReturnActions { get; set; }

        }
    }
