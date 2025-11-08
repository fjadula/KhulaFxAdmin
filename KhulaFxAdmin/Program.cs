using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Quartz;
using Quartz.AspNetCore;
using KhulaFxAdmin.Services;
using KhulaFxAdmin.Schedulers;
using Serilog;
using Microsoft.AspNetCore.Builder;

AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.UseManagedNetworkingOnWindows", true);
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowedOrigins = builder.Environment.IsDevelopment()
    ? new[] {
        "https://localhost:4200",
        "https://localhost:3000",
        "https://localhost:7222",
        "https://www.khulafx.com",
        "https://khulafx.com",
        "http://108.181.161.170",
        "http://localhost"
      }
    : new[] {
        "https://www.khulafx.com",
        "https://khulafx.com",
        "http://108.181.161.170"
      };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure CORS for Angular app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("https://localhost:4200","https://www.khulafx.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure JWT authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = true;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Register services
builder.Services.AddSingleton<NotifierSettingsService>();
builder.Services.AddSingleton<ReportService>();

// Configure Quartz for scheduled jobs
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    // Daily Report Job - 11:58 PM SAST (21:58 UTC)
    var dailyJobKey = new JobKey("DailyReportJob");
    q.AddJob<DailyReportJob>(opts => opts.WithIdentity(dailyJobKey));
    q.AddTrigger(opts => opts
        .ForJob(dailyJobKey)
        .WithIdentity("DailyReportTrigger")
        .WithCronSchedule("0 58 21 * * ?", x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time"))) // 11:58 PM SAST
    );

    // Weekly Report Job - Saturday 10:00 AM SAST (08:00 UTC)
    var weeklyJobKey = new JobKey("WeeklyReportJob");
    q.AddJob<WeeklyReportJob>(opts => opts.WithIdentity(weeklyJobKey));
    q.AddTrigger(opts => opts
        .ForJob(weeklyJobKey)
        .WithIdentity("WeeklyReportTrigger")
        .WithCronSchedule("0 0 8 ? * SAT", x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time"))) // Saturday 10:00 AM SAST
    );
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();  
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();