using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Logging.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Logging
{
    public partial class LoggerService : ILoggerService
    {
        #region Fields
        private readonly IRepository<Log> _logRepository;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public LoggerService(IRepository<Log> logRepository,
            IWebHelper webHelper)
        {
            _logRepository = logRepository;
            _webHelper = webHelper;
        }

        #endregion

        #region Utilities


        #endregion

        #region Methods

        public virtual bool IsEnabled(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => false,
                _ => true,
            };
        }

        public virtual void DeleteLog(Log log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            _logRepository.Delete(log);
        }

        public virtual void DeleteLogs(IList<Log> logs)
        {
            if (logs == null)
                throw new ArgumentNullException(nameof(logs));

            foreach(var log in logs)
                _logRepository.Delete(log);
        }

        public virtual void ClearLog()
        {
            _logRepository.Truncate("Log");
        }

        public virtual IPagedList<Log> GetAllLogs(DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = "", int? logLevelId = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _logRepository.Query();
            if (fromUtc.HasValue)
                query = query.Where(l => fromUtc.Value <= l.CreatedOnUtc);
            if (toUtc.HasValue)
                query = query.Where(l => toUtc.Value >= l.CreatedOnUtc);
            if (logLevelId.HasValue)
            {
                query = query.Where(l => (int)logLevelId.Value == l.LogLevelId);
            }

            if (!string.IsNullOrEmpty(message))
                query = query.Where(l => l.ShortMessage.Contains(message) || l.FullMessage.Contains(message));
            query = query.OrderByDescending(l => l.CreatedOnUtc);

            var log = new PagedList<Log>(query, pageIndex, pageSize);
            return log;
        }

        public virtual Log GetLogById(int logId)
        {
            if (logId == 0)
                return null;

            return _logRepository.GetById(logId);
        }

        public virtual IList<Log> GetLogByIds(int[] logIds)
        {
            if (logIds == null || logIds.Length == 0)
                return new List<Log>();

            var query = from l in _logRepository.Query()
                        where logIds.Contains(l.Id)
                        select l;
            var logItems = query.ToList();
            var sortedLogItems = new List<Log>();
            foreach (var id in logIds)
            {
                var log = logItems.Find(x => x.Id == id);
                if (log != null)
                    sortedLogItems.Add(log);
            }

            return sortedLogItems;
        }

        public virtual Log InsertLog(int logLevel, string shortMessage, string fullMessage = "", Customer customer = null)
        {
            var log = new Log
            {
                LogLevelId = logLevel,
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                IpAddress = _webHelper.GetCurrentIpAddress(),
                CustomerId = customer?.Id,
                PageUrl = _webHelper.GetThisPageUrl(true),
                ReferrerUrl = _webHelper.GetUrlReferrer(),
                CreatedOnUtc = DateTime.UtcNow
            };

            _logRepository.Insert(log);

            return log;
        }

        public virtual void Information(string message, Exception exception = null, Customer customer = null)
        {
            if (exception is System.Threading.ThreadAbortException)
                return;

            InsertLog(InovatiqaDefaults.Information, message, exception?.ToString() ?? string.Empty, customer);
        }

        public virtual void Warning(string message, Exception exception = null, Customer customer = null)
        {
            if (exception is System.Threading.ThreadAbortException)
                return;

            InsertLog(InovatiqaDefaults.Warning, message, exception?.ToString() ?? string.Empty, customer);
        }

        public virtual void Error(string message, Exception exception = null, Customer customer = null)
        {
            if (exception is System.Threading.ThreadAbortException)
                return;

            InsertLog(InovatiqaDefaults.Error, message, exception?.ToString() ?? string.Empty, customer);
        }

        #endregion
    }
}