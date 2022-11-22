using Inovatiqa.Database;
using Inovatiqa.Database.DbContexts;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Services.Catalog;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Web.Factories;
using Inovatiqa.Web.Factories.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Inovatiqa.Web.Routing;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Core;
using Inovatiqa.Services.Media;
using Inovatiqa.Services.Media.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.Shipping;
using Inovatiqa.Services.Vendors;
using Inovatiqa.Services.Vendors.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Services;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Orders;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Seo;
using Inovatiqa.Services.Customers;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Common;
using Inovatiqa.Services.Topics;
using Inovatiqa.Services.Topics.Interfaces;
using Inovatiqa.Services.News;
using Inovatiqa.Services.News.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Services.Helpers;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Services.Security;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Inovatiqa.Services.Directory;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Messages;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Discounts.Interfaces;
using Inovatiqa.Services.Discounts;
using Inovatiqa.Services.Payments.Interfaces;
using Inovatiqa.Services.Payments;
using Inovatiqa.Services.Logging.Interfaces;
using Inovatiqa.Services.Logging;
using Inovatiqa.Services.Settings.Interfces;
using Inovatiqa.Services.Settings;
using Inovatiqa.Web.Extensions;
using System.ComponentModel;
using System.Collections.Generic;
using Inovatiqa.Core.Caching;
using Inovatiqa.Services.Caching.Interfaces;
using Inovatiqa.Services.Caching;
using Inovatiqa.Payments.Services;
using System.Net.Http;
using Inovatiqa.Web.UI;
using Inovatiqa.Services.Shipping.Tracking.Interfaces;
using Inovatiqa.Web.Framework.Factories.Interfaces;
using Inovatiqa.Web.Framework.Factories;
using AutoMapper;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper;
using Inovatiqa.Services.Events.Interfaces;
using Inovatiqa.Services.Events;
using Inovatiqa.Services.Tasks;
using System.Threading.Tasks;
using InovatiqaElasticSearch;

namespace Inovatiqa.Web
{
    public class Startup
    {
        IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            var authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = InovatiqaDefaults.AuthenticationScheme;
                options.DefaultScheme = InovatiqaDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = InovatiqaDefaults.ExternalAuthenticationScheme;
            });

            //add main cookie authentication
            authenticationBuilder.AddCookie(InovatiqaDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = $"{InovatiqaDefaults.Prefix}{InovatiqaDefaults.AuthenticationCookie}";
                options.Cookie.HttpOnly = true;
                options.LoginPath = InovatiqaDefaults.LoginPath;
                options.AccessDeniedPath = InovatiqaDefaults.AccessDeniedPath;

                //whether to allow the use of authentication cookies from SSL protected page on the other store pages which are not
                options.Cookie.SecurePolicy = InovatiqaDefaults.SslEnabled
                    ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.None;
            });

            ////add external authentication
            //authenticationBuilder.AddCookie(InovatiqaDefaults.ExternalAuthenticationScheme, options =>
            //{
            //    options.Cookie.Name = $"{InovatiqaDefaults.Prefix}{InovatiqaDefaults.ExternalAuthenticationCookie}";
            //    options.Cookie.HttpOnly = true;
            //    options.LoginPath = InovatiqaDefaults.LoginPath;
            //    options.AccessDeniedPath = InovatiqaDefaults.AccessDeniedPath;

            //    //whether to allow the use of authentication cookies from SSL protected page on the other store pages which are not
            //    options.Cookie.SecurePolicy = InovatiqaDefaults.SslEnabled
            //        ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.None;
            //});

            services.AddControllers().AddNewtonsoftJson();
            services.AddApplicationInsightsTelemetry();
            services.AddLazyResolution();

            services.AddRazorPages();

            services.AddSession(options =>
            {
                options.IdleTimeout = System.TimeSpan.FromMinutes(2);
            });


            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDbContextPool<InovatiqaContext>(options =>
            options.UseSqlServer(_configuration.GetConnectionString("InovatiqaConnection")));

            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));

            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<IUrlRecordService, UrlRecordService>();
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<IPictureService, PictureService>();
            services.AddTransient<ISpecificationAttributeService, SpecificationAttributeService>();
            services.AddTransient<IProductAttributeParserService, ProductAttributeParserService>();
            services.AddTransient<IProductAttributeService, ProductAttributeService>();
            services.AddTransient<IDateRangeService, DateRangeService>();
            services.AddTransient<IVendorService, VendorService>();
            services.AddTransient<IWorkContextService, WorkContextService>();
            services.AddTransient<ICustomerService, CustomerService>();
            services.AddTransient<IManufacturerService, ManufacturerService>();
            services.AddTransient<IShoppingCartService, ShoppingCartService>();
            services.AddTransient<IGenericAttributeService, GenericAttributeService>();
            services.AddTransient<Inovatiqa.Services.Authentication.Interfaces.IAuthenticationService, Inovatiqa.Services.Authentication.CookieAuthenticationService>();
            services.AddTransient<IShoppingCartService, ShoppingCartService>();
            services.AddTransient<IPriceCalculationService, PriceCalculationService>();
            services.AddTransient<IShippingService, ShippingService>();
            services.AddTransient<ITopicService, TopicService>();
            services.AddTransient<INewsService, NewsService>();
            services.AddTransient<IPermissionService, PermissionService>();
            services.AddTransient<IOrderTotalCalculationService, OrderTotalCalculationService>();
            services.AddTransient<ICheckoutAttributeParserService, CheckoutAttributeParserService>();
            services.AddTransient<ICheckoutAttributeService, CheckoutAttributeService>();
            services.AddTransient<IOrderProcessingService, OrderProcessingService>();
            services.AddTransient<IProductAttributeFormatterService, ProductAttributeFormatterService>();
            services.AddTransient<ICustomerActivityService, CustomerActivityService>();
            services.AddTransient<ICountryService, CountryService>();
            services.AddTransient<IStateProvinceService, StateProvinceService>();
            services.AddTransient<ICustomerAttributeService, CustomerAttributeService>();
            services.AddTransient<IDownloadService, DownloadService>();
            services.AddTransient<ICustomerAttributeParserService, CustomerAttributeParserService>();
            services.AddTransient<ICustomerRegistrationService, CustomerRegistrationService>();
            services.AddTransient<IEncryptionService, EncryptionService>();
            services.AddTransient<INewsLetterSubscriptionService, NewsLetterSubscriptionService>();
            services.AddTransient<IWorkflowMessageService, WorkflowMessageService>();
            services.AddTransient<IMessageTemplateService, MessageTemplateService>();
            services.AddTransient<IMessageTokenProviderService, MessageTokenProviderService>();
            services.AddTransient<IEmailAccountService, EmailAccountService>();
            services.AddTransient<IAddressService, AddressService>();
            services.AddTransient<IAddressAttributeService, AddressAttributeService>();
            services.AddTransient<IAddressAttributeParserService, AddressAttributeParserService>();
            services.AddTransient<ITokenizerService, TokenizerService>();
            services.AddTransient<IQueuedEmailService, QueuedEmailService>();
            services.AddTransient<IAddressAttributeFormatterService, AddressAttributeFormatterService>();
            services.AddTransient<IDiscountService, DiscountService>();
            services.AddTransient<IPaymentService, PaymentService>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<ILoggerService, LoggerService>();
            services.AddTransient<ICompareProductsService, CompareProductsService>();
            services.AddTransient<IOrderReportService, OrderReportService>();
            services.AddTransient<ISettingService, SettingService>();
            services.AddTransient<IRecentlyViewedProductsService, RecentlyViewedProductsService>();
            services.AddTransient<IReviewTypeService, ReviewTypeService>();
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<IOrderTotalCalculationService, OrderTotalCalculationService>();
            services.AddTransient<IShippingRateComputationMethodService, FedexComputationMethodService>();
            services.AddTransient<IFedexService, FedexService>();
            services.AddTransient<ICurrencyService, CurrencyService>();
            services.AddTransient<IMeasureService, MeasureService>();
            services.AddTransient<IShipmentTrackerService, FedexShipmentTrackerService>();
            services.AddTransient<IReturnRequestService, ReturnRequestService>();
            services.AddTransient<ISearchTermService, SearchTermService>();
            services.AddTransient<IAclService, AclService>();

            var config = new MapperConfiguration(cfg =>
            {

                cfg.AddProfile(typeof(AdminMapperConfiguration));
            });

            AutoMapperConfiguration.Init(config);

            services.AddTransient<IReturnRequestModelFactory, ReturnRequestModelFactory>();
            services.AddTransient<IOrderModelFactory, OrderModelFactory>();
            services.AddTransient<IPaymentModelFactory, PaymentModelFactory>();
            services.AddTransient<ICatalogModelFactory, CatalogModelFactory>();
            services.AddTransient<IProductModelFactory, ProductModelFactory>();
            services.AddTransient<ITopicModelFactory, TopicModelFactory>();
            services.AddTransient<IEventPublisher, EventPublisher>();
            services.AddTransient<INewsModelFactory, NewsModelFactory>();
            services.AddTransient<ICommonModelFactory, CommonModelFactory>();
            services.AddTransient<IShoppingCartModelFactory, ShoppingCartModelFactory>();
            services.AddTransient<INewsletterModelFactory, NewsletterModelFactory>();
            services.AddTransient<ICustomerModelFactory, CustomerModelFactory>();
            services.AddTransient<ICountryModelFactory, CountryModelFactory>();
            services.AddTransient<IAddressModelFactory, AddressModelFactory>();
            services.AddTransient<IPaymentModelFactory, PaymentModelFactory>();
            services.AddTransient<Inovatiqa.Web.Areas.Admin.Factories.Interfaces.IShoppingCartModelFactory, Inovatiqa.Web.Areas.Admin.Factories.ShoppingCartModelFactory>();

            services.AddTransient<Inovatiqa.Web.Areas.Admin.Factories.Interfaces.ICountryModelFactory, Inovatiqa.Web.Areas.Admin.Factories.CountryModelFactory>();
            services.AddTransient<Inovatiqa.Web.Areas.Admin.Factories.Interfaces.IReportModelFactory, Inovatiqa.Web.Areas.Admin.Factories.ReportModelFactory>();
            services.AddTransient<Inovatiqa.Web.Areas.Admin.Factories.Interfaces.IVendorModelFactory, Inovatiqa.Web.Areas.Admin.Factories.VendorModelFactory>();
            services.AddTransient<Inovatiqa.Web.Areas.Admin.Factories.Interfaces.ICustomerModelFactory, Inovatiqa.Web.Areas.Admin.Factories.CustomerModelFactory>();
            services.AddTransient<Inovatiqa.Web.Areas.Admin.Factories.Interfaces.ICustomerRoleModelFactory, Inovatiqa.Web.Areas.Admin.Factories.CustomerRoleModelFactory>();
            services.AddTransient<Inovatiqa.Web.Areas.Admin.Factories.Interfaces.IReturnRequestModelFactory, Inovatiqa.Web.Areas.Admin.Factories.ReturnRequestModelFactory>();
            services.AddTransient<Inovatiqa.Web.Areas.Admin.Factories.Interfaces.IManufacturerModelFactory, Inovatiqa.Web.Areas.Admin.Factories.ManufacturerModelFactory>();
            services.AddTransient<Inovatiqa.Web.Areas.Admin.Factories.Interfaces.ICategoryModelFactory, Inovatiqa.Web.Areas.Admin.Factories.CategoryModelFactory>();
            services.AddTransient<Inovatiqa.Web.Areas.Admin.Factories.Interfaces.IHomeModelFactory, Inovatiqa.Web.Areas.Admin.Factories.HomeModelFactory>();
            services.AddTransient<Inovatiqa.Web.Areas.Admin.Factories.Interfaces.ICommonModelFactory, Inovatiqa.Web.Areas.Admin.Factories.CommonModelFactory>();
            services.AddTransient<Inovatiqa.Web.Areas.Admin.Factories.Interfaces.IOrderModelFactory, Inovatiqa.Web.Areas.Admin.Factories.OrderModelFactory>();
            services.AddTransient<Inovatiqa.Web.Factories.Interfaces.IPaymentModelFactory, Inovatiqa.Web.Factories.PaymentModelFactory>();
            services.AddTransient<Inovatiqa.Web.Areas.Admin.Factories.Interfaces.IProductModelFactory, Inovatiqa.Web.Areas.Admin.Factories.ProductModelFactory>();
            services.AddTransient<Inovatiqa.Web.Areas.Admin.Factories.Interfaces.IBaseAdminModelFactory, Inovatiqa.Web.Areas.Admin.Factories.BaseAdminModelFactory>();
            services.AddTransient<Inovatiqa.Web.Areas.Admin.Factories.Interfaces.ISettingModelFactory, Inovatiqa.Web.Areas.Admin.Factories.SettingModelFactory>();
            services.AddTransient<Inovatiqa.Web.Areas.Admin.Factories.Interfaces.IActivityLogModelFactory, Inovatiqa.Web.Areas.Admin.Factories.ActivityLogModelFactory>();

            services.AddTransient<IProductTemplateService, ProductTemplateService>();
            services.AddTransient<ICustomerReportService, CustomerReportService>();
            services.AddTransient<IGeoLookupService, GeoLookupService>();
            services.AddTransient<IManufacturerTemplateService, ManufacturerTemplateService>();
            services.AddTransient<ICategoryTemplateService, CategoryTemplateService>();
            services.AddTransient<ILocalizedModelFactory, LocalizedModelFactory>();
            services.AddTransient<IAclSupportedModelFactory, AclSupportedModelFactory>();
            services.AddTransient<IPaymentMethodService, PaymentMethodService>();
            services.AddTransient<IVendorAttributeService, VendorAttributeService>();
            services.AddTransient<IVendorAttributeParserService, VendorAttributeParserService>();
            services.AddTransient<ICheckoutModelFactory, CheckoutModelFactory>();
            services.AddTransient<ICheckoutAttributeFormatterService, CheckoutAttributeFormatterService>();
            services.AddTransient<ICustomNumberFormatterService, CustomNumberFormatterService>();
            services.AddTransient<IPdfService, PdfService>();
            services.AddTransient<IShipmentService, ShipmentService>();

            services.AddTransient<Inovatiqa.Web.Areas.Admin.Factories.Interfaces.IAddressAttributeModelFactory, Inovatiqa.Web.Areas.Admin.Factories.AddressAttributeModelFactory>();


            services.AddTransient<IPriceFormatter, PriceFormatter>();
            services.AddTransient<IInovatiqaFileProvider, InovatiqaFileProvider>();
            services.AddTransient<IDateTimeHelperService, DateTimeHelperService>();
            services.AddTransient<IWebHelper, WebHelper>();
            services.AddTransient<IActionContextAccessor, ActionContextAccessor>();
            services.AddTransient<ISquarePaymentManagerService, SquarePaymentManagerService>();
            services.AddTransient<ISquareAuthorizationHttpClientService, SquareAuthorizationHttpClientService>();

            services.AddTransient<SlugRouteTransformer>();
            services.AddTransient<HttpClient>();
            services.AddTransient<InovatiqaHttpClient>();

            services.AddSingleton<IStaticCacheManager, MemoryCacheManager>();
            services.AddSingleton<ICacheKeyService, CacheKeyService>();
            services.AddSingleton<IPageHeadBuilder, PageHeadBuilder>();

            //services.AddHostedService<UpdateEntityCounterTaskService>();
            //services.AddHostedService<EmailSenderTaskService>();
            //services.AddHostedService<DeleteGuestsTaskService>();
            //services.AddHostedService<UpdateRootCategoryIdsForProductsTaskService>();
            services.AddHostedService<UpdateRolesForEachCategoryTaskService>();

            services.AddElasticsearch(_configuration);

            TypeDescriptor.AddAttributes(typeof(ShippingOption), new TypeConverterAttribute(typeof(ShippingOptionTypeConverter)));
            TypeDescriptor.AddAttributes(typeof(List<ShippingOption>), new TypeConverterAttribute(typeof(ShippingOptionListTypeConverter)));
            TypeDescriptor.AddAttributes(typeof(IList<ShippingOption>), new TypeConverterAttribute(typeof(ShippingOptionListTypeConverter)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSession();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseStatusCodePages(async context =>
            {
                //handle 404 Not Found
                if (context.HttpContext.Response.StatusCode == StatusCodes.Status404NotFound)
                {
                    try
                    {
                        //get new path
                        var pageNotFoundPath = "/page-not-found";
                        //re-execute request with new path
                        context.HttpContext.Response.Redirect(context.HttpContext.Request.PathBase + pageNotFoundPath);
                    }
                    finally
                    {
                    }

                    await Task.CompletedTask;
                }
            });

            var pattern = "{SeName}";
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDynamicControllerRoute<SlugRouteTransformer>(pattern);

                endpoints.MapControllerRoute(name: "areaRoute",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                name: "Default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                name: "GenericUrl",
                pattern: "{GenericSeName}",
                new { controller = "Common", action = "GenericUrl" });


                endpoints.MapControllerRoute("Homepage", "",
                    new { controller = "Home", action = "Index" });

                endpoints.MapControllerRoute("ContactUs", "Contact-Us",
                new { controller = "Common", action = "ContactUs" });

                endpoints.MapControllerRoute("OrderStatus", "Order-Status",
                       new { controller = "Common", action = "OrderStatus" });

                endpoints.MapControllerRoute("ReturnPolicy", "Return-Policy",
                new { controller = "Common", action = "ReturnPolicy" });

                endpoints.MapControllerRoute("ShippingPolicy", "Shipping-Policy",
                new { controller = "Common", action = "ShippingPolicy" });

                endpoints.MapControllerRoute("PaymentOptions", "Payment-Options",
                new { controller = "Common", action = "PaymentOptions" });

                endpoints.MapControllerRoute("ReOrderManagement", "Reorder-Management",
                new { controller = "Common", action = "ReOrderManagement" });

                endpoints.MapControllerRoute("FAQHelpCenter", "Faq-Help-Center",
                new { controller = "Common", action = "FAQHelpCenter" });

                endpoints.MapControllerRoute("RXPrescription", "RX-Prescription",
                new { controller = "Common", action = "RXPrescription" });

                endpoints.MapControllerRoute("ECatalogs", "ecatalogs",
                new { controller = "Common", action = "ECatalogs" });

                endpoints.MapControllerRoute("RequestQuote", "Request-Quote",
                new { controller = "Common", action = "RequestQuote" });

                endpoints.MapControllerRoute("ContractPricing", "Contract-Pricing",
                new { controller = "Common", action = "ContractPricing" });

                endpoints.MapControllerRoute("PaymentTerms", "Payment-Terms",
                new { controller = "Common", action = "PaymentTerms" });

                endpoints.MapControllerRoute("GPOPurchasing", "Group-Purchasing-Organization",
                new { controller = "Common", action = "GPOPurchasing" });


                endpoints.MapControllerRoute("PriceMatch", "Price-Match-Policy",
                new { controller = "Common", action = "PriceMatch" });

                endpoints.MapControllerRoute("CustomersWeServe", "customers-we-serve",
                new { controller = "Common", action = "CustomersWeServe" });

                endpoints.MapControllerRoute("WhyInovatiqa", "whyinovatiqa",
                new { controller = "Common", action = "WhyInovatiqa" });

                endpoints.MapControllerRoute("Brands", "Brands",
                new { controller = "Common", action = "Brands" });

                endpoints.MapControllerRoute("Testimonials", "Testimonials",
                new { controller = "Common", action = "Testimonials" });

                endpoints.MapControllerRoute("PurchaseOrders", "Purchase-Order",
                new { controller = "Common", action = "PurchaseOrders" });

                endpoints.MapControllerRoute("AboutUs", "About-Us",
                new { controller = "Common", action = "AboutUs" });
 

                //endpoints.MapControllerRoute("EuCookieLawAccept", "eucookielawaccept",
                //new { controller = "Common", action = "EuCookieLawAccept" });

                endpoints.MapControllerRoute("ProductSearchAutoComplete", "catalog/searchtermautocomplete",
                new { controller = "Catalog", action = "SearchTermAutoComplete" });

                endpoints.MapControllerRoute("ProductSearch", "product-search",
                new { controller = "Product", action = "ProductSearch" });

                endpoints.MapControllerRoute("AddProductToCart-Catalog",
                "addproducttocart/catalog/{productId:min(0)}/{shoppingCartTypeId:min(0)}/{quantity:min(0)}/{wishListId:min(0)}",
                new { controller = "ShoppingCart", action = "AddProductToCart_Catalog" });

                endpoints.MapControllerRoute("GetStatesByCountryId", "country/getstatesbycountryid/",
                new { controller = "Country", action = "GetStatesByCountryId" });

                endpoints.MapControllerRoute("CustomerAddressAdd", $"customer/addressadd",
                new { controller = "Customer", action = "AddressAdd" });

                endpoints.MapControllerRoute("Register", "register/{type?}",
                new { controller = "Customer", action = "Register" });

                endpoints.MapControllerRoute("RegisterResult",
                "registerresult/{resultId:min(0)}",
                new { controller = "Customer", action = "RegisterResult" });

                endpoints.MapControllerRoute("Login", "login/",
                new { controller = "Customer", action = "Login" });

                endpoints.MapControllerRoute("Logout", "logout/",
                new { controller = "Customer", action = "Logout" });

                endpoints.MapControllerRoute("ShoppingCart", "cart/",
                new { controller = "ShoppingCart", action = "Cart" });

                endpoints.MapControllerRoute("Wishlist", "Wishlist/{customerGuid?}/{wishListId:min(0)?}",
                new { controller = "ShoppingCart", action = "Wishlist" });

                endpoints.MapControllerRoute("WishlistByName", "Wishlist/{wishListId:min(0)?}",
                new { controller = "ShoppingCart", action = "WishlistByName" });

                endpoints.MapControllerRoute("AddProductToCart-WishList",
                "addproducttocart/wishlist/{qty:min(0)}",
                new { controller = "ShoppingCart", action = "AddItemsToCartFromWishlist" });

                endpoints.MapControllerRoute("AddProductToCart-ProductListing",
                "addproducttocart/productlisting/{productId:min(0)}/{quantity:min(0)}",
                new { controller = "ShoppingCart", action = "AddProductToCart_ProductListing" });

                endpoints.MapControllerRoute("AddbulkProductToCart_WishList",
                "addbulkproducttocart/WishList",
                new { controller = "ShoppingCart", action = "AddbulkProductToCart_WishList" });

                endpoints.MapControllerRoute("CompareProducts", "Compare-Products",
                new { controller = "Product", action = "CompareProducts" });

                endpoints.MapControllerRoute("AddProductToCompare", "compareproducts/add/{productId:min(0)}",
                new { controller = "Product", action = "AddProductToCompareList" });

                endpoints.MapControllerRoute("ClearCompareList", "clearcomparelist/",
                new { controller = "Product", action = "ClearCompareList" });

                endpoints.MapControllerRoute("ClearCompareListWithoutRedirect", "clearcomparelistwithoutredirect/",
                new { controller = "Product", action = "ClearCompareListWithoutRedirect" });

                endpoints.MapControllerRoute("RemoveProductFromCompareList",
                "compareproducts/remove/{productId}",
                new { controller = "Product", action = "RemoveProductFromCompareList" });

                endpoints.MapControllerRoute("RemoveProductFromCompareListAndStayonSamePage",
                "compareproducts/removeproduct/{productId}",
                new { controller = "Product", action = "RemoveProductFromCompareListAndStayonSamePage" });

                endpoints.MapControllerRoute("ManufacturerList", "manufacturer/all/",
               new { controller = "Catalog", action = "ManufacturerAll" });

                //add product to cart (with attributes and options). used on the product details pages.
                endpoints.MapControllerRoute("AddProductToCart-Details",
                "addproducttocart/details/{productId:min(0)}/{shoppingCartTypeId:min(0)}/{wishListId:min(0)}",
                new { controller = "ShoppingCart", action = "AddProductToCart_Details" });

                endpoints.MapControllerRoute("AddProductToCart-Details",
                "addproducttocartusingSKU/details/{skuId:min(0)}/{shoppingCartTypeId:min(0)}/{wishListId:min(0)}",
                new { controller = "ShoppingCart", action = "AddProductToCartSKU_Details" });

                //update add product to cart (with attributes and options). used on the product details pages.
                endpoints.MapControllerRoute("UpdateProductToCart-Details",
                "updateproducttocart/details/{productId:min(0)}/{shoppingCartTypeId:min(0)}/{wishListId:min(0)}",
                new { controller = "ShoppingCart", action = "UpdateProductToCart_Details" });

                //define this routes to use in UI views (in case if you want to customize some of them later)
                endpoints.MapControllerRoute("Product", pattern,
                    new { controller = "Product", action = "ProductDetails" });

                endpoints.MapControllerRoute("PageNotFound", "page-not-found",
                new { controller = "Common", action = "PageNotFound" });

                endpoints.MapControllerRoute("Category", pattern,
                    new { controller = "Catalog", action = "Category" });

                endpoints.MapControllerRoute("Manufacturer", pattern,
                    new { controller = "Catalog", action = "Manufacturer" });

                endpoints.MapControllerRoute("Vendor", pattern,
                    new { controller = "Catalog", action = "Vendor" });

                endpoints.MapControllerRoute("NewsItem", pattern,
                    new { controller = "News", action = "NewsItem" });

                endpoints.MapControllerRoute("BlogPost", pattern,
                    new { controller = "Blog", action = "BlogPost" });

                endpoints.MapControllerRoute("Topic", pattern,
                    new { controller = "Topic", action = "TopicDetails" });

                endpoints.MapControllerRoute("ProductsByTag", pattern,
                    new { controller = "Catalog", action = "ProductsByTag" });

                endpoints.MapControllerRoute("Products", "products",
                new { controller = "Product", action = "Products" });

                endpoints.MapControllerRoute("SetProductReviewHelpfulness", $"setproductreviewhelpfulness",
                new { controller = "Product", action = "SetProductReviewHelpfulness" });

                endpoints.MapControllerRoute("RecentlyViewedProducts", $"recentlyviewedproducts/",
                new { controller = "Product", action = "RecentlyViewedProducts" });

                endpoints.MapControllerRoute("ProductReviews", "productreviews/{productId}",
                    new { controller = "Product", action = "ProductReviews" });

                endpoints.MapControllerRoute("CustomerProductReviews", $"customer/productreviews",
                    new { controller = "Product", action = "CustomerProductReviews" });

                endpoints.MapControllerRoute("CustomerProductReviewsPaged",
                    "customer/productreviews/page/{pageNumber:min(0)}",
                    new { controller = "Product", action = "CustomerProductReviews" });

                //endpoints.MapControllerRoute("Checkout", pattern,
                //    new { controller = "Checkout", action = "Index" });

                endpoints.MapControllerRoute("Checkout", $"checkout/",
                new { controller = "Checkout", action = "Index" });

                endpoints.MapControllerRoute("NewProducts", $"newproducts/",
                new { controller = "Product", action = "NewProducts" });

                endpoints.MapControllerRoute("ShopProducts", $"Shop-Products/",
                new { controller = "Product", action = "ShopProducts" });

                endpoints.MapControllerRoute("NewProductsRSS", $"newproducts/rss",
                new { controller = "Product", action = "NewProductsRss" });

                endpoints.MapControllerRoute("EstimateShipping", $"cart/estimateshipping",
                new { controller = "ShoppingCart", action = "GetEstimateShipping" });

                endpoints.MapControllerRoute("CheckoutOnePage", $"onepagecheckout/",
                    new { controller = "Checkout", action = "OnePageCheckout" });

                endpoints.MapControllerRoute("CheckoutShippingAddress", $"checkout/shippingaddress",
                    new { controller = "Checkout", action = "ShippingAddress" });

                endpoints.MapControllerRoute("CheckoutSelectShippingAddress", $"checkout/selectshippingaddress",
                    new { controller = "Checkout", action = "SelectShippingAddress" });

                endpoints.MapControllerRoute("CheckoutBillingAddress", $"checkout/billingaddress",
                    new { controller = "Checkout", action = "BillingAddress" });

                endpoints.MapControllerRoute("CheckoutSelectBillingAddress", $"checkout/selectbillingaddress",
                    new { controller = "Checkout", action = "SelectBillingAddress" });

                endpoints.MapControllerRoute("CheckoutShippingMethod", $"checkout/shippingmethod",
                    new { controller = "Checkout", action = "ShippingMethod" });

                endpoints.MapControllerRoute("CheckoutPaymentMethod", $"checkout/paymentmethod",
                    new { controller = "Checkout", action = "PaymentMethod" });

                endpoints.MapControllerRoute("CheckoutPaymentInfo", $"checkout/paymentinfo",
                    new { controller = "Checkout", action = "PaymentInfo" });

                endpoints.MapControllerRoute("CheckoutConfirm", $"checkout/confirm",
                    new { controller = "Checkout", action = "Confirm" });

                endpoints.MapControllerRoute("CheckoutCompleted",
                    pattern + "checkout/completed/{orderId:int}",
                    new { controller = "Checkout", action = "Completed" });

                endpoints.MapControllerRoute("CustomerOrders", $"Order/History",
                new { controller = "Order", action = "CustomerOrders" });

                endpoints.MapControllerRoute("CustomerInvoicedOrders", $"invoicedorders",
                new { controller = "Order", action = "CustomerInvoicedOrders" });

                endpoints.MapControllerRoute("PrintOrderDetails",
                "orderdetails/print/{orderId}",
                new { controller = "Order", action = "PrintOrderDetails" });

                endpoints.MapControllerRoute("GetOrderPdfInvoice",
                "orderdetails/pdf/{orderId}",
                new { controller = "Order", action = "GetPdfInvoice" });

                endpoints.MapControllerRoute("ShipmentDetails",
                "orderdetails/shipment/{shipmentId}",
                new { controller = "Order", action = "ShipmentDetails" });

                endpoints.MapControllerRoute("ReOrder",
                "reorder/{orderId:min(0)}",
                new { controller = "Order", action = "ReOrder" });

                endpoints.MapControllerRoute("ReturnRequest",
                "returnrequest/{orderId:min(0)}",
                new { controller = "ReturnRequest", action = "ReturnRequest" });

                endpoints.MapControllerRoute("CreateReturnRequest", $"Return/Create-Step-1",
                new { controller = "ReturnRequest", action = "CreateReturnRequest" });

                endpoints.MapControllerRoute("CustomerInfo", $"customer/info",
                new { controller = "Customer", action = "Info" });

                endpoints.MapControllerRoute("OrderDetails",
                "orderdetails/{orderId:min(0)}",
                new { controller = "Order", action = "Details" });

                endpoints.MapControllerRoute("OrderShipments",
                "ordershipment/{orderId:min(0)}",
                new { controller = "Order", action = "OrderShipments" });

                endpoints.MapControllerRoute("CheckoutCompleted",
                "checkout/completed/{orderId:int}",
                new { controller = "Checkout", action = "Completed" });

                endpoints.MapControllerRoute("CustomerReturnRequests", $"Return/History",
                new { controller = "ReturnRequest", action = "CustomerReturnRequests" });

                endpoints.MapControllerRoute("ViewWishLists", $"My-Lists",
                new { controller = "ShoppingCart", action = "ViewAllWishlists" });

                endpoints.MapControllerRoute("CreateWishList", $"Create-List",
                new { controller = "ShoppingCart", action = "CreateWishList" });

                endpoints.MapControllerRoute("ReorderGuide", $"order/reorderguide",
                new { controller = "Order", action = "ReorderGuide" });

                endpoints.MapControllerRoute("AddProductToCart-Reorder",
                "addproducttocart/reorder/{orderId:min(0)}/{orderLineId:min(0)}/{qty:min(0)}",
                new { controller = "Order", action = "AddProductToCart_Reorder" });

                endpoints.MapControllerRoute("AddbulkProductToCart_Reorder",
                "addbulkproducttocart/reorder",
                new { controller = "Order", action = "AddbulkProductToCart_Reorder" });

                endpoints.MapControllerRoute("AddbulkProductToCart_Details",
                "addbulkproducttocart/details",
                new { controller = "Order", action = "AddbulkProductToCart_Reorder" });

                endpoints.MapControllerRoute("CustomerSuspendedCarts", $"suspendedcarts",
                new { controller = "ShoppingCart", action = "SuspendedCarts" });

                endpoints.MapControllerRoute("CustomerOrderItems", $"order/items/history",
                new { controller = "Order", action = "CustomerOrderItems" });

                endpoints.MapControllerRoute("CustomerAddresses", $"customer/addresses",
                new { controller = "Customer", action = "Addresses" });

                endpoints.MapControllerRoute("CustomerChangePassword", $"customer/change-password",
                new { controller = "Customer", action = "ChangePassword" });

                //passwordrecovery
                endpoints.MapControllerRoute("PasswordRecovery", $"{pattern}passwordrecovery",
                    new { controller = "Customer", action = "PasswordRecovery" });

                //password recovery confirmation
                endpoints.MapControllerRoute("PasswordRecoveryConfirm", "PasswordRecoveryConfirm/{customerGuid?}/{token?}",
                    new { controller = "Customer", action = "PasswordRecoveryConfirm" });
                //Account Activation
                endpoints.MapControllerRoute("AccountActivation", "Activate/{customerGuid?}",
                    new { controller = "Customer", action = "AccountActivate" });

                endpoints.MapControllerRoute("CustomerChangePassword", $"{pattern}customer/changepassword",
                new { controller = "Customer", action = "ChangePassword" });

                endpoints.MapControllerRoute("PaymentPortal", "Payment-Portal",
                new { controller = "Payment", action = "PaymentPortal" });

                endpoints.MapControllerRoute("OpenInvoices", "Open-Invoices",
                new { controller = "Payment", action = "OpenInvoices" });

                endpoints.MapControllerRoute("UnappliedCash", "Unapplied-Cash",
                new { controller = "Payment", action = "UnappliedCash" });

                endpoints.MapControllerRoute("PaidInvoices", "Paid-Invoices",
                new { controller = "Payment", action = "PaidInvoices" });

                endpoints.MapControllerRoute("MakePayment", "makepayment",
                new { controller = "Payment", action = "MakePayment" });

                endpoints.MapControllerRoute("SavePaymentInfo", "savepaymentinfo",
                new { controller = "Payment", action = "SavePaymentInfo" });

                endpoints.MapControllerRoute("MakingPayment", "makingpayment",
                new { controller = "Payment", action = "MakingPayment" });

                endpoints.MapControllerRoute("MakingPayment", "makingpayment",
                new { controller = "Payment", action = "MakingPayment" });

                endpoints.MapControllerRoute("SavePaymentInfo", "savepaymentinfo",
                new { controller = "Payment", action = "SavePaymentInfo" });


                endpoints.MapControllerRoute("FooterSendEmails", "footersendemails",
                new { controller = "Payment", action = "FooterSendEmails" });


                endpoints.MapControllerRoute("AccountInformation", $"customer/accountinfo",
                new { controller = "Customer", action = "AccountInformation" });

                endpoints.MapControllerRoute("AddSubAccount", $"accounts/subaccount/add",
                    new { controller = "Customer", action = "AddSubAccount" });
                endpoints.MapControllerRoute("ManageSubAccounts", $"accounts/managesubaccounts",
                    new { controller = "customer", action = "ManageSubAccounts" });

                endpoints.MapControllerRoute("ShippingPdfInvoice",
                "shippinginvoice/pdf",
                new { controller = "Payment", action = "ShippingPdfInvoice" });
                endpoints.MapControllerRoute("ApproveOrders", $"Order-Approvals",
                    new { controller = "Order", action = "ApproveOrder" });
                endpoints.MapControllerRoute("MakeAchPayment", $"process-payment",
                    new { controller = "Payment", action = "MakeAchPayment" });

                endpoints.MapControllerRoute("ShoppingCart", $"viewsuspendedcart",
                    new { controller = "ShoppingCart", action = "SuspendedCartItems" });

                endpoints.MapControllerRoute("Home", "essentials",
                    new { controller = "Home", action = "EssentialTasks" });

                endpoints.MapControllerRoute("accountinfoedit", "edit-account-info",
                    new { controller = "Customer", action = "EditAccountInfo" });

                endpoints.MapControllerRoute("BillingAddressList", "Billing-Address",
                    new { controller = "Customer", action = "ChooseBillingAddress" });
                endpoints.MapControllerRoute("ShippingAddressList", "Shipping-Address",
                    new { controller = "Customer", action = "ChooseShippingAddress" });
                endpoints.MapControllerRoute("SwitchDefaultAddress", "customer/address/switch/{type}",
                    new { controller = "Customer", action = "SwitchDefaultAddress" });
                endpoints.MapControllerRoute("SwitchAddress", "customer/address/switch/{type}/{id}",
                    new { controller = "Customer", action = "SwitchAddress" });

                endpoints.MapControllerRoute("InternationalShipping", "international-shipping",
                    new { controller = "Common", action = "InternationalShipping" });

                endpoints.MapControllerRoute("DistributionServices", "/distribution-services",
                    new { controller = "Common", action = "DistributionServices" });

                endpoints.MapControllerRoute("HealthAccounts", "Health-Accounts",
                    new { controller = "Common", action = "HealthAccounts" });

                endpoints.MapControllerRoute("GovernmentContracts", "Government-Contracts",
                    new { controller = "Common", action = "GovernmentContracts" });

                endpoints.MapControllerRoute("GuestCart", "guest/cart",
                    new { controller = "ShoppingCart", action = "GuestCart" });

                endpoints.MapControllerRoute("GuestCheckout", "guest/checkout",
                    new { controller = "ShoppingCart", action = "GuestCheckout" });

                endpoints.MapControllerRoute("ChangeFlyoutQuantity", "flyout/update/quantity",
                    new { controller = "ShoppingCart", action = "ChangeQuantities" });

                endpoints.MapControllerRoute("ScheduledOrders", "scheduled-orders",
                    new { controller = "Order", action = "ScheduledOrders" });

                endpoints.MapControllerRoute("InvoicedOrderDetails", "InvoicedOrderDetails/{orderId:min(0)}",
                    new { controller = "Order", action = "InvoicedOrderDetails" });
             
            });
        }
    }
}