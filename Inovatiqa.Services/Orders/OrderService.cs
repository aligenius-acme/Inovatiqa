using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Orders
{
    public partial class OrderService : IOrderService
    {
        #region Fields
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<OrderNote> _orderNoteRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        private readonly IShipmentService _shipmentService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerService _customerService;
        private readonly ICustomNumberFormatterService _customNumberFormatterService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IShippingService _shippingService;
        private readonly IProductAttributeFormatterService _productAttributeFormatterService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEmailAccountService _emailAccountService;

        #endregion

        #region Ctor

        public OrderService(IRepository<Address> addressRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<OrderNote> orderNoteRepository,
            IRepository<Product> productRepository,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
            IShipmentService shipmentService,
            IShoppingCartService shoppingCartService,
            ICustomerService customerService,
            ICustomNumberFormatterService customNumberFormatterService,
            IPriceCalculationService priceCalculationService,
            IShippingService shippingService,
            IProductAttributeFormatterService productAttributeFormatterService,
            IQueuedEmailService queuedEmailService,
            IEmailAccountService emailAccountService)
        {
            _addressRepository = addressRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _orderNoteRepository = orderNoteRepository;
            _productRepository = productRepository;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _shipmentService = shipmentService;
            _shoppingCartService = shoppingCartService;
            _customerService = customerService;
            _customNumberFormatterService = customNumberFormatterService;
            _priceCalculationService = priceCalculationService;
            _shippingService = shippingService;
            _productAttributeFormatterService = productAttributeFormatterService;
            _queuedEmailService = queuedEmailService;
            _emailAccountService = emailAccountService;
        }

        #endregion

        #region Methods

        #region Orders

        public virtual Order GetOrderByCustomOrderNumber(string customOrderNumber)
        {
            if (string.IsNullOrEmpty(customOrderNumber))
                return null;

            return _orderRepository.Query().FirstOrDefault(o => o.CustomOrderNumber == customOrderNumber);
        }

        public virtual bool HasItemsToShip(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            foreach (var orderItem in GetOrderItems(order.Id, isShipEnabled: true)) //we can ship only shippable products
            {
                var totalNumberOfNotYetShippedItems = GetTotalNumberOfNotYetShippedItems(orderItem);
                if (totalNumberOfNotYetShippedItems <= 0)
                    continue;

                return true;
            }

            return false;
        }

        public virtual void DeleteOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            order.Deleted = true;
            UpdateOrder(order);

            //event notification
            //_eventPublisher.EntityDeleted(order);
        }

        public virtual bool HasItemsToAddToShipment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            foreach (var orderItem in GetOrderItems(order.Id, isShipEnabled: true)) //we can ship only shippable products
            {
                var totalNumberOfItemsCanBeAddedToShipment = GetTotalNumberOfItemsCanBeAddedToShipment(orderItem);
                if (totalNumberOfItemsCanBeAddedToShipment <= 0)
                    continue;

                //yes, we have at least one item to create a new shipment
                return true;
            }

            return false;
        }

        public virtual Order GetOrderById(int orderId)
        {
            if (orderId == 0)
                return null;

            return _orderRepository.GetById(orderId);
        }

        public virtual IPagedList<Order> SearchOrders(int storeId = 0,
            int vendorId = 0, int customerId = 0,
            int productId = 0, int affiliateId = 0, int warehouseId = 0,
            int billingCountryId = 0, string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "",
            string orderNotes = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var query = _orderRepository.Query();

            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);

            if (vendorId > 0)
                query = from o in query
                        join oi in _orderItemRepository.Query() on o.Id equals oi.OrderId
                        join p in _productRepository.Query() on oi.ProductId equals p.Id
                        where p.VendorId == vendorId
                        select o;

            if (customerId > 0)
                query = query.Where(o => o.CustomerId == customerId);

            if (productId > 0)
                query = from o in query
                        join oi in _orderItemRepository.Query() on o.Id equals oi.OrderId
                        where oi.ProductId == productId
                        select o;

            if (warehouseId > 0)
            {
                var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;

                query = from o in query
                        join oi in _orderItemRepository.Query() on o.Id equals oi.OrderId
                        join p in _productRepository.Query() on oi.ProductId equals p.Id
                        join pwi in _productWarehouseInventoryRepository.Query() on p.Id equals pwi.ProductId
                        where
                            (p.ManageInventoryMethodId == manageStockInventoryMethodId && p.UseMultipleWarehouses && pwi.WarehouseId == warehouseId) ||
                            ((p.ManageInventoryMethodId != manageStockInventoryMethodId || !p.UseMultipleWarehouses) && p.WarehouseId == warehouseId)
                        select o;
            }

            if (!string.IsNullOrEmpty(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);

            if (affiliateId > 0)
                query = query.Where(o => o.AffiliateId == affiliateId);

            if (createdFromUtc.HasValue)
                query = query.Where(o => o.CreatedOnUtc >= createdFromUtc.Value.Date);

            if (createdToUtc.HasValue)
                query = query.Where(o => o.CreatedOnUtc <= createdToUtc.Value.Date);

            if (osIds != null && osIds.Any())
                query = query.Where(o => osIds.Contains(o.OrderStatusId));

            if (psIds != null && psIds.Any())
                query = query.Where(o => psIds.Contains(o.PaymentStatusId));

            if (ssIds != null && ssIds.Any())
                query = query.Where(o => ssIds.Contains(o.ShippingStatusId));

            if (!string.IsNullOrEmpty(orderNotes))
                query = query.Where(o => _orderNoteRepository.Query().Any(oNote => oNote.OrderId == o.Id && oNote.Note.Contains(orderNotes)));

            query = from o in query
                    join oba in _addressRepository.Query() on o.BillingAddressId equals oba.Id
                    where
                        (billingCountryId <= 0 || (oba.CountryId == billingCountryId)) &&
                        (string.IsNullOrEmpty(billingPhone) || (!string.IsNullOrEmpty(oba.PhoneNumber) && oba.PhoneNumber.Contains(billingPhone))) &&
                        (string.IsNullOrEmpty(billingEmail) || (!string.IsNullOrEmpty(oba.Email) && oba.Email.Contains(billingEmail))) &&
                        (string.IsNullOrEmpty(billingLastName) || (!string.IsNullOrEmpty(oba.LastName) && oba.LastName.Contains(billingLastName)))
                    select o;

            query = query.Where(o => !o.Deleted);
            query = query.OrderByDescending(o => o.CreatedOnUtc);

            return new PagedList<Order>(query, pageIndex, pageSize, getOnlyTotalCount);
        }

        public virtual void InsertOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _orderRepository.Insert(order);

            ////event notification
            //_eventPublisher.EntityInserted(order);
        }

        public virtual void UpdateOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _orderRepository.Update(order);

            ////event notification
            //_eventPublisher.EntityUpdated(order);
        }

        public virtual void InsertOrderItem(OrderItem orderItem)
        {
            if (orderItem is null)
                throw new ArgumentNullException(nameof(orderItem));

            _orderItemRepository.Insert(orderItem);

            ////event notification
            //_eventPublisher.EntityInserted(orderItem);
        }

        public virtual bool HasItemsToDeliver(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            foreach (var orderItem in GetOrderItems(order.Id, isShipEnabled: true)) //we can ship only shippable products
            {
                var totalNumberOfShippedItems = GetTotalNumberOfShippedItems(orderItem);
                var totalNumberOfDeliveredItems = GetTotalNumberOfDeliveredItems(orderItem);
                if (totalNumberOfShippedItems <= totalNumberOfDeliveredItems)
                    continue;

                return true;
            }

            return false;
        }

        public virtual bool RejectOrder(int ID)
        {

            // Reject Process
            var cart = _shoppingCartService.GetOrderWaitingForApproval(ID);
            var items = _shoppingCartService.GetAllOrderApprovalItems(ID);
            decimal OrderTotal = 0;
            var customer = _customerService.GetCustomerById(cart.CustomerId);
            foreach(var item in items)
            {
                OrderTotal += (item.Quantity * (_productRepository.GetById(item.ProductId).Price));
            }
            var order = new Order
            {
                CustomerId = cart.CustomerId,
                BillingAddressId = Convert.ToInt32(customer.BillingAddressId),
                ShippingAddressId = customer.ShippingAddressId,
                OrderGuid = Guid.NewGuid(),
                StoreId = InovatiqaDefaults.StoreId,
                PickupInStore = false,
                OrderStatusId = 40,
                ShippingStatusId = 0,
                PaymentStatusId = 0,
                CustomerCurrencyCode = InovatiqaDefaults.CurrencyCode,
                CurrencyRate = InovatiqaDefaults.CurrencyRate,
                CustomerTaxDisplayTypeId = 0,
                OrderSubtotalExclTax = OrderTotal,
                OrderSubtotalInclTax = OrderTotal,
                OrderSubTotalDiscountInclTax = 0,
                OrderSubTotalDiscountExclTax = 0,
                OrderShippingInclTax = 0,
                OrderShippingExclTax = 0,
                PaymentMethodAdditionalFeeExclTax = 0,
                PaymentMethodAdditionalFeeInclTax = 0,
                OrderTax = 0,
                OrderDiscount = 0,
                OrderTotal = OrderTotal,
                RefundedAmount = 0,
                CustomerLanguageId = 0,
                AffiliateId = 0,
                AllowStoringCreditCardNumber = false,
                Deleted = false,
                CreatedOnUtc = DateTime.UtcNow,
                CustomOrderNumber = String.Empty
            };
            InsertOrder(order);
            order.CustomOrderNumber = _customNumberFormatterService.GenerateOrderCustomNumber(order);
            UpdateOrder(order);
            // Insert Order Items
            foreach(var item in items)
            {
                var product = _productRepository.GetById(item.ProductId);
                var scSubTotal = _shoppingCartService.GetSubTotal(item, true, out var discountAmount,
                    out var scDiscounts, out _);
                var itemWeight = _shippingService.GetShoppingCartItemWeight(item);
                var OrderItemModel = new OrderItem()
                {
                    OrderItemGuid = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = product.Id,
                    UnitPriceInclTax = _shoppingCartService.GetUnitPrice(item),
                    UnitPriceExclTax = _shoppingCartService.GetUnitPrice(item),
                    PriceInclTax = scSubTotal,
                    PriceExclTax = scSubTotal,
                    OriginalProductCost = _priceCalculationService.GetProductCost(product, item.AttributesXml),
                    AttributeDescription = _productAttributeFormatterService.FormatAttributes(product, item.AttributesXml, customer),
                    AttributesXml = item.AttributesXml,
                    Quantity = item.Quantity,
                    DownloadCount = 0,
                    IsDownloadActivated = false,
                    LicenseDownloadId = 0,
                    ItemWeight = itemWeight,
                    RentalStartDateUtc = item.RentalStartDateUtc,
                    RentalEndDateUtc = item.RentalEndDateUtc,
                    ReorderDateUtc = item.Reordered ? DateTime.Now : (DateTime?)null
                };
                InsertOrderItem(OrderItemModel);
                _shoppingCartService.DeleteShoppingCartItem(item);
            }

            _shoppingCartService.DeleteOrderFromWaitingList(ID);
            var address = _customerService.GetAddressesByCustomerId(customer.Id).FirstOrDefault();
            var defaultMail = _emailAccountService.GetEmailAccountById(InovatiqaDefaults.DefaultEmailAccountId);
            var email = new QueuedEmail
            {
                From = defaultMail.Email,
                FromName = defaultMail.DisplayName,
                To = customer.Email,
                ToName = address.FirstName + address.LastName,
                Subject = "Your Order Number" + ID + " Has Been Rejected",
                EmailAccountId = InovatiqaDefaults.DefaultEmailAccountId,
                PriorityId = InovatiqaDefaults.QueuedEmailPrioritHigh,
                Body = "We are sorry to inform you that Your Order Number " + ID + " has been rejected. Please Contact your Administrator for further details",
                CreatedOnUtc = DateTime.Now,
                SentTries = 0,
                AttachedDownloadId = 0
            };
            _queuedEmailService.InsertQueuedEmail(email);

            return true;
        }

        #endregion

        #region Orders items

        public virtual void DeleteOrderItem(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            _orderItemRepository.Delete(orderItem);

            //event notification
            //_eventPublisher.EntityDeleted(orderItem);
        }

        public virtual void UpdateOrderItem(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            _orderItemRepository.Update(orderItem);

            //event notification
            //_eventPublisher.EntityUpdated(orderItem);
        }

        public virtual int GetTotalNumberOfShippedItems(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            var result = 0;
            var shipments = _shipmentService.GetShipmentsByOrderId(orderItem.OrderId);
            for (var i = 0; i < shipments.Count; i++)
            {
                var shipment = shipments[i];
                if (!shipment.ShippedDateUtc.HasValue)
                    continue;

                var si = _shipmentService.GetShipmentItemsByShipmentId(shipment.Id)
                    .FirstOrDefault(x => x.OrderItemId == orderItem.Id);
                if (si != null)
                {
                    result += si.Quantity;
                }
            }

            return result;
        }

        public virtual int GetTotalNumberOfDeliveredItems(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            var result = 0;
            var shipments = _shipmentService.GetShipmentsByOrderId(orderItem.OrderId);

            for (var i = 0; i < shipments.Count; i++)
            {
                var shipment = shipments[i];
                if (!shipment.DeliveryDateUtc.HasValue)
                    //not delivered yet
                    continue;

                var si = _shipmentService.GetShipmentItemsByShipmentId(shipment.Id)
                    .FirstOrDefault(x => x.OrderItemId == orderItem.Id);
                if (si != null)
                {
                    result += si.Quantity;
                }
            }

            return result;
        }

        public virtual int GetTotalNumberOfNotYetShippedItems(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            var result = 0;
            var shipments = _shipmentService.GetShipmentsByOrderId(orderItem.OrderId);
            for (var i = 0; i < shipments.Count; i++)
            {
                var shipment = shipments[i];
                if (shipment.ShippedDateUtc.HasValue)
                    continue;

                var si = _shipmentService.GetShipmentItemsByShipmentId(shipment.Id)
                    .FirstOrDefault(x => x.OrderItemId == orderItem.Id);
                if (si != null)
                {
                    result += si.Quantity;
                }
            }

            return result;
        }

        public virtual Product GetProductByOrderItemId(int orderItemId)
        {
            if (orderItemId == 0)
                return null;

            return (from p in _productRepository.Query()
                    join oi in _orderItemRepository.Query() on p.Id equals oi.ProductId
                    where oi.Id == orderItemId
                    select p).SingleOrDefault();
        }

        public virtual int GetTotalNumberOfItemsInAllShipment(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            var totalInShipments = 0;
            var shipments = _shipmentService.GetShipmentsByOrderId(orderItem.OrderId);

            for (var i = 0; i < shipments.Count; i++)
            {
                var shipment = shipments[i];
                var si = _shipmentService.GetShipmentItemsByShipmentId(shipment.Id)
                    .FirstOrDefault(x => x.OrderItemId == orderItem.Id);
                if (si != null)
                {
                    totalInShipments += si.Quantity;
                }
            }

            return totalInShipments;
        }

        public virtual int GetTotalNumberOfItemsCanBeAddedToShipment(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            var totalInShipments = GetTotalNumberOfItemsInAllShipment(orderItem);

            var qtyOrdered = orderItem.Quantity;
            var qtyCanBeAddedToShipmentTotal = qtyOrdered - totalInShipments;
            if (qtyCanBeAddedToShipmentTotal < 0)
                qtyCanBeAddedToShipmentTotal = 0;

            return qtyCanBeAddedToShipmentTotal;
        }

        public virtual OrderItem GetOrderItemById(int orderItemId)
        {
            if (orderItemId == 0)
                return null;

            return _orderItemRepository.GetById(orderItemId);
        }

        public virtual IList<OrderItem> GetOrderItems(int orderId, bool? isNotReturnable = null, bool? isShipEnabled = null, int vendorId = 0)
        {
            if (orderId == 0)
                return new List<OrderItem>();

            return (from oi in _orderItemRepository.Query()
                    join p in _productRepository.Query() on oi.ProductId equals p.Id
                    where
                    oi.OrderId == orderId &&
                    (!isShipEnabled.HasValue || (p.IsShipEnabled == isShipEnabled.Value)) &&
                    (!isNotReturnable.HasValue || (p.NotReturnable == isNotReturnable)) &&
                    (vendorId <= 0 || (p.VendorId == vendorId))
                    select oi).ToList();
        }

        public virtual IList<Order> GetAllShippedOrdersByCustomer(Customer customer)
        {
            var Orders = _orderRepository.Query();
            return (from oi in _orderRepository.Query() 
                    where (oi.ShippingStatusId == (int)ShippingStatus.Shipped) && (oi.CustomerId == customer.Id) 
                    select oi).ToList();    
        }

        public virtual IList<Order> GetAllOrdersByCustomer(Customer customer)
        {
            var Orders = _orderRepository.Query();
            return (from oi in _orderRepository.Query()
                    where oi.CustomerId == customer.Id
                    select oi).ToList();
        }

        #endregion

        #region Orders notes

        public virtual void DeleteOrderNote(OrderNote orderNote)
        {
            if (orderNote == null)
                throw new ArgumentNullException(nameof(orderNote));

            _orderNoteRepository.Delete(orderNote);

            //event notification
            //_eventPublisher.EntityDeleted(orderNote);
        }

        public virtual OrderNote GetOrderNoteById(int orderNoteId)
        {
            if (orderNoteId == 0)
                return null;

            return _orderNoteRepository.GetById(orderNoteId);
        }

        public virtual void InsertOrderNote(OrderNote orderNote)
        {
            if (orderNote is null)
                throw new ArgumentNullException(nameof(orderNote));

            _orderNoteRepository.Insert(orderNote);

            ////event notification
            //_eventPublisher.EntityInserted(orderNote);
        }

        public virtual IList<OrderNote> GetOrderNotesByOrderId(int orderId, bool? displayToCustomer = null)
        {
            if (orderId == 0)
                return new List<OrderNote>();

            var query = _orderNoteRepository.Query().Where(on => on.OrderId == orderId);

            if (displayToCustomer.HasValue)
            {
                query = query.Where(on => on.DisplayToCustomer == displayToCustomer);
            }

            return query.ToList();
        }

        public virtual string FormatOrderNoteText(OrderNote orderNote)
        {
            if (orderNote == null)
                throw new ArgumentNullException(nameof(orderNote));

            var text = orderNote.Note;

            if (string.IsNullOrEmpty(text))
                return string.Empty;

            text = HtmlHelper.FormatText(text, false, true, false, false, false, false);

            return text;
        }

        #endregion

        #region Recurring payments



        #endregion

        #region Recurring payments history



        #endregion

        #endregion
    }
}