using Inovatiqa.Core.Caching;
using Microsoft.AspNetCore.Http;

namespace Inovatiqa.Core
{
    public static partial class InovatiqaDefaults
    {
        #region Tax

        public static int DefaultTaxCategoryId => 0;

        public static bool AllowCustomersToSelectTaxDisplayType => false;

        #endregion

        #region Admin
        public static bool HideAdvertisementsOnAdminArea => true;
        public static int PasswordLifetime => 90;
        public static int LastActivityMinutes => 15;
        public static bool StoreLastVisitedPage => true;
        public static bool UseIsoDateFormatInJsonResult => true;
        public static int DefaultGridPageSize => 15;
        public static string GridPageSizes => "7, 15, 20, 50, 100";

        #endregion

        #region Store

        public static int DeleteGuestTaskOlderThanMinutes => 2000;

        public static string DefaultLocalePattern => "en";

        public static string LocalePatternPath => "lib\\cldr-data\\main\\{0}";

        public static int LanguageId => 0;

        public static int StoreId => 0;

        public static int PrimaryStoreId => 1;
        //public static string StoreUrl => "https://app-inovatiqa-prod-01.azurewebsites.net/";

        //public static string StoreUrl => "https://inovatiqadev.azurewebsites.net/";

        public static string StoreUrl => "https://localhost:5001/";

        //public static string StoreUrl => "https://inovatiqa-stg.azurewebsites.net/";

        public static string CurrentStoreName => "Inovatiqa";

        public static string CompanyAddress => "12815 Capricorn St, Stafford, TX 77477, United States";

        public static string CompanyPhoneNo => "(346) 229-4142";
        public static string CompanyFaxNo => "281-220-1350";

        public static string FacebookLink => "https://www.facebook.com/inovatiqa.crop";

        public static string TwitterLink => "https://twitter.com/inovatiqa";

        public static string LinkedInLink => "https://www.linkedin.com/in/inovatiqa-corp-684b71178";

        public static bool ShowBestsellersOnHomepage => true;

        public static int NumberOfBestsellersOnHomepage => 6;

        public static int CustomerCookieExpires => 8760;

        public static int AcceptCookieExpires => 6;

        public static bool IsCurrentConnectionSecured => true;

        public static bool HideStoresList => true;

        public static string CurrencyCode => "USD";

        public static int PrimaryStoreCurrencyId => 1;

        public static string LanguageCulture => "en-US";

        public static bool LoadAllUrlRecordsOnStartup => false;

        public static int MinimumOrderPlacementInterval => 30;

        public static decimal CurrencyRate => 1;

        public static bool ForceTaxExclusionFromOrderSubtotal => true;

        public static int LanguageDefaultCurrencyId => 0;

        public static int DisplayOrder => 0;

        public static string FlagImageFileName => "us.png";

        public static bool LimitedToStores => false;

        public static string LanguageName => "English";

        public static bool Rtl => false;

        public static string UniqueSeoCode => "en";

        public static bool CompleteOrderWhenDelivered => false;


        #endregion

        public static int searchedParentCategoryCount => 30; //added by hamza for how much parent categories admin want to show in search page
        public static bool ShowSkuOnProductDetailsPage => true;
        public static bool ShowManufacturerPartNumber => true;
        public static bool ShowFreeShippingNotification => false;
        public static bool ShowGtin => false;
        public static bool DisplayDiscontinuedMessageForUnpublishedProducts => false;
        public static bool EmailAFriendEnabled => true;
        public static bool CompareProductsEnabled => true;
        public static bool DisplayBackInStockSubscription => false;
        public static bool DisplayPickupInStoreOnShippingMethodPage => false;
        public static bool DisplayPickupPointsOnMap => false;
        public static bool GoogleMapsApiKey => false;
        public static bool NotifyCustomerAboutShippingFromMultipleLocations => false;
        public static bool BypassShippingMethodSelectionIfOnlyOne => false;
        public static bool BypassPaymentMethodSelectionIfOnlyOne => true;
        public static bool SkipPaymentInfoStepForRedirectionPaymentMethods => true;
        public static bool OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab => false;

        #region Customer

        #region System customer roles

        public static string PORoleName => "PO";

        public static string B2BRoleName => "B2B";

        public static string AdministratorsRoleName => "Administrators";

        public static string ForumModeratorsRoleName => "ForumModerators";

        public static string RegisteredRoleName => "Registered";

        public static string GuestsRoleName => "Guests";

        public static string VendorsRoleName => "Vendors";

        public const string RetailRoleName = "Retail";
        public const string BronzeRoleName = "Bronze";
        public const string BronzePremierRoleName = "Bronze Premier";
        public const string GoldRoleName = "Gold";
        public const string GoldPremierRoleName = "Gold Premier";
        public const string OnyxRoleName = "Onyx";
        public const string OnyxPremierRoleName = "Onyx Premier";
        public const string DiamondRoleName = "Diamond";
        public const string DiamondPremierRoleName = "Diamond Premier";
        public const string DistributorRoleName = "Distributor";
        public const string DistributorPremierRoleName = "Distributor Premier";

        #endregion

        #region System customers

        public static string SearchEngineCustomerName => "SearchEngine";

        public static string BackgroundTaskCustomerName => "BackgroundTask";

        public static bool AllowViewingProfiles => false;

        public static bool AllowAnonymousUsersToReviewProduct => false;

        public static bool AllowUsersToChangeUsernames => false;

        public static CacheKey CustomerPasswordLifetimeCacheKey => new CacheKey("Inovatiqa.customers.passwordlifetime-{0}");

        #endregion

        #region Customer attributes

        public static string FirstNameAttribute => "FirstName";

        public static string LastNameAttribute => "LastName";

        public static string GenderAttribute => "Gender";

        public static string DateOfBirthAttribute => "DateOfBirth";

        public static string CompanyAttribute => "Company";

        public static string StreetAddressAttribute => "StreetAddress";

        public static string StreetAddress2Attribute => "StreetAddress2";

        public static string ZipPostalCodeAttribute => "ZipPostalCode";

        public static string CityAttribute => "City";

        public static string CountyAttribute => "County";

        public static string CountryIdAttribute => "CountryId";

        public static string StateProvinceIdAttribute => "StateProvinceId";

        public static string PhoneAttribute => "Phone";

        public static string FaxAttribute => "Fax";

        public static string VatNumberAttribute => "VatNumber";

        public static string VatNumberStatusIdAttribute => "VatNumberStatusId";

        public static string TimeZoneIdAttribute => "TimeZoneId";

        public static string CustomCustomerAttributes => "CustomCustomerAttributes";

        public static string DiscountCouponCodeAttribute => "DiscountCouponCode";

        public static string GiftCardCouponCodesAttribute => "GiftCardCouponCodes";

        public static string AvatarPictureIdAttribute => "AvatarPictureId";

        public static string ForumPostCountAttribute => "ForumPostCount";

        public static string SignatureAttribute => "Signature";

        public static string PasswordRecoveryTokenAttribute => "PasswordRecoveryToken";

        public static string PasswordRecoveryTokenDateGeneratedAttribute => "PasswordRecoveryTokenDateGenerated";

        public static string AccountActivationTokenAttribute => "AccountActivationToken";

        public static string EmailRevalidationTokenAttribute => "EmailRevalidationToken";

        public static string LastVisitedPageAttribute => "LastVisitedPage";

        public static string ImpersonatedCustomerIdAttribute => "ImpersonatedCustomerId";

        public static string AdminAreaStoreScopeConfigurationAttribute => "AdminAreaStoreScopeConfiguration";

        public static string CurrencyIdAttribute => "CurrencyId";

        public static string LanguageIdAttribute => "LanguageId";

        public static string LanguageAutomaticallyDetectedAttribute => "LanguageAutomaticallyDetected";

        public static string SelectedPaymentMethodAttribute => "SelectedPaymentMethod";

        public static string SelectedShippingOptionAttribute => "SelectedShippingOption";

        public static string SelectedPickupPointAttribute => "SelectedPickupPoint";

        public static string CheckoutAttributes => "CheckoutAttributes";

        public static string OfferedShippingOptionsAttribute => "OfferedShippingOptions";

        public static string LastContinueShoppingPageAttribute => "LastContinueShoppingPage";

        public static string NotifiedAboutNewPrivateMessagesAttribute => "NotifiedAboutNewPrivateMessages";

        public static string WorkingThemeNameAttribute => "WorkingThemeName";

        public static string TaxDisplayTypeIdAttribute => "TaxDisplayTypeId";

        public static string UseRewardPointsDuringCheckoutAttribute => "UseRewardPointsDuringCheckout";

        public static string CustomerAttributePrefix => "customer_attribute_";

        public static string ShipToCountryIdAttribute => "ShipToCountryId";
        public static string ShipToStateProvinceIdAttribute => "ShipToStateProvinceId";
        public static string ShipToPhoneAttribute => "ShipToPhone";
        public static string ShipToZipPostalCodeAttribute => "ShipToZipPostalCode";
        public static string ShipToStreetAddressAttribute => "ShipToStreetAddress";
        public static string ShipToCityAttribute => "ShipToCity";
        public static string ShipToCompanyAttribute => "ShipToCompany";


        public static string BillToCountryIdAttribute => "BillToCountryId";
        public static string BillToStateProvinceIdAttribute => "BillToStateProvinceId";
        public static string BillToPhoneAttribute => "BillToPhone";
        public static string BillToZipPostalCodeAttribute => "BillToZipPostalCode";
        public static string BillToStreetAddressAttribute => "BillToStreetAddress";
        public static string BillToCityAttribute => "BillToCity";
        public static string BillToAttribute => "BillToCompany";



        #endregion

        #region PDF


        public static string StoredFilePath = "Files";
        //public static string FontFileName => "FreeSerif.ttf";
        public static string FontFileName => "Roboto.ttf";

        public static bool RenderOrderNotes => true;

        public static bool DisablePdfInvoicesForPendingOrders => false;

        public static bool LetterPageSizeEnabled => false;

        public static bool AttachPdfInvoiceToOrderCompletedEmail => true;

        public static bool AttachPdfInvoiceToOrderPaidEmail => true;

        #endregion

        #region Address ttributes

        public static string AddressAttributeControlName => "address_attribute_{0}";

        #endregion

        #endregion

        #region Attribute Value Type

        public const int Simple = 0;

        public const int AssociatedToProduct = 10;

        #endregion

        #region Cookie Defaults
        public static string AcceptCookie => ".Accept";

        public static string Prefix => ".Inovatiqa";

        public static string CustomerCookie => ".Customer";

        public static string AntiforgeryCookie => ".Antiforgery";

        public static string SessionCookie => ".Session";

        public static string TempDataCookie => ".TempData";

        public static string InstallationLanguageCookie => ".InstallationLanguage";

        public static string ComparedProductsCookie => ".ComparedProducts";

        public static string RecentlyViewedProductsCookie => ".RecentlyViewedProducts";

        public static string AuthenticationCookie => ".Authentication";

        public static string ExternalAuthenticationCookie => ".ExternalAuthentication";

        public static string IgnoreEuCookieLawWarning => ".IgnoreEuCookieLawWarning";

        #endregion

        #region Shopping Cart

        public static bool AllowCartItemEditing => true;

        public static int MiniShoppingCartProductNumber => 5;

        public static bool RenderAssociatedAttributeValueQuantity => true;

        public static bool ShowProductImagesInMiniShoppingCart => true;

        public static int MaximumShoppingCartItems => 20;

        public static int MaximumWishlistItems => int.MaxValue; // set by Ali Ahmad for allowing any number of items to be stored in wishlist

        public static bool AllowOutOfStockItemsToBeAddedToWishlist => false;

        public static bool DisplayWishlistAfterAddingProduct => false;

        public static bool DisplayCartAfterAddingProduct => false;

        public static bool MiniShoppingCartEnabled => true;

        //public const int ShoppingCart = 1;

        //public const int Wishlist = 2;

        public static bool EnableShoppingCart => true;

        public static bool EnableWishlist => true;

        public static bool EmailWishlistEnabled => false;

        public static bool ShowProductImagesOnShoppingCart => true;

        public static bool ShowProductImagesOnWishlist => true;

        public static bool MoveItemsFromWishlistToCart => true;

        public static bool TermsOfServiceOnShoppingCartPage => true;

        public static bool DisplayTaxShippingInfoShoppingCart => true;

        public static bool DisplayTaxShippingInfoWishlist => true;

        public const int Days = 0;

        public const int Weeks = 10;

        public const int Months = 20;

        public const int Years = 30;

        public static bool DisplayPrices => true;

        public static bool AllowPickupInStore => false;

        public static bool CheckoutDisabled => false;

        public static bool GroupTierPricesForDistinctShoppingCartItems => false;

        #endregion

        //#region Attribute Control Type

        //public const int DropdownList = 1;

        //public const int RadioList = 2;

        //public const int Checkboxes = 3;

        //public const int TextBox = 4;

        //public const int MultilineTextbox = 10;

        //public const int Datepicker = 20;

        //public const int FileUpload = 30;

        //public const int ColorSquares = 40;

        //public const int ImageSquares = 45;

        //public const int ReadonlyCheckboxes = 50;

        //#endregion

        #region Media

        public static int ManufacturerThumbPictureSize => 150;

        public static int ImageSquarePictureSize => 20;

        public static int MaximumImageSize => 1980;

        public static bool StoreInDb => true;

        public static int CartThumbPictureSize => 80;

        public static int ProductThumbPictureSize => 415;

        public static int CategoryThumbPictureSize => 450;

        public static int MultipleThumbDirectoriesLength => 3;

        public static string ImageThumbsPath => @"images\thumbs";

        public static string DefaultAvatarFileName => "default-avatar.jpg";

        public static string DefaultImageFileName => "default-image.png";

        public static bool LoadPictureFromDb => true;

        public static int DefaultImageQuality => 80;

        public static bool DefaultPictureZoomEnabled => true;

        public static int AssociatedProductPictureSize => 220;

        public static int ProductDetailsPictureSize => 550;

        public static int ProductThumbPictureSizeOnProductDetailsPage => 100;

        public static int MiniCartThumbPictureSize => 70;

        public static int AutoCompleteSearchThumbPictureSize => 70;

        #endregion

        #region Orders

        public static string Color1 => "#EEEEEE";

        public static string Color2 => "#FAFAFA"; 

        public static string Color3 => "#F5F5F5";

        public static bool AnonymousCheckoutAllowed => false;

        public static bool SubTotalIncludingTax => true;

        public static bool OnePageCheckoutEnabled => true;

        public static decimal MinOrderSubtotalAmount => 0;

        public static bool MinOrderSubtotalAmountIncludingTax => false;

        public static bool TermsOfServiceOnOrderConfirmPage => false;

        public static bool RoundPricesDuringCalculation => false;

        public const int Rounding001 = 0;

        public static decimal MinOrderTotalAmount => 0.1m;

        public static bool AttachPdfInvoiceToOrderPlacedEmail => true;

        public static bool ReturnRequestsEnabled => true;

        public static int NumberOfDaysReturnRequestAvailable => 10;

        public static bool IsReOrderAllowed => true;

        public static bool DisableOrderCompletedPage => false;

        /// <summary>
        /// <![CDATA[Prices are rounded up to the nearest multiple of 5 cents for sales ending in: 3¢ & 4¢ round to 5¢; and, 8¢ & 9¢ round to 10¢]]>
        /// </summary>
        public const int Rounding005Up = 10;

        /// <summary>
        /// <![CDATA[Prices are rounded down to the nearest multiple of 5 cents for sales ending in: 1¢ & 2¢ to 0¢; and, 6¢ & 7¢ to 5¢]]>
        /// </summary>
        public const int Rounding005Down = 20;

        /// <summary>
        /// <![CDATA[Round up to the nearest 10 cent value for sales ending in 5¢]]>
        /// </summary>
        public const int Rounding01Up = 30;

        /// <summary>
        /// <![CDATA[Round down to the nearest 10 cent value for sales ending in 5¢]]>
        /// </summary>
        public const int Rounding01Down = 40;

        /// <summary>
        /// <![CDATA[Sales ending in 1–24 cents round down to 0¢
        /// Sales ending in 25–49 cents round up to 50¢
        /// Sales ending in 51–74 cents round down to 50¢
        /// Sales ending in 75–99 cents round up to the next whole dollar]]>
        /// </summary>
        public const int Rounding05 = 50;

        /// <summary>
        /// Sales ending in 1–49 cents round down to 0
        /// Sales ending in 50–99 cents round up to the next whole dollar
        /// For example, Swedish Krona
        /// </summary>
        public const int Rounding1 = 60;

        /// <summary>
        /// Sales ending in 1–99 cents round up to the next whole dollar
        /// </summary>
        public const int Rounding1Up = 70;

        public static bool HideTaxInOrderSummary => true;

        public static bool DisplayTaxRates => false;

        public static bool DisplayTax => false;

        public static bool RewardPointsEnabled => false;

        public static int CrossSellsNumber => 0;

        public static bool HideZeroTax => true;

        public static bool DisplayTaxShippingInfoOrderDetailsPage => true;

        #endregion

        #region Authentication

        public static string AuthenticationScheme => "Authentication";

        public static string ExternalAuthenticationScheme => "ExternalAuthentication";

        public static string ClaimsIssuer => "inovatiqa";

        public static PathString LoginPath => new PathString("/login");

        public static PathString LogoutPath => new PathString("/logout");

        public static PathString AccessDeniedPath => new PathString("/page-not-found");

        public static string ReturnUrlParameter => string.Empty;

        public static string ExternalAuthenticationErrorsSessionKey => "inovatiqa.externalauth.errors";

        #endregion

        #region Slugs

        public static string CategorySlugName => "Category";

        public static string ProductSlugName => "Product";

        public static string VendorSlugName => "Vendor";

        public static string ManufacturerSlugName => "Manufacturer";

        public static string TopicSlugName => "Topic";

        public static string NewsSlugName => "NewsItem";

        #endregion

        #region News

        public static int MainPageNewsCount => 3;

        public static bool ShowNewsOnMainPage => true;

        #endregion

        #region Catalog

        public static bool PublishBackProductWhenCancellingOrders => false;

        public static string ProductAttributePrefix => "product_attribute_";

        public static bool AjaxProcessAttributeChange => true;

        public static bool IncludeShortDescriptionInCompareProducts => true;

        public static bool IncludeFullDescriptionInCompareProducts => true;

        public const int DontManageStock = 0;

        public const int ManageStock = 1;

        public const int CompareProductsNumber = 10;

        public const int CompareProductsCookieExpires = 10;

        public const int ManageStockByAttributes = 2;

        public static bool ShowSkuOnCatalogPages => true;

        public static bool UseLinksInRequiredProductWarnings => true;

        public const int SimpleProduct = 5;

        public const int GroupedProduct = 10;

        //public const int NoBackorders = 0;

        //public const int AllowQtyBelow0 = 1;

        //public const int AllowQtyBelow0AndNotifyCustomer = 2;

        public static bool RemoveRequiredProducts => true;

        public static bool IgnoreFeaturedProducts => true;

        public static bool IncludeFeaturedProductsInNormalLists => true;

        public static bool RecentlyViewedProductsEnabled => true;

        public static bool MostViewedProductsEnabled => true;

        public static int RecentlyViewedProductsNumber => 4;

        public static int RecentlyViewedProductsCookieExpires => 8760;

        public static int MostViewedProductsNumber => 4;

        public static bool ProductReviewsSortByCreatedDateAscending => false;

        public static bool ProductReviewPossibleOnlyAfterPurchasing => false;

        public static int DefaultProductRatingValue => 5;

        public static bool ProductReviewsMustBeApproved => true;

        public static bool NotifyStoreOwnerAboutNewProductReviews => true;

        public static bool ShowProductReviewsTabOnAccountPage => true;

        public static int ProductReviewsPageSizeOnAccountPage => 10;

        public static bool NewProductsEnabled => true;

        public static int NewProductsNumber => 10;

        #endregion

        #region Common

        public static bool SubjectFieldOnContactUsForm => true;

        #endregion

        #region Captcha

        public static bool Enabled => false;

        public static bool ShowOnContactUsPage => false;

        public static bool ShowOnRegistrationPage => false;

        public static bool ShowOnLoginPage => false;

        public static bool ShowOnProductReviewPage => false;

        public static bool ShowOnForgotPasswordPage => false;

        public static string CaptchaSiteKey => "REDACTED_RECAPTCHA_SITE_KEY";

        public static string CaptchaPrivateKey => "REDACTED_RECAPTCHA_PRIVATE_KEY";

        #endregion

        #region EUCookieLaw

        public static string EuCookieLawAcceptedAttribute => "EuCookieLaw.Accepted";

        #endregion

        #region Searching

        public static bool UseFullTextSearch => false;

        public static bool ProductSearchAutoCompleteEnabled => true;

        public static bool ProductSearchEnabled => true;

        public const int ProductSearchTermMinimumLength = 3;

        public const int ProductSearchAutoCompleteNumberOfProducts = 10;

        public static bool ShowLinkToAllResultInSearchAutoComplete { get; set; }

        public static bool ShowProductImagesInSearchAutoComplete => true;

        #endregion

        #region Http

        public static string DefaultHttpClient => "default";

        public static string IsPostBeingDoneRequestItem => "inovatiqa.IsPOSTBeingDone";

        public static string HttpClusterHttpsHeader => "HTTP_CLUSTER_HTTPS";

        public static string HttpXForwardedProtoHeader => "X-Forwarded-Proto";

        public static string XForwardedForHeader => "X-FORWARDED-FOR";

        public static bool SslEnabled => true;

        #endregion

        #region Messages

        public const string NotificationListKey = "NotificationList";

        public static bool UsePopupNotifications => true;

        public const string CustomerEmailRevalidationMessage = "Customer.EmailRevalidationMessage";

        public const string CustomerRegisteredNotification = "NewCustomer.Notification";

        public const string CustomerEmailValidationMessage = "Customer.EmailValidationMessage";

        public static bool CaseInvariantReplacement => true;

        #endregion

        #region Datetime

        public static bool AllowCustomersToSetTimeZone => false;

        #endregion

        #region Register

        public static bool EuVatEnabled => false;

        public static bool FirstNameEnabled => true;

        public static bool LastNameEnabled => true;

        public static bool FirstNameRequired => true;

        public static bool LastNameRequired => true;

        public static bool GenderEnabled => false;

        public static bool DateOfBirthEnabled => false;

        public static bool DateOfBirthRequired => false;

        public static bool CompanyEnabled => true;

        public static bool CompanyRequired => false;

        public static bool StreetAddressEnabled => true;

        public static bool StreetAddressRequired => true;

        public static bool StreetAddress2Enabled => false;

        public static bool StreetAddress2Required => false;

        public static bool ZipPostalCodeEnabled => true;

        public static bool ZipPostalCodeRequired => true;

        public static bool CityEnabled => true;

        public static bool CityRequired => true;

        public static bool CountyEnabled => false;

        public static bool CountyRequired => false;

        public static bool CountryEnabled => true;

        public static bool CountryRequired => true;

        public static bool StateProvinceEnabled => true;

        public static bool StateProvinceRequired => true;

        public static bool PhoneEnabled => true;

        public static bool PhoneRequired => true;

        public static bool FaxEnabled => false;

        public static bool FaxRequired => false;

        public static bool NewsletterEnabled => true;

        public static bool AcceptPrivacyPolicyEnabled => false;

        public static bool PopupForTermsOfServiceLinks => true;

        public static bool UsernamesEnabled => false;

        public static bool CheckUsernameAvailabilityEnabled => true;

        public static bool HoneypotEnabled => false;

        public static bool EnteringEmailTwice => false;

        public static bool NewsletterTickedByDefault => false;

        public static bool GdprEnabled => false;

        public static bool StandardRegistration => false;

        public const int Standard = 1;

        public const int EmailValidation = 2;

        public const int AdminApproval = 3;

        public const int Disabled = 4;

        public static int CustomerUsernameLength => 5;

        public static bool NotifyNewCustomerRegistration => true;

        #endregion

        #region Login

        public const int Clear = 0;

        public const int Hashed = 1;

        public const int Encrypted = 2;
        public static int FailedPasswordAllowedAttempts => 3;

        public static int FailedPasswordLockoutMinutes => 10;

        public static int UnduplicatedPasswordsNumber => 4;

        #endregion

        #region Security

        public static string EncryptionKey => "654ece6f97dd999d";

        public static string HashedPasswordFormat => "SHA512";

        public static int PasswordSaltKeySize => 5;

        public static string DefaultHashedPasswordFormat => "SHA512";

        #endregion

        #region Email

        public static int QueuedEmailPrioritHigh => 5;

        public static int QueuedEmailPrioritLow => 0;

        public static int DelayPeriod => 1;

        public static int DefaultEmailAccountId => 1;
        public static string CustomerServiceEmail => "customerservice@inovatiqa.com";

        public static string AdministratorEmail => "aligenius@gmail.com";

        #endregion

        #region Areas

        public const string Admin = "Admin";

        public static bool ShowVendorOnOrderDetailsPage => false;

        #endregion

        #region Shipping

        public static bool DisplayShipmentEventsToStoreOwner => true;

        public static bool FreeShippingOverXEnabled => false;

        public static bool FreeShippingOverXIncludingTax => false;

        public static decimal FreeShippingOverXValue => 0;

        public static bool IgnoreAdditionalShippingChargeForPickupInStore => true;

        public static bool IgnoreDiscounts => false;

        public static bool EstimateShippingCartPageEnabled => true;

        public static bool ShipSeparatelyOneItemEach => false;

        public static bool DisplayShipmentEventsToCustomers => true;

        #endregion

        #region Discounts

        public static int AssignedToOrderTotal => 1;

        public static int AssignedToSkus => 2;

        public static int AssignedToCategories => 5;

        public static int AssignedToManufacturers => 6;

        public static int AssignedToShipping => 10;

        public static int AssignedToOrderSubTotal => 20;

        public const int Unlimited = 0;

        public const int NTimesOnly = 15;

        public const int NTimesPerCustomer = 25;

        public static int And => 0;

        public static int Or => 2;

        #endregion

        #region Vendors

        public static bool ShowVendorOnProductDetailsPage => true;

        public static bool AllowVendorsToImportProducts => false;

        public static string VendorAttributePrefix => "vendor_attribute_";

        public static string VendorAttributes => "VendorAttributes";

        #endregion

        #region Log

        public static int Debug => 10;

        public static int Information => 20;

        public static int Warning => 30;

        public static int Error => 40;

        public static int Fatal => 50;

        #endregion

        #region Manufacturers

        public static int ManufacturersBlockItemsToDisplay => 5000;

        public static bool FeaturedManufacturerEnabled => true;

        public static int FeaturedManufacturerNumber => 10;

        #endregion

        #region Settings

        public static int SliderTypeId => 1;

        #endregion

        #region Misc

        public const string ProductReviewStoreOwnerNotification = "Product.ProductReview";

        #endregion

        #region Newly Added Products

        public static bool AllowCustomersToSelectPageSize => true;

        public static string PageSizeOptions => "6,12,18,24,50,75,100";

        public static int PageSize => 6;

        #endregion

        #region Warehouse

        public static bool UseWarehouseLocation => false;

        #endregion

        #region FedEx Shipping

        public static int Offline => 10;

        public static int Realtime => 20;

        public static int BaseDimensionId => 1;

        public static int BaseWeightId => 2;

        public const decimal MAX_PACKAGE_WEIGHT = 150;

        public const string MEASURE_WEIGHT_SYSTEM_KEYWORD = "lb";

        public const string MEASURE_DIMENSION_SYSTEM_KEYWORD = "inches";

        public const string FEDEXKey = "REDACTED_FEDEX_KEY_OLD3";

        public const string Password = "REDACTED_FEDEX_PASSWORD_OLD3";

        public const string AccountNumber = "510087380";

        public const string MeterNumber = "119164650";

        public const bool PassDimensions = true;

        public const int PackByDimensions = 0;

        public const int PackByOneItemPerPackage = 1;

        public const int PackByVolume = 2;

        public const int DefaultPackageType = 0;

        public const string CarrierServicesOffered = "FEDEX_1_DAY_FREIGHT:FEDEX_2_DAY:FEDEX_2_DAY_FREIGHT:FEDEX_3_DAY_FREIGHT:FEDEX_GROUND:FIRST_OVERNIGHT:";

        public const bool ApplyDiscounts = false;

        public const decimal AdditionalHandlingCharge = 0;

        public const bool UseResidentialRates = false;

        public const int PackingPackageVolume = 5184;

        public const string DropoffType = "BusinessServiceCenter";

        public const string BusinessServiceCenter = "0";

        public const string DropBox = "10";

        public const string RegularPickup = "20";

        public const string RequestCourier = "30";

        public const string Station = "40";

        public const string FEDEXUrl = "https://wsbeta.fedex.com:443/web-services";

        public const bool UseCubeRootMethod = true;

        public const bool ReturnValidOptionsIfThereAreAny = true;

        public const bool ConsiderAssociatedProductsDimensions = true;

        public const string FedExShippingMethodName = "Shipping.FedEx";

        #region "Shipping Origin Address"

        // Always query Setting Table to look for shippingsettings.shippingoriginaddressid and update Id below.
        public const int ShippingOriginAddressId = 1;

        #endregion

        #endregion

        #region Square Payment

        public static string RestrictedCountriesSettingName => "PaymentMethodRestictions.{0}";

        public static int StandardType => 10;

        public static int RedirectionType => 15;

        public static int ButtonType => 20;

        public const int NotSupported = 0;

        public const int Manual = 10;

        public const int Automatic = 20;

        public const string VIEW_COMPONENT_NAME = "PaymentSquare";

        public const string PAYMENT_APPROVED_STATUS = "APPROVED";

        public const string PAYMENT_COMPLETED_STATUS = "COMPLETED";

        public const string PAYMENT_FAILED_STATUS = "FAILED";

        public const string PAYMENT_CANCELED_STATUS = "CANCELED";

        public const string LOCATION_STATUS_ACTIVE = "ACTIVE";

        public const string LOCATION_CAPABILITIES_PROCESSING = "CREDIT_CARD_PROCESSING";

        public const string REFUND_STATUS_PENDING = "PENDING";

        public const string REFUND_STATUS_COMPLETED = "COMPLETED";

        public static string SystemName => "Payments.Square";

        public static string UserAgent => $"inovatiqa-1.0";

        public static string IntegrationId => "sqi_4efb0346e2ef4b1375319dcd6e9977c0";

        public static string OnePageCheckoutRouteName => "CheckoutOnePage";

        public static string PaymentFormScriptPath => "https://js.squareup.com/v2/paymentform";

        public static string SandboxPaymentFormScriptPath => "https://sandbox.web.squarecdn.com/v1/square.js";

        public static string SandboxBaseUrl => "https://connect.squareupsandbox.com";

        public static string CustomerIdAttribute => "SquareCustomerId";

        public static string AccessTokenRoute => "Payments.Square.AccessToken";

        public static string RenewAccessTokenTaskName => "Renew access token (Square payment)";

        public static string RenewAccessTokenTask => "Payments.Square.Services.RenewAccessTokenTask";

        public static int AccessTokenRenewalPeriodRecommended => 14;

        public static int AccessTokenRenewalPeriodMax => 30;

        public static string SandboxCredentialsPrefix => "sandbox-";

        public static string PaymentNote => "inovatiqa: {0}";

        public const int Authorize = 0;

        public const int Charge = 2;

        public static bool DisableBillingAddressCheckoutStep => false;

        public static bool ShipToSameAddress => true;

        public static decimal AdditionalFee => 0.0m;

        public static bool AdditionalFeePercentage => true;

        public static string ReturnRequestNumberMask => "";

        public static string CustomOrderNumberMask => "";

        public static bool AllowRePostingPayments => false;

        public static string PaymentMethodName => "Credit Card";

        #region Return Requests

        public static bool ReturnRequestsAllowFiles => false;

        #endregion

        #region Settings

        //public static string ApplicationId => "sandbox-sq0idb-w9qW1YJHmuVGgzBfIW4TUA"; original Inovatiqa commented for testing purpose
        public static string ApplicationId => "sandbox-sq0idb-W03tSWuQwZpjRo6rl3rSxg";

        public static string ApplicationSecret => "";

        public static string RefreshToken => "";

        //public static string AccessToken => "EAAAEBVN6obAsYNGG3WIxlQhjhItFPZCN_sSFsctyjOm5mtxgia0I1zmV9XnGpWu"; token commented for testing
        public static string AccessToken => "EAAAEDD2M6uGJmYkgShovGbvjPjzZZFEsvcYWn-X1B79v2Imx51cpfVrAX3DtLdB";

        public static bool UseSandbox => true;

        //public static string LocationId => "LTJB2SGJ5CDM2";
        public static string LocationId => "L99HYK1A9TSV8";

        public static string AccessTokenVerificationString => "";

        public static bool Use3ds => true;

        public const int RegenerateOrderGuidInterval = 180;

        #endregion

        #endregion

        #region Cache

        public static int CacheTime => 60;

        public static int DefaultCacheTime => 60;

        public static int BundledFilesCacheTime => 120;

        public static int ShortTermCacheTime => 5;

        public static string InovatiqaEntityCacheKey => "Inovatiqa.{0}.id-{1}";

        #endregion

        #region Caching defaults

        #region Categories

        public static CacheKey CategoriesListKey => new CacheKey("Inovatiqa.pres.admin.categories.list-{0}", CategoriesListPrefixCacheKey);
        public static string CategoriesListPrefixCacheKey => "Inovatiqa.pres.admin.categories.list";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : parent category ID
        /// {1} : show hidden records?
        /// {2} : current customer ID
        /// {3} : store ID
        /// </remarks>
        public static CacheKey CategoriesByParentCategoryIdCacheKey => new CacheKey("Inovatiqa.category.byparent-{0}-{1}-{2}-{3}", CategoriesByParentCategoryPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {0} : parent category ID
        /// </remarks>
        public static string CategoriesByParentCategoryPrefixCacheKey => "Inovatiqa.category.byparent-{0}";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : parent category id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// {3} : show hidden records?
        /// </remarks>
        public static CacheKey CategoriesChildIdentifiersCacheKey => new CacheKey("Inovatiqa.category.childidentifiers-{0}-{1}-{2}-{3}", CategoriesChildIdentifiersPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {0} : parent category ID
        /// </remarks>
        public static string CategoriesChildIdentifiersPrefixCacheKey => "Inovatiqa.category.childidentifiers-{0}";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static CacheKey CategoriesAllDisplayedOnHomepageCacheKey => new CacheKey("Inovatiqa.category.homepage.all", CategoriesDisplayedOnHomepagePrefixCacheKey);

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// {1} : roles of the current user
        /// </remarks>
        public static CacheKey CategoriesDisplayedOnHomepageWithoutHiddenCacheKey => new CacheKey("Inovatiqa.category.homepage.withouthidden-{0}-{1}", CategoriesDisplayedOnHomepagePrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string CategoriesDisplayedOnHomepagePrefixCacheKey => "Inovatiqa.category.homepage";

        /// <summary>
        /// Key for caching of category breadcrumb
        /// </summary>
        /// <remarks>
        /// {0} : category id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// {3} : language ID
        /// </remarks>
        public static CacheKey CategoryBreadcrumbCacheKey => new CacheKey("Inovatiqa.category.breadcrumb-{0}-{1}-{2}-{3}", CategoryBreadcrumbPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string CategoryBreadcrumbPrefixCacheKey => "Inovatiqa.category.breadcrumb";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// {1} : roles of the current user
        /// {2} : show hidden records?
        /// </remarks>
        public static CacheKey CategoriesAllCacheKey => new CacheKey("Inovatiqa.category.all-{0}-{1}-{2}", CategoriesAllPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string CategoriesAllPrefixCacheKey => "Inovatiqa.category.all";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// {1} : show hidden records?
        /// {2} : current customer ID
        /// {3} : store ID
        /// </remarks>
        public static CacheKey ProductCategoriesAllByProductIdCacheKey => new CacheKey("Inovatiqa.productcategory.allbyproductid-{0}-{1}-{2}-{3}", ProductCategoriesByProductPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string ProductCategoriesByProductPrefixCacheKey => "Inovatiqa.productcategory.allbyproductid-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer roles ID hash
        /// {1} : current store ID
        /// {2} : categories ID hash
        /// </remarks>
        public static CacheKey CategoryNumberOfProductsCacheKey => new CacheKey("Inovatiqa.productcategory.numberofproducts-{0}-{1}-{2}", CategoryNumberOfProductsPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string CategoryNumberOfProductsPrefixCacheKey => "Inovatiqa.productcategory.numberofproducts";

        #endregion

        #region Manufacturers

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// {1} : show hidden records?
        /// {2} : current customer ID
        /// {3} : store ID
        /// </remarks>
        public static CacheKey ProductManufacturersAllByProductIdCacheKey => new CacheKey("Inovatiqa.productmanufacturer.allbyproductid-{0}-{1}-{2}-{3}", ProductManufacturersByProductPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        public static string ProductManufacturersByProductPrefixCacheKey => "Inovatiqa.productmanufacturer.allbyproductid-{0}";

        #endregion

        #region Products

        /// <summary>
        /// Key for "related" product displayed on the product details page
        /// </summary>
        /// <remarks>
        /// {0} : current product id
        /// {1} : show hidden records?
        /// </remarks>
        public static CacheKey ProductsRelatedCacheKey => new CacheKey("Inovatiqa.product.related-{0}-{1}", ProductsRelatedPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        public static string ProductsRelatedPrefixCacheKey => "Inovatiqa.product.related-{0}";

        /// <summary>
        /// Key for "related" product identifiers displayed on the product details page
        /// </summary>
        /// <remarks>
        /// {0} : current product id
        /// </remarks>
        public static CacheKey ProductTierPricesCacheKey => new CacheKey("Inovatiqa.product.tierprices-{0}");

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static CacheKey ProductsAllDisplayedOnHomepageCacheKey => new CacheKey("Inovatiqa.product.homepage");

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {0} : product IDs hash
        /// </remarks>
        public static CacheKey ProductsByIdsCacheKey => new CacheKey("Inovatiqa.product.ids-{0}", ProductsByIdsPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string ProductsByIdsPrefixCacheKey => "Inovatiqa.product.ids";

        /// <summary>
        /// Gets a key for product prices
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : overridden product price
        /// {2} : additional charge
        /// {3} : include discounts (true, false)
        /// {4} : quantity
        /// {5} : roles of the current user
        /// {6} : current store ID
        /// </remarks>
        public static CacheKey ProductPriceCacheKey => new CacheKey("Inovatiqa.totals.productprice-{0}-{1}-{2}-{3}-{4}-{5}-{6}", ProductPricePrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// </remarks>
        public static string ProductPricePrefixCacheKey => "Inovatiqa.totals.productprice-{0}";

        #endregion

        #region Product attributes

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        public static CacheKey ProductAttributeMappingsAllCacheKey => new CacheKey("Inovatiqa.productattributemapping.all-{0}", ProductAttributeMappingsPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string ProductAttributeMappingsPrefixCacheKey => "Inovatiqa.productattributemapping.";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product attribute mapping ID
        /// </remarks>
        public static CacheKey ProductAttributeValuesAllCacheKey => new CacheKey("Inovatiqa.productattributevalue.all-{0}", ProductAttributeValuesAllPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string ProductAttributeValuesAllPrefixCacheKey => "Inovatiqa.productattributevalue.all";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        public static CacheKey ProductAttributeCombinationsAllCacheKey => new CacheKey("Inovatiqa.productattributecombination.all-{0}", ProductAttributeCombinationsAllPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string ProductAttributeCombinationsAllPrefixCacheKey => "Inovatiqa.productattributecombination.all";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : Product attribute ID
        /// </remarks>
        public static CacheKey PredefinedProductAttributeValuesAllCacheKey => new CacheKey("Inovatiqa.predefinedproductattributevalues.all-{0}");

        #endregion

        #region Product tags

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static CacheKey ProductTagAllCacheKey => new CacheKey("Inovatiqa.producttag.all", ProductTagPrefixCacheKey);

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// {1} : hash of list of customer roles IDs
        /// {2} : show hidden records?
        /// </remarks>
        public static CacheKey ProductTagCountCacheKey => new CacheKey("Inovatiqa.producttag.all.count-{0}-{1}-{2}", ProductTagPrefixCacheKey);

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        public static CacheKey ProductTagAllByProductIdCacheKey => new CacheKey("Inovatiqa.producttag.allbyproductid-{0}", ProductTagPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string ProductTagPrefixCacheKey => "Inovatiqa.producttag.";

        #endregion

        #region Review type

        /// <summary>
        /// Key for caching all review types
        /// </summary>
        public static CacheKey ReviewTypeAllCacheKey => new CacheKey("Inovatiqa.reviewType.all");

        /// <summary>
        /// Key for caching product review and review type mapping
        /// </summary>
        /// <remarks>
        /// {0} : product review ID
        /// </remarks>
        public static CacheKey ProductReviewReviewTypeMappingAllCacheKey => new CacheKey("Inovatiqa.productReviewReviewTypeMapping.all-{0}", ProductReviewReviewTypeMappingAllPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string ProductReviewReviewTypeMappingAllPrefixCacheKey => "Inovatiqa.productReviewReviewTypeMapping.all";

        #endregion

        #region Specification attributes

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// {1} : specification attribute option ID
        /// {2} : allow filtering
        /// {3} : show on product page
        /// </remarks>
        public static CacheKey ProductSpecificationAttributeAllByProductIdCacheKey => new CacheKey("Inovatiqa.productspecificationattribute.allbyproductid-{0}-{1}-{2}-{3}", ProductSpecificationAttributeAllByProductIdPrefixCacheKey, ProductSpecificationAttributeAllByProductIdsPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        public static string ProductSpecificationAttributeAllByProductIdPrefixCacheKey => "Inovatiqa.productspecificationattribute.allbyproductid-{0}";

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {1} (not 0, see the <ref>ProductSpecificationAttributeAllByProductIdCacheKey</ref>) :specification attribute option ID
        /// </remarks>
        public static string ProductSpecificationAttributeAllByProductIdsPrefixCacheKey => "Inovatiqa.productspecificationattribute.allbyproductid";

        /// <summary>
        /// Key for specification attributes caching (product details page)
        /// </summary>
        public static CacheKey SpecAttributesWithOptionsCacheKey => new CacheKey("Inovatiqa.productspecificationattribute.with.options");

        /// <summary>
        /// Key for specification attributes caching
        /// </summary>
        /// <remarks>
        /// {0} : specification attribute ID
        /// </remarks>
        public static CacheKey SpecAttributesOptionsCacheKey => new CacheKey("Inovatiqa.productspecificationattribute.options-{0}");

        #endregion

        #region Category template

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static CacheKey CategoryTemplatesAllCacheKey => new CacheKey("Inovatiqa.categorytemplate.all");

        #endregion

        #region Manufacturer template

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static CacheKey ManufacturerTemplatesAllCacheKey => new CacheKey("Inovatiqa.manufacturertemplate.all");

        #endregion

        #region Product template

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static CacheKey ProductTemplatesAllCacheKey => new CacheKey("Inovatiqa.producttemplates.all");

        #endregion

        #region URL records

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static CacheKey UrlRecordAllCacheKey => new CacheKey("Inovatiqa.urlrecord.all");

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : entity ID
        /// {1} : entity name
        /// {2} : language ID
        /// </remarks>
        public static CacheKey UrlRecordActiveByIdNameLanguageCacheKey => new CacheKey("Inovatiqa.urlrecord.active.id-name-language-{0}-{1}-{2}");

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : IDs hash
        /// </remarks>
        public static CacheKey UrlRecordByIdsCacheKey => new CacheKey("Inovatiqa.urlrecord.byids-{0}", UrlRecordByIdsPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string UrlRecordByIdsPrefixCacheKey => "Inovatiqa.urlrecord.byids";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : slug
        /// </remarks>
        public static CacheKey UrlRecordBySlugCacheKey => new CacheKey("Inovatiqa.urlrecord.active.slug-{0}");

        #endregion

        #region Customer roles

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// </remarks>
        public static CacheKey CustomerRolesAllCacheKey => new CacheKey("Inovatiqa.customerrole.all-{0}", CustomerRolesPrefixCacheKey);

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : system name
        /// </remarks>
        public static CacheKey CustomerRolesBySystemNameCacheKey => new CacheKey("Inovatiqa.customerrole.systemname-{0}", CustomerRolesPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string CustomerRolesPrefixCacheKey => "Inovatiqa.customerrole.";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer identifier
        /// {1} : show hidden
        /// </remarks>
        public static CacheKey CustomerRoleIdsCacheKey => new CacheKey("Inovatiqa.customer.customerrole.ids-{0}-{1}", CustomerCustomerRolesPrefixCacheKey);

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer identifier
        /// {1} : show hidden
        /// </remarks>
        public static CacheKey CustomerRolesCacheKey => new CacheKey("Inovatiqa.customer.customerrole-{0}-{1}", CustomerCustomerRolesPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string CustomerCustomerRolesPrefixCacheKey => "Inovatiqa.customer.customerrole";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer identifier
        /// </remarks>
        public static CacheKey CustomerAddressesByCustomerIdCacheKey => new CacheKey("Inovatiqa.customer.addresses.by.id-{0}", CustomerAddressesPrefixCacheKey);

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer identifier
        /// {1} : address identifier
        /// </remarks>
        public static CacheKey CustomerAddressCacheKeyCacheKey => new CacheKey("Inovatiqa.customer.addresses.address-{0}-{1}", CustomerAddressesPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string CustomerAddressesPrefixCacheKey => "Inovatiqa.customer.addresses";

        #endregion

        #endregion

        #region Purchase Order

        public const string VIEW_COMPONENT_NAME_PO = "PaymentPurchaseOrder";

        public static bool ShippableProductRequired => true;

        public static string PurchaseOrderPaymentName => "Payments.PurchaseOrder";

        #endregion

        #region Passwords

        public static int PasswordRecoveryLinkDaysValid => 1;

        #endregion

        #region Elastic
        public static string ElasticEndPoint => "https://elastic-inovatiqa-prod-01.es.centralus.azure.elastic-cloud.com";
        //public static string ElasticEndPoint => "https://inovatiqaelasticcloud.es.eastus2.azure.elastic-cloud.com";
        public static string ElasticUsername => "elastic";
        public static string ElasticPassword => "nhlT60QSDLGaJKyH7CCArPPb";
        public static string DefaultIndexName => "inovatiqa";

        #endregion
    }
}
