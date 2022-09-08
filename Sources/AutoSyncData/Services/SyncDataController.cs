using AutoSyncData.Models;
using System.Threading.Tasks;
using AutoSyncData.Constants;
using AutoSyncData.Exceptions;

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

        public SyncDataControllerImp(LogDetailService logDetailService,
            ValidationService validationService,
            SyncDataService syncDataService,
            SendMailService sendMailService)
        {
            this.logDetailService = logDetailService;
            this.validationService = validationService;
            this.syncDataService = syncDataService;
            this.sendMailService = sendMailService;
        }

        public async Task Process(string server)
        {
            var logDetail = await logDetailService.GetLatestLog(server);
            try
            {
                if (logDetail.LogId > 0)
                {
                    await UpdateLogDetail(logDetail, StatusId.Waiting);
                    var isTicketsValid = await validationService.ValidateTickets(logDetail);
                    if (isTicketsValid)
                    {
                        await UpdateLogDetail(logDetail, StatusId.Running);
                        var statusId = await syncDataService.SyncData(logDetail);
                        if (statusId == StatusId.Completed)
                        {
                            await UpdateLogDetail(logDetail, StatusId.Completed);
                        }
                        else
                        {
                           throw new SyncDataFailedException();
                        }
                    }
                    else
                    {
                        throw new InvalidTicketCountException();
                    }
                }
            }
            catch (SyncDataException exception)
            {
                await HandleException(exception, logDetail);
            }
            finally
            {
                logDetailService.RemoveLog(logDetail);
            }
        }

        private async Task HandleException(SyncDataException exception, LogDetail logDetail)
        {
            await UpdateLogDetail(logDetail, StatusId.Failed);
            sendMailService.SendMail(logDetail, exception.Description);
        }

        private async Task UpdateLogDetail(LogDetail logDetail, byte statusId)
        {
            logDetail.StatusId = statusId;
            await syncDataService.UpdateLogDetail(logDetail);
        }
    }
}
