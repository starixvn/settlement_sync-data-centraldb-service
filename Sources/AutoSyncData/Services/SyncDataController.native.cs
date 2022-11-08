namespace AutoSyncData.Services;

public interface SyncDataController
{
    Task Process(SiteInfo siteInfo);
}

public class SyncDataControllerImp : SyncDataController
{
    private readonly SyncDataHandler syncDataHandler;
    private const int TimeDelayCheckStartService = 3000;

    public SyncDataControllerImp(SyncDataHandler syncDataHandler)
    {
        this.syncDataHandler = syncDataHandler;
    }

    private async Task Start(SiteInfo siteInfo)
    {
        var isStart = await syncDataHandler.IsStartService();

        if (isStart)
        {
            await syncDataHandler.StartHandle(siteInfo);
        }
    }

    public async Task Process(SiteInfo siteInfo)
    {
        await Start(siteInfo);

        while (true)
        {
            var isStart = await syncDataHandler.IsStartService();

            if (!isStart)
            {
                await Task.Delay(TimeDelayCheckStartService);
                continue;
            }

            await syncDataHandler.ProcessHandle(siteInfo);
        }
    }
}