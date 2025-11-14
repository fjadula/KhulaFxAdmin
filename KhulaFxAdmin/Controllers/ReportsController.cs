using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KhulaFxAdmin.Services;
using Quartz;
using KhulaFxAdmin.Schedulers;
using Serilog;

namespace KhulaFxAdmin.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _reportService;
        private readonly ISchedulerFactory _schedulerFactory;

        public ReportsController(ReportService reportService, ISchedulerFactory schedulerFactory)
        {
            _reportService = reportService;
            _schedulerFactory = schedulerFactory;
        }

        [HttpGet("daily")]
        public async Task<ActionResult> GetDailyReport([FromQuery] string? date = null)
        {
            DateTime? targetDate = null;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsed))
            {
                targetDate = parsed;
            }

            var report = await _reportService.GetDailyReportAsync(targetDate);
            return Ok(report);
        }

        [HttpGet("weekly")]
        public async Task<ActionResult> GetWeeklyReport()
        {
            var message = await _reportService.GenerateWeeklyReportMessageAsync();
            return Ok(new { message });
        }

        [HttpPost("daily/trigger")]
        public async Task<ActionResult> TriggerDailyReport()
        {
            try
            {
                Console.WriteLine("🔔 [TRIGGER] Daily trigger endpoint hit");
                Log.Information("🔔 [TRIGGER] Daily trigger endpoint hit");

                var scheduler = await _schedulerFactory.GetScheduler();
                Console.WriteLine($"🔔 [TRIGGER] Scheduler running: {scheduler.IsStarted}");
                Log.Information("🔔 [TRIGGER] Scheduler running: {IsStarted}", scheduler.IsStarted);

                var jobKey = new JobKey("DailyReportJob");
                Console.WriteLine($"🔔 [TRIGGER] Triggering job: {jobKey.Name}");
                Log.Information("🔔 [TRIGGER] Triggering job: {JobName}", jobKey.Name);

                await scheduler.TriggerJob(jobKey);

                Console.WriteLine("✅ [TRIGGER] Daily report job triggered successfully");
                Log.Information("✅ [TRIGGER] Daily report job triggered successfully");

                return Ok(new { message = "Daily report job triggered successfully", triggeredAt = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TRIGGER] Error: {ex.Message}");
                Log.Error(ex, "❌ [TRIGGER] Error triggering daily report: {Message}", ex.Message);
                return StatusCode(500, new { message = "Failed to trigger daily report", error = ex.Message });
            }
        }

        [HttpPost("weekly/trigger")]
        public async Task<ActionResult> TriggerWeeklyReport()
        {
            try
            {
                Console.WriteLine("🔔 [TRIGGER] Weekly trigger endpoint hit");
                Log.Information("🔔 [TRIGGER] Weekly trigger endpoint hit");

                var scheduler = await _schedulerFactory.GetScheduler();
                Console.WriteLine($"🔔 [TRIGGER] Scheduler running: {scheduler.IsStarted}");
                Log.Information("🔔 [TRIGGER] Scheduler running: {IsStarted}", scheduler.IsStarted);

                var jobKey = new JobKey("WeeklyReportJob");
                Console.WriteLine($"🔔 [TRIGGER] Triggering job: {jobKey.Name}");
                Log.Information("🔔 [TRIGGER] Triggering job: {JobName}", jobKey.Name);

                await scheduler.TriggerJob(jobKey);

                Console.WriteLine("✅ [TRIGGER] Weekly report job triggered successfully");
                Log.Information("✅ [TRIGGER] Weekly report job triggered successfully");

                return Ok(new { message = "Weekly report job triggered successfully", triggeredAt = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TRIGGER] Error: {ex.Message}");
                Log.Error(ex, "❌ [TRIGGER] Error triggering weekly report: {Message}", ex.Message);
                return StatusCode(500, new { message = "Failed to trigger weekly report", error = ex.Message });
            }
        }
    }
}