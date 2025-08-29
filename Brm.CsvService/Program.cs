using System;
using System.Threading.Tasks;
using Brm.CsvService.Models;

namespace Brm.CsvService
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            //if (args.Length == 0)
            //{
            //    Console.WriteLine("Usage: Brm.CsvService <csv-file-path>");
            //    return;
            //}

            var csvFilePath = @"c:\source\ss_application.csv"; ;

            // Create your Oracle EF context
            using var context = new SSModelContext();

            var csvService = new CsvService(context);
            await csvService.ImportSsAppToOracleAsync(csvFilePath);

            Console.WriteLine("CSV import completed.");
        }
    }
}
