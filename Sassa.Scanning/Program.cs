using Docnet.Core;
using Docnet.Core.Models;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using Tesseract;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.Linq;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    //.AddEnvironmentVariables()
    .Build();

string pdfPath = configuration["PdfPath"] ?? @"C:\RawScan\OKdurban_kzn_lo2738.pdf";
int cfgWidth = configuration.GetValue<int?>("PageDimensions:Width") ?? 1080;
int cfgHeight = configuration.GetValue<int?>("PageDimensions:Height") ?? 1920;

bool autoRotate = configuration.GetValue<bool?>("Barcode:AutoRotate") ?? true;
bool tryInverted = configuration.GetValue<bool?>("Barcode:TryInverted") ?? true;
bool tryHarder = configuration.GetValue<bool?>("Barcode:TryHarder") ?? true;
var formatStrings = configuration.GetSection("Barcode:PossibleFormats").Get<string[]>() ?? Array.Empty<string>();
var possibleFormats = formatStrings
    .Select(s => Enum.TryParse<BarcodeFormat>(s, true, out var f) ? f : (BarcodeFormat?)null)
    .Where(f => f.HasValue)
    .Select(f => f!.Value)
    .ToList();
if (possibleFormats.Count == 0)
{
    possibleFormats = new() { BarcodeFormat.QR_CODE, BarcodeFormat.CODE_128 };
}

bool previewEnabled = configuration.GetValue<bool?>("Ui:PreviewWindow:Enabled") ?? true;
string previewTitle = configuration["Ui:PreviewWindow:Title"] ?? "Bitmap Preview";
int previewWidth = configuration.GetValue<int?>("Ui:PreviewWindow:Width") ?? 800;
int previewHeight = configuration.GetValue<int?>("Ui:PreviewWindow:Height") ?? 600;

//<div id="sizer" style="width: 1133px; height: 193482px;"></div>
using (var docReader = DocLib.Instance.GetDocReader(pdfPath, new PageDimensions(cfgWidth, cfgHeight)))
using (var pageReader = docReader.GetPageReader(0))
{
    int width = pageReader.GetPageWidth();
    int height = pageReader.GetPageHeight();
    byte[] rawBytes = pageReader.GetImage(); // BGRA format

    // Create a Bitmap from the raw BGRA bytes
    var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
    var rect = new Rectangle(0, 0, width, height);
    var bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
    System.Runtime.InteropServices.Marshal.Copy(rawBytes, 0, bmpData.Scan0, rawBytes.Length);
    bitmap.UnlockBits(bmpData);

    // Create luminance source
    var luminanceSource = new BitmapLuminanceSource(bitmap);

    var reader = new BarcodeReaderGeneric
    {
        AutoRotate = autoRotate,
        Options =
        {
            TryInverted = tryInverted,
            TryHarder = tryHarder,
            PossibleFormats = possibleFormats
        }
    };

    var results = reader.DecodeMultiple(luminanceSource) ?? Array.Empty<Result>();

    Console.WriteLine("📦 Barcodes found:");
    foreach (var result in results)
    {
        Console.WriteLine($"Type: {result.BarcodeFormat}, Value: {result.Text}");
    }

    if (previewEnabled)
    {
        var form = new Form
        {
            Text = previewTitle,
            Width = previewWidth,
            Height = previewHeight
        };

        var pictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = bitmap
        };

        form.Controls.Add(pictureBox);
        Application.Run(form);
    }
}

//static void PreviewBitmap(Bitmap bmp)
//{ 
//    using (var g = Graphics.FromImage(bmp))
//    {
//        g.Clear(Color.White);
//        g.DrawString("Hello Bitmap", new Font("Arial", 16), Brushes.Black, new PointF(10, 40));
//    }

//    // Save to temp file
//    string tempPath = Path.Combine(Path.GetTempPath(), "preview.png");
//    bmp.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);

//    // Open with default image viewer
//    Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });

//    Console.WriteLine($"Preview opened: {tempPath}");
//}
