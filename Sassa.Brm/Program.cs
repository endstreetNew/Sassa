using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using razor.Components;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Models;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Components;
using Sassa.BRM.Models;
using Sassa.BRM.Services;
using Sassa.Socpen.Data;

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

        builder.Services.AddSingleton<BrmApiService>();
        builder.Services.AddScoped<MisFileService>();
        builder.Services.AddScoped<DestructionService>();
        builder.Services.AddScoped<SocpenService>();
        builder.Services.AddScoped<TdwBatchService>();
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
        builder.Services.AddScoped<IAlertService, AlertService>();
        builder.Services.AddScoped<Navigation>();
        builder.Services.AddScoped<CSService>();
        builder.Services.AddScoped<ReportDataService>();
        builder.Services.AddScoped<ProgressService>();
        builder.Services.AddScoped<Helper>();

        builder.Services.AddScoped<ActiveUser>();
        builder.Services.AddSingleton<ActiveUserList>();

        builder.Services.AddHttpClient();
        //builder.Services.AddHttpClient("BrmApi", client =>
        //{
        //    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("BrmApi:BaseAddress")!);
        //});
        // UI Services
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
            options.SlidingExpiration = true;
        });
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

        var app = builder.Build();


        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        //app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .DisableAntiforgery()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
