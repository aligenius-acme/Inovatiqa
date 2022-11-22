using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Shipping.Tracking.Interfaces;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Services.Shipping.Interfaces
{
    public partial interface IShipmentService
    {
        IList<Shipment> GetShipmentsByIds(int[] shipmentIds);

        void DeleteShipment(Shipment shipment);

        IList<ShipmentItem> GetShipmentItemsByShipmentId(int shipmentId);

        Shipment GetShipmentById(int shipmentId);

        IList<Shipment> GetShipmentsByOrderId(int orderId, bool? shipped = null, int vendorId = 0);

        IShipmentTrackerService GetShipmentTracker(Shipment shipment);

        int GetQuantityInShipments(Product product, int warehouseId,
            bool ignoreShipped, bool ignoreDelivered);

        IPagedList<Shipment> GetAllShipments(int vendorId = 0, int warehouseId = 0,
            int shippingCountryId = 0,
            int shippingStateId = 0,
            string shippingCounty = null,
            string shippingCity = null,
            string trackingNumber = null,
            bool loadNotShipped = false,
            bool loadNotDelivered = false,
            int orderId = 0,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        IPagedList<Shipment> GetAllInvoicedShipments(int vendorId = 0, int warehouseId = 0,
            int shippingCountryId = 0,
            int shippingStateId = 0,
            string shippingCounty = null,
            string shippingCity = null,
            string trackingNumber = null,
            bool loadNotShipped = false,
            bool loadNotDelivered = false,
            int orderId = 0,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        void InsertShipment(Shipment shipment);

        void InsertShipmentItem(ShipmentItem shipmentItem);

        void UpdateShipment(Shipment shipment);

        IList<Shipment> GetAllShipmentsForCustomer(Customer customer);
    }
}