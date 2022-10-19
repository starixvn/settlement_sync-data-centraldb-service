using AutoSyncData.Models;
using Microsoft.Extensions.Options;

namespace AutoSyncData.Services;

public interface AutoSyncDataController
{
    void ProcessAsync();
}

public class AutoSyncDataControllerImp : AutoSyncDataController
{
    private readonly SyncDataController syncDataController;

    private readonly AppSettingsKey appSettingsKey;

    public AutoSyncDataControllerImp(SyncDataController syncDataController, IOptions<AppSettingsKey> appSettingsKey)
    {
        this.syncDataController = syncDataController;
        this.appSettingsKey = appSettingsKey.Value;
    }

    public void ProcessAsync()
    {
        var servers = appSettingsKey.SiteDbServer;

        servers.ForEach(server =>
        {
            Task.Run(() => syncDataController.Process(server))
                .ConfigureAwait(false);
        });
    }
}