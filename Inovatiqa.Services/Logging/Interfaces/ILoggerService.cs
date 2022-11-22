using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Services.Logging.Interfaces
{
    public partial interface ILoggerService
    {
        bool IsEnabled(LogLevel level);

        void DeleteLog(Log log);

        void DeleteLogs(IList<Log> logs);

        void ClearLog();

        IPagedList<Log> GetAllLogs(DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = "", int? logLevelId = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        Log GetLogById(int logId);

        IList<Log> GetLogByIds(int[] logIds);

        Log InsertLog(int logLevelId, string shortMessage, string fullMessage = "", Customer customer = null);

        void Information(string message, Exception exception = null, Customer customer = null);

        void Warning(string message, Exception exception = null, Customer customer = null);

        void Error(string message, Exception exception = null, Customer customer = null);
    }
}