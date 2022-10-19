using AutoSyncData.Constants;
using AutoSyncData.Models;
using Fanex.Caching;

namespace AutoSyncData.Services
{
    public interface SyncDataHandler
    {
        Task<bool> IsStartService();

        Task Handle(string server);
    }

    public class SyncDataHandlerImp : SyncDataHandler
    {
        private const string AutoSyncKey = "Settlement-Auto-Sync-Sportsbook";
        private const int TimeDeplayGetLog = 500;

        private readonly ICache cache;
        private readonly LogDetailService logDetailService;
        private readonly ValidationService validationService;
        private readonly SyncDataService syncDataService;
        private readonly SendMailService sendMailService;


        public SyncDataHandlerImp(
            ICache cache,
            LogDetailService logDetailService,
            ValidationService validationService,
            SyncDataService syncDataService,
            SendMailService sendMailService)
        {
            this.cache = cache;
            this.logDetailService = logDetailService;
            this.validationService = validationService;
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

        public async Task Handle(string server)
        {
            var logDetail = await logDetailService.GetLatestLog(server);

            if (logDetail.LogId <= 0)
            {
                await Task.Delay(TimeDeplayGetLog);
                return;
            }

            await syncDataService.UpdateLogDetail(logDetail, StatusId.Waiting, string.Empty);

            var isTicketsValid = await validationService.ValidateTickets(logDetail);
            if (isTicketsValid)
            {
                await SyncTicketToCentral(logDetail);
            }
            else
            {
                await UpdateLogDetailAndSendMail(logDetail, "Failed in validation ticket count");
            }
        }

        private async Task SyncTicketToCentral(LogDetail logDetail)
        {
            await syncDataService.UpdateLogDetail(logDetail, StatusId.Running, string.Empty);
            var result = await syncDataService.SyncData(logDetail);

            if (result.ErrCode == 0)
            {
                await syncDataService.UpdateLogDetail(logDetail, StatusId.Completed, string.Empty);
            }
            else
            {
                await UpdateLogDetailAndSendMail(logDetail, $"Failed to sync data: {result.ErrMsg}");
            }
        }

        private async Task UpdateLogDetailAndSendMail(LogDetail logDetail, string message)
        {
            await syncDataService.UpdateLogDetail(logDetail, StatusId.Failed, message);
            sendMailService.SendMail(logDetail, message);
        }
    }
}
