using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Mvc
{
    public class NullJsonResult : JsonResult
    {
        public NullJsonResult() : base(null)
        {
        }
    }
}
