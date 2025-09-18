using iText.Kernel.Pdf;
using Microsoft.Extensions.Configuration;
using Sassa.BRM.Services;
using Sassa.Scanning.Models;
using Sassa.Scanning.Services;
using Sassa.Scanning.Settings;
using Sassa.Scanning.UI;
using Serilog;
using Serilog.Events;
using Svg;
using System.Diagnostics;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);
// Ensure detailed startup info
builder.WebHost.CaptureStartupErrors(true);
builder.WebHost.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");

// Configure Serilog early (console + debug + file) and allow overrides from configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)          // Serilog section in appsettings
    .Enrich.FromLogContext()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("Logs/fasttrack-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        shared: true,
        buffered: false)
    .CreateLogger();
builder.Host.UseSerilog();

bool EnableCsWatcher = builder.Configuration.GetValue<bool>("Functions:ScanFileWatcher");

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(builder.Configuration.GetValue<int>("Urls:AppPort")); // HTTP
                                                                                    // serverOptions.ListenAnyIP(5001, listenOptions => listenOptions.UseHttps()); // HTTPS
});

// Bind ScanningSettings from configuration and register with DI
var scanningSettings = builder.Configuration.GetSection("ScanningSettings").Get<ScanningSettings>();
builder.Services.AddSingleton(scanningSettings??new ScanningSettings());

builder.Services.AddSingleton<ProgressWindow>();
builder.Services.AddSingleton<PdfBarcodeSplitter>();
builder.Services.AddSingleton<ScanFileWatcher>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<ScanFileWatcher>());
Log.Information("ScanFileWatcher registered.");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}


//app.UseAntiforgery();

//app.MapStaticAssets();
//app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();





