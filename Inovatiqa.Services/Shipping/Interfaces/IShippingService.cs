using Inovatiqa.Database.Models;
using Inovatiqa.Services.Shipping.Pickup;
using System.Collections.Generic;

namespace Inovatiqa.Services.Shipping.Interfaces
{
    public partial interface IShippingService
    {
        #region Shipping methods

        IList<ShippingMethod> GetAllShippingMethods(int? filterByCountryId = null);

        #endregion

        #region Warehouses

        Warehouse GetWarehouseById(int warehouseId);

        Warehouse GetNearestWarehouse(Address address, IList<Warehouse> warehouses = null);

        IList<Warehouse> GetAllWarehouses(string name = null);

        #endregion

        #region Workflow

        bool IsShipEnabled(ShoppingCartItem shoppingCartItem);

        bool IsFreeShipping(ShoppingCartItem shoppingCartItem);

        decimal GetAdditionalShippingCharge(ShoppingCartItem shoppingCartItem);

        GetShippingOptionResponse GetShippingOptions(IList<ShoppingCartItem> cart, Address shippingAddress,
            Customer customer = null, string allowedShippingRateComputationMethodSystemName = "", int storeId = 0);

        IList<GetShippingOptionRequest> CreateShippingOptionRequests(IList<ShoppingCartItem> cart,
            Address shippingAddress, int storeId, out bool shippingFromMultipleLocations);

        void GetDimensions(IList<GetShippingOptionRequest.PackageItem> packageItems,
            out decimal width, out decimal length, out decimal height, bool ignoreFreeShippedItems = false);

        void GetAssociatedProductDimensions(ShoppingCartItem shoppingCartItem,
            out decimal width, out decimal length, out decimal height, bool ignoreFreeShippedItems = false);

        decimal GetTotalWeight(GetShippingOptionRequest request, bool includeCheckoutAttributes = true, bool ignoreFreeShippedItems = false);

        decimal GetShoppingCartItemWeight(ShoppingCartItem shoppingCartItem, bool ignoreFreeShippedItems = false);

        decimal GetShoppingCartItemWeight(Product product, string attributesXml, bool ignoreFreeShippedItems = false);

        GetPickupPointsResponse GetPickupPoints(int addressId, Customer customer = null, string providerSystemName = null, int storeId = 0);

        #endregion
    }
}