using System;
using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Core.Caching;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Caching.Extensions.Interfaces;
using Inovatiqa.Services.Caching.Interfaces;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;

namespace Inovatiqa.Services.Catalog
{
    public partial class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<ProductCategoryMapping> _productCategoryMappingRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ICacheKeyService _cacheKeyService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContextService _workContextService;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductCategoryMapping> _productCategoryRepository;

        public CategoryService(IRepository<Category> categoryRepository,
            IRepository<ProductCategoryMapping> productCategoryMapping,
            IStaticCacheManager staticCacheManager,
            ICacheKeyService cacheKeyService,
            ICustomerService customerService,
            IWorkContextService workContextService,
            IRepository<Product> productRepository,
            IRepository<ProductCategoryMapping> productCategoryRepository)
        {
            _categoryRepository = categoryRepository;
            _productCategoryMappingRepository = productCategoryMapping;
            _staticCacheManager = staticCacheManager;
            _cacheKeyService = cacheKeyService;
            _customerService = customerService;
            _workContextService = workContextService;
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
        }

        public virtual IList<ProductCategoryMapping> GetProductCategoriesByProductId(int productId, bool showHidden = false)
        {
            return GetProductCategoriesByProductId(productId, InovatiqaDefaults.StoreId, showHidden);
        }

        public virtual IList<ProductCategoryMapping> GetProductCategoriesByProductId(int productId, int storeId,
            bool showHidden = false)
        {
            if (productId == 0)
                return new List<ProductCategoryMapping>();

            var key = InovatiqaDefaults.ProductCategoriesAllByProductIdCacheKey;

            var query = from pc in _productCategoryMappingRepository.Query()
                        join c in _categoryRepository.Query() on pc.CategoryId equals c.Id
                        where pc.ProductId == productId &&
                              !c.Deleted &&
                              (showHidden || c.Published)
                        orderby pc.DisplayOrder, pc.Id
                        select pc;

            //return query.ToCachedList(key, _staticCacheManager);
            return query.ToList();
        }

        public virtual IList<Category> GetAllCategories(int storeId = 0, bool showHidden = false)
        {
            var key = _cacheKeyService.PrepareKeyForDefaultCache(InovatiqaDefaults.CategoriesAllCacheKey,
                storeId,
                _customerService.GetCustomerRoleIds(_workContextService.CurrentCustomer),
                showHidden);
            var categories = _staticCacheManager.Get(key, () => GetAllCategories(string.Empty, storeId, showHidden: showHidden).ToList());

            //var categories = GetAllCategories(string.Empty, storeId, showHidden: showHidden).ToList();

            return categories;
        }

        public virtual IPagedList<Category> GetAllCategories(string categoryName, int storeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = _categoryRepository.Query();
            if (!showHidden)
                query = query.Where(c => c.Published);
            if (!string.IsNullOrWhiteSpace(categoryName))
                query = query.Where(c => c.Name.Contains(categoryName));
            query = query.Where(c => !c.Deleted);
            query = query.OrderBy(c => c.ParentCategoryId).ThenBy(c => c.DisplayOrder).ThenBy(c => c.Id);
            if (overridePublished.HasValue)
                query = query.Where(c => c.Published == overridePublished.Value);

            var unsortedCategories = query.ToList();

            var sortedCategories = SortCategoriesForTree(unsortedCategories);

            return new PagedList<Category>(sortedCategories, pageIndex, pageSize);
        }

        public virtual IList<Category> SortCategoriesForTree(IList<Category> source, int parentId = 0,
            bool ignoreCategoriesWithoutExistingParent = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var result = new List<Category>();

            foreach (var cat in source.Where(c => c.ParentCategoryId == parentId).ToList())
            {
                result.Add(cat);
                result.AddRange(SortCategoriesForTree(source, cat.Id, true));
            }

            if (ignoreCategoriesWithoutExistingParent || result.Count == source.Count)
                return result;

            foreach (var cat in source)
                if (result.FirstOrDefault(x => x.Id == cat.Id) == null)
                    result.Add(cat);

            return result;
        }

        public virtual Category GetCategoryById(int categoryId)
        {
            if (categoryId == 0)
                return null;

            return _categoryRepository.ToCachedCategoryGetById(_staticCacheManager, _cacheKeyService, categoryId);
            //return _categoryRepository.GetById(categoryId);
        }

        public virtual IList<Category> GetCategoryBreadCrumb(Category category, IList<Category> allCategories = null, bool showHidden = false)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var result = new List<Category>();

            var alreadyProcessedCategoryIds = new List<int>();

            while (category != null &&
                   !category.Deleted &&
                   (showHidden || category.Published) &&
                   !alreadyProcessedCategoryIds.Contains(category.Id))
            {
                result.Add(category);

                alreadyProcessedCategoryIds.Add(category.Id);

                category = allCategories != null
                    ? allCategories.FirstOrDefault(c => c.Id == category.ParentCategoryId)
                    : GetCategoryById(category.ParentCategoryId);
            }

            result.Reverse();
            return result;
        }

        public virtual IList<int> GetChildCategoryIds(int parentCategoryId, int storeId = 0, bool showHidden = false)
        {

            //var categoriesIds = new List<int>();
            //var categories = GetAllCategories(storeId: storeId, showHidden: showHidden)
            //    .Where(c => c.ParentCategoryId == parentCategoryId)
            //    .Select(c => c.Id)
            //    .ToList();
            //categoriesIds.AddRange(categories);
            //categoriesIds.AddRange(categories.SelectMany(cId => GetChildCategoryIds(cId, storeId, showHidden)));

            var cacheKey = InovatiqaDefaults.CategoriesChildIdentifiersCacheKey;

            return _staticCacheManager.Get(cacheKey, () =>
            {
                var categoriesIds = new List<int>();
                var categories = GetAllCategories(storeId: storeId, showHidden: showHidden)
                    .Where(c => c.ParentCategoryId == parentCategoryId)
                    .Select(c => c.Id)
                    .ToList();
                categoriesIds.AddRange(categories);
                categoriesIds.AddRange(categories.SelectMany(cId => GetChildCategoryIds(cId, storeId, showHidden)));

                return categoriesIds;
            });

            //return categoriesIds;
        }

        public virtual IList<Category> GetAllCategoriesDisplayedOnHomepage(bool showHidden = false)
        {
            var query = from c in _categoryRepository.Query()
                        orderby c.DisplayOrder, c.Id
                        where c.Published &&
                        !c.Deleted &&
                        c.ShowOnHomepage
                        select c;

            var categories = query.ToList();

            return categories;

        }

        public virtual string GetFormattedBreadCrumb(Category category, IList<Category> allCategories = null,
            string separator = ">>", int languageId = 0)
        {
            var result = string.Empty;

            var breadcrumb = GetCategoryBreadCrumb(category, allCategories, true);
            for (var i = 0; i <= breadcrumb.Count - 1; i++)
            {
                var categoryName = breadcrumb[i].Name;
                result = string.IsNullOrEmpty(result) ? categoryName : $"{result} {separator} {categoryName}";
            }

            return result;
        }

        public virtual IPagedList<ProductCategoryMapping> GetProductCategoriesByCategoryId(int categoryId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            if (categoryId == 0)
                return new PagedList<ProductCategoryMapping>(new List<ProductCategoryMapping>(), pageIndex, pageSize);

            var query = from pc in _productCategoryMappingRepository.Query()
                        join p in _productRepository.Query() on pc.ProductId equals p.Id
                        where pc.CategoryId == categoryId &&
                              !p.Deleted &&
                              (showHidden || p.Published)
                        orderby pc.DisplayOrder, pc.Id
                        select pc;

           
            var productCategories = new PagedList<ProductCategoryMapping>(query, pageIndex, pageSize);

            return productCategories;
        }

        public virtual ProductCategoryMapping GetProductCategoryById(int productCategoryId)
        {
            if (productCategoryId == 0)
                return null;
            return _productCategoryRepository.GetById(productCategoryId);
        }

        public virtual void UpdateProductCategory(ProductCategoryMapping productCategory)
        {
            if (productCategory == null)
                throw new ArgumentNullException(nameof(productCategory));

            _productCategoryRepository.Update(productCategory);

            //event notification
            //_eventPublisher.EntityUpdated(productCategory);
        }

        //category Update by hamza
        //public virtual void UpdateCategory(Category category)
        //{
        //    if (category == null)
        //        throw new ArgumentNullException(nameof(category));
        //    _categoryRepository.Update(category);
        //}

        public virtual void DeleteProductCategory(ProductCategoryMapping productCategory)
        {
            if (productCategory == null)
                throw new ArgumentNullException(nameof(productCategory));

            _productCategoryRepository.Delete(productCategory);

            //event notification
            //_eventPublisher.EntityDeleted(productCategory);
        }

        public virtual ProductCategoryMapping FindProductCategory(IList<ProductCategoryMapping> source, int productId, int categoryId)
        {
            foreach (var productCategory in source)
                if (productCategory.ProductId == productId && productCategory.CategoryId == categoryId)
                    return productCategory;

            return null;
        }

        public virtual void InsertProductCategory(ProductCategoryMapping productCategory)
        {
            if (productCategory == null)
                throw new ArgumentNullException(nameof(productCategory));

            _productCategoryRepository.Insert(productCategory);

            //event notification
            //_eventPublisher.EntityInserted(productCategory);
        }
        public List<Category> GetParentCategories()
        {
           return _categoryRepository.Query().Where(cat => cat.ParentCategoryId == 0).ToList();
        }
        public List<Category> GetChildCategories(int CategoryId)
        {
           var model = _categoryRepository.Query().Where(cat => cat.ParentCategoryId == CategoryId).ToList();
           return model;
        }
    }
}