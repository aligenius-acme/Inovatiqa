using System.Linq;
using Inovatiqa.Services.Media.Interfaces;
using Inovatiqa.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class PictureController : BaseAdminController
    {
        #region Fields

        private readonly IPictureService _pictureService;

        #endregion

        #region Ctor

        public PictureController(IPictureService pictureService,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _pictureService = pictureService;
        }

        #endregion

        #region Methods

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult AsyncUpload()
        {
            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No file uploaded"
                });
            }

            const string qqFileNameParameter = "qqfilename";

            var qqFileName = Request.Form.ContainsKey(qqFileNameParameter)
                ? Request.Form[qqFileNameParameter].ToString()
                : string.Empty;

            var picture = _pictureService.InsertPicture(httpPostedFile, qqFileName);

            return Json(new
            {
                success = true,
                pictureId = picture.Id,
                imageUrl = _pictureService.GetPictureUrl(ref picture, 100)
            });
        }

        #endregion
    }
}