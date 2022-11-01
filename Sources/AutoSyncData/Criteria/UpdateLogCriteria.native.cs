using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoSyncData.Criteria;

public class UpdateLogCriteria : NonQueryCommand
{
    public int MinLogId { get; set; }

    public int MaxLogId { get; set; }

    public int SessionId { get; set; }

    public byte StatusId { get; set; }

    public string ErrMsg { get; set; }

    [SpParam(Ignored = true)]
    public string DbServer { get; set; }

    public override bool IsValid() => SessionId > 0;

    public override string GetSettingKey() => $"SiteDB_NAP_SettlementService_CalculationLogUpd_{DbServer}";
}