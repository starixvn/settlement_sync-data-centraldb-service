using AutoSyncData.Models;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;

namespace AutoSyncData.Services;

public interface AutoSyncDataController
{
    void ProcessAsync();
}

public class AutoSyncDataControllerImp : AutoSyncDataController
{
    private readonly SyncDataController syncDataController;

    private readonly ScaleOutSettleService scaleOutSettleService;

    private readonly AppSettingsKey appSettingsKey;

    public AutoSyncDataControllerImp(SyncDataController syncDataController, IOptions<AppSettingsKey> appSettingsKey, ScaleOutSettleService scaleOutSettleService)
    {
        this.syncDataController = syncDataController;
        this.appSettingsKey = appSettingsKey.Value;
        this.scaleOutSettleService = scaleOutSettleService;
    }

    public void ProcessAsync()
    {
        var servers = appSettingsKey.SiteDbServer;
        var siteDbGroupId = scaleOutSettleService.GetSiteGroupId(servers[0]);

        syncDataController.Process(servers[0], siteDbGroupId.Result);
        //servers.ForEach(server =>
        //{
        //    Task.Run(() => syncDataController.Process(server))
        //        .ConfigureAwait(false);
        //});
    }
}