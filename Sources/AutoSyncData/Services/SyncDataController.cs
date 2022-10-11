using AutoSyncData.Constants;
using AutoSyncData.Models;
using System.Threading.Tasks;

namespace AutoSyncData.Services
{
    public interface SyncDataController
    {
        Task Process(string server);
    }

    public class SyncDataControllerImp : SyncDataController
    {
        private readonly LogDetailService logDetailService;

        private readonly ValidationService validationService;

        private readonly SyncDataService syncDataService;

        private readonly SendMailService sendMailService;

        private readonly IMaintenanceService maintenanceService;

        public SyncDataControllerImp(LogDetailService logDetailService,
            ValidationService validationService,
            SyncDataService syncDataService,
            SendMailService sendMailService,
            IMaintenanceService maintenanceService)
        {
            this.logDetailService = logDetailService;
            this.validationService = validationService;
            this.syncDataService = syncDataService;
            this.sendMailService = sendMailService;
            this.maintenanceService = maintenanceService;
        }

        public async Task Process(string server)
        {
            while (!maintenanceService.IsMaintenance)
            {
                var logDetail = await logDetailService.GetLatestLog(server);

                if (logDetail.LogId > 0)
                {
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
                else
                {
                    await Task.Delay(500);
                }
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