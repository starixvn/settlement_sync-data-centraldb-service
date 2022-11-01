using AutoSyncData.Constants;
using AutoSyncData.Criteria;
using AutoSyncData.Models;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Services;
using System.Data;

namespace AutoSyncData.Services;

public interface SyncDataService
{
    Task UpdateLogStatus(MinMaxLog log, string server);

    Task InsertLog(DataTable table, int siteDbGroupId);
}

public class SyncDataServiceImp : SyncDataService
{
    private readonly IDynamicRepository repository;

    private readonly ScaleOutSettleService scaleOutSettleService;

    public SyncDataServiceImp(IDynamicRepository repository, ScaleOutSettleService scaleOutSettleService)
    {
        this.repository = repository;
        this.scaleOutSettleService = scaleOutSettleService;
    }

    public async Task UpdateLogStatus(MinMaxLog log, string server)
    {
        var criteria = new UpdateLogCriteria
        {
            SessionId = log.SessionId,
            MinLogId = log.MinLogId,
            MaxLogId = log.MaxLogId,
            DbServer = server,
            StatusId = StatusId.Completed
        };

        await repository.ExecuteAsync(criteria);
    }

    public async Task InsertLog(DataTable table, int siteDbGroupId)
    {
        var criteria = new InsertLogCriteria
        {
            SiteDBGroupId = siteDbGroupId,
            SiteDBCalculationLog = table
        };

        await repository.ExecuteAsync(criteria);
    }
}