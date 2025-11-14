using Microsoft.Extensions.Hosting;
using KhulaFxTradeMonitor;
using Serilog;

namespace KhulaFxAdmin.Services
{
    public class NotifierBackgroundService : BackgroundService
    {
        private readonly TelegramNotifier _telegramNotifier;
        private readonly WhatsAppNotifier _whatsAppNotifier;

        public NotifierBackgroundService(TelegramNotifier telegramNotifier, WhatsAppNotifier whatsAppNotifier)
        {
            _telegramNotifier = telegramNotifier;
            _whatsAppNotifier = whatsAppNotifier;
            Console.WriteLine("✅ NotifierBackgroundService created - keeping notifiers alive");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Console.WriteLine("🚀 NotifierBackgroundService started - keeping message queues alive");
                Log.Information("NotifierBackgroundService started");

                // Keep the service running to prevent app shutdown
                // The notifiers' background tasks will continue running
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(5000, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("🛑 NotifierBackgroundService stopping");
                Log.Information("NotifierBackgroundService stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ NotifierBackgroundService error: {ex.Message}");
                Log.Error(ex, "NotifierBackgroundService error");
            }
        }
    }
}