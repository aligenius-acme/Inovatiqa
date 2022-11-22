using System;
using System.Collections.Generic;
using Inovatiqa.Core;
using Inovatiqa.Services.Logging.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace Inovatiqa.Services.Messages
{
    public partial class NotificationService : INotificationService
    {
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILoggerService _loggerService;
        private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
        private readonly IWorkContextService _workContextService;

        #endregion

        #region Ctor

        public NotificationService(IHttpContextAccessor httpContextAccessor,
            ILoggerService loggerService,
            ITempDataDictionaryFactory tempDataDictionaryFactory,
            IWorkContextService workContextService)
        {
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
            _tempDataDictionaryFactory = tempDataDictionaryFactory;
            _workContextService = workContextService;
        }

        #endregion

        #region Utilities

        protected virtual void PrepareTempData(NotifyType type, string message, bool encode = true)
        {
            var context = _httpContextAccessor.HttpContext;
            var tempData = _tempDataDictionaryFactory.GetTempData(context);

            var messages = tempData.ContainsKey(InovatiqaDefaults.NotificationListKey)
                ? JsonConvert.DeserializeObject<IList<NotifyData>>(tempData[InovatiqaDefaults.NotificationListKey].ToString())
                : new List<NotifyData>();

            messages.Add(new NotifyData
            {
                Message = message,
                Type = type,
                Encode = encode
            });

            tempData[InovatiqaDefaults.NotificationListKey] = JsonConvert.SerializeObject(messages);
        }

        protected virtual void LogException(Exception exception)
        {
            if (exception == null)
                return;
            var customer = _workContextService.CurrentCustomer;
            _loggerService.Error(exception.Message, exception, customer);
        }

        #endregion

        #region Methods

        public virtual void Notification(NotifyType type, string message, bool encode = true)
        {
            PrepareTempData(type, message, encode);
        }

        public virtual void SuccessNotification(string message, bool encode = true)
        {
            PrepareTempData(NotifyType.Success, message, encode);
        }

        public virtual void WarningNotification(string message, bool encode = true)
        {
            PrepareTempData(NotifyType.Warning, message, encode);
        }

        public virtual void ErrorNotification(string message, bool encode = true)
        {
            PrepareTempData(NotifyType.Error, message, encode);
        }

        public virtual void ErrorNotification(Exception exception, bool logException = true)
        {
            if (exception == null)
                return;

            if (logException)
                LogException(exception);

            ErrorNotification(exception.Message);
        }

        #endregion
    }
}
