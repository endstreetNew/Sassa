using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using razor.Components;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Models;
using Sassa.Brm.Common.Services;
using Sassa.Brm.Health;
using Sassa.BRM.Models;
using Sassa.BRM.Services;
using Sassa.BRM.UI;
using Sassa.Services;
using Sassa.Socpen.Data;
using Serilog;

namespace Sassa.BRM;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //Authentication Services
        builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = options.DefaultPolicy;
        });
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<AuthenticationStateProvider, WindowsAuthenticationStateProvider>();
        builder.Services.AddHttpContextAccessor();
        // connection strings   
        string BrmConnection = builder.Configuration.GetConnectionString("BrmConnection")!;
        string CsConnection = builder.Configuration.GetConnectionString("CsConnection")!;


        //Options pattern for scheduled tasks
        builder.Services.Configure<ScheduleOptions>(options =>
        {
            options.Enabled = builder.Configuration.GetValue<bool>("ScheduleOptions:Enabled");
            options.RunAtHour = builder.Configuration.GetValue<int>("ScheduleOptions:RunAtHour");
            options.ConnectionString = BrmConnection;
        });
        //Factory pattern
        builder.Services.AddDbContextFactory<ModelContext>(options =>
        {
            options.UseOracle(BrmConnection, opt => opt.CommandTimeout(180));
        });
        builder.Services.AddDbContextFactory<SocpenContext>(options =>
        options.UseOracle(BrmConnection));
        //Services 
        builder.Services.AddScoped<BRMDbService>();
        builder.Services.AddSingleton<StaticService>();
        builder.Services.AddScoped<SessionService>();
        builder.Services.AddScoped<SocpenService>();
        builder.Services.AddSingleton<BrmApiService>();
        builder.Services.AddScoped<MisFileService>();
        builder.Services.AddScoped<DestructionService>();

        //builder.Services.AddScoped<LoggingService>();
        builder.Services.AddScoped<TdwBatchService>();
        builder.Services.AddScoped<CoverSheetService>();
        builder.Services.AddSingleton<BarCodeService>();
        builder.Services.AddSingleton<RawSqlService>();
        builder.Services.AddSingleton<FileService>();


        builder.Services.AddSingleton<IEmailSettings, EmailSettings>(c =>
        {
            EmailSettings emailSettings = new EmailSettings();
            emailSettings.ContentRootPath = builder.Environment.ContentRootPath;
            emailSettings.WebRootPath = builder.Environment.WebRootPath;
            emailSettings.ReportFolder = builder.Configuration.GetValue<string>("Folders:Reports")!;
            emailSettings.DocumentFolder = builder.Configuration.GetValue<string>("Folders:Documents")!;
            emailSettings.SmtpServer = builder.Configuration.GetValue<string>("Email:SMTPServer")!;
            emailSettings.SmtpPort = builder.Configuration.GetValue<int>("Email:SMTPPort");
            emailSettings.SmtpUser = builder.Configuration.GetValue<string>("Email:SMTPUser")!;
            emailSettings.SmtpPassword = builder.Configuration.GetValue<string>("Email:SMTPPassword")!;
            emailSettings.TdwReturnedBox = builder.Configuration.GetValue<string>("TDWReturnedBox");
            emailSettings.RegionEmails.Add("GAUTENG", builder.Configuration.GetValue<string>("TDWEmail:GAUTENG")!);
            emailSettings.RegionEmails.Add("FREE STATE", builder.Configuration.GetValue<string>("TDWEmail:FREE STATE")!);
            emailSettings.RegionEmails.Add("KWA-ZULU NATAL", builder.Configuration.GetValue<string>("TDWEmail:KWA-ZULU NATAL")!);
            emailSettings.RegionEmails.Add("KWAZULU NATAL", builder.Configuration.GetValue<string>("TDWEmail:KWA-ZULU NATAL")!);
            emailSettings.RegionEmails.Add("NORTH WEST", builder.Configuration.GetValue<string>("TDWEmail:NORTH WEST")!);
            emailSettings.RegionEmails.Add("MPUMALANGA", builder.Configuration.GetValue<string>("TDWEmail:MPUMALANGA")!);
            emailSettings.RegionEmails.Add("EASTERN CAPE", builder.Configuration.GetValue<string>("TDWEmail:EASTERN CAPE")!);
            emailSettings.RegionEmails.Add("WESTERN CAPE", builder.Configuration.GetValue<string>("TDWEmail:WESTERN CAPE")!);
            emailSettings.RegionEmails.Add("LIMPOPO", builder.Configuration.GetValue<string>("TDWEmail:LIMPOPO")!);
            emailSettings.RegionEmails.Add("NORTHERN CAPE", builder.Configuration.GetValue<string>("TDWEmail:NORTHERN CAPE")!);
            emailSettings.RegionIDEmails.Add("7", builder.Configuration.GetValue<string>("TDWEmail:GAUTENG")!);
            emailSettings.RegionIDEmails.Add("4", builder.Configuration.GetValue<string>("TDWEmail:FREE STATE")!);
            emailSettings.RegionIDEmails.Add("5", builder.Configuration.GetValue<string>("TDWEmail:KWA-ZULU NATAL")!);
            emailSettings.RegionIDEmails.Add("6", builder.Configuration.GetValue<string>("TDWEmail:NORTH WEST")!);
            emailSettings.RegionIDEmails.Add("8", builder.Configuration.GetValue<string>("TDWEmail:MPUMALANGA")!);
            emailSettings.RegionIDEmails.Add("2", builder.Configuration.GetValue<string>("TDWEmail:EASTERN CAPE")!);
            emailSettings.RegionIDEmails.Add("1", builder.Configuration.GetValue<string>("TDWEmail:WESTERN CAPE")!);
            emailSettings.RegionIDEmails.Add("9", builder.Configuration.GetValue<string>("TDWEmail:LIMPOPO")!);
            emailSettings.RegionIDEmails.Add("3", builder.Configuration.GetValue<string>("TDWEmail:NORTHERN CAPE")!);
            return emailSettings;
        });
        builder.Services.AddSingleton<EmailClient>();
        builder.Services.AddSingleton<MailMessages>();
        builder.Services.AddSingleton<CsServiceSettings>(c =>
        {
            CsServiceSettings csServiceSettings = new CsServiceSettings();
            csServiceSettings.CsConnection = CsConnection;
            csServiceSettings.CsWSEndpoint = builder.Configuration.GetValue<string>("ContentServer:CSWSEndpoint")!;
            csServiceSettings.CsServiceUser = builder.Configuration.GetValue<string>("ContentServer:CSServiceUser")!;
            csServiceSettings.CsServicePass = builder.Configuration.GetValue<string>("ContentServer:CSServicePass")!;
            csServiceSettings.CsDocFolder = $"{builder.Environment.WebRootPath}\\{builder.Configuration.GetValue<string>("Folders:CS")}\\";
            csServiceSettings.CsBeneficiaryRoot = builder.Configuration.GetValue<string>("ContentServer:CSBeneficaryRoot")!;
            csServiceSettings.CsMaxRetries = builder.Configuration.GetValue<int>("ContentServer:CSMaxRetries");
            return csServiceSettings;
        });
        builder.Services.AddScoped<CSService>();
        builder.Services.AddScoped<IAlertService, AlertService>();
        builder.Services.AddScoped<Navigation>();
        builder.Services.AddScoped<ReportDataService>();
        builder.Services.AddScoped<ProgressService>();
        builder.Services.AddScoped<Helper>();
        
        builder.Services.AddScoped<DailyCheck>();
        builder.Services.AddScoped<ActiveUser>();
        builder.Services.AddSingleton<ActiveUserList>();

        builder.Services.AddHttpClient();
        //builder.Services.AddHttpClient("BrmApi", client =>
        //{
        //    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("BrmApi:BaseAddress")!);
        //});
        // UI Services
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();
        builder.Services.AddQuickGridEntityFrameworkAdapter();

        //builder.Services.ConfigureApplicationCookie(options =>
        //{
        //    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        //    options.SlidingExpiration = true;
        //});
        builder.Services.AddRazorPages().AddRazorPagesOptions(options =>
        {
            options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
        });
        builder.Services.AddAntiforgery(options =>
        {
            options.SuppressXFrameOptionsHeader = true;
            options.Cookie.Expiration = TimeSpan.Zero;
        });

        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                name: "AllowOrigin",
                builder =>
                {
                    builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                });
        });

        Log.Logger = new LoggerConfiguration()
        .WriteTo.File("Logs/app-Brm-Error.log", rollingInterval: RollingInterval.Day,restrictedToMinimumLevel:Serilog.Events.LogEventLevel.Error)
        .CreateLogger();

        builder.Host.UseSerilog();
        builder.Services.AddSingleton<SocpenUpdateService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<SocpenUpdateService>>();
            return new SocpenUpdateService( logger, CsConnection);
        });
        builder.Services.AddSingleton<ScheduleService>();
        builder.Services.AddHostedService(sp => sp.GetRequiredService<ScheduleService>());

        var app = builder.Build();
        // --- Database connectivity check on startup ---
        using (var scope = app.Services.CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            try
            {
                var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ModelContext>>();
                using var dbContext = dbFactory.CreateDbContext();
                dbContext.Database.OpenConnection();
                dbContext.Database.CloseConnection();
                logger.LogInformation("Database connectivity check succeeded.");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Database connectivity check failed. Application will terminate.");
                Log.CloseAndFlush();
                Environment.Exit(1);
            }
        }
        // --- End database connectivity check ---

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        //app.UseHttpsRedirection();

        //Enable re-authentication for windows authentication
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseStaticFiles();
        //app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .DisableAntiforgery()
            .AddInteractiveServerRenderMode();


        app.Run();
    }
}
