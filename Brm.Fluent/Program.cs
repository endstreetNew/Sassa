using Brm.Fluent.Components;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Models;
using Sassa.BRM.Models;
using Sassa.Socpen.Data;

//using Plugin.Chat.Services;

namespace Brm.Fluent;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        //Connection strings   
        string BrmConnection = builder.Configuration.GetConnectionString("BrmConnection")!;
        //string CsConnection = builder.Configuration.GetConnectionString("CsConnection")!;
        //Framework Services

        builder.Services.AddAuthenticationServices();

        builder.Services.AddScoped<Navigation>();

        //Factory pattern for contexts
        builder.Services.AddPooledContextServices(BrmConnection);
        //DataServices 
        builder.Services.AddDataServices();
        //Common Services
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
        builder.Services.AddSharedServices();
        //Api
        builder.Services.AddSingleton<BrmApiService>();
        //HttpClient (for API and Aspire)
        builder.Services.AddHttpClient();
        //Chat
        //builder.Services.AddChatService();
        //Log to the web console
        builder.Services.AddScoped<LoggingService>();
        // UI 
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();
        builder.Services.AddFluentUIComponents();
        builder.Services.AddDataGridEntityFrameworkAdapter();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
            options.SlidingExpiration = true;
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
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        app.Run();
    }
}
