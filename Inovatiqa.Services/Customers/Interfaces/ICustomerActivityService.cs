using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Services.Customers.Interfaces
{
    public partial interface ICustomerActivityService
    {
        ActivityLog InsertActivity(string systemKeyword, string comment, int Id, string entityName = null);

        ActivityLog InsertActivity(Customer customer, string systemKeyword, string comment, int Id, string entityName = null);

        IList<ActivityLogType> GetAllActivityTypes();

        IPagedList<ActivityLog> GetAllActivities(DateTime? createdOnFrom = null, DateTime? createdOnTo = null,
            int? customerId = null, int? activityLogTypeId = null, string ipAddress = null, string entityName = null, int? entityId = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        ActivityLogType GetActivityTypeById(int activityLogTypeId);

        ActivityLog InsertActivity(string systemKeyword, string comment);

        ActivityLog InsertActivity(Customer customer, string systemKeyword, string comment);

        void UpdateActivityType(ActivityLogType activityLogType);

        ActivityLog GetActivityById(int activityLogId);

        void DeleteActivityType(ActivityLogType activityLogType);

        void DeleteActivity(ActivityLog activityLog);

        void ClearAllActivities();
    }
}
