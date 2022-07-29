using AutoSyncData.Criteria;
using AutoSyncData.Models;
using Fanex.Data.Repository;
using System.Collections.Generic;
using System.Linq;
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

            var lastLog = await repository.GetAsync<LogDetail>(getLogCriteria)?? new LogDetail();
            
            lock(lockObject)
            {
                lastLog.DbServer = server;

                if (lastLog.LogId > 0 && poolOfLogs.All(log => log.DbServer != server))
                {
                    poolOfLogs.Add(lastLog);
                    return lastLog;
                }

                return new LogDetail();
            }
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
