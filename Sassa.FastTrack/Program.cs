using Microsoft.EntityFrameworkCore;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Models;
using Sassa.BRM.Services;
using Sassa.FastTrack.UI;
using Sassa.Models;
using Sassa.Services;
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
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(builder.Configuration.GetValue<int>("Urls:AppPort")); // HTTP
                                                                                    // serverOptions.ListenAnyIP(5001, listenOptions => listenOptions.UseHttps()); // HTTPS
});

builder.Services.AddSingleton<CsFileWatcher>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<CsFileWatcher>());
Log.Information("CsFileWatcher registered.");

if (EnableKofaxWatcher)
{
    builder.Services.AddSingleton<KofaxFileWatcher>();
    builder.Services.AddHostedService(sp => sp.GetRequiredService<KofaxFileWatcher>());
    Log.Information("KofaxFileWatcher registered.");
}
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.UserInteractive)
{
    var port = builder.Configuration.GetValue<int>("Urls:AppPort");
    StartTrayIcon(app.Lifetime, port, builder.Environment.WebRootPath, app.Services);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();

static void StartTrayIcon(IHostApplicationLifetime lifetime, int port, string webRootPath, IServiceProvider services)
{
    var thread = new Thread(() =>
    {
        Application.Run(new TrayContext(lifetime, port, webRootPath, services));
    });
    thread.SetApartmentState(ApartmentState.STA);
    thread.IsBackground = true;
    thread.Start();
}

sealed class TrayContext : ApplicationContext
{
    private readonly NotifyIcon _tray;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly string _homeUrl;
    private readonly string _webRoot;
    private readonly CsFileWatcher? _csWatcher;
    private readonly KofaxFileWatcher? _kofaxWatcher;

    public TrayContext(IHostApplicationLifetime lifetime, int port, string webRootPath, IServiceProvider services)
    {
        _lifetime = lifetime;
        _homeUrl = $"http://localhost:{port}/";
        _webRoot = webRootPath;
        _csWatcher = services.GetService<CsFileWatcher>();
        _kofaxWatcher = services.GetService<KofaxFileWatcher>();

        var menu = new ContextMenuStrip { ImageScalingSize = new Size(16, 16) };
        var imgHome = LoadSvgImage("open-iconic/svg/home.svg", 16);
        var imgStart = LoadSvgImage("open-iconic/svg/media-play.svg", 16);
        var imgStop = LoadSvgImage("open-iconic/svg/media-stop.svg", 16);
        var imgPause = LoadSvgImage("open-iconic/svg/media-pause.svg", 16);

        menu.Items.Add(new ToolStripMenuItem("Home", imgHome, (_, __) => Open(_homeUrl)));
        menu.Items.Add(new ToolStripSeparator());

        // App lifecycle
        //menu.Items.Add(new ToolStripMenuItem("Start", imgStart, async (_, __) => await RestartAsync()));
        menu.Items.Add(new ToolStripMenuItem("Stop", imgStop, (_, __) => _lifetime.StopApplication()));

        // CS File Watcher controls
        var pauseWatcher = new ToolStripMenuItem("Pause CS Watcher", imgPause, (_, __) => PauseCsFileWatcher()) { Enabled = _csWatcher is not null };
        var resumeWatcher = new ToolStripMenuItem("Resume CS Watcher", imgStart, (_, __) => ResumeCsFileWatcher()) { Enabled = _csWatcher is not null };
        // Kofax File Watcher controls
        var pauseKofaxWatcher = new ToolStripMenuItem("Pause Kofax Watcher", imgPause, (_, __) => PauseKofaxFileWatcher()) { Enabled = _kofaxWatcher is not null };
        var resumeKofaxWatcher = new ToolStripMenuItem("Resume Kofax Watcher", imgStart, (_, __) => ResumeKofaxFileWatcher()) { Enabled = _kofaxWatcher is not null };
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(pauseWatcher);
        menu.Items.Add(resumeWatcher);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(pauseKofaxWatcher);
        menu.Items.Add(resumeKofaxWatcher);

        var contentDir = Path.Combine(AppContext.BaseDirectory, "images");
        var icoPath = Path.Combine(contentDir, "trayIcon.ico");

        _tray = new NotifyIcon
        {
            Icon = new Icon(icoPath, SystemInformation.SmallIconSize),
            Text = "Sassa.FastTrack",
            Visible = true,
            ContextMenuStrip = menu
        };
        _tray.DoubleClick += (_, __) => Open(_homeUrl);

        _lifetime.ApplicationStopping.Register(() =>
        {
            try { _tray.Visible = false; _tray.Dispose(); } catch { }
            Application.ExitThread();
        });
    }

    // Public functions to control CsFileWatcher from external threads or menu handlers
    public void PauseCsFileWatcher() => _csWatcher?.Pause();
    public void ResumeCsFileWatcher() => _csWatcher?.Resume();
    public void PauseKofaxFileWatcher() => _kofaxWatcher?.Pause();
    public void ResumeKofaxFileWatcher() => _kofaxWatcher?.Resume();


    private string PathFromWebRoot(string relative)
        => System.IO.Path.Combine(_webRoot, relative.Replace('/', System.IO.Path.DirectorySeparatorChar));

    private Image? LoadSvgImage(string rel, int size)
    {
        try
        {
            var doc = SvgDocument.Open(PathFromWebRoot(rel));
            return doc.Draw(size, size);
        }
        catch { return null; }
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool DestroyIcon(IntPtr handle);

    private Icon LoadSvgIcon(string rel, int size)
    {

        var doc = SvgDocument.Open(PathFromWebRoot(rel));
        using var bmp = doc.Draw(size, size) as Bitmap ?? new Bitmap(size, size);
        var hIcon = bmp.GetHicon();
        var icon = Icon.FromHandle(hIcon);
        var clone = (Icon)icon.Clone();
        DestroyIcon(hIcon); // avoid handle leak
        icon.Dispose();
        return clone;
    }

    private static void Open(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { }
    }

    //private async Task RestartAsync()
    //{
    //    try
    //    {
    //        var exe = Environment.ProcessPath!;
    //        var args = string.Join(' ', Environment.GetCommandLineArgs().Skip(1).Select(a => a.Contains(' ') ? $"\"{a}\"" : a));
    //        Process.Start(new ProcessStartInfo("cmd.exe", $"/C timeout /T 1 /NOBREAK >NUL & \"{exe}\" {args}")
    //        {
    //            UseShellExecute = false,
    //            CreateNoWindow = true
    //        });
    //    }
    //    catch { }
    //    finally
    //    {
    //        _lifetime.StopApplication();
    //    }
    //}
}




