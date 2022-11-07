using AutoSyncData.Models;
using Microsoft.Extensions.Options;
using System.Data;

namespace AutoSyncData.Services;

public interface SyncDataService
{
    Task<bool> SyncCommandTicketToCentral(DataTable lastLog, string server, int siteDbGroupId);

    Task<bool> RetrySyncCommandTicketToCentral(string server, int siteDbGroupId);
}

public class SyncDataServiceImp : SyncDataService
{
    private readonly QueryLogService queryLogService;

    private readonly AppSettingsKey appSettingsKey;

    public SyncDataServiceImp(
        QueryLogService queryLogService,
        IOptions<AppSettingsKey> appSettingsKey)
    {
        this.queryLogService = queryLogService;
        this.appSettingsKey = appSettingsKey.Value;
    }

    public async Task<bool> SyncCommandTicketToCentral(DataTable lastLog, string server, int siteDbGroupId)
    {
        try
        {
            await queryLogService.InsertLog(lastLog, siteDbGroupId);
            await queryLogService.UpdateLogStatus(lastLog, server);
            return true;
        }
        catch
        {
            return await RetrySyncCommandTicketToCentral(server, siteDbGroupId);
        }
    }

    public async Task<bool> RetrySyncCommandTicketToCentral(string server, int siteDbGroupId)
    {
        for (var i = 0; i < appSettingsKey.RetryCount; i++)
        {
            var isPassed = await SyncPendingCommandTicketToCentral(server, siteDbGroupId);
            if (isPassed)
            {
                return true;
            }
            if (i < appSettingsKey.RetryCount - 1)
            {
                await Task.Delay(appSettingsKey.RetryInterval);
            }
        }

        return false;
    }

    private async Task<bool> SyncPendingCommandTicketToCentral(string server, int siteDbGroupId)
    {
        try
        {
            var latestLogPending = await queryLogService.GetLatestLogPending(server);
            if (latestLogPending.Rows.Count == 0)
            {
                return true;
            }

            await queryLogService.InsertPendingLog(latestLogPending, siteDbGroupId);
            await queryLogService.UpdateLogStatus(latestLogPending, server);
            return true;
        }
        catch
        {
            return false;
        }
    }
}