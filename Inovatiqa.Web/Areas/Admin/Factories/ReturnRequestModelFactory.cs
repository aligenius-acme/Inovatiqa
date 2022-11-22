using System;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Web.Areas.Admin.Models.Orders;
using Inovatiqa.Web.Extensions;
using Inovatiqa.Web.Framework.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial class ReturnRequestModelFactory : IReturnRequestModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IDateTimeHelperService _dateTimeHelperService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IReturnRequestService _returnRequestService;

        #endregion

        #region Ctor

        public ReturnRequestModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            IDateTimeHelperService dateTimeHelperService,
            ICustomerService customerService,
            ILocalizedModelFactory localizedModelFactory,
            IOrderService orderService,
            IProductService productService,
            IReturnRequestService returnRequestService)
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _dateTimeHelperService = dateTimeHelperService;
            _customerService = customerService;
            _localizedModelFactory = localizedModelFactory;
            _orderService = orderService;
            _productService = productService;
            _returnRequestService = returnRequestService;
        }

        #endregion

        #region Methods

        public virtual ReturnRequestSearchModel PrepareReturnRequestSearchModel(ReturnRequestSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseAdminModelFactory.PrepareReturnRequestStatuses(searchModel.ReturnRequestStatusList, false);

            searchModel.ReturnRequestStatusId = -1;
            searchModel.ReturnRequestStatusList.Insert(0, new SelectListItem
            {
                Value = "-1",
                Text = "All"
            });

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual ReturnRequestListModel PrepareReturnRequestListModel(ReturnRequestSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.StartDate.Value, _dateTimeHelperService.CurrentTimeZone);
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.EndDate.Value, _dateTimeHelperService.CurrentTimeZone).AddDays(1);
            var returnRequestStatus = searchModel.ReturnRequestStatusId == -1 ? null : (ReturnRequestStatus?)searchModel.ReturnRequestStatusId;

            var returnRequests = _returnRequestService.SearchReturnRequests(customNumber: searchModel.CustomNumber,
                rs: returnRequestStatus,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new ReturnRequestListModel().PrepareToGrid(searchModel, returnRequests, () =>
            {
                return returnRequests.Select(returnRequest =>
                {
                    var returnRequestModel = returnRequest.ToReturnRequestModel<ReturnRequestModel>();

                    var customer = _customerService.GetCustomerById(returnRequest.CustomerId);

                    returnRequestModel.CreatedOn = _dateTimeHelperService.ConvertToUserTime(returnRequest.CreatedOnUtc, DateTimeKind.Utc);

                    returnRequestModel.CustomerInfo = _customerService.IsRegistered(customer)
                        ? customer.Email : "Guest";
                    returnRequestModel.ReturnRequestStatusStr = Enum.GetName(typeof(ReturnRequestStatus), returnRequest.ReturnRequestStatusId).Replace("ReturnAuthorized", "Return authorized").Replace("ItemsRepaired", "Items repaired").Replace("ItemsRefunded", "Items refunded").Replace("RequestRejected", "Request rejected");
                    var orderItem = _orderService.GetOrderItemById(returnRequest.OrderItemId);
                    if (orderItem == null)
                        return returnRequestModel;

                    var order = _orderService.GetOrderById(orderItem.OrderId);
                    var product = _productService.GetProductById(orderItem.ProductId);

                    returnRequestModel.ProductId = orderItem.ProductId;
                    returnRequestModel.ProductName = product.Name;
                    returnRequestModel.OrderId = order.Id;
                    returnRequestModel.AttributeInfo = orderItem.AttributeDescription;
                    returnRequestModel.CustomOrderNumber = order.CustomOrderNumber;

                    return returnRequestModel;
                });
            });

            return model;
        }

        public virtual ReturnRequestModel PrepareReturnRequestModel(ReturnRequestModel model,
            ReturnRequest returnRequest, bool excludeProperties = false)
        {
            if (returnRequest == null)
                return model;

            model ??= new ReturnRequestModel
            {
                Id = returnRequest.Id,
                CustomNumber = returnRequest.CustomNumber,
                CustomerId = returnRequest.CustomerId,
                Quantity = returnRequest.Quantity
            };

            var customer = _customerService.GetCustomerById(returnRequest.CustomerId);

            model.CreatedOn = _dateTimeHelperService.ConvertToUserTime(returnRequest.CreatedOnUtc, DateTimeKind.Utc);

            model.CustomerInfo = _customerService.IsRegistered(customer)
                ? customer.Email : "Guest";
            model.UploadedFileGuid = Guid.Empty;
            var orderItem = _orderService.GetOrderItemById(returnRequest.OrderItemId);
            if (orderItem != null)
            {
                var order = _orderService.GetOrderById(orderItem.OrderId);
                var product = _productService.GetProductById(orderItem.ProductId);

                model.ProductId = product.Id;
                model.ProductName = product.Name;
                model.OrderId = order.Id;
                model.AttributeInfo = orderItem.AttributeDescription;
                model.CustomOrderNumber = order.CustomOrderNumber;
            }

            if (excludeProperties)
                return model;

            model.ReasonForReturn = returnRequest.ReasonForReturn;
            model.RequestedAction = returnRequest.RequestedAction;
            model.CustomerComments = returnRequest.CustomerComments;
            model.StaffNotes = returnRequest.StaffNotes;
            model.ReturnRequestStatusId = returnRequest.ReturnRequestStatusId;

            return model;
        }

        public virtual ReturnRequestReasonSearchModel PrepareReturnRequestReasonSearchModel(ReturnRequestReasonSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual ReturnRequestReasonListModel PrepareReturnRequestReasonListModel(ReturnRequestReasonSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var reasons = _returnRequestService.GetAllReturnRequestReasons().ToPagedList(searchModel);

            var model = new ReturnRequestReasonListModel().PrepareToGrid(searchModel, reasons, () =>
            {
                return reasons.Select(reason => reason.ToReturnRequestReasonModel<ReturnRequestReasonModel>());
            });

            return model;
        }

        public virtual ReturnRequestReasonModel PrepareReturnRequestReasonModel(ReturnRequestReasonModel model,
            ReturnRequestReason returnRequestReason, bool excludeProperties = false)
        {
            Action<ReturnRequestReasonLocalizedModel, int> localizedModelConfiguration = null;

            if (returnRequestReason != null)
            {
                model ??= returnRequestReason.ToReturnRequestReasonModel<ReturnRequestReasonModel>();

                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = returnRequestReason.Name;
                };
            }

            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            return model;
        }

        public virtual ReturnRequestActionSearchModel PrepareReturnRequestActionSearchModel(ReturnRequestActionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual ReturnRequestActionListModel PrepareReturnRequestActionListModel(ReturnRequestActionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var actions = _returnRequestService.GetAllReturnRequestActions().ToPagedList(searchModel);

            var model = new ReturnRequestActionListModel().PrepareToGrid(searchModel, actions, () =>
            {
                return actions.Select(reason => reason.ToReturnRequestActionModel<ReturnRequestActionModel>());
            });

            return model;
        }

        public virtual ReturnRequestActionModel PrepareReturnRequestActionModel(ReturnRequestActionModel model,
            ReturnRequestAction returnRequestAction, bool excludeProperties = false)
        {
            Action<ReturnRequestActionLocalizedModel, int> localizedModelConfiguration = null;

            if (returnRequestAction != null)
            {
                model ??= returnRequestAction.ToReturnRequestActionModel<ReturnRequestActionModel>();

                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = returnRequestAction.Name;
                };
            }

            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            return model;
        }

        #endregion
    }
}