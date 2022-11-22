using System;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Payments.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Order;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Controllers
{
    [AutoValidateAntiforgeryToken]
    public partial class ReturnRequestController : BasePublicController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ICustomNumberFormatterService _customNumberFormatterService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IReturnRequestModelFactory _returnRequestModelFactory;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IWorkContextService _workContextService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IShipmentService _shipmentService;
        private readonly IProductAttributeParserService _productAttributeParserService;
        private readonly IPaymentService _paymentService;
        private readonly IAddressService _addressService;
        #endregion

        #region Ctor

        public ReturnRequestController(ICustomerService customerService,
            ICustomNumberFormatterService customNumberFormatterService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IReturnRequestModelFactory returnRequestModelFactory,
            IReturnRequestService returnRequestService,
            IWorkContextService workContextService,
            IWorkflowMessageService workflowMessageService,
            IRazorViewEngine viewEngine,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IShipmentService shipmentService,
            IProductAttributeParserService productAttributeParserService,
            IPaymentService paymentService,
            IAddressService addressService) : base(viewEngine)
        {
            _customerService = customerService;
            _customNumberFormatterService = customNumberFormatterService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _returnRequestModelFactory = returnRequestModelFactory;
            _returnRequestService = returnRequestService;
            _workContextService = workContextService;
            _workflowMessageService = workflowMessageService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _shipmentService = shipmentService;
            _productAttributeParserService = productAttributeParserService;
            _paymentService = paymentService;
            _addressService = addressService;
        }

        #endregion

        #region Methods

        [RequireHttps]
        public virtual IActionResult CustomerReturnRequests(DateTime? startDateValue = null, DateTime? endDateValue = null, int returnStatusId = -1, bool orderByDate = true, int returnRequestFindByKey = -1, string returnRequestFindByKeyValue = null)
        {
            var cutomer = _workContextService.CurrentCustomer;

            if (!_customerService.IsRegistered(cutomer))
                return Challenge();

            var model = _returnRequestModelFactory.PrepareCustomerReturnRequestsModel(startDateValue, endDateValue, returnStatusId, orderByDate, returnRequestFindByKey, returnRequestFindByKeyValue);
            
            return View(model);
        }

        [RequireHttps]
        public virtual IActionResult ReturnRequest(int orderId)
        {
            var cutomer = _workContextService.CurrentCustomer;

            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || cutomer.Id != order.CustomerId)
                return Challenge();

            if (!_orderProcessingService.IsReturnRequestAllowed(order))
                return RedirectToRoute("Homepage");

            var model = new SubmitReturnRequestModel();
            model = _returnRequestModelFactory.PrepareSubmitReturnRequestModel(model, order);
            return View(model);
        }

        [HttpPost, ActionName("ReturnRequest")]
        public virtual IActionResult ReturnRequestSubmit(int orderId, SubmitReturnRequestModel model, IFormCollection form)
        {
            var cutomer = _workContextService.CurrentCustomer;

            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || cutomer.Id != order.CustomerId)
                return Challenge();

            if (!_orderProcessingService.IsReturnRequestAllowed(order))
                return RedirectToRoute("Homepage");

            var count = 0;

            var orderItems = _orderService.GetOrderItems(order.Id, isNotReturnable: false);
            foreach (var orderItem in orderItems)
            {
                var quantity = 0;  
                foreach (var formKey in form.Keys)
                    if (formKey.Equals($"quantity{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(form[formKey], out quantity);
                        break;
                    }
                if (quantity > 0)
                {
                    var rrr = _returnRequestService.GetReturnRequestReasonById(model.ReturnRequestReasonId);
                    var rra = _returnRequestService.GetReturnRequestActionById(model.ReturnRequestActionId);
                    var rr = new ReturnRequest
                    {
                        CustomNumber = "",
                        StoreId = InovatiqaDefaults.StoreId,
                        OrderItemId = orderItem.Id,
                        Quantity = quantity,
                        CustomerId = cutomer.Id,
                        ReasonForReturn = rrr != null ? rrr.Name : "not available",
                        RequestedAction = rra != null ? rra.Name : "not available",
                        CustomerComments = model.Comments,
                        StaffNotes = string.Empty,
                        ReturnRequestStatusId = (int)ReturnRequestStatus.Pending,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow
                    };

                    _returnRequestService.InsertReturnRequest(rr);

                    rr.CustomNumber = _customNumberFormatterService.GenerateReturnRequestCustomNumber(rr);
                    _customerService.UpdateCustomer(cutomer);
                    _returnRequestService.UpdateReturnRequest(rr);

                    _workflowMessageService.SendNewReturnRequestStoreOwnerNotification(rr, orderItem, order, InovatiqaDefaults.LanguageId);
                    _workflowMessageService.SendNewReturnRequestCustomerNotification(rr, orderItem, order);

                    count++;
                }
            }

            model = _returnRequestModelFactory.PrepareSubmitReturnRequestModel(model, order);
            if (count > 0)
                model.Result = "Your return request has been submitted successfully.";
            else
                model.Result = "Your return request has not been submitted because you haven't chosen any items.";

            return View(model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult UploadFileReturnRequest()
        {
            if (!InovatiqaDefaults.ReturnRequestsEnabled || !InovatiqaDefaults.ReturnRequestsAllowFiles)
            {
                return Json(new
                {
                    success = false,
                    downloadGuid = Guid.Empty,
                });
            }

            return Json(new
            {
                success = false,
                downloadGuid = Guid.Empty,
            });
        }

        [RequireHttps]
        public virtual IActionResult CreateReturnRequest()
        {
            var customer = _workContextService.CurrentCustomer;

            if (!_customerService.IsRegistered(customer))
                return Challenge();

            return View();
        }
        
        public virtual IActionResult ReturnPolicy()
        {
            return View();
        }

        public IActionResult SelectItems(DateTime? startDateValue = null, DateTime? endDateValue = null, int itemId = 0, int itemQuantity = 0, int returnReason = 0, int itemAction = 0, int[] selected = null, int[] selectedQuantity = null, int[] selectedReason = null, int[] selectedAction = null)
        {
            if(itemId > 0)
            {
                var item = _orderService.GetOrderItemById(itemId);
                itemQuantity = item.Quantity >= itemQuantity ? itemQuantity : item.Quantity;
            }
            var model = _returnRequestModelFactory.PrepareItemsSelectionModel(startDateValue, endDateValue);
            model = _returnRequestModelFactory.SelectItemsFromModel(model, selected, selectedQuantity, selectedReason, selectedAction);
            model = _returnRequestModelFactory.SelectCurrentItem(model, itemId, itemQuantity, returnReason, itemAction);
            return View(model);
        }
        public virtual IActionResult ShippingInfo(int[] items, int[] quantities, decimal totalPrice, int[] returnReasons, int[] selectedActions = null)
        {
            var model = new ReturnRequestItemsSelectionModel();
            model = _returnRequestModelFactory.PrepareShippingInfoModel(model, items, quantities, totalPrice, returnReasons, selectedActions);
            return View(model);
        }
        public virtual IActionResult Review(int[] items, int[] quantities, int[] reasons, int[] actions, decimal totalPrice, int shippingLabel, string email1, string email2)
        {
            var customer = _workContextService.CurrentCustomer;
            if (!_customerService.IsRegistered(customer))
                return Challenge();

            var model = new ReturnRequestItemsSelectionModel();
            model = _returnRequestModelFactory.PrepareCustomerReturnRequestReviewModel(model, customer, items, quantities, reasons, actions, totalPrice, shippingLabel, email1, email2);
            return View(model);
        }
        public virtual IActionResult Completed(int[] selected = null, int[] quantity = null, decimal credit = 0, int shippingLabel = 0, string email1 = "", string email2 = "", int[] reasons = null, int[] actions = null)
        {
            var customer = _workContextService.CurrentCustomer;
            var model = new ReturnRequestItemsSelectionModel();
            model = _returnRequestModelFactory.PrepareCustomerReturnRequestCompletedModel(model, customer, selected, quantity, credit, shippingLabel, email1, email2, reasons, actions);
            return View(model);
        }

        #endregion
    }
}