using System;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Discounts.Interfaces;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Catalog;
using Inovatiqa.Web.Extensions;
using Inovatiqa.Web.Framework.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial class CategoryModelFactory : ICategoryModelFactory
    {
        #region Fields

        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly IDiscountService _discountService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public CategoryModelFactory(IAclSupportedModelFactory aclSupportedModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICategoryService categoryService,
            IDiscountService discountService,
            ILocalizedModelFactory localizedModelFactory,
            IProductService productService,
            IUrlRecordService urlRecordService)
        {
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _baseAdminModelFactory = baseAdminModelFactory;
            _categoryService = categoryService;
            _discountService = discountService;
            _localizedModelFactory = localizedModelFactory;
            _productService = productService;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Utilities

        protected virtual CategoryProductSearchModel PrepareCategoryProductSearchModel(CategoryProductSearchModel searchModel, Category category)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (category == null)
                throw new ArgumentNullException(nameof(category));

            searchModel.CategoryId = category.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        #endregion

        #region Methods

        public virtual CategorySearchModel PrepareCategorySearchModel(CategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            searchModel.HideStoresList = InovatiqaDefaults.HideStoresList;

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

        public virtual CategoryListModel PrepareCategoryListModel(CategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            var categories = _categoryService.GetAllCategories(categoryName: searchModel.SearchCategoryName,
                showHidden: true,
                storeId: searchModel.SearchStoreId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                overridePublished: searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1));

            var model = new CategoryListModel().PrepareToGrid(searchModel, categories, () =>
            {
                return categories.Select(category =>
                {
                    var categoryModel = category.ToCategoryModel<CategoryModel>();

                    categoryModel.Breadcrumb = _categoryService.GetFormattedBreadCrumb(category);
                    categoryModel.SeName = _urlRecordService.GetActiveSlug(category.Id, InovatiqaDefaults.CategorySlugName, InovatiqaDefaults.LanguageId);

                    return categoryModel;
                });
            });

            return model;
        }

        public virtual CategoryModel PrepareCategoryModel(CategoryModel model, Category category, bool excludeProperties = false)
        {
            Action<CategoryLocalizedModel, int> localizedModelConfiguration = null;

            if (category != null)
            {
                if (model == null)
                {
                    model = category.ToCategoryModel<CategoryModel>();
                    model.SeName = _urlRecordService.GetActiveSlug(category.Id, InovatiqaDefaults.CategorySlugName, InovatiqaDefaults.LanguageId);
                }

                PrepareCategoryProductSearchModel(model.CategoryProductSearchModel, category);

                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = category.Name;
                    locale.Description = category.Description;
                    locale.MetaKeywords = category.MetaKeywords;
                    locale.MetaDescription = category.MetaDescription;
                    locale.MetaTitle = category.MetaTitle;
                    locale.SeName = _urlRecordService.GetActiveSlug(category.Id, InovatiqaDefaults.CategorySlugName, InovatiqaDefaults.LanguageId);
                };
            }

            if (category == null)
            {
                model.PageSize = InovatiqaDefaults.PageSize;
                model.PageSizeOptions = InovatiqaDefaults.PageSizeOptions;
                model.Published = true;
                model.IncludeInTopMenu = true;
                model.AllowCustomersToSelectPageSize = true;
            }

            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            _baseAdminModelFactory.PrepareCategoryTemplates(model.AvailableCategoryTemplates, false);

            _baseAdminModelFactory.PrepareCategories(model.AvailableCategories,
                defaultItemText: "[None]");

            return model;
        }

        public virtual CategoryProductListModel PrepareCategoryProductListModel(CategoryProductSearchModel searchModel, Category category)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var productCategories = _categoryService.GetProductCategoriesByCategoryId(category.Id,
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new CategoryProductListModel().PrepareToGrid(searchModel, productCategories, () =>
            {
                return productCategories.Select(productCategory =>
                {
                    var categoryProductModel = productCategory.ToCategoryProductModel<CategoryProductModel>();

                    categoryProductModel.ProductName = _productService.GetProductById(productCategory.ProductId)?.Name;

                    return categoryProductModel;
                });
            });

            return model;
        }

        #endregion
    }
}