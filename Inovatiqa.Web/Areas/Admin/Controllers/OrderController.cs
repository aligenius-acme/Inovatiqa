using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Orders;
using Inovatiqa.Web.Areas.Admin.Models.Reports;
using Inovatiqa.Web.Controllers;
using Inovatiqa.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Inovatiqa.Web.Extensions;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Web.Mvc.Filters;
using Inovatiqa.Services.Orders;


namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    public partial class OrderController : BaseAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IWorkContextService _workContextService;
        private readonly IProductAttributeParserService _productAttributeParserService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly Web.Factories.Interfaces.IProductModelFactory _productModelFactory;
        private readonly IDateTimeHelperService _dateTimeHelperService;
        private readonly IOrderService _orderService;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IShipmentService _shipmentService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly INotificationService _notificationService;
        private readonly IPdfService _pdfService;
        private readonly IAddressService _addressService;
        private readonly IAddressAttributeParserService _addressAttributeParserService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public OrderController(IPermissionService permissionService,
            IWorkContextService workContextService,
            IProductAttributeParserService productAttributeParserService,
            IShoppingCartService shoppingCartService,
            Web.Factories.Interfaces.IProductModelFactory productModelFactory,
            IDateTimeHelperService dateTimeHelperService,
            IOrderService orderService,
            IOrderModelFactory orderModelFactory,
            IShipmentService shipmentService,
            IWorkflowMessageService workflowMessageService,
            IOrderProcessingService orderProcessingService,
            ICustomerActivityService customerActivityService,
            INotificationService notificationService,
            IPdfService pdfService,
            IAddressService addressService,
            IProductService productService,
            IAddressAttributeParserService addressAttributeParserService,
            ICustomerService customerService,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _permissionService = permissionService;
            _workContextService = workContextService;
            _productAttributeParserService = productAttributeParserService;
            _shoppingCartService = shoppingCartService;
            _productModelFactory = productModelFactory;
            _dateTimeHelperService = dateTimeHelperService;
            _orderService = orderService;
            _orderModelFactory = orderModelFactory;
            _shipmentService = shipmentService;
            _workflowMessageService = workflowMessageService;
            _orderProcessingService = orderProcessingService;
            _customerActivityService = customerActivityService;
            _notificationService = notificationService;
            _pdfService = pdfService;
            _addressService = addressService;
            _addressAttributeParserService = addressAttributeParserService;
            _productService = productService;
            _customerService = customerService;
        }

        #endregion

        #region Utilities

        protected virtual bool HasAccessToProduct(OrderItem orderItem)
        {
            if (orderItem == null || orderItem.ProductId == 0)
                return false;

            var vendor = _workContextService.CurrentVendor;

            if (vendor == null)

                return true;

            var vendorId = vendor.Id;

            return _productService.GetProductById(orderItem.ProductId)?.VendorId == vendorId;
        }

        protected virtual void LogEditOrder(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);

            _customerActivityService.InsertActivity("EditOrder",
                string.Format("Edited an order (Order number = {0}). See order notes for details", order.CustomOrderNumber), order.Id, order.GetType().Name);
        }

        protected virtual bool HasAccessToShipment(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var vendor = _workContextService.CurrentVendor;

            if (vendor == null)
                return true;

            return HasAccessToOrder(shipment.OrderId);
        }

        protected virtual bool HasAccessToOrder(Order order)
        {
            return order != null && HasAccessToOrder(order.Id);
        }

        protected virtual bool HasAccessToOrder(int orderId)
        {
            if (orderId == 0)
                return false;

            var vendor = _workContextService.CurrentVendor;

            if (vendor == null)
                return true;

            var vendorId = vendor.Id;
            var hasVendorProducts = _orderService.GetOrderItems(orderId, vendorId: vendorId).Any();

            return hasVendorProducts;
        }

        #endregion

        #region Order list

        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-order-by-number")]
        public virtual IActionResult GoToOrderId(OrderSearchModel model)
        {
            var order = _orderService.GetOrderByCustomOrderNumber(model.GoDirectlyToCustomOrderNumber);

            if (order == null)
                return List();

            return RedirectToAction("Edit", "Order", new { id = order.Id });
        }

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List(List<int> orderStatuses = null, List<int> paymentStatuses = null, List<int> shippingStatuses = null)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = _orderModelFactory.PrepareOrderSearchModel(new OrderSearchModel
            {
                OrderStatusIds = orderStatuses,
                PaymentStatusIds = paymentStatuses,
                ShippingStatusIds = shippingStatuses
            });

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult ReportAggregates(OrderSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            var model = _orderModelFactory.PrepareOrderAggregatorModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult OrderList(OrderSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            var model = _orderModelFactory.PrepareOrderListModel(searchModel);

            return Json(model);
        }

        #endregion

        #region Export / Import



        #endregion

        #region Order details

        #region Payments and other order workflow

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("captureorder")]
        public virtual IActionResult CaptureOrder(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;
            if (vendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                var errors = _orderProcessingService.Capture(order);
                LogEditOrder(order.Id);

                var model = _orderModelFactory.PrepareOrderModel(null, order);

                foreach (var error in errors)
                    _notificationService.ErrorNotification(error);

                return View(model);
            }
            catch (Exception exc)
            {
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        public virtual IActionResult PartiallyRefundOrderPopup(int id, bool online)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            var model = _orderModelFactory.PrepareOrderModel(null, order);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("partialrefundorder")]
        public virtual IActionResult PartiallyRefundOrderPopup(int id, bool online, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                var amountToRefund = model.AmountToRefund;
                if (amountToRefund <= decimal.Zero)
                    throw new InovatiqaException("Enter amount to refund");

                var maxAmountToRefund = order.OrderTotal - order.RefundedAmount;
                if (amountToRefund > maxAmountToRefund)
                    amountToRefund = maxAmountToRefund;

                var errors = new List<string>();
                if (online)
                    errors = _orderProcessingService.PartiallyRefund(order, amountToRefund).ToList();
                else
                    _orderProcessingService.PartiallyRefundOffline(order, amountToRefund);

                LogEditOrder(order.Id);

                if (!errors.Any())
                {
                    ViewBag.RefreshPage = true;

                    model = _orderModelFactory.PrepareOrderModel(model, order);

                    return View(model);
                }

                model = _orderModelFactory.PrepareOrderModel(model, order);

                foreach (var error in errors)
                    _notificationService.ErrorNotification(error);

                return View(model);
            }
            catch (Exception exc)
            {
                model = _orderModelFactory.PrepareOrderModel(model, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundorder")]
        public virtual IActionResult RefundOrder(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                var errors = _orderProcessingService.Refund(order);
                LogEditOrder(order.Id);

                var model = _orderModelFactory.PrepareOrderModel(null, order);

                foreach (var error in errors)
                    _notificationService.ErrorNotification(error);

                return View(model);
            }
            catch (Exception exc)
            {
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundorderoffline")]
        public virtual IActionResult RefundOrderOffline(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                _orderProcessingService.RefundOffline(order);
                LogEditOrder(order.Id);

                var model = _orderModelFactory.PrepareOrderModel(null, order);

                return View(model);
            }
            catch (Exception exc)
            {
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markorderaspaid")]
        public virtual IActionResult MarkOrderAsPaid(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                _orderProcessingService.MarkOrderAsPaid(order);
                LogEditOrder(order.Id);

                var model = _orderModelFactory.PrepareOrderModel(null, order);

                return View(model);
            }
            catch (Exception exc)
            {
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("cancelorder")]
        public virtual IActionResult CancelOrder(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                _orderProcessingService.CancelOrder(order, true);
                LogEditOrder(order.Id);

                var model = _orderModelFactory.PrepareOrderModel(null, order);

                return View(model);
            }
            catch (Exception exc)
            {
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveOrderStatus")]
        public virtual IActionResult ChangeOrderStatus(int id, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                order.OrderStatusId = model.OrderStatusId;
                _orderService.UpdateOrder(order);

                _orderService.InsertOrderNote(new OrderNote
                {
                    OrderId = order.Id,
                    Note = $"Order status has been edited. New status: {Enum.GetName(typeof(OrderStatus), order.OrderStatusId)}",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                LogEditOrder(order.Id);

                model = _orderModelFactory.PrepareOrderModel(model, order);

                return View(model);
            }
            catch (Exception exc)
            {
                model = _orderModelFactory.PrepareOrderModel(model, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("voidorder")]
        public virtual IActionResult VoidOrder(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                var errors = _orderProcessingService.Void(order);
                LogEditOrder(order.Id);

                var model = _orderModelFactory.PrepareOrderModel(null, order);

                foreach (var error in errors)
                    _notificationService.ErrorNotification(error);

                return View(model);
            }
            catch (Exception exc)
            {
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        #endregion

        #region Edit, delete

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("save-shipping-method")]
        public virtual IActionResult EditShippingMethod(int id, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;


            if (vendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            order.ShippingMethod = model.ShippingMethod;
            _orderService.UpdateOrder(order);

            //add a note
            _orderService.InsertOrderNote(new OrderNote
            {
                OrderId = order.Id,
                Note = "Shipping method has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            LogEditOrder(order.Id);

            model = _orderModelFactory.PrepareOrderModel(model, order);

            SaveSelectedPanelName("order-billing-shipping", persistForTheNextRequest: false);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnSaveOrderItem")]
        public virtual IActionResult EditOrderItem(int id, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;


            if (vendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            //get order item identifier
            var orderItemId = 0;
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnSaveOrderItem", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnSaveOrderItem".Length));
            
            var orderItem = _orderService.GetOrderItemById(orderItemId)
                ?? throw new ArgumentException("No order item found with the specified id");

            if (!decimal.TryParse(form["pvUnitPriceInclTax" + orderItemId], out var unitPriceInclTax))
                unitPriceInclTax = orderItem.UnitPriceInclTax;
            if (!decimal.TryParse(form["pvUnitPriceExclTax" + orderItemId], out var unitPriceExclTax))
                unitPriceExclTax = orderItem.UnitPriceExclTax;
            if (!int.TryParse(form["pvQuantity" + orderItemId], out var quantity))
                quantity = orderItem.Quantity;
            if (!decimal.TryParse(form["pvDiscountInclTax" + orderItemId], out var discountInclTax))
                discountInclTax = orderItem.DiscountAmountInclTax;
            if (!decimal.TryParse(form["pvDiscountExclTax" + orderItemId], out var discountExclTax))
                discountExclTax = orderItem.DiscountAmountExclTax;
          
            /*if (!decimal.TryParse(form["pvPriceInclTax" + orderItemId], out var priceInclTax))
                priceInclTax = orderItem.PriceInclTax;
            if (!decimal.TryParse(form["pvPriceExclTax" + orderItemId], out var priceExclTax))
                priceExclTax = orderItem.PriceExclTax;
            */
            //increase in quantity not change in total price isuue by hamza
            var product = _productService.GetProductById(orderItem.ProductId);
            
            if (quantity > 0)
            {
                var qtyDifference = orderItem.Quantity - quantity;

                orderItem.UnitPriceInclTax = unitPriceInclTax;
                orderItem.UnitPriceExclTax = unitPriceExclTax;
                orderItem.Quantity = quantity;
                orderItem.DiscountAmountInclTax = discountInclTax;
                orderItem.DiscountAmountExclTax = discountExclTax;
                //change
                orderItem.PriceInclTax = unitPriceInclTax * quantity;
                orderItem.PriceExclTax = unitPriceExclTax * quantity;
                //increase in quantity not change in total price isuue by hamza
                //orderItem.PriceInclTax = priceInclTax;
                //orderItem.PriceExclTax = priceExclTax;
                _orderService.UpdateOrderItem(orderItem);

                _productService.AdjustInventory(product, qtyDifference, orderItem.AttributesXml,
                    string.Format("The stock quantity has been changed by editing the order #{0}", order.Id));
            }
            else
            {
                _productService.AdjustInventory(product, orderItem.Quantity, orderItem.AttributesXml,
                    string.Format("The stock quantity has been increased by deleting an order item from the order #{0}", order.Id));

                _orderService.DeleteOrderItem(orderItem);
            }

            var updateOrderParameters = new UpdateOrderParameters(order, orderItem)
            {
                PriceInclTax = unitPriceInclTax,
                PriceExclTax = unitPriceExclTax,
                DiscountAmountInclTax = discountInclTax,
                DiscountAmountExclTax = discountExclTax,
                SubTotalInclTax = orderItem.PriceInclTax,
                SubTotalExclTax = orderItem.PriceExclTax,
                // SubTotalInclTax = PriceInclTax,
                //SubTotalExclTax = PriceExclTax,
                //increase in quantity not change in total price isuue by hamza
                Quantity = quantity
            };
            _orderProcessingService.UpdateOrderTotals(updateOrderParameters);

            //add a note
            _orderService.InsertOrderNote(new OrderNote
            {
                OrderId = order.Id,
                Note = "Order item has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            LogEditOrder(order.Id);

            //prepare model
            var model = _orderModelFactory.PrepareOrderModel(null, order);

            foreach (var warning in updateOrderParameters.Warnings)
                _notificationService.WarningNotification(warning);

            //selected panel
            SaveSelectedPanelName("order-products", persistForTheNextRequest: false);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveOrderTotals")]
        public virtual IActionResult EditOrderTotals(int id, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            order.OrderSubtotalInclTax = model.OrderSubtotalInclTaxValue;
            order.OrderSubtotalExclTax = model.OrderSubtotalExclTaxValue;
            order.OrderSubTotalDiscountInclTax = model.OrderSubTotalDiscountInclTaxValue;
            order.OrderSubTotalDiscountExclTax = model.OrderSubTotalDiscountExclTaxValue;
            order.OrderShippingInclTax = model.OrderShippingInclTaxValue;
            order.OrderShippingExclTax = model.OrderShippingExclTaxValue;
            order.PaymentMethodAdditionalFeeInclTax = model.PaymentMethodAdditionalFeeInclTaxValue;
            order.PaymentMethodAdditionalFeeExclTax = model.PaymentMethodAdditionalFeeExclTaxValue;
            order.TaxRates = model.TaxRatesValue;
            order.OrderTax = model.TaxValue;
            order.OrderDiscount = model.OrderTotalDiscountValue;
            order.OrderTotal = model.OrderTotalValue;
            _orderService.UpdateOrder(order);

            //add a note
            _orderService.InsertOrderNote(new OrderNote
            {
                OrderId = order.Id,
                Note = "Order totals have been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            LogEditOrder(order.Id);

            //prepare model
            model = _orderModelFactory.PrepareOrderModel(model, order);

            return View(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && !HasAccessToOrder(order))
                return RedirectToAction("List");

            var model = _orderModelFactory.PrepareOrderModel(null, order);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            _orderProcessingService.DeleteOrder(order);

            _customerActivityService.InsertActivity("DeleteOrder",
                string.Format("Deleted an order (ID = {0})", order.Id), order.Id, order.GetType().Name);

            return RedirectToAction("List");
        }

        public virtual IActionResult PdfInvoice(int orderId, int shipmentId = 0)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var vendorId = 0;

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
            {
                vendorId = vendor.Id;
            }

            var order = _orderService.GetOrderById(orderId);
            //////////////////var orderCustomer = _customerService.GetCustomerById(order.CustomerId);
            //////////////////if (_customerService.IsB2B(orderCustomer) == false)
            //////////////////    shipmentId = 0;
            var orders = new List<Order>
            {
                order
            };

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintOrdersToPdf(stream, orders, InovatiqaDefaults.LanguageId, vendorId, shipmentId);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, $"order_{order.Id}.pdf");
        }
        public virtual IActionResult DeleteProductToOrder(int orderId, int itemId)
        {
            var order = _orderService.GetOrderById(orderId);
            var item = _orderService.GetOrderItemById(itemId);
            var product = _productService.GetProductById(item.ProductId);
            //item.Quantity = -item.Quantity;
            _productService.AdjustInventory(product, item.Quantity, item.AttributesXml,
                    string.Format("The stock quantity has been changed by editing the order #{0}", orderId));
            order.OrderItem.Remove(item);
            _orderService.UpdateOrder(order);
            var updateOrderParameters = new UpdateOrderParameters(order, item)
            {
                PriceInclTax = item.UnitPriceInclTax,
                PriceExclTax = item.UnitPriceExclTax,
                DiscountAmountInclTax = item.DiscountAmountInclTax,
                DiscountAmountExclTax = item.DiscountAmountExclTax,
                SubTotalInclTax = item.PriceInclTax,
                SubTotalExclTax = item.PriceExclTax,
                // SubTotalInclTax = PriceInclTax,
                //SubTotalExclTax = PriceExclTax,
                //increase in quantity not change in total price isuue by hamza
                Quantity = item.Quantity
            };
            _orderProcessingService.UpdateOrderTotals(updateOrderParameters);
            return Json(new
            {
                success = true
            });
        }
        public virtual IActionResult AddProductToOrder(int orderId, int Sku, int Qyt, IFormCollection form = null)
        {
            if (Sku == 0)
            {
                return Json(new
                {
                    success = false
                });
            }
            if (Qyt == 0)
            {
                Qyt = 1;
            }
            var product = _productService.GetProductByMSku(Sku);
            var order = _orderService.GetOrderById(orderId);
            var model = _productModelFactory.PrepareProductDetailsModel(product);
            if (product == null)
            {
                return Json(new
                {
                    success = false
                });
            }
            var addToCartWarnings = new List<string>();
            var price = model.ProductPrice.Price.Replace("$", "").Replace(" ", "").Replace(",", "");
            var fullprice = Convert.ToDecimal(price) * Qyt;
            var attributes = _productAttributeParserService.ParseProductAttributes(product, form, addToCartWarnings);
            var attr = _productAttributeParserService.ParseProductAttributeValues(attributes);
            var AttrDescription = String.Empty;
            foreach(var attribute in attr)
            {
                if (String.IsNullOrEmpty(AttrDescription))
                {
                    AttrDescription = String.Format("{0}: {1}", attribute.ProductAttributeMapping.ProductAttribute.Name, attribute.Name);
                }
                else
                {
                    AttrDescription = String.Format("{0}<br />{1}: {2}", AttrDescription, attribute.ProductAttributeMapping.ProductAttribute.Name, attribute.Name);
                }
            }
            Guid guid = Guid.NewGuid();
            var item = new OrderItem{
                OrderId = orderId,
                ProductId = product.Id,
                OrderItemGuid = guid,
                Quantity = Qyt,
                AttributesXml = attributes,
                UnitPriceExclTax = Convert.ToDecimal(price),
                UnitPriceInclTax = Convert.ToDecimal(price),
                PriceInclTax = fullprice,
                PriceExclTax = fullprice,
                AttributeDescription = AttrDescription
            };
            
            order.OrderItem.Add(item);
            _orderService.UpdateOrder(order);
            var qty = -Qyt;
            _productService.AdjustInventory(product, qty, item.AttributesXml,
                    string.Format("The stock quantity has been changed by editing the order #{0}", orderId));
            var updateOrderParameters = new UpdateOrderParameters(order, item)
            {
                PriceInclTax = item.UnitPriceInclTax,
                PriceExclTax = item.UnitPriceExclTax,
                DiscountAmountInclTax = item.DiscountAmountInclTax,
                DiscountAmountExclTax = item.DiscountAmountExclTax,
                SubTotalInclTax = item.PriceInclTax,
                SubTotalExclTax = item.PriceExclTax,
                // SubTotalInclTax = PriceInclTax,
                //SubTotalExclTax = PriceExclTax,
                //increase in quantity not change in total price isuue by hamza
                Quantity = Qyt
            };
            _orderProcessingService.UpdateOrderTotals(updateOrderParameters);
            return Json(new
            {
                success = true
            });
        }

        #endregion

        #endregion

        #region Addresses

        public virtual IActionResult AddressEdit(int addressId, int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;
            if (vendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            var address = _addressService.GetAddressById(addressId)
                ?? throw new ArgumentException("No address found with the specified id", nameof(addressId));

            var model = _orderModelFactory.PrepareOrderAddressModel(new OrderAddressModel(), order, address);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult AddressEdit(OrderAddressModel model, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;
            if (vendor != null)
                return RedirectToAction("Edit", "Order", new { id = order.Id });

            var address = _addressService.GetAddressById(model.Address.Id)
                ?? throw new ArgumentException("No address found with the specified id");

            var customAttributes = _addressAttributeParserService.ParseCustomAddressAttributes(form);
            var customAttributeWarnings = _addressAttributeParserService.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            if (ModelState.IsValid)
            {
                address = model.Address.ToAdminAddressEntity(address);
                address.CustomAttributes = customAttributes;
                _addressService.UpdateAddress(address);

                _orderService.InsertOrderNote(new OrderNote
                {
                    OrderId = order.Id,
                    Note = "Address has been edited",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                LogEditOrder(order.Id);

                return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, orderId = model.OrderId });
            }

            model = _orderModelFactory.PrepareOrderAddressModel(model, order, address);

            return View(model);
        }

        #endregion

        #region Invoices

        [HttpPost]
        public virtual IActionResult InvoicedShipmentListSelect(ShipmentSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            var model = _orderModelFactory.PrepareInvoicedShipmentListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult InvoicedShipmentList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = _orderModelFactory.PrepareInvoicedShipmentSearchModel(new ShipmentSearchModel());

            return View(model);
        }

        #endregion

        #region Shipments

        [HttpPost]
        public virtual IActionResult ShipmentListSelect(ShipmentSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            var model = _orderModelFactory.PrepareShipmentListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult ShipmentList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = _orderModelFactory.PrepareShipmentSearchModel(new ShipmentSearchModel());

            return View(model);
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setasshipped")]
        public virtual IActionResult SetAsShipped(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            try
            {
                _orderProcessingService.Ship(shipment, true);
                LogEditOrder(shipment.OrderId);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("saveshippeddate")]
        public virtual IActionResult EditShippedDate(ShipmentModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            try
            {
                if (!model.ShippedDateUtc.HasValue)
                {
                    throw new Exception("Enter shipped date");
                }

                shipment.ShippedDateUtc = model.ShippedDateUtc;
                _shipmentService.UpdateShipment(shipment);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setasdelivered")]
        public virtual IActionResult SetAsDelivered(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            try
            {
                _orderProcessingService.Deliver(shipment, true);
                LogEditOrder(shipment.OrderId);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("savedeliverydate")]
        public virtual IActionResult EditDeliveryDate(ShipmentModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            try
            {
                if (!model.DeliveryDateUtc.HasValue)
                {
                    throw new Exception("Enter delivery date");
                }

                shipment.DeliveryDateUtc = model.DeliveryDateUtc;
                _shipmentService.UpdateShipment(shipment);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        public virtual IActionResult PdfPackagingSlip(int shipmentId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(shipmentId);
            if (shipment == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            var shipments = new List<Shipment>
            {
                shipment
            };

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintPackagingSlipsToPdf(stream, shipments, InovatiqaDefaults.LanguageId);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, $"packagingslip_{shipment.Id}.pdf");
        }

        [HttpPost, ActionName("ShipmentList")]
        [FormValueRequired("exportpackagingslips-all")]
        public virtual IActionResult PdfPackagingSlipAll(ShipmentSearchModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var startDateValue = model.StartDate == null ? null
                            : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelperService.CurrentTimeZone);

            var endDateValue = model.EndDate == null ? null
                            : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelperService.CurrentTimeZone).AddDays(1);

            var vendorId = 0;

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                vendorId = vendor.Id;

            var shipments = _shipmentService.GetAllShipments(vendorId: vendorId,
                warehouseId: model.WarehouseId,
                shippingCountryId: model.CountryId,
                shippingStateId: model.StateProvinceId,
                shippingCounty: model.County,
                shippingCity: model.City,
                trackingNumber: model.TrackingNumber,
                loadNotShipped: model.LoadNotShipped,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue);

            if (!shipments.Any())
            {
                _notificationService.ErrorNotification("No shipments selected");
                return RedirectToAction("ShipmentList");
            }

            try
            {
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintPackagingSlipsToPdf(stream, shipments, InovatiqaDefaults.LanguageId);
                    bytes = stream.ToArray();
                }

                return File(bytes, MimeTypes.ApplicationPdf, "packagingslips.pdf");
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("ShipmentList");
            }
        }

        [HttpPost]
        public virtual IActionResult PdfPackagingSlipSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipments = new List<Shipment>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                shipments.AddRange(_shipmentService.GetShipmentsByIds(ids));
            }
            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
            {
                shipments = shipments.Where(HasAccessToShipment).ToList();
            }

            try
            {
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintPackagingSlipsToPdf(stream, shipments, InovatiqaDefaults.LanguageId);
                    bytes = stream.ToArray();
                }

                return File(bytes, MimeTypes.ApplicationPdf, "packagingslips.pdf");
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("ShipmentList");
            }
        }

        [HttpPost]
        public virtual IActionResult SetAsShippedSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipments = new List<Shipment>();
            if (selectedIds != null)
            {
                shipments.AddRange(_shipmentService.GetShipmentsByIds(selectedIds.ToArray()));
            }
            var vendor = _workContextService.CurrentVendor;
            if (vendor != null)
            {
                shipments = shipments.Where(HasAccessToShipment).ToList();
            }

            foreach (var shipment in shipments)
            {
                try
                {
                    _orderProcessingService.Ship(shipment, true);
                }
                catch
                {
                    //ignore any exception
                }
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual IActionResult SetAsDeliveredSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipments = new List<Shipment>();
            if (selectedIds != null)
            {
                shipments.AddRange(_shipmentService.GetShipmentsByIds(selectedIds.ToArray()));
            }
            var vendor = _workContextService.CurrentVendor;
            if (vendor != null)
            {
                shipments = shipments.Where(HasAccessToShipment).ToList();
            }

            foreach (var shipment in shipments)
            {
                try
                {
                    _orderProcessingService.Deliver(shipment, true);
                }
                catch
                {
                    //ignore any exception
                }
            }

            return Json(new { Result = true });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setadmincomment")]
        public virtual IActionResult SetShipmentAdminComment(ShipmentModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            shipment.AdminComment = model.AdminComment;
            _shipmentService.UpdateShipment(shipment);

            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }

        public virtual IActionResult AddShipment(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && !HasAccessToOrder(order))
                return RedirectToAction("List");

            var model = _orderModelFactory.PrepareShipmentModel(new ShipmentModel(), null, order);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual IActionResult AddShipment(ShipmentModel model, IFormCollection form, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && !HasAccessToOrder(order))
                return RedirectToAction("List");

            var orderItems = _orderService.GetOrderItems(order.Id, isShipEnabled: true);

            if (vendor != null)
            {
                orderItems = orderItems.Where(HasAccessToProduct).ToList();
            }

            var customer = _customerService.GetCustomerById(order.CustomerId);
            var paymentTermsId = customer.PaymentTermsId;
            var invoiceDueDateUtc = paymentTermsId == null ? DateTime.UtcNow : DateTime.UtcNow.AddDays(double.Parse(paymentTermsId.ToString()));

            var shipment = new Shipment
            {
                OrderId = order.Id,
                TrackingNumber = model.TrackingNumber,
                TotalWeight = null,
                AdminComment = model.AdminComment,
                CreatedOnUtc = DateTime.UtcNow,
                PaymentStatusId = (int)PaymentStatus.Pending,
                InvoiceDueDateUtc = invoiceDueDateUtc
            };

            if (order.PaymentStatusId == (int)PaymentStatus.Paid)
                shipment.PaymentStatusId = (int)PaymentStatus.Paid;

            var shipmentItems = new List<ShipmentItem>();

            decimal? totalWeight = null;
            decimal? totalAmount = null;

            foreach (var orderItem in orderItems)
            {
                var product = _productService.GetProductById(orderItem.ProductId);


                var maxQtyToAdd = _orderService.GetTotalNumberOfItemsCanBeAddedToShipment(orderItem);
                if (maxQtyToAdd <= 0)
                    continue;

                var qtyToAdd = 0; //parse quantity
                foreach (var formKey in form.Keys)
                    if (formKey.Equals($"qtyToAdd{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(form[formKey], out qtyToAdd);
                        break;
                    }

                var warehouseId = 0;
                if (product.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock &&
                    product.UseMultipleWarehouses)
                {
                    foreach (var formKey in form.Keys)
                        if (formKey.Equals($"warehouse_{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int.TryParse(form[formKey], out warehouseId);
                            break;
                        }
                }
                else
                {
                    warehouseId = product.WarehouseId;
                }


                if (qtyToAdd <= 0)
                    continue;
                if (qtyToAdd > maxQtyToAdd)
                    qtyToAdd = maxQtyToAdd;


                var orderItemTotalWeight = orderItem.ItemWeight * qtyToAdd;
                if (orderItemTotalWeight.HasValue)
                {
                    if (!totalWeight.HasValue)
                        totalWeight = 0;
                    totalWeight += orderItemTotalWeight.Value;
                }

                var orderItemTotalAmount = orderItem.UnitPriceInclTax * qtyToAdd;
                if (!totalAmount.HasValue)
                    totalAmount = 0;
                totalAmount += orderItemTotalAmount;

                shipmentItems.Add(new ShipmentItem
                {
                    OrderItemId = orderItem.Id,
                    Quantity = qtyToAdd,
                    WarehouseId = warehouseId
                });

                var quantityWithReserved = _productService.GetTotalStockQuantity(product, true, warehouseId);
                var quantityTotal = _productService.GetTotalStockQuantity(product, false, warehouseId);

                var quantityReserved = quantityTotal - quantityWithReserved;

                if (!(quantityReserved == qtyToAdd && quantityReserved == maxQtyToAdd))
                    _productService.BalanceInventory(product, warehouseId, qtyToAdd);
            }

            if (shipmentItems.Any())
            {
                shipment.TotalWeight = totalWeight;
                shipment.TotalAmount = totalAmount;

                if(_customerService.IsB2B(customer) == false)
                {
                    shipment.AmountPaid = shipment.TotalAmount;
                    shipment.InvoiceDueDateUtc = shipment.CreatedOnUtc;
                    shipment.InvoicePayedDateUtc = shipment.CreatedOnUtc;
                }

                _shipmentService.InsertShipment(shipment);

                foreach (var shipmentItem in shipmentItems)
                {
                    shipmentItem.ShipmentId = shipment.Id;
                    _shipmentService.InsertShipmentItem(shipmentItem);
                }

                _orderService.InsertOrderNote(new OrderNote
                {
                    OrderId = order.Id,
                    Note = "A shipment has been added",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                if (model.CanShip)
                    _orderProcessingService.Ship(shipment, true);

                if (model.CanShip && model.CanDeliver)
                    _orderProcessingService.Deliver(shipment, true);

                LogEditOrder(order.Id);

                _notificationService.SuccessNotification("The new shipment has been added successfully.");
                return continueEditing
                        ? RedirectToAction("ShipmentDetails", new { id = shipment.Id })
                        : RedirectToAction("Edit", new { id = model.OrderId });
            }

            _notificationService.ErrorNotification("No products selected");
            
            return RedirectToAction("AddShipment", model);
        }

        [HttpPost]
        public virtual IActionResult ShipmentsItemsByShipmentId(ShipmentItemSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            var shipment = _shipmentService.GetShipmentById(searchModel.ShipmentId)
                ?? throw new ArgumentException("No shipment found with the specified id");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && !HasAccessToShipment(shipment))
                return Content(string.Empty);

            var order = _orderService.GetOrderById(shipment.OrderId)
                ?? throw new ArgumentException("No order found with the specified id");

            if (vendor != null && !HasAccessToOrder(order))
                return Content(string.Empty);

            searchModel.SetGridPageSize();
            var model = _orderModelFactory.PrepareShipmentItemListModel(searchModel, shipment);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult ShipmentsByOrder(OrderShipmentSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            var order = _orderService.GetOrderById(searchModel.OrderId)
                ?? throw new ArgumentException("No order found with the specified id");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && !HasAccessToOrder(order))
                return Content(string.Empty);

            var model = _orderModelFactory.PrepareOrderShipmentListModel(searchModel, order);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult DeleteShipment(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            foreach (var shipmentItem in _shipmentService.GetShipmentItemsByShipmentId(shipment.Id))
            {
                var orderItem = _orderService.GetOrderItemById(shipmentItem.OrderItemId);
                if (orderItem == null)
                    continue;

                var product = _productService.GetProductById(orderItem.ProductId);

                _productService.ReverseBookedInventory(product, shipmentItem,
                    string.Format("The stock quantity has been increased by deleting a shipment from the order #{0}", shipment.OrderId));
            }

            var orderId = shipment.OrderId;
            _shipmentService.DeleteShipment(shipment);

            var order = _orderService.GetOrderById(orderId);
            _orderService.InsertOrderNote(new OrderNote
            {
                OrderId = order.Id,
                Note = "A shipment has been deleted",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            LogEditOrder(order.Id);

            _notificationService.SuccessNotification("The shipment has been deleted successfully.");
            return RedirectToAction("Edit", new { id = orderId });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("settrackingnumber")]
        public virtual IActionResult SetTrackingNumber(ShipmentModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;
            if (vendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            shipment.TrackingNumber = model.TrackingNumber;
            _shipmentService.UpdateShipment(shipment);

            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }

        public virtual IActionResult ShipmentDetails(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                return RedirectToAction("List");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            var model = _orderModelFactory.PrepareShipmentModel(null, shipment, null);

            return View(model);
        }

        #endregion

        #region Order notes

        [HttpPost]
        public virtual IActionResult OrderNotesSelect(OrderNoteSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            var order = _orderService.GetOrderById(searchModel.OrderId)
                ?? throw new ArgumentException("No order found with the specified id");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                return Content(string.Empty);

            var model = _orderModelFactory.PrepareOrderNoteListModel(searchModel, order);

            return Json(model);
        }

        public virtual IActionResult OrderNoteAdd(int orderId, int downloadId, bool displayToCustomer, string message)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(message))
                return ErrorJson("Order note can not be empty.");

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return ErrorJson("Order cannot be loaded");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                return ErrorJson("No access for vendors");

            var orderNote = new OrderNote
            {
                OrderId = order.Id,
                DisplayToCustomer = displayToCustomer,
                Note = message,
                DownloadId = downloadId,
                CreatedOnUtc = DateTime.UtcNow
            };

            _orderService.InsertOrderNote(orderNote);

            if (displayToCustomer)
            {
                _workflowMessageService.SendNewOrderNoteAddedCustomerNotification(orderNote, InovatiqaDefaults.LanguageId);
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual IActionResult OrderNoteDelete(int id, int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            _ = _orderService.GetOrderById(orderId)
                ?? throw new ArgumentException("No order found with the specified id");

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            var orderNote = _orderService.GetOrderNoteById(id)
                ?? throw new ArgumentException("No order note found with the specified id");

            _orderService.DeleteOrderNote(orderNote);

            return new NullJsonResult();
        }

        #endregion

        #region Reports

        [HttpPost]
        public virtual IActionResult OrderIncompleteReportList(OrderIncompleteReportSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                return Content(string.Empty);

            var model = _orderModelFactory.PrepareOrderIncompleteReportListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult BestsellersBriefReportByAmountList(BestsellerBriefSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            var model = _orderModelFactory.PrepareBestsellerBriefListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult BestsellersBriefReportByQuantityList(BestsellerBriefSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            var model = _orderModelFactory.PrepareBestsellerBriefListModel(searchModel);

            return Json(model);
        }
        public virtual IActionResult OrderAverageReportList(OrderAverageReportSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                return Content(string.Empty);

            var model = _orderModelFactory.PrepareOrderAverageReportListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult LoadOrderStatistics(string period)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content(string.Empty);

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                return Content(string.Empty);

            var result = new List<object>();

            var nowDt = _dateTimeHelperService.ConvertToUserTime(DateTime.Now);
            var timeZone = _dateTimeHelperService.CurrentTimeZone;

            var culture = new CultureInfo(InovatiqaDefaults.LanguageCulture);

            switch (period)
            {
                case "year":
                    var yearAgoDt = nowDt.AddYears(-1).AddMonths(1);
                    var searchYearDateUser = new DateTime(yearAgoDt.Year, yearAgoDt.Month, 1);
                    for (var i = 0; i <= 12; i++)
                    {
                        result.Add(new
                        {
                            date = searchYearDateUser.Date.ToString("Y", culture),
                            value = _orderService.SearchOrders(
                                createdFromUtc: _dateTimeHelperService.ConvertToUtcTime(searchYearDateUser, timeZone),
                                createdToUtc: _dateTimeHelperService.ConvertToUtcTime(searchYearDateUser.AddMonths(1), timeZone),
                                pageIndex: 0,
                                pageSize: 1, getOnlyTotalCount: true).TotalCount.ToString()
                        });

                        searchYearDateUser = searchYearDateUser.AddMonths(1);
                    }

                    break;
                case "month":
                    //month statistics
                    var monthAgoDt = nowDt.AddDays(-30);
                    var searchMonthDateUser = new DateTime(monthAgoDt.Year, monthAgoDt.Month, monthAgoDt.Day);
                    for (var i = 0; i <= 30; i++)
                    {
                        result.Add(new
                        {
                            date = searchMonthDateUser.Date.ToString("M", culture),
                            value = _orderService.SearchOrders(
                                createdFromUtc: _dateTimeHelperService.ConvertToUtcTime(searchMonthDateUser, timeZone),
                                createdToUtc: _dateTimeHelperService.ConvertToUtcTime(searchMonthDateUser.AddDays(1), timeZone),
                                pageIndex: 0,
                                pageSize: 1, getOnlyTotalCount: true).TotalCount.ToString()
                        });

                        searchMonthDateUser = searchMonthDateUser.AddDays(1);
                    }

                    break;
                case "week":
                default:
                    //week statistics
                    var weekAgoDt = nowDt.AddDays(-7);
                    var searchWeekDateUser = new DateTime(weekAgoDt.Year, weekAgoDt.Month, weekAgoDt.Day);
                    for (var i = 0; i <= 7; i++)
                    {
                        result.Add(new
                        {
                            date = searchWeekDateUser.Date.ToString("d dddd", culture),
                            value = _orderService.SearchOrders(
                                createdFromUtc: _dateTimeHelperService.ConvertToUtcTime(searchWeekDateUser, timeZone),
                                createdToUtc: _dateTimeHelperService.ConvertToUtcTime(searchWeekDateUser.AddDays(1), timeZone),
                                pageIndex: 0,
                                pageSize: 1, getOnlyTotalCount: true).TotalCount.ToString()
                        });

                        searchWeekDateUser = searchWeekDateUser.AddDays(1);
                    }

                    break;
            }

            return Json(result);
        }

        #endregion
    }
}
