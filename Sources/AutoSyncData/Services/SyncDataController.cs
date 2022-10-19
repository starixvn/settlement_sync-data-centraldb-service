namespace AutoSyncData.Services;

public interface SyncDataController
{
    Task Process(string server);
}

public class SyncDataControllerImp : SyncDataController
{

    private readonly SyncDataHandler syncDataHandler;
    private const int TimeDeplayCheckStartService = 3000;

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
                await Task.Delay(TimeDeplayCheckStartService);
                continue;
            }

            await syncDataHandler.Handle(server);
        }
    }


}