using Quartz;
using KhulaFxAdmin.Services;
// using KhulaFxTradeMonitor; // Your main app's notifiers
using Serilog;
namespace KhulaFxAdmin.Schedulers
{
    // Daily Report Job - 11:58 PM SAST (UTC+2)
    public class DailyReportJob : IJob
    {
        private readonly ReportService _reportService;
        private readonly NotifierSettingsService _notifierSettings;
        // private readonly TelegramNotifier _telegramNotifier;
        // private readonly WhatsAppNotifier _whatsAppNotifier;
        public DailyReportJob(
            ReportService reportService,
            NotifierSettingsService notifierSettings
        // TelegramNotifier telegramNotifier,
        // WhatsAppNotifier whatsAppNotifier
        )
        {
            _reportService = reportService;
            _notifierSettings = notifierSettings;
            // _telegramNotifier = telegramNotifier;
            // _whatsAppNotifier = whatsAppNotifier;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Log.Information("Generating daily report...");
                var message = await _reportService.GenerateDailyReportMessageAsync();
                // Check if Telegram is enabled
                // if (await _notifierSettings.IsNotifierEnabledAsync("Telegram"))
                // {
                //     await _telegramNotifier.SendToPublicChannelAsync(message);
                //     Log.Information("Daily report sent to Telegram");
                // }
                // Check if WhatsApp is enabled
                // if (await _notifierSettings.IsNotifierEnabledAsync("WhatsApp"))
                // {
                //     await _whatsAppNotifier.SendMessageAsync(message);
                //     Log.Information("Daily report sent to WhatsApp");
                // }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating/sending daily report");
            }
        }
    }
    // Weekly Report Job - Saturday 10:00 AM SAST (UTC+2)
    public class WeeklyReportJob : IJob
    {
        private readonly ReportService _reportService;
        private readonly NotifierSettingsService _notifierSettings;
        // private readonly TelegramNotifier _telegramNotifier;
        // private readonly WhatsAppNotifier _whatsAppNotifier;
        public WeeklyReportJob(
            ReportService reportService,
            NotifierSettingsService notifierSettings
        // TelegramNotifier telegramNotifier,
        // WhatsAppNotifier whatsAppNotifier
        )
        {
            _reportService = reportService;
            _notifierSettings = notifierSettings;
            // _telegramNotifier = telegramNotifier;
            // _whatsAppNotifier = whatsAppNotifier;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Log.Information("Generating weekly report...");
                var message = await _reportService.GenerateWeeklyReportMessageAsync();
                // Check if Telegram is enabled
                // if (await _notifierSettings.IsNotifierEnabledAsync("Telegram"))
                // {
                //     await _telegramNotifier.SendToPublicChannelAsync(message);
                //     Log.Information("Weekly report sent to Telegram");
                // }
                // Check if WhatsApp is enabled
                // if (await _notifierSettings.IsNotifierEnabledAsync("WhatsApp"))
                // {
                //     await _whatsAppNotifier.SendMessageAsync(message);
                //     Log.Information("Weekly report sent to WhatsApp");
                // }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating/sending weekly report");
            }
        }
    }
}