using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.Shipping.Tracking.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Shipping
{
    public partial class ShipmentService : IShipmentService
    {
        #region Fields

        private readonly IRepository<ShipmentItem> _siRepository;
        private readonly IRepository<Shipment> _shipmentRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IShippingRateComputationMethodService _shippingRateComputationMethodService;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Product> _productRepository;

        #endregion

        #region Ctor

        public ShipmentService(IRepository<ShipmentItem> siRepository,
            IRepository<Shipment> shipmentRepository,
            IShippingRateComputationMethodService shippingRateComputationMethodService,
            IRepository<OrderItem> orderItemRepository,
            IRepository<Address> addressRepository,
            IRepository<Product> productRepository,
            IRepository<Order> orderRepository)
        {
            _siRepository = siRepository;
            _shipmentRepository = shipmentRepository;
            _shippingRateComputationMethodService = shippingRateComputationMethodService;
            _orderItemRepository = orderItemRepository;
            _addressRepository = addressRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
        }

        #endregion

        #region Methods

        public virtual IList<Shipment> GetShipmentsByIds(int[] shipmentIds)
        {
            if (shipmentIds == null || shipmentIds.Length == 0)
                return new List<Shipment>();

            var query = from o in _shipmentRepository.Query()
                        where shipmentIds.Contains(o.Id)
                        select o;
            var shipments = query.ToList();
            var sortedOrders = new List<Shipment>();
            foreach (var id in shipmentIds)
            {
                var shipment = shipments.Find(x => x.Id == id);
                if (shipment != null)
                    sortedOrders.Add(shipment);
            }

            return sortedOrders;
        }

        public virtual void DeleteShipment(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            _shipmentRepository.Delete(shipment);

            //event notification
            //_eventPublisher.EntityDeleted(shipment);
        }

        public virtual void InsertShipmentItem(ShipmentItem shipmentItem)
        {
            if (shipmentItem == null)
                throw new ArgumentNullException(nameof(shipmentItem));

            _siRepository.Insert(shipmentItem);

            //event notification
            //_eventPublisher.EntityInserted(shipmentItem);
        }

        public virtual void InsertShipment(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            _shipmentRepository.Insert(shipment);

            //event notification
            //_eventPublisher.EntityInserted(shipment);
        }

        public virtual IList<ShipmentItem> GetShipmentItemsByShipmentId(int shipmentId)
        {
            if (shipmentId == 0)
                return null;

            return _siRepository.Query().Where(si => si.ShipmentId == shipmentId).ToList();
        }

        public virtual Shipment GetShipmentById(int shipmentId)
        {
            if (shipmentId == 0)
                return null;

            return _shipmentRepository.GetById(shipmentId);
        }

        public virtual IList<Shipment> GetShipmentsByOrderId(int orderId, bool? shipped = null, int vendorId = 0)
        {
            if (orderId == 0)
                return new List<Shipment>();

            var shipments = _shipmentRepository.Query();

            if (shipped.HasValue)
            {
                shipments = shipments.Where(s => s.ShippedDateUtc.HasValue == shipped);
            }

            return shipments.Where(shipment => shipment.OrderId == orderId).ToList();
        }

        public virtual IShipmentTrackerService GetShipmentTracker(Shipment shipment)
        {
            var order = _orderRepository.GetById(shipment.OrderId);

            return _shippingRateComputationMethodService?.ShipmentTracker;
        }

        public virtual int GetQuantityInShipments(Product product, int warehouseId,
            bool ignoreShipped, bool ignoreDelivered)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (product.ManageInventoryMethodId != (int)ManageInventoryMethod.ManageStock)
                return 0;
            if (!product.UseMultipleWarehouses)
                return 0;

            const int cancelledOrderStatusId = (int)OrderStatus.Cancelled;

            var query = _siRepository.Query();

            query = from si in query
                    join s in _shipmentRepository.Query() on si.ShipmentId equals s.Id
                    join o in _orderRepository.Query() on s.OrderId equals o.Id
                    where !o.Deleted && o.OrderStatusId != cancelledOrderStatusId
                    select si;

            query = query.Distinct();

            if (warehouseId > 0)
                query = query.Where(si => si.WarehouseId == warehouseId);
            if (ignoreShipped)
            {
                query = from si in query
                        join s in _shipmentRepository.Query() on si.ShipmentId equals s.Id
                        where !s.ShippedDateUtc.HasValue
                        select si;
            }

            if (ignoreDelivered)
            {
                query = from si in query
                        join s in _shipmentRepository.Query() on si.ShipmentId equals s.Id
                        where !s.DeliveryDateUtc.HasValue
                        select si;
            }

            var queryProductOrderItems = from orderItem in _orderItemRepository.Query()
                                         where orderItem.ProductId == product.Id
                                         select orderItem.Id;
            query = from si in query
                    where queryProductOrderItems.Any(orderItemId => orderItemId == si.OrderItemId)
                    select si;

            var result = Convert.ToInt32(query.Sum(si => (int?)si.Quantity));
            return result;
        }

        public virtual IPagedList<Shipment> GetAllShipments(int vendorId = 0, int warehouseId = 0,
            int shippingCountryId = 0,
            int shippingStateId = 0,
            string shippingCounty = null,
            string shippingCity = null,
            string trackingNumber = null,
            bool loadNotShipped = false,
            bool loadNotDelivered = false,
            int orderId = 0,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _shipmentRepository.Query();

            if (orderId > 0)
                query = query.Where(o => o.OrderId == orderId);

            if (!string.IsNullOrEmpty(trackingNumber))
                query = query.Where(s => s.TrackingNumber.Contains(trackingNumber));

            if (shippingCountryId > 0)
                query = from s in query
                        join o in _orderRepository.Query() on s.OrderId equals o.Id
                        where _addressRepository.Query().Any(a =>
                            a.Id == (o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId) &&
                            a.CountryId == shippingCountryId)
                        select s;

            if (shippingStateId > 0)
                query = from s in query
                        join o in _orderRepository.Query() on s.OrderId equals o.Id
                        where _addressRepository.Query().Any(a =>
                            a.Id == (o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId) &&
                            a.StateProvinceId == shippingStateId)
                        select s;

            if (!string.IsNullOrWhiteSpace(shippingCounty))
                query = from s in query
                        join o in _orderRepository.Query() on s.OrderId equals o.Id
                        where _addressRepository.Query().Any(a =>
                            a.Id == (o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId) &&
                            a.County.Contains(shippingCounty))
                        select s;

            if (!string.IsNullOrWhiteSpace(shippingCity))
                query = from s in query
                        join o in _orderRepository.Query() on s.OrderId equals o.Id
                        where _addressRepository.Query().Any(a =>
                            a.Id == (o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId) &&
                            a.City.Contains(shippingCity))
                        select s;

            if (loadNotShipped)
                query = query.Where(s => !s.ShippedDateUtc.HasValue);

            if (loadNotDelivered)
                query = query.Where(s => !s.DeliveryDateUtc.HasValue);

            if (createdFromUtc.HasValue)
                query = query.Where(s => createdFromUtc.Value <= s.CreatedOnUtc);

            if (createdToUtc.HasValue)
                query = query.Where(s => createdToUtc.Value >= s.CreatedOnUtc);

            query = from s in query
                    join o in _orderRepository.Query() on s.OrderId equals o.Id
                    where !o.Deleted
                    select s;

            query = query.Distinct();

            if (vendorId > 0)
            {
                var queryVendorOrderItems = from orderItem in _orderItemRepository.Query()
                                            join p in _productRepository.Query() on orderItem.ProductId equals p.Id
                                            where p.VendorId == vendorId
                                            select orderItem.Id;

                query = from s in query
                        join si in _siRepository.Query() on s.Id equals si.ShipmentId
                        where queryVendorOrderItems.Contains(si.OrderItemId)
                        select s;

                query = query.Distinct();
            }

            if (warehouseId > 0)
            {
                query = from s in query
                        join si in _siRepository.Query() on s.Id equals si.ShipmentId
                        where si.WarehouseId == warehouseId
                        select s;

                query = query.Distinct();
            }

            query = query.OrderByDescending(s => s.CreatedOnUtc);

            var shipments = new PagedList<Shipment>(query, pageIndex, pageSize);
            return shipments;
        }

        public virtual IPagedList<Shipment> GetAllInvoicedShipments(int vendorId = 0, int warehouseId = 0,
            int shippingCountryId = 0,
            int shippingStateId = 0,
            string shippingCounty = null,
            string shippingCity = null,
            string trackingNumber = null,
            bool loadNotShipped = false,
            bool loadNotDelivered = false,
            int orderId = 0,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _shipmentRepository.Query();

            if (orderId > 0)
                query = query.Where(o => o.OrderId == orderId);

            if (!string.IsNullOrEmpty(trackingNumber))
                query = query.Where(s => s.TrackingNumber.Contains(trackingNumber));

            if (shippingCountryId > 0)
                query = from s in query
                        join o in _orderRepository.Query() on s.OrderId equals o.Id
                        where _addressRepository.Query().Any(a =>
                            a.Id == (o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId) &&
                            a.CountryId == shippingCountryId)
                        select s;

            if (shippingStateId > 0)
                query = from s in query
                        join o in _orderRepository.Query() on s.OrderId equals o.Id
                        where _addressRepository.Query().Any(a =>
                            a.Id == (o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId) &&
                            a.StateProvinceId == shippingStateId)
                        select s;

            if (!string.IsNullOrWhiteSpace(shippingCounty))
                query = from s in query
                        join o in _orderRepository.Query() on s.OrderId equals o.Id
                        where _addressRepository.Query().Any(a =>
                            a.Id == (o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId) &&
                            a.County.Contains(shippingCounty))
                        select s;

            if (!string.IsNullOrWhiteSpace(shippingCity))
                query = from s in query
                        join o in _orderRepository.Query() on s.OrderId equals o.Id
                        where _addressRepository.Query().Any(a =>
                            a.Id == (o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId) &&
                            a.City.Contains(shippingCity))
                        select s;

            //if (loadNotShipped)
            //    query = query.Where(s => !s.ShippedDateUtc.HasValue);

            //if (loadNotDelivered)
            //    query = query.Where(s => !s.DeliveryDateUtc.HasValue);

            if (createdFromUtc.HasValue)
                query = query.Where(s => createdFromUtc.Value <= s.CreatedOnUtc);

            if (createdToUtc.HasValue)
                query = query.Where(s => createdToUtc.Value >= s.CreatedOnUtc);

            query = query.Where(s => s.ShippedDateUtc.HasValue);

            query = from s in query
                    join o in _orderRepository.Query() on s.OrderId equals o.Id
                    where !o.Deleted
                    select s;

            query = query.Distinct();

            if (vendorId > 0)
            {
                var queryVendorOrderItems = from orderItem in _orderItemRepository.Query()
                                            join p in _productRepository.Query() on orderItem.ProductId equals p.Id
                                            where p.VendorId == vendorId
                                            select orderItem.Id;

                query = from s in query
                        join si in _siRepository.Query() on s.Id equals si.ShipmentId
                        where queryVendorOrderItems.Contains(si.OrderItemId)
                        select s;

                query = query.Distinct();
            }

            if (warehouseId > 0)
            {
                query = from s in query
                        join si in _siRepository.Query() on s.Id equals si.ShipmentId
                        where si.WarehouseId == warehouseId
                        select s;

                query = query.Distinct();
            }

            query = query.OrderByDescending(s => s.CreatedOnUtc);

            var shipments = new PagedList<Shipment>(query, pageIndex, pageSize);
            return shipments;
        }

        public virtual void UpdateShipment(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            _shipmentRepository.Update(shipment);

            //event notification
            //_eventPublisher.EntityUpdated(shipment);
        }

        public virtual IList<Shipment> GetAllShipmentsForCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(Customer));

            var query = _shipmentRepository.Query();

            query = from s in query
                    join o in _orderRepository.Query() on s.OrderId equals o.Id
                    where !o.Deleted && o.CustomerId == customer.Id
                    select s;

            query = query.Distinct();

            query = query.OrderByDescending(s => s.CreatedOnUtc);

            var shipments = new List<Shipment>(query);
            return shipments;
        }

        #endregion
    }
}