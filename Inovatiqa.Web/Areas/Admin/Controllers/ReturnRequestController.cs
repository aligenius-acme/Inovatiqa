using System;
using System.Linq;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Web.Areas.Admin.Models.Orders;
using Inovatiqa.Web.Controllers;
using Inovatiqa.Web.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    public partial class ReturnRequestController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly IReturnRequestModelFactory _returnRequestModelFactory;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly INotificationService _notificationService;

        #endregion Fields

        #region Ctor

        public ReturnRequestController(ICustomerActivityService customerActivityService,
            IOrderService orderService,
            IPermissionService permissionService,
            IReturnRequestModelFactory returnRequestModelFactory,
            IReturnRequestService returnRequestService,
            IWorkflowMessageService workflowMessageService,
            INotificationService notificationService,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _customerActivityService = customerActivityService;
            _orderService = orderService;
            _permissionService = permissionService;
            _returnRequestModelFactory = returnRequestModelFactory;
            _returnRequestService = returnRequestService;
            _workflowMessageService = workflowMessageService;
            _notificationService = notificationService;
        }

        #endregion

        #region Utilities


        #endregion

        #region Methods

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            var model = _returnRequestModelFactory.PrepareReturnRequestSearchModel(new ReturnRequestSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult List(ReturnRequestSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedDataTablesJson();

            var model = _returnRequestModelFactory.PrepareReturnRequestListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            var returnRequest = _returnRequestService.GetReturnRequestById(id);
            if (returnRequest == null)
                return RedirectToAction("List");

            var model = _returnRequestModelFactory.PrepareReturnRequestModel(null, returnRequest);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual IActionResult Edit(ReturnRequestModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            var returnRequest = _returnRequestService.GetReturnRequestById(model.Id);
            if (returnRequest == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                returnRequest = model.ToReturnRequestEntity(returnRequest);
                returnRequest.ReturnRequestStatusId = model.ReturnRequestStatusId;
                returnRequest.UpdatedOnUtc = DateTime.UtcNow;
                
                _returnRequestService.UpdateReturnRequest(returnRequest);

                _customerActivityService.InsertActivity("EditReturnRequest",
                    string.Format("Edited a return request (ID = {0})", returnRequest.Id), returnRequest.Id, returnRequest.GetType().Name);

                _notificationService.SuccessNotification("The return request has been updated successfully.");

                return continueEditing ? RedirectToAction("Edit", new { id = returnRequest.Id }) : RedirectToAction("List");
            }

            model = _returnRequestModelFactory.PrepareReturnRequestModel(model, returnRequest, true);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("notify-customer")]
        public virtual IActionResult NotifyCustomer(ReturnRequestModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            var returnRequest = _returnRequestService.GetReturnRequestById(model.Id);
            if (returnRequest == null)
                return RedirectToAction("List");

            var orderItem = _orderService.GetOrderItemById(returnRequest.OrderItemId);
            if (orderItem is null)
            {
                _notificationService.ErrorNotification("Order item is deleted");
                return RedirectToAction("Edit", new { id = returnRequest.Id });
            }

            var order = _orderService.GetOrderById(orderItem.OrderId);

            var queuedEmailIds = _workflowMessageService.SendReturnRequestStatusChangedCustomerNotification(returnRequest, orderItem, order);
            if (queuedEmailIds.Any())
                _notificationService.SuccessNotification("The customer has been notified successfully.");

            return RedirectToAction("Edit", new { id = returnRequest.Id });
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            var returnRequest = _returnRequestService.GetReturnRequestById(id);
            if (returnRequest == null)
                return RedirectToAction("List");

            _returnRequestService.DeleteReturnRequest(returnRequest);

            _customerActivityService.InsertActivity("DeleteReturnRequest",
                string.Format("Deleted a return request (ID = {0})", returnRequest.Id), returnRequest.Id, Request.GetType().Name);

            _notificationService.SuccessNotification("The return request has been deleted successfully.");

            return RedirectToAction("List");
        }

        #region Return request reasons

        public virtual IActionResult ReturnRequestReasonList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            SaveSelectedPanelName("ordersettings-return-request");

            return RedirectToAction("Order", "Setting");
        }

        [HttpPost]
        public virtual IActionResult ReturnRequestReasonList(ReturnRequestReasonSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedDataTablesJson();

            var model = _returnRequestModelFactory.PrepareReturnRequestReasonListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult ReturnRequestReasonCreate()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = _returnRequestModelFactory.PrepareReturnRequestReasonModel(new ReturnRequestReasonModel(), null);
            
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult ReturnRequestReasonCreate(ReturnRequestReasonModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var returnRequestReason = model.ToReturnRequestReasonEntity<ReturnRequestReason>();
                _returnRequestService.InsertReturnRequestReason(returnRequestReason);


                _notificationService.SuccessNotification("The new return request reason has been added successfully.");

                return continueEditing 
                    ? RedirectToAction("ReturnRequestReasonEdit", new { id = returnRequestReason.Id })
                    : RedirectToAction("ReturnRequestReasonList");
            }

            model = _returnRequestModelFactory.PrepareReturnRequestReasonModel(model, null, true);

            return View(model);
        }

        public virtual IActionResult ReturnRequestReasonEdit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var returnRequestReason = _returnRequestService.GetReturnRequestReasonById(id);
            if (returnRequestReason == null)
                return RedirectToAction("ReturnRequestReasonList");

            var model = _returnRequestModelFactory.PrepareReturnRequestReasonModel(null, returnRequestReason);
            
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult ReturnRequestReasonEdit(ReturnRequestReasonModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var returnRequestReason = _returnRequestService.GetReturnRequestReasonById(model.Id);
            if (returnRequestReason == null)
                return RedirectToAction("ReturnRequestReasonList");

            if (ModelState.IsValid)
            {
                returnRequestReason = model.ToReturnRequestReasonEntity<ReturnRequestReason>();
                _returnRequestService.UpdateReturnRequestReason(returnRequestReason);

                _notificationService.SuccessNotification("The return request reason has been updated successfully.");

                if (!continueEditing)
                    return RedirectToAction("ReturnRequestReasonList");
                
                return RedirectToAction("ReturnRequestReasonEdit", new { id = returnRequestReason.Id });
            }

            model = _returnRequestModelFactory.PrepareReturnRequestReasonModel(model, returnRequestReason, true);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult ReturnRequestReasonDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var returnRequestReason = _returnRequestService.GetReturnRequestReasonById(id) 
                ?? throw new ArgumentException("No return request reason found with the specified id", nameof(id));

            _returnRequestService.DeleteReturnRequestReason(returnRequestReason);

            _notificationService.SuccessNotification("The return request reason has been deleted successfully");

            return RedirectToAction("ReturnRequestReasonList");
        }

        #endregion

        #region Return request actions

        public virtual IActionResult ReturnRequestActionList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            SaveSelectedPanelName("ordersettings-return-request");

            return RedirectToAction("Order", "Setting");
        }

        [HttpPost]
        public virtual IActionResult ReturnRequestActionList(ReturnRequestActionSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedDataTablesJson();

            var model = _returnRequestModelFactory.PrepareReturnRequestActionListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult ReturnRequestActionCreate()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = _returnRequestModelFactory.PrepareReturnRequestActionModel(new ReturnRequestActionModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult ReturnRequestActionCreate(ReturnRequestActionModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var returnRequestAction = model.ToReturnRequestActionEntity<ReturnRequestAction>();
                _returnRequestService.InsertReturnRequestAction(returnRequestAction);


                _notificationService.SuccessNotification("The new return request action has been added successfully.");

                return continueEditing 
                    ? RedirectToAction("ReturnRequestActionEdit", new { id = returnRequestAction.Id }) 
                    : RedirectToAction("ReturnRequestActionList");
            }

            model = _returnRequestModelFactory.PrepareReturnRequestActionModel(model, null, true);

            return View(model);
        }

        public virtual IActionResult ReturnRequestActionEdit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var returnRequestAction = _returnRequestService.GetReturnRequestActionById(id);
            if (returnRequestAction == null)
                return RedirectToAction("ReturnRequestActionList");

            var model = _returnRequestModelFactory.PrepareReturnRequestActionModel(null, returnRequestAction);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult ReturnRequestActionEdit(ReturnRequestActionModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var returnRequestAction = _returnRequestService.GetReturnRequestActionById(model.Id);
            if (returnRequestAction == null)
                return RedirectToAction("ReturnRequestActionList");

            if (ModelState.IsValid)
            {
                returnRequestAction = model.ToReturnRequestActionEntity<ReturnRequestAction>();
                _returnRequestService.UpdateReturnRequestAction(returnRequestAction);

                _notificationService.SuccessNotification("The return request action has been updated successfully.");

                if (!continueEditing)
                    return RedirectToAction("ReturnRequestActionList");
                
                return RedirectToAction("ReturnRequestActionEdit", new { id = returnRequestAction.Id });
            }

            model = _returnRequestModelFactory.PrepareReturnRequestActionModel(model, returnRequestAction, true);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult ReturnRequestActionDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var returnRequestAction = _returnRequestService.GetReturnRequestActionById(id)
                ?? throw new ArgumentException("No return request action found with the specified id", nameof(id));

            _returnRequestService.DeleteReturnRequestAction(returnRequestAction);

            _notificationService.SuccessNotification("The return request action has been deleted successfully");

            return RedirectToAction("ReturnRequestActionList");
        }

        #endregion

        #endregion
    }
}