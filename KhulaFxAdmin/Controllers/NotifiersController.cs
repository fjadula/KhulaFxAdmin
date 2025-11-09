using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KhulaFxAdmin.Models;
using KhulaFxAdmin.Services;
using System.Security.Claims;

namespace KhulaFxAdmin.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class NotifiersController : ControllerBase
    {
        private readonly NotifierSettingsService _notifierService;

        public NotifiersController(NotifierSettingsService notifierService)
        {
            _notifierService = notifierService;
        }

        [HttpGet]
        public async Task<ActionResult<List<NotifierSetting>>> GetSettings()
        {
            var settings = await _notifierService.GetAllSettingsAsync();
            return Ok(settings);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateSetting([FromBody] UpdateNotifierRequest request)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";

            var success = await _notifierService.UpdateSettingAsync(
                request.NotifierName,
                request.IsEnabled,
                email);

            if (success)
            {
                return Ok(new { message = "Setting updated successfully" });
            }

            return BadRequest(new { message = "Failed to update setting" });
        }
    }
}