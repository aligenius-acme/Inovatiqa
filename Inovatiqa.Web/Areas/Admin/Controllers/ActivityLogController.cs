using System;
using System.Linq;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Logging;
using Inovatiqa.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    public partial class ActivityLogController : BaseAdminController
    {
        #region Fields

        private readonly IActivityLogModelFactory _activityLogModelFactory;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;

        #endregion

        #region Ctor

        public ActivityLogController(IActivityLogModelFactory activityLogModelFactory,
            ICustomerActivityService customerActivityService,
            INotificationService notificationService,
            IPermissionService permissionService,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _activityLogModelFactory = activityLogModelFactory;
            _customerActivityService = customerActivityService;
            _notificationService = notificationService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        public virtual IActionResult ActivityTypes()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            var model = _activityLogModelFactory.PrepareActivityLogTypeSearchModel(new ActivityLogTypeSearchModel());

            return View(model);
        }

        [HttpPost, ActionName("SaveTypes")]
        public virtual IActionResult SaveTypes(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            _customerActivityService.InsertActivity("EditActivityLogTypes", "Edited activity log types");

            var selectedActivityTypesIds = form["checkbox_activity_types"]
                .SelectMany(value => value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(idString => int.TryParse(idString, out var id) ? id : 0)
                .Distinct().ToList();

            var activityTypes = _customerActivityService.GetAllActivityTypes();
            foreach (var activityType in activityTypes)
            {
                activityType.Enabled = selectedActivityTypesIds.Contains(activityType.Id);
                _customerActivityService.UpdateActivityType(activityType);
            }

            _notificationService.SuccessNotification("The types have been updated successfully.");

            return RedirectToAction("ActivityTypes");
        }

        public virtual IActionResult ActivityLogs()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            var model = _activityLogModelFactory.PrepareActivityLogSearchModel(new ActivityLogSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult ListLogs(ActivityLogSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedDataTablesJson();

            var model = _activityLogModelFactory.PrepareActivityLogListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult ActivityLogDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            var logItem = _customerActivityService.GetActivityById(id)
                ?? throw new ArgumentException("No activity log found with the specified id", nameof(id));

            _customerActivityService.DeleteActivity(logItem);

            _customerActivityService.InsertActivity("DeleteActivityLog",
                "Deleted activity log", logItem.Id);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual IActionResult ClearAll()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            _customerActivityService.ClearAllActivities();

            _customerActivityService.InsertActivity("DeleteActivityLog", "Deleted activity log");

            return RedirectToAction("ActivityLogs");
        }

        #endregion
    }
}