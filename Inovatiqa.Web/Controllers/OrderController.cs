using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Payments.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Web.Models.Order;
using Inovatiqa.Services.Catalog.Interfaces;

namespace Inovatiqa.Web.Controllers
{
    public partial class OrderController : BasePublicController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IPdfService _pdfService;
        private readonly IShipmentService _shipmentService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContextService _workContextService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPermissionService _permissionService;
        private readonly ICategoryService _categoryService;



        #endregion

        #region Ctor

        public OrderController(ICustomerService customerService,
            IOrderModelFactory orderModelFactory,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPdfService pdfService,
            IPriceFormatter priceFormatter,
            IShipmentService shipmentService,
            IWebHelper webHelper,
            IWorkContextService workContextService,
            IShoppingCartService shoppingCartService,
            IPermissionService permissionService,
            IRazorViewEngine viewEngine,
            ICategoryService categoryService) : base(viewEngine)
        {
            _customerService = customerService;
            _orderModelFactory = orderModelFactory;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentService = paymentService;
            _pdfService = pdfService;
            _priceFormatter = priceFormatter;
            _shipmentService = shipmentService;
            _webHelper = webHelper;
            _permissionService = permissionService;
            _workContextService = workContextService;
            _shoppingCartService = shoppingCartService;
            _categoryService = categoryService;
        }

        #endregion

        #region Methods

        [RequireHttps]
        public virtual IActionResult CustomerOrderItems(DateTime? startDateValue = null, DateTime? endDateValue = null, int shippedItems = 0 , int filterCategoryId = -1)
        {
            var customer = _workContextService.CurrentCustomer;
            var model = new CustomerOrderItemsListModel();
            if (!_customerService.IsRegistered(customer))
                return Challenge();
            if(customer.ParentId != null && customer.ParentId != 0)
            {
                if(_customerService.IsInCustomerRole(customer, "Subaccount_DAOH"))
                {
                    var siblingCustomerOrderItems = _customerService.getAllChildAccounts(_customerService.GetCustomerById(Convert.ToInt32(customer.ParentId)));
                    foreach(var items in siblingCustomerOrderItems)
                    {
                        //changes by hamza
                        //model = _orderModelFactory.PrepareCustomerOrderItemsListModel(startDateValue, endDateValue, shippedItems, items);
                        model = _orderModelFactory.PrepareCustomerOrderItemsListModel(startDateValue, endDateValue, shippedItems, filterCategoryId, items);

                    }
                }
                else
                {
                    //model = _orderModelFactory.PrepareCustomerOrderItemsListModel(startDateValue, endDateValue, shippedItems);
                    model = _orderModelFactory.PrepareCustomerOrderItemsListModel(startDateValue, endDateValue, shippedItems, filterCategoryId);
                }
            }
            else
            {
                //model = _orderModelFactory.PrepareCustomerOrderItemsListModel(startDateValue, endDateValue, shippedItems);
                model = _orderModelFactory.PrepareCustomerOrderItemsListModel(startDateValue, endDateValue, shippedItems, filterCategoryId);
            }
            var allItems = model.Orders.SelectMany(order => order.Items).ToList().Select(item => item.ProductId).Distinct().ToList();
            List<KeyValuePair<int, string>> AvailableCategories = new List<KeyValuePair<int, string>>();
            foreach(var id in allItems)
            {
                var category = _categoryService.GetProductCategoriesByProductId(id);
                foreach(var cat in category)
                {
                    cat.Category = _categoryService.GetCategoryById(cat.CategoryId);
                    var newCategory = new KeyValuePair<int, string>(cat.Category.Id, cat.Category.Name);
                    if (!AvailableCategories.Contains(newCategory))
                        AvailableCategories.Add(newCategory);
                }
            }
            model.AvailableCatagories = AvailableCategories;
            model.ShippedItemsCount = shippedItems == 25 ? model.Orders.Sum(order => order.Items.Count) : model.Orders.Sum(order =>  order.TotalItems - order.Items.Count);
            model.BackorderItemsCount = shippedItems == 30 ? model.Orders.Sum(order => order.Items.Count) : model.Orders.Sum(order => order.TotalItems - order.Items.Count);
            if(shippedItems == 0)
            {
                model.ShippedItemsCount = model.Orders.SelectMany(order => order.Items).Where(item => item.Shipped == item.Quantity).ToList().Count;
                model.BackorderItemsCount = model.Orders.SelectMany(order => order.Items).Where(item => item.Shipped != item.Quantity).ToList().Count;
            }
            model.ActiveStatusId = shippedItems;
            model.SelectedCategory = filterCategoryId;
            return View(model);
        }

        [RequireHttps]
        public virtual IActionResult CustomerOrders(DateTime? startDateValue = null, DateTime? endDateValue = null, int orderStatusId = 0, bool returnView = false)
        {
            var customer = _workContextService.CurrentCustomer;
            var model = new CustomerOrderListModel();
            if (!_customerService.IsRegistered(customer))
                return Challenge();
             //Implementation of Role - Display all Orders History by hamza
            if (customer.ParentId != null && customer.ParentId != 0)
            {
                if (_customerService.IsInCustomerRole(customer, "Subaccount_DAOH"))
                {
                    var sibingCustomerOrders = _customerService.getAllChildAccounts(_customerService.GetCustomerById(Convert.ToInt32(customer.ParentId)));
                    foreach(var order in sibingCustomerOrders)
                    {
                        model = _orderModelFactory.PrepareCustomerOrderListModel(startDateValue, endDateValue, orderStatusId, returnView, order);
                        model.CurrentActive = orderStatusId;
                        model.IsReturnView = returnView;
                    }
                    return View(model);
                }
                else
                {
                    model = _orderModelFactory.PrepareCustomerOrderListModel(startDateValue, endDateValue, orderStatusId, returnView);
                    model.CurrentActive = orderStatusId;
                    model.IsReturnView = returnView;
                    return View(model);
                }
            }
            // Implementatiuon of Role Done
            model = _orderModelFactory.PrepareCustomerOrderListModel(startDateValue, endDateValue, orderStatusId, returnView);
            model.CurrentActive = orderStatusId;
            model.IsReturnView = returnView;
            return View(model);
        }

        [RequireHttps]
        public virtual IActionResult CustomerInvoicedOrders(DateTime? startDateValue = null, DateTime? endDateValue = null, int invoiceFindByKey = -1, string invoiceFindByKeyValue = null)
        {
            var customer = _workContextService.CurrentCustomer;
            var model = new CustomerOrderListModel();
            if (!_customerService.IsRegistered(customer))
                return Challenge();
            if(customer.ParentId != null && customer.ParentId != 0)
            {
                if(_customerService.IsInCustomerRole(customer, "Subaccount_DAOH"))
                {
                    var siblingCustomerOrderInvoiced = _customerService.getAllChildAccounts(_customerService.GetCustomerById(Convert.ToInt32(customer.ParentId)));
                    foreach(var invoiced in siblingCustomerOrderInvoiced)
                    {
                        model = _orderModelFactory.PrepareCustomerInvoicedOrderListModel(startDateValue, endDateValue, invoiceFindByKey, invoiceFindByKeyValue, invoiced);

                        return View(model);
                    }
                }
            }

            model = _orderModelFactory.PrepareCustomerInvoicedOrderListModel(startDateValue, endDateValue, invoiceFindByKey, invoiceFindByKeyValue);

            return View(model);
        }

        [RequireHttps]
        public virtual IActionResult Details(int orderId, bool trackView = false)
        {
            var customer = _workContextService.CurrentCustomer;
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || customer.Id != order.CustomerId)
                return Challenge();

            var shipments = _shipmentService.GetShipmentsByOrderId(order.Id);
            var model = _orderModelFactory.PrepareOrderDetailsModel(order);
           
            foreach(var shipment in shipments)
            {
                var shipmentModel = _orderModelFactory.PrepareShipmentDetailsModel(shipment);
                model.ShipmentDetails.Add(shipmentModel);
            }
           
            if (trackView)
                model.IsTrackView = true;
            else
                model.IsTrackView = false;

            return View(model);
        }
        [RequireHttps]
        public virtual IActionResult OrderShipments(int orderId, bool trackView = false)
        {
            var customer = _workContextService.CurrentCustomer;

            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || customer.Id != order.CustomerId)
                return Challenge();

            var model = _orderModelFactory.PrepareOrderDetailsModel(order);

            if (trackView)
                model.IsTrackView = true;
            else
                model.IsTrackView = false;

            return View(model);
        }
        [RequireHttps]
        public virtual IActionResult PrintOrderDetails(int orderId)
        {
            var customer = _workContextService.CurrentCustomer;

            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || customer.Id != order.CustomerId)
                return Challenge();

            var model = _orderModelFactory.PrepareOrderDetailsModel(order);
            model.PrintMode = true;

            return View("Details", model);
        }

        public virtual IActionResult GetPdfInvoice(int orderId)
        {
            var customer = _workContextService.CurrentCustomer;

            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || customer.Id != order.CustomerId)
                return Challenge();

            var orders = new List<Order>();
            orders.Add(order);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintOrdersToPdf(stream, orders, InovatiqaDefaults.LanguageId);
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, $"order_{order.Id}.pdf");
        }
        public virtual IActionResult PdfInvoice(int orderId, int shipmentId)
        {
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
        public virtual IActionResult ReOrder(int orderId)
        {
            var customer = _workContextService.CurrentCustomer;

            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || customer.Id != order.CustomerId)
                return Challenge();

            _orderProcessingService.ReOrder(order);
            return RedirectToRoute("ShoppingCart");
        }

        [RequireHttps]
        public virtual IActionResult ShipmentDetails(int shipmentId)
        {
            var customer = _workContextService.CurrentCustomer;

            var shipment = _shipmentService.GetShipmentById(shipmentId);
            if (shipment == null)
                return Challenge();

            var order = _orderService.GetOrderById(shipment.OrderId);

            if (order == null || order.Deleted || customer.Id != order.CustomerId)
                return Challenge();

            var model = _orderModelFactory.PrepareShipmentDetailsModel(shipment);
            return View(model);
        }
        //////[RequireHttps]
        //////public virtual IActionResult PaymentPortal()
        //////{
        //////    //ShipmentSearchModel searchModel = new ShipmentSearchModel();
        //////    var model = _orderModelFactory.PrepareInvoicedShipmentListModel();

        //////    return View(model);
        //////}
        [RequireHttps]
        public virtual IActionResult ReorderGuide(int filterCategories = -1)
        {
            var customer = _workContextService.CurrentCustomer;

            if (!_customerService.IsRegistered(customer))
                return Challenge();
            CustomerReorderGuideModel model = new CustomerReorderGuideModel();
            model.ReorderProducts = _orderModelFactory.PrepareCustomerReOrderGuideModel(filterCategories);
            List<KeyValuePair<int, string>> availableCategory = new List<KeyValuePair<int, string>>();
            foreach(var product in model.ReorderProducts)
            {
                var category = _categoryService.GetProductCategoriesByProductId(product.ProductId);
                foreach(var cat in category)
                {
                    cat.Category = _categoryService.GetCategoryById(cat.CategoryId);
                    var newCategory = new KeyValuePair<int, string>(cat.Category.Id, cat.Category.Name);
                    if (!availableCategory.Contains(newCategory))
                    {
                        availableCategory.Add(newCategory);
                    }
                }
            }
            model.SelectedCategory = filterCategories;
            model.AvailableCatagories = availableCategory;

            return View(model);
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult AddProductToCart_Reorder(int orderId, int orderLineId, int qty, Microsoft.AspNetCore.Http.IFormCollection form)
        {
            var customer = _workContextService.CurrentCustomer;
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || customer.Id != order.CustomerId)
                return Challenge();
            _orderProcessingService.ReOrderOrderedItem(order, orderLineId, qty, form);

            var shoppingCarts = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            var updatetopcartsectionhtml = string.Format(
                            "{0}",
                            shoppingCarts.Count);
            var totalprice = _priceFormatter.FormatPrice(shoppingCarts.Sum(item => _shoppingCartService.GetUnitPrice(item) * item.Quantity));
            //var totalprice = _priceFormatter.FormatPrice(shoppingCarts.Sum(item => item.Quantity * item.Product.Price));
            var updateflyoutcartsectionhtml = InovatiqaDefaults.MiniShoppingCartEnabled
                ? RenderViewComponentToString("FlyoutShoppingCart")
                : string.Empty;

            return Json(new
            {
                success = true,
                message = string.Format("The product has been added to your <a href=\"{0}\">shopping cart</a>",
                                 _customerService.IsRegistered(customer) ? Url.RouteUrl("ShoppingCart") : Url.RouteUrl("GuestCart")),
                updatetopcartsectionhtml,
                updateflyoutcartsectionhtml,
                totalprice
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult AddbulkProductToCart_Reorder(Microsoft.AspNetCore.Http.IFormCollection form)
        {
            List<string> orderIds = form["orderIds"].FirstOrDefault().Split(',').ToList();
            List<string> orderItemIds = form["orderItemIds"].FirstOrDefault().Split(',').ToList();
            List<string> quantities = form["quantities"].FirstOrDefault().Split(',').ToList();
            var customer = _workContextService.CurrentCustomer;
            for (int i = 0; i < orderIds.Count; i++)
            {
                var order = _orderService.GetOrderById(Convert.ToInt32(orderIds[i]));
                if (order == null || order.Deleted || customer.Id != order.CustomerId)
                    return Challenge();
                _orderProcessingService.ReOrderOrderedItem(order, Convert.ToInt32(orderItemIds[i]), Convert.ToInt32(quantities[i]), form);
            }
            var shoppingCarts = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            decimal price = 0; //Need to check
            for (int i = 0; i < shoppingCarts.Count; i++)
            {
                var UnitPrice = shoppingCarts[i].Product.Price;
                var Quantity = shoppingCarts[i].Quantity;
                price += Convert.ToDecimal(UnitPrice * Quantity);
            }
            string totalprice = _priceFormatter.FormatPrice(price);

            var updatetopcartsectionhtml = string.Format(
                            "{0}",
                            shoppingCarts.Count());

            var updateflyoutcartsectionhtml = InovatiqaDefaults.MiniShoppingCartEnabled
                ? RenderViewComponentToString("FlyoutShoppingCart")
                : string.Empty;

            return Json(new
            {
                success = true,
                message = string.Format("The product has been added to your <a href=\"{0}\">shopping cart</a>",
                    _customerService.IsRegistered(customer) ? Url.RouteUrl("ShoppingCart") : Url.RouteUrl("GuestCart")),
                updatetopcartsectionhtml,
                updateflyoutcartsectionhtml,
                totalprice
            });
        }
        #endregion
        public virtual IActionResult ApproveOrder()
        {
            var model = new OrderApprovalCompositeModel();
            model.showApprovalTable = true;
            model.showQueueTable = false;
            var customer = _workContextService.CurrentCustomer;
            if(customer.ParentId != null && customer.ParentId > 0)
            {
                if (_customerService.IsInCustomerRole(customer, "Subaccount_RABCO"))
                {
                    model.showQueueTable = true;
                }
                if (_customerService.IsInCustomerRole(customer, "Subaccount_CAO"))
                {
                    model.showApprovalTable = true;
                }
                else
                {
                    model.showApprovalTable = false;
                }
            }
            model.customerOrdersInQueue = _orderModelFactory.CustomerOrderInQueue();
            model.OrdersWaitingForApproval = _orderModelFactory.OrderWaitingForApproval();
            return View(model);
        }
        public virtual JsonResult RequestApproval()
        {
            var customer = _workContextService.CurrentCustomer;
            var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);
            if(cart.Count == 0)
            {
                return Json(false);
            }
            var OrderApprovalRequest = _shoppingCartService.SaveOrderApprovalRequest(cart, customer);
            // Send Email 
            return Json(true);
        }
        public virtual JsonResult RejectOrder(int ID)
        {
            _orderService.RejectOrder(ID);
            return Json(true);
        }
        public virtual JsonResult MarkAsApproved(int ID)
        {
            

            _shoppingCartService.MarkOrderApproved(ID);
            
            return Json(true); 
        }
        public virtual JsonResult PurchaseOrder(int ID)
        {
            var customer = _workContextService.CurrentCustomer;
             var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart);
             // remove all items from cart
             _shoppingCartService.ResetChildShoppingCart(cart);
             // add approval order items to current cart...
             cart = _shoppingCartService.GetSuspendedShoppingCart(customer, ID);
             //send approval order items to cart
             _shoppingCartService.CopySuspendedItemsToShoppingCartItems(customer, ID);
             // set customer to purchase this cart
             customer.CanPurchaseCart = true;
             _customerService.UpdateCustomer(customer);
            // remove order from list of approved orders...
            _shoppingCartService.DeleteSuspendedShoppingCart(customer, ID);
            return Json(true);
        }
        public virtual IActionResult ScheduledOrders()
        {
            return View();
        }

        public virtual IActionResult InvoicedOrderDetails(int orderId, bool trackView = false)
        {
            var customer = _workContextService.CurrentCustomer;

            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || customer.Id != order.CustomerId)
                return Challenge();

            var model = _orderModelFactory.PrepareOrderDetailsModel(order);

            if (trackView)
                model.IsTrackView = true;
            else
                model.IsTrackView = false;

            return View(model);
        }
       
    }
}