using AutoSyncData.Criteria;
using AutoSyncData.Models;
using Fanex.Data.Repository;

namespace AutoSyncData.Services;

public interface LogDetailService
{
    Task<LogDetail> GetLatestLog(string server);
}

public class LogDetailServiceImp : LogDetailService
{
    private readonly IDynamicRepository repository;

    public LogDetailServiceImp(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task<LogDetail> GetLatestLog(string server)
    {
        var getLogCriteria = new GetLastestLogCriteria
        {
            DbServer = server
        };

        var latestLog = await repository.GetAsync<LogDetail>(getLogCriteria) ?? new LogDetail();
        latestLog.DbServer = server;

        return latestLog;
    }
}