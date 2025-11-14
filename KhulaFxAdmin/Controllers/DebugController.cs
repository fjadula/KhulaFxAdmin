using KhulaFxTradeMonitor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class DebugController : ControllerBase
{
    private readonly TelegramNotifier _telegram;
    private readonly WhatsAppNotifier _whatsapp;

    public DebugController(TelegramNotifier telegram, WhatsAppNotifier whatsapp)
    {
        _telegram = telegram;
        _whatsapp = whatsapp;
    }

    [HttpGet("check-services")]
    [AllowAnonymous]
    public IActionResult CheckServices()
    {
        return Ok(new
        {
            telegramNotifier = _telegram != null ? "✅ Registered" : "❌ Not registered",
            whatsappNotifier = _whatsapp != null ? "✅ Registered" : "❌ Not registered",
            timestamp = DateTime.UtcNow
        });
    }
}