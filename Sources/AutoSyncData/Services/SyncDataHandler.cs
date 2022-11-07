using Fanex.Caching;
using Fanex.Logging;
using System.Data;

namespace AutoSyncData.Services
{
    public interface SyncDataHandler
    {
        Task<bool> IsStartService();

        Task StartHandle(string server, int siteDbGroupId);

        Task ProcessHandle(string server, int siteDbGroupId);
    }

    public class SyncDataHandlerImp : SyncDataHandler
    {
        private const string AutoSyncKey = "Settlement-Auto-Sync-Sportsbook";

        private const int TimeDelayGetLog = 500;

        private const int BatchSize = 10;

        private readonly ICache cache;

        private readonly QueryLogService queryLogService;

        private readonly SyncDataService syncDataService;

        private readonly SendMailService sendMailService;

        public SyncDataHandlerImp(
            ICache cache,
            QueryLogService queryLogService,
            SyncDataService syncDataService,
            SendMailService sendMailService)
        {
            this.cache = cache;
            this.queryLogService = queryLogService;
            this.syncDataService = syncDataService;
            this.sendMailService = sendMailService;
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

        public async Task StartHandle(string server, int siteDbGroupId)
        {
            try
            {
                var isSync = await syncDataService.RetrySyncCommandTicketToCentral(server, siteDbGroupId);

                if (!isSync)
                {
                    sendMailService.SendMail($"SiteDbGroupId:{siteDbGroupId} - Pending log is still wrong when restarting service.");
                }
            }
            catch (Exception exception)
            {
                await LogService.ErrorAsync($"AutoSyncDataCentralDb - Server:{server} - Restarting service:{exception}");
            }
        }

        public async Task ProcessHandle(string server, int siteDbGroupId)
        {
            try
            {
                var lastLog = await queryLogService.GetLatestLog(server, BatchSize);
                if (lastLog.Rows.Count == 0)
                {
                    await Task.Delay(TimeDelayGetLog);
                    return;
                }

                await SyncCommandTicketToCentral(lastLog, server, siteDbGroupId);
            }
            catch (Exception exception)
            {
                await LogService.ErrorAsync($"AutoSyncDataCentralDb - SiteDbGroupId:{siteDbGroupId} - Exception in main thread - Process:{exception}");
            }
        }

        private async Task SyncCommandTicketToCentral(DataTable lastLog, string server, int siteDbGroupId)
        {
            var isSync = await syncDataService.SyncCommandTicketToCentral(lastLog, server, siteDbGroupId);

            if (!isSync)
            {
                var logIds = queryLogService.GetLogIds(lastLog);
                sendMailService.SendMail($"SiteDbGroupId:{siteDbGroupId} - Failed to sync with logIds: {logIds}");
            }
        }
    }
}