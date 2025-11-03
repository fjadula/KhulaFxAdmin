namespace KhulaFxAdmin.Models
{
    public class NotifierSetting
    {
        public int Id { get; set; }
        public string NotifierName { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class UpdateNotifierRequest
    {
        public string NotifierName { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
    }

    public class DailyReport
    {
        public string Date { get; set; } = string.Empty;
        public int TotalTrades { get; set; }
        public int ItmCount { get; set; }
        public int OtmCount { get; set; }
        public decimal WinRate { get; set; }
    }
}