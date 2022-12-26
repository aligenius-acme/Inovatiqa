using System;
using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Web.Models.Catalog;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Media.Interfaces;
using Inovatiqa.Web.Models.Media;
using Inovatiqa.Core.Interfaces;
using Nest;
using Inovatiqa.Web.Models;
using Inovatiqa.Services.Logging.Interfaces;
using static Inovatiqa.Web.Models.Catalog.CategorySearchModel;
using Inovatiqa.Services.WorkContext.Interfaces;

namespace Inovatiqa.Web.Factories
{
    public partial class CatalogModelFactory : ICatalogModelFactory
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly IElasticClient _client;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ILoggerService _loggerService;
        private readonly IWorkContextService _workContextService;
        private readonly ICompareProductsService _compareProductsService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        protected readonly Database.Interfaces.IRepository<EntityTierPrice> _entityTierPriceRepository;


        #endregion

        #region Ctor

        public CatalogModelFactory(ICategoryService categoryService,
            IUrlRecordService urlRecordService,
            IProductService productService,
            IProductModelFactory productModelFactory,
            IManufacturerService manufacturerService,
            IPictureService pictureService,
            IElasticClient elasticClient,
            IProductAttributeService productAttributeService,
            ILoggerService loggerService,
            IWorkContextService workContextService,
            ICompareProductsService compareProductsService,
            IPriceFormatter priceFormatter,
            IPriceCalculationService priceCalculationService,
            Database.Interfaces.IRepository<EntityTierPrice> entityTierPriceRepository)
        {
            _categoryService = categoryService;
            _urlRecordService = urlRecordService;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _manufacturerService = manufacturerService;
            _pictureService = pictureService;
            _client = elasticClient;
            _productAttributeService = productAttributeService;
            _loggerService = loggerService;
            _workContextService = workContextService;
            _compareProductsService = compareProductsService;
            _priceFormatter = priceFormatter;
            _priceCalculationService = priceCalculationService;
            _entityTierPriceRepository = entityTierPriceRepository;
        }

        #endregion

        #region Categories

        public virtual CategoryNavigationModel PrepareCategoryNavigationModel(int currentCategoryId, int currentProductId)
        {
            var activeCategoryId = 0;
            if (currentCategoryId > 0)
            {
                activeCategoryId = currentCategoryId;
            }
            else if (currentProductId > 0)
            {
                var productCategories = _categoryService.GetProductCategoriesByProductId(currentProductId);
                if (productCategories.Any())
                    activeCategoryId = productCategories[0].CategoryId;
            }
            Category _cat = _categoryService.GetCategoryById(activeCategoryId);
            var categoriesModel = PrepareCategorySimpleModels(activeCategoryId);
            var model = new CategoryNavigationModel
            {
                CurrentCategoryId = activeCategoryId,
                CurrentCategoryName = _cat != null ? _cat.Name : "",
                Categories = categoriesModel
            };

            return model;
        }

        public virtual List<CategorySimpleModel> PrepareCategorySimpleModels(int rootCategoryId, bool loadSubCategories = true)
        {
            var result = new List<CategorySimpleModel>();

            var allCategories = _categoryService.GetAllCategories(storeId: InovatiqaDefaults.StoreId);

            var categories = allCategories.Where(c => c.ParentCategoryId == rootCategoryId).OrderBy(c => c.DisplayOrder).ToList();

            foreach (var category in categories)
            {
                int totalProductCount = 0;
                if (!string.IsNullOrEmpty(category.ProductCount.ToString()))
                    totalProductCount = int.Parse(category.ProductCount.ToString());
                var categoryModel = new CategorySimpleModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    SeName = _urlRecordService.GetActiveSlug(category.Id, InovatiqaDefaults.CategorySlugName, InovatiqaDefaults.LanguageId),
                    IncludeInTopMenu = false,
                    NumberOfProducts = totalProductCount
                };

                ////////////////if (category.ParentCategoryId != 0)
                ////////////////{
                ////////////////    var categoryIds = new List<int> { category.Id };
                ////////////////    categoryIds.AddRange(
                ////////////////            _categoryService.GetChildCategoryIds(category.Id, InovatiqaDefaults.StoreId));
                ////////////////    categoryModel.NumberOfProducts =
                ////////////////        _productService.GetNumberOfProductsInCategory(categoryIds, InovatiqaDefaults.StoreId);
                ////////////////}

                ////////////////if (loadSubCategories)
                ////////////////{
                ////////////////    var subCategories = PrepareCategorySimpleModels(category.Id);
                ////////////////    categoryModel.SubCategories.AddRange(subCategories);
                ////////////////}

                ////////////////categoryModel.HaveSubCategories = categoryModel.SubCategories.Count > 0;

                result.Add(categoryModel);
            }

            return result;
        }

        public virtual CategoryModel PrepareCategoryModel(Category category, CatalogPagingFilteringModel command, int minPrice = 0, int maxPrice = 0)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));
            var CC = _categoryService.GetChildCategories(category.Id);

            var model = new CategoryModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                MetaKeywords = category.MetaKeywords,
                MetaDescription = category.MetaDescription,
                MetaTitle = category.MetaTitle,
                SeName = _urlRecordService.GetActiveSlug(category.Id, InovatiqaDefaults.CategorySlugName, InovatiqaDefaults.LanguageId)
            };
            foreach(var childCategory in CC)
            {
                model.childCategoriesLinks.Add(new CategoryModel
                {
                    Name = childCategory.Name,
                    SeName = _urlRecordService.GetActiveSlug(childCategory.Id, InovatiqaDefaults.CategorySlugName, InovatiqaDefaults.LanguageId),
                    PictureModel = new PictureModel
                    {
                        FullSizeImageUrl = childCategory.PictureId > 0 ? _pictureService.GetPictureUrl(childCategory.PictureId) : _pictureService.GetDefaultPictureUrl()
                    }
                }); 
            }
            PreparePageSizeOptions(model.PagingFilteringContext, command,
                category.AllowCustomersToSelectPageSize,
                category.PageSizeOptions,
                category.PageSize);

            model.DisplayCategoryBreadcrumb = true;

            model.CategoryBreadcrumb = _categoryService.GetCategoryBreadCrumb(category).Select(catBr =>
                new CategoryModel
                {
                    Id = catBr.Id,
                    Name = catBr.Name,
                    SeName = _urlRecordService.GetActiveSlug(catBr.Id, InovatiqaDefaults.CategorySlugName, InovatiqaDefaults.LanguageId) 
                }).ToList();

            var pictureSize = InovatiqaDefaults.CategoryThumbPictureSize;

            var categoryIds = new List<int> { category.Id };
            Nullable<int> min = null;
            Nullable<int> max = null;

            if (minPrice != 0)
                min = minPrice;
            if (maxPrice != 0)
                max = maxPrice;

            var products = _productService.SearchProducts(out var filterableSpecificationAttributeOptionIds,
                true,
                categoryIds: categoryIds,
                storeId: InovatiqaDefaults.StoreId,
                visibleIndividuallyOnly: true,
                featuredProducts: false,
                priceMin: min,
                priceMax: max,
                filteredSpecs: null,
                orderBy: ProductSortingEnum.CreatedOn,
                pageIndex: command.PageNumber - 1,
                pageSize: command.PageSize);


            model.Products = _productModelFactory.PrepareProductOverviewModels(products, prepareProductAttributes: true).ToList();

            model.PagingFilteringContext.LoadPagedList(products);

            return model;
        }

        public virtual void PreparePageSizeOptions(CatalogPagingFilteringModel pagingFilteringModel, CatalogPagingFilteringModel command,
            bool allowCustomersToSelectPageSize, string pageSizeOptions, int fixedPageSize)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException(nameof(pagingFilteringModel));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (command.PageNumber <= 0)
                command.PageNumber = 1;

            command.PageSize = fixedPageSize;
        }

        public virtual List<CategoryModel> PrepareHomepageCategoryModels()
        {
            List<CategoryModel> model = new List<CategoryModel>();

            var pictureSize = InovatiqaDefaults.CategoryThumbPictureSize;

            var categories = _categoryService.GetAllCategoriesDisplayedOnHomepage();

            if (categories != null && categories.Count > 0)
            {
                foreach (var category in categories)
                {
                    var catModel = new CategoryModel
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Description = category.Description,
                        MetaKeywords = category.MetaKeywords,
                        MetaDescription = category.MetaDescription,
                        MetaTitle = category.MetaTitle,
                        SeName = _urlRecordService.GetActiveSlug(category.Id, InovatiqaDefaults.CategorySlugName, InovatiqaDefaults.LanguageId),
                        ChildCategories = _categoryService.GetChildCategories(category.Id).Take(4).ToList()
                    };
                    foreach(var cc in catModel.ChildCategories)
                    {
                        catModel.childCategoriesLinks.Add(new CategoryModel
                        {
                            SeName = _urlRecordService.GetActiveSlug(cc.Id, InovatiqaDefaults.CategorySlugName, InovatiqaDefaults.LanguageId),
                            Name = cc.Name
                        });
                    }

                    var picture = _pictureService.GetPictureById(category.PictureId);
                    var pictureModel = new PictureModel
                    {
                        FullSizeImageUrl = _pictureService.GetPictureUrl(ref picture),
                        ImageUrl = _pictureService.GetPictureUrl(ref picture, pictureSize),
                        Title = string.Format(
                            "Show products in category {0}",
                            catModel.Name),
                        AlternateText =
                            string.Format(
                                "Picture for category {0}",
                                catModel.Name)
                    };

                    catModel.PictureModel = pictureModel;
                    model.Add(catModel);
                }
            }

            return model;
        }

        #endregion

        #region Searching

        public virtual SearchBoxModel PrepareSearchBoxModel()
        {
            var model = new SearchBoxModel
            {
                //By Naveed
                AutoCompleteEnabled = InovatiqaDefaults.ProductSearchAutoCompleteEnabled,
                ShowProductImagesInSearchAutoComplete = InovatiqaDefaults.ShowProductImagesInSearchAutoComplete,
                SearchTermMinimumLength = InovatiqaDefaults.ProductSearchTermMinimumLength,
                ShowSearchBox = InovatiqaDefaults.ProductSearchEnabled
            };
            var categories = _categoryService.GetParentCategories();
            model.Categories = categories.Select(cat => new KeyValuePair<int, string>(cat.Id, cat.Name)).ToList();
            return model;
        }

        public virtual List<ManufacturerSearchModel> PrepareProductManufacturerSearchModel(CatalogPagingFilteringModel command, string term, int categoryId, List<string> catSearchFilter, List<string> attSearchFilter, List<string> manList, int pageSize, int minPrice, int maxPrice)
        {
            var model = new List<ManufacturerSearchModel>();

            Nullable<int> min = null;
            Nullable<int> max = null;
            catSearchFilter = catSearchFilter ?? new List<string>();
            manList = manList ?? new List<string>();
            attSearchFilter = attSearchFilter ?? new List<string>();


            if (categoryId > 0)
            {
                catSearchFilter.Insert(0, Convert.ToString(categoryId));
            }
            var lastAtt = attSearchFilter.LastOrDefault();
            var lastcat = catSearchFilter.LastOrDefault();
            if (lastcat != null)
            {
                List<string> newCatSearchFilter = new List<string>();
                foreach (var cat in catSearchFilter)
                {
                    if (cat == lastcat)
                    {
                        newCatSearchFilter.Add(cat);
                        break;
                    }
                    else
                        newCatSearchFilter.Add(cat);
                }
                catSearchFilter = newCatSearchFilter;
            }
            if(lastAtt != null)
            {
                List<string> newAttSearchFilter = new List<string>();
                foreach(var att in attSearchFilter)
                {
                    if (att == lastAtt)
                    {
                        newAttSearchFilter.Add(att);
                        break;
                    }
                    else
                        newAttSearchFilter.Add(att);
                }
                attSearchFilter = newAttSearchFilter;
            }
            if (minPrice != 0)
                min = minPrice;
            if (maxPrice != 0)
                max = maxPrice;

            //var attribute = new List<string> { "5169", "5170" };

            //get manufacturers
            var elasticProductManufacturers = _client.Search<ProductOverviewModel>(s => s
                   .Query(q => q
                       .QueryString(d => d
                           .Query(term)
                       ) && q
                       .QueryString(m => m
                           .Query(string.Join(" AND ", catSearchFilter.ToList()))
                           .DefaultField(f => f.ProductCategories.Select(s => s.Id))
                       ) && q
                        .QueryString(at => at
                            .Query(String.Join(" OR ", attSearchFilter.ToList()))
                            .DefaultField(f => f.ProductAttributes.Select(s => s.Values.Select(s => s.Id)))
                        ) && q
                       .Range(r => r
                           .Field(f => f.ProductPrice.PriceValue)
                           .GreaterThanOrEquals(minPrice)
                           .LessThanOrEquals(maxPrice > 0 ? maxPrice : Int32.MaxValue)
                       )

                   )
                   .Aggregations(agg => agg
                        .Terms("Brands", tm => tm
                            .Field(f => f.ProductManufacturers.Select(s => s.Id))
                            .Size(10000)
                        )
                   )
                   .Size(6)
               );

            var manuList = new List<string>();
            var manufacturerCount = new List<KeyValuePair<string, string>>();
            try
            {
                BucketAggregate manBucket = (BucketAggregate)elasticProductManufacturers.Aggregations.Values.First();
                foreach (Nest.KeyedBucket<System.Object> man in manBucket.Items)
                {
                    manuList.Add(Convert.ToString(man.Key));
                    manufacturerCount.Add(new KeyValuePair<string, string>(Convert.ToString(man.Key), Convert.ToString(man.DocCount)));
                }
                manuList.Remove("0");
            }
            catch { }
            var url = InovatiqaDefaults.ElasticEndPoint;
            var settings = new ConnectionSettings(new Uri(url))
                .DefaultIndex("manufacturers")
                .BasicAuthentication(InovatiqaDefaults.ElasticUsername, InovatiqaDefaults.ElasticPassword);
            var clientManufacturers = new ElasticClient(settings);
            var elasticManufacturers = clientManufacturers.Search<ManufacturerBriefInfoModel>(s => s
                .Query(q => q
                    .QueryString(d => d
                        .Query(String.Join(" OR ", manuList.Take(1024).ToList()))
                        .DefaultField(f => f.Id)
                    )
                )
                .Size(manuList.Count())
            );
            foreach (var man in elasticManufacturers.Documents)
            {
                var tempList = manufacturerCount.Select(s => s.Key).ToList();
                if (tempList.Contains(Convert.ToString(man.Id)))
                {
                    model.Add(new ManufacturerSearchModel
                    {
                        Id = Convert.ToString(man.Id),
                        Name = man.Name,
                        IsActive = man.IsActive,
                        IsSelected = manList.Contains(man.Id.ToString()),
                        ManufacturersCount = manufacturerCount[tempList.IndexOf(Convert.ToString(man.Id))].Value
                    });
                }
            }
            var khdfsk = String.Join(" OR ", manuList);
            model = model.OrderBy(mm => mm.Name).ToList();
            return model;
        }

        public virtual CategorySearchModel PrepareProductCategorySearchModel(CatalogPagingFilteringModel command, string term, int categoryId, List<string> catSearchFilter, List<string> attSearchFilter, List<string> manList, int pageSize, int minPrice, int maxPrice)
        {
            var model = new CategorySearchModel();

            int searchedParentCategoryCount = InovatiqaDefaults.searchedParentCategoryCount + 1;

            Nullable<int> min = null;
            Nullable<int> max = null;
            catSearchFilter = catSearchFilter ?? new List<string>();
            manList = manList ?? new List<string>();
            attSearchFilter = attSearchFilter ?? new List<string>();
            if (catSearchFilter.Count == 0 && categoryId == -1)
            {
                model.IsCatAvailable = -1;
            }
            else if (catSearchFilter.Count == 0 && categoryId > 0)
            {
                model.IsCatAvailable = -1;
            }
            else
            {
                model.IsCatAvailable = 1;
            }
            if (categoryId > 0)
            {
                catSearchFilter.Insert(0, Convert.ToString(categoryId));
            }
            var lastAtt = attSearchFilter.LastOrDefault();
            if(attSearchFilter.Count == 0)
            {
                model.IsAttAvailable = -1;
            }
            else
            {
                model.IsAttAvailable = 1;
            }
            var lastcat = catSearchFilter.LastOrDefault();
            if (lastcat != null)
            {
                List<string> newCatSearchFilter = new List<string>();
                foreach (var cat in catSearchFilter)
                {
                    if (cat == lastcat)
                    {
                        newCatSearchFilter.Add(cat);
                        break;
                    }
                    else
                        newCatSearchFilter.Add(cat);
                }
                catSearchFilter = newCatSearchFilter;
            }
            model.searchedCategories = catSearchFilter;

            if (lastAtt != null)
            {
                List<string> newAttSearchFilter = new List<string>();
                foreach (var att in attSearchFilter)
                {
                    if (att == lastAtt)
                    {
                        newAttSearchFilter.Add(att);
                        break;
                    }
                    else
                        newAttSearchFilter.Add(att);
                }
                attSearchFilter = newAttSearchFilter;
            }
            model.searchedAttributes = attSearchFilter;
            var checkSearchedAttributes = new List<string>();
            foreach (var attrr in attSearchFilter)
            {
                attrr.Replace(" OR ", ",");
                var checkSearchAttList = attrr.Replace(" OR ", ",").Split(',').ToList();
                checkSearchedAttributes.AddRange(checkSearchAttList);
            }

            //foreach (var cat in catSearchFilter)
            //{
            //    var cta = _categoryService.GetCategoryById(Convert.ToInt32(cat));
            //    model.searchedCategoriesName.Add(cta.Name);
            //}
            //catSearchFilter.GroupBy(c => c).ToList();
            if (minPrice != 0)
                min = minPrice;
            if (maxPrice != 0)
                max = maxPrice;

            //var attribute = new List<string> { "5169", "5170" };



            //get Categories

            //by hamza for elastic categories aggregations
            if(attSearchFilter.Count == 0)
            {
                model.NoFurtherAttribut = -1;
                var elasticProductsCategory = _client.Search<ProductOverviewModel>(s => s
                     .Query(q => q
                         .QueryString(d => d
                             .Query(term)
                         ) && q
                         .QueryString(m => m
                             .Query(string.Join(" OR ", manList.ToList()))
                             .DefaultField(f => f.ProductManufacturers.Select(s => s.Id))
                         )
                         && q
                         .QueryString(m => m
                             .Query(string.Join(" AND ", catSearchFilter.ToList()))
                             .DefaultField(f => f.ProductCategories.Select(s => s.Id))
                         ) && q
                        .QueryString(at => at
                            .Query(String.Join(" OR ", attSearchFilter.ToList()))
                            .DefaultField(f => f.ProductAttributes.Select(s => s.Values.Select(s => s.Id)))
                        ) && q
                         .Range(r => r
                             .Field(f => f.ProductPrice.PriceValue)
                             .GreaterThanOrEquals(minPrice)
                             .LessThanOrEquals(maxPrice > 0 ? maxPrice : Int32.MaxValue)
                         )

                     )
                     .Aggregations(agg => agg
                          .Terms("Categories", tm => tm
                              .Field(f => f.ProductCategories.Select(s => s.ParentCategoriesId))
                              .Size(10000)
                          )
                     )
                     .Size(6)
                 );


                var catList = new List<string>();
                var categoryCount = new List<KeyValuePair<string, string>>();
                try
                {
                    BucketAggregate catBucket = (BucketAggregate)elasticProductsCategory.Aggregations.Values.First();
                    foreach (Nest.KeyedBucket<System.Object> cat in catBucket.Items)
                    {
                        //catList.Add(Convert.ToString(cat.Key));
                        categoryCount.Add(new KeyValuePair<string, string>(Convert.ToString(cat.Key), Convert.ToString(cat.DocCount)));
                    }
                    categoryCount.OrderByDescending(cc => Convert.ToInt32(cc.Value));
                    catList = categoryCount.Select(c => c.Key).Take(searchedParentCategoryCount).ToList();
                    catList.Remove("0");
                }
                catch { }
                //for child categories

                var elasticProductsChildCategory = _client.Search<ProductOverviewModel>(s => s
                    .Query(q => q
                        .QueryString(d => d
                            .Query(term)
                        ) && q
                        .QueryString(m => m
                            .Query(string.Join(" OR ", manList.ToList()))
                            .DefaultField(f => f.ProductManufacturers.Select(s => s.Id))
                        )
                        && q
                        .QueryString(m => m
                            .Query(string.Join(" AND ", catSearchFilter.ToList()))
                            .DefaultField(f => f.ProductCategories.Select(s => s.Id))
                        ) && q
                        .QueryString(at => at
                            .Query(String.Join(" OR ", attSearchFilter.ToList()))
                            .DefaultField(f => f.ProductAttributes.Select(s => s.Values.Select(s => s.Id)))
                        )
                        && q
                        .Range(r => r
                            .Field(f => f.ProductPrice.PriceValue)
                            .GreaterThanOrEquals(minPrice)
                            .LessThanOrEquals(maxPrice > 0 ? maxPrice : Int32.MaxValue)
                        )

                    )
                    .Aggregations(agg => agg
                         .Terms("Categories", tm => tm
                             .Field(f => f.ProductCategories.Select(s => s.Id))
                             .Size(10000)
                         )
                    )
                    .Size(6)
                );
                var childCatList = new List<string>();
                var childCategoryCount = new List<KeyValuePair<string, string>>();
                try
                {
                    BucketAggregate childCategoryBucket = (BucketAggregate)elasticProductsChildCategory.Aggregations.Values.First();
                    foreach (Nest.KeyedBucket<System.Object> cat in childCategoryBucket.Items)
                    {
                        childCatList.Add(Convert.ToString(cat.Key));
                        childCategoryCount.Add(new KeyValuePair<string, string>(Convert.ToString(cat.Key), Convert.ToString(cat.DocCount)));
                    }
                    model.ChildCategoryCount = childCategoryCount;
                }
                catch { }


                //for category details
                var url1 = InovatiqaDefaults.ElasticEndPoint;
                var settings1 = new ConnectionSettings(new Uri(url1))
                    .DefaultIndex("categories")
                    .BasicAuthentication(InovatiqaDefaults.ElasticUsername, InovatiqaDefaults.ElasticPassword);
                var clientCategories = new ElasticClient(settings1);
                var elasticCategories = clientCategories.Search<CategoryModel>(s => s
                    .Query(q => q
                        .QueryString(d => d
                            .Query(String.Join(" OR ", catList.Take(1024).ToList()))
                            .DefaultField(f => f.Id)
                        )
                    )
                    .Source(sc => sc
                        .Includes(ic => ic
                            .Field(f => f.Id)
                            .Field(f => f.Name)
                            .Field(f => f.ParentCategoriesId)
                            .Field(f => f.ChildCategories.Select(sf => sf.Id))
                            .Field(f => f.ChildCategories.Select(sf => sf.Name))
                            .Field(f => f.ChildCategories.Select(sf => sf.ParentCategoryId))
                        )
                    )
                    .Size(catList.Count())
                );
                foreach (var category in elasticCategories.Documents)
                {
                    var tempList = categoryCount.Select(s => s.Key).ToList();
                    if (tempList.Contains(Convert.ToString(category.Id)))
                    {
                        if(!(category.ChildCategories.Where(ch => catSearchFilter.Contains(ch.Id.ToString()))).Any())
                        {
                            model.Categories.Add(new ParentCategoriesModel
                            {
                                Id = Convert.ToString(category.Id),
                                Name = category.Name,
                                ParentCategoryId = category.ParentCategoriesId,
                                CategoryCount = categoryCount[tempList.IndexOf(Convert.ToString(category.Id))].Value,
                                ChildCategories = category.ChildCategories.Where(c => childCatList.Contains(c.Id.ToString())).Select(s => new ChildCategoryModel
                                {
                                    Id = s.Id,
                                    Name = s.Name,
                                    ChildCategoriesCount = childCategoryCount.Where(cc => cc.Key == s.Id.ToString()).First().Value
                                }).ToList()
                            });
                        }
                    }
                }
                if(model.Categories.Count() == 0)
                {
                    model.NoFurtherAttribut = 1;
                }

            }
            var attributeModel = new List<AttributesModel>();
            if (catSearchFilter.Any() && model.Categories.Count() == 0)
            {
                model.NoFurtherChild = -1;
                model.NoFurtherAttribut = 1;
                //get attributes
                var elasticProductsAttributes = _client.Search<ProductOverviewModel>(s => s
                     .Query(q => q
                         .QueryString(d => d
                             .Query(term)
                         ) && q
                         .QueryString(m => m
                             .Query(string.Join(" OR ", manList.ToList()))
                             .DefaultField(f => f.ProductManufacturers.Select(s => s.Id))
                         )
                         && q
                         .QueryString(m => m
                             .Query(string.Join(" AND ", catSearchFilter.ToList()))
                             .DefaultField(f => f.ProductCategories.Select(s => s.Id))
                         ) && q
                         .QueryString(at => at
                             .Query(String.Join(" OR ", attSearchFilter.ToList()))
                             .DefaultField(f => f.ProductAttributes.Select(s => s.Values.Select(s => s.Id)))
                         ) && q
                         .Range(r => r
                             .Field(f => f.ProductPrice.PriceValue)
                             .GreaterThanOrEquals(minPrice)
                             .LessThanOrEquals(maxPrice > 0 ? maxPrice : Int32.MaxValue)
                         )

                    )
                    .Aggregations(agg => agg
                        .Terms("Attributes", tm => tm
                            .Field(f => f.ProductAttributes.Select(s => s.Id))
                            .Size(10000)
                        )
                    )
                    .Size(6)
                );


                var attList = new List<string>();
                var attributeCount = new List<KeyValuePair<string, string>>();
                try
                {
                    BucketAggregate attBucket = (BucketAggregate)elasticProductsAttributes.Aggregations.Values.First();
                    foreach (Nest.KeyedBucket<System.Object> cat in attBucket.Items)
                    {
                        attList.Add(Convert.ToString(cat.Key));
                        attributeCount.Add(new KeyValuePair<string, string>(Convert.ToString(cat.Key), Convert.ToString(cat.DocCount)));
                    }
                }
                catch { }

                //get attribute values
                var elasticProductsAttributesValues = _client.Search<ProductOverviewModel>(s => s
               .Query(q => q
                   .QueryString(d => d
                       .Query(term)
                   ) && q
                   .QueryString(m => m
                       .Query(string.Join(" OR ", manList.ToList()))
                       .DefaultField(f => f.ProductManufacturers.Select(s => s.Id))
                   )
                   && q
                   .QueryString(m => m
                       .Query(string.Join(" AND ", catSearchFilter.ToList()))
                       .DefaultField(f => f.ProductCategories.Select(s => s.Id))
                   ) && q
                   .QueryString(at => at
                       .Query(String.Join(" OR ", attSearchFilter.ToList()))
                       .DefaultField(f => f.ProductAttributes.Select(s => s.Values.Select(s => s.Id)))
                   )
                   && q
                   .Range(r => r
                       .Field(f => f.ProductPrice.PriceValue)
                       .GreaterThanOrEquals(minPrice)
                       .LessThanOrEquals(maxPrice > 0 ? maxPrice : Int32.MaxValue)
                   )

               )
               .Aggregations(agg => agg
                    .Terms("Attributs", tm => tm
                        .Field(f => f.ProductAttributes.Select(s => s.Values.Select(s => s.Id)))
                        .Size(10000)
                    )
               )
               .Size(6)
           );
                var attValuesList = new List<string>();
                var attributesvaluesCount = new List<KeyValuePair<string, string>>();
                try
                {
                    BucketAggregate attributesValuesBucket = (BucketAggregate)elasticProductsAttributesValues.Aggregations.Values.First();
                    foreach (Nest.KeyedBucket<System.Object> cat in attributesValuesBucket.Items)
                    {
                        attValuesList.Add(Convert.ToString(cat.Key));
                        attributesvaluesCount.Add(new KeyValuePair<string, string>(Convert.ToString(cat.Key), Convert.ToString(cat.DocCount)));
                    }
                    model.AttributesValueCount = attributesvaluesCount;
                }
                catch { }
                //get attributes model
                var url2 = InovatiqaDefaults.ElasticEndPoint;
                var settings2 = new ConnectionSettings(new Uri(url2))
                    .DefaultIndex("attributes")
                    .BasicAuthentication(InovatiqaDefaults.ElasticUsername, InovatiqaDefaults.ElasticPassword);
                var clientAttributes = new ElasticClient(settings2);
                var elasticAttributes = clientAttributes.Search<ProductDetailsModel.ProductAttributeModel>(s => s
                    .Query(q => q
                        .QueryString(d => d
                            .Query(String.Join(" OR ", attList.Take(1024).ToList()))
                            .DefaultField(f => f.Id)
                        )
                    )
                    .Size(attList.Count())
                );
                
                foreach (var attributes in elasticAttributes.Documents)
                {
                    var temp = attributeModel.Where(attr => attr.Name == attributes.Name);
                    if (temp.Any())
                    {
                        if (!(attributes.Values.Where(atv => checkSearchedAttributes.Contains(atv.Id.ToString())).Any()))
                        {
                            int index = attributeModel.IndexOf(temp.First());
                            attributeModel[index].AttributesValues.AddRange(attributes.Values.Where(a => attValuesList.Contains(a.Id.ToString())).Select(s => new ProductDetailsModel.ProductAttributeValueModel
                            {
                                Id = s.Id,
                                Name = s.Name.Replace("\r", "").Replace("\n", "").Replace(" ", ""),
                                AttributesValuesCount = attributesvaluesCount.Where(av => av.Key == s.Id.ToString()).First().Value
                            }).ToList());
                        }
                    }
                    else
                    {
                        var tempList = attributeCount.Select(s => s.Key).ToList();
                        if (tempList.Contains(Convert.ToString(attributes.Id)))
                        {
                            if (!(attributes.Values.Where(atv => checkSearchedAttributes.Contains(atv.Id.ToString())).Any()))
                            {
                                attributeModel.Add(new AttributesModel
                                {
                                    Id = Convert.ToString(attributes.Id),
                                    Name = attributes.Name,
                                    AttributesValues = attributes.Values.Where(a => attValuesList.Contains(a.Id.ToString())).Select(s => new ProductDetailsModel.ProductAttributeValueModel
                                    {
                                        Id = s.Id,
                                        Name = s.Name.Replace("\r", "").Replace("\n", "").Replace(" ", ""),
                                        AttributesValuesCount = attributesvaluesCount.Where(av => av.Key == s.Id.ToString()).First().Value
                                    }).ToList()
                                });
                            }
                        }
                    }
                }
                if (attributeModel.Count == 0)
                {
                    model.NoFurtherAttribut = -1;
                }
            }
            if(model.NoFurtherAttribut == 1 && model.NoFurtherChild == 1)
            {
                model.NoFurtherChild = -1;
                model.NoFurtherAttribut = -1;
            }
            //var attributesValueList = attributeModel.GroupBy(sm => sm.AttributesValues.GroupBy(g => g.Name));
            if(attributeModel.Count > 0)
            {
                foreach (var attribute in attributeModel)
                {
                    var attributesValueList = attribute.AttributesValues.GroupBy(g => g.Name);
                    var p = new List<Tuple<string, string, string>>();
                    foreach (var attributeValue in attributesValueList)
                    {
                        var ids = string.Join(" OR ", attributeValue.Select(aa => aa.Id));
                        var count = attributeValue.Sum(aaa => Convert.ToInt32(aaa.AttributesValuesCount));
                        p.Add(new Tuple<string, string, string>(attributeValue.Key, ids, Convert.ToString(count)));
                    }
                    model.Attributes.Add(new AttributesModel
                    {
                        Id = attribute.Id,
                        Name = attribute.Name,
                        AttributesValues = p.Select(ss => new ProductDetailsModel.ProductAttributeValueModel
                        {
                            Ids = ss.Item2,
                            Name = ss.Item1,
                            AttributesValuesCount = ss.Item3
                        }).ToList()
                    });
                }
            }
            model.CategoriesCount = model.Categories.Count();
            return model;
        }

        public virtual ProductSearchModel PrepareProductSearchModel(CatalogPagingFilteringModel command, string term, int categoryId, List<string> catSearchFilter, List<string> attSearchFilter, List<string> manList, int pageSize, int minPrice, int maxPrice)
        {
            var model = new ProductSearchModel();
            
            PreparePageSizeOptions(model.PagingFilteringContext, command,
              InovatiqaDefaults.AllowCustomersToSelectPageSize,
              InovatiqaDefaults.PageSizeOptions,
              InovatiqaDefaults.PageSize);

            command.PageSize = pageSize;
            
            Nullable<int> min = null;
            Nullable<int> max = null;
            catSearchFilter = catSearchFilter ?? new List<string>();
            manList = manList ?? new List<string>();
            attSearchFilter = attSearchFilter ?? new List<string>();
            

           

            if (catSearchFilter.Count == 0 && categoryId == -1)
            {
                model.IsCatAvailable = -1;
            }
            else if (catSearchFilter.Count == 0 && categoryId > 0)
            {
                model.IsCatAvailable = -1;
            }
            else
            {
                model.IsCatAvailable = 1;
            }
            if (categoryId > 0)
            {
                catSearchFilter.Insert(0, Convert.ToString(categoryId));
            }
            var lastAtt = attSearchFilter.LastOrDefault();
            if (attSearchFilter.Count == 0)
            {
                model.IsAttAvailable = -1;
            }
            else
            {
                model.IsAttAvailable = 1;
            }
            var lastcat = catSearchFilter.LastOrDefault();
            if (lastcat != null)
            {
                List<string> newCatSearchFilter = new List<string>();
                foreach (var cat in catSearchFilter)
                {
                    if (cat == lastcat)
                    {
                        newCatSearchFilter.Add(cat);
                        break;
                    }
                    else
                        newCatSearchFilter.Add(cat);
                }
                catSearchFilter = newCatSearchFilter;
            }
            model.searchedCategories = catSearchFilter;
            if (lastAtt != null)
            {
                List<string> newAttSearchFilter = new List<string>();
                foreach (var att in attSearchFilter)
                {
                    if (att == lastAtt)
                    {
                        newAttSearchFilter.Add(att);
                        break;
                    }
                    else
                        newAttSearchFilter.Add(att);
                }
                attSearchFilter = newAttSearchFilter;
            }
            model.searchedAttributes = attSearchFilter;

            foreach (var cat in catSearchFilter)
            {
                try
                {
                    var cta = _categoryService.GetCategoryById(Convert.ToInt32(cat));
                    model.searchedCategoriesName.Add(cta?.Name);
                }
                catch{ }
            }
            //foreach (var att in attSearchFilter)
            //{
            //    var attr = _productAttributeService.GetAllProductAttributes(Convert.ToInt32(att));
            //    model.searchedAttributesName.Add(attr);
            //}
            //catSearchFilter.GroupBy(c => c).ToList();
            if (minPrice != 0)
                min = minPrice;
            if (maxPrice != 0)
                max = maxPrice;

            //var products = _productService.SearchProducts(out var filterableSpecificationAttributeOptionIds,
            //    true,
            //    storeId: InovatiqaDefaults.StoreId,
            //    visibleIndividuallyOnly: true,
            //    keywords: term,
            //    featuredProducts: false,
            //    priceMin: min,
            //    priceMax: max,
            //    filteredSpecs: null,
            //    orderBy: ProductSortingEnum.CreatedOn,
            //    pageIndex: command.PageNumber - 1,
            //    pageSize: command.PageSize,
            //    categoryIds: category);

            // elastic search implementation by Ali Ahmad

            //var elasticProducts = _client.Search<ProductOverviewModel>(
            //    s => s
            //    .Query(q => q
            //        .QueryString(d => d.
            //            Query(term)

            //            .DefaultField("name")))
            //    .From(0)
            //    .Size(6));
            //List<KeyValuePair<string, int>> CategoriesCount = new List<KeyValuePair<string, int>>();
            //var productByManufacturer = _client.Search<ProductOverviewModel>(s => s
            //    .Query(q => q
            //        .Match(m => m
            //            .Field(f => f.ProductManufacturers.Select(sm => sm.Name))
            //                .Query("MCKESSON")
            //            )
            //        )
            //    );
            //var productByPrice = _client.Search<ProductOverviewModel>(s => s
            //    .Query(q => q
            //        .Range(r => r
            //            .Field(f => f.ProductPrice.PriceValue)
            //            .GreaterThanOrEquals(minPrice)
            //            .LessThanOrEquals(maxPrice)
            //        )
            //    )
            //);

            //List<KeyValuePair<string, int>> ManufacturerCount = new List<KeyValuePair<string, int>>();
            //foreach (var product in elasticProducts.Documents)
            //{
            //    foreach(var manufacturer in product.ProductManufacturers.Select(s => s.Name))
            //    {
            //        KeyValuePair<string, int> tempPair = ManufacturerCount.Where(m => m.Key == manufacturer).FirstOrDefault();
            //        if (tempPair.Equals(new KeyValuePair<string, int>())){
            //            ManufacturerCount.Add(new KeyValuePair<string, int>(manufacturer, 1));
            //        }
            //        else
            //        {
            //            ManufacturerCount[ManufacturerCount.IndexOf(tempPair)] = new KeyValuePair<string, int>(tempPair.Key, tempPair.Value + 1);
            //        }
            //    }
            //}
            //ManufacturerCount.Sort((x, y) => x.Key.CompareTo(y.Key));



            //List<KeyValuePair<string, int>> CategoriesCount = new List<KeyValuePair<string, int>>();
            //foreach (var product in elasticProducts.Documents)
            //{
            //    foreach (var categoryName in product.ProductCategories.Select(s => s.Name))
            //    {
            //        //string categoryName = _categoryService.GetCategoryById(categoryId).Name;
            //        KeyValuePair<string, int> tempPair = CategoriesCount.Where(c => c.Key == categoryName).FirstOrDefault();
            //        if (tempPair.Equals(new KeyValuePair<string, int>()))
            //        {
            //            CategoriesCount.Add(new KeyValuePair<string, int>(categoryName, 1)); // This category is not present and we found first product of it so add it with product count 1
            //        }
            //        else
            //        {
            //            CategoriesCount[CategoriesCount.IndexOf(tempPair)] = new KeyValuePair<string, int>(tempPair.Key, tempPair.Value + 1);
            //        }
            //    }
            //}
            //CategoriesCount.Sort((x, y) => x.Key.CompareTo(y.Key));
            ////end elastic search implementation
            //model.CategoriesCount = CategoriesCount;
            ////model.Products = _productModelFactory.PrepareProductOverviewModels(products, prepareProductAttributes: true).ToList();
            
            var elasticProducts = _client.Search<ProductOverviewModel>(s => s
                    .Query(q => q
                        .QueryString(d => d
                            .Query(term)
                        ) && q
                        .QueryString(m => m
                            .Query(string.Join(" OR ", manList.ToList()))
                            .DefaultField(f => f.ProductManufacturers.Select(s => s.Id))
                        )
                        && q
                        //.Terms(t => t
                        //   .Field(f => f.ProductCategories.Select(s => s.Id))
                        //   .Terms(catSearchFilter)
                        //)
                        .QueryString(c => c
                            .Query(string.Join(" AND ", catSearchFilter.ToList()))
                            .DefaultField(f => f.ProductCategories.Select(s => s.Id))
                        )&& q
                        .QueryString(at => at
                            .Query(String.Join(" OR ", attSearchFilter.ToList()))
                            .DefaultField(f => f.ProductAttributes.Select(s => s.Values.Select(s => s.Id)))
                        )
                        //.Match(mc => mc
                        //    .Field(f => f.ProductCategories.Select(sc => sc.Id))
                        //    .Query(categoryId.ToString() == "-1" ? "" : categoryId.ToString())
                        //)
                        && q
                        .Range(r => r
                            .Field(f => f.ProductPrice.PriceValue)
                            .GreaterThanOrEquals(minPrice)
                            .LessThanOrEquals(maxPrice > 0 ? maxPrice : Int32.MaxValue)
                        )

                    )
                     .Aggregations(agg => agg
                        .Terms("Brands", tm => tm
                            .Field(f => f.ProductManufacturers.Select(s => s.Id))
                            .Size(10000)
                        )
                   )
                    //.Aggregations(aggs => aggs
                    //    .Terms("Category", ta =>ta
                    //        .Field(f => f.ProductCategories.Select(s => s.Id))
                    //        .Size(10000)
                    //    )
                    //)
                    //.Aggregations(agg => agg
                    //     .Terms("Brands", tm => tm
                    //         .Field(f => f.ProductManufacturers.Select(s => s.Id))
                    //         .Size(10000)
                    //     )
                    //)
                    .From(((command.PageNumber - 1) * command.PageSize))
                    .Size(command.PageSize)
                );

            var manufacturerCount = new List<KeyValuePair<string, string>>();
            try
            {
                BucketAggregate manBucket = (BucketAggregate)elasticProducts.Aggregations.Values.First();
                foreach (Nest.KeyedBucket<System.Object> man in manBucket.Items)
                {
                    manufacturerCount.Add(new KeyValuePair<string, string>(Convert.ToString(man.Key), Convert.ToString(man.DocCount)));
                }
                manufacturerCount.Remove(manufacturerCount.Where(m => m.Key == "0").First());
            }
            catch { }
            var productCount = manufacturerCount.Sum(ms => Convert.ToInt32(ms.Value));

            //get manufacturers
            // var elasticProductManufacturers = _client.Search<ProductOverviewModel>(s => s
            //        .Query(q => q
            //            .QueryString(d => d
            //                .Query(term)
            //            ) && q
            //            //.QueryString(m => m
            //            //    .Query(string.Join(" OR ", manList.ToList()))
            //            //    .DefaultField(f => f.ProductManufacturers.Select(s => s.Id))
            //            //)
            //            //&& q
            //            //.Terms(t => t
            //            //   .Field(f => f.ProductCategories.Select(s => s.Id))
            //            //   .Terms(catSearchFilter)
            //            //)
            //            .QueryString(m => m
            //                .Query(string.Join(" AND ", catSearchFilter.ToList()))
            //                .DefaultField(f => f.ProductCategories.Select(s => s.Id))
            //            )
            //            //.Match(mc => mc
            //            //    .Field(f => f.ProductCategories.Select(sc => sc.Id))
            //            //    .Query(categoryId.ToString() == "-1" ? "" : categoryId.ToString())
            //            //)
            //            && q
            //            .Range(r => r
            //                .Field(f => f.ProductPrice.PriceValue)
            //                .GreaterThanOrEquals(minPrice)
            //                .LessThanOrEquals(maxPrice > 0 ? maxPrice : Int32.MaxValue)
            //            )

            //        )
            //        //.Aggregations(aggs => aggs
            //        //    .Terms("Category", ta =>ta
            //        //        .Field(f => f.ProductCategories.Select(s => s.Id))
            //        //        .Size(10000)
            //        //    )
            //        //)
            //        .Aggregations(agg => agg
            //             .Terms("Brands", tm => tm
            //                 .Field(f => f.ProductManufacturers.Select(s => s.Id))
            //                 .Size(3500)
            //             )
            //        )
            //        .From(((command.PageNumber - 1) * command.PageSize))
            //        .Size(command.PageSize)
            //    );

            // BucketAggregate manBucket = (BucketAggregate)elasticProductManufacturers.Aggregations.Values.First();
            // var manuList = new List<string>();
            // var manufacturerCount = new List<KeyValuePair<string, string>>();
            // foreach (Nest.KeyedBucket<System.Object> man in manBucket.Items)
            // {
            //     manuList.Add(Convert.ToString(man.Key));
            //     manufacturerCount.Add(new KeyValuePair<string, string>(Convert.ToString(man.Key), Convert.ToString(man.DocCount)));
            // }
            // var url = InovatiqaDefaults.ElasticEndPoint;
            // var settings = new ConnectionSettings(new Uri(url))
            //     .DefaultIndex("manufacturers")
            //     .BasicAuthentication(InovatiqaDefaults.ElasticUsername, InovatiqaDefaults.ElasticPassword);
            // var clientManufacturers = new ElasticClient(settings);
            // var elasticManufacturers = clientManufacturers.Search<ManufacturerBriefInfoModel>(s => s
            //     .Query(q => q
            //         .QueryString(d => d
            //             .Query(String.Join(" OR ", manuList.ToList()))
            //             .DefaultField(f => f.Id)
            //         )
            //     )
            //     .Size(manuList.Count)
            // );
            // foreach(var man in elasticManufacturers.Documents)
            // {
            //     var tempList = manufacturerCount.Select(s => s.Key).ToList();
            //     if (tempList.Contains(Convert.ToString(man.Id)))
            //     {
            //         model.Manufacturers.Add(new ManufacturerBriefInfoModel
            //         {
            //             Id = man.Id,
            //             Name = man.Name,
            //             SeName = man.SeName,
            //             IsActive = man.IsActive,
            //             IsSelected = manList.Contains(man.Id.ToString()),
            //             Count = manufacturerCount[tempList.IndexOf(Convert.ToString(man.Id))].Value
            //         });
            //     }
            // }

            // //get Categories

            // //by hamza for elastic categories aggregations

            // var elasticProductsCategory = _client.Search<ProductOverviewModel>(s => s
            //      .Query(q => q
            //          .QueryString(d => d
            //              .Query(term)
            //          ) && q
            //          .QueryString(m => m
            //              .Query(string.Join(" OR ", manList.ToList()))
            //              .DefaultField(f => f.ProductManufacturers.Select(s => s.Id))
            //          )
            //          && q
            //          .QueryString(m => m
            //              .Query(string.Join(" AND ", catSearchFilter.ToList()))
            //              .DefaultField(f => f.ProductCategories.Select(s => s.Id))
            //          )
            //          && q
            //          .Range(r => r
            //              .Field(f => f.ProductPrice.PriceValue)
            //              .GreaterThanOrEquals(minPrice)
            //              .LessThanOrEquals(maxPrice > 0 ? maxPrice : Int32.MaxValue)
            //          )

            //      )
            //      .Aggregations(agg => agg
            //           .Terms("Categories", tm => tm
            //               .Field(f => f.ProductCategories.Select(s => s.ParentCategoriesId))
            //               .Size(3000)
            //           )
            //      )
            //      .From(((command.PageNumber - 1) * command.PageSize))
            //      .Size(command.PageSize)
            //  );


            // BucketAggregate catBucket = (BucketAggregate)elasticProductsCategory.Aggregations.Values.First();
            // var catList = new List<string>();
            // var categoryCount = new List<KeyValuePair<string, string>>();
            // foreach(Nest.KeyedBucket<System.Object> cat in catBucket.Items)
            // {
            //     catList.Add(Convert.ToString(cat.Key));
            //     categoryCount.Add(new KeyValuePair<string, string>(Convert.ToString(cat.Key), Convert.ToString(cat.DocCount)));
            // }

            // var url1 = InovatiqaDefaults.ElasticEndPoint;
            // var settings1 = new ConnectionSettings(new Uri(url1))
            //     .DefaultIndex("categories")
            //     .BasicAuthentication(InovatiqaDefaults.ElasticUsername, InovatiqaDefaults.ElasticPassword);
            // var clientCategories = new ElasticClient(settings1);
            // var elasticCategories = clientCategories.Search<CategoryModel>(s => s
            //     .Query(q => q
            //         .QueryString(d => d
            //             .Query(String.Join(" OR ", catList.ToList()))
            //             .DefaultField(f => f.Id)
            //         )
            //     )
            //     .Size(6)
            // );
            // foreach(var category in elasticCategories.Documents)
            // {
            //     var tempList = categoryCount.Select(s => s.Key).ToList();
            //     if (tempList.Contains(Convert.ToString(category.Id)))
            //     {
            //         model.Categories.Add(new ParentCategoryModel
            //         {
            //             Id = category.Id,
            //             Name = category.Name,
            //             ParentCategoryId = category.ParentCategoriesId,
            //             CategoryCount = categoryCount[tempList.IndexOf(Convert.ToString(category.Id))].Value,
            //             ChildCategories = category.ChildCategories.Select(s => new ChildCategoryModel
            //             {
            //                 Id = s.Id,
            //                 Name = s.Name
            //             }).ToList()
            //         });
            //     }
            // }
            // //for child categories

            //var elasticProductsChildCategory = _client.Search<ProductOverviewModel>(s => s
            //    .Query(q => q
            //        .QueryString(d => d
            //            .Query(term)
            //        ) && q
            //        .QueryString(m => m
            //            .Query(string.Join(" OR ", manList.ToList()))
            //            .DefaultField(f => f.ProductManufacturers.Select(s => s.Id))
            //        )
            //        && q
            //        .QueryString(m => m
            //            .Query(string.Join(" AND ", catSearchFilter.ToList()))
            //            .DefaultField(f => f.ProductCategories.Select(s => s.Id))
            //        )
            //        && q
            //        .Range(r => r
            //            .Field(f => f.ProductPrice.PriceValue)
            //            .GreaterThanOrEquals(minPrice)
            //            .LessThanOrEquals(maxPrice > 0 ? maxPrice : Int32.MaxValue)
            //        )

            //    )
            //    .Aggregations(agg => agg
            //         .Terms("Categories", tm => tm
            //             .Field(f => f.ProductCategories.Select(s => s.Id))
            //             .Size(3000)
            //         )
            //    )
            //    .From(((command.PageNumber - 1) * command.PageSize))
            //    .Size(command.PageSize)
            //);
            // BucketAggregate childCategoryBucket = (BucketAggregate)elasticProductsChildCategory.Aggregations.Values.First();
            // var childCategoryCount = new List<KeyValuePair<string, string>>();
            // foreach (Nest.KeyedBucket<System.Object> cat in childCategoryBucket.Items)
            // {
            //     childCategoryCount.Add(new KeyValuePair<string, string>(Convert.ToString(cat.Key), Convert.ToString(cat.DocCount)));
            // }
            // model.ChildCategoryCount = childCategoryCount;
            //if (manList.Count > 0)
            //{
            //    var elasticManufacturerAndCategories = _client.Search<ProductOverviewModel>(s => s
            //                .Query(q => q
            //                    .QueryString(qs => qs
            //                        .Query(term)
            //                     ) && q
            //                    //.Terms(t => t
            //                    //   .Field(f => f.ProductCategories.Select(s => s.Id))
            //                    //   .Terms(catSearchFilter)
            //                    //)
            //                    .QueryString(m => m
            //                        .Query(string.Join(" AND ", catSearchFilter.ToList()))
            //                        .DefaultField(f => f.ProductCategories.Select(s => s.Id))
            //                    )
            //                    && q
            //                    .Range(r => r
            //                        .Field(f => f.ProductPrice.PriceValue)
            //                        .GreaterThanOrEquals(minPrice)
            //                        .LessThanOrEquals(maxPrice > 0 ? maxPrice : Int32.MaxValue)
            //                    )
            //                )
            //                .Source(src => src
            //                    .Includes(im => im
            //                        .Field(f => f.ProductManufacturers)
            //                        .Field(f => f.ProductCategories)
            //                    )
            //                )
            //                .Size(10000)
            //            );
            //    foreach (var item in elasticManufacturerAndCategories.Documents)
            //    {
            //        var man = item.ProductManufacturers.First();
            //        model.Manufacturers.Add(new ManufacturerBriefInfoModel
            //        {
            //            Id = man.Id,
            //            Name = man.Name,
            //            SeName = man.SeName,
            //            IsActive = man.IsActive,
            //            IsSelected = manList.Contains(man.Id.ToString())
            //        });
            //    }

            //    foreach (var categories in elasticManufacturerAndCategories.Documents)
            //    {
            //        foreach (var cat in categories.ProductCategories)
            //        {
            //            model.Categories.Add(new ParentCategoryModel
            //            {
            //                Id = cat.Id,
            //                Name = cat.Name,
            //                ParentCategoryId = cat.ParentCategoriesId,
            //                ChildCategories = cat.childCategory.Select(s => new ChildCategoryModel
            //                {
            //                    Id = s.Value,
            //                    Name = s.Key
            //                }).ToList()
            //            });
            //        }
            //    }
            //}

            //else
            //{
            //    var elasticManufacturerAndCategories = _client.Search<ProductOverviewModel>(s => s
            //                .Query(q => q
            //                    .QueryString(qs => qs
            //                        .Query(term)
            //                     ) && q
            //                    //.Terms(t => t
            //                    //   .Field(f => f.ProductCategories.Select(s => s.Id))
            //                    //   .Terms(catSearchFilter)
            //                    //)
            //                    .QueryString(m => m
            //                        .Query(string.Join(" AND ", catSearchFilter.ToList()))
            //                        .DefaultField(f => f.ProductCategories.Select(s => s.Id))
            //                    )
            //                    && q
            //                    .QueryString(m => m
            //                        .Query(string.Join(" OR ", manList.ToList()))
            //                        .DefaultField(f => f.ProductManufacturers.Select(s => s.Id))
            //                    ) && q
            //                    .Range(r => r
            //                        .Field(f => f.ProductPrice.PriceValue)
            //                        .GreaterThanOrEquals(minPrice)
            //                        .LessThanOrEquals(maxPrice > 0 ? maxPrice : Int32.MaxValue)
            //                    )
            //                )
            //                .Source(src => src
            //                    .Includes(im => im
            //                        .Field(f => f.ProductManufacturers)
            //                        .Field(f => f.ProductCategories)
            //                    )
            //                )
            //                .Size(10000)
            //            );
            //    foreach (var item in elasticManufacturerAndCategories.Documents)
            //    {
            //        var man = item.ProductManufacturers.First();
            //        model.Manufacturers.Add(new ManufacturerBriefInfoModel
            //        {
            //            Id = man.Id,
            //            Name = man.Name,
            //            SeName = man.SeName,
            //            IsActive = man.IsActive,
            //            IsSelected = manList.Contains(man.Id.ToString())
            //        });
            //    }

            //    foreach (var categories in elasticManufacturerAndCategories.Documents)
            //    {
            //        foreach (var cat in categories.ProductCategories)
            //        {
            //            model.Categories.Add(new ParentCategoryModel
            //            {
            //                Id = cat.Id,
            //                Name = cat.Name,
            //                ParentCategoryId = cat.ParentCategoriesId,
            //                ChildCategories = cat.childCategory.Select(s => new ChildCategoryModel
            //                {
            //                    Id = s.Value,
            //                    Name = s.Key
            //                }).ToList()
            //            });
            //        }
            //    }
            //}

            var ComprisonList = _compareProductsService.GetComparedProducts();
            var list = new List<KeyValuePair<string, int>>();
            foreach (var cat in elasticProducts.Documents)
            {
                var product = _productService.GetProductById(cat.Id);
                var customer = _workContextService.CurrentCustomer;
                cat.ProductPrice.OrignalPrice = Convert.ToDecimal(cat.ProductPrice.PriceValue);
                cat.ProductPrice.PriceValue = _priceCalculationService.GetFinalPrice(product, customer);
                cat.ProductPrice.Price = _priceFormatter.FormatPrice(cat.ProductPrice.PriceValue);
                var price = cat.ProductPrice.PriceValue;
                DateTime now = DateTime.Now;
                var hasTierPrice = _entityTierPriceRepository.Query().Where(tp => tp.EntityId == product.Id && tp.Rate == price && tp.CustomerId == customer.Id && tp.StartDateTimeUtc <= now && tp.EndDateTimeUtc >= now);
                if (hasTierPrice.Count() > 0)
                {
                    cat.ProductPrice.EntityName = "Product";
                }
                cat.IsInCompareList = ComprisonList.Where(prod => prod.Id == cat.Id).ToList().Count > 0;
                foreach (var ccat in cat.ProductCategories.Select(s => s.childCategory))
                {
                    list.AddRange(ccat.Select(s => new KeyValuePair<string, int>(s.Key, s.Value)).ToList());
                }
            }
            var group1 = list.GroupBy(gc => gc.Key,
            gc => new
            {
                id = gc.Value
            }
            ).ToList();
            int count = 8;
            if (group1.Count < count)
            {
                count = group1.Count;
            }
            for (int i = 0; i < count; i++)
            {
                var s = group1[i].First().id;
                var catt = _categoryService.GetCategoryById(group1[i].First().id);
                if (catt != null)
                {
                    model.ChildCategories.Add(new ChildCategoryModel
                    {
                        Id = catt.Id,
                        Name = catt.Name,
                        SeName = _urlRecordService.GetActiveSlug(catt.Id, InovatiqaDefaults.CategorySlugName, InovatiqaDefaults.LanguageId),
                        PictureModel = new PictureModel
                        {
                            FullSizeImageUrl = catt.PictureId > 0 ? _pictureService.GetPictureUrl(catt.PictureId) : _pictureService.GetDefaultPictureUrl()
                        }
                    });
                }
            }



            model.Products = elasticProducts.Documents;
            model.PagingFilteringContext.LoadPagedList(new PagedList<ProductOverviewModel>(model.Products, command.PageIndex, command.PageSize, productCount));
            //model.PagingFilteringContext.LoadPagedList(new PagedList<ProductOverviewModel>(model.Products, command.PageIndex, command.PageSize, Convert.ToInt32(elasticProducts.HitsMetadata?.Total.Value)));
            return model;
        }

        public virtual CategoryModel PrepareCategorySearchModel(Category category, CatalogPagingFilteringModel command, string term)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var model = new CategoryModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                MetaKeywords = category.MetaKeywords,
                MetaDescription = category.MetaDescription,
                MetaTitle = category.MetaTitle,
                SeName = _urlRecordService.GetActiveSlug(category.Id, InovatiqaDefaults.CategorySlugName, InovatiqaDefaults.LanguageId),
            };

            PreparePageSizeOptions(model.PagingFilteringContext, command,
                category.AllowCustomersToSelectPageSize,
                category.PageSizeOptions,
                category.PageSize);

            model.DisplayCategoryBreadcrumb = true;

            model.CategoryBreadcrumb = _categoryService.GetCategoryBreadCrumb(category).Select(catBr =>
                new CategoryModel
                {
                    Id = catBr.Id,
                    Name = catBr.Name,
                    SeName = _urlRecordService.GetActiveSlug(catBr.Id, InovatiqaDefaults.CategorySlugName, InovatiqaDefaults.LanguageId)
                }).ToList();

            var pictureSize = InovatiqaDefaults.CategoryThumbPictureSize;

            var categoryIds = new List<int> { category.Id };

            var products = _productService.SearchProducts(out var filterableSpecificationAttributeOptionIds,
                true,
                categoryIds: categoryIds,
                storeId: InovatiqaDefaults.StoreId,
                visibleIndividuallyOnly: true,
                keywords: term,
                featuredProducts: false,
                priceMin: null,
                priceMax: null,
                filteredSpecs: null,
                orderBy: ProductSortingEnum.CreatedOn,
                pageIndex: command.PageNumber - 1,
                pageSize: command.PageSize);

            model.Products = _productModelFactory.PrepareProductOverviewModels(products, prepareProductAttributes: true).ToList();

            model.PagingFilteringContext.LoadPagedList(products);

            return model;
        }

        public virtual ProductSearchModel PrepareSearchCategory(string term)
        {
            var model = new ProductSearchModel();
            var elasticProducts = _client.Search<ElasticProduct>(
                s => s.Query(q => q.QueryString(d => d.Query(term).DefaultField("name"))));
            var allCategories = elasticProducts.Documents.Select(doc => doc.Categories).Select(cat => cat).ToList();
            var uniqueCategories = new List<int>();
            var totalCats = new List<int>();
            var AllRawCategories = new List<Category>();
            foreach (var categoryList in allCategories)
            {
                foreach (var category in categoryList)
                {
                    if (!uniqueCategories.Contains(category))
                    {
                        uniqueCategories.Add(category);
                    }
                    totalCats.Add(category);
                }
            }
            foreach (var category in uniqueCategories)
            {
                var categoryName = _categoryService.GetCategoryById(category);
                AllRawCategories.Add(categoryName);
                AllRawCategories.OrderBy(c => c.ParentCategoryId)
                                .GroupBy(c => c.ParentCategoryId);
            }
            var ParentIds = new List<int>();
            foreach (var cats in AllRawCategories)
            {
                if (!ParentIds.Contains(cats.ParentCategoryId))
                {
                    ParentIds.Add(cats.ParentCategoryId);
                }
            }
            foreach (var id in ParentIds)
            {
                var item = new ParentCategoryModel();
                item.Id = id;
                item.Name = _categoryService.GetCategoryById(id).Name;
                foreach (var c in AllRawCategories)
                {
                    var childModel = new ChildCategoryModel();
                    if (c.ParentCategoryId == id)
                    {
                        childModel.Id = c.Id;
                        childModel.Name = c.Name;
                        childModel.Count = totalCats.Where(cat => cat == c.Id).Count();
                        item.ChildCategories.Add(childModel);
                    }
                }
                model.Categories.Add(item);
            }
            var allManufacturers = elasticProducts.Documents.Select(doc => doc.Manufacturers);
            return model;
        }

        #endregion

        #region Indexing
        public virtual void IndexProducts(int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                try
                {
                    Product prod = _productService.GetProductById(i);
                    if (prod != null)
                    {
                        List<Product> products = new List<Product>();
                        products.Add(prod);
                        var catalog = _productModelFactory.PrepareProductOverviewModels(products, prepareProductAttributes: true, prepareProductCategories: true);
                        _client.IndexDocument(catalog.First());
                    }
                }
                catch (Exception ex)
                {
                    _loggerService.Warning(ex.Message + i, ex, _workContextService.CurrentCustomer);
                    //Log log = new Log
                    //{
                    //    CreatedOnUtc = DateTime.UtcNow,
                    //    Customer = _workContextService.CurrentCustomer,
                    //    CustomerId = _workContextService.CurrentCustomer.Id,
                    //    FullMessage = ex.Message,
                    //    ShortMessage = String.Format("Product Indexing Error {0}.", i)
                    //};
                }
            }
            //var products = _productService.GetAllProducts();
            //var catalog = _productModelFactory.PrepareProductOverviewModels(products, prepareProductAttributes: true, prepareProductCategories: true);
            //foreach(var product in catalog)
            //{
            //    _client.IndexDocument(product);
            //}
        }
        //by hamza for Indexing bug fixing
        //public virtual void IndexProductsCheck(int start, int end)
        //{
        //    var url = InovatiqaDefaults.ElasticEndPoint;

        //    var settings = new ConnectionSettings(new Uri(url))
        //        .DefaultIndex("ProductsLogCheck")
        //        .BasicAuthentication(InovatiqaDefaults.ElasticUsername, InovatiqaDefaults.ElasticPassword);

        //    //AddDefaultMappings(settings);

        //    var clientProductCheck = new ElasticClient(settings);
        //    for (int i = start; i < end; i++)
        //    {
        //        try
        //        {
        //            Product prod = _productService.GetProductById(i);
        //            if (prod != null)
        //            {
        //                List<Product> products = new List<Product>();
        //                products.Add(prod);
        //                products = i == 18 ? null : products;
        //                var catalog = _productModelFactory.PrepareProductOverviewModels(products, prepareProductAttributes: true, prepareProductCategories: true);
        //                clientProductCheck.IndexDocument<ProductOverviewModel>(catalog.First());
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _loggerService.Warning(ex.Message, ex, _workContextService.CurrentCustomer);
        //            //Log log = new Log
        //            //{
        //            //    CreatedOnUtc = DateTime.UtcNow,
        //            //    Customer = _workContextService.CurrentCustomer,
        //            //    CustomerId = _workContextService.CurrentCustomer.Id,
        //            //    FullMessage = ex.Message,
        //            //    ShortMessage = String.Format("Product Indexing Error {0}.", i)
        //            //};
        //        }
        //    }
        //    //var products = _productService.GetAllProducts();
        //    //var catalog = _productModelFactory.PrepareProductOverviewModels(products, prepareProductAttributes: true, prepareProductCategories: true);
        //    //foreach(var product in catalog)
        //    //{
        //    //    _client.IndexDocument(product);
        //    //}
        //}

        //by hamza for elasticsearch
        public virtual void IndexManufacturers(int start, int end)
        {
            var url = InovatiqaDefaults.ElasticEndPoint;

            var settings = new ConnectionSettings(new Uri(url))
                .DefaultIndex("manufacturers")
                .BasicAuthentication(InovatiqaDefaults.ElasticUsername, InovatiqaDefaults.ElasticPassword);

            //AddDefaultMappings(settings);

            var clientManufacturers = new ElasticClient(settings);

            for (int i = start; i < end; i++)
            {
                try
                {
                    Manufacturer man = _manufacturerService.GetManufacturerById(i);
                    if (man != null)
                    {
                        List<Manufacturer> manufacturer = new List<Manufacturer>();
                        manufacturer.Add(man);
                        var catalogManufacturers = _productModelFactory.PrepareManufacturersModels(manufacturer);
                        clientManufacturers.IndexDocument<ManufacturerBriefInfoModel>(catalogManufacturers.First());
                    }
                }
                catch (Exception ex)
                {
                    _loggerService.Warning(ex.Message + i, ex, _workContextService.CurrentCustomer);
                    //Log log = new Log
                    //{
                    //    CreatedOnUtc = DateTime.UtcNow,
                    //    Customer = _workContextService.CurrentCustomer,
                    //    CustomerId = _workContextService.CurrentCustomer.Id,
                    //    FullMessage = ex.Message,
                    //    ShortMessage = String.Format("Manufacturers Indexing Error {0}.", i)
                    //};
                }
            }


            //var manufacturers = _manufacturerService.GetAllManufacturers();
            //var catalogManufacturers = _productModelFactory.PrepareManufacturersModels(manufacturers);
            //foreach(var manufacturer in catalogManufacturers)
            //{
            //    client.IndexDocument<ManufacturerBriefInfoModel>(manufacturer);
            //}
        }

        //by hamza for elasticsearch
        public virtual void IndexCategories(int start, int end)
        {

            var url = InovatiqaDefaults.ElasticEndPoint;
            var settings = new ConnectionSettings(new Uri(url))
                .DefaultIndex("categories")
                .BasicAuthentication(InovatiqaDefaults.ElasticUsername, InovatiqaDefaults.ElasticPassword);
            var clientCategories = new ElasticClient(settings);

            for (int i = start; i < end; i++)
            {
                try
                {
                    var category = _categoryService.GetCategoryById(i);
                    if (category != null)
                    {
                        List<Category> categories = new List<Category>();
                        categories.Add(category);
                        var catalogCategories = _productModelFactory.PrepareCategoryModel(categories);
                        clientCategories.IndexDocument<CategoryModel>(catalogCategories.First());
                    }
                }
                catch (Exception ex)
                {
                    _loggerService.Warning(ex.Message + i, ex, _workContextService.CurrentCustomer);
                    //Log log = new Log
                    //{
                    //    CreatedOnUtc = DateTime.UtcNow,
                    //    Customer = _workContextService.CurrentCustomer,
                    //    CustomerId = _workContextService.CurrentCustomer.Id,
                    //    FullMessage = ex.Message,
                    //    ShortMessage = String.Format("Categories Indexing Error {0}.", i)
                    //};
                }
            }

            //var categories = _categoryService.GetAllCategories();
            //var catalogCategories = _productModelFactory.PrepareCategoryModel(categories);
            //foreach(var category in catalogCategories)
            //{
            //    client.IndexDocument<CategoryModel>(category);
            //}

        }

        //by hamza for elasticsearch
        public virtual void IndexAttributes(int start, int end)
        {
            var url = InovatiqaDefaults.ElasticEndPoint;
            var settings = new ConnectionSettings(new Uri(url))
                .DefaultIndex("attributes")
                .BasicAuthentication(InovatiqaDefaults.ElasticUsername, InovatiqaDefaults.ElasticPassword);
            var clientAttributes = new ElasticClient(settings);
            for (int i = start; i < end; i++)
            {
                try
                {
                    Product prod = _productService.GetProductById(i);
                    if (prod != null)
                    {
                        var catalogAttributes = _productModelFactory.PrepareProductAttributeModels(prod, null);
                        clientAttributes.IndexMany<ProductDetailsModel.ProductAttributeModel>(catalogAttributes);
                    }
                }
                catch (Exception ex)
                {
                    _loggerService.Warning(ex.Message + i, ex, _workContextService.CurrentCustomer);
                    //Log log = new Log
                    //{
                    //    CreatedOnUtc = DateTime.UtcNow,
                    //    Customer = _workContextService.CurrentCustomer,
                    //    CustomerId = _workContextService.CurrentCustomer.Id,
                    //    FullMessage = ex.Message,
                    //    ShortMessage = String.Format("Attribute Indexing Error {0}.", i)
                    //};
                }
            }
        }

        #endregion

        #region Manufactureres

        public virtual ManufacturerNavigationModel PrepareManufacturerNavigationModel(int currentManufacturerId, List<string> selectedManufacturers)
        {
            var currentManufacturer = _manufacturerService.GetManufacturerById(currentManufacturerId);

            var manufacturers = _manufacturerService.GetAllManufacturers(storeId: InovatiqaDefaults.StoreId,
                pageSize: InovatiqaDefaults.ManufacturersBlockItemsToDisplay);
            var model = new ManufacturerNavigationModel
            {
                TotalManufacturers = manufacturers.TotalCount
            };

            foreach (var manufacturer in manufacturers)
            {
                int totalProductCount = 0;
                if (!string.IsNullOrEmpty(manufacturer.ProductCount.ToString()))
                    totalProductCount = int.Parse(manufacturer.ProductCount.ToString());
                var modelMan = new ManufacturerBriefInfoModel
                {
                    Id = manufacturer.Id,
                    Name = manufacturer.Name,
                    SeName = _urlRecordService.GetActiveSlug(manufacturer.Id, InovatiqaDefaults.ManufacturerSlugName, InovatiqaDefaults.LanguageId),
                    IsActive = currentManufacturer != null && currentManufacturer.Id == manufacturer.Id,
                    NumberOfProducts = totalProductCount,
                    IsSelected = selectedManufacturers.Contains(manufacturer.Id.ToString())
                };
                model.Manufacturers.Add(modelMan);
            }

            return model;
        }

        public virtual List<ManufacturerModel> PrepareManufacturerAllModels()
        {
            var model = new List<ManufacturerModel>();
            var manufacturers = _manufacturerService.GetAllManufacturers(storeId: InovatiqaDefaults.StoreId);
            foreach (var manufacturer in manufacturers)
            {
                var modelMan = new ManufacturerModel
                {
                    Id = manufacturer.Id,
                    Name = manufacturer.Name,
                    Description = manufacturer.Description,
                    MetaKeywords = manufacturer.MetaKeywords,
                    MetaDescription = manufacturer.MetaDescription,
                    MetaTitle = manufacturer.MetaTitle,
                    SeName = _urlRecordService.GetActiveSlug(manufacturer.Id, InovatiqaDefaults.ManufacturerSlugName, InovatiqaDefaults.LanguageId),
                };

                var pictureSize = InovatiqaDefaults.ManufacturerThumbPictureSize;

                var picture = _pictureService.GetPictureById(manufacturer.PictureId);
                var pictureModel = new PictureModel
                {
                    FullSizeImageUrl = _pictureService.GetPictureUrl(ref picture),
                    ImageUrl = _pictureService.GetPictureUrl(ref picture, pictureSize),
                    Title = string.Format("Show products manufactured by {0}", modelMan.Name),
                    AlternateText = string.Format("Picture for manufacturer {0}", modelMan.Name)
                };

                modelMan.PictureModel = pictureModel;
                model.Add(modelMan);
            }

            return model;
        }

        public virtual ManufacturerModel PrepareManufacturerModel(Manufacturer manufacturer, CatalogPagingFilteringModel command, int minPrice = 0, int maxPrice = 0)
        {
            if (manufacturer == null)
                throw new ArgumentNullException(nameof(manufacturer));

            var model = new ManufacturerModel
            {
                Id = manufacturer.Id,
                Name = manufacturer.Name,
                Description = manufacturer.Description,
                MetaKeywords = manufacturer.MetaKeywords,
                MetaDescription = manufacturer.MetaDescription,
                MetaTitle = manufacturer.MetaTitle,
                SeName = _urlRecordService.GetActiveSlug(manufacturer.Id, InovatiqaDefaults.ManufacturerSlugName, InovatiqaDefaults.LanguageId),
            };

            PreparePageSizeOptions(model.PagingFilteringContext, command,
                manufacturer.AllowCustomersToSelectPageSize,
                manufacturer.PageSizeOptions,
                manufacturer.PageSize);

            if (!InovatiqaDefaults.IgnoreFeaturedProducts)
            {
                IPagedList<Product> featuredProducts = null;

                featuredProducts = _productService.SearchProducts(
                       manufacturerId: manufacturer.Id,
                       storeId: InovatiqaDefaults.StoreId,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true);

                if (featuredProducts != null)
                {
                    model.FeaturedProducts = _productModelFactory.PrepareProductOverviewModels(featuredProducts, prepareProductAttributes: true).ToList();
                }
            }
            Nullable<int> min = null;
            Nullable<int> max = null;
            if (minPrice != 0)
            {
                min = minPrice;
            }
            if(maxPrice != 0)
            {
                max = maxPrice;
            }
            var products = _productService.SearchProducts(out _, true,
                manufacturerId: manufacturer.Id,
                storeId: InovatiqaDefaults.StoreId,
                visibleIndividuallyOnly: true,
                featuredProducts: InovatiqaDefaults.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
                priceMin: min,
                priceMax: max,
                orderBy: ProductSortingEnum.CreatedOn,
                pageIndex: command.PageNumber - 1,
                pageSize: command.PageSize);
            model.Products = _productModelFactory.PrepareProductOverviewModels(products, prepareProductAttributes: true).ToList();
            
            model.PagingFilteringContext.LoadPagedList(products);

            return model;
        }

        public virtual List<ManufacturerModel> PrepareFeaturedManufacturerModel(int number)
        {
            var model = new List<ManufacturerModel>();
            var manufacturers = _manufacturerService.GetAllFeaturedManufacturers(number);
            foreach (var manufacturer in manufacturers)
            {
                var modelMan = new ManufacturerModel
                {
                    Id = manufacturer.Id,
                    Name = manufacturer.Name,
                    Description = manufacturer.Description,
                    MetaKeywords = manufacturer.MetaKeywords,
                    MetaDescription = manufacturer.MetaDescription,
                    MetaTitle = manufacturer.MetaTitle,
                    SeName = _urlRecordService.GetActiveSlug(manufacturer.Id, InovatiqaDefaults.ManufacturerSlugName, InovatiqaDefaults.LanguageId),
                };

                var pictureSize = InovatiqaDefaults.ManufacturerThumbPictureSize;

                var picture = _pictureService.GetPictureById(manufacturer.PictureId);
                var pictureModel = new PictureModel
                {
                    FullSizeImageUrl = _pictureService.GetPictureUrl(ref picture),
                    ImageUrl = _pictureService.GetPictureUrl(ref picture, pictureSize),
                    Title = string.Format("Show products manufactured by {0}", modelMan.Name),
                    AlternateText = string.Format("Picture for manufacturer {0}", modelMan.Name)
                };

                modelMan.PictureModel = pictureModel;
                model.Add(modelMan);
            }

            return model;
        }

        #endregion
    }
}