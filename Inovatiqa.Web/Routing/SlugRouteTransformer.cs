using System.Threading.Tasks;
using Inovatiqa.Services.Seo.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Inovatiqa.Web.Defaults;

namespace Inovatiqa.Web.Routing
{
    public class SlugRouteTransformer : DynamicRouteValueTransformer
    {
        #region Fields

        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public SlugRouteTransformer(IUrlRecordService urlRecordService)
        {
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Methods

        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            if (values == null)
                return new ValueTask<RouteValueDictionary>(values);

            if (!values.TryGetValue("SeName", out var slugValue) || string.IsNullOrEmpty(slugValue as string))
                return new ValueTask<RouteValueDictionary>(values);

            var slug = slugValue as string;
            var urlRecord = _urlRecordService.GetBySlug(slug);

            if (urlRecord == null)
                return new ValueTask<RouteValueDictionary>(values);

            var pathBase = httpContext.Request.PathBase;

            switch (urlRecord.EntityName.ToLowerInvariant())
            {
                case "product":
                    values[InovatiqaPathRouteDefaults.ControllerFieldKey] = "Product";
                    values[InovatiqaPathRouteDefaults.ActionFieldKey] = "ProductDetails";
                    values[InovatiqaPathRouteDefaults.ProductIdFieldKey] = urlRecord.EntityId;
                    values[InovatiqaPathRouteDefaults.SeNameFieldKey] = urlRecord.Slug;
                    break;
                case "producttag":
                    values[InovatiqaPathRouteDefaults.ControllerFieldKey] = "Catalog";
                    values[InovatiqaPathRouteDefaults.ActionFieldKey] = "ProductsByTag";
                    values[InovatiqaPathRouteDefaults.ProducttagIdFieldKey] = urlRecord.EntityId;
                    values[InovatiqaPathRouteDefaults.SeNameFieldKey] = urlRecord.Slug;
                    break;
                case "category":
                    values[InovatiqaPathRouteDefaults.ControllerFieldKey] = "Catalog";
                    values[InovatiqaPathRouteDefaults.ActionFieldKey] = "Category";
                    values[InovatiqaPathRouteDefaults.CategoryIdFieldKey] = urlRecord.EntityId;
                    values[InovatiqaPathRouteDefaults.SeNameFieldKey] = urlRecord.Slug;
                    break;
                case "manufacturer":
                    values[InovatiqaPathRouteDefaults.ControllerFieldKey] = "Catalog";
                    values[InovatiqaPathRouteDefaults.ActionFieldKey] = "Manufacturer";
                    values[InovatiqaPathRouteDefaults.ManufacturerIdFieldKey] = urlRecord.EntityId;
                    values[InovatiqaPathRouteDefaults.SeNameFieldKey] = urlRecord.Slug;
                    break;
                case "vendor":
                    values[InovatiqaPathRouteDefaults.ControllerFieldKey] = "Catalog";
                    values[InovatiqaPathRouteDefaults.ActionFieldKey] = "Vendor";
                    values[InovatiqaPathRouteDefaults.VendorIdFieldKey] = urlRecord.EntityId;
                    values[InovatiqaPathRouteDefaults.SeNameFieldKey] = urlRecord.Slug;
                    break;
                case "newsitem":
                    values[InovatiqaPathRouteDefaults.ControllerFieldKey] = "News";
                    values[InovatiqaPathRouteDefaults.ActionFieldKey] = "NewsItem";
                    values[InovatiqaPathRouteDefaults.NewsItemIdFieldKey] = urlRecord.EntityId;
                    values[InovatiqaPathRouteDefaults.SeNameFieldKey] = urlRecord.Slug;
                    break;
                case "blogpost":
                    values[InovatiqaPathRouteDefaults.ControllerFieldKey] = "Blog";
                    values[InovatiqaPathRouteDefaults.ActionFieldKey] = "BlogPost";
                    values[InovatiqaPathRouteDefaults.BlogPostIdFieldKey] = urlRecord.EntityId;
                    values[InovatiqaPathRouteDefaults.SeNameFieldKey] = urlRecord.Slug;
                    break;
                case "topic":
                    values[InovatiqaPathRouteDefaults.ControllerFieldKey] = "Topic";
                    values[InovatiqaPathRouteDefaults.ActionFieldKey] = "TopicDetails";
                    values[InovatiqaPathRouteDefaults.TopicIdFieldKey] = urlRecord.EntityId;
                    values[InovatiqaPathRouteDefaults.SeNameFieldKey] = urlRecord.Slug;
                    break;
                default:
                    break;
            }

            return new ValueTask<RouteValueDictionary>(values);
        }

        #endregion
    }
}