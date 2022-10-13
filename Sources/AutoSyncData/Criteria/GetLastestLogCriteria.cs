using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoSyncData.Criteria;

public class GetLastestLogCriteria : CriteriaBase
{
    [SpParam(Ignored = true)]
    public string DbServer { get; set; }

    public override string GetSettingKey()
    {
        return $"SiteDB_NAP_SettlementService_CalculationLogSel_{DbServer}";
    }

    public override bool IsValid() => !string.IsNullOrWhiteSpace(DbServer);
}