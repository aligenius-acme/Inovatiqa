using System;
using System.Linq;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Common;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Core;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Web.Extensions;
using Inovatiqa.Services.Common.Interfaces;

namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial class CommonModelFactory : ICommonModelFactory
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IProductService _productService;
        private readonly ISearchTermService _searchTermService;

        #endregion

        #region CtorIReturnRequestService _returnRequestService

        public CommonModelFactory(IOrderService orderService,
            ICustomerService customerService,
            IReturnRequestService returnRequestService,
            IProductService productService,
            ISearchTermService searchTermService)
        {
            _orderService = orderService;
            _customerService = customerService;
            _returnRequestService = returnRequestService;
            _productService = productService;
            _searchTermService = searchTermService;
    }

        #endregion

        #region Utilities



        #endregion

        #region Methods

        public virtual PopularSearchTermSearchModel PreparePopularSearchTermSearchModel(PopularSearchTermSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            searchModel.SetGridPageSize(5);

            return searchModel;
        }

        public virtual CommonStatisticsModel PrepareCommonStatisticsModel()
        {
            var model = new CommonStatisticsModel
            {
                NumberOfOrders = _orderService.SearchOrders(pageIndex: 0, pageSize: 1, getOnlyTotalCount: true).TotalCount
            };

            var customerRoleIds = new[] { _customerService.GetCustomerRoleBySystemName(InovatiqaDefaults.RegisteredRoleName).Id };
            model.NumberOfCustomers = _customerService.GetAllCustomers(customerRoleIds: customerRoleIds,
                pageIndex: 0, pageSize: 1, getOnlyTotalCount: true).TotalCount;

            var returnRequestStatus = ReturnRequestStatus.Pending;
            model.NumberOfPendingReturnRequests = _returnRequestService.SearchReturnRequests(rs: returnRequestStatus,
                pageIndex: 0, pageSize: 1, getOnlyTotalCount: true).TotalCount;

            model.NumberOfLowStockProducts =
                _productService.GetLowStockProducts(getOnlyTotalCount: true).TotalCount +
                _productService.GetLowStockProductCombinations(getOnlyTotalCount: true).TotalCount;

            return model;
        }

        public virtual PopularSearchTermListModel PreparePopularSearchTermListModel(PopularSearchTermSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var searchTermRecordLines = _searchTermService.GetStats(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new PopularSearchTermListModel().PrepareToGrid(searchModel, searchTermRecordLines, () =>
            {
                return searchTermRecordLines.Select(searchTerm => new PopularSearchTermModel
                {
                    Keyword = searchTerm.Keyword,
                    Count = searchTerm.Count
                });
            });

            return model;
        }

        #endregion
    }
}