using AutoSyncData.Models;
using Fanex.MailDeliver.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AutoSyncData.Services
{
    public interface SendMailService
    {
        void SendMail(LogDetail logDetail);
    }

    public class SendMailServiceImp : SendMailService
    {

        private readonly IConfiguration configuration;

        public SendMailServiceImp(
            IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void SendMail(LogDetail logDetail)
        {
            var mailRecord = new MailRecord
            {
                MailTo = MailTo(),
                Title = "NAP - Sync Data To Central Db Trans",
                Subject = Subject(logDetail),
                Body = MailBody(logDetail)
            };

            using var mailClient = new Fanex.MailDeliver.MailClient.API.MailDeliver();

            mailClient.SendMail(mailRecord);
        }

        private string Subject(LogDetail logDetail)
        {
            return $"Sync Data issue - Log Id No. {logDetail.LogId}";

        }

        private string MailTo()
        {
            return configuration.GetSection("EmailGroups:Soccer").Value;
        }

        private string MailBody(LogDetail logDetail)
        {
            return JsonConvert.SerializeObject(logDetail);
        }
    }
}