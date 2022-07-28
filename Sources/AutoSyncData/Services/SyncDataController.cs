using AutoSyncData.Criteria;
using AutoSyncData.Models;
using Fanex.Data.Repository;
using System;
using System.Collections.Generic;
using System.Text;
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
            var logDetail = await logDetailService.GetLastestLog(server);

            try
            {
                if (logDetail != null)
                {
                    var isTicketsValid = await validationService.ValidateTickets(logDetail);
                    if (isTicketsValid)
                    {
                        logDetail.StatusId = StatusId.Running;
                        await syncDataService.UpdateLogDetail(logDetail);
                        var result = await syncDataService.SyncData(logDetail);
                        if (result == StatusId.Completed)
                        {
                            logDetail.StatusId = result;
                            await syncDataService.UpdateLogDetail(logDetail);
                        }
                        else
                        {
                            logDetail.StatusId = result;
                            throw new SyncDataFailedException();
                        }
                    }
                    else
                    {
                        logDetail.StatusId = StatusId.Failed;
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
                if (logDetail != null)
                {
                    logDetailService.RemoveLog(logDetail);
                }
            }
        }

        public async Task HandleException(SyncDataException exception, LogDetail logDetail)
        {
            await syncDataService.UpdateLogDetail(logDetail);
            sendMailService.SendMail(logDetail);
        }
    }
}
