using Docnet.Core;
using Docnet.Core.Models;
using iText.Kernel.Pdf;
using Sassa.Scanning.Models;
using Sassa.Scanning.Services;
using Sassa.Scanning.Settings;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

public class PdfBarcodeSplitter
{
    ProgressWindow _window;
    public PdfBarcodeSplitter(ProgressWindow window)
    {
        _window = window;
        _window.Initialize();
    }
    //KZN: \\ssvsqakfshc05.sassa.local\KZN_E_Coversheet
    //MPU: \\ssvsqakfshc05.sassa.local\MPU_E_Coversheet
    //GAU: \\ssvsqakfshc05.sassa.local\GAU_E_Coversheet
    public void SplitPdfByTwoBarcodes(PdfScanDocument inputDocument, string outputFolder, ScanningSettings settings)
    {
        Bitmap bmp = new Bitmap(100, 100);



        var barcodeReader = new BarcodeReaderGeneric
        {
            AutoRotate = settings.Barcode.AutoRotate,
            Options = new DecodingOptions
            {
                TryHarder = settings.Barcode.TryHarder,
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.CODE_128 },
                AllowedLengths = new int[] { 8, 16 },
            }
        };

        PdfScanDocument outputDocument = new PdfScanDocument();
        outputDocument.startPage = 1;

        try
        {
            foreach (var page in inputDocument.Pages)
            {
                if (page.ps.GetWidth() > page.ps.GetHeight())
                {
                    outputDocument.endPage = page.PageNumber;
                    continue;
                }

                // Convert pdf page to bitmap
                bmp = ConvertBgraToBitmap(page.Content, page.ps, settings.ScanDpi);


                int rectWidth = (int)(bmp.Width * 0.35);
                int rectHeight = (int)(bmp.Height * 0.22);
                int rectX = bmp.Width - rectWidth;
                int rectY = 0;

                using (Graphics g = Graphics.FromImage(bmp))
                {
                    using (var pen = new Pen(System.Drawing.Color.Red, 3))
                    {
                        g.DrawRectangle(pen, rectX, rectY, rectWidth, rectHeight);
                    }
                }
                Bitmap pmap = (Bitmap)bmp.Clone();
                //Draw red rectangle arounf the barcode area
                // Copy the rectangle region into a new bitmap
                Bitmap barcodeBitmap = new Bitmap(rectWidth, rectHeight);
                using (Graphics g = Graphics.FromImage(barcodeBitmap))
                {
                    g.DrawImage(bmp, new System.Drawing.Rectangle(0, 0, rectWidth, rectHeight), new System.Drawing.Rectangle(rectX, rectY, rectWidth, rectHeight), GraphicsUnit.Pixel);
                }

                BitmapLuminanceSource ls = new BitmapLuminanceSource(barcodeBitmap);

                var result = barcodeReader.Decode(ls);

                if (result != null && result.Text.Length == 8)
                {
                    if (!string.IsNullOrEmpty(outputDocument.OutputFile))
                    {
                        SavePdfFireAndForget(inputDocument.Content, outputDocument);
                        outputDocument = new PdfScanDocument();
                        outputDocument.startPage = page.PageNumber;
                    }
                    string barcode1 = SanitizeFileName(result.Text);
                    string barcode2 = SanitizeFileName("Dummy"); // or results[1].Text

                    outputDocument.OutputFile = System.IO.Path.Combine(outputFolder, $"{barcode2}.{barcode1}.pdf");
                    if (settings.PreviewWindow.Enabled)
                    {
                        _window.UpdatePreviewImage(pmap);
                    }
                }

                outputDocument.endPage = page.PageNumber;
                outputDocument.Pages.Add(page);

                // Dispose or reuse bmp after decoding; keep last instance alive for preview cloning
                bmp.Dispose();
            }

            SavePdf(inputDocument.Content, outputDocument);
        }
        catch (Exception ex)
        {
            _ = ex;
        }
    }

    static void SavePdfFireAndForget(byte[] source, PdfScanDocument outputDocument)
    {
        _ = Task.Run(() =>
        {
            try
            {
                if (File.Exists(outputDocument.OutputFile)) { File.Move(outputDocument.OutputFile, outputDocument.OutputFile.Replace("Dummy", "DuplicateBarcode.Dummy"), true); }
                byte[] subsetBytes = ExtractPageRange(source, outputDocument.startPage, outputDocument.endPage);
                File.WriteAllBytes(outputDocument.OutputFile, subsetBytes);
            }
            catch (Exception)
            {
                // TODO: log the exception
            }
        });
    }
    static void SavePdf(byte[] source, PdfScanDocument outputDocument)
    {
        try
        {
            if (File.Exists(outputDocument.OutputFile)) { File.Move(outputDocument.OutputFile, outputDocument.OutputFile.Replace("Dummy", "DuplicateBarcode.Dummy"), true); }
            byte[] subsetBytes = ExtractPageRange(source, outputDocument.startPage, outputDocument.endPage);
            File.WriteAllBytes(outputDocument.OutputFile, subsetBytes);
        }
        catch (Exception)
        {
            // TODO: log the exception
        }
    }
    static byte[] ExtractPageRange(byte[] sourcePdf, int startPage, int endPage)
    {
        var writerProps = new WriterProperties().UseSmartMode().SetFullCompressionMode(true);

        using var srcStream = new MemoryStream(sourcePdf);
        using var reader = new PdfReader(srcStream);
        using var srcDoc = new PdfDocument(reader);

        using var outStream = new MemoryStream();
        using (var writer = new PdfWriter(outStream, writerProps))
        using (var newDoc = new PdfDocument(writer))
        {
            // Copy the specified range into the new document
            srcDoc.CopyPagesTo(startPage, endPage, newDoc);
        }

        return outStream.ToArray();
    }


    //}
    static string SanitizeFileName(string text)
    {
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            text = text.Replace(c, '_');
        return text;
    }
    static PageDimensions GetPageDimensions(iText.Kernel.Geom.Rectangle ps, int dpi)
    {
        int w = (int)Math.Ceiling(ps.GetWidth() / 72f * dpi);
        int h = (int)Math.Ceiling(ps.GetHeight() / 72f * dpi);
        return new PageDimensions(w, h);
    }

    private static Bitmap ConvertBgraToBitmap(byte[] rawBytes, iText.Kernel.Geom.Rectangle ps, int dpi)//, IReadOnlyDictionary<string, object> context)
    {
        using var docReader = DocLib.Instance.GetDocReader(rawBytes, GetPageDimensions(ps, dpi));
        using var pageReader = docReader.GetPageReader(0);
        var width = pageReader.GetPageWidth();
        var height = pageReader.GetPageHeight();
        var pixelData = pageReader.GetImage();

        using var img = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(pixelData, width, height);
        using var ms = new MemoryStream();
        img.SaveAsBmp(ms);
        ms.Position = 0;
        using var tmp = new Bitmap(ms);
        // Detach from MemoryStream to avoid lifetime coupling
        return new Bitmap(tmp);
    }
}