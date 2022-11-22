using System;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Web.Areas.Admin.Models.ShoppingCart;
using Inovatiqa.Web.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial class ShoppingCartModelFactory : IShoppingCartModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelperService _dateTimeHelperService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeFormatterService _productAttributeFormatterService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;

        #endregion

        #region Ctor

        public ShoppingCartModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            ICustomerService customerService,
            IDateTimeHelperService dateTimeHelperService,
            IPriceFormatter priceFormatter,
            IProductAttributeFormatterService productAttributeFormatterService,
            IProductService productService,
            IShoppingCartService shoppingCartService)
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _countryService = countryService;
            _customerService = customerService;
            _dateTimeHelperService = dateTimeHelperService;
            _priceFormatter = priceFormatter;
            _productAttributeFormatterService = productAttributeFormatterService;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
        }

        #endregion

        #region Utilities

        protected virtual ShoppingCartItemSearchModel PrepareShoppingCartItemSearchModel(ShoppingCartItemSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        #endregion

        #region Methods

        public virtual ShoppingCartSearchModel PrepareShoppingCartSearchModel(ShoppingCartSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseAdminModelFactory.PrepareShoppingCartTypes(searchModel.AvailableShoppingCartTypes, false);

            searchModel.ShoppingCartTypeId = (int)ShoppingCartType.ShoppingCart;

            searchModel.AvailableCountries = _countryService.GetAllCountriesForBilling(showHidden: true)
                .Select(country => new SelectListItem { Text = country.Name, Value = country.Id.ToString() }).ToList();
            searchModel.AvailableCountries.Insert(0, new SelectListItem { Text = "All", Value = "0" });

            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            searchModel.HideStoresList = InovatiqaDefaults.HideStoresList;

            PrepareShoppingCartItemSearchModel(searchModel.ShoppingCartItemSearchModel);

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual ShoppingCartListModel PrepareShoppingCartListModel(ShoppingCartSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var customers = _customerService.GetCustomersWithShoppingCarts(searchModel.ShoppingCartTypeId,
                storeId: searchModel.StoreId,
                productId: searchModel.ProductId,
                createdFromUtc: searchModel.StartDate,
                createdToUtc: searchModel.EndDate,
                countryId: searchModel.BillingCountryId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new ShoppingCartListModel().PrepareToGrid(searchModel, customers, () =>
            {
                return customers.Select(customer =>
                {
                    var shoppingCartModel = new ShoppingCartModel
                    {
                        CustomerId = customer.Id
                    };

                    shoppingCartModel.CustomerEmail = _customerService.IsRegistered(customer)
                        ? customer.Email : "Guest";
                    shoppingCartModel.TotalItems = _shoppingCartService.GetShoppingCart(customer, searchModel.ShoppingCartTypeId,
                        searchModel.StoreId, searchModel.ProductId, searchModel.StartDate, searchModel.EndDate).Sum(item => item.Quantity);

                    return shoppingCartModel;
                });
            });

            return model;
        }

        public virtual ShoppingCartItemListModel PrepareShoppingCartItemListModel(ShoppingCartItemSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var items = _shoppingCartService.GetShoppingCart(customer, searchModel.ShoppingCartTypeId,
                searchModel.StoreId, searchModel.ProductId, searchModel.StartDate, searchModel.EndDate).ToPagedList(searchModel);
            
            var isSearchProduct = searchModel.ProductId > 0;

            Product product = null;

            if (isSearchProduct)
            {
                product = _productService.GetProductById(searchModel.ProductId) ?? throw new Exception("Product is not found");
            }

            var model = new ShoppingCartItemListModel().PrepareToGrid(searchModel, items, () =>
            {
                return items.Select(item =>
                {
                    var itemModel = item.ToShoppingCartItemModel<ShoppingCartItemModel>();

                    if (!isSearchProduct)
                        product = _productService.GetProductById(item.ProductId);

                    itemModel.UpdatedOn = _dateTimeHelperService.ConvertToUserTime(item.UpdatedOnUtc, DateTimeKind.Utc);

                    itemModel.Store = InovatiqaDefaults.CurrentStoreName;
                    itemModel.AttributeInfo = _productAttributeFormatterService.FormatAttributes(product, item.AttributesXml, customer);
                    var unitPrice = _shoppingCartService.GetUnitPrice(item);
                    itemModel.UnitPrice = unitPrice.ToString();
                    var subTotal = _shoppingCartService.GetSubTotal(item);
                    itemModel.Total = _priceFormatter.FormatPrice(subTotal);

                    itemModel.ProductName = product.Name;

                    return itemModel;
                });
            });

            return model;
        }

        #endregion
    }
}