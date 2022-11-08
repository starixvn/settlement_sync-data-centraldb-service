namespace AutoSyncData.Services;

public interface SyncDataHandler
{
    Task<bool> IsStartService();

    Task StartHandle(SiteInfo siteInfo);

    Task ProcessHandle(SiteInfo siteInfo);
}

public class SyncDataHandlerImp : SyncDataHandler
{
    private const string AutoSyncKey = "Settlement-Auto-Sync-Sportsbook";

    private const int TimeDelayGetLog = 500;

    private const int BatchSize = 10;

    private readonly ICache cache;

    private readonly QueryLogService queryLogService;

    private readonly SyncDataService syncDataService;

    private readonly SendMailService sendMailService;

    public SyncDataHandlerImp(
        ICache cache,
        QueryLogService queryLogService,
        SyncDataService syncDataService,
        SendMailService sendMailService)
    {
        this.cache = cache;
        this.queryLogService = queryLogService;
        this.syncDataService = syncDataService;
        this.sendMailService = sendMailService;
    }

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
                sendMailService.SendMail($"SiteDbGroupId:{siteInfo.SiteGroupId} - Pending log is still wrong when restarting service.");
            }
        }
        catch (Exception exception)
        {
            await LogService.ErrorAsync($"AutoSyncDataCentralDb - Server:{siteInfo.AliasName} - Restarting service:{exception}");
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
            await LogService.ErrorAsync($"AutoSyncDataCentralDb - SiteDbGroupId:{siteInfo.SiteGroupId} - Exception in main thread - Process:{exception}");
        }
    }

    private async Task SyncCommandTicketToCentral(DataTable lastLog, SiteInfo siteInfo)
    {
        var isSync = await syncDataService.SyncCommandTicketToCentral(lastLog, siteInfo);

        if (!isSync)
        {
            var logIds = queryLogService.GetLogIds(lastLog);
            sendMailService.SendMail($"SiteDbGroupId:{siteInfo.SiteGroupId} - Failed to sync with logIds: {logIds}");
        }
    }
}