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
                var scheduler = await _schedulerFactory.GetScheduler();
                var jobKey = new JobKey("DailyReportJob");

                // Trigger the job immediately
                await scheduler.TriggerJob(jobKey);

                Log.Information("Daily report job triggered manually");
                return Ok(new { message = "Daily report job triggered successfully", triggeredAt = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error triggering daily report: {Message}", ex.Message);
                return StatusCode(500, new { message = "Failed to trigger daily report", error = ex.Message });
            }
        }

        [HttpPost("weekly/trigger")]
        public async Task<ActionResult> TriggerWeeklyReport()
        {
            try
            {
                var scheduler = await _schedulerFactory.GetScheduler();
                var jobKey = new JobKey("WeeklyReportJob");

                // Trigger the job immediately
                await scheduler.TriggerJob(jobKey);

                Log.Information("Weekly report job triggered manually");
                return Ok(new { message = "Weekly report job triggered successfully", triggeredAt = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error triggering weekly report: {Message}", ex.Message);
                return StatusCode(500, new { message = "Failed to trigger weekly report", error = ex.Message });
            }
        }
    }
}