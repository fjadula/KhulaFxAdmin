using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KhulaFxAdmin.Services;

namespace KhulaFxAdmin.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportsController(ReportService reportService)
        {
            _reportService = reportService;
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
    }
}