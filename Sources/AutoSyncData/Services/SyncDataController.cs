using AutoSyncData.Constants;
using AutoSyncData.Models;
using Fanex.Caching;

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

    private readonly ICache cache;

    private const string AutoSyncKey = "Settlement-Auto-Sync-Sportsbook";

    public SyncDataControllerImp(LogDetailService logDetailService,
        ValidationService validationService,
        SyncDataService syncDataService,
        SendMailService sendMailService,
        ICache cache,
        IMaintenanceService maintenanceService)
    {
        this.logDetailService = logDetailService;
        this.validationService = validationService;
        this.syncDataService = syncDataService;
        this.sendMailService = sendMailService;
        this.cache = cache;
        this.maintenanceService = maintenanceService;
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

            isStart = await IsStartService();
        }
    }

    private async Task<bool> IsStartService()
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