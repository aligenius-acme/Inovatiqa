using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Customers
{
    public class CustomerActivityService : ICustomerActivityService
    {
        #region Fields

        private readonly IRepository<ActivityLog> _activityLogRepository;
        private readonly IRepository<ActivityLogType> _activityLogTypeRepository;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContextService _workContextService;

        #endregion

        #region Ctor

        public CustomerActivityService(IRepository<ActivityLog> activityLogRepository,
            IRepository<ActivityLogType> activityLogTypeRepository,
            IWebHelper webHelper,
            IWorkContextService workContextService)
        {
            _activityLogRepository = activityLogRepository;
            _activityLogTypeRepository = activityLogTypeRepository;
            _webHelper = webHelper;
            _workContextService = workContextService;
        }

        #endregion
        
        #region Methods

        public virtual ActivityLog InsertActivity(string systemKeyword, string comment, int id, string entityName = null)
        {
            return InsertActivity(_workContextService.CurrentCustomer, systemKeyword, comment, id, entityName);
        }

        public virtual ActivityLog InsertActivity(Customer customer, string systemKeyword, string comment, int id, string entityName = null)
        {
            if (customer == null)
                return null;

            var activityLogType = GetAllActivityTypes().FirstOrDefault(type => type.SystemKeyword.Equals(systemKeyword));
            if (!activityLogType?.Enabled ?? true)
                return null;

            var logItem = new ActivityLog
            {
                ActivityLogTypeId = activityLogType.Id,
                EntityId = id,
                EntityName = entityName,
                CustomerId = customer.Id,
                Comment = CommonHelper.EnsureMaximumLength(comment ?? string.Empty, 4000),
                CreatedOnUtc = DateTime.UtcNow,
                IpAddress = _webHelper.GetCurrentIpAddress()
            };
            _activityLogRepository.Insert(logItem);

            //_eventPublisher.EntityInserted(logItem);

            return logItem;
        }

        public virtual IList<ActivityLogType> GetAllActivityTypes()
        {
            var query = from alt in _activityLogTypeRepository.Query()
                        orderby alt.Name
                        select alt;
            var activityLogTypes = query.ToList();

            return activityLogTypes;
        }

        public virtual IPagedList<ActivityLog> GetAllActivities(DateTime? createdOnFrom = null, DateTime? createdOnTo = null,
            int? customerId = null, int? activityLogTypeId = null, string ipAddress = null, string entityName = null, int? entityId = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _activityLogRepository.Query();

            if (!string.IsNullOrEmpty(ipAddress))
                query = query.Where(logItem => logItem.IpAddress.Contains(ipAddress));

            if (createdOnFrom.HasValue)
                query = query.Where(logItem => createdOnFrom.Value <= logItem.CreatedOnUtc);
            if (createdOnTo.HasValue)
                query = query.Where(logItem => createdOnTo.Value >= logItem.CreatedOnUtc);

            if (activityLogTypeId.HasValue && activityLogTypeId.Value > 0)
                query = query.Where(logItem => activityLogTypeId == logItem.ActivityLogTypeId);

            if (customerId.HasValue && customerId.Value > 0)
                query = query.Where(logItem => customerId.Value == logItem.CustomerId);

            if (!string.IsNullOrEmpty(entityName))
                query = query.Where(logItem => logItem.EntityName.Equals(entityName));
            if (entityId.HasValue && entityId.Value > 0)
                query = query.Where(logItem => entityId.Value == logItem.EntityId);

            query = query.OrderByDescending(logItem => logItem.CreatedOnUtc).ThenBy(logItem => logItem.Id);

            return new PagedList<ActivityLog>(query, pageIndex, pageSize);
        }

        public virtual ActivityLogType GetActivityTypeById(int activityLogTypeId)
        {
            if (activityLogTypeId == 0)
                return null;

            return _activityLogTypeRepository.GetById(activityLogTypeId);
        }

        public virtual ActivityLog InsertActivity(string systemKeyword, string comment)
        {
            return InsertActivity(_workContextService.CurrentCustomer, systemKeyword, comment);
        }

        public virtual ActivityLog InsertActivity(Customer customer, string systemKeyword, string comment)
        {
            if (customer == null)
                return null;

            var activityLogType = GetAllActivityTypes().FirstOrDefault(type => type.SystemKeyword.Equals(systemKeyword));
            if (!activityLogType?.Enabled ?? true)
                return null;

            var logItem = new ActivityLog
            {
                ActivityLogTypeId = activityLogType.Id,
                EntityId = customer.Id,
                EntityName = customer.GetType().Name,
                CustomerId = customer.Id,
                Comment = CommonHelper.EnsureMaximumLength(comment ?? string.Empty, 4000),
                CreatedOnUtc = DateTime.UtcNow,
                IpAddress = _webHelper.GetCurrentIpAddress()
            };
            _activityLogRepository.Insert(logItem);

            //event notification
            //_eventPublisher.EntityInserted(logItem);

            return logItem;
        }

        public virtual void UpdateActivityType(ActivityLogType activityLogType)
        {
            if (activityLogType == null)
                throw new ArgumentNullException(nameof(activityLogType));

            _activityLogTypeRepository.Update(activityLogType);

            //event notification
            //_eventPublisher.EntityUpdated(activityLogType);
        }

        public virtual ActivityLog GetActivityById(int activityLogId)
        {
            if (activityLogId == 0)
                return null;

            return _activityLogRepository.GetById(activityLogId);
        }

        public virtual void DeleteActivityType(ActivityLogType activityLogType)
        {
            if (activityLogType == null)
                throw new ArgumentNullException(nameof(activityLogType));

            _activityLogTypeRepository.Delete(activityLogType);

            //event notification
            //_eventPublisher.EntityDeleted(activityLogType);
        }

        public virtual void DeleteActivity(ActivityLog activityLog)
        {
            if (activityLog == null)
                throw new ArgumentNullException(nameof(activityLog));

            _activityLogRepository.Delete(activityLog);

            //event notification
            //_eventPublisher.EntityDeleted(activityLog);
        }

        public virtual void ClearAllActivities()
        {
            _activityLogRepository.Truncate("ActivityLog");
        }

        #endregion
    }
}