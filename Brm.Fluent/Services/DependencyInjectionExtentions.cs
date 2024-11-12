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
    public static class DependencyInjectionExtentions
    {
    //    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    //    {
    //        services.AddCascadingAuthenticationState();
    //        services.AddHttpContextAccessor();
    //        services.AddScoped<AuthenticationStateProvider, WindowsAuthenticationStateProvider>();
    //        return services;
    //    }

    //    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, string connection)
    //    {
    //        //services.AddDbContextPool<ModelContext>(options => options.UseOracle(connection));
    //        services.AddDbContextFactory<ModelContext>(options => options.UseOracle(connection));
    //        services.AddPooledDbContextFactory<SocpenContext>(options => options.UseOracle(connection));
    //        return services;
    //    }
    //    public static IServiceCollection AddPooledContextServices(this IServiceCollection services,string connection)
    //    {
    //        services.AddPooledDbContextFactory<ModelContext>(options => options.UseOracle(connection));
    //        services.AddPooledDbContextFactory<SocpenContext>(options => options.UseOracle(connection));
    //        return services;
    //    }
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddScoped<BRMDbService>();
        services.AddScoped<QueryableDataService>();
        services.AddScoped<MisFileService>();
        services.AddScoped<DestructionService>();
        services.AddScoped<SocpenService>();
        services.AddScoped<TdwBatchService>();
        services.AddScoped<ReportDataService>();
        services.AddScoped<ProgressService>();
        services.AddSingleton<RawSqlService>();
        services.AddGeneralServices();
        return services;
    }
        public static IServiceCollection AddGeneralServices(this IServiceCollection services)
        {
            services.AddSingleton<FileService>();
            services.AddScoped<CSService>();
            services.AddSingleton<BarCodeService>();
            return services;
        }
    }
}
