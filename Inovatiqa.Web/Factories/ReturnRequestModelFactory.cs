using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Payments.Interfaces;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Order;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Web.Factories
{
    public partial class ReturnRequestModelFactory : IReturnRequestModelFactory
    {
        #region Fields

        private readonly ICurrencyService _currencyService;
        private readonly IDateTimeHelperService _dateTimeHelperService;
        private readonly IOrderService _orderService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContextService _workContextService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IPaymentService _paymentService;
        private readonly IAddressService _addressService;
        private readonly IShipmentService _shipmentService;
        private readonly ICustomNumberFormatterService _customNumberFormatterService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public ReturnRequestModelFactory(ICurrencyService currencyService,
            IDateTimeHelperService dateTimeHelperService,
            IOrderService orderService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IReturnRequestService returnRequestService,
            IUrlRecordService urlRecordService,
            IWorkContextService workContextService,
            IManufacturerService manufacturerService,
            IWorkflowMessageService workflowMessageService,
            IPaymentService paymentService,
            IAddressService addressService,
            ICustomerService customerService,
            ICustomNumberFormatterService customNumberFormatterService,
            IShipmentService shipmentService)
        {
            _currencyService = currencyService;
            _dateTimeHelperService = dateTimeHelperService;
            _orderService = orderService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _returnRequestService = returnRequestService;
            _urlRecordService = urlRecordService;
            _workContextService = workContextService;
            _manufacturerService = manufacturerService;
            _workflowMessageService = workflowMessageService;
            _paymentService = paymentService;
            _addressService = addressService;
            _shipmentService = shipmentService;
            _customerService = customerService;
            _customNumberFormatterService = customNumberFormatterService;
        }

        #endregion

        #region Utilities

        protected virtual IList<Inovatiqa.Web.Models.Order.CustomerReturnRequestsModel.ManufacturerBriefInfoModel> PrepareProductManufacturerModels(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = _manufacturerService.GetProductManufacturersByProductId(product.Id)
                .Select(pm =>
                {
                    var manufacturer = _manufacturerService.GetManufacturerById(pm.ManufacturerId);
                    var modelMan = new Inovatiqa.Web.Models.Order.CustomerReturnRequestsModel.ManufacturerBriefInfoModel
                    {
                        Id = manufacturer.Id,
                        Name = manufacturer.Name,
                        SeName = _urlRecordService.GetActiveSlug(manufacturer.Id, InovatiqaDefaults.ManufacturerSlugName, InovatiqaDefaults.LanguageId)
                    };

                    return modelMan;
                }).ToList();

            return model;
        }

        #endregion

        #region Methods

        public virtual SubmitReturnRequestModel.OrderItemModel PrepareSubmitReturnRequestOrderItemModel(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            var order = _orderService.GetOrderById(orderItem.OrderId);
            var product = _productService.GetProductById(orderItem.ProductId);

            var model = new SubmitReturnRequestModel.OrderItemModel
            {
                Id = orderItem.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId),
                AttributeInfo = orderItem.AttributeDescription,
                Quantity = orderItem.Quantity
            };

            var languageId = InovatiqaDefaults.LanguageId;

            if (order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax)
            {
                var unitPriceInclTaxInCustomerCurrency = orderItem.UnitPriceInclTax;
                model.UnitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency);
            }
            else
            {
                var unitPriceExclTaxInCustomerCurrency = orderItem.UnitPriceExclTax;
                model.UnitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency);
            }

            return model;
        }

        public virtual SubmitReturnRequestModel PrepareSubmitReturnRequestModel(SubmitReturnRequestModel model,
            Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.OrderId = order.Id;
            model.AllowFiles = InovatiqaDefaults.ReturnRequestsAllowFiles;
            model.CustomOrderNumber = order.CustomOrderNumber;

            model.AvailableReturnReasons = _returnRequestService.GetAllReturnRequestReasons()
                .Select(rrr => new SubmitReturnRequestModel.ReturnRequestReasonModel
                {
                    Id = rrr.Id,
                    Name = rrr.Name
                }).ToList();

            model.AvailableReturnActions = _returnRequestService.GetAllReturnRequestActions()
                .Select(rra => new SubmitReturnRequestModel.ReturnRequestActionModel
                {
                    Id = rra.Id,
                    Name = rra.Name
                })
                .ToList();

            var orderItems = _orderService.GetOrderItems(order.Id, isNotReturnable: false);
            foreach (var orderItem in orderItems)
            {
                var orderItemModel = PrepareSubmitReturnRequestOrderItemModel(orderItem);
                model.Items.Add(orderItemModel);
            }

            return model;
        }

        public virtual CustomerReturnRequestsModel PrepareCustomerReturnRequestsModel(DateTime? startDateValue = null, DateTime? endDateValue = null, int returnStatusId = -1, bool orderByDate = true, int returnRequestFindByKey = -1, string returnRequestFindByKeyValue = null)
        {
            //change by hamza
            var model = new CustomerReturnRequestsModel();

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

            var customer = _workContextService.CurrentCustomer;

            ReturnRequestStatus? returnRequestStatus = null;

            if (returnStatusId != -1)
                returnRequestStatus = ((ReturnRequestStatus)returnStatusId);

            var returnRequests = _returnRequestService.SearchReturnRequests(InovatiqaDefaults.StoreId, customer.Id, 0, "", returnRequestStatus, startDateValue, endDateValue, 0, int.MaxValue, false, orderByDate);
            foreach (var returnRequest in returnRequests)
            {
                var orderItem = _orderService.GetOrderItemById(returnRequest.OrderItemId);
                var order = _orderService.GetOrderById(orderItem.OrderId);
                var poNumber = string.Empty;
                if(order != null)
                {
                    var customValues = _paymentService.DeserializeCustomValues(order);
                    if (customValues != null)
                    {
                        foreach (var item in customValues)
                        {
                            if (item.Key == "PO Number")
                                poNumber = item.Value.ToString();
                        }
                    }
                }

                
                if (orderItem != null)
                {
                    var product = _productService.GetProductById(orderItem.ProductId);

                    var returnStatus = ((ReturnRequestStatus)returnRequest.ReturnRequestStatusId).ToString() == "Pending" | ((ReturnRequestStatus)returnRequest.ReturnRequestStatusId).ToString() == "Received" ? "Open" : "Closed";
                    var address = _addressService.GetAddressById(order.BillingAddressId);
                    var itemModel = new CustomerReturnRequestsModel.ReturnRequestModel
                    {
                        Id = returnRequest.Id,
                        CustomNumber = returnRequest.CustomNumber,
                        ReturnRequestStatus = returnStatus,
                        ProductId = product.Id,
                        ProductName = product.Name,
                        ProductSeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId),
                        Quantity = returnRequest.Quantity,
                        ReturnAction = returnRequest.RequestedAction,
                        ReturnReason = returnRequest.ReasonForReturn,
                        Comments = returnRequest.CustomerComments,
                        CreatedOn = _dateTimeHelperService.ConvertToUserTime(returnRequest.CreatedOnUtc, DateTimeKind.Utc),
                        PendingCredit = _priceFormatter.FormatPrice(orderItem.PriceInclTax),
                        ManufacturerPartNumber = product.ManufacturerPartNumber,
                        ProductManufacturers = PrepareProductManufacturerModels(product),
                        AttributeInfo = orderItem.AttributeDescription,
                        OrderId = orderItem.OrderId,
                        PoNumber = poNumber,
                        Freight = _priceFormatter.FormatPrice(order.OrderShippingInclTax),
                        TotalProductCredit = _priceFormatter.FormatPrice(orderItem.PriceInclTax - order.OrderShippingInclTax),
                        BillingAddress = new CustomerReturnRequestsModel.BillingAddressModel
                        {
                            AddressLine = address.Address1,
                            CompanyName = address.Company,
                            Country = address.City,
                            ZipCode = address.ZipPostalCode
                        }
                    };
                    if (orderItem.AttributeDescription != "")
                    {
                        int startIndex = orderItem.AttributeDescription.IndexOf("UOM:") + 4;
                        //string UOMParam = orderItem.AttributeDescription.Substring(startIndex, orderItem.AttributeDescription.IndexOf("\r"));
                        //itemModel.AttributeInfo = UOMParam.Trim();
                    }
                    else
                        itemModel.AttributeInfo = "";
                    model.Items.Add(itemModel);
                }
            }

            if(returnRequestFindByKey != -1 && !string.IsNullOrEmpty(returnRequestFindByKeyValue))
            {
                if (returnRequestFindByKey == 0)
                    model.Items = model.Items.Where(x => x.PoNumber == returnRequestFindByKeyValue).ToList();
                else if (returnRequestFindByKey == 1)
                    model.Items = model.Items.Where(x => x.OrderId == int.Parse(returnRequestFindByKeyValue)).ToList();
                else if (returnRequestFindByKey == 2)
                    model.Items = model.Items.Where(x => x.Id == int.Parse(returnRequestFindByKeyValue)).ToList();
                else if (returnRequestFindByKey == 3)
                    model.Items = model.Items.Where(x => x.ProductId == int.Parse(returnRequestFindByKeyValue)).ToList();
                else if (returnRequestFindByKey == 4)
                    model.Items = model.Items.Where(x => x.ProductName.ToLower().Contains(returnRequestFindByKeyValue.ToLower())).ToList();
                else if (returnRequestFindByKey == 5)
                {
                    var items = new List<CustomerReturnRequestsModel.ReturnRequestModel>();
                    foreach (var item in model.Items)
                    {
                        foreach(var manufacturer in item.ProductManufacturers)
                        {
                            if(manufacturer.Name.ToLower().Contains(returnRequestFindByKeyValue.ToLower()))
                            {
                                items.Add(item);
                            }
                        }
                    }
                    model.Items = items;
                }
                else if (returnRequestFindByKey == 6)
                    model.Items = model.Items.Where(x => x.ManufacturerPartNumber.ToLower().Contains(returnRequestFindByKeyValue.ToLower())).ToList();
            }
            return model;
        }
        public virtual ReturnRequestItemsSelectionModel PrepareItemsSelectionModel(DateTime? startDateValue = null, DateTime? endDateValue = null)
        {
            var model = new ReturnRequestItemsSelectionModel();
            var customer = _workContextService.CurrentCustomer;
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
            //var AllOrders = _orderService.GetAllShippedOrdersByCustomer(customer);
            var AllOrders = _orderService.SearchOrders(storeId: InovatiqaDefaults.StoreId,
                customerId: customer.Id,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue
                );

            foreach (var order in AllOrders)
            {
                var attributeValues = _paymentService.DeserializeCustomValues(order);
                var AllItems = _orderService.GetOrderItems(order.Id);
                foreach (var item in AllItems)
                {
                    if(item.Quantity > 0)
                    {
                        _productService.GetProductById(item.Id);
                        var Product = _productService.GetProductById(item.ProductId);
                        var shipment = _shipmentService.GetShipmentsByOrderId(order.Id);
                        if(shipment.Count > 0)
                        {
                            var shipment1 = _shipmentService.GetShipmentById(shipment[0].Id);
                            var Object = new ReturnRequestItemsSelectionModel
                            {
                                Itemno = item.Id,
                                Quantity = item.Quantity,
                                Price = item.UnitPriceInclTax,
                                Description = Product.Name,
                                OrderNo = order.Id,
                                InvoiceDate = order.PaidDateUtc.ToString(),
                                PO = "N/A",
                                UOM = item.AttributeDescription.Replace("UOM: ", "").Split(new string[] { "<br />" }, StringSplitOptions.None)[0],
                                Invoice = "",
                                IsSelected = false,
                                ReturnQuantity = 0
                            };
                            foreach (var keys in attributeValues)
                                if (keys.Key == "PO Number")
                                    Object.PO = keys.Value.ToString();
                            foreach (var ShipmentItems in shipment1.Order.OrderItem)
                                if (ShipmentItems.Id == item.Id)
                                    Object.Invoice = shipment1.Id.ToString();
                            model.Items.Add(Object);
                        }
                        
                    }
                }
            }
            foreach(var action in _returnRequestService.GetAllReturnRequestActions())
            {
                model.ReturnActions.Add(new System.Collections.Generic.KeyValuePair<int, string>(action.Id, action.Name));
            }
            foreach(var reason in _returnRequestService.GetAllReturnRequestReasons())
            {
                model.ReturnReasons.Add(new System.Collections.Generic.KeyValuePair<int, string>(reason.Id, reason.Name));
            }
            return model;
        }
        public virtual ReturnRequestItemsSelectionModel SelectItemsFromModel(ReturnRequestItemsSelectionModel model, int[] SelectedItems = null, int[] SelectedQuantity = null, int[] SelectedReason = null, int[] SelectedAction = null)
        {
            for(int i = 0; i < model.Items.Count; i++)
                for(int j = 0; j<SelectedItems.Length; j++)
                    if(model.Items[i].Itemno == SelectedItems[j])
                    {
                        model.Items[i].IsSelected = true;
                        model.Items[i].ReturnQuantity = SelectedQuantity[j];
                        model.SelectedItemsPrice += model.Items[i].Price * Convert.ToDecimal(model.Items[i].ReturnQuantity);
                    }
            for (int i = 0; i < SelectedItems.Length; i++)
            {
                model.TotalItems += 1;
                model.SelectedItems.Add(SelectedItems[i]);
                model.SelectedItemsQuantity.Add(SelectedQuantity[i]);
                model.SelectedItemsReturnReason.Add(SelectedReason[i]);
                model.SelectedItemsReturnAction.Add(SelectedAction[i]);
            }
            return model;
        }
        public virtual ReturnRequestItemsSelectionModel SelectCurrentItem(ReturnRequestItemsSelectionModel model, int Selected = 0, int Quantity = 0, int Reason = 0, int Action = 0)
        {
            for(int i = 0; i < model.Items.Count; i++)
                if(model.Items[i].Itemno == Selected && Quantity > 0 && !model.SelectedItems.Contains(Selected))
                {
                    model.Items[i].IsSelected = true;
                    model.Items[i].ReturnQuantity = Quantity;
                    model.TotalItems += 1;
                    model.SelectedItemsPrice += model.Items[i].Price * Quantity;
                }
                else if (model.SelectedItems.Contains(Selected) && Quantity > 0 && model.Items[i].Itemno == Selected)
                {
                    model.Items[i].IsSelected = true;
                    model.SelectedItemsPrice -= model.Items[i].Price * model.Items[i].ReturnQuantity;
                    model.Items[i].ReturnQuantity = Quantity;
                    model.SelectedItemsPrice += model.Items[i].Price * model.Items[i].ReturnQuantity;
                    model.SelectedItemsQuantity[i] = Quantity;
                }
            if (Selected > 0 && Quantity > 0 && !model.SelectedItems.Contains(Selected))        // if we have a new item to select
            {
                model.SelectedItems.Add(Selected);
                model.SelectedItemsQuantity.Add(Quantity);
                model.SelectedItemsReturnAction.Add(Action);
                model.SelectedItemsReturnReason.Add(Reason);
            }
            else if (Selected > 0 && Quantity == 0)                                             // if we have to remove an item
            {
                int index = model.SelectedItems.IndexOf(Selected);
                model.SelectedItems.RemoveAt(index);
                model.SelectedItemsQuantity.RemoveAt(index);
                model.SelectedItemsReturnAction.RemoveAt(index);
                model.SelectedItemsReturnReason.RemoveAt(index);
                model.TotalItems--;
                for (int i = 0; i < model.Items.Count; i++)
                if (model.Items[i].Itemno == Selected)
                {
                    model.Items[i].IsSelected = false;
                    model.SelectedItemsPrice -= model.Items[i].Price * model.Items[i].ReturnQuantity;
                }
            }
            
            return model;
        }

        public virtual ReturnRequestItemsSelectionModel PrepareShippingInfoModel(ReturnRequestItemsSelectionModel model, int[] items, int[] quantities, decimal totalPrice, int[] returnReasons, int[] selectedActions = null)
        {
            model.SelectedItemsPrice = totalPrice;
            foreach (var quantity in quantities)
                model.SelectedItemsQuantity.Add(quantity);
            foreach (var item in items)
                model.SelectedItems.Add(item);
            foreach (var returnReason in returnReasons)
                model.SelectedItemsReturnReason.Add(returnReason);
            foreach (var action in selectedActions)
                model.SelectedItemsReturnAction.Add(action);
            return model;
        }

        public virtual ReturnRequestItemsSelectionModel PrepareCustomerReturnRequestCompletedModel(ReturnRequestItemsSelectionModel model, Customer customer, int[] selected = null, int[] quantity = null, decimal credit = 0, int shippingLabel = 0, string email1 = "", string email2 = "", int[] reasons = null, int[] actions = null)
        {
            var customerReturnRequestModel = new CustomerReturnRequest
            {
                CustomerId = customer.Id,
                ShippingLabel = shippingLabel,
                PrimaryContantEmail = email1,
                CreatedDateUtc = DateTime.UtcNow,
                UpdatedDateUtc = DateTime.UtcNow
            };
            _returnRequestService.InsertCustomerReturnRequest(customerReturnRequestModel);
            //changes by hamza for fixing bug email not submitted
            var rr = new ReturnRequest();
            var item = new OrderItem();
            var order = new Order();
            for (int i = 0; i < selected.Length; i++)
            {
                var rrr = _returnRequestService.GetReturnRequestReasonById(reasons[i]);
                var rra = _returnRequestService.GetReturnRequestActionById(actions[i]);
                rr = new ReturnRequest
                {
                    CustomNumber = "",
                    StoreId = InovatiqaDefaults.StoreId,
                    OrderItemId = selected[i],
                    Quantity = quantity[i],
                    CustomerId = customer.Id,
                    ReasonForReturn = rrr != null ? rrr.Name : "not available",
                    RequestedAction = rra != null ? rra.Name : "not available",
                    CustomerComments = "",
                    StaffNotes = string.Empty,
                    ReturnRequestStatusId = (int)ReturnRequestStatus.Pending,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    CustomerReturnRequestId = customerReturnRequestModel.Id
                };
                item = _orderService.GetOrderItemById(selected[i]);
                order = _orderService.GetOrderById(item.OrderId);
                item.Quantity -= quantity[i];
                _orderService.UpdateOrderItem(item);
                _returnRequestService.InsertReturnRequest(rr);
                rr.CustomNumber = _customNumberFormatterService.GenerateReturnRequestCustomNumber(rr);
                _customerService.UpdateCustomer(customer);
                _returnRequestService.UpdateReturnRequest(rr);
            }

            _workflowMessageService.SendNewReturnRequestStoreOwnerNotification(rr, item, order, InovatiqaDefaults.LanguageId);
            _workflowMessageService.SendNewReturnRequestCustomerNotification(rr, item, order);
            model.SelectedItemsPrice = credit;
            var address = _addressService.GetAddressById((int)customer.ShippingAddressId);
            model.User = address.FirstName + " " + address.LastName;
            model.CreatedOn = DateTime.UtcNow;
            model.Email1 = email1;
            model.ReturnNo = customerReturnRequestModel.Id;
            return model;
        }

        public virtual ReturnRequestItemsSelectionModel PrepareCustomerReturnRequestReviewModel(ReturnRequestItemsSelectionModel model, Customer customer, int[] items, int[] quantities, int[] reasons, int[] actions, decimal totalPrice, int shippingLabel, string email1, string email2)
        {
            var Orders = _orderService.GetAllShippedOrdersByCustomer(customer);

            var address = _addressService.GetAddressById((int)customer.ShippingAddressId);
            foreach (var order in Orders)
            {
                var orderItems = _orderService.GetOrderItems(order.Id);
                foreach (var OrderItem in orderItems)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (OrderItem.Id == items[i])
                        {
                            var Product = _productService.GetProductById(OrderItem.ProductId);
                            var shipment = _shipmentService.GetShipmentsByOrderId(order.Id);
                            var attributeValues = _paymentService.DeserializeCustomValues(order);
                            var shipment1 = _shipmentService.GetShipmentById(shipment[0].Id);
                            var Object = new ReturnRequestItemsSelectionModel
                            {
                                PO = "",
                                OrderNo = order.Id,
                                Itemno = OrderItem.Id,
                                Description = Product.Name,
                                UOM = (OrderItem.AttributeDescription),
                                ReturnQuantity = quantities[i],
                                Price = OrderItem.UnitPriceInclTax,
                                ReturnReason = _returnRequestService.GetReturnRequestReasonById(Convert.ToInt32(reasons[i])).Name
                            };
                            foreach (var ShipmentItems in shipment1.Order.OrderItem)
                                if (ShipmentItems.Id == OrderItem.Id)
                                    Object.Invoice = shipment1.Id.ToString();
                            foreach (var keys in attributeValues)
                                if (keys.Key == "PO Number")
                                    Object.PO = keys.Value.ToString();
                            var uom = OrderItem.AttributeDescription.Replace("UOM: ", "").Split(new string[] { "<br />" }, StringSplitOptions.None)[0];
                            Object.UOM = uom;
                            model.Items.Add(Object);
                        }
                    }
                }
            }
            foreach (var item in items)
                model.SelectedItems.Add(item);
            foreach (var quantity in quantities)
                model.SelectedItemsQuantity.Add(quantity);
            foreach (var reason in reasons)
                model.SelectedItemsReturnReason.Add(reason);
            foreach (var action in actions)
                model.SelectedItemsReturnAction.Add(action);
            model.SelectedItemsPrice = totalPrice;
            model.Email1 = email1;
            model.Email2 = email2;
            model.ShippingLabel = Convert.ToInt32(shippingLabel);
            model.Company = address.Company;
            model.AddressLine1 = address.Address1;
            model.City = address.City;
            model.Zip = address.ZipPostalCode;
            return model;
        }

        #endregion
    }
}