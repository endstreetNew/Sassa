using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sassa.Services;

// Simple console tool to delete a list of Content Server nodes (DataIDs)
// Reads IDs from CSDUPS.txt (one per line), calls CsUploadService.DeleteForDataId for each.

var builder = Host.CreateApplicationBuilder(args);

// Load configuration from appsettings.json and environment variables
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables();

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

// Bind CsServiceSettings
var settings = new CsServiceSettings();
builder.Configuration.GetSection("CsServiceSettings").Bind(settings);

builder.Services.AddSingleton(settings);

// Register CsUploadService and its logging
builder.Services.AddLogging();
builder.Services.AddSingleton<CsUploadService>();

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("CsDeleteTool");
var cs = host.Services.GetRequiredService<CsUploadService>();

string inputFile = args.Length > 0 ? args[0] : "CSDUPS.txt";
if (!File.Exists(inputFile))
{
    logger.LogError("Input file not found: {File}", inputFile);
    return;
}

var lines = await File.ReadAllLinesAsync(inputFile);
if (lines.Length == 0)
{
    logger.LogWarning("No IDs found in {File}", inputFile);
    return;
}

int success = 0, failed = 0, total = 0;
foreach (var raw in lines)
{
    var line = raw?.Trim();
    if (string.IsNullOrWhiteSpace(line)) continue;

    // Allow lines like "NAME\tDATAID\t..." or plain IDs; pick the second column if tab-separated
    long id;
    if (line.Contains('\t'))
    {
        var parts = line.Split('\t');
        if (parts.Length < 2 || !long.TryParse(parts[1], out id))
        {
            logger.LogWarning("Skipping line (no DATAID): {Line}", line);
            continue;
        }
    }
    else if (!long.TryParse(line, out id))
    {
        // Skip header or invalid line
        logger.LogWarning("Skipping line (not an ID): {Line}", line);
        continue;
    }
    total++;
    try
    {
        await cs.DeleteForDataId(id);
        success++;
        logger.LogInformation("Deleted DataID {Id}", id);
    }
    catch (Exception ex)
    {
        failed++;
        logger.LogError(ex, "Failed to delete DataID {Id}", id);
    }
}

logger.LogInformation("Done. Total: {Total}, Success: {Success}, Failed: {Failed}", total, success, failed);
