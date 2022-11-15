namespace AutoSyncData.Criteria;

public class UpdateLogCriteria : NonQueryCommand
{
    public string LogIds { get; set; }

    public byte StatusId { get; set; }

    [SpParam(Ignored = true)]
    public string DbServer { get; set; }

    public override bool IsValid() => !string.IsNullOrWhiteSpace(LogIds);

    public override string GetSettingKey() => $"SiteDB_NAP_SettlementService_CalculationLogUpd_{DbServer}";
}