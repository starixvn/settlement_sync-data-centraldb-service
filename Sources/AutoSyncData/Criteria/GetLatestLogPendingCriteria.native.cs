namespace AutoSyncData.Criteria;

public class GetLatestLogPendingCriteria : CriteriaBase
{
    [SpParam(Ignored = true)]
    public string DbServer { get; set; }

    public override string GetSettingKey()
    {
        return $"SiteDB_NAP_SettlementService_PendingLog_{DbServer}";
    }

    public override bool IsValid() => !string.IsNullOrWhiteSpace(DbServer);
}