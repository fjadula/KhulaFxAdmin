using Quartz;
using KhulaFxAdmin.Services;
using KhulaFxTradeMonitor; // Your main app's notifiers
using Serilog;
namespace KhulaFxAdmin.Schedulers
{
    // Daily Report Job - 11:58 PM SAST (UTC+2)
    public class DailyReportJob : IJob
    {
        private readonly ReportService _reportService;
        private readonly NotifierSettingsService _notifierSettings;
        private readonly TelegramNotifier _telegramNotifier;
        private readonly WhatsAppNotifier _whatsAppNotifier;

        public DailyReportJob(
            ReportService reportService,
            NotifierSettingsService notifierSettings,
            TelegramNotifier telegramNotifier,
            WhatsAppNotifier whatsAppNotifier)
        {
            _reportService = reportService;
            _notifierSettings = notifierSettings;
            _telegramNotifier = telegramNotifier;
            _whatsAppNotifier = whatsAppNotifier;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Console.WriteLine("📅 [DAILY JOB] ===== START =====");
                Log.Information("📅 [DAILY JOB] Starting daily report job");

                var message = await _reportService.GenerateDailyReportMessageAsync();
                Console.WriteLine($"📅 [DAILY JOB] Generated message: {message}");
                Log.Information("📅 [DAILY JOB] Generated message: {Message}", message);

                var telegramEnabled = await _notifierSettings.IsNotifierEnabledAsync("Telegram");
                Console.WriteLine($"📅 [DAILY JOB] Telegram enabled: {telegramEnabled}");
                Log.Information("📅 [DAILY JOB] Telegram enabled: {Enabled}", telegramEnabled);

                if (telegramEnabled && _telegramNotifier != null)
                {
                    try
                    {
                        Console.WriteLine("📅 [DAILY JOB] Sending to Telegram...");
                        Log.Information("📅 [DAILY JOB] Sending to Telegram...");
                        await _telegramNotifier.SendToPublicChannelAsync(message);
                        await _telegramNotifier.SendToPrivateChannelAsync(message);
                        Console.WriteLine("✅ [DAILY JOB] Telegram sent successfully");
                        Log.Information("✅ [DAILY JOB] Telegram sent successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ [DAILY JOB] Telegram error: {ex.Message}");
                        Log.Error(ex, "❌ [DAILY JOB] Telegram failed: {Message}", ex.Message);
                    }
                }

                var whatsappEnabled = await _notifierSettings.IsNotifierEnabledAsync("WhatsApp");
                Console.WriteLine($"📅 [DAILY JOB] WhatsApp enabled: {whatsappEnabled}");
                Log.Information("📅 [DAILY JOB] WhatsApp enabled: {Enabled}", whatsappEnabled);

                if (whatsappEnabled && _whatsAppNotifier != null)
                {
                    try
                    {
                        Console.WriteLine("📅 [DAILY JOB] Sending to WhatsApp...");
                        Log.Information("📅 [DAILY JOB] Sending to WhatsApp...");
                        await _whatsAppNotifier.SendMessageAsync(message);
                        Console.WriteLine("✅ [DAILY JOB] WhatsApp sent successfully");
                        Log.Information("✅ [DAILY JOB] WhatsApp sent successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ [DAILY JOB] WhatsApp error: {ex.Message}");
                        Log.Error(ex, "❌ [DAILY JOB] WhatsApp failed: {Message}", ex.Message);
                    }
                }

                Console.WriteLine("📅 [DAILY JOB] ===== END =====");
                Log.Information("📅 [DAILY JOB] Job completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DAILY JOB] FATAL ERROR: {ex.Message}");
                Log.Error(ex, "❌ [DAILY JOB] Fatal error: {Message}", ex.Message);
            }
        }
    }

    // Same for WeeklyReportJob
    public class WeeklyReportJob : IJob
    {
        private readonly ReportService _reportService;
        private readonly NotifierSettingsService _notifierSettings;
        private readonly TelegramNotifier _telegramNotifier;
        private readonly WhatsAppNotifier _whatsAppNotifier;

        public WeeklyReportJob(
            ReportService reportService,
            NotifierSettingsService notifierSettings,
            TelegramNotifier telegramNotifier,
            WhatsAppNotifier whatsAppNotifier)
        {
            _reportService = reportService;
            _notifierSettings = notifierSettings;
            _telegramNotifier = telegramNotifier;
            _whatsAppNotifier = whatsAppNotifier;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Console.WriteLine("📅 [WEEKLY JOB] ===== START =====");
                Log.Information("📅 [WEEKLY JOB] Starting weekly report job");

                var message = await _reportService.GenerateWeeklyReportMessageAsync();
                Console.WriteLine($"📅 [WEEKLY JOB] Generated message: {message}");
                Log.Information("📅 [WEEKLY JOB] Generated message: {Message}", message);

                var telegramEnabled = await _notifierSettings.IsNotifierEnabledAsync("Telegram");
                Console.WriteLine($"📅 [WEEKLY JOB] Telegram enabled: {telegramEnabled}");
                Log.Information("📅 [WEEKLY JOB] Telegram enabled: {Enabled}", telegramEnabled);

                if (telegramEnabled && _telegramNotifier != null)
                {
                    try
                    {
                        Console.WriteLine("📅 [WEEKLY JOB] Sending to Telegram...");
                        Log.Information("📅 [WEEKLY JOB] Sending to Telegram...");
                        await _telegramNotifier.SendToPublicChannelAsync(message);
                        await _telegramNotifier.SendToPrivateChannelAsync(message);
                        Console.WriteLine("✅ [WEEKLY JOB] Telegram sent successfully");
                        Log.Information("✅ [WEEKLY JOB] Telegram sent successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ [WEEKLY JOB] Telegram error: {ex.Message}");
                        Log.Error(ex, "❌ [WEEKLY JOB] Telegram failed: {Message}", ex.Message);
                    }
                }

                var whatsappEnabled = await _notifierSettings.IsNotifierEnabledAsync("WhatsApp");
                Console.WriteLine($"📅 [WEEKLY JOB] WhatsApp enabled: {whatsappEnabled}");
                Log.Information("📅 [WEEKLY JOB] WhatsApp enabled: {Enabled}", whatsappEnabled);

                if (whatsappEnabled && _whatsAppNotifier != null)
                {
                    try
                    {
                        Console.WriteLine("📅 [WEEKLY JOB] Sending to WhatsApp...");
                        Log.Information("📅 [WEEKLY JOB] Sending to WhatsApp...");
                        await _whatsAppNotifier.SendMessageAsync(message);
                        Console.WriteLine("✅ [WEEKLY JOB] WhatsApp sent successfully");
                        Log.Information("✅ [WEEKLY JOB] WhatsApp sent successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ [WEEKLY JOB] WhatsApp error: {ex.Message}");
                        Log.Error(ex, "❌ [WEEKLY JOB] WhatsApp failed: {Message}", ex.Message);
                    }
                }

                Console.WriteLine("📅 [WEEKLY JOB] ===== END =====");
                Log.Information("📅 [WEEKLY JOB] Job completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [WEEKLY JOB] FATAL ERROR: {ex.Message}");
                Log.Error(ex, "❌ [WEEKLY JOB] Fatal error: {Message}", ex.Message);
            }
        }
    }
}