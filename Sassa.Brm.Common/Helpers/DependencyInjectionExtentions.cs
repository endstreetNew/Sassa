using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sassa.Brm.Common.Models;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Models;
using Sassa.Socpen.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sassa.Brm.Common.Helpers
{
    public static class ServiceInstaller
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
        {
            services.AddCascadingAuthenticationState();
            services.AddHttpContextAccessor();
            services.AddScoped<AuthenticationStateProvider, WindowsAuthenticationStateProvider>();
            return services;
        }

        public static IServiceCollection AddPooledContextServices(this IServiceCollection services, string connection)
        {
            services.AddPooledDbContextFactory<ModelContext>(options => options.UseOracle(connection));
            services.AddPooledDbContextFactory<SocpenContext>(options => options.UseOracle(connection));
            return services;
        }

        public static IServiceCollection AddSharedServices(this IServiceCollection services)
        {
            services.AddSessionServices();
            services.AddEmailService();
            services.AddUserService();
            services.AddScoped<Helper>();
            return services;
        }
        public static IServiceCollection AddSessionServices(this IServiceCollection services)
        {
            services.AddSingleton<StaticService>();
            services.AddScoped<SessionService>();
            
            return services;
        }
        public static IServiceCollection AddEmailService(this IServiceCollection services)
        {
            services.AddSingleton<EmailClient>();
            services.AddSingleton<MailMessages>();
            return services;
        }

        public static IServiceCollection AddUserService(this IServiceCollection services)
        {
            services.AddScoped<ActiveUser>();
            services.AddSingleton<ActiveUserList>();
            return services;
        }
    }
}
