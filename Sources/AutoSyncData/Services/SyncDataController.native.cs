namespace AutoSyncData.Services;

public interface SyncDataController
{
    Task Process(string server, int siteDbGroupId);
}

public class SyncDataControllerImp : SyncDataController
{
    private readonly SyncDataHandler syncDataHandler;
    private const int TimeDelayCheckStartService = 3000;

    public SyncDataControllerImp(SyncDataHandler syncDataHandler)
    {
        this.syncDataHandler = syncDataHandler;
    }

    private async Task Start(string server, int siteDbGroupId)
    {
        var isStart = await syncDataHandler.IsStartService();

        if (isStart)
        {
            await syncDataHandler.StartHandle(server, siteDbGroupId);
        }
    }

    public async Task Process(string server, int siteDbGroupId)
    {
        await Start(server, siteDbGroupId);

        while (true)
        {
            var isStart = await syncDataHandler.IsStartService();

            if (!isStart)
            {
                await Task.Delay(TimeDelayCheckStartService);
                continue;
            }

            await syncDataHandler.ProcessHandle(server, siteDbGroupId);
        }
    }
}