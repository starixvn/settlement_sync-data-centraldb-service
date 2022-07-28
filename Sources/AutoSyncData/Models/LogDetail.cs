using System;
using System.Collections.Generic;
using System.Text;

namespace AutoSyncData.Models
{
    public class LogDetail
    {
        public int LogId { get; set; }

        public string TransInfo { get; set; }

        public string TableName { get; set; }

        public byte StatusId { get; set; }

        public string DbServer { get; set; }

        public int MatchId { get; set; }
    }
}
