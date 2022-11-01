namespace AutoSyncData.Models;

public class LogDetail
{
    public int LogId { get; set; }

    public string TransInfo { get; set; }
    public string TransInfoJson { get; set; }

    public string TransInfoJson { get; set; }

    public string TableName { get; set; }

    public int MatchId { get; set; }

    public string Action { get; set; }

    public string SettlementAction { get; set; }

    public string TicketType { get; set; }

    public int SessionId { get; set; }

    public bool IsParallel { get; set; }
}