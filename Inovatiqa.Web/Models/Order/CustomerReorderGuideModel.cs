using Inovatiqa.Web.Models.Catalog;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Order
{
    public partial class CustomerReorderGuideModel
    {
        
        public CustomerReorderGuideModel()
        {
            AvailableCatagories = new List<KeyValuePair<int, string>>();
            ProductAttributes = new List<ProductAttributeModel>();
            ProductManufacturers = new List<ManufacturerBriefInfoModel>();
            ReorderProducts = new List<CustomerReorderGuideModel>();
        }
        public List<CustomerReorderGuideModel> ReorderProducts { get; set; }
        public int SelectedCategory { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int? MSku { get; set; }
        public string ProductSeName { get; set; }
        public int Qty { get; set; }
        public string OrderItemPrice { get; set; }
        public string AttributeInfo { get; set; }
        public string AttributesXml { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ReOrderDate { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public List<KeyValuePair<int, string>> AvailableCatagories { get; set; }
        public IList<ProductAttributeModel> ProductAttributes { get; set; }
        public IList<ProductDetailsModel.ProductAttributeModel> ProductAttributesAndValues { get; set; }
        public IList<ManufacturerBriefInfoModel> ProductManufacturers { get; set; }
      
        #region Nested Classes

        public partial class ProductPriceModel
        {
            public string CurrencyCode { get; set; }

            public string OldPrice { get; set; }

            public string Price { get; set; }
            public string PriceWithDiscount { get; set; }
            public decimal PriceValue { get; set; }

            public bool CustomerEntersPrice { get; set; }

            public bool CallForPrice { get; set; }

            public int ProductId { get; set; }

            public bool HidePrices { get; set; }

            public bool IsRental { get; set; }
            public string RentalPrice { get; set; }

            public bool DisplayTaxShippingInfo { get; set; }
            public string BasePricePAngV { get; set; }
        }

        public partial class ProductAttributeModel
        {
            public ProductAttributeModel()
            {
                AllowedFileExtensions = new List<string>();
                Values = new List<ProductAttributeValueModel>();
            }

            public int Id { get; set; }

            public int ProductId { get; set; }

            public int ProductAttributeId { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public string TextPrompt { get; set; }

            public bool IsRequired { get; set; }

            public string DefaultValue { get; set; }
            public int? SelectedDay { get; set; }
            public int? SelectedMonth { get; set; }
            public int? SelectedYear { get; set; }

            public bool HasCondition { get; set; }

            public IList<string> AllowedFileExtensions { get; set; }

            public int AttributeControlTypeId { get; set; }

            public IList<ProductAttributeValueModel> Values { get; set; }
        }

        public partial class ProductAttributeValueModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string PriceAdjustment { get; set; }
            public bool PriceAdjustmentUsePercentage { get; set; }
            public decimal PriceAdjustmentValue { get; set; }
            public bool IsPreSelected { get; set; }
            public int Quantity { get; set; }
        }

        #endregion
    }
}