using Microsoft.Extensions.DependencyInjection;
using Sassa.Brm.Common.Models;
using Sassa.Brm.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sassa.Brm.Common.Helpers
{
    public static class DependencyInjectionExtentions
    {
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
