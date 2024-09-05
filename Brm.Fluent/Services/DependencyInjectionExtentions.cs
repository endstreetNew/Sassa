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
