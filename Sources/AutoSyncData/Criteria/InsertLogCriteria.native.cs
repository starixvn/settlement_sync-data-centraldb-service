namespace AutoSyncData.Criteria;

public class InsertLogCriteria : NonQueryCommand
{
    public int SiteDBGroupId { get; set; }

    public DataTable SiteDBCalculationLog { get; set; }

    public override bool IsValid() => SiteDBGroupId > 0 && SiteDBCalculationLog.Rows.Count > 0;

    public override string GetSettingKey() => "NAP_SettlementService_CalculationLogIns";
}