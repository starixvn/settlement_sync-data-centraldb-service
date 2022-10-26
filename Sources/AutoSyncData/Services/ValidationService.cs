using AutoSyncData.Criteria;
using AutoSyncData.Models;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;

namespace AutoSyncData.Services;

public interface ValidationService
{
    Task<bool> ValidateTickets(LogDetail logDetail);
}

public class ValidationServiceImp : ValidationService
{
    private readonly IDynamicRepository repository;

    private readonly AppSettingsKey appSettingsKey;

    public ValidationServiceImp(IDynamicRepository repository, IOptions<AppSettingsKey> appSettingsKey)
    {
        this.repository = repository;
        this.appSettingsKey = appSettingsKey.Value;
    }

    public async Task<bool> ValidateTickets(LogDetail logDetail)
    {
        var criteria = new ValidateTicketsCriteria
        {
            TransInfo = logDetail.TransInfo,
            TableName = logDetail.TableName,
        };

        return await TryValidateTickets(criteria);
    }

    private async Task<bool> TryValidateTickets(ValidateTicketsCriteria criteria)
    {
        for (var i = 0; i < appSettingsKey.RetryCount; i++)
        {
            await repository.ExecuteAsync(criteria);
            if (criteria.IsPassed)
            {
                return true;
            }

            if (i < appSettingsKey.RetryCount - 1)
            {
                await Task.Delay(appSettingsKey.RetryInterval);
            }
        }

        return false;
    }
}