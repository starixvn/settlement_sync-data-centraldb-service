using AutoSyncData.Constants;
using AutoSyncData.Models;

namespace AutoSyncData.Services;

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

    private readonly ControlSyncService controlSyncService;

    public SyncDataControllerImp(LogDetailService logDetailService,
        ValidationService validationService,
        SyncDataService syncDataService,
        SendMailService sendMailService,
        IMaintenanceService maintenanceService,
        ControlSyncService controlSyncService)
    {
        this.logDetailService = logDetailService;
        this.validationService = validationService;
        this.syncDataService = syncDataService;
        this.sendMailService = sendMailService;
        this.maintenanceService = maintenanceService;
        this.controlSyncService = controlSyncService;
    }

    public async Task Process(string server)
    {
        var isStart = true;
        while (isStart)
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

            isStart = await controlSyncService.IsStartService();
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