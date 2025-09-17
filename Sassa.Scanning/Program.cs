using iText.Kernel.Pdf;
using Microsoft.Extensions.Configuration;
using Sassa.Scanning.Models;
using Sassa.Scanning.Settings;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    //.AddEnvironmentVariables()
    .Build();

// Bind configuration into strongly-typed settings with sensible defaults
var settings = configuration.Get<ScanningSettings>() ?? new ScanningSettings();

//Get the source document
byte[] sourcePdfBytes = File.ReadAllBytes(settings.PdfPath);

// Split into individual page PDFs in memory
PdfScanDocument scanDocument = SplitPdfToPages(sourcePdfBytes);

PdfBarcodeSplitter splitter = new PdfBarcodeSplitter();
splitter.SplitPdfByTwoBarcodes(scanDocument, settings.OutputPath, settings);

static PdfScanDocument SplitPdfToPages(byte[] sourcePdfBytes)
{
    var result = new PdfScanDocument();
    result.Content = sourcePdfBytes;
    using (var srcStream = new MemoryStream(sourcePdfBytes))
    using (var pdfDoc = new PdfDocument(new PdfReader(srcStream)))
    {
        result.endPage = pdfDoc.GetNumberOfPages();

        for (int pageNum = 1; pageNum <= result.endPage; pageNum++)
        {
            PdfScanPage page = new PdfScanPage() { PageNumber = pageNum };
            using (var outStream = new MemoryStream())
            {
                using (var writer = new PdfWriter(outStream))
                using (var newDoc = new PdfDocument(writer))
                {
                    pdfDoc.CopyPagesTo(pageNum, pageNum, newDoc);
                    page.ps = newDoc.GetFirstPage().GetPageSize();
                }
                page.Content = outStream.ToArray();
                page.PageNumber = pageNum;
            }
            result.Pages.Add(page);
        }
    }

    return result;
}




