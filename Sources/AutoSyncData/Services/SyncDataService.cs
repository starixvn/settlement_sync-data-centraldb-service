using AutoSyncData.Criteria;
using AutoSyncData.Models;
using Fanex.Data.Repository;
using System.Threading.Tasks;

namespace AutoSyncData.Services
{
    public interface SyncDataService
    {
        Task<byte> SyncData(LogDetail logDetail);

        Task UpdateLogDetail(LogDetail logDetail);
    }

    public class SyncDataServiceImp: SyncDataService
    {
        private readonly IDynamicRepository repository;

        public SyncDataServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task UpdateLogDetail(LogDetail logDetail)
        {
            var criteria = new UpdateLogCriteria
            {
                LogId = logDetail.LogId,
                StatusId = logDetail.StatusId,
                DbServer = logDetail.DbServer
            };

            await repository.ExecuteAsync(criteria);
        }

        public async Task<byte> SyncData(LogDetail logDetail)
        {
            var criteria = new SyncDataCriteria
            {
                TransInfo = logDetail.TransInfo,
                TableName = logDetail.TableName
            };

            await repository.ExecuteAsync(criteria);
            return criteria.StatusId;
        }
    }
}
