using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Brm.CsvService.Models;
using Sassa.BRM.Models; // Or your Oracle context namespace

namespace Brm.CsvService
{
    public class CsvService
    {
        private readonly SSModelContext _context; // Oracle EF context

        public CsvService(SSModelContext context)
        {
            _context = context;
        }

        public async Task ImportCsvToOracleAsync(string csvFilePath)
        {
            const int batchSize = 10000;
            var batch = new List<SsApp>(batchSize);

            using (var reader = new StreamReader(csvFilePath))
            {
                string? headerLine = await reader.ReadLineAsync(); // skip header
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var fields = line.Split(',');
                    if (fields.Length < 22) continue;
                    var record = new SsApp
                    {
                        FormNo = decimal.Parse(fields[0], CultureInfo.InvariantCulture),
                        IdNumber = fields[1],
                        Name = fields[2],
                        Surname = fields[3],
                        ApplicationDate = DateTime.TryParse(fields[4], CultureInfo.InvariantCulture, DateTimeStyles.None, out var appDate) ? appDate : (DateTime?)null,
                        GrantType = fields[5].Length > 5? "U" : fields[5],
                        FormType = fields[6].Length > 5 ? "U" : fields[6 ],
                        DisabilityType = string.Empty,
                        DisabilityDesc = string.Empty,
                        MedNo = string.Empty,
                        Gender = string.Empty,
                        Race = string.Empty,
                        RegionCode = fields[12],
                        DistrictOffice = fields[13],
                        ServiceOffice = fields[14],
                        Box = string.Empty,
                        Position = string.Empty,
                        AYear = decimal.TryParse(fields[17], CultureInfo.InvariantCulture, out var aYear) ? aYear : (decimal?)null,
                        BoxType = string.Empty,
                        ApplStatus = fields[19],
                        ActionDate = DateTime.TryParse(fields[20], CultureInfo.InvariantCulture, DateTimeStyles.None, out var actDate) ? actDate : (DateTime?)null,
                        ActionResult = string.Empty,
                    };
                    batch.Add(record);

                    if (batch.Count >= batchSize)
                    {
                        await SaveBatchAsync(batch);
                        batch.Clear();
                    }
                }
            }

            // Save any remaining records
            if (batch.Count > 0)
            {
                await SaveBatchAsync(batch);
                batch.Clear();
            }
        }

        // Helper method to save a batch and clear change tracker for memory conservation
        private async Task SaveBatchAsync(List<SsApp> batch)
        {
            try
            {

                _context.SsApps.AddRange(batch);
                await _context.SaveChangesAsync();
                _context.ChangeTracker.Clear(); // Release tracked entities from memory
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                Console.WriteLine($"Error saving batch: {ex.Message}");
                //throw; // or handle it as needed
            }
        }
    }
}

