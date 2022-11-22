using System;
using System.Collections.Generic;
using Inovatiqa.Web.Models.Catalog;
using Inovatiqa.Web.Models.Media;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Models.ShoppingCart
{
    public partial class WishlistModel
    {
        public WishlistModel()
        {
            Items = new List<ShoppingCartItemModel>();
            Warnings = new List<string>();
            Categories = new List<KeyValuePair<int, string>>();
        }
        public string Name { get; set; }
        public Guid CustomerGuid { get; set; }
        public string CustomerFullname { get; set; }

        public bool EmailWishlistEnabled { get; set; }

        public bool ShowSku { get; set; }

        public bool ShowProductImages { get; set; }

        public bool IsEditable { get; set; }

        public bool DisplayAddToCart { get; set; }

        public bool DisplayTaxShippingInfo { get; set; }

        public IList<ShoppingCartItemModel> Items { get; set; }

        public IList<string> Warnings { get; set; }
        public List<KeyValuePair<int, string>> Categories { get; set; }
        public int SelectedCategory { get; set; }

        public bool IsRegistered { get; set; }

        public bool IsShared { get; set; }

        #region Nested Classes

        public partial class ShoppingCartItemModel
        {
            public ShoppingCartItemModel()
            {
                Picture = new PictureModel();
                AllowedQuantities = new List<SelectListItem>();
                Warnings = new List<string>();
            }

            public int Id { get; set; }

            public int Category { get; set; }

            public string Sku { get; set; }

            public PictureModel Picture {get;set;}

            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

            public string UnitPrice { get; set; }

            public string SubTotal { get; set; }

            public string Discount { get; set; }
            public int? MaximumDiscountedQty { get; set; }

            public int Quantity { get; set; }
            public List<SelectListItem> AllowedQuantities { get; set; }

            public string AttributeInfo { get; set; }

            public string RecurringInfo { get; set; }

            public string RentalInfo { get; set; }

            public bool AllowItemEditing { get; set; }

            public IList<string> Warnings { get; set; }
            public IList<ManufacturerBriefInfoModel> ProductManufacturers { get; set; }
            public string VendorName { get; set; }
            public string ManufacturerPartNumber { get; set; }
            public string UOM { get; set; }
            public IList<ProductDetailsModel.ProductAttributeModel> ProductAttributesAndValues { get; set; }


        }

        #endregion
    }
}