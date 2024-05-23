namespace AutoSyncData.Services;

public interface AutoSyncDataController
{
    void Process();
}

public class AutoSyncDataControllerImp(
    SyncDataController syncDataController,
    IOptions<AppSettingsKey> appSettingsKey,
    ScaleOutSettleService scaleOutSettleService)
    : AutoSyncDataController
{
    public void Process()
    {
        var servers = appSettingsKey.Value.SiteDbServer;
        servers.ForEach(server =>
        {
            var siteGroupId = scaleOutSettleService.GetSiteGroupId(server);
            var siteInfo = new SiteInfo
            {
                AliasName = server,
                SiteGroupId = siteGroupId.Result
            };
            Task.Run(() => syncDataController.Process(siteInfo))
                .ConfigureAwait(false);
        });
    }
}