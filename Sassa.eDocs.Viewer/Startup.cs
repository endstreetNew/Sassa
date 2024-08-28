using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sassa.eDocs.Viewer.Services;
using Sassa.eForms.Services;
using System;

namespace Sassa.eDocs.Viewer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHttpClient("DocumentService", c =>
            {
                c.BaseAddress = new Uri(Configuration["DocumentService:Url"]);
                c.DefaultRequestHeaders.Add("ApiKey", Configuration["DocumentService:ApiKey"]);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddScoped<StoreDbService>().AddDbContext<eDocumentContext>(options =>
            options.UseOracle(
            Configuration.GetConnectionString("edocsConnectionstring")));

            services.AddSingleton<DocumentStore>();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
