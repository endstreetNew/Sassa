using Microsoft.EntityFrameworkCore;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Models;
using Sassa.BRM.Services;
using Sassa.FastTrack.UI;
using Sassa.Models;
using Sassa.Services;
using Serilog;
using Serilog.Events;

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

bool EnableCsWatcher = builder.Configuration.GetValue<bool>("Functions:CsFileWatcher");
bool EnableKofaxWatcher = builder.Configuration.GetValue<bool>("Functions:KofaxFileWatcher");

string? BrmConnectionString = builder.Configuration.GetConnectionString("BrmConnection");
string? LoConnectionString = builder.Configuration.GetConnectionString("LoConnection");
string? CsConnectionString = builder.Configuration.GetConnectionString("CsConnection");

static string Require(string? v, string name)
    => !string.IsNullOrWhiteSpace(v) ? v : throw new InvalidOperationException($"Missing connection string: {name}");

BrmConnectionString = Require(BrmConnectionString, "BrmConnection");
LoConnectionString = Require(LoConnectionString, "LoConnection");
CsConnectionString = Require(CsConnectionString, "CsConnection");
// Add services to the container.
//Factory pattern
builder.Services.AddDbContextFactory<ModelContext>(options =>
options.UseOracle(BrmConnectionString));
builder.Services.AddDbContextFactory<LoModelContext>(options =>
options.UseOracle(LoConnectionString));
builder.Services.AddSingleton<StaticService>();
builder.Services.AddSingleton<LoService>();
builder.Services.AddSingleton<CsServiceSettings>(c =>
{
    CsServiceSettings csServiceSettings = new CsServiceSettings();
    csServiceSettings.CsConnection = CsConnectionString;
    csServiceSettings.CsWSEndpoint = builder.Configuration.GetValue<string>("ContentServer:CSWSEndpoint")!;
    csServiceSettings.CsServiceUser = builder.Configuration.GetValue<string>("ContentServer:CSServiceUser")!;
    csServiceSettings.CsServicePass = builder.Configuration.GetValue<string>("ContentServer:CSServicePass")!;
    csServiceSettings.CsDocFolder = $"{builder.Environment.WebRootPath}\\{builder.Configuration.GetValue<string>("Folders:CS")}\\";
    csServiceSettings.CsBeneficiaryRoot = builder.Configuration.GetValue<string>("ContentServer:CSBeneficiaryRoot")!;
    csServiceSettings.CsMaxRetries = builder.Configuration.GetValue<int>("ContentServer:CSMaxRetries");
    return csServiceSettings;
});
builder.Services.AddScoped<CSService>();
builder.Services.AddSingleton<CsUploadService>();
builder.Services.AddSingleton<CoverSheetService>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(builder.Configuration.GetValue<int>("Urls:AppPort")); // HTTP
                                                                                    // serverOptions.ListenAnyIP(5001, listenOptions => listenOptions.UseHttps()); // HTTPS
});
if (EnableCsWatcher)
{
    builder.Services.AddSingleton<CsFileWatcher>();
    builder.Services.AddHostedService(sp => sp.GetRequiredService<CsFileWatcher>());
    Log.Information("CsFileWatcher registered.");
}
if (EnableKofaxWatcher)
{
    builder.Services.AddSingleton<KofaxFileWatcher>();
    builder.Services.AddHostedService(sp => sp.GetRequiredService<KofaxFileWatcher>());
    Log.Information("KofaxFileWatcher registered.");
}
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}


//app.UseAntiforgery();

//app.MapStaticAssets();
//app.MapRazorComponents<App>()
//    .AddInteractiveServerRenderMode();

app.Run();
