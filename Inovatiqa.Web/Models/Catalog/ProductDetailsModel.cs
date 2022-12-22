using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Core;
using Inovatiqa.Web.Models.Media;
using Microsoft.AspNetCore.Mvc.Rendering;
using Inovatiqa.Web.Models.ShoppingCart;
using Inovatiqa.Database.Models;

namespace Inovatiqa.Web.Models.Catalog
{
    public partial class ProductDetailsModel
    {
        public ProductDetailsModel()
        {
            DefaultPictureModel = new PictureModel();
            PictureModels = new List<PictureModel>();
            GiftCard = new GiftCardModel();
            ProductPrice = new ProductPriceModel();
            AddToCart = new AddToCartModel();
            ProductAttributes = new List<ProductAttributeModel>();
            AssociatedProducts = new List<ProductDetailsModel>();
            VendorModel = new VendorBriefInfoModel();
            Breadcrumb = new ProductBreadcrumbModel();
            ProductTags = new List<ProductTagModel>();
            ProductSpecifications= new List<ProductSpecificationModel>();
            ProductManufacturers = new List<ManufacturerBriefInfoModel>();
            ProductReviewOverview = new ProductReviewOverviewModel();
            TierPrices = new List<TierPriceModel>();
            ProductEstimateShipping = new ProductEstimateShippingModel();
        }

        public bool DefaultPictureZoomEnabled { get; set; }
        public PictureModel DefaultPictureModel { get; set; }
        public IList<PictureModel> PictureModels { get; set; }

        public int Id { get; set; }
        //added by hamza
        public bool editing { get; set; }
        public decimal unitPrice { get; set; }
        public ShoppingCartItem updatecartitem { get; set; }
        public int cartQuantity { get; set; }
        public string stockStatus { get; set; }
        public int quantity { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }

        public int ProductTypeId { get; set; }

        public bool ShowSku { get; set; }
        public string Sku { get; set; }

        public bool ShowManufacturerPartNumber { get; set; }
        public string ManufacturerPartNumber { get; set; }

        public bool ShowGtin { get; set; }
        public string Gtin { get; set; }

        public bool ShowVendor { get; set; }
        public VendorBriefInfoModel VendorModel { get; set; }

        public bool HasSampleDownload { get; set; }

        public GiftCardModel GiftCard { get; set; }

        public bool IsShipEnabled { get; set; }
        public bool IsFreeShipping { get; set; }
        public bool FreeShippingNotificationEnabled { get; set; }
        public string DeliveryDate { get; set; }

        public bool IsRental { get; set; }
        public DateTime? RentalStartDate { get; set; }
        public DateTime? RentalEndDate { get; set; }

        public DateTime? AvailableEndDate { get; set; }

        public int ManageInventoryMethodId { get; set; }

        public string StockAvailability { get; set; }

        public bool DisplayBackInStockSubscription { get; set; }

        public bool EmailAFriendEnabled { get; set; }
        public bool CompareProductsEnabled { get; set; }

        public string PageShareCode { get; set; }

        public ProductPriceModel ProductPrice { get; set; }

        public AddToCartModel AddToCart { get; set; }

        public ProductBreadcrumbModel Breadcrumb { get; set; }

        public IList<ProductTagModel> ProductTags { get; set; }

        public IList<ProductAttributeModel> ProductAttributes { get; set; }

        public IList<ProductSpecificationModel> ProductSpecifications { get; set; }

        public IList<ManufacturerBriefInfoModel> ProductManufacturers { get; set; }

        public ProductReviewOverviewModel ProductReviewOverview { get; set; }

        public ProductEstimateShippingModel ProductEstimateShipping { get; set; }

        public IList<TierPriceModel> TierPrices { get; set; }

        public IList<ProductDetailsModel> AssociatedProducts { get; set; }

        public bool DisplayDiscontinuedMessage { get; set; }

        public string CurrentStoreName { get; set; }

        #region Nested Classes

        public partial class ProductBreadcrumbModel
        {
            public ProductBreadcrumbModel()
            {
                CategoryBreadcrumb = new List<CategorySimpleModel>();
            }

            public bool Enabled { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductSeName { get; set; }
            public IList<CategorySimpleModel> CategoryBreadcrumb { get; set; }
        }

        public partial class AddToCartModel
        {
            public AddToCartModel()
            {
                AllowedQuantities = new List<SelectListItem>();
            }
            public int ProductId { get; set; }

            public int EnteredQuantity { get; set; }
            public string MinimumQuantityNotification { get; set; }
            public List<SelectListItem> AllowedQuantities { get; set; }

            public bool CustomerEntersPrice { get; set; }
            public decimal CustomerEnteredPrice { get; set; }
            public string CustomerEnteredPriceRange { get; set; }

            public bool DisableBuyButton { get; set; }
            public bool DisableWishlistButton { get; set; }

            public bool IsRental { get; set; }

            public bool AvailableForPreOrder { get; set; }
            public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }
            public string PreOrderAvailabilityStartDateTimeUserTime { get; set; }

            public int UpdatedShoppingCartItemId { get; set; }
            public int UpdateShoppingCartItemTypeId { get; set; }
        }

        public partial class ProductPriceModel
        {
            public decimal OrignalPrice { get; set; }
            public string EntityName { get; set; }
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

        public partial class GiftCardModel
        {
            public bool IsGiftCard { get; set; }

            public string RecipientName { get; set; }

            [DataType(DataType.EmailAddress)]
            public string RecipientEmail { get; set; }

            public string SenderName { get; set; }

            [DataType(DataType.EmailAddress)]
            public string SenderEmail { get; set; }

            public string Message { get; set; }

            public GiftCardType GiftCardType { get; set; }
        }

        public partial class TierPriceModel
        {
            public string Price { get; set; }

            public int Quantity { get; set; }
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
            public ProductAttributeValueModel()
            {
                ImageSquaresPictureModel = new PictureModel();
            }
            public string AttributesValuesCount { get; set; }

            public int Id { get; set; }
            public string Ids { get; set; }
            public string Name { get; set; }

            public string ColorSquaresRgb { get; set; }

            public PictureModel ImageSquaresPictureModel { get; set; }

            public string PriceAdjustment { get; set; }
            
            public bool PriceAdjustmentUsePercentage { get; set; }

            public decimal PriceAdjustmentValue { get; set; }

            public bool IsPreSelected { get; set; }

            public int PictureId { get; set; }

            public bool CustomerEntersQty { get; set; }

            public int Quantity { get; set; }

            public int MSku { get; set; }

            public string ParentProductURL { get; set; }
        }

        public partial class ProductEstimateShippingModel : EstimateShippingModel
        {
            public int ProductId { get; set; }
        }

        #endregion
    }
}