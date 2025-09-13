using Microsoft.EntityFrameworkCore;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Api.Services;
using Sassa.BRM.Models;
using Sassa.Models;
using Sassa.Services;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;


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
    .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("Logs/fasttrack-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        shared: true,
        buffered: false)
    .CreateLogger();
builder.Host.UseSerilog();

string? BrmConnectionString = builder.Configuration.GetConnectionString("BrmConnection");
string? LoConnectionString  = builder.Configuration.GetConnectionString("LoConnection");
string? CsConnectionString  = builder.Configuration.GetConnectionString("CsConnection");

static string Require(string? v, string name)
    => !string.IsNullOrWhiteSpace(v) ? v : throw new InvalidOperationException($"Missing connection string: {name}");

BrmConnectionString = Require(BrmConnectionString, "BrmConnection");
LoConnectionString  = Require(LoConnectionString, "LoConnection");
CsConnectionString  = Require(CsConnectionString, "CsConnection");
// Add services to the container.
//Factory pattern
builder.Services.AddDbContextFactory<ModelContext>(options =>
options.UseOracle(BrmConnectionString));
builder.Services.AddDbContextFactory<LoModelContext>(options =>
options.UseOracle(LoConnectionString));
builder.Services.AddSingleton<StaticService>();
builder.Services.AddScoped<ActivityService>();
builder.Services.AddScoped<ApplicationService>();

//builder.Services.AddScoped<FasttrackService>();
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
//builder.Services.AddSingleton<CoverSheetService>();


builder.Services.AddHttpClient();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(builder.Configuration.GetValue<int>("Urls:AppPort")); // HTTP
                                                                                    // serverOptions.ListenAnyIP(5001, listenOptions => listenOptions.UseHttps()); // HTTPS
});

//Log.Logger = new LoggerConfiguration()
//.WriteTo.File("Logs/FastTrack-Error.log", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
//.CreateLogger();




var app = builder.Build();

// Windows tray icon (only on Windows desktop sessions)
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    var appPort = builder.Configuration.GetValue<int>("Urls:AppPort");
    StartTrayIcon(app.Lifetime, app, appPort);
}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// ---------- Tray icon helpers ----------

static void StartTrayIcon(IHostApplicationLifetime lifetime, IHost host, int port)
{
    var trayThread = new Thread(() =>
    {
        Application.Run(new TrayApplicationContext(lifetime, host, port));
    })
    {
        IsBackground = true,
        Name = "TrayIconThread",
        Priority = ThreadPriority.BelowNormal
    };
    trayThread.SetApartmentState(ApartmentState.STA);
    trayThread.Start();
}

sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IHost _host;
    private readonly string _swaggerUrl;


    public TrayApplicationContext(IHostApplicationLifetime lifetime, IHost host, int port)
    {
        _lifetime = lifetime;
        _host = host;
        _swaggerUrl = $"http://localhost:{port}/swagger";

        // Menu
        var menu = new ContextMenuStrip();
        var openSwagger = new ToolStripMenuItem("Open Swagger", null, (_, __) => OpenSwagger());
        var restart = new ToolStripMenuItem("Restart API", null, async (_, __) => await RestartAsync());
        var stop = new ToolStripMenuItem("Stop API (Exit)", null, (_, __) => StopAndExit());
        menu.Items.Add(openSwagger);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(restart);
        menu.Items.Add(stop);

        var contentDir = Path.Combine(AppContext.BaseDirectory, "images");
        var icoPath    = Path.Combine(contentDir, "trayIcon.ico");

        _notifyIcon = new NotifyIcon
        {
            Icon = new Icon(icoPath, SystemInformation.SmallIconSize),
            Text = "Sassa.Brm.Api",
            Visible = true,
            ContextMenuStrip = menu
        };
        _notifyIcon.DoubleClick += (_, __) => OpenSwagger();

        // Clean up icon when the app is stopping
        _lifetime.ApplicationStopping.Register(() =>
        {
            try
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            catch { /* ignore */ }
            Application.ExitThread(); // end tray thread loop
        });
    }

    private void OpenSwagger()
    {
        try
        {
            Process.Start(new ProcessStartInfo(_swaggerUrl) { UseShellExecute = true });
        }
        catch { /* ignore */ }
    }

    private async Task RestartAsync()
    {
        try
        {
            // Start a new instance of the same executable with same args
            var exe = Environment.ProcessPath!;
            var args = string.Join(' ', Environment.GetCommandLineArgs().Skip(1).Select(QuoteArg));
            Process.Start(new ProcessStartInfo(exe, args) { UseShellExecute = true });
        }
        catch { /* ignore */ }
        finally
        {
            StopAndExit();
        }

        static string QuoteArg(string a) => a.Contains(' ') ? $"\"{a}\"" : a;
    }

    private void StopAndExit()
    {
        // Triggers graceful shutdown; Program.app.Run() will return and the process exits.
        _lifetime.StopApplication();
    }

}