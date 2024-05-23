namespace AutoSyncData.Services;

public interface SyncDataService
{
    Task<bool> SyncCommandTicketToCentral(DataTable lastLog, SiteInfo siteInfo);

    Task<bool> RetrySyncCommandTicketToCentral(SiteInfo siteInfo);
}

public class SyncDataServiceImp(
    QueryLogService queryLogService,
    IOptions<AppSettingsKey> appSettingsKey)
    : SyncDataService
{
    public async Task<bool> SyncCommandTicketToCentral(DataTable lastLog, SiteInfo siteInfo)
    {
        try
        {
            await queryLogService.InsertLog(lastLog, siteInfo.SiteGroupId);
            await queryLogService.UpdateLogStatus(lastLog, siteInfo.AliasName);
            return true;
        }
        catch
        {
            return await RetrySyncCommandTicketToCentral(siteInfo);
        }
    }

    public async Task<bool> RetrySyncCommandTicketToCentral(SiteInfo siteInfo)
    {
        var settingKey = appSettingsKey.Value;

        for (var i = 0; i < settingKey.RetryCount; i++)
        {
            var isPassed = await SyncPendingCommandTicketToCentral(siteInfo);
            if (isPassed)
            {
                return true;
            }
            if (i < settingKey.RetryCount - 1)
            {
                await Task.Delay(settingKey.RetryInterval);
            }
        }

        return false;
    }

    private async Task<bool> SyncPendingCommandTicketToCentral(SiteInfo siteInfo)
    {
        try
        {
            var latestLogPending = await queryLogService.GetLatestLogPending(siteInfo.AliasName);
            if (latestLogPending.Rows.Count == 0)
            {
                return true;
            }

            await queryLogService.InsertPendingLog(latestLogPending, siteInfo.SiteGroupId);
            await queryLogService.UpdateLogStatus(latestLogPending, siteInfo.AliasName);
            return true;
        }
        catch
        {
            return false;
        }
    }
}