using Microsoft.EntityFrameworkCore;
using Sassa.Brm.Common.Services;

//using Sassa.Brm.Common.Services;
using Sassa.BRM.Models;
using Sassa.Services;
using Sassa.FastTrackService.UI;
using Sassa.Models;
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
builder.Host.UseWindowsService();

var settings = builder.Configuration.Get<FastTrackServiceSettings>();
builder.Services.AddSingleton(settings ?? new FastTrackServiceSettings());
if (settings!.ContentServer.CSBeneficiaryRoot == "Default")
{
    Log.Warning("Content Server Beneficiary Root is set to 'Default'. Please update the configuration to the correct root folder name.");
    Application.Exit();
}

//Factory pattern
builder.Services.AddDbContextFactory<ModelContext>(options =>
options.UseOracle(settings!.ConnectionStrings.BrmConnection));
builder.Services.AddDbContextFactory<LoModelContext>(options =>
options.UseOracle(settings!.ConnectionStrings.LoConnection));
builder.Services.AddSingleton<StaticService>();
builder.Services.AddSingleton<LoService>();
builder.Services.AddSingleton<ValidationService>();

builder.Services.AddSingleton<CsServiceSettings>(c =>
{
    var csServiceSettings = new CsServiceSettings
    {
        CsServiceUser = settings.ContentServer.CSServiceUser,
        CsServicePass = settings.ContentServer.CSServicePass,
        CsWSEndpoint = settings.ContentServer.CSWSEndpoint,
        CsConnection = settings.ConnectionStrings.CsConnection, 
        CsDocFolder = settings.ContentServer.CSFILEURL,
        CsBeneficiaryRoot = settings.ContentServer.CSBeneficiaryRoot,
        CsMaxRetries = settings.ContentServer.CSMaxRetries
    };
    return csServiceSettings;
});
builder.Services.AddScoped<CSService>();
builder.Services.AddSingleton<CsUploadService>();
//builder.Services.AddSingleton<CoverSheetService>();
// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(settings!.Urls.AppPort); // HTTP
    // serverOptions.ListenAnyIP(5001, listenOptions => listenOptions.UseHttps()); // HTTPS
});

builder.Services.AddSingleton<CsFileWatcher>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<CsFileWatcher>());
Log.Information("CsFileWatcher registered.");

if (settings!.Functions.KofaxFileWatcher)
{
    builder.Services.AddSingleton<KofaxFileWatcher>();
    builder.Services.AddHostedService(sp => sp.GetRequiredService<KofaxFileWatcher>());
    Log.Information("KofaxFileWatcher registered.");
}
builder.Services.AddBlazorBootstrap();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();


