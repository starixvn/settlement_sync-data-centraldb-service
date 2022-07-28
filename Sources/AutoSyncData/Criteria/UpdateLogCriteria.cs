using Fanex.Data;
using Fanex.Data.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoSyncData.Criteria
{
    public class UpdateLogCriteria : NonQueryCommand
    {
        public int LogId { get; set; }

        public byte StatusId { get; set; }

        [SpParam(Ignored = true)]
        public string DbServer { get; set; }

        public override bool IsValid()
        {
            return LogId > 0;
        }

        public override string GetSettingKey()
        {
            return $"SiteDB_NAP_SettlementService_CalculationLogUpd_{DbServer}";
        }
    }
}
