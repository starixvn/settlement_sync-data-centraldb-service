namespace AutoSyncData.Services;

public interface QueryLogService
{
    Task<DataTable> GetLatestLogPending(string server);

    Task<DataTable> GetLatestLog(string server, int batchSize);

    Task UpdateLogStatus(DataTable lastLog, string server);

    Task InsertLog(DataTable table, int siteDbGroupId);

    Task InsertPendingLog(DataTable table, int siteDbGroupId);

    string GetLogIds(DataTable latestLogTable);
}

public class QueryLogServiceImp : QueryLogService
{
    private readonly IDynamicRepository repository;

    public QueryLogServiceImp(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task<DataTable> GetLatestLog(string server, int batchSize)
    {
        var getLogCriteria = new GetLatestLogCriteria
        {
            DbServer = server,
            Batch = batchSize
        };

        var objectDb = ObjectDbFactory.CreateInstance(getLogCriteria.GetSettingKey());

        using var dataSet = await objectDb.ExecuteDataSetAsync(getLogCriteria).ConfigureAwait(false);
        return dataSet.Tables[0];
    }

    public async Task<DataTable> GetLatestLogPending(string server)
    {
        var getLogCriteria = new GetLatestLogPendingCriteria
        {
            DbServer = server,
        };

        var objectDb = ObjectDbFactory.CreateInstance(getLogCriteria.GetSettingKey());

        using var dataSet = await objectDb.ExecuteDataSetAsync(getLogCriteria).ConfigureAwait(false);
        return dataSet.Tables[0];
    }

    public string GetLogIds(DataTable latestLogTable)
    {
        if (latestLogTable.Rows.Count == 0)
        {
            return string.Empty;
        }

        var ids = latestLogTable.Rows.OfType<DataRow>().Select(dr => dr.Field<Int64>("LogId")!.ToString());
        return string.Join(",", ids);
    }

    public async Task UpdateLogStatus(DataTable lastLog, string server)
    {
        var logIds = GetLogIds(lastLog);
        var criteria = new UpdateLogCriteria
        {
            LogIds = logIds,
            DbServer = server,
            StatusId = Status.Completed
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

    public async Task InsertPendingLog(DataTable table, int siteDbGroupId)
    {
        var criteria = new InsertPendingLogCriteria
        {
            SiteDBGroupId = siteDbGroupId,
            SiteDBCalculationLog = table
        };

        await repository.ExecuteAsync(criteria);
    }
}