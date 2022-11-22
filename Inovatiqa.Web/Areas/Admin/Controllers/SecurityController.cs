using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Logging.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    public partial class SecurityController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ILoggerService _loggerService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContextService _workContextService;

        #endregion

        #region Ctor

        public SecurityController(ICustomerService customerService,
            ILoggerService loggerService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IWorkContextService workContextService,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _customerService = customerService;
            _loggerService = loggerService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _workContextService = workContextService;
        }

        #endregion

        #region Methods

        public virtual IActionResult AccessDenied(string pageUrl)
        {
            var currentCustomer = _workContextService.CurrentCustomer;
            if (currentCustomer == null || _customerService.IsGuest(currentCustomer))
            {
                _loggerService.Information($"Access denied to anonymous request on {pageUrl}");
                return View();
            }

            _loggerService.Information($"Access denied to user #{currentCustomer.Email} '{currentCustomer.Email}' on {pageUrl}");

            return View();
        }

        #endregion
    }
}