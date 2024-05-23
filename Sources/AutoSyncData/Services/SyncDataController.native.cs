namespace AutoSyncData.Services;

public interface SyncDataController
{
    Task Process(SiteInfo siteInfo);
}

public class SyncDataControllerImp(SyncDataHandler syncDataHandler) : SyncDataController
{
    private const int TimeDelayCheckStartService = 3000;

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