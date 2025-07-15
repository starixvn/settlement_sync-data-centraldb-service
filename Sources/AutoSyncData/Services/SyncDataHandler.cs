using Fanex.Logging;

namespace AutoSyncData.Services;

public interface SyncDataHandler
{
    Task<bool> IsStartService();

    Task StartHandle(SiteInfo siteInfo);

    Task ProcessHandle(SiteInfo siteInfo);
}

public class SyncDataHandlerImp(
    ICache cache,
    QueryLogService queryLogService,
    SyncDataService syncDataService)
    : SyncDataHandler
{
    private const string AutoSyncKey = "Settlement-Auto-Sync-Sportsbook";

    private const int TimeDelayGetLog = 500;

    private const int BatchSize = 10;

    public async Task<bool> IsStartService()
    {
        try
        {
            var isExistedKey = await cache.ContainAsync(AutoSyncKey);
            if (isExistedKey)
            {
                var (_, value) = await cache.TryGetAsync<bool>(AutoSyncKey);
                return value;
            }

            return true;
        }
        catch
        {
            return true;
        }
    }

    public async Task StartHandle(SiteInfo siteInfo)
    {
        try
        {
            var isSync = await syncDataService.RetrySyncCommandTicketToCentral(siteInfo);

            if (!isSync)
            {
                await WriteErrorLog(siteInfo.AliasName, "Pending log is still wrong when restarting service.");
            }
        }
        catch (Exception exception)
        {
            await WriteErrorLog(siteInfo.AliasName, $"Restarting service:{exception}");
        }
    }

    public async Task ProcessHandle(SiteInfo siteInfo)
    {
        try
        {
            var lastLog = await queryLogService.GetLatestLog(siteInfo.AliasName, BatchSize);
            if (lastLog.Rows.Count == 0)
            {
                await Task.Delay(TimeDelayGetLog);
                return;
            }

            await SyncCommandTicketToCentral(lastLog, siteInfo);
        }
        catch (Exception exception)
        {
            if (!exception.Message.Contains("Reason: The account is disabled"))
            {
                await WriteErrorLog(siteInfo.AliasName, $"Exception in main thread - Process:{exception}");
            }
        }
    }

    private async Task SyncCommandTicketToCentral(DataTable lastLog, SiteInfo siteInfo)
    {
        var isSync = await syncDataService.SyncCommandTicketToCentral(lastLog, siteInfo);

        if (!isSync)
        {
            var logIds = queryLogService.GetLogIds(lastLog);
            await WriteErrorLog(siteInfo.AliasName, $"Failed to sync with logIds: {logIds}");
        }
    }

    private static async Task WriteErrorLog(string siteName, string message)
    {
        await LogService.ErrorAsync($"Auto Sync Log Sport - Server:{siteName} - {message}");
    }
}