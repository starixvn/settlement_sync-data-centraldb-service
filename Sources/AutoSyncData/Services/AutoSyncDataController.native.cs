using AutoSyncData.Models;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;

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
        //var siteDbGroupId = await scaleOutSettleService.GetSiteGroupId(servers[0]);

        //await syncDataController.Process(servers[0], siteDbGroupId);
        servers.ForEach(server =>
        {
            var siteDbGroupId = scaleOutSettleService.GetSiteGroupId(server);

            Task.Run(() => syncDataController.Process(server, siteDbGroupId.Result))
                .ConfigureAwait(false);
        });
    }
}