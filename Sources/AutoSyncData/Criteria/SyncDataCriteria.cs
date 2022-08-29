using Fanex.Data.Repository;

namespace AutoSyncData.Criteria
{
    public class SyncDataCriteria : NonQueryCommand
    {
        public string TransInfo { get; set; }

        public string TableName { get; set; }

        public byte StatusId { get; set; }

        public int MatchId { get; set; }

        public string Action { get; set; }

        public string SettlementAction { get; set; }

        public string TicketType { get; set; }

        public override bool IsValid()
        {
            return true;
        }

        public override string GetSettingKey()
        {
            return "NAP_SettlementService_Calculation";
        }
    }
}
