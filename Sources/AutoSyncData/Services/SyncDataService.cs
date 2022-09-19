using AutoSyncData.Criteria;
using AutoSyncData.Models;
using Fanex.Data.Repository;
using System.Threading.Tasks;

namespace AutoSyncData.Services
{
    public interface SyncDataService
    {
        Task<Result> SyncData(LogDetail logDetail);

        Task UpdateLogDetail(LogDetail logDetail, byte statusId, string errorMessage);
    }

    public class SyncDataServiceImp : SyncDataService
    {
        private readonly IDynamicRepository repository;

        public SyncDataServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task UpdateLogDetail(LogDetail logDetail, byte statusId, string errorMessage)
        {
            var criteria = new UpdateLogCriteria
            {
                LogId = logDetail.LogId,
                StatusId = statusId,
                ErrMsg = errorMessage,
                DbServer = logDetail.DbServer
            };

            await repository.ExecuteAsync(criteria);
        }

        public async Task<Result> SyncData(LogDetail logDetail)
        {
            var criteria = new SyncDataCriteria
            {
                TransInfo = logDetail.TransInfo,
                TableName = logDetail.TableName,
                MatchId = logDetail.MatchId,
                Action = logDetail.Action,
                SettlementAction = logDetail.SettlementAction,
                TicketType = logDetail.TicketType
            };

            await repository.ExecuteAsync(criteria);

            return new Result
            {
                ErrCode = criteria.ErrCode,
                ErrMsg = criteria.ErrMsg
            };
        }
    }
}