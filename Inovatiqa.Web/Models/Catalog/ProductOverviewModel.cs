using Inovatiqa.Web.Models.Media;
using System;
using System.Collections.Generic;
using static Inovatiqa.Web.Models.Catalog.ProductDetailsModel;

namespace Inovatiqa.Web.Models.Catalog
{
    public partial class ProductOverviewModel
    {
        public ProductOverviewModel()
        {
            ProductPrice = new ProductPriceModel();
            DefaultPictureModel = new PictureModel();
            SpecificationAttributeModels = new List<ProductSpecificationModel>();
            ReviewOverviewModel = new ProductReviewOverviewModel();
            ProductCategories = new List<CategoryModel>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string SeName { get; set; }
        public bool IsInCompareList { get; set; }
        public string stockStatus { get; set; }
        public int quantity { get; set; }
        public string Sku { get; set; }

        public int ProductTypeId { get; set; }

        public bool MarkAsNew { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public ProductPriceModel ProductPrice { get; set; }
        public PictureModel DefaultPictureModel { get; set; }
        public IList<ProductSpecificationModel> SpecificationAttributeModels { get; set; }
        public virtual IList<ManufacturerBriefInfoModel> ProductManufacturers { get; set; }
        public virtual IList<ProductAttributeModel> ProductAttributes { get; set; }
        public ProductReviewOverviewModel ReviewOverviewModel { get; set; }
        public IList<CategoryModel> ProductCategories { get; set; }

		#region Nested Classes

        public partial class ProductPriceModel
        {
            public decimal OrignalPrice { get; set; }
            public string EntityName { get; set; }
            public string OldPrice { get; set; }
            public string Price { get; set; }
            public decimal PriceValue { get; set; }
            public string BasePricePAngV { get; set; }

            public bool DisableBuyButton { get; set; }
            public bool DisableWishlistButton { get; set; }
            public bool DisableAddToCompareListButton { get; set; }

            public bool AvailableForPreOrder { get; set; }
            public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }

            public bool IsRental { get; set; }

            public bool ForceRedirectionAfterAddingToCart { get; set; }

            public bool DisplayTaxShippingInfo { get; set; }
        }

		#endregion
    }
}