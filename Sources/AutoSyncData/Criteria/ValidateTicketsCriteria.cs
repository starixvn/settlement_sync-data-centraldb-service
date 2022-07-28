using Fanex.Data.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoSyncData.Criteria
{
    public class ValidateTicketsCriteria : NonQueryCommand
    {
        public string TransInfo { get; set; }

        public string TableName { get; set; }

        public bool IsPassed { get; set; }

        public override bool IsValid()
        {
            return !string.IsNullOrEmpty(TransInfo);
        }

        public override string GetSettingKey()
        {
            return "NAP_SettlementService_Validation";
        }
    }
}
