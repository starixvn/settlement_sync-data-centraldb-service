namespace AutoSyncData.Criteria;

public class GetLatestLogCriteria : CriteriaBase
{
    [SpParam(Ignored = true)]
    public string DbServer { get; set; }

    public int Batch { get; set; }

    public override string GetSettingKey()
    {
        return $"SiteDB_NAP_SettlementService_CalculationLogSel_{DbServer}";
    }

    public override bool IsValid() => !string.IsNullOrWhiteSpace(DbServer);
}