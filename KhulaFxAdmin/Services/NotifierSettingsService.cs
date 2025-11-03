using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using KhulaFxAdmin.Models;
using Serilog;

namespace KhulaFxAdmin.Services
{
    public class NotifierSettingsService
    {
        private readonly string _connectionString;
        private static Dictionary<string, bool> _cache = new();
        private static DateTime _lastCacheUpdate = DateTime.MinValue;

        public NotifierSettingsService(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionStrings:DefaultConnection"]
                ?? throw new ArgumentNullException("Database connection string not configured");
        }

        public async Task<List<NotifierSetting>> GetAllSettingsAsync()
        {
            var settings = new List<NotifierSetting>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT Id, NotifierName, IsEnabled, LastUpdated, UpdatedBy FROM NotifierSettings";
            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                settings.Add(new NotifierSetting
                {
                    Id = reader.GetInt32(0),
                    NotifierName = reader.GetString(1),
                    IsEnabled = reader.GetBoolean(2),
                    LastUpdated = reader.GetDateTime(3),
                    UpdatedBy = reader.IsDBNull(4) ? null : reader.GetString(4)
                });
            }

            // Update cache
            _cache = settings.ToDictionary(s => s.NotifierName, s => s.IsEnabled);
            _lastCacheUpdate = DateTime.UtcNow;

            return settings;
        }

        public async Task<bool> UpdateSettingAsync(string notifierName, bool isEnabled, string updatedBy)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                UPDATE NotifierSettings 
                SET IsEnabled = @IsEnabled, 
                    LastUpdated = GETUTCDATE(),
                    UpdatedBy = @UpdatedBy
                WHERE NotifierName = @NotifierName";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@NotifierName", notifierName);
            command.Parameters.AddWithValue("@IsEnabled", isEnabled);
            command.Parameters.AddWithValue("@UpdatedBy", updatedBy);

            var rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                // Update cache immediately
                _cache[notifierName] = isEnabled;
                Log.Information("Notifier {NotifierName} set to {State} by {User}",
                    notifierName, isEnabled ? "ENABLED" : "DISABLED", updatedBy);
                return true;
            }

            return false;
        }

        public async Task<bool> IsNotifierEnabledAsync(string notifierName)
        {
            // Refresh cache if older than 30 seconds
            if ((DateTime.UtcNow - _lastCacheUpdate).TotalSeconds > 30)
            {
                await GetAllSettingsAsync();
            }

            return _cache.TryGetValue(notifierName, out var enabled) && enabled;
        }
    }
}