using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Settings
{
    public partial class ProductEditorSettingsModel : BaseInovatiqaModel, ISettingsModel
    {
        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }

        [Display(Name = "Product type")]
        public bool ProductType { get; set; }

        [Display(Name = "Visible individually")]
        public bool VisibleIndividually { get; set; }

        [Display(Name = "Product template")]
        public bool ProductTemplate { get; set; }

        [Display(Name = "Admin comment")]
        public bool AdminComment { get; set; }

        [Display(Name = "Vendor")]
        public bool Vendor { get; set; }

        [Display(Name = "Stores")]
        public bool Stores { get; set; }

        [Display(Name = "Customer roles")]
        public bool ACL { get; set; }

        [Display(Name = "Show on home page")]
        public bool ShowOnHomepage { get; set; }

        [Display(Name = "Allow customer reviews")]
        public bool AllowCustomerReviews { get; set; }

        [Display(Name = "Product tags")]
        public bool ProductTags { get; set; }

        [Display(Name = "Manufacturer part number")]
        public bool ManufacturerPartNumber { get; set; }

        [Display(Name = "GTIN (global trade item number)")]
        public bool GTIN { get; set; }

        [Display(Name = "Product cost")]
        public bool ProductCost { get; set; }

        [Display(Name = "Tier prices")]
        public bool TierPrices { get; set; }

        [Display(Name = "Discounts")]
        public bool Discounts { get; set; }

        [Display(Name = "Disable buy button")]
        public bool DisableBuyButton { get; set; }

        [Display(Name = "Disable wishlist button")]
        public bool DisableWishlistButton { get; set; }

        [Display(Name = "Available for pre-order")]
        public bool AvailableForPreOrder { get; set; }

        [Display(Name = "Call for price")]
        public bool CallForPrice { get; set; }

        [Display(Name = "Old price")]
        public bool OldPrice { get; set; }

        [Display(Name = "Customer enters price")]
        public bool CustomerEntersPrice { get; set; }

        [Display(Name = "PAngV (base price) enabled")]
        public bool PAngV { get; set; }

        [Display(Name = "Require other products")]
        public bool RequireOtherProductsAddedToCart { get; set; }

        [Display(Name = "Is gift card")]
        public bool IsGiftCard { get; set; }

        [Display(Name = "Downloadable product")]
        public bool DownloadableProduct { get; set; }

        [Display(Name = "Recurring product")]
        public bool RecurringProduct { get; set; }

        [Display(Name = "Is rental")]
        public bool IsRental { get; set; }

        [Display(Name = "Free shipping")]
        public bool FreeShipping { get; set; }

        [Display(Name = "Ship separately")]
        public bool ShipSeparately { get; set; }

        [Display(Name = "Additional shipping charge")]
        public bool AdditionalShippingCharge { get; set; }

        [Display(Name = "Delivery date")]
        public bool DeliveryDate { get; set; }

        [Display(Name = "Telecommunications, broadcasting and electronic services")]
        public bool TelecommunicationsBroadcastingElectronicServices { get; set; }

        [Display(Name = "Product availability range")]
        public bool ProductAvailabilityRange { get; set; }

        [Display(Name = "Multiple warehouses")]
        public bool UseMultipleWarehouses { get; set; }

        [Display(Name = "Warehouses")]
        public bool Warehouse { get; set; }

        [Display(Name = "Display availability")]
        public bool DisplayStockAvailability { get; set; }

        [Display(Name = "Minimum stock qty")]
        public bool MinimumStockQuantity { get; set; }

        [Display(Name = "Low stock activity")]
        public bool LowStockActivity { get; set; }

        [Display(Name = "Notify for qty below")]
        public bool NotifyAdminForQuantityBelow { get; set; }

        [Display(Name = "Backorders")]
        public bool Backorders { get; set; }

        [Display(Name = "Allow back in stock subscriptions")]
        public bool AllowBackInStockSubscriptions { get; set; }

        [Display(Name = "Minimum cart qty")]
        public bool MinimumCartQuantity { get; set; }

        [Display(Name = "Maximum cart qty")]
        public bool MaximumCartQuantity { get; set; }

        [Display(Name = "Allowed quantities")]
        public bool AllowedQuantities { get; set; }

        [Display(Name = "Allow only existing attribute combinations")]
        public bool AllowAddingOnlyExistingAttributeCombinations { get; set; }

        [Display(Name = "Not returnable")]
        public bool NotReturnable { get; set; }

        [Display(Name = "Weight")]
        public bool Weight { get; set; }

        [Display(Name = "Dimensions")]
        public bool Dimensions { get; set; }

        [Display(Name = "Available start date")]
        public bool AvailableStartDate { get; set; }

        [Display(Name = "Available end date")]
        public bool AvailableEndDate { get; set; }

        [Display(Name = "Mark as new")]
        public bool MarkAsNew { get; set; }

        [Display(Name = "Published")]
        public bool Published { get; set; }
        
        [Display(Name = "Related products")]
        public bool RelatedProducts { get; set; }

        [Display(Name = "Cross-sells products")]
        public bool CrossSellsProducts { get; set; }

        [Display(Name = "SEO")]
        public bool Seo { get; set; }

        [Display(Name = "Purchased with orders")]
        public bool PurchasedWithOrders { get; set; }
       
        [Display(Name = "Product attributes")]
        public bool ProductAttributes { get; set; }

        [Display(Name = "Specification attributes")]
        public bool SpecificationAttributes { get; set; }

        [Display(Name = "Manufacturers")]
        public bool Manufacturers { get; set; }

        [Display(Name = "Stock quantity history")]
        public bool StockQuantityHistory { get; set; }

        #endregion
    }
}
