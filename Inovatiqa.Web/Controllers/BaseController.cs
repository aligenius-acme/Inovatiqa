using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text.Encodings.Web;
using Inovatiqa.Web.Mvc.Filters;
using Inovatiqa.Core;
using System.Net;

namespace Inovatiqa.Web.Controllers
{
    [ValidatePassword]
    [SaveLastActivity]
    [SaveLastVisitedPage]
    public abstract class BaseController : Controller
    {
        protected IRazorViewEngine _viewEngine;

        public BaseController(IRazorViewEngine viewEngine)
        {
            _viewEngine = viewEngine;
        }

        #region Rendering

        protected virtual string RenderViewComponentToString(string componentName, object arguments = null)
        {
            if (string.IsNullOrEmpty(componentName))
                throw new ArgumentNullException(nameof(componentName));

            var actionContextAccessor = HttpContext.RequestServices.GetService(typeof(IActionContextAccessor)) as IActionContextAccessor;
            if (actionContextAccessor == null)
                throw new Exception("IActionContextAccessor cannot be resolved");

            var context = actionContextAccessor.ActionContext;

            var viewComponentResult = ViewComponent(componentName, arguments);

            var viewData = ViewData;
            if (viewData == null)
            {
                throw new NotImplementedException();
            }

            var tempData = TempData;
            if (tempData == null)
            {
                throw new NotImplementedException();
            }

            using var writer = new StringWriter();
            var viewContext = new ViewContext(
                context,
                NullView.Instance,
                viewData,
                tempData,
                writer,
                new HtmlHelperOptions());

            var viewComponentHelper = context.HttpContext.RequestServices.GetRequiredService<IViewComponentHelper>();
            (viewComponentHelper as IViewContextAware)?.Contextualize(viewContext);

            var result = viewComponentResult.ViewComponentType == null ?
                viewComponentHelper.InvokeAsync(viewComponentResult.ViewComponentName, viewComponentResult.Arguments) :
                viewComponentHelper.InvokeAsync(viewComponentResult.ViewComponentType, viewComponentResult.Arguments);

            result.Result.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }

        protected virtual string RenderPartialViewToString(string viewName, object model)
        {
            var actionContext = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor, ModelState);

            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;

            ViewData.Model = model;

            var viewResult = _viewEngine.FindView(actionContext, viewName, false);
            if (viewResult.View == null)
            {
                viewResult = _viewEngine.GetView(null, viewName, false);
                if (viewResult.View == null)
                    throw new ArgumentNullException($"{viewName} view was not found");
            }
            using var stringWriter = new StringWriter();
            var viewContext = new ViewContext(actionContext, viewResult.View, ViewData, TempData, stringWriter, new HtmlHelperOptions());

            var t = viewResult.View.RenderAsync(viewContext);
            t.Wait();
            return stringWriter.GetStringBuilder().ToString();
        }

        #endregion

        #region Security

        protected virtual IActionResult AccessDeniedView()
        {
            return RedirectToAction("AccessDenied", "Security", new { pageUrl = InovatiqaDefaults.StoreUrl });
        }

        protected JsonResult AccessDeniedDataTablesJson()
        {
            return ErrorJson("You do not have permission to perform the selected operation.");
        }

        #endregion

        #region Notifications

        protected JsonResult ErrorJson(string error)
        {
            return Json(new
            {
                error = error
            });
        }

        protected JsonResult ErrorJson(object errors)
        {
            return Json(new
            {
                error = errors
            });
        }

        #endregion

        #region Panels and tabs

        public virtual void SaveSelectedPanelName(string tabName, bool persistForTheNextRequest = true)
        {
            if (string.IsNullOrEmpty(tabName))
                throw new ArgumentNullException(nameof(tabName));

            const string dataKey = "inovatiqa.selected-panel-name";
            if (persistForTheNextRequest)
            {
                TempData[dataKey] = tabName;
            }
            else
            {
                ViewData[dataKey] = tabName;
            }
        }

        public virtual void SaveSelectedTabName(string tabName = "", bool persistForTheNextRequest = true)
        {
            SaveSelectedTabName(tabName, "selected-tab-name", null, persistForTheNextRequest);

            if (!Request.Method.Equals(WebRequestMethods.Http.Post, StringComparison.InvariantCultureIgnoreCase))
                return;

            foreach (var key in Request.Form.Keys)
                if (key.StartsWith("selected-tab-name-", StringComparison.InvariantCultureIgnoreCase))
                    SaveSelectedTabName(null, key, key.Substring("selected-tab-name-".Length), persistForTheNextRequest);
        }

        protected virtual void SaveSelectedTabName(string tabName, string formKey, string dataKeyPrefix, bool persistForTheNextRequest)
        {
            if (string.IsNullOrEmpty(tabName))
            {
                tabName = Request.Form[formKey];
            }

            if (string.IsNullOrEmpty(tabName))
                return;

            var dataKey = "inovatiqa.selected-tab-name";
            if (!string.IsNullOrEmpty(dataKeyPrefix))
                dataKey += $"-{dataKeyPrefix}";

            if (persistForTheNextRequest)
            {
                TempData[dataKey] = tabName;
            }
            else
            {
                ViewData[dataKey] = tabName;
            }
        }

        #endregion

    }
}
