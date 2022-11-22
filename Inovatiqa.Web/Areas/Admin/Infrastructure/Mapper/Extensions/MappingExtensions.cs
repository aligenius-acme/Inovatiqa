using Inovatiqa.Web.Framework.Models;
using Inovatiqa.Core;
using System;
using Inovatiqa.Database.Models;
using Inovatiqa.Web.Areas.Admin.Models.Catalog;
using Inovatiqa.Web.Areas.Admin.Models.Orders;
using Inovatiqa.Web.Areas.Admin.Models.Common;
using Inovatiqa.Web.Areas.Admin.Models.Customers;
using Inovatiqa.Web.Areas.Admin.Models.Vendors;
using Inovatiqa.Web.Areas.Admin.Models.Logging;
using Inovatiqa.Web.Areas.Admin.Models.Directory;
using Inovatiqa.Web.Areas.Admin.Models.Shipping;

namespace Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions
{
    public static class MappingExtensions
    {
        #region Utilities

        private static TDestination Map<TDestination>(this object source)
        {
            return AutoMapperConfiguration.Mapper.Map<TDestination>(source);
        }

        private static TDestination Map<TSource, TDestination>(this TSource source, TDestination destination)
        {
            return AutoMapperConfiguration.Mapper.Map(source, destination);
        }

        #endregion

        #region Methods

        #region Model-Entity

        public static TModel ToTierPriceEntity<TModel>(this TierPriceModel entity) where TModel : TierPrice
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToCustomerRoleEntity<TModel>(this CustomerRoleModel entity) where TModel : CustomerRole
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToReturnRequestEntity<TModel>(this ReturnRequestModel entity) where TModel : ReturnRequest
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToReturnRequestReasonEntity<TModel>(this ReturnRequestReasonModel entity) where TModel : ReturnRequestReason
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToReturnRequestActionEntity<TModel>(this ReturnRequestActionModel entity) where TModel : ReturnRequestAction
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToManufacturerEntity<TModel>(this ManufacturerModel entity) where TModel : Manufacturer
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToVendorEntity<TModel>(this VendorModel entity) where TModel : Vendor
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToAddressEntity<TModel>(this AddressModel entity) where TModel : Address
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToCustomerEntity<TModel>(this CustomerModel entity) where TModel : Customer
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToActivityLogTypeModel<TModel>(this ActivityLogType entity) where TModel : ActivityLogTypeModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToActivityLogModel<TModel>(this ActivityLog entity) where TModel : ActivityLogModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToShippingMethodModel<TModel>(this ShippingMethod entity) where TModel : ShippingMethodModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToDeliveryDateModel<TModel>(this DeliveryDate entity) where TModel : DeliveryDateModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToProductAvailabilityRangeModel<TModel>(this ProductAvailabilityRange entity) where TModel : ProductAvailabilityRangeModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToStateProvinceModel<TModel>(this StateProvince entity) where TModel : StateProvinceModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToCountryModel<TModel>(this Country entity) where TModel : CountryModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToCustomerRoleModel<TModel>(this CustomerRole entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToVendorModel<TModel>(this Vendor entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToCustomerModel<TModel>(this Customer entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToCategoryModel<TModel>(this Category entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToShoppingCartItemModel<TModel>(this ShoppingCartItem entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToCustomerActivityLogModel<TModel>(this ActivityLog entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToOnlineCustomerModel<TModel>(this Customer entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToManufacturerProductModel<TModel>(this ProductManufacturerMapping entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToManufacturerModel<TModel>(this Manufacturer entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToCategoryProductModel<TModel>(this ProductCategoryMapping entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToProductPictureModel<TModel>(this ProductPictureMapping entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToReturnRequestModel<TModel>(this ReturnRequest entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToReturnRequestReasonModel<TModel>(this ReturnRequestReason entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }
        public static TModel ToReturnRequestActionModel<TModel>(this ReturnRequestAction entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToProductModel<TModel>(this Product entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToTierPriceModel<TModel>(this TierPrice entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToProductSpecificationAttributeMappingEntity<TModel>(this AddSpecificationAttributeModel entity) where TModel : ProductSpecificationAttributeMapping
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToAddressModel<TModel>(this Address entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToCustomerOrderModel<TModel>(this Order entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static AddressModel ToAddressModel<Address, AddressModel>(this Address entity, AddressModel model)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return entity.Map(model);
        }

        public static Address ToAddressEntity<AddressModel, Address>(this AddressModel model, Address entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return model.Map(entity);
        }

        public static Product ToProductEntity<ProductModel, Product>(this ProductModel model, Product entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return model.Map(entity);
        }

        public static ReturnRequest ToReturnRequestEntity<ReturnRequestModel, ReturnRequest>(this ReturnRequestModel model, ReturnRequest entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return model.Map(entity);
        }

        public static ProductCategoryMapping ToProductCategoryMappingEntity<CategoryProductModel, ProductCategoryMapping>(this CategoryProductModel model, ProductCategoryMapping entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return model.Map(entity);
        }

        public static TierPrice ToTierPriceEntity<TierPriceModel, TierPrice>(this TierPriceModel model, TierPrice entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return model.Map(entity);
        }

        public static Manufacturer ToManufacturerEntity<ManufacturerModel, Manufacturer>(this ManufacturerModel model, Manufacturer entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return model.Map(entity);
        }

        public static TModel ToShipmentModel<TModel>(this Shipment entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToOrderNoteModel<TModel>(this OrderNote entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToProductAttributeCombinationModel<TModel>(this ProductAttributeCombination entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        public static TModel ToProductSpecificationAttributeModel<TModel>(this ProductSpecificationAttributeMapping entity) where TModel : BaseInovatiqaEntityModel
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TModel>();
        }

        #endregion

        #region Model-Settings mapping

        public static TModel ToSettingsModel<TModel>(this ISettings settings) where TModel : BaseInovatiqaModel, ISettingsModel
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            return settings.Map<TModel>();
        }

        #endregion

        #region Model-Plugin mapping



        #endregion

        #endregion
    }
}