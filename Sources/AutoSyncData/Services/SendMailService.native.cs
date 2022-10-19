using AutoSyncData.Models;
using Fanex.MailDeliver.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AutoSyncData.Services;

public interface SendMailService
{
    void SendMail(LogDetail logDetail, string message);
}

public class SendMailServiceImp : SendMailService
{
    private readonly AppSettingsKey appSettingsKey;

    public SendMailServiceImp(IOptions<AppSettingsKey> appSettingsKey)
    {
        this.appSettingsKey = appSettingsKey.Value;
    }

    public void SendMail(LogDetail logDetail, string message)
    {
        var content = JsonConvert.SerializeObject(logDetail);
        var mailRecord = new MailRecord
        {
            MailTo = appSettingsKey.EmailGroup,
            Title = "NAP - Sync Data To Central DBTrans",
            Subject = $"[{appSettingsKey.EnvironmentName}] - Sync Data issue - Log Id No. {logDetail.LogId}",
            Body = $"{message} {content}"
        };

        using var mailClient = new Fanex.MailDeliver.MailClient.API.MailDeliver();

        mailClient.SendMail(mailRecord);
    }
}