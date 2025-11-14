using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Quartz;
using Quartz.AspNetCore;
using KhulaFxAdmin.Services;
using KhulaFxAdmin.Schedulers;
using Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using KhulaFxTradeMonitor;

AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.UseManagedNetworkingOnWindows", true);

var builder = WebApplication.CreateBuilder(args);

// ==================== SERVICES CONFIGURATION ====================

// 1. Add basic services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your JWT token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// 2. Configure CORS Policy in Services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "https://khulafx.com",
                "https://www.khulafx.com",
                "http://khulafx.com"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("Authorization");
    });
});

// 3. Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT Secret not configured");
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

// 4. Register Application Services
builder.Services.AddSingleton<NotifierSettingsService>();
builder.Services.AddSingleton<ReportService>();
builder.Services.AddSingleton<KhulaFxTradeMonitor.ConfigurationManager>();
builder.Services.AddSingleton<TelegramNotifier>();
builder.Services.AddSingleton<WhatsAppNotifier>();
builder.Services.AddHostedService<NotifierBackgroundService>();

// 5. Configure Quartz Scheduler
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    // Daily Report Job - 11:58 PM SAST
    var dailyJobKey = new JobKey("DailyReportJob");
    q.AddJob<DailyReportJob>(opts => opts.WithIdentity(dailyJobKey));
    q.AddTrigger(opts => opts
        .ForJob(dailyJobKey)
        .WithIdentity("DailyReportTrigger")
        .WithCronSchedule("0 58 21 * * ?", x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time")))
    );

    // Weekly Report Job - Saturday 10:00 AM SAST
    var weeklyJobKey = new JobKey("WeeklyReportJob");
    q.AddJob<WeeklyReportJob>(opts => opts.WithIdentity(weeklyJobKey));
    q.AddTrigger(opts => opts
        .ForJob(weeklyJobKey)
        .WithIdentity("WeeklyReportTrigger")
        .WithCronSchedule("0 0 8 ? * SAT", x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time")))
    );
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// 6. Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// ==================== BUILD APP ====================
var app = builder.Build();

// ==================== MIDDLEWARE PIPELINE CONFIGURATION ====================
// ⚠️ ORDER MATTERS! Do NOT change this order!

// 1. Development tools (these run early)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 2. HTTPS Redirection
app.UseHttpsRedirection();

// 5. Set API base path
app.UsePathBase("/api");

// 3. ✅ CORS MIDDLEWARE - MUST BE BEFORE Authentication/Authorization
app.UseCors("AllowFrontend");

// 4. Handle preflight OPTIONS requests
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }
    await next();
});



// 6. Authentication
app.UseAuthentication();

// 7. Authorization
app.UseAuthorization();

// 8. Map all controller endpoints
app.MapControllers();

// 9. Run the application
app.Run();