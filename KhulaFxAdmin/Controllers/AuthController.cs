using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth;
using KhulaFxAdmin.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;

namespace KhulaFxAdmin.Controllers
{
    [EnableCors("AllowFrontend")]
    [ApiController]
    [Route("api/Auth/[action]")]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly string _jwtSecret;
        private readonly string _googleClientId;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;

            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Database connection string not configured");

            _jwtSecret = configuration["Jwt:Secret"]
                ?? throw new ArgumentNullException("JWT secret not configured");

            _googleClientId = configuration["Google:ClientId"]
                ?? throw new ArgumentNullException("Google Client ID not configured");
        }
       
        [HttpPost("google")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                // Verify Google token WITH audience
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _googleClientId }
                };
                // Verify Google token
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential);
              

                if (payload == null)
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid Google token"
                    });
                }

                // Check if user is authorized
                var isAuthorized = await IsUserAuthorizedAsync(payload.Email);

                if (!isAuthorized)
                {
                    Log.Warning("Unauthorized login attempt by {Email}", payload.Email);

                    // Log activity
                    await LogActivityAsync(payload.Email, "Unauthorized login attempt", GetClientIp());

                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Access Denied. Your account is not authorized."
                    });
                }

                // Update or create admin user
                await UpdateAdminUserAsync(payload);

                // Generate JWT token
                var jwtToken = GenerateJwtToken(payload.Email, payload.Name);

                // Log successful login
                await LogActivityAsync(payload.Email, "Successful login", GetClientIp());

                Log.Information("Admin {Email} logged in successfully", payload.Email);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Token = jwtToken,
                    Email = payload.Email,
                    Name = payload.Name
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during Google login: {Message}", ex.Message);
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = ex.Message  // Show actual error for debugging
                }); ;
            }
        }

        private async Task<bool> IsUserAuthorizedAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT COUNT(*) FROM AdminUsers WHERE GoogleEmail = @Email";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);

            var count = (int)await command.ExecuteScalarAsync();
            return count > 0;
        }

        private async Task UpdateAdminUserAsync(GoogleJsonWebSignature.Payload payload)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                UPDATE AdminUsers 
                SET GoogleId = @GoogleId,
                    FullName = @FullName,
                    ProfilePicture = @ProfilePicture,
                    LastLogin = GETUTCDATE()
                WHERE GoogleEmail = @Email";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", payload.Email);
            command.Parameters.AddWithValue("@GoogleId", payload.Subject);
            command.Parameters.AddWithValue("@FullName", payload.Name);
            command.Parameters.AddWithValue("@ProfilePicture", (object?)payload.Picture ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }

        private string GenerateJwtToken(string email, string name)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.Role, "Admin")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task LogActivityAsync(string email, string action, string ipAddress)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    INSERT INTO AdminActivityLog (AdminEmail, Action, IpAddress)
                    VALUES (@Email, @Action, @IpAddress)";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Action", action);
                command.Parameters.AddWithValue("@IpAddress", ipAddress);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error logging activity");
            }
        }

        private string GetClientIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                Console.WriteLine($"Connection string: {connectionString?.Substring(0, 30)}...");

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM AdminUsers";
                var count = await command.ExecuteScalarAsync();

                return Ok(new
                {
                    success = true,
                    message = "Database connected",
                    adminUserCount = count,
                    server = connection.DataSource,
                    database = connection.Database
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
    }
}