namespace AutoSyncData.Services;

public interface AutoSyncDataController
{
    void Process();
}

public class AutoSyncDataControllerImp(
    SyncDataController syncDataController,
    IOptions<AppSettingsKey> appSettingsKey,
    IDynamicRepository repository)
    : AutoSyncDataController
{
    public void Process()
    {
        var servers = appSettingsKey.Value.SiteDbServer;
        servers.ForEach(server =>
        {
            var siteGroupId = GetSiteGroupId(server);
            var siteInfo = new SiteInfo
            {
                AliasName = server,
                SiteGroupId = siteGroupId.Result
            };
            Task.Run(() => syncDataController.Process(siteInfo))
                .ConfigureAwait(false);
        });
    }

    private async Task<int> GetSiteGroupId(string dbServer)
    {
        var criteria = new GetSiteGroupCriteria { DbServer = dbServer };
        var site = await repository.GetAsync<SiteGroup>(criteria);
        return site.SiteDbGroupId;
    }
}