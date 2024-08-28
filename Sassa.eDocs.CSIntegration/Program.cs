using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sassa.eDocs.Services;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Sassa.eDocs.CSIntegration
{
    class Program
    {
        public static IConfiguration configuration;
        static HttpClient client = new HttpClient();

        static int Main(string[] args)
        {
            if(args.Length != 1)
            {
                Console.WriteLine("Please provide the Application reference as an argument <reference>");
                return 1;
            }
            else
            {
                try
                {
                    // Start!
                    MainAsync(args).Wait();
                    return 0;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return 1;
                }
            }
        }

        static async Task MainAsync(string[] args)
        {
            // Create service collection
            //Log.Information("Creating service collection");
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Create service provider
            //Log.Information("Building service provider");
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            try
            {
                //Log.Information("Starting service");
                await serviceProvider.GetService<CSService>().Run(args[0]);
                //Log.Information("Ending service");
            }
            catch (Exception ex)
            {
                var err = ex.Message;
                //Log.Fatal(ex, "Error running service");
                throw;
            }
            finally
            {
                //Log.CloseAndFlush();
            }
        }
        private static void ConfigureServices(IServiceCollection services)
        {

            // Add logging
            //serviceCollection.AddSingleton(LoggerFactory.Create(builder =>
            //{
            //    builder
            //        .AddSerilog(dispose: true);
            //}));

            //serviceCollection.AddLogging();

            // Build configuration
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            // Add access to generic IConfigurationRoot
            services.AddSingleton<IConfiguration>(configuration);

            services.AddHttpClient("DocumentService", c =>
            {
                c.BaseAddress = new Uri(configuration["DocumentService:Url"]);
                c.DefaultRequestHeaders.Add("ApiKey", configuration["DocumentService:ApiKey"]);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddSingleton<DocumentStore>();

            // Add app
            services.AddTransient<CSService>();
        }
    }
    }
