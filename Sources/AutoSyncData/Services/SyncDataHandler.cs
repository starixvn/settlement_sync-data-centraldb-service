using Fanex.Caching;
using Fanex.Logging;

namespace AutoSyncData.Services
{
    public interface SyncDataHandler
    {
        Task<bool> IsStartService();

        Task Handle(string server, int siteDbGroupId);
    }

    public class SyncDataHandlerImp : SyncDataHandler
    {
        private const string AutoSyncKey = "Settlement-Auto-Sync-Sportsbook";

        private readonly ICache cache;
        private readonly LogDetailService logDetailService;
        private readonly SyncDataService syncDataService;

        public SyncDataHandlerImp(
            ICache cache,
            LogDetailService logDetailService,
            SyncDataService syncDataService)
        {
            this.cache = cache;
            this.logDetailService = logDetailService;
            this.syncDataService = syncDataService;
        }

        public async Task<bool> IsStartService()
        {
            try
            {
                var isExistedKey = await cache.ContainAsync(AutoSyncKey);
                if (isExistedKey)
                {
                    var (_, value) = await cache.TryGetAsync<bool>(AutoSyncKey);
                    return value;
                }

                return true;
            }
            catch
            {
                return true;
            }
        }

        public async Task Handle(string server, int siteDbGroupId)
        {
            try
            {
                var batchSize = 10;
                var watch = System.Diagnostics.Stopwatch.StartNew();
                //var list = await logDetailService.GetCalculationLogList(server, batchSize);
                var tableCommand = await logDetailService.GetCalculationLog(server, batchSize);
                watch.Stop();

                var watch1 = System.Diagnostics.Stopwatch.StartNew();
                await syncDataService.InsertLog(tableCommand, siteDbGroupId);
                watch1.Stop();

                //var watch1 = System.Diagnostics.Stopwatch.StartNew();
                //var list = await logDetailService.GetCalculationLogList(server, batchSize);
                //watch1.Stop();

                //var minMaxLog = logDetailService.GetMinMaxLogId(tableCommand);
                //await syncDataService.UpdateLogStatus(minMaxLog, server);

                var log = $"TALBE\t{batchSize}\t{watch.ElapsedMilliseconds}\t{watch1.ElapsedMilliseconds}\r\n";
                Console.WriteLine(log);
                await File.AppendAllTextAsync("log.csv", log);
            }
            catch (Exception exception)
            {
                await LogService.ErrorAsync($"AutoSyncDataCentralDb - Server:{server} - Exception in main thread - Process:{exception}");
            }
        }
    }
}