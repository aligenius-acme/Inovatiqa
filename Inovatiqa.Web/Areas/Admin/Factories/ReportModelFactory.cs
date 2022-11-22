using System;
using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Reports;
using Inovatiqa.Web.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial class ReportModelFactory : IReportModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICountryService _countryService;
        private readonly ICustomerReportService _customerReportService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelperService _dateTimeHelperService;
        private readonly IOrderReportService _orderReportService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeFormatterService _productAttributeFormatterService;
        private readonly IProductService _productService;
        private readonly IWorkContextService _workContextService;

        #endregion

        #region Ctor

        public ReportModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            ICustomerReportService customerReportService,
            ICustomerService customerService,
            IDateTimeHelperService dateTimeHelperService,
            IOrderReportService orderReportService,
            IPriceFormatter priceFormatter,
            IProductAttributeFormatterService productAttributeFormatterService,
            IProductService productService,
            IWorkContextService workContextService)
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _countryService = countryService;
            _customerReportService = customerReportService;
            _customerService = customerService;
            _dateTimeHelperService = dateTimeHelperService;
            _orderReportService = orderReportService;
            _priceFormatter = priceFormatter;
            _productAttributeFormatterService = productAttributeFormatterService;
            _productService = productService;
            _workContextService = workContextService;
        }

        #endregion

        #region Utilities

        protected virtual IPagedList<BestsellersReportLine> GetBestsellersReport(BestsellerSearchModel searchModel)
        {
            var orderStatus = searchModel.OrderStatusId > 0 ? (OrderStatus?)searchModel.OrderStatusId : null;
            var paymentStatus = searchModel.PaymentStatusId > 0 ? (PaymentStatus?)searchModel.PaymentStatusId : null;
            if (_workContextService.CurrentVendor != null)
                searchModel.VendorId = _workContextService.CurrentVendor.Id;
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.StartDate.Value, _dateTimeHelperService.CurrentTimeZone);
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.EndDate.Value, _dateTimeHelperService.CurrentTimeZone).AddDays(1);

            var bestsellers = _orderReportService.BestSellersReport(showHidden: true,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                os: orderStatus,
                ps: paymentStatus,
                billingCountryId: searchModel.BillingCountryId,
                orderBy: Inovatiqa.Services.Orders.OrderByEnum.OrderByTotalAmount,
                vendorId: searchModel.VendorId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                storeId: searchModel.StoreId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            return bestsellers;
        }

        #endregion

        #region Methods

        #region LowStock

        public virtual LowStockProductSearchModel PrepareLowStockProductSearchModel(LowStockProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "0",
                Text = "All"
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "1",
                Text = "Published only"
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "2",
                Text = "Unpublished only"
            });

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual LowStockProductListModel PrepareLowStockProductListModel(LowStockProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var publishedOnly = searchModel.SearchPublishedId == 0 ? null : searchModel.SearchPublishedId == 1 ? true : (bool?)false;
            var vendorId = _workContextService.CurrentVendor?.Id ?? 0;

            var products = _productService.GetLowStockProducts(vendorId: vendorId, loadPublishedOnly: publishedOnly);
            var combinations = _productService.GetLowStockProductCombinations(vendorId: vendorId, loadPublishedOnly: publishedOnly);

            var lowStockProductModels = new List<LowStockProductModel>();
            lowStockProductModels.AddRange(products.Select(product => new LowStockProductModel
            {
                Id = product.Id,
                Name = product.Name,
                ManageInventoryMethod = Enum.GetName(typeof(ManageInventoryMethod), product.ManageInventoryMethodId),
                StockQuantity = _productService.GetTotalStockQuantity(product),
                Published = product.Published
            }));

            lowStockProductModels.AddRange(combinations.Select(combination => {

                var product = _productService.GetProductById(combination.ProductId);

                return new LowStockProductModel
                {
                    Id = combination.ProductId,
                    Name = product.Name,
                    Attributes = _productAttributeFormatterService
                        .FormatAttributes(product, combination.AttributesXml, _workContextService.CurrentCustomer, "<br />", true, true, true, false),
                    ManageInventoryMethod = Enum.GetName(typeof(ManageInventoryMethod), product.ManageInventoryMethodId),
                    StockQuantity = combination.StockQuantity,
                    Published = product.Published
                };
            }));

            var pagesList = lowStockProductModels.ToPagedList(searchModel);

            var model = new LowStockProductListModel().PrepareToGrid(searchModel, pagesList, () =>
            {
                return pagesList;
            });

            return model;
        }

        #endregion

        #region Bestsellers

        public virtual BestsellerSearchModel PrepareBestsellerSearchModel(BestsellerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.IsLoggedInAsVendor = _workContextService.CurrentVendor != null;

            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            _baseAdminModelFactory.PrepareOrderStatuses(searchModel.AvailableOrderStatuses);

            _baseAdminModelFactory.PreparePaymentStatuses(searchModel.AvailablePaymentStatuses);

            _baseAdminModelFactory.PrepareCategories(searchModel.AvailableCategories);

            _baseAdminModelFactory.PrepareManufacturers(searchModel.AvailableManufacturers);

            searchModel.AvailableCountries = _countryService.GetAllCountriesForBilling(showHidden: true)
                .Select(country => new SelectListItem { Text = country.Name, Value = country.Id.ToString() }).ToList();
            searchModel.AvailableCountries.Insert(0, new SelectListItem { Text = "All", Value = "0" });

            _baseAdminModelFactory.PrepareVendors(searchModel.AvailableVendors);

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual BestsellerListModel PrepareBestsellerListModel(BestsellerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var bestsellers = GetBestsellersReport(searchModel);

            var model = new BestsellerListModel().PrepareToGrid(searchModel, bestsellers, () =>
            {
                return bestsellers.Select(bestseller =>
                {
                    var bestsellerModel = new BestsellerModel
                    {
                        ProductId = bestseller.ProductId,
                        TotalQuantity = bestseller.TotalQuantity
                    };

                    bestsellerModel.ProductName = _productService.GetProductById(bestseller.ProductId)?.Name;
                    bestsellerModel.TotalAmount = _priceFormatter.FormatPrice(bestseller.TotalAmount);

                    return bestsellerModel;
                });
            });

            return model;
        }

        public virtual string GetBestsellerTotalAmount(BestsellerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var bestsellers = GetBestsellersReport(searchModel);

            var totalAmount = _priceFormatter.FormatPrice(bestsellers.Sum(bestseller => bestseller.TotalAmount));

            return totalAmount;
        }

        #endregion

        #region NeverSold

        public virtual NeverSoldReportSearchModel PrepareNeverSoldSearchModel(NeverSoldReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.IsLoggedInAsVendor = _workContextService.CurrentVendor != null;

            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            _baseAdminModelFactory.PrepareCategories(searchModel.AvailableCategories);

            _baseAdminModelFactory.PrepareManufacturers(searchModel.AvailableManufacturers);

            _baseAdminModelFactory.PrepareVendors(searchModel.AvailableVendors);

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual NeverSoldReportListModel PrepareNeverSoldListModel(NeverSoldReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (_workContextService.CurrentVendor != null)
                searchModel.SearchVendorId = _workContextService.CurrentVendor.Id;
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.StartDate.Value, _dateTimeHelperService.CurrentTimeZone);
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.EndDate.Value, _dateTimeHelperService.CurrentTimeZone).AddDays(1);

            var items = _orderReportService.ProductsNeverSold(showHidden: true,
                vendorId: searchModel.SearchVendorId,
                storeId: searchModel.SearchStoreId,
                categoryId: searchModel.SearchCategoryId,
                manufacturerId: searchModel.SearchManufacturerId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new NeverSoldReportListModel().PrepareToGrid(searchModel, items, () =>
            {
                return items.Select(item => new NeverSoldReportModel
                {
                    ProductId = item.Id,
                    ProductName = item.Name
                });
            });

            return model;
        }

        #endregion

        #region Country sales

        public virtual CountryReportSearchModel PrepareCountrySalesSearchModel(CountryReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseAdminModelFactory.PrepareOrderStatuses(searchModel.AvailableOrderStatuses);

            _baseAdminModelFactory.PreparePaymentStatuses(searchModel.AvailablePaymentStatuses);

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual CountryReportListModel PrepareCountrySalesListModel(CountryReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var orderStatus = searchModel.OrderStatusId > 0 ? (OrderStatus?)searchModel.OrderStatusId : null;
            var paymentStatus = searchModel.PaymentStatusId > 0 ? (PaymentStatus?)searchModel.PaymentStatusId : null;
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.StartDate.Value, _dateTimeHelperService.CurrentTimeZone);
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.EndDate.Value, _dateTimeHelperService.CurrentTimeZone).AddDays(1);

            var items = _orderReportService.GetCountryReport(os: orderStatus,
                ps: paymentStatus,
                startTimeUtc: startDateValue,
                endTimeUtc: endDateValue).ToPagedList(searchModel);

            var model = new CountryReportListModel().PrepareToGrid(searchModel, items, () =>
            {
                return items.Select(item =>
                {
                    var countryReportModel = new CountryReportModel
                    {
                        TotalOrders = item.TotalOrders
                    };

                    countryReportModel.SumOrders = _priceFormatter.FormatPrice(item.SumOrders);
                    countryReportModel.CountryName = _countryService.GetCountryById(item.CountryId ?? 0)?.Name;

                    return countryReportModel;
                });
            });

            return model;
        }

        #endregion

        #region Customer reports

        public virtual CustomerReportsSearchModel PrepareCustomerReportsSearchModel(CustomerReportsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            PrepareBestCustomersReportByOrderTotalSearchModel(searchModel.BestCustomersByOrderTotal);
            PrepareBestCustomersReportSearchModel(searchModel.BestCustomersByNumberOfOrders);
            PrepareRegisteredCustomersReportSearchModel(searchModel.RegisteredCustomers);

            return searchModel;
        }

        protected virtual BestCustomersReportSearchModel PrepareBestCustomersReportSearchModel(BestCustomersReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseAdminModelFactory.PrepareOrderStatuses(searchModel.AvailableOrderStatuses);
            _baseAdminModelFactory.PreparePaymentStatuses(searchModel.AvailablePaymentStatuses);
            _baseAdminModelFactory.PrepareShippingStatuses(searchModel.AvailableShippingStatuses);

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual BestCustomersReportSearchModel PrepareBestCustomersReportByOrderTotalSearchModel(BestCustomersReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseAdminModelFactory.PrepareOrderStatuses(searchModel.AvailableOrderStatuses);
            _baseAdminModelFactory.PreparePaymentStatuses(searchModel.AvailablePaymentStatuses);
            _baseAdminModelFactory.PrepareShippingStatuses(searchModel.AvailableShippingStatuses);

            searchModel.SetGridPageSize();

            return searchModel;
        }


        protected virtual RegisteredCustomersReportSearchModel PrepareRegisteredCustomersReportSearchModel(RegisteredCustomersReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual BestCustomersReportListModel PrepareBestCustomersReportListModel(BestCustomersReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.StartDate.Value, _dateTimeHelperService.CurrentTimeZone);
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.EndDate.Value, _dateTimeHelperService.CurrentTimeZone).AddDays(1);
            var orderStatus = searchModel.OrderStatusId > 0 ? (OrderStatus?)searchModel.OrderStatusId : null;
            var paymentStatus = searchModel.PaymentStatusId > 0 ? (PaymentStatus?)searchModel.PaymentStatusId : null;
            var shippingStatus = searchModel.ShippingStatusId > 0 ? (ShippingStatus?)searchModel.ShippingStatusId : null;

            var reportItems = _customerReportService.GetBestCustomersReport(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                os: orderStatus,
                ps: paymentStatus,
                ss: shippingStatus,
                orderBy: searchModel.OrderBy,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new BestCustomersReportListModel().PrepareToGrid(searchModel, reportItems, () =>
            {
                return reportItems.Select(item =>
               {
                    var bestCustomersReportModel = new BestCustomersReportModel
                   {
                       CustomerId = item.CustomerId,
                       OrderTotal = _priceFormatter.FormatPrice(item.OrderTotal),
                       OrderCount = item.OrderCount
                   };

                    var customer = _customerService.GetCustomerById(item.CustomerId);
                   if (customer != null)
                   {
                       bestCustomersReportModel.CustomerName = _customerService.IsRegistered(customer) ? customer.Email :
                          "Guest";
                   }

                   return bestCustomersReportModel;
               });
            });

            return model;
        }
                
        public virtual RegisteredCustomersReportListModel PrepareRegisteredCustomersReportListModel(RegisteredCustomersReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var reportItems = new List<RegisteredCustomersReportModel>
            {
                new RegisteredCustomersReportModel
                {
                    Period = "In the last 7 days",
                    Customers = _customerReportService.GetRegisteredCustomersReport(7)
                },
                new RegisteredCustomersReportModel
                {
                    Period = "In the last 7 days",
                    Customers = _customerReportService.GetRegisteredCustomersReport(14)
                },
                new RegisteredCustomersReportModel
                {
                    Period = "In the last month",
                    Customers = _customerReportService.GetRegisteredCustomersReport(30)
                },
                new RegisteredCustomersReportModel
                {
                    Period = "In the last year",
                    Customers = _customerReportService.GetRegisteredCustomersReport(365)
                }
            };

            var pagedList = reportItems.ToPagedList(searchModel);

            var model = new RegisteredCustomersReportListModel().PrepareToGrid(searchModel, pagedList, () =>
            {
                return pagedList;
            });

            return model;
        }

        #endregion

        #endregion
    }
}