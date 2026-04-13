using EkycService.Api.Middleware;
using EkycService.Application.Interfaces;
using EkycService.Application.Services;
using EkycService.Infrastructure.Crypto.Services;
using EkycService.Infrastructure.Database;
using EkycService.Infrastructure.Parsers;
using EkycService.Infrastructure.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
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
builder.Services.AddScoped<IEkycRecordRepository, EkycRecordRepository>();
builder.Services.AddScoped<IVaultRepository, VaultRepository>();
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

app.MapControllers();

app.Run();