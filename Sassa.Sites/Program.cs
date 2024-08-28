using Microsoft.AspNetCore.Server.IISIntegration;
using Sassa.Sites.Components;
using Sassa.Sites.Services;

namespace Sassa.Sites
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            //builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);
            //builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddSingleton<ServerList>();
            builder.Services.AddSingleton<ServerService>();
            // Add services to the container.
            builder.Services.AddRazorComponents().AddInteractiveServerComponents();

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
}
