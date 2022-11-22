using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Extensions;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Payments.Interfaces;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.Shipping.Tracking.Interfaces;
using Inovatiqa.Services.Vendors.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Catalog;
using Inovatiqa.Web.Models.Common;
using Inovatiqa.Web.Models.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Inovatiqa.Web.Factories
{
    public partial class OrderModelFactory : IOrderModelFactory
    {
        #region Fields

        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelperService _dateTimeHelperService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPaymentService _paymentService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IShipmentService _shipmentService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContextService _workContextService;
        private readonly IShipmentTrackerService _shipmentTrackerService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IRepository<SuspendedCart> _suspendedCartRepository;
        private readonly IRepository<ShoppingCartItem> _shoppingCartItemRepository;
        private readonly IRepository<ProductCategoryMapping> _productCategoryRepository;

        #endregion

        #region Ctor

        public OrderModelFactory(IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateTimeHelperService dateTimeHelperService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentService paymentService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IShipmentService shipmentService,
            IUrlRecordService urlRecordService,
            IVendorService vendorService,
            IWorkContextService workContextService,
            IShipmentTrackerService shipmentTrackerService,
            IStateProvinceService stateProvinceService,
            IProductAttributeService productAttributeService,
            IProductModelFactory productModelFactory,
            IManufacturerService manufacturerService,
            IRepository<SuspendedCart> suspendedCartRepository,
            IRepository<ShoppingCartItem> shoppingCartItemRepository,
            IRepository<ProductCategoryMapping> productCategoryRepository)
        {
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _dateTimeHelperService = dateTimeHelperService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _paymentService = paymentService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _shipmentService = shipmentService;
            _urlRecordService = urlRecordService;
            _productModelFactory = productModelFactory;
            _vendorService = vendorService;
            _workContextService = workContextService;
            _shipmentTrackerService = shipmentTrackerService;
            _stateProvinceService = stateProvinceService;
            _productAttributeService = productAttributeService;
            _manufacturerService = manufacturerService;
            _suspendedCartRepository = suspendedCartRepository;
            _shoppingCartItemRepository = shoppingCartItemRepository;
            _productCategoryRepository = productCategoryRepository;
        }

        #endregion

        #region Utilities

        protected virtual IList<ManufacturerBriefInfoModel> PrepareProductManufacturerModels(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = _manufacturerService.GetProductManufacturersByProductId(product.Id)
                .Select(pm =>
                {
                    var manufacturer = _manufacturerService.GetManufacturerById(pm.ManufacturerId);
                    var modelMan = new ManufacturerBriefInfoModel
                    {
                        Id = manufacturer.Id,
                        Name = manufacturer.Name,
                        SeName = _urlRecordService.GetActiveSlug(manufacturer.Id, InovatiqaDefaults.ManufacturerSlugName, InovatiqaDefaults.LanguageId)
                    };

                    return modelMan;
                }).ToList();

            return model;
        }

        protected virtual IList<CustomerReorderGuideModel.ProductAttributeModel> PrepareProductAttributeModels(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new List<CustomerReorderGuideModel.ProductAttributeModel>();

            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            foreach (var attribute in productAttributeMapping)
            {
                var productAttrubute = _productAttributeService.GetProductAttributeById(attribute.ProductAttributeId);

                var attributeModel = new CustomerReorderGuideModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductId = product.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = productAttrubute.Name,
                    Description = productAttrubute.Description,
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlTypeId = attribute.AttributeControlTypeId,
                    HasCondition = !string.IsNullOrEmpty(attribute.ConditionAttributeXml)
                };
                if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new CustomerReorderGuideModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected,
                            Quantity = attributeValue.Quantity
                        };
                        attributeModel.Values.Add(valueModel);
                    }
                }
                model.Add(attributeModel);
            }

            return model;
        }

        #endregion

        #region Methods

        public virtual CustomerOrderItemsListModel PrepareCustomerOrderItemsListModel(DateTime? startDateValue = null, DateTime? endDateValue = null, int shippedItems = 0 , int filterCategoryId = -1, int customerId = 0)
        {
            var currentCustomer = _workContextService.CurrentCustomer;
            var customer = customerId == 0 ? currentCustomer : _customerService.GetCustomerById(customerId);
            if (startDateValue == null || endDateValue == null)
            {
                startDateValue = DateTime.Now.AddDays(-35);
                endDateValue = DateTime.Now.AddDays(1);
            }
            else
            {
                if (((DateTime)endDateValue - (DateTime)startDateValue).TotalDays > 35)
                {
                    endDateValue = DateTime.Parse(startDateValue.ToString()).AddDays(35);
                }
                else
                    endDateValue = DateTime.Parse(endDateValue.ToString()).AddDays(1);
            }

            var model = new CustomerOrderItemsListModel();
            List<int> ssIds = new List<int>();
            //if (shippedItems != 0)
            //    ssIds.Add(shippedItems);

            /*var orders = _orderService.SearchOrders(storeId: InovatiqaDefaults.StoreId,
                customerId: customer.Id,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                ssIds: ssIds
                );*/
            var orders = _orderService.SearchOrders(
                storeId: InovatiqaDefaults.StoreId,
                customerId: customer.Id,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                ssIds: ssIds
                );
            foreach (var order in orders)
            {
                var orderShipments = _shipmentService.GetShipmentsByOrderId(order.Id, true);
                var orderItems = _orderService.GetOrderItems(order.Id);
                
                List<ShipmentItem> consolidatedShipmentItems = new List<ShipmentItem>();

                foreach (var shipment in orderShipments)
                    consolidatedShipmentItems.AddRange(_shipmentService.GetShipmentItemsByShipmentId(shipment.Id));

                var orderModel = new CustomerOrderItemsListModel.OrderDetailsModel
                {
                    Id = order.Id,
                    CreatedOn = _dateTimeHelperService.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc),
                    OrderStatusId = order.OrderStatusId,
                    OrderStatus = ((OrderStatus)order.OrderStatusId).ToString(),
                    PaymentStatusId = order.PaymentStatusId,
                    ShippingStatusId = order.ShippingStatusId,
                    CustomOrderNumber = order.CustomOrderNumber,
                    TotalItems = orderItems.Count
                };

                orderModel.Items = new List<CustomerOrderItemsListModel.OrderItemModel>();
                foreach (var orderItem in orderItems)
                {
                   

                    var product = _productService.GetProductById(orderItem.ProductId);

                    var currentOrderShippedItems = 0;
                    foreach (var shipmentItem in consolidatedShipmentItems)
                    {
                        if (orderItem.Id == shipmentItem.OrderItemId)
                            currentOrderShippedItems += shipmentItem.Quantity;
                    }

                    var orderedItems = orderItem.Quantity;

                    if (orderedItems - currentOrderShippedItems == 0)
                    {
                        orderModel.OrderStatusId = (int)OrderStatus.Complete;
                        orderModel.OrderStatus = ((OrderStatus)orderModel.OrderStatusId).ToString();
                    }

                    else if (orderedItems - currentOrderShippedItems != 0)
                    {
                        orderModel.OrderStatusId = (int)OrderStatus.Backorder;
                        orderModel.OrderStatus = ((OrderStatus)orderModel.OrderStatusId).ToString();
                    }

                    if (shippedItems != 0)
                    {
                        if(shippedItems == 25) // return shipped items
                        {
                            if (orderedItems - currentOrderShippedItems == 0)
                            {
                                var orderItemModel = new CustomerOrderItemsListModel.OrderItemModel
                                {
                                    Id = orderItem.Id,
                                    OrderItemGuid = orderItem.OrderItemGuid,
                                    Sku = _productService.FormatSku(product, orderItem.AttributesXml),
                                    VendorName = _vendorService.GetVendorById(product.VendorId)?.Name ?? string.Empty,
                                    ProductId = product.Id,
                                    ProductName = product.Name,
                                    ProductSeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId),
                                    Quantity = orderItem.Quantity,
                                    ManufacturerPartNumber = product.ManufacturerPartNumber,
                                    ProductManufacturers = PrepareProductManufacturerModels(product),
                                    Bo = orderedItems - currentOrderShippedItems,
                                    Shipped = currentOrderShippedItems
                                };
                                if (orderItem.AttributeDescription != "")
                                {
                                    orderItemModel.AttributeInfo = orderItem.AttributeDescription.Replace("UOM: ", "").Split(new string[] { "<br />" }, StringSplitOptions.None)[0];
                                    orderItemModel.AttributeInfo = Regex.Replace(orderItemModel.AttributeInfo, @"\[(-|\+)\$[0-9]+(.{0,1})[0-9]*\]", "");
                                }
                                else
                                    orderItemModel.AttributeInfo = "";
                                //add by hamza for filter by category
                                if (filterCategoryId > 0)
                                {
                                    var allMappings = _productCategoryRepository.Query();
                                    var allProducts = allMappings.Where(x => x.CategoryId == filterCategoryId);
                                    foreach (var products in allProducts)
                                    {
                                        if (products.ProductId == orderItem.ProductId)
                                        {
                                            orderModel.Items.Add(orderItemModel);
                                        }
                                    }
                                }
                                else
                                    orderModel.Items.Add(orderItemModel);
                            }
                        }
                        else if(shippedItems == 30) // return back ordered items
                        {
                            if (orderedItems - currentOrderShippedItems > 0)
                            {
                                var orderItemModel = new CustomerOrderItemsListModel.OrderItemModel
                                {
                                    Id = orderItem.Id,
                                    OrderItemGuid = orderItem.OrderItemGuid,
                                    Sku = _productService.FormatSku(product, orderItem.AttributesXml),
                                    VendorName = _vendorService.GetVendorById(product.VendorId)?.Name ?? string.Empty,
                                    ProductId = product.Id,
                                    ProductName = product.Name,
                                    ProductSeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId),
                                    Quantity = orderItem.Quantity,
                                    ManufacturerPartNumber = product.ManufacturerPartNumber,
                                    ProductManufacturers = PrepareProductManufacturerModels(product),
                                    Bo = orderedItems - currentOrderShippedItems,
                                    Shipped = currentOrderShippedItems
                                };
                                if (orderItem.AttributeDescription != "")
                                {
                                    orderItemModel.AttributeInfo = orderItem.AttributeDescription.Replace("UOM: ", "").Split(new string[] { "<br />" }, StringSplitOptions.None)[0];
                                    orderItemModel.AttributeInfo = Regex.Replace(orderItemModel.AttributeInfo, @"\[(-|\+)\$[0-9]+(.{0,1})[0-9]*\]", "");
                                }
                                else
                                    orderItemModel.AttributeInfo = "";

                                //add by hamza for filter by category
                                if (filterCategoryId > 0)
                                {
                                    var allMappings = _productCategoryRepository.Query();
                                    var allProducts = allMappings.Where(x => x.CategoryId == filterCategoryId);
                                    foreach (var products in allProducts)
                                    {
                                        if (products.ProductId == orderItem.ProductId)
                                        {
                                            orderModel.Items.Add(orderItemModel);
                                        }
                                    }
                                }
                                else
                                    orderModel.Items.Add(orderItemModel);
                            }
                        }
                        
                    }
                    else
                    {
                        var orderItemModel = new CustomerOrderItemsListModel.OrderItemModel
                        {
                            Id = orderItem.Id,
                            OrderItemGuid = orderItem.OrderItemGuid,
                            Sku = _productService.FormatSku(product, orderItem.AttributesXml),
                            VendorName = _vendorService.GetVendorById(product.VendorId)?.Name ?? string.Empty,
                            ProductId = product.Id,
                            ProductName = product.Name,
                            ProductSeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId),
                            Quantity = orderItem.Quantity,
                            ManufacturerPartNumber = product.ManufacturerPartNumber,
                            ProductManufacturers = PrepareProductManufacturerModels(product),
                            Bo = orderedItems - currentOrderShippedItems,
                            Shipped = currentOrderShippedItems
                        };
                        if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                        {
                            orderItemModel.AttributeInfo = orderItem.AttributeDescription.Replace("UOM: ", "").Split(new string[] { "<br />" }, StringSplitOptions.None)[0];
                            orderItemModel.AttributeInfo = Regex.Replace(orderItemModel.AttributeInfo, @"\[(-|\+)\$[0-9]+(.{0,1})[0-9]*\]", "");
                        }
                        else
                            orderItemModel.AttributeInfo = "";
                        //change by hamza for unit price not show in itemstatus
                        if (order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax)
                        {
                            var unitPriceInclTaxInCustomerCurrency = orderItem.UnitPriceInclTax;
                            orderItemModel.UnitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency);

                            var priceInclTaxInCustomerCurrency = orderItem.PriceInclTax;
                            orderItemModel.SubTotal = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency);
                        }
                        else
                        {
                            var unitPriceExclTaxInCustomerCurrency = orderItem.UnitPriceExclTax;
                            orderItemModel.UnitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency);

                            var priceExclTaxInCustomerCurrency = orderItem.PriceExclTax;
                            orderItemModel.SubTotal = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency);
                        }
                        //add by hamza for filter by category
                        if (filterCategoryId > 0)
                        {
                            var allMappings = _productCategoryRepository.Query();
                            var allProducts = allMappings.Where(x => x.CategoryId == filterCategoryId);
                            foreach (var products in allProducts)
                            {
                                if (products.ProductId == orderItem.ProductId)
                                {
                                    orderModel.Items.Add(orderItemModel);
                                }
                            }
                        }
                        else
                            orderModel.Items.Add(orderItemModel);
                    }

                    /*if (order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax)
                    {
                        var unitPriceInclTaxInCustomerCurrency = orderItem.UnitPriceInclTax;
                        orderItemModel.UnitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency);

                        var priceInclTaxInCustomerCurrency = orderItem.PriceInclTax;
                        orderItemModel.SubTotal = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency);
                    }
                    else
                    {
                        var unitPriceExclTaxInCustomerCurrency = orderItem.UnitPriceExclTax;
                        orderItemModel.UnitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency);

                        var priceExclTaxInCustomerCurrency = orderItem.PriceExclTax;
                        orderItemModel.SubTotal = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency);
                    }*/
                }

                

                var customValues = _paymentService.DeserializeCustomValues(order);

                if (customValues != null)
                {
                    foreach (var item in customValues)
                    {
                        if (item.Key == "PO Number")
                            orderModel.PONumber = item.Value.ToString();
                    }
                }

                if (!order.PickupInStore)
                {
                    var shippingAddress = _addressService.GetAddressById(order.ShippingAddressId ?? 0);

                    if (shippingAddress == null)
                        continue;

                    if (!string.IsNullOrEmpty(shippingAddress.Company))
                        orderModel.ShipTo = shippingAddress.Company;
                    else
                        orderModel.ShipTo = order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName;
                    orderModel.ShipToCity = shippingAddress.City;
                    orderModel.ShipToState = _stateProvinceService.GetStateProvinceByAddress(shippingAddress) is StateProvince stateProvince ? stateProvince.Abbreviation : null;
                }

                var orderTotalInCustomerCurrency = order.OrderTotal;
                orderModel.OrderTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency);
               
                model.Orders.Add(orderModel);
            }
            return model;
        }

        public virtual CustomerOrderListModel PrepareCustomerOrderListModel(DateTime? startDateValue = null, DateTime? endDateValue = null, int orderStatusId = 0, bool returnView = false, int customerId = 0)
        {
            var currentCustomer = _workContextService.CurrentCustomer;
            var customer = customerId == 0 ? currentCustomer : _customerService.GetCustomerById(customerId);
            if (startDateValue == null || endDateValue == null)
            {
                startDateValue = DateTime.Now.AddDays(-35);
                endDateValue = DateTime.Now.AddDays(1);
            }
            else
            {
                if (((DateTime)endDateValue - (DateTime)startDateValue).TotalDays > 35)
                {
                    endDateValue = DateTime.Parse(startDateValue.ToString()).AddDays(35);
                }
                else
                    endDateValue = DateTime.Parse(endDateValue.ToString()).AddDays(1);
            }

            var model = new CustomerOrderListModel();
            List<int> osIds = new List<int>();
            if (orderStatusId != 0)
                osIds.Add(orderStatusId);
            var orders = _orderService.SearchOrders(storeId: InovatiqaDefaults.StoreId,
                customerId: customer.Id,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: osIds
                );
            var orderForCount = _orderService.SearchOrders(storeId: InovatiqaDefaults.StoreId,
            customerId: customer.Id,
            createdFromUtc: startDateValue,
            createdToUtc: endDateValue
            );
            model.AllOrdersCount = orderForCount.Count();
            model.OpenOrdersCount = orderForCount.Where(order => order.OrderStatusId == Convert.ToInt32(OrderStatus.Submitted) || order.OrderStatusId == Convert.ToInt32(OrderStatus.Processing) || order.OrderStatusId == Convert.ToInt32(OrderStatus.Pending)).ToList().Count();
            model.BackOrderedCount = orderForCount.Where(order => order.OrderStatusId == Convert.ToInt32(OrderStatus.Backorder)).ToList().Count();
            model.ShippedOrdersCount = orderForCount.Where(order => order.OrderStatusId == Convert.ToInt32(OrderStatus.Complete)).ToList().Count();
            foreach (var order in orders)
            {
                var returnAllowed = _orderProcessingService.IsReturnRequestAllowed(order);
                if (returnView == true && returnAllowed == false)
                    continue;

                var orderShipments = _shipmentService.GetShipmentsByOrderId(order.Id, true);
                var orderItems = _orderService.GetOrderItems(order.Id);

                var totalLines = 0;

                List<int> backOrderLineIds = new List<int>();

                foreach (var orderItem in orderItems)
                {
                    totalLines++;
                    var orderItemQty = orderItem.Quantity;
                    var shippedItemQty = 0;
                    foreach (var shipment in orderShipments)
                    {
                        var shipmentItems = _shipmentService.GetShipmentItemsByShipmentId(shipment.Id);
                        {
                            foreach (var shipmentItem in shipmentItems)
                            {
                                if (shipmentItem.OrderItemId == orderItem.Id)
                                {
                                    shippedItemQty += shipmentItem.Quantity;
                                }
                            }
                        }
                    }
                    if (orderItemQty != shippedItemQty)
                    {
                        if (backOrderLineIds.Contains(orderItem.Id) == false)
                            backOrderLineIds.Add(orderItem.Id);
                    }
                }
                //foreach (var shipment in orderShipments)
                //{
                //    var shipmentItems = _shipmentService.GetShipmentItemsByShipmentId(shipment.Id);
                //    foreach (var shipmentItem in shipmentItems)
                //    {
                //        foreach (var orderItem in orderItems)
                //        {
                //            totalLines++;
                //            if (shipmentItem.OrderItemId == orderItem.Id)
                //            {
                //                if (orderItem.Quantity != shipmentItem.Quantity)
                //                {
                //                    totalBackOrderLines++;
                //                    continue;
                //                }
                //            }
                //            else
                //            {
                //                totalBackOrderLines++;
                //                continue;
                //            }

                //        }
                //    }
                //}


                //var backOrderCounter = 0;
                //var totalLine = 0;
                //foreach (var orderItem in orderItems)
                //{
                //    totalLine++;
                //    var shippedLines = 0;
                //    foreach (var shipment in orderShipments)
                //    {
                //        var shipmentItems = _shipmentService.GetShipmentItemsByShipmentId(shipment.Id);
                //        foreach (var shipmentItem in shipmentItems)
                //        {
                //            if (shipmentItem.OrderItemId == orderItem.Id)
                //            {
                //                if (orderItem.Quantity == shipmentItem.Quantity)
                //                    shippedLines++;
                //                else
                //                    backOrderCounter++;
                //            }
                //            else
                //                backOrderCounter++;
                //        }
                //    }
                //}

                var orderModel = new CustomerOrderListModel.OrderDetailsModel
                {
                    Id = order.Id,
                    CreatedOn = _dateTimeHelperService.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc),
                    OrderStatusId = order.OrderStatusId,
                    OrderStatus = ((OrderStatus)order.OrderStatusId).ToString(),
                    PaymentStatusId = order.PaymentStatusId,
                    ShippingStatusId = order.ShippingStatusId,
                    IsReturnRequestAllowed = returnAllowed,
                    CustomOrderNumber = order.CustomOrderNumber,
                    TotalItems = totalLines,
                    Bo = backOrderLineIds.Count()
                };

                var customValues = _paymentService.DeserializeCustomValues(order);

                if (customValues != null)
                {
                    foreach (var item in customValues)
                    {
                        if (item.Key == "PO Number")
                            orderModel.PONumber = item.Value.ToString();
                    }
                }

                if (string.IsNullOrEmpty(orderModel.PONumber))
                    orderModel.PONumber = "N/A";

                if (!order.PickupInStore)
                {
                    var shippingAddress = _addressService.GetAddressById(order.ShippingAddressId ?? 0);

                    if (shippingAddress == null)
                        continue;

                    orderModel.ShipToId = shippingAddress.Id;

                    if (!string.IsNullOrEmpty(shippingAddress.Company))
                        orderModel.ShipTo = shippingAddress.Company;
                    else
                        orderModel.ShipTo = order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName;
                    orderModel.ShipToCity = shippingAddress.City;
                    orderModel.ShipToState = _stateProvinceService.GetStateProvinceByAddress(shippingAddress) is StateProvince stateProvince ? stateProvince.Abbreviation : null;
                }

                var orderTotalInCustomerCurrency = order.OrderTotal;
                orderModel.OrderTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency);

                model.Orders.Add(orderModel);
            }
            return model;
        }

        public virtual CustomerOrderListModel PrepareCustomerInvoicedOrderListModel(DateTime? startDateValue = null, DateTime? endDateValue = null, int invoiceFindByKey = -1, string invoiceFindByKeyValue = null, int customerId = 0)
        {
            var currentCustomer = _workContextService.CurrentCustomer;
            var customer = customerId == 0 ? currentCustomer : _customerService.GetCustomerById(customerId);
            if (startDateValue == null || endDateValue == null)
            {
                startDateValue = DateTime.Now.AddDays(-35);
                endDateValue = DateTime.Now.AddDays(1);
            }
            else
            {
                if (((DateTime)endDateValue - (DateTime)startDateValue).TotalDays > 35)
                {
                    endDateValue = DateTime.Parse(startDateValue.ToString()).AddDays(35);
                }
                else
                    endDateValue = DateTime.Parse(endDateValue.ToString()).AddDays(1);
            }

            var model = new CustomerOrderListModel();
            List<int> psIds = new List<int>();
            psIds.Add((int)PaymentStatus.Paid);

            List<int> osIds = new List<int>();
            osIds.Add((int)OrderStatus.Complete);

            List<int> ssIds = new List<int>();
            ssIds.Add((int)ShippingStatus.Shipped);

            var orders = _orderService.SearchOrders(storeId: InovatiqaDefaults.StoreId,
                customerId: customer.Id,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                psIds: psIds,
                osIds: osIds,
                ssIds: ssIds
                );
            foreach (var order in orders)
            {
                var orderModel = new CustomerOrderListModel.OrderDetailsModel
                {
                    Id = order.Id,
                    CreatedOn = _dateTimeHelperService.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc),
                    OrderStatusId = order.OrderStatusId,
                    OrderStatus = ((OrderStatus)order.OrderStatusId).ToString(),
                    PaymentStatusId = order.PaymentStatusId,
                    ShippingStatusId = order.ShippingStatusId,
                    CustomOrderNumber = order.CustomOrderNumber,
                    TotalItems = _orderService.GetOrderItems(order.Id).Count,
                    PaymentStatus = ((PaymentStatus)order.PaymentStatusId).ToString()
                };

                var customValues = _paymentService.DeserializeCustomValues(order);

                if (customValues != null)
                {
                    foreach (var item in customValues)
                    {
                        if (item.Key == "PO Number")
                            orderModel.PONumber = item.Value.ToString();
                    }
                }

                if (string.IsNullOrEmpty(orderModel.PONumber))
                    orderModel.PONumber = "N/A";

                if (!order.PickupInStore)
                {
                    var shippingAddress = _addressService.GetAddressById(order.ShippingAddressId ?? 0);

                    if (shippingAddress == null)
                        continue;
                    orderModel.ShipToId = shippingAddress.Id;
                    if (!string.IsNullOrEmpty(shippingAddress.Company))
                        orderModel.ShipTo = shippingAddress.Company;
                    else
                        orderModel.ShipTo = order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName;
                    orderModel.ShipToCity = shippingAddress.City;
                    orderModel.ShipToState = _stateProvinceService.GetStateProvinceByAddress(shippingAddress) is StateProvince stateProvince ? stateProvince.Abbreviation : null;
                }

                var orderTotalInCustomerCurrency = order.OrderTotal;
                orderModel.OrderTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency);

                if (invoiceFindByKey != -1 && !string.IsNullOrEmpty(invoiceFindByKeyValue))
                {
                    if (invoiceFindByKey == 0)
                    {
                        if (orderModel.PONumber == invoiceFindByKeyValue)
                            model.Orders.Add(orderModel);
                    }
                    else if (invoiceFindByKey == 1)
                    {
                        if (orderModel.CustomOrderNumber == invoiceFindByKeyValue)
                            model.Orders.Add(orderModel);
                    }
                    else if (invoiceFindByKey == 2)
                    {
                        if (orderModel.Id == int.Parse(invoiceFindByKeyValue))
                            model.Orders.Add(orderModel);
                    }
                    else if (invoiceFindByKey == 3)
                    {
                        if (orderModel.ShipTo.ToLower().Contains(invoiceFindByKeyValue.ToLower()))
                            model.Orders.Add(orderModel);
                    }
                    else if (invoiceFindByKey == 4)
                    {
                        if (orderModel.ShipToId == Convert.ToInt32(invoiceFindByKeyValue))
                            model.Orders.Add(orderModel);
                    }
                }
                else
                    model.Orders.Add(orderModel);
            }
            return model;
        }

        public virtual OrderDetailsModel PrepareOrderDetailsModel(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));
            IList<Shipment> shipments = null;
            var orderItems = _orderService.GetOrderItems(order.Id);
            var model = new OrderDetailsModel
            {
                Id = order.Id,
                CreatedOn = _dateTimeHelperService.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc),
                OrderStatusId = order.OrderStatusId,
                OrderStatus = ((OrderStatus)order.OrderStatusId).ToString(),
                IsReOrderAllowed = InovatiqaDefaults.IsReOrderAllowed,
                IsReturnRequestAllowed = _orderProcessingService.IsReturnRequestAllowed(order),
                PdfInvoiceDisabled = false, //InovatiqaDefaults.DisablePdfInvoicesForPendingOrders && order.OrderStatusId == (int)OrderStatus.Pending,
                CustomOrderNumber = order.CustomOrderNumber,
                ShippingStatusId = order.ShippingStatusId,
                ShippingStatus = ((ShippingStatus)order.ShippingStatusId).ToString()
            };
            if (order.ShippingStatusId != (int)ShippingStatus.ShippingNotRequired)
            {
                model.IsShippable = true;
                model.PickupInStore = order.PickupInStore;
                if (!order.PickupInStore)
                {
                    var shippingAddress = _addressService.GetAddressById(order.ShippingAddressId ?? 0);

                    _addressModelFactory.PrepareAddressModel(model.ShippingAddress,
                        address: shippingAddress,
                        excludeProperties: false);
                }
                else if (order.PickupAddressId.HasValue && _addressService.GetAddressById(order.PickupAddressId.Value) is Address pickupAddress)
                {
                    model.PickupAddress = new AddressModel
                    {
                        Address1 = pickupAddress.Address1,
                        City = pickupAddress.City,
                        County = pickupAddress.County,
                        CountryName = _countryService.GetCountryByAddress(pickupAddress)?.Name ?? string.Empty,
                        ZipPostalCode = pickupAddress.ZipPostalCode
                    };
                }

                model.ShippingMethod = order.ShippingMethod;

                shipments = _shipmentService.GetShipmentsByOrderId(order.Id, true).OrderBy(x => x.CreatedOnUtc).ToList();
                foreach (var shipment in shipments)
                {
                    var shipmentModel = new OrderDetailsModel.ShipmentBriefModel
                    {
                        Id = shipment.Id,
                        TrackingNumber = shipment.TrackingNumber,
                    };
                    if (shipment.ShippedDateUtc.HasValue)
                        shipmentModel.ShippedDate = _dateTimeHelperService.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc);
                    if (shipment.DeliveryDateUtc.HasValue)
                        shipmentModel.DeliveryDate = _dateTimeHelperService.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc);

                    var shipmentTotalPartialInclTax = 0.0m;
                    var shipmentTotalPartialExclTax = 0.0m;
                    foreach (var orderItem in orderItems)
                    {
                        var shipmentItems = _shipmentService.GetShipmentItemsByShipmentId(shipment.Id);
                        if (shipmentItems != null)
                        {
                            var item = shipmentItems.Where(x => x.OrderItemId == orderItem.Id).FirstOrDefault();
                            if (item == null)
                                continue;
                            else
                            {
                                //orderItem.Quantity = item.Quantity;
                                shipmentTotalPartialInclTax += orderItem.Quantity * orderItem.UnitPriceInclTax;
                                shipmentTotalPartialExclTax += orderItem.Quantity * orderItem.UnitPriceExclTax;
                            }
                        }
                    }

                    shipmentModel.ShipmentTotalPartialInclTax = _priceFormatter.FormatPrice(shipmentTotalPartialInclTax);
                    shipmentModel.ShipmentTotalPartialExclTax = _priceFormatter.FormatPrice(shipmentTotalPartialExclTax);

                    model.Shipments.Add(shipmentModel);
                }
            }

            var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

            _addressModelFactory.PrepareAddressModel(model.BillingAddress,
                address: billingAddress,
                excludeProperties: false);

            model.VatNumber = order.VatNumber;

            var languageId = InovatiqaDefaults.LanguageId;

            var customer = _customerService.GetCustomerById(order.CustomerId);
            var paymentMethod = InovatiqaDefaults.PaymentMethodName;
            model.PaymentMethod = paymentMethod;
            model.PaymentMethodStatusId = order.PaymentStatusId;
            model.PaymentMethodStatus = ((PaymentStatus)order.PaymentStatusId).ToString();
            model.CanRePostProcessPayment = _paymentService.CanRePostProcessPayment(order);
            model.CustomValues = _paymentService.DeserializeCustomValues(order);

            if (model.CustomValues != null)
            {
                foreach (var item in model.CustomValues)
                {
                    if(item.Key == "PO Number")
                    {
                        model.PONumber = item.Value.ToString();
                    }
                }
            }

            if (order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax && !InovatiqaDefaults.ForceTaxExclusionFromOrderSubtotal)
            {
                var orderSubtotalInclTaxInCustomerCurrency = order.OrderSubtotalInclTax;
                model.OrderSubtotal = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency);
                var orderSubTotalDiscountInclTaxInCustomerCurrency = order.OrderSubTotalDiscountInclTax;
                if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
                    model.OrderSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency);
            }
            else
            {
                var orderSubtotalExclTaxInCustomerCurrency = order.OrderSubtotalExclTax;
                model.OrderSubtotal = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency);
                var orderSubTotalDiscountExclTaxInCustomerCurrency = order.OrderSubTotalDiscountExclTax;
                if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero)
                    model.OrderSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInCustomerCurrency);
            }

            if (order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax)
            {
                var orderShippingInclTaxInCustomerCurrency = order.OrderShippingInclTax;
                model.OrderShipping = _priceFormatter.FormatPrice(orderShippingInclTaxInCustomerCurrency);
                var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = order.PaymentMethodAdditionalFeeInclTax;
                if (paymentMethodAdditionalFeeInclTaxInCustomerCurrency > decimal.Zero)
                    model.PaymentMethodAdditionalFee = _priceFormatter.FormatPrice(paymentMethodAdditionalFeeInclTaxInCustomerCurrency);
            }
            else
            {
                var orderShippingExclTaxInCustomerCurrency = order.OrderShippingExclTax;
                model.OrderShipping = _priceFormatter.FormatPrice(orderShippingExclTaxInCustomerCurrency);
                var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = order.PaymentMethodAdditionalFeeExclTax;
                if (paymentMethodAdditionalFeeExclTaxInCustomerCurrency > decimal.Zero)
                    model.PaymentMethodAdditionalFee = _priceFormatter.FormatPrice(paymentMethodAdditionalFeeExclTaxInCustomerCurrency);
            }

            var displayTax = true;
            var displayTaxRates = true;
            if (InovatiqaDefaults.HideTaxInOrderSummary && order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                if (order.OrderTax == 0 && InovatiqaDefaults.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
            }
            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;
            model.DisplayTaxShippingInfo = InovatiqaDefaults.DisplayTaxShippingInfoOrderDetailsPage;
            model.PricesIncludeTax = order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax;

            var orderDiscountInCustomerCurrency = order.OrderDiscount;
            if (orderDiscountInCustomerCurrency > decimal.Zero)
                model.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency);


            var orderTotalInCustomerCurrency = order.OrderTotal;
            model.OrderTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency);

            model.CheckoutAttributeInfo = order.CheckoutAttributeDescription;

            foreach (var orderNote in _orderService.GetOrderNotesByOrderId(order.Id, true)
                .OrderByDescending(on => on.CreatedOnUtc)
                .ToList())
            {
                model.OrderNotes.Add(new OrderDetailsModel.OrderNote
                {
                    Id = orderNote.Id,
                    HasDownload = orderNote.DownloadId > 0,
                    Note = _orderService.FormatOrderNoteText(orderNote),
                    CreatedOn = _dateTimeHelperService.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc)
                });
            }

            model.ShowSku = InovatiqaDefaults.ShowSkuOnProductDetailsPage;
            model.ShowVendorName = InovatiqaDefaults.ShowVendorOnOrderDetailsPage;

            //var orderItems = _orderService.GetOrderItems(order.Id);

            foreach (var orderItem in orderItems)
            {
                var product = _productService.GetProductById(orderItem.ProductId);
                var data = orderItem.AttributeDescription;
                //var UOM = data.Substring(data.IndexOf("UOM"), data.IndexOf("<") >= 0 ? data.IndexOf("<") : data.Length);
                var UOM = orderItem.AttributeDescription.Replace("UOM: ", "").Split(new string[] { "<br />" }, StringSplitOptions.None)[0];
                //change by hamza
                UOM = Regex.Replace(UOM, @"\[(-|\+)\$[0-9]+(.{0,1})[0-9]*\]", "");
                var orderItemModel = new OrderDetailsModel.OrderItemModel
                {
                    Id = orderItem.Id,
                    OrderItemGuid = orderItem.OrderItemGuid,
                    Sku = _productService.FormatSku(product, orderItem.AttributesXml),
                    VendorName = _vendorService.GetVendorById(product.VendorId)?.Name ?? string.Empty,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductSeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId),
                    Quantity = orderItem.Quantity,
                    //AttributeInfo = orderItem.AttributeDescription.Trim().Remove(orderItem.AttributeDescription.IndexOf("UOM:")+4),
                    AttributeInfo = UOM,
                    ManufacturerPartNumber = product.ManufacturerPartNumber,
                    ProductManufacturers = PrepareProductManufacturerModels(product)
                };

                if (shipments != null)
                {
                    var currentShippingQuantity = 0;
                    foreach (var shipment in shipments)
                    {
                        var shipmentItems = _shipmentService.GetShipmentItemsByShipmentId(shipment.Id);
                        if(shipmentItems != null)
                        {
                            var shippingItem = shipmentItems.Where(x => x.OrderItemId == orderItemModel.Id);
                            if(shippingItem != null)
                            {
                                if (!string.IsNullOrEmpty(shipment.TrackingNumber))
                                {
                                    var shipmentTracker = _shipmentService.GetShipmentTracker(shipment);
                                    if (shipmentTracker != null)
                                    {
                                        orderItemModel.TrackingNumberUrl = shipmentTracker.GetUrl(shipment.TrackingNumber);
                                    }
                                }
                                foreach (var item in shippingItem)
                                {
                                    currentShippingQuantity += item.Quantity;
                                }
                            }
                        }
                    }
                    if (currentShippingQuantity == orderItemModel.Quantity)
                        orderItemModel.IsShipped = true;
                    else
                        orderItemModel.IsShipped = false;

                    orderItemModel.Bo = orderItemModel.Quantity - currentShippingQuantity;
                    orderItemModel.Shipped = currentShippingQuantity;
                }

                model.Items.Add(orderItemModel);

                if (order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax)
                {
                    var unitPriceInclTaxInCustomerCurrency = orderItem.UnitPriceInclTax;
                    orderItemModel.UnitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency);

                    var priceInclTaxInCustomerCurrency = orderItem.PriceInclTax;
                    orderItemModel.SubTotal = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency);
                }
                else
                {
                    var unitPriceExclTaxInCustomerCurrency = orderItem.UnitPriceExclTax;
                    orderItemModel.UnitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency);

                    var priceExclTaxInCustomerCurrency = orderItem.PriceExclTax;
                    orderItemModel.SubTotal = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency);
                }
            }

            return model;
        }

        public virtual ShipmentDetailsModel PrepareShipmentDetailsModel(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var order = _orderService.GetOrderById(shipment.OrderId);

            if (order == null)
                throw new Exception("order cannot be loaded");
            var model = new ShipmentDetailsModel
            {
                Id = shipment.Id
            };
            if (shipment.ShippedDateUtc.HasValue)
                model.ShippedDate = _dateTimeHelperService.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc);
            if (shipment.DeliveryDateUtc.HasValue)
                model.DeliveryDate = _dateTimeHelperService.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc);

            if (!string.IsNullOrEmpty(shipment.TrackingNumber))
            {
                model.TrackingNumber = shipment.TrackingNumber;
                model.TrackingNumberUrl = _shipmentTrackerService.GetUrl(shipment.TrackingNumber);
                if (InovatiqaDefaults.DisplayShipmentEventsToCustomers)
                {
                    var shipmentEvents = _shipmentTrackerService.GetShipmentEvents(shipment.TrackingNumber);
                    if (shipmentEvents != null)
                        foreach (var shipmentEvent in shipmentEvents)
                        {
                            var shipmentStatusEventModel = new ShipmentDetailsModel.ShipmentStatusEventModel();
                            var shipmentEventCountry = _countryService.GetCountryByTwoLetterIsoCode(shipmentEvent.CountryCode);
                            shipmentStatusEventModel.Country = shipmentEventCountry != null
                                ? shipmentEventCountry.Name : shipmentEvent.CountryCode;
                            shipmentStatusEventModel.Date = shipmentEvent.Date;
                            shipmentStatusEventModel.EventName = shipmentEvent.EventName;
                            shipmentStatusEventModel.Location = shipmentEvent.Location;
                            model.ShipmentStatusEvents.Add(shipmentStatusEventModel);
                        }
                }
            }

            model.ShowSku = InovatiqaDefaults.ShowSkuOnProductDetailsPage;
            foreach (var shipmentItem in _shipmentService.GetShipmentItemsByShipmentId(shipment.Id))
            {
                var orderItem = _orderService.GetOrderItemById(shipmentItem.OrderItemId);
                if (orderItem == null)
                    continue;

                var product = _productService.GetProductById(orderItem.ProductId);

                var shipmentItemModel = new ShipmentDetailsModel.ShipmentItemModel
                {
                    Id = shipmentItem.Id,
                    Sku = _productService.FormatSku(product, orderItem.AttributesXml),
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductSeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId),
                    AttributeInfo = orderItem.AttributeDescription,
                    QuantityOrdered = orderItem.Quantity,
                    QuantityShipped = shipmentItem.Quantity,
                };
                model.Items.Add(shipmentItemModel);
            }

            model.Order = PrepareOrderDetailsModel(order);

            return model;
        }
        //change by hamza
        public virtual List<CustomerReorderGuideModel> PrepareCustomerReOrderGuideModel(int filterCategories = -1)
        {
            var customer = _workContextService.CurrentCustomer;

            DateTime startDateValue = DateTime.Now.AddDays(-90);
            DateTime endDateValue = DateTime.Now.AddDays(1);

            var orders = _orderService.SearchOrders(storeId: InovatiqaDefaults.StoreId,
                customerId: customer.Id,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue
                );
            List<CustomerReorderGuideModel> models = new List<CustomerReorderGuideModel>();
            
            foreach (var order in orders)
            {
                var orderItems = _orderService.GetOrderItems(order.Id);
                foreach (var orderLine in orderItems)
                {
                    var product = _productService.GetProductById(orderLine.ProductId);
                    var matches = models.Where(item => item.MSku == product.Msku);
                    if (matches.Count() > 0)
                        continue;
                    var model = new CustomerReorderGuideModel
                    {
                        OrderId = order.Id,
                        Id = orderLine.Id,
                        //added by hamza
                        ProductId = product.Id,
                        Name = product.Name,
                        ManufacturerPartNumber = product.ManufacturerPartNumber,
                        MSku = product.Msku,
                        ProductSeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId),
                        OrderDate = order.CreatedOnUtc,
                        ReOrderDate = orderLine.ReorderDateUtc,
                        Qty = orderLine.Quantity,
                        OrderItemPrice = _priceFormatter.FormatPrice(orderLine.UnitPriceInclTax),
                        AttributeInfo = orderLine.AttributeDescription,
                        //AttributesXml = orderLine.AttributesXml,
                        ProductAttributesAndValues = _productModelFactory.PrepareProductAttributeModels(product, null)
                    };
                    //////////////////model.ProductAttributes = PrepareProductAttributeModels(product);
                    model.ProductManufacturers = PrepareProductManufacturerModels(product);
                    //add by hamza for filter categories
                    if(filterCategories > 0)
                    {
                        var allMappings = _productCategoryRepository.Query();
                        var allProducts = allMappings.Where(x => x.CategoryId == filterCategories);
                        foreach(var products in allProducts)
                        {
                            if(products.ProductId == orderLine.ProductId)
                            {
                                models.Add(model);
                            }
                        }
                    }
                    else
                    {
                        models.Add(model);
                    }
                }  
            }
         
            return models;
        }

        public virtual List<OrderApprovalModel> OrderWaitingForApproval()
        {
            var customer = _workContextService.CurrentCustomer;
            var model = new List<OrderApprovalModel>();
            var childIds = _customerService.getAllChildAccounts(customer);
            var allOrders = _suspendedCartRepository.Query().Where(ApprovalOrders => childIds.Contains(ApprovalOrders.CustomerId) && ApprovalOrders.SuspendedCartTypeId == 10);
            foreach(var Order in allOrders)
            {
                var Items = _shoppingCartItemRepository.Query().Where(item => item.SuspendedCartId != null && item.SuspendedCartId == Order.Id);
                var OAM = new OrderApprovalModel();
                OAM.Email = _customerService.GetCustomerById(Order.CustomerId).Email;
                var temp = _customerService.GetAddressesByCustomerId(Order.CustomerId).FirstOrDefault();
                OAM.Name = temp.FirstName + " " + temp.LastName;
                decimal price = 0;
                var totalQuantity = 0;
                foreach(var itm in Items)
                {
                    var product = _productService.GetProductById(itm.ProductId);
                    price += ((itm.Quantity) * (product.Price));
                    totalQuantity += itm.Quantity;
                    OAM.Items.Add(new ItemModel
                    {
                        Id = itm.ProductId,
                        Quantity = itm.Quantity,
                        Name = product.Name,
                        UnitPrice = _priceFormatter.FormatPrice(product.Price),
                        TotalPrice = _priceFormatter.FormatPrice(product.Price * itm.Quantity)
                    });
                }
                OAM.TotalItemsQuantity = totalQuantity;
                OAM.TotalPrice = _priceFormatter.FormatPrice(price);
                OAM.Id = Order.Id;
                OAM.UserId = Order.CustomerId;
                if(customer.ParentId != null && customer.ParentId != 0 && customer.MaxOrderApprovalValue != 0) // if child account
                {
                    if(price <= customer.MaxOrderApprovalValue)         // check for price amouint condition
                        model.Add(OAM);
                }
                else
                {
                    model.Add(OAM);                                     // otherwise, don't check
                }
            }
            return model;
        }

        public virtual List<CustomerOrderApprovalQueue> CustomerOrderInQueue()
        {
            var model = new List<CustomerOrderApprovalQueue>();
            var customer = _workContextService.CurrentCustomer;
            var orders = _suspendedCartRepository.Query().Where(sc => sc.CustomerId == customer.Id && sc.SuspendedCartTypeId != null).ToList();
            foreach(var Order in orders)
            {
                int quantity = 0;
                decimal price = 0;
                var order = new CustomerOrderApprovalQueue();
                order.Id = Order.Id;
                order.IsApproved = Order.SuspendedCartTypeId == (int)ShoppingCartType.ApprovedOrder;
                var items = _shoppingCartItemRepository.Query().Where(sci => sci.ShoppingCartTypeId == (int)ShoppingCartType.SuspendedCart && sci.SuspendedCartId == Order.Id);
                foreach (var item in items)
                {
                    quantity += item.Quantity;
                    var product = _productService.GetProductById(item.ProductId);
                    price += (item.Quantity * product.Price);
                    order.Items.Add(new OrderItems
                    {
                        Id = item.Id,
                        Name = product.Name,
                        UnitPrice = _priceFormatter.FormatPrice(product.Price),
                        Quantity = item.Quantity,
                        TotalPrice = _priceFormatter.FormatPrice(item.Quantity * product.Price)
                    });
                }
                order.TotalItems = items.Count();
                order.TotalQunaity = quantity;
                order.TotalPrice = _priceFormatter.FormatPrice(price);
                model.Add(order);
            }
            return model;
        }

        #endregion
    }
}