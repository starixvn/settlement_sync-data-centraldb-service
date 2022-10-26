namespace AutoSyncData.Services;

public interface SyncDataController
{
    Task Process(string server);
}

public class SyncDataControllerImp : SyncDataController
{
    private readonly SyncDataHandler syncDataHandler;
    private const int TimeDelayCheckStartService = 3000;

    public SyncDataControllerImp(SyncDataHandler syncDataHandler)
    {
        this.syncDataHandler = syncDataHandler;
    }

    public async Task Process(string server)
    {
        while (true)
        {
            var isStart = await syncDataHandler.IsStartService();

            if (!isStart)
            {
                await Task.Delay(TimeDelayCheckStartService);
                continue;
            }

            await syncDataHandler.Handle(server);
        }
    }
}