using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sassa.eDocs;
using Sassa.eForms.Identity;
using Sassa.eForms.Models;
using Sassa.eForms.Services;
using Sassa.Surveys.Data;
using Sotsera.Blazor.Toaster.Core.Models;
using System;

namespace Sassa.eForms
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultIdentity<SassaUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.SignIn.RequireConfirmedEmail = false;

            }).AddUserStore<SassaUserStore>()
           .AddDefaultTokenProviders();

            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("RequireAdmin", c => c.RequireRole("Admin"));
            //});

            services.AddRazorPages();
            services.AddControllers();
            services.AddServerSideBlazor();
            services.AddServerSideBlazor().AddCircuitOptions(o =>
            {
                o.DetailedErrors = true;
            });
            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<SassaUser>>();
            services.AddAuthorization();

            // Access to the http Context

            services.AddHttpContextAccessor();

            services.AddRazorPages();
            services.AddControllers();
            services.AddServerSideBlazor();
            services.AddServerSideBlazor().AddCircuitOptions(o =>
            {
                o.DetailedErrors = true;
            });

            services.AddSingleton<EFormService>();
            services.AddSingleton<MaintenanceService>();
            services.AddSingleton<ILOService, LOService>();

            services.AddScoped<SurveyService>().AddDbContext<SurveyContext>(options =>
            options.UseOracle(
            Configuration.GetConnectionString("SurveysOracle")));

            //For saving large files direct
            services.AddScoped<StoreDbService>().AddDbContext<eDocumentContext>(options =>
            options.UseOracle(
            Configuration.GetConnectionString("edocsConnectionstring"))); 

            //Recaptcha--------------
            //services.AddControllersWithViews();
            //-----------------------
            services.AddSingleton<SMSSender>();
            services.AddSingleton<SassaUserStore>();
            services.AddSingleton<DocumentStore>();
            services.AddScoped<AppState>();
            //HttpClients
            services.AddHttpClient("UserService", c =>
            {
                c.BaseAddress = new Uri(Configuration["UserService:Url"]);
                c.DefaultRequestHeaders.Add("ApiKey", Configuration["UserService:ApiKey"]);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddHttpClient("DocumentService", c =>
            {
                c.BaseAddress = new Uri(Configuration["DocumentService:Url"]);
                c.DefaultRequestHeaders.Add("ApiKey", Configuration["DocumentService:ApiKey"]);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddHttpClient("SMSClient", c =>
            {
                c.BaseAddress = new Uri(Configuration["SMSService:Url"]);
                c.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Configuration["SMSService:Authorization"]);
            });
            services.AddToaster(config =>
            {
                //example customizations
                config.PositionClass = Defaults.Classes.Position.TopCenter;
                config.PreventDuplicates = true;
                config.NewestOnTop = true;
                config.VisibleStateDuration = 35;
                config.HideTransitionDuration = 1;
            });
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UsePathBase("/eservices");
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
