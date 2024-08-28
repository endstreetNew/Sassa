using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sassa.eDocs;
//using Sassa.eForms.Models;
using Sassa.eServices.Admin.Services;
//using Sassa.Surveys.Data;
using Sotsera.Blazor.Toaster.Core.Models;
using System;

namespace Sassa.eServices.Admin
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
           // services.AddDefaultIdentity<SassaUser>(options =>
           // {
           //     options.Password.RequiredLength = 8;
           //     options.Password.RequireUppercase = false;
           //     options.Password.RequireLowercase = false;
           //     options.Password.RequireUppercase = false;
           //     options.Password.RequireNonAlphanumeric = false;
           //     options.Password.RequireDigit = false;
           //     options.SignIn.RequireConfirmedEmail = false;

           // }).AddUserStore<SassaUserStore>()
           //.AddDefaultTokenProviders();

            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("RequireAdmin", c => c.RequireRole("Admin"));
            //});

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddServerSideBlazor().AddCircuitOptions(o =>
            {
                o.DetailedErrors = true;
            });
            //Using windows
            //services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<SassaUser>>();
            //services.AddAuthorization();

            // Access to the http Context

            services.AddHttpContextAccessor();
            services.AddSingleton<AssetService>();
            //services.AddSingleton<EFormService>();
            services.AddSingleton<MaintenanceService>();
            
            //services.AddSingleton<ILOService, LOService>();

            //services.AddScoped<SurveyService>().AddDbContext<SurveyContext>(options =>
            //options.UseOracle(
            //Configuration.GetConnectionString("SurveysOracle")));

            ////For saving large files direct
            //services.AddScoped<StoreDbService>().AddDbContext<eDocumentContext>(options =>
            //options.UseOracle(
            //Configuration.GetConnectionString("edocsConnectionstring")));

            //Recaptcha--------------
            //services.AddControllersWithViews();
            //-----------------------
            //services.AddSingleton<SMSSender>();
            //services.AddSingleton<SassaUserStore>();
            //services.AddSingleton<DocumentStore>();
            services.AddScoped<AppState>();

            //services.AddTransient<Monitor.Client.Services.TimerService>();
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

            services.AddCors(options =>
            {
                options.AddPolicy("DevCorsPolicy", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            services.AddSingleton<CSService>();
            //And this to Configure method:

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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
