using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Web.Extensions;

namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial class ActivityLogModelFactory : IActivityLogModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelperService _dateTimeHelperService;

        #endregion

        #region Ctor

        public ActivityLogModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDateTimeHelperService dateTimeHelperService)
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _dateTimeHelperService = dateTimeHelperService;
        }

        #endregion

        #region Utilities

        protected virtual IList<ActivityLogTypeModel> PrepareActivityLogTypeModels()
        {
            var availableActivityTypes = _customerActivityService.GetAllActivityTypes();
            var models = availableActivityTypes.Select(activityType => activityType.ToActivityLogTypeModel<ActivityLogTypeModel>()).ToList();

            return models;
        }

        #endregion

        #region Methods

        public virtual ActivityLogTypeSearchModel PrepareActivityLogTypeSearchModel(ActivityLogTypeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.ActivityLogTypeListModel = PrepareActivityLogTypeModels();

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual ActivityLogSearchModel PrepareActivityLogSearchModel(ActivityLogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseAdminModelFactory.PrepareActivityLogTypes(searchModel.ActivityLogType);

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual ActivityLogListModel PrepareActivityLogListModel(ActivityLogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = searchModel.CreatedOnFrom == null ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.CreatedOnFrom.Value, _dateTimeHelperService.CurrentTimeZone);
            var endDateValue = searchModel.CreatedOnTo == null ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.CreatedOnTo.Value, _dateTimeHelperService.CurrentTimeZone).AddDays(1);

            var activityLog = _customerActivityService.GetAllActivities(createdOnFrom: startDateValue,
                createdOnTo: endDateValue,
                activityLogTypeId: searchModel.ActivityLogTypeId,
                ipAddress: searchModel.IpAddress,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            if (activityLog is null)
                return new ActivityLogListModel();

            var model = new ActivityLogListModel().PrepareToGrid(searchModel, activityLog, () =>
            {
                var activityLogCustomers = _customerService.GetCustomersByIds(activityLog.GroupBy(x => x.CustomerId).Select(x => x.Key).ToArray());

                return activityLog.Select(logItem =>
                {
                    var logItemModel = logItem.ToActivityLogModel<ActivityLogModel>();
                    logItemModel.ActivityLogTypeName = _customerActivityService.GetActivityTypeById(logItem.ActivityLogTypeId)?.Name;
                    logItemModel.CustomerEmail = activityLogCustomers?.FirstOrDefault(x => x.Id == logItem.CustomerId)?.Email;

                    logItemModel.CreatedOn = _dateTimeHelperService.ConvertToUserTime(logItem.CreatedOnUtc, DateTimeKind.Utc);

                    return logItemModel;

                });
            });

            return model;
        }

        #endregion
    }
}