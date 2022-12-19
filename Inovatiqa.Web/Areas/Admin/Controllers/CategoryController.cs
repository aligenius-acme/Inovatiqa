using System;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Web.Areas.Admin.Models.Catalog;
using Inovatiqa.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    public partial class CategoryController : BaseAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ICategoryModelFactory _categoryModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;

        #endregion

        #region Ctor

        public CategoryController(IPermissionService permissionService,
            ICategoryModelFactory categoryModelFactory,
            ICategoryService categoryService,
            IBaseAdminModelFactory baseAdminModelFactory,
             IRazorViewEngine viewEngine
             ) : base(viewEngine)
        {
            _permissionService = permissionService;
            _categoryModelFactory = categoryModelFactory;
            _categoryService = categoryService;
            _baseAdminModelFactory = baseAdminModelFactory;
        }

        #endregion

        #region Utilities


        #endregion

        #region List

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var model = _categoryModelFactory.PrepareCategorySearchModel(new CategorySearchModel());

            return View(model);
        }

        public virtual IActionResult PopulateCategoryList()
        {
            ProductSearchModel searchModel = new ProductSearchModel();
            _baseAdminModelFactory.PrepareCategories(searchModel.AvailableCategories);

            return Json(searchModel);
        }

        [HttpPost]
        public virtual IActionResult List(CategorySearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedDataTablesJson();

            var model = _categoryModelFactory.PrepareCategoryListModel(searchModel);

            return Json(model);
        }

        #endregion

        #region Create / Edit / Delete

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var category = _categoryService.GetCategoryById(id);
            if (category == null || category.Deleted)
                return RedirectToAction("List");

            var model = _categoryModelFactory.PrepareCategoryModel(null, category);

            return View(model);
        }

        //commented by hamza because according to ali bhai admin can't update or edit category
        //[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        //public virtual IActionResult Edit(CategoryModel model, bool continueEditing)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
        //        return AccessDeniedView();
        //    var category = _categoryService.GetCategoryById(model.Id);
        //    if (category == null || category.Deleted)
        //    {
        //        return RedirectToAction("List");
        //    }
        //    if (ModelState.IsValid)
        //    {
        //        var prevPictureId = category.PictureId;
        //        category.UpdatedOnUtc = DateTime.UtcNow;
        //    }
        //    var editCategory = new Category
        //    {
        //        Name = model.Name,
        //        Description = model.Description,
        //        ParentCategoryId = model.ParentCategoryId,
        //        PictureId = model.PictureId,
        //        Published = model.Published,
        //        ShowOnHomepage = model.ShowOnHomepage
        //    };
        //    _categoryService.UpdateCategory(editCategory);

        //    return RedirectToAction("List");
        //}
        #endregion

        #region Products

        public virtual IActionResult ProductUpdate(CategoryProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var productCategory = _categoryService.GetProductCategoryById(model.Id)
                ?? throw new ArgumentException("No product category mapping found with the specified id");

            productCategory = model.ToProductCategoryMappingEntity(productCategory);
            _categoryService.UpdateProductCategory(productCategory);

            return new NullJsonResult();
        }

        public virtual IActionResult ProductDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var productCategory = _categoryService.GetProductCategoryById(id)
                ?? throw new ArgumentException("No product category mapping found with the specified id", nameof(id));

            _categoryService.DeleteProductCategory(productCategory);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual IActionResult ProductList(CategoryProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedDataTablesJson();

            var category = _categoryService.GetCategoryById(searchModel.CategoryId)
                ?? throw new ArgumentException("No category found with the specified id");

            var model = _categoryModelFactory.PrepareCategoryProductListModel(searchModel, category);

            return Json(model);
        }

        #endregion
    }
}