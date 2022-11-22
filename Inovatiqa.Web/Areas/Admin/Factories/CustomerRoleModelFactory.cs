using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Web.Areas.Admin.Models.Catalog;
using Inovatiqa.Web.Areas.Admin.Models.Customers;
using Inovatiqa.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial class CustomerRoleModelFactory : ICustomerRoleModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContextService _workContextService;

        #endregion

        #region Ctor

        public CustomerRoleModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            ICustomerService customerService,
            IProductService productService,
            IUrlRecordService urlRecordService,
            IWorkContextService workContextService)
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _customerService = customerService;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _workContextService = workContextService;
        }

        #endregion

   
        #region Methods

        public virtual CustomerRoleSearchModel PrepareCustomerRoleSearchModel(CustomerRoleSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual CustomerRoleListModel PrepareCustomerRoleListModel(CustomerRoleSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var customerRoles = _customerService.GetAllCustomerRoles(true).ToPagedList(searchModel);

            var model = new CustomerRoleListModel().PrepareToGrid(searchModel, customerRoles, () =>
            {
                return customerRoles.Select(role =>
                {
                    var customerRoleModel = role.ToCustomerRoleModel<CustomerRoleModel>();

                    customerRoleModel.PurchasedWithProductName = _productService.GetProductById(role.PurchasedWithProductId)?.Name;

                    return customerRoleModel;
                });
            });

            return model;
        }

        public virtual CustomerRoleModel PrepareCustomerRoleModel(CustomerRoleModel model, CustomerRole customerRole, bool excludeProperties = false)
        {
            if (customerRole != null)
            {
                model ??= customerRole.ToCustomerRoleModel<CustomerRoleModel>();
                model.PurchasedWithProductName = _productService.GetProductById(customerRole.PurchasedWithProductId)?.Name;
            }

            if (customerRole == null)
                model.Active = true;

            //_baseAdminModelFactory.PrepareTaxDisplayTypes(model.TaxDisplayTypeValues, false);

            return model;
        }

        public virtual CustomerRoleProductSearchModel PrepareCustomerRoleProductSearchModel(CustomerRoleProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.IsLoggedInAsVendor = _workContextService.CurrentVendor != null;

            _baseAdminModelFactory.PrepareCategories(searchModel.AvailableCategories);

            _baseAdminModelFactory.PrepareManufacturers(searchModel.AvailableManufacturers);

            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            _baseAdminModelFactory.PrepareVendors(searchModel.AvailableVendors);

            _baseAdminModelFactory.PrepareProductTypes(searchModel.AvailableProductTypes);

            searchModel.SetPopupGridPageSize();

            return searchModel;
        }

        public virtual CustomerRoleProductListModel PrepareCustomerRoleProductListModel(CustomerRoleProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (_workContextService.CurrentVendor != null)
                searchModel.SearchVendorId = _workContextService.CurrentVendor.Id;

            var products = _productService.SearchProducts(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerId: searchModel.SearchManufacturerId,
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new CustomerRoleProductListModel().PrepareToGrid(searchModel, products, () =>
            {
                return products.Select(product =>
                {
                    var productModel = product.ToProductModel<ProductModel>();
                    productModel.SeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId);

                    return productModel;
                });
            });

            return model;
        }

        #endregion
    }
}