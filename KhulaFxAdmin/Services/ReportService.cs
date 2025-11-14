using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using KhulaFxAdmin.Models;
using Serilog;

namespace KhulaFxAdmin.Services
{
    public class ReportService
    {
        private readonly string _connectionString;

        public ReportService(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionStrings:DefaultConnection"]
                ?? throw new ArgumentNullException("Database connection string not configured");
        }

        public async Task<DailyReport> GetDailyReportAsync(DateTime? date = null)
        {
            var targetDate = date ?? DateTime.UtcNow.Date;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    COUNT(*) as TotalTrades,
                    SUM(CASE WHEN Result = 'ITM' THEN 1 ELSE 0 END) as ItmCount,
                    SUM(CASE WHEN Result = 'OTM' THEN 1 ELSE 0 END) as OtmCount
                FROM BinaryOptionTrades
                WHERE CAST(OpenTime AS DATE) = @Date
                AND CloseTime IS NOT NULL";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Date", targetDate);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var totalTrades = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                var itmCount = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                var otmCount = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                var winRate = totalTrades > 0 ? Math.Round((decimal)itmCount / totalTrades * 100, 2) : 0;

                return new DailyReport
                {
                    Date = targetDate.ToString("yyyy-MM-dd"),
                    TotalTrades = totalTrades,
                    ItmCount = itmCount,
                    OtmCount = otmCount,
                    WinRate = winRate
                };
            }

            return new DailyReport { Date = targetDate.ToString("yyyy-MM-dd") };
        }

        public async Task<string> GenerateDailyReportMessageAsync(DateTime? date = null)
        {
            var report = await GetDailyReportAsync(date);

            return $"📊VIP signal report for {report.Date}\n" +
                   $"✅ITM: {report.ItmCount}\n" +
                   $"❌OTM: {report.OtmCount}\n" +
                   $"📊Win % rate: {report.WinRate}%";
        }

        public async Task<string> GenerateWeeklyReportMessageAsync()
        {
            // Get Monday of current week
            var today = DateTime.UtcNow.Date;
            var daysSinceMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var monday = today.AddDays(-daysSinceMonday);
            var friday = monday.AddDays(4);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    COUNT(*) as TotalTrades,
                    SUM(CASE WHEN Result = 'ITM' THEN 1 ELSE 0 END) as ItmCount,
                    SUM(CASE WHEN Result = 'OTM' THEN 1 ELSE 0 END) as OtmCount
                FROM BinaryOptionTrades
                WHERE CAST(OpenTime AS DATE) BETWEEN @Monday AND @Friday
                AND CloseTime IS NOT NULL";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Monday", monday);
            command.Parameters.AddWithValue("@Friday", friday);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var totalTrades = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                var itmCount = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                var otmCount = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                var winRate = totalTrades > 0 ? Math.Round((decimal)itmCount / totalTrades * 100, 2) : 0;

                return $"📊Weekly VIP signal report:({monday:yyyy-MM-dd} to {friday:yyyy-MM-dd})\n" +
                       $"✅ITM: {itmCount}\n" +
                       $"❌OTM: {otmCount}\n" +
                       $"📊Win % rate: {winRate}%";
            }

            return $"Weekly Report ({monday:yyyy-MM-dd} to {friday:yyyy-MM-dd})\nNo trades this week";
        }
    }
}