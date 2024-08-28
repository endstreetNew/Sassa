//using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sassa.Brm.Reports.Pages.Components;
using razor.Components;
using Sassa.BRM.Models;
using Sassa.Brm.Reports.Services;
using System.Security.Principal;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Sassa.Brm.Common.Models;
using Sassa.Brm.Common.Services;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Sassa.Brm.Common.Helpers;

namespace Sassa.Brm.Reports
{

    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            string BrmConnectionString = builder.Configuration.GetConnectionString("BrmConnection")!;
            // Add services to the container.
            builder.Services.AddDbContextFactory<ModelContext>(options =>
            options.UseOracle(BrmConnectionString));
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = options.DefaultPolicy;
            });
            builder.Services.AddSingleton<StaticDataService>();
            //Factory pattern
            builder.Services.AddDbContextFactory<ModelContext>(options =>
            options.UseOracle(BrmConnectionString));
            //builder.Services.AddScoped<WindowsIdentity>();
            //Services 
            builder.Services.AddScoped<SessionService>().AddDbContext<ModelContext>(options =>
            options.UseOracle(BrmConnectionString));
            //Host Environment
            builder.Services.AddSingleton<IEmailSettings,EmailSettings>(c =>
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

            builder.Services.AddScoped<StaticService>().AddDbContext<ModelContext>(options =>
            options.UseOracle(BrmConnectionString));

            builder.Services.AddScoped<ReportDataService>();
            builder.Services.AddScoped<ProgressService>();
            builder.Services.AddScoped<Helper>();
            builder.Services.AddSingleton<RawSqlService>();
            builder.Services.AddSingleton<MailMessages>();

            //builder.SAddSingleton<FileService>();

            builder.Services.AddScoped<IAlertService, AlertService>();

            builder.Services.AddScoped<ActiveUser>();
            builder.Services.AddSingleton<ActiveUserList>();

            builder.Services.AddHttpClient();

            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; }); ;

            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            //builder.Services.AddScoped<WindowsIdentity>(provider =>
            //{
            //    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            //    return (WindowsIdentity)httpContextAccessor.HttpContext.User.Identity;
            //});
            //app.UseStaticFiles();
            //app.UseRouting();
            //var wsOptions = new WebSocketOptions()
            //{
            //    KeepAliveInterval = TimeSpan.FromSeconds(120)
            //};
            //app.UseWebSockets(wsOptions);
            //app.UseHttpsRedirection();
            //app.UseEndpoints(endpoints =>
            //{
            //    //endpoints.MapControllers();
            //    endpoints.MapBlazorHub();
            //    //endpoints.MapBlazorHub(options => options.WebSockets.CloseTimeout = new TimeSpan(1, 1, 1));
            //    endpoints.MapFallbackToPage("/_Host");
            //});
            //app.MapControllers();
            //app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseStaticFiles();
            app.UseRouting();
            var wsOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120)
            };
            app.UseWebSockets(wsOptions);
            app.UseHttpsRedirection();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                //endpoints.MapBlazorHub(options => options.WebSockets.CloseTimeout = new TimeSpan(1, 1, 1));
                endpoints.MapFallbackToPage("/_Host");
            });
            app.Run();
        }
    }
    //public class Program
    //{
    //    public static void Main(string[] args)
    //    {
    //        CreateHostBuilder(args).Build().Run();
    //    }

    //    public static IHostBuilder CreateHostBuilder(string[] args) =>
    //        Host.CreateDefaultBuilder(args)
    //            .ConfigureWebHostDefaults(webBuilder =>
    //            {
    //                webBuilder.UseStaticWebAssets();
    //                webBuilder.UseStartup<Startup>();
    //            });
    //}
}
