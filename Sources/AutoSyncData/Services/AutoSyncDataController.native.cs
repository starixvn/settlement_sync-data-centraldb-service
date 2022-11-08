namespace AutoSyncData.Services;

public interface AutoSyncDataController
{
    void Process();
}

public class AutoSyncDataControllerImp : AutoSyncDataController
{
    private readonly SyncDataController syncDataController;

    private readonly ScaleOutSettleService scaleOutSettleService;

    private readonly AppSettingsKey appSettingsKey;

    public AutoSyncDataControllerImp(
        SyncDataController syncDataController,
        IOptions<AppSettingsKey> appSettingsKey,
        ScaleOutSettleService scaleOutSettleService)
    {
        this.syncDataController = syncDataController;
        this.appSettingsKey = appSettingsKey.Value;
        this.scaleOutSettleService = scaleOutSettleService;
    }

    public void Process()
    {
        var servers = appSettingsKey.SiteDbServer;
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