using AutoMapper;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Web.Areas.Admin.Models.Catalog;
using Inovatiqa.Web.Areas.Admin.Models.Common;
using Inovatiqa.Web.Areas.Admin.Models.Customers;
using Inovatiqa.Web.Areas.Admin.Models.Directory;
using Inovatiqa.Web.Areas.Admin.Models.Logging;
using Inovatiqa.Web.Areas.Admin.Models.Orders;
using Inovatiqa.Web.Areas.Admin.Models.Settings;
using Inovatiqa.Web.Areas.Admin.Models.Shipping;
using Inovatiqa.Web.Areas.Admin.Models.ShoppingCart;
using Inovatiqa.Web.Areas.Admin.Models.Vendors;
using Inovatiqa.Web.Framework.Models;

namespace Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper
{
    public class AdminMapperConfiguration : Profile, IOrderedMapperProfile
    {
        #region Ctor

        public AdminMapperConfiguration()
        {
            CreateCatalogMaps();
            CreateCommonMaps();
            CreateShippingMaps();
            CreateOrdersMaps();
            CreateCustomersMaps();
            CreateVendorsMaps();
            CreateLoggingMaps();
            CreateDirectoryMaps();

            ForAllMaps((mapConfiguration, map) =>
            {
                if (typeof(BaseInovatiqaModel).IsAssignableFrom(mapConfiguration.DestinationType))
                {
                    map.ForMember(nameof(BaseInovatiqaModel.CustomProperties), options => options.Ignore());
                }

                if (typeof(ISettingsModel).IsAssignableFrom(mapConfiguration.DestinationType))
                    map.ForMember(nameof(ISettingsModel.ActiveStoreScopeConfiguration), options => options.Ignore());

                if (typeof(ILocalizedModel).IsAssignableFrom(mapConfiguration.DestinationType))
                    map.ForMember(nameof(ILocalizedModel<ILocalizedModel>.Locales), options => options.Ignore());

                if (typeof(IStoreMappingSupportedModel).IsAssignableFrom(mapConfiguration.DestinationType))
                {
                    map.ForMember(nameof(IStoreMappingSupportedModel.AvailableStores), options => options.Ignore());
                    map.ForMember(nameof(IStoreMappingSupportedModel.SelectedStoreIds), options => options.Ignore());
                }

                if (typeof(IAclSupported).IsAssignableFrom(mapConfiguration.DestinationType))
                    map.ForMember(nameof(IAclSupported.SubjectToAcl), options => options.Ignore());
                if (typeof(IAclSupportedModel).IsAssignableFrom(mapConfiguration.DestinationType))
                {
                    map.ForMember(nameof(IAclSupportedModel.AvailableCustomerRoles), options => options.Ignore());
                    map.ForMember(nameof(IAclSupportedModel.SelectedCustomerRoleIds), options => options.Ignore());
                }

                if (typeof(IDiscountSupportedModel).IsAssignableFrom(mapConfiguration.DestinationType))
                {
                    map.ForMember(nameof(IDiscountSupportedModel.AvailableDiscounts), options => options.Ignore());
                    map.ForMember(nameof(IDiscountSupportedModel.SelectedDiscountIds), options => options.Ignore());
                }
            });
        }

        #endregion

        #region Utilities

        protected virtual void CreateDirectoryMaps()
        {
            CreateMap<Country, CountryModel>()
                .ForMember(model => model.NumberOfStates, options => options.Ignore())
                .ForMember(model => model.StateProvinceSearchModel, options => options.Ignore());
            CreateMap<CountryModel, Country>();
            
            CreateMap<StateProvince, StateProvinceModel>();
            CreateMap<StateProvinceModel, StateProvince>();
        }

        protected virtual void CreateLoggingMaps()
        {
            CreateMap<ActivityLog, ActivityLogModel>()
                .ForMember(model => model.ActivityLogTypeName, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.CustomerEmail, options => options.Ignore());
            CreateMap<ActivityLogModel, ActivityLog>()
                .ForMember(entity => entity.ActivityLogTypeId, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.EntityId, options => options.Ignore())
                .ForMember(entity => entity.EntityName, options => options.Ignore());

            CreateMap<ActivityLogType, ActivityLogTypeModel>();
            CreateMap<ActivityLogTypeModel, ActivityLogType>()
                .ForMember(entity => entity.SystemKeyword, options => options.Ignore());

            CreateMap<Log, LogModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.FullMessage, options => options.Ignore())
                .ForMember(model => model.CustomerEmail, options => options.Ignore());
            CreateMap<LogModel, Log>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.LogLevelId, options => options.Ignore());
        }

        protected virtual void CreateVendorsMaps()
        {
            CreateMap<Vendor, VendorModel>()
                .ForMember(model => model.Address, options => options.Ignore())
                .ForMember(model => model.AddVendorNoteMessage, options => options.Ignore())
                .ForMember(model => model.AssociatedCustomers, options => options.Ignore())
                .ForMember(model => model.SeName, options => options.Ignore())
                .ForMember(model => model.VendorAttributes, options => options.Ignore())
                .ForMember(model => model.VendorNoteSearchModel, options => options.Ignore());
            CreateMap<VendorModel, Vendor>()
                .ForMember(entity => entity.Deleted, options => options.Ignore());

            CreateMap<VendorNote, VendorNoteModel>()
               .ForMember(model => model.CreatedOn, options => options.Ignore())
               .ForMember(model => model.Note, options => options.Ignore());

            CreateMap<VendorAttribute, VendorAttributeModel>()
                .ForMember(model => model.AttributeControlTypeName, options => options.Ignore())
                .ForMember(model => model.VendorAttributeValueSearchModel, options => options.Ignore());
            CreateMap<VendorAttributeModel, VendorAttribute>()
                .ForMember(entity => entity.AttributeControlTypeId, options => options.Ignore());

            CreateMap<VendorAttributeValue, VendorAttributeValueModel>();
            CreateMap<VendorAttributeValueModel, VendorAttributeValue>();
        }

        protected virtual void CreateCustomersMaps()
        {
            CreateMap<ActivityLog, CustomerActivityLogModel>()
              .ForMember(model => model.CreatedOn, options => options.Ignore())
              .ForMember(model => model.ActivityLogTypeName, options => options.Ignore());

            CreateMap<CustomerAttribute, CustomerAttributeModel>()
                .ForMember(model => model.AttributeControlTypeName, options => options.Ignore())
                .ForMember(model => model.CustomerAttributeValueSearchModel, options => options.Ignore());
            CreateMap<CustomerAttributeModel, CustomerAttribute>()
                .ForMember(entity => entity.AttributeControlTypeId, options => options.Ignore());

            CreateMap<CustomerAttributeValue, CustomerAttributeValueModel>();
            CreateMap<CustomerAttributeValueModel, CustomerAttributeValue>();

            CreateMap<CustomerRole, CustomerRoleModel>()
                .ForMember(model => model.PurchasedWithProductName, options => options.Ignore())
                .ForMember(model => model.TaxDisplayTypeValues, options => options.Ignore());
            CreateMap<CustomerRoleModel, CustomerRole>();
            CreateMap<RewardPointsHistory, CustomerRewardPointsModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.PointsBalance, options => options.Ignore())
                .ForMember(model => model.EndDate, options => options.Ignore())
                .ForMember(model => model.StoreName, options => options.Ignore());

            CreateMap<ActivityLog, CustomerActivityLogModel>()
               .ForMember(model => model.CreatedOn, options => options.Ignore())
               .ForMember(model => model.ActivityLogTypeName, options => options.Ignore());

            CreateMap<Customer, CustomerModel>()
                .ForMember(model => model.Email, options => options.Ignore())
                .ForMember(model => model.FullName, options => options.Ignore())
                .ForMember(model => model.Company, options => options.Ignore())
                .ForMember(model => model.Phone, options => options.Ignore())
                .ForMember(model => model.ZipPostalCode, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.LastActivityDate, options => options.Ignore())
                .ForMember(model => model.CustomerRoleNames, options => options.Ignore())
                .ForMember(model => model.AvatarUrl, options => options.Ignore())
                .ForMember(model => model.UsernamesEnabled, options => options.Ignore())
                .ForMember(model => model.Password, options => options.Ignore())
                .ForMember(model => model.AvailableVendors, options => options.Ignore())
                .ForMember(model => model.GenderEnabled, options => options.Ignore())
                .ForMember(model => model.Gender, options => options.Ignore())
                .ForMember(model => model.FirstNameEnabled, options => options.Ignore())
                .ForMember(model => model.FirstName, options => options.Ignore())
                .ForMember(model => model.LastNameEnabled, options => options.Ignore())
                .ForMember(model => model.LastName, options => options.Ignore())
                .ForMember(model => model.DateOfBirthEnabled, options => options.Ignore())
                .ForMember(model => model.DateOfBirth, options => options.Ignore())
                .ForMember(model => model.CompanyEnabled, options => options.Ignore())
                .ForMember(model => model.StreetAddressEnabled, options => options.Ignore())
                .ForMember(model => model.StreetAddress, options => options.Ignore())
                .ForMember(model => model.StreetAddress2Enabled, options => options.Ignore())
                .ForMember(model => model.StreetAddress2, options => options.Ignore())
                .ForMember(model => model.ZipPostalCodeEnabled, options => options.Ignore())
                .ForMember(model => model.CityEnabled, options => options.Ignore())
                .ForMember(model => model.City, options => options.Ignore())
                .ForMember(model => model.CountyEnabled, options => options.Ignore())
                .ForMember(model => model.County, options => options.Ignore())
                .ForMember(model => model.CountryEnabled, options => options.Ignore())
                .ForMember(model => model.CountryId, options => options.Ignore())
                .ForMember(model => model.AvailableCountries, options => options.Ignore())
                .ForMember(model => model.StateProvinceEnabled, options => options.Ignore())
                .ForMember(model => model.StateProvinceId, options => options.Ignore())
                .ForMember(model => model.AvailableStates, options => options.Ignore())
                .ForMember(model => model.PhoneEnabled, options => options.Ignore())
                .ForMember(model => model.FaxEnabled, options => options.Ignore())
                .ForMember(model => model.Fax, options => options.Ignore())
                .ForMember(model => model.CustomerAttributes, options => options.Ignore())
                .ForMember(model => model.RegisteredInStore, options => options.Ignore())
                .ForMember(model => model.DisplayRegisteredInStore, options => options.Ignore())
                .ForMember(model => model.AffiliateName, options => options.Ignore())
                .ForMember(model => model.TimeZoneId, options => options.Ignore())
                .ForMember(model => model.AllowCustomersToSetTimeZone, options => options.Ignore())
                .ForMember(model => model.AvailableTimeZones, options => options.Ignore())
                .ForMember(model => model.VatNumber, options => options.Ignore())
                .ForMember(model => model.VatNumberStatusNote, options => options.Ignore())
                .ForMember(model => model.DisplayVatNumber, options => options.Ignore())
                .ForMember(model => model.LastVisitedPage, options => options.Ignore())
                .ForMember(model => model.AvailableNewsletterSubscriptionStores, options => options.Ignore())
                .ForMember(model => model.SelectedNewsletterSubscriptionStoreIds, options => options.Ignore())
                .ForMember(model => model.DisplayRewardPointsHistory, options => options.Ignore())
                .ForMember(model => model.AddRewardPoints, options => options.Ignore())
                .ForMember(model => model.CustomerRewardPointsSearchModel, options => options.Ignore())
                .ForMember(model => model.SendEmail, options => options.Ignore())
                .ForMember(model => model.SendPm, options => options.Ignore())
                .ForMember(model => model.AllowSendingOfPrivateMessage, options => options.Ignore())
                .ForMember(model => model.AllowSendingOfWelcomeMessage, options => options.Ignore())
                .ForMember(model => model.AllowReSendingOfActivationMessage, options => options.Ignore())
                .ForMember(model => model.GdprEnabled, options => options.Ignore())
                .ForMember(model => model.CustomerAssociatedExternalAuthRecordsSearchModel, options => options.Ignore())
                .ForMember(model => model.CustomerAddressSearchModel, options => options.Ignore())
                .ForMember(model => model.CustomerOrderSearchModel, options => options.Ignore())
                .ForMember(model => model.CustomerShoppingCartSearchModel, options => options.Ignore())
                .ForMember(model => model.CustomerActivityLogSearchModel, options => options.Ignore())
                .ForMember(model => model.CustomerBackInStockSubscriptionSearchModel, options => options.Ignore());

            CreateMap<CustomerModel, Customer>()
                .ForMember(entity => entity.CustomerGuid, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.LastActivityDateUtc, options => options.Ignore())
                .ForMember(entity => entity.EmailToRevalidate, options => options.Ignore())
                .ForMember(entity => entity.HasShoppingCartItems, options => options.Ignore())
                .ForMember(entity => entity.RequireReLogin, options => options.Ignore())
                .ForMember(entity => entity.FailedLoginAttempts, options => options.Ignore())
                .ForMember(entity => entity.CannotLoginUntilDateUtc, options => options.Ignore())
                .ForMember(entity => entity.Deleted, options => options.Ignore())
                .ForMember(entity => entity.IsSystemAccount, options => options.Ignore())
                .ForMember(entity => entity.SystemName, options => options.Ignore())
                .ForMember(entity => entity.LastLoginDateUtc, options => options.Ignore())
                .ForMember(entity => entity.BillingAddressId, options => options.Ignore())
                .ForMember(entity => entity.ShippingAddressId, options => options.Ignore())
                .ForMember(entity => entity.RegisteredInStoreId, options => options.Ignore());

            CreateMap<Customer, OnlineCustomerModel>()
                .ForMember(model => model.LastActivityDate, options => options.Ignore())
                .ForMember(model => model.CustomerInfo, options => options.Ignore())
                .ForMember(model => model.LastIpAddress, options => options.Ignore())
                .ForMember(model => model.Location, options => options.Ignore())
                .ForMember(model => model.LastVisitedPage, options => options.Ignore());

            CreateMap<BackInStockSubscription, CustomerBackInStockSubscriptionModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.StoreName, options => options.Ignore())
                .ForMember(model => model.ProductName, options => options.Ignore());
        }

        protected virtual void CreateOrdersMaps()
        {
            CreateMap<Order, CustomerOrderModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.OrderStatus, options => options.Ignore())
                .ForMember(model => model.PaymentStatus, options => options.Ignore())
                .ForMember(model => model.ShippingStatus, options => options.Ignore())
                .ForMember(model => model.OrderTotal, options => options.Ignore())
                .ForMember(model => model.StoreName, options => options.Ignore());

            CreateMap<OrderNote, OrderNoteModel>()
                .ForMember(model => model.DownloadGuid, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore());

            CreateMap<CheckoutAttribute, CheckoutAttributeModel>()
                .ForMember(model => model.AttributeControlTypeName, options => options.Ignore())
                .ForMember(model => model.AvailableTaxCategories, options => options.Ignore())
                .ForMember(model => model.CheckoutAttributeValueSearchModel, options => options.Ignore())
                .ForMember(model => model.ConditionAllowed, options => options.Ignore())
                .ForMember(model => model.ConditionModel, options => options.Ignore());
            CreateMap<CheckoutAttributeModel, CheckoutAttribute>()
                .ForMember(entity => entity.AttributeControlTypeId, options => options.Ignore())
                .ForMember(entity => entity.ConditionAttributeXml, options => options.Ignore());

            CreateMap<CheckoutAttributeValue, CheckoutAttributeValueModel>()
                .ForMember(model => model.BaseWeightIn, options => options.Ignore())
                .ForMember(model => model.DisplayColorSquaresRgb, options => options.Ignore())
                .ForMember(model => model.PrimaryStoreCurrencyCode, options => options.Ignore());
            CreateMap<CheckoutAttributeValueModel, CheckoutAttributeValue>();

            CreateMap<ReturnRequestAction, ReturnRequestActionModel>();
            CreateMap<ReturnRequestActionModel, ReturnRequestAction>();

            CreateMap<ReturnRequestReason, ReturnRequestReasonModel>();
            CreateMap<ReturnRequestReasonModel, ReturnRequestReason>();

            CreateMap<ReturnRequest, ReturnRequestModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.CustomerInfo, options => options.Ignore())
                .ForMember(model => model.ReturnRequestStatusStr, options => options.Ignore())
                .ForMember(model => model.ProductId, options => options.Ignore())
                .ForMember(model => model.ProductName, options => options.Ignore())
                .ForMember(model => model.OrderId, options => options.Ignore())
                .ForMember(model => model.AttributeInfo, options => options.Ignore())
                .ForMember(model => model.CustomOrderNumber, options => options.Ignore())
                .ForMember(model => model.UploadedFileGuid, options => options.Ignore())
                .ForMember(model => model.ReturnRequestStatusStr, options => options.Ignore());
            CreateMap<ReturnRequestModel, ReturnRequest>()
                 .ForMember(entity => entity.CustomNumber, options => options.Ignore())
                 .ForMember(entity => entity.StoreId, options => options.Ignore())
                 .ForMember(entity => entity.OrderItemId, options => options.Ignore())
                 .ForMember(entity => entity.UploadedFileId, options => options.Ignore())
                 .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                 .ForMember(entity => entity.ReturnRequestStatusId, options => options.Ignore())
                 .ForMember(entity => entity.CustomerId, options => options.Ignore())
                 .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());
           
            CreateMap<ShoppingCartItem, ShoppingCartItemModel>()
                .ForMember(model => model.Store, options => options.Ignore())
                .ForMember(model => model.AttributeInfo, options => options.Ignore())
                .ForMember(model => model.UnitPrice, options => options.Ignore())
                .ForMember(model => model.UpdatedOn, options => options.Ignore())
                .ForMember(model => model.ProductName, options => options.Ignore())
                .ForMember(model => model.Total, options => options.Ignore());
        }

        protected virtual void CreateCatalogMaps()
        {
            CreateMap<ProductManufacturerMapping, ManufacturerProductModel>()
                .ForMember(model => model.ProductName, options => options.Ignore());
            CreateMap<ManufacturerProductModel, ProductManufacturerMapping>()
                .ForMember(entity => entity.ManufacturerId, options => options.Ignore())
                .ForMember(entity => entity.ProductId, options => options.Ignore());

            CreateMap<Manufacturer, ManufacturerModel>()
                .ForMember(model => model.AvailableManufacturerTemplates, options => options.Ignore())
                .ForMember(model => model.ManufacturerProductSearchModel, options => options.Ignore())
                .ForMember(model => model.SeName, options => options.Ignore());
            CreateMap<ManufacturerModel, Manufacturer>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.Deleted, options => options.Ignore())
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());

            CreateMap<ProductCategoryMapping, CategoryProductModel>()
                .ForMember(model => model.ProductName, options => options.Ignore());
            CreateMap<CategoryProductModel, ProductCategoryMapping>()
                .ForMember(entity => entity.CategoryId, options => options.Ignore())
                .ForMember(entity => entity.ProductId, options => options.Ignore());

            CreateMap<Category, CategoryModel>()
                .ForMember(model => model.AvailableCategories, options => options.Ignore())
                .ForMember(model => model.AvailableCategoryTemplates, options => options.Ignore())
                .ForMember(model => model.Breadcrumb, options => options.Ignore())
                .ForMember(model => model.CategoryProductSearchModel, options => options.Ignore())
                .ForMember(model => model.SeName, options => options.Ignore());
            CreateMap<CategoryModel, Category>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.Deleted, options => options.Ignore())
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());

            CreateMap<ProductPictureMapping, ProductPictureModel>()
                .ForMember(model => model.OverrideAltAttribute, options => options.Ignore())
                .ForMember(model => model.OverrideTitleAttribute, options => options.Ignore())
                .ForMember(model => model.PictureUrl, options => options.Ignore());
            
            CreateMap<TierPrice, TierPriceModel>()
                .ForMember(model => model.Store, options => options.Ignore())
                .ForMember(model => model.AvailableCustomerRoles, options => options.Ignore())
                .ForMember(model => model.AvailableStores, options => options.Ignore())
                .ForMember(model => model.CustomerRole, options => options.Ignore());
            CreateMap<TierPriceModel, TierPrice>()
                .ForMember(entity => entity.CustomerRoleId, options => options.Ignore())
                .ForMember(entity => entity.ProductId, options => options.Ignore());

            CreateMap<ProductEditorSettings, ProductEditorSettingsModel>();
            CreateMap<ProductEditorSettingsModel, ProductEditorSettings>();

            CreateMap<Product, ProductModel>()
                .ForMember(model => model.AddPictureModel, options => options.Ignore())
                .ForMember(model => model.AssociatedProductSearchModel, options => options.Ignore())
                .ForMember(model => model.AssociatedToProductId, options => options.Ignore())
                .ForMember(model => model.AssociatedToProductName, options => options.Ignore())
                .ForMember(model => model.AvailableBasepriceBaseUnits, options => options.Ignore())
                .ForMember(model => model.AvailableBasepriceUnits, options => options.Ignore())
                .ForMember(model => model.AvailableCategories, options => options.Ignore())
                .ForMember(model => model.AvailableDeliveryDates, options => options.Ignore())
                .ForMember(model => model.AvailableManufacturers, options => options.Ignore())
                .ForMember(model => model.AvailableProductAvailabilityRanges, options => options.Ignore())
                .ForMember(model => model.AvailableProductTemplates, options => options.Ignore())
                .ForMember(model => model.AvailableTaxCategories, options => options.Ignore())
                .ForMember(model => model.AvailableVendors, options => options.Ignore())
                .ForMember(model => model.AvailableWarehouses, options => options.Ignore())
                .ForMember(model => model.BaseDimensionIn, options => options.Ignore())
                .ForMember(model => model.BaseWeightIn, options => options.Ignore())
                .ForMember(model => model.CopyProductModel, options => options.Ignore())
                .ForMember(model => model.CrossSellProductSearchModel, options => options.Ignore())
                .ForMember(model => model.HasAvailableSpecificationAttributes, options => options.Ignore())
                .ForMember(model => model.IsLoggedInAsVendor, options => options.Ignore())
                .ForMember(model => model.LastStockQuantity, options => options.Ignore())
                .ForMember(model => model.PictureThumbnailUrl, options => options.Ignore())
                .ForMember(model => model.PrimaryStoreCurrencyCode, options => options.Ignore())
                .ForMember(model => model.ProductAttributeCombinationSearchModel, options => options.Ignore())
                .ForMember(model => model.ProductAttributeMappingSearchModel, options => options.Ignore())
                .ForMember(model => model.ProductAttributesExist, options => options.Ignore())
                .ForMember(model => model.CanCreateCombinations, options => options.Ignore())
                .ForMember(model => model.ProductEditorSettingsModel, options => options.Ignore())
                .ForMember(model => model.ProductOrderSearchModel, options => options.Ignore())
                .ForMember(model => model.ProductPictureModels, options => options.Ignore())
                .ForMember(model => model.ProductPictureSearchModel, options => options.Ignore())
                .ForMember(model => model.ProductSpecificationAttributeSearchModel, options => options.Ignore())
                .ForMember(model => model.ProductsTypesSupportedByProductTemplates, options => options.Ignore())
                .ForMember(model => model.ProductTags, options => options.Ignore())
                .ForMember(model => model.ProductTypeName, options => options.Ignore())
                .ForMember(model => model.ProductWarehouseInventoryModels, options => options.Ignore())
                .ForMember(model => model.RelatedProductSearchModel, options => options.Ignore())
                .ForMember(model => model.SelectedCategoryIds, options => options.Ignore())
                .ForMember(model => model.SelectedManufacturerIds, options => options.Ignore())
                .ForMember(model => model.SeName, options => options.Ignore())
                //.ForMember(model => model.StockQuantityHistory, options => options.Ignore())
                .ForMember(model => model.StockQuantityHistorySearchModel, options => options.Ignore())
                .ForMember(model => model.StockQuantityStr, options => options.Ignore())
                .ForMember(model => model.TierPriceSearchModel, options => options.Ignore())
                .ForMember(model => model.InitialProductTags, options => options.Ignore());
            CreateMap<ProductModel, Product>()
                .ForMember(entity => entity.ApprovedRatingSum, options => options.Ignore())
                .ForMember(entity => entity.ApprovedTotalReviews, options => options.Ignore())
                .ForMember(entity => entity.BackorderModeId, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.Deleted, options => options.Ignore())
                .ForMember(entity => entity.DownloadActivationTypeId, options => options.Ignore())
                .ForMember(entity => entity.GiftCardTypeId, options => options.Ignore())
                .ForMember(entity => entity.HasDiscountsApplied, options => options.Ignore())
                .ForMember(entity => entity.HasTierPrices, options => options.Ignore())
                .ForMember(entity => entity.LowStockActivityId, options => options.Ignore())
                .ForMember(entity => entity.ManageInventoryMethodId, options => options.Ignore())
                .ForMember(entity => entity.NotApprovedRatingSum, options => options.Ignore())
                .ForMember(entity => entity.NotApprovedTotalReviews, options => options.Ignore())
                .ForMember(entity => entity.ParentGroupedProductId, options => options.Ignore())
                .ForMember(entity => entity.ProductTypeId, options => options.Ignore())
                .ForMember(entity => entity.RecurringCyclePeriodId, options => options.Ignore())
                .ForMember(entity => entity.RentalPricePeriodId, options => options.Ignore())
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());
            CreateMap<StockQuantityHistory, StockQuantityHistoryModel>()
                .ForMember(model => model.WarehouseName, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.AttributeCombination, options => options.Ignore());
        }

        protected virtual void CreateCommonMaps()
        {
            CreateMap<Address, AddressModel>()
                .ForMember(model => model.AddressHtml, options => options.Ignore())
                .ForMember(model => model.AvailableCountries, options => options.Ignore())
                .ForMember(model => model.AvailableStates, options => options.Ignore())
                .ForMember(model => model.CityEnabled, options => options.Ignore())
                .ForMember(model => model.CityRequired, options => options.Ignore())
                .ForMember(model => model.CompanyEnabled, options => options.Ignore())
                .ForMember(model => model.CompanyRequired, options => options.Ignore())
                .ForMember(model => model.CountryEnabled, options => options.Ignore())
                .ForMember(model => model.CountryName, options => options.Ignore())
                .ForMember(model => model.CountryRequired, options => options.Ignore())
                .ForMember(model => model.CountyEnabled, options => options.Ignore())
                .ForMember(model => model.CountyRequired, options => options.Ignore())
                .ForMember(model => model.CustomAddressAttributes, options => options.Ignore())
                .ForMember(model => model.EmailEnabled, options => options.Ignore())
                .ForMember(model => model.EmailRequired, options => options.Ignore())
                .ForMember(model => model.FaxEnabled, options => options.Ignore())
                .ForMember(model => model.FaxRequired, options => options.Ignore())
                .ForMember(model => model.FirstNameEnabled, options => options.Ignore())
                .ForMember(model => model.FirstNameRequired, options => options.Ignore())
                .ForMember(model => model.FormattedCustomAddressAttributes, options => options.Ignore())
                .ForMember(model => model.LastNameEnabled, options => options.Ignore())
                .ForMember(model => model.LastNameRequired, options => options.Ignore())
                .ForMember(model => model.PhoneEnabled, options => options.Ignore())
                .ForMember(model => model.PhoneRequired, options => options.Ignore())
                .ForMember(model => model.StateProvinceEnabled, options => options.Ignore())
                .ForMember(model => model.StateProvinceName, options => options.Ignore())
                .ForMember(model => model.StreetAddress2Enabled, options => options.Ignore())
                .ForMember(model => model.StreetAddress2Required, options => options.Ignore())
                .ForMember(model => model.StreetAddressEnabled, options => options.Ignore())
                .ForMember(model => model.StreetAddressRequired, options => options.Ignore())
                .ForMember(model => model.ZipPostalCodeEnabled, options => options.Ignore())
                .ForMember(model => model.ZipPostalCodeRequired, options => options.Ignore());
            CreateMap<AddressModel, Address>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.CustomAttributes, options => options.Ignore());

            CreateMap<AddressAttribute, Inovatiqa.Web.Areas.Admin.Models.Common.AddressAttributeModel>()
                .ForMember(model => model.AddressAttributeValueSearchModel, options => options.Ignore())
                .ForMember(model => model.AttributeControlTypeName, options => options.Ignore());
            CreateMap<AddressAttributeModel, AddressAttribute>()
                .ForMember(entity => entity.AttributeControlTypeId, options => options.Ignore());

            CreateMap<AddressAttributeValue, Inovatiqa.Web.Areas.Admin.Models.Common.AddressAttributeValueModel>();
            CreateMap<AddressAttributeValueModel, AddressAttributeValue>();

            //CreateMap<AddressSettings, AddressSettingsModel>();
            //CreateMap<AddressSettingsModel, AddressSettings>()
            //    .ForMember(settings => settings.PreselectCountryIfOnlyOne, options => options.Ignore());

            //CreateMap<Setting, SettingModel>()
            //    .ForMember(setting => setting.AvailableStores, options => options.Ignore())
            //    .ForMember(setting => setting.Store, options => options.Ignore());
        }

        protected virtual void CreateShippingMaps()
        {
            CreateMap<Shipment, ShipmentModel>()
                .ForMember(model => model.ShippedDate, options => options.Ignore())
                .ForMember(model => model.DeliveryDate, options => options.Ignore())
                .ForMember(model => model.TotalWeight, options => options.Ignore())
                .ForMember(model => model.TrackingNumberUrl, options => options.Ignore())
                .ForMember(model => model.Items, options => options.Ignore())
                .ForMember(model => model.ShipmentStatusEvents, options => options.Ignore())
                .ForMember(model => model.CanShip, options => options.Ignore())
                .ForMember(model => model.CanDeliver, options => options.Ignore())
                .ForMember(model => model.CustomOrderNumber, options => options.Ignore());

            CreateMap<DeliveryDate, DeliveryDateModel>();
            CreateMap<DeliveryDateModel, DeliveryDate>();

            CreateMap<ProductAvailabilityRange, ProductAvailabilityRangeModel>();
            CreateMap<ProductAvailabilityRangeModel, ProductAvailabilityRange>();

            CreateMap<ShippingMethod, ShippingMethodModel>();
            CreateMap<ShippingMethodModel, ShippingMethod>();
        }
        #endregion

        #region Properties

        public int Order => 0;

        #endregion
    }
}