using AutoSyncData.Criteria;
using AutoSyncData.Models;
using Fanex.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSyncData.Services
{
    public interface LogDetailService
    {
        Task<LogDetail> GetLastestLog(string server);

        void RemoveLog(LogDetail logDetail);
    }

    public class LogDetailServiceImp : LogDetailService
    {
        private readonly IDynamicRepository repository;

        private readonly List<LogDetail> poolOfLogs;

        private readonly object lockObject;

        public LogDetailServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
            poolOfLogs = new List<LogDetail>();
            lockObject = new object();
        }

        public async Task<LogDetail> GetLastestLog(string server)
        {
            var getLogCriteria = new GetLastestLogCriteria
            {
                DbServer = server
            };

            var lastestLog = await repository.GetAsync<LogDetail>(getLogCriteria);
            if (lastestLog != null)
            {
                lock(lockObject)
                {
                    lastestLog.DbServer = server;
                    if (!poolOfLogs.Any(log => log.DbServer == server))
                    {
                        poolOfLogs.Add(lastestLog);
                        return lastestLog;
                    }

                    return null;
                }
            }

            return null;
        }

        public void RemoveLog(LogDetail logDetail)
        {
            lock(lockObject)
            {
                poolOfLogs.RemoveAll(log => log.LogId == logDetail.LogId && log.DbServer == logDetail.DbServer);
            }
        }
    }
}
