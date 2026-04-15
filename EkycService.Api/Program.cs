using EkycService.Api.Middleware;
using EkycService.Application.Interfaces;
using EkycService.Application.Services;
using EkycService.Infrastructure.Crypto.Services;
using EkycService.Infrastructure.Database;
using EkycService.Infrastructure.Logging;
using EkycService.Infrastructure.Parsers;
using EkycService.Infrastructure.Repositories;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Http.BatchFormatters;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Serilog
var now = DateTime.Now;

var logFolder = Path.Combine(
    "logs",
    now.Year.ToString(),
    now.ToString("MMMM"),
    $"Week-{ISOWeek.GetWeekOfYear(now)}",
    now.Day.ToString()
);

Directory.CreateDirectory(logFolder);

var template =
"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [CID:{CorrelationId}] {Message:lj}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // IMPORTANT
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()

    // Console
    .WriteTo.Console(outputTemplate: template)

    // FULL APPLICATION LOGS
    .WriteTo.File(
        Path.Combine(logFolder, "application.log"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: template
    )

    // ERROR LOGS
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
        .WriteTo.File(
            Path.Combine(logFolder, "error.log"),
            rollingInterval: RollingInterval.Day,
            outputTemplate: template
        )
    )

    // ONLY REQUEST LOGS
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Properties.ContainsKey("RequestPath"))
        .WriteTo.File(
            Path.Combine(logFolder, "request.log"),
            rollingInterval: RollingInterval.Day,
            outputTemplate: template
        )
    )
        
    // OpenObserve (YOUR SCREENSHOT ENDPOINT)
    .WriteTo.Http(
        requestUri: "http://localhost:5080/api/default/dotnetlogs/_json",
        queueLimitBytes: null,
        logEventLimitBytes: null,
        batchSizeLimitBytes: 1000000,
        period: TimeSpan.FromSeconds(2),

        textFormatter: new RenderedCompactJsonFormatter(),
        batchFormatter: new Serilog.Sinks.Http.BatchFormatters.ArrayBatchFormatter(),
        httpClient: new OpenObserveHttpClient()
    )

    .CreateLogger();

builder.Host.UseSerilog();

// DI
builder.Services.AddControllers();
builder.Services.AddScoped<IEkycService, EkycService.Application.Services.EkycService>();
builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddSingleton<IHsmProvider, HsmProvider>();
builder.Services.AddScoped<IKycResParser, KycResParser>();
builder.Services.AddScoped<IUidaiDecryptionService, UidaiDecryptionService>();
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<IVaultRepository, VaultRepository>();
builder.Services.AddScoped<IEkycSaveRepository, EkycSaveRepository>();
builder.Services.AddScoped<IDemographicsRepository, DemographicsRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseSerilogRequestLogging();

app.MapControllers();

app.Run();