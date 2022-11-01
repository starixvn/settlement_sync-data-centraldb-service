using AutoSyncData.Criteria;
using AutoSyncData.Models;
using Fanex.Data;
using Fanex.Data.Repository;
using System.Data;

namespace AutoSyncData.Services;

public interface LogDetailService
{
    Task<List<LogDetail>> GetCalculationLogList(string server, int batchSize);

    Task<DataTable> GetCalculationLog(string server, int batchSize);

    MinMaxLog GetMinMaxLogId(DataTable table);
}

public class LogDetailServiceImp : LogDetailService
{
    private readonly IDynamicRepository repository;

    public LogDetailServiceImp(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task<List<LogDetail>> GetCalculationLogList(string server, int batchSize)
    {
        var getLogCriteria = new GetLastestLogCriteria
        {
            DbServer = server,
            Batch = batchSize
        };

        return (await repository.FetchAsync<LogDetail>(getLogCriteria)).ToList();
    }

    public async Task<DataTable> GetCalculationLog(string server, int batchSize)
    {
        var getLogCriteria = new GetLastestLogCriteria
        {
            DbServer = server,
            Batch = batchSize
        };

        var objectDb = ObjectDbFactory.CreateInstance(getLogCriteria.GetSettingKey());

        using var dataSet = await objectDb.ExecuteDataSetAsync(getLogCriteria).ConfigureAwait(false);
        return dataSet.Tables[0];
    }

    public MinMaxLog GetMinMaxLogId(DataTable table)
    {
        if (table.Rows.Count == 0)
        {
            return new MinMaxLog();
        }

        var minLogId = table.AsEnumerable().Min(row => row["LogId"]);
        var maxLogId = table.AsEnumerable().Max(row => row["LogId"]);
        var sessionId = table.Rows[0]["SessionId"];

        return new MinMaxLog
        {
            MinLogId = Convert.ToInt32(minLogId),
            MaxLogId = Convert.ToInt32(maxLogId),
            SessionId = Convert.ToInt32(sessionId)
        };
    }
}