using Fanex.Data.Repository;

namespace AutoSyncData.Criteria;

public class ValidateTicketsCriteria : NonQueryCommand
{
    public string TransInfo { get; set; }

    public string TableName { get; set; }

    public bool IsPassed { get; set; }

    public override bool IsValid() => true;

    public override string GetSettingKey()
    {
        return "NAP_SettlementService_Validation";
    }
}