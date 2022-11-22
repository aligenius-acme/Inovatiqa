using System.Net;
using Inovatiqa.Services.Payments;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    public class PaymentPurchaseOrderViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel();
            if (Request.Method != WebRequestMethods.Http.Get)
            {
                model.PurchaseOrderNumber = HttpContext.Request.Form["PurchaseOrderNumber"];
                //added email by hamza in purchase order
                //model.PurchaseOrderEmail = HttpContext.Request.Form["PurchaseOrderEmail"];
            }

            return View("~/Views/PurchaseOrder/PaymentInfo.cshtml", model);
        }
    }
}
