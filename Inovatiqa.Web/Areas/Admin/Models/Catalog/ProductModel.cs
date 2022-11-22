using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Areas.Admin.Models.Settings;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class ProductModel : BaseInovatiqaEntityModel,
        IAclSupportedModel, IDiscountSupportedModel, ILocalizedModel<ProductLocalizedModel>, IStoreMappingSupportedModel
    {
        #region Ctor

        public ProductModel()
        {
            ProductPictureModels = new List<ProductPictureModel>();
            Locales = new List<ProductLocalizedModel>();
            CopyProductModel = new CopyProductModel();
            AddPictureModel = new ProductPictureModel();
            ProductWarehouseInventoryModels = new List<ProductWarehouseInventoryModel>();
            ProductEditorSettingsModel = new ProductEditorSettingsModel();
            //StockQuantityHistory = new StockQuantityHistoryModel();

            AvailableBasepriceUnits = new List<SelectListItem>();
            AvailableBasepriceBaseUnits = new List<SelectListItem>();
            AvailableProductTemplates = new List<SelectListItem>();
            AvailableTaxCategories = new List<SelectListItem>();
            AvailableDeliveryDates = new List<SelectListItem>();
            AvailableProductAvailabilityRanges = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
            ProductsTypesSupportedByProductTemplates = new Dictionary<int, IList<SelectListItem>>();

            AvailableVendors = new List<SelectListItem>();

            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();

            SelectedManufacturerIds = new List<int>();
            AvailableManufacturers = new List<SelectListItem>();

            SelectedCategoryIds = new List<int>();
            AvailableCategories = new List<SelectListItem>();

            SelectedCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();

            SelectedDiscountIds = new List<int>();
            AvailableDiscounts = new List<SelectListItem>();

            RelatedProductSearchModel = new RelatedProductSearchModel();
            CrossSellProductSearchModel = new CrossSellProductSearchModel();
            AssociatedProductSearchModel = new AssociatedProductSearchModel();
            ProductPictureSearchModel = new ProductPictureSearchModel();
            ProductSpecificationAttributeSearchModel = new ProductSpecificationAttributeSearchModel();
            ProductOrderSearchModel = new ProductOrderSearchModel();
            TierPriceSearchModel = new TierPriceSearchModel();
            StockQuantityHistorySearchModel = new StockQuantityHistorySearchModel();
            ProductAttributeMappingSearchModel = new ProductAttributeMappingSearchModel();
            ProductAttributeCombinationSearchModel = new ProductAttributeCombinationSearchModel();
        }

        #endregion

        #region Properties

        [Display(Name = "Picture")]
        public string PictureThumbnailUrl { get; set; }

        [Display(Name = "Product type")]
        public int ProductTypeId { get; set; }

        [Display(Name = "Product type")]
        public string ProductTypeName { get; set; }

        [Display(Name = "Associated to product")]
        public int AssociatedToProductId { get; set; }

        [Display(Name = "Associated to product")]
        public string AssociatedToProductName { get; set; }

        [Display(Name = "Visible individually")]
        public bool VisibleIndividually { get; set; }

        [Display(Name = "Product template")]
        public int ProductTemplateId { get; set; }
        public IList<SelectListItem> AvailableProductTemplates { get; set; }

        public Dictionary<int, IList<SelectListItem>> ProductsTypesSupportedByProductTemplates { get; set; }

        [Display(Name = "Product name")]
        public string Name { get; set; }

        [Display(Name = "Short description")]
        public string ShortDescription { get; set; }

        [Display(Name = "Full description")]
        public string FullDescription { get; set; }

        [Display(Name = "Admin comment")]
        public string AdminComment { get; set; }

        [Display(Name = "Show on home page")]
        public bool ShowOnHomepage { get; set; }

        [Display(Name = "Meta keywords")]
        public string MetaKeywords { get; set; }

        [Display(Name = "Meta description")]
        public string MetaDescription { get; set; }

        [Display(Name = "Meta title")]
        public string MetaTitle { get; set; }

        [Display(Name = "Search engine friendly page name")]
        public string SeName { get; set; }

        [Display(Name = "Allow customer reviews")]
        public bool AllowCustomerReviews { get; set; }

        [Display(Name = "Product tags")]
        public string ProductTags { get; set; }

        public string InitialProductTags { get; set; }

        [Display(Name = "SKU")]
        public string Sku { get; set; }

        [Display(Name = "Manufacturer part number")]
        public string ManufacturerPartNumber { get; set; }

        [Display(Name = "GTIN (global trade item number)")]
        public virtual string Gtin { get; set; }

        [Display(Name = "Is gift card")]
        public bool IsGiftCard { get; set; }

        [Display(Name = "Gift card type")]
        public int GiftCardTypeId { get; set; }

        [Display(Name = "Overridden gift card amount")]
        [UIHint("DecimalNullable")]
        public decimal? OverriddenGiftCardAmount { get; set; }

        [Display(Name = "Require other products")]
        public bool RequireOtherProducts { get; set; }

        [Display(Name = "Required product IDs")]
        public string RequiredProductIds { get; set; }

        [Display(Name = "Automatically add these products to the cart")]
        public bool AutomaticallyAddRequiredProducts { get; set; }

        [Display(Name = "Downloadable product")]
        public bool IsDownload { get; set; }

        [Display(Name = "Download file")]
        [UIHint("Download")]
        public int DownloadId { get; set; }

        [Display(Name = "Unlimited downloads")]
        public bool UnlimitedDownloads { get; set; }

        [Display(Name = "Max. downloads")]
        public int MaxNumberOfDownloads { get; set; }

        [Display(Name = "Number of days")]
        [UIHint("Int32Nullable")]
        public int? DownloadExpirationDays { get; set; }

        [Display(Name = "Download activation type")]
        public int DownloadActivationTypeId { get; set; }

        [Display(Name = "Has sample download file")]
        public bool HasSampleDownload { get; set; }

        [Display(Name = "Sample download file")]
        [UIHint("Download")]
        public int SampleDownloadId { get; set; }

        [Display(Name = "Has user agreement")]
        public bool HasUserAgreement { get; set; }

        [Display(Name = "User agreement text")]
        public string UserAgreementText { get; set; }

        [Display(Name = "Recurring product")]
        public bool IsRecurring { get; set; }

        [Display(Name = "Cycle length")]
        public int RecurringCycleLength { get; set; }

        [Display(Name = "Cycle period")]
        public int RecurringCyclePeriodId { get; set; }

        [Display(Name = "Total cycles")]
        public int RecurringTotalCycles { get; set; }

        [Display(Name = "Is rental")]
        public bool IsRental { get; set; }

        [Display(Name = "Rental period length")]
        public int RentalPriceLength { get; set; }

        [Display(Name = "Rental period")]
        public int RentalPricePeriodId { get; set; }

        [Display(Name = "Shipping enabled")]
        public bool IsShipEnabled { get; set; }

        [Display(Name = "Free shipping")]
        public bool IsFreeShipping { get; set; }

        [Display(Name = "Ship separately")]
        public bool ShipSeparately { get; set; }

        [Display(Name = "Additional shipping charge")]
        public decimal AdditionalShippingCharge { get; set; }

        [Display(Name = "Delivery date")]
        public int DeliveryDateId { get; set; }
        public IList<SelectListItem> AvailableDeliveryDates { get; set; }

        [Display(Name = "Tax exempt")]
        public bool IsTaxExempt { get; set; }

        [Display(Name = "Tax category")]
        public int TaxCategoryId { get; set; }
        public IList<SelectListItem> AvailableTaxCategories { get; set; }

        [Display(Name = "Telecommunications, broadcasting and electronic services")]
        public bool IsTelecommunicationsOrBroadcastingOrElectronicServices { get; set; }

        [Display(Name = "Inventory method")]
        public int ManageInventoryMethodId { get; set; }

        [Display(Name = "Product availability range")]
        public int ProductAvailabilityRangeId { get; set; }
        public IList<SelectListItem> AvailableProductAvailabilityRanges { get; set; }

        [Display(Name = "Multiple warehouses")]
        public bool UseMultipleWarehouses { get; set; }

        [Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }
        public IList<SelectListItem> AvailableWarehouses { get; set; }

        [Display(Name = "Stock quantity")]
        public int StockQuantity { get; set; }

        public int LastStockQuantity { get; set; }

        [Display(Name = "Stock quantity")]
        public string StockQuantityStr { get; set; }

        [Display(Name = "Display availability")]
        public bool DisplayStockAvailability { get; set; }

        [Display(Name = "Display stock quantity")]
        public bool DisplayStockQuantity { get; set; }

        [Display(Name = "Minimum stock qty")]
        public int MinStockQuantity { get; set; }

        [Display(Name = "Low stock activity")]
        public int LowStockActivityId { get; set; }

        [Display(Name = "Notify for qty below")]
        public int NotifyAdminForQuantityBelow { get; set; }

        [Display(Name = "Backorders")]
        public int BackorderModeId { get; set; }

        [Display(Name = "Allow back in stock subscriptions")]
        public bool AllowBackInStockSubscriptions { get; set; }

        [Display(Name = "Minimum cart qty")]
        public int OrderMinimumQuantity { get; set; }

        [Display(Name = "Maximum cart qty")]
        public int OrderMaximumQuantity { get; set; }

        [Display(Name = "Allowed quantities")]
        public string AllowedQuantities { get; set; }

        [Display(Name = "Allow only existing attribute combinations")]
        public bool AllowAddingOnlyExistingAttributeCombinations { get; set; }

        [Display(Name = "Not returnable")]
        public bool NotReturnable { get; set; }

        [Display(Name = "Disable buy button")]
        public bool DisableBuyButton { get; set; }

        [Display(Name = "Disable wishlist button")]
        public bool DisableWishlistButton { get; set; }

        [Display(Name = "Available for pre-order")]
        public bool AvailableForPreOrder { get; set; }

        [Display(Name = "Pre-order availability start date")]
        [UIHint("DateTimeNullable")]
        public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }

        [Display(Name = "Call for price")]
        public bool CallForPrice { get; set; }

        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Display(Name = "Old price")]
        public decimal OldPrice { get; set; }

        [Display(Name = "Product cost")]
        public decimal ProductCost { get; set; }

        [Display(Name = "Customer enters price")]
        public bool CustomerEntersPrice { get; set; }

        [Display(Name = "Minimum amount")]
        public decimal MinimumCustomerEnteredPrice { get; set; }

        [Display(Name = "Maximum amount")]
        public decimal MaximumCustomerEnteredPrice { get; set; }

        [Display(Name = "PAngV (base price) enabled")]
        public bool BasepriceEnabled { get; set; }

        [Display(Name = "Amount in product")]
        public decimal BasepriceAmount { get; set; }

        [Display(Name = "Unit of product")]
        public int BasepriceUnitId { get; set; }
        public IList<SelectListItem> AvailableBasepriceUnits { get; set; }

        [Display(Name = "Reference amount")]
        public decimal BasepriceBaseAmount { get; set; }

        [Display(Name = "Reference unit")]
        public int BasepriceBaseUnitId { get; set; }
        public IList<SelectListItem> AvailableBasepriceBaseUnits { get; set; }

        [Display(Name = "Mark as new")]
        public bool MarkAsNew { get; set; }

        [Display(Name = "Mark as new. Start date")]
        [UIHint("DateTimeNullable")]
        public DateTime? MarkAsNewStartDateTimeUtc { get; set; }

        [Display(Name = "Mark as new. End date")]
        [UIHint("DateTimeNullable")]
        public DateTime? MarkAsNewEndDateTimeUtc { get; set; }

        [Display(Name = "Weight")]
        public decimal Weight { get; set; }

        [Display(Name = "Length")]
        public decimal Length { get; set; }

        [Display(Name = "Width")]
        public decimal Width { get; set; }

        [Display(Name = "Height")]
        public decimal Height { get; set; }

        [Display(Name = "Available start date")]
        [UIHint("DateTimeNullable")]
        public DateTime? AvailableStartDateTimeUtc { get; set; }

        [Display(Name = "Available end date")]
        [UIHint("DateTimeNullable")]
        public DateTime? AvailableEndDateTimeUtc { get; set; }

        [Display(Name = "Display order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Published")]
        public bool Published { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }

        public string BaseDimensionIn { get; set; }

        public string BaseWeightIn { get; set; }

        public IList<ProductLocalizedModel> Locales { get; set; }

        [Display(Name = "Customer roles")]
        public IList<int> SelectedCustomerRoleIds { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        [Display(Name = "Limited to stores")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        [Display(Name = "Categories")]
        public IList<int> SelectedCategoryIds { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }

        [Display(Name = "Manufacturers")]
        public IList<int> SelectedManufacturerIds { get; set; }
        public IList<SelectListItem> AvailableManufacturers { get; set; }

        [Display(Name = "Vendor")]
        public int VendorId { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }

        [Display(Name = "Discounts")]
        public IList<int> SelectedDiscountIds { get; set; }
        public IList<SelectListItem> AvailableDiscounts { get; set; }

        public bool IsLoggedInAsVendor { get; set; }

        public ProductPictureModel AddPictureModel { get; set; }
        public IList<ProductPictureModel> ProductPictureModels { get; set; }

        public bool ProductAttributesExist { get; set; }
        public bool CanCreateCombinations { get; set; }

        [Display(Name = "Warehouses")]
        public IList<ProductWarehouseInventoryModel> ProductWarehouseInventoryModels { get; set; }

        public bool HasAvailableSpecificationAttributes { get; set; }

        public CopyProductModel CopyProductModel { get; set; }

        public ProductEditorSettingsModel ProductEditorSettingsModel { get; set; }

        //public StockQuantityHistoryModel StockQuantityHistory { get; set; }

        public RelatedProductSearchModel RelatedProductSearchModel { get; set; }

        public CrossSellProductSearchModel CrossSellProductSearchModel { get; set; }

        public AssociatedProductSearchModel AssociatedProductSearchModel { get; set; }

        public ProductPictureSearchModel ProductPictureSearchModel { get; set; }

        public ProductSpecificationAttributeSearchModel ProductSpecificationAttributeSearchModel { get; set; }

        public ProductOrderSearchModel ProductOrderSearchModel { get; set; }

        public TierPriceSearchModel TierPriceSearchModel { get; set; }

        public StockQuantityHistorySearchModel StockQuantityHistorySearchModel { get; set; }

        public ProductAttributeMappingSearchModel ProductAttributeMappingSearchModel { get; set; }

        public ProductAttributeCombinationSearchModel ProductAttributeCombinationSearchModel { get; set; }

        #endregion
    }

    public partial class ProductLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [Display(Name = "Product name")]
        public string Name { get; set; }

        [Display(Name = "Short description")]
        public string ShortDescription { get; set; }

        [Display(Name = "Full description")]
        public string FullDescription { get; set; }

        [Display(Name = "Meta keywords")]
        public string MetaKeywords { get; set; }

        [Display(Name = "Meta description")]
        public string MetaDescription { get; set; }

        [Display(Name = "Meta title")]
        public string MetaTitle { get; set; }

        [Display(Name = "Search engine friendly page name")]
        public string SeName { get; set; }
    }
}
