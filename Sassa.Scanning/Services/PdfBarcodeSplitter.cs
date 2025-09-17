using Docnet.Core;
using Docnet.Core.Models;
using iText.Kernel.Pdf;
using Sassa.Scanning.Models;
using Sassa.Scanning.Settings;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

public class PdfBarcodeSplitter
{
    // Preview window plumbing
    private Thread? _previewThread;
    private Form? _previewForm;
    private PictureBox? _previewPictureBox;
    private ManualResetEventSlim? _previewReady;
    private CheckBox? _previewToggle;
    private volatile bool _previewEnabled;

    private void StartPreviewWindow(int width, int height, string title = "Preview")
    {
        _previewReady = new ManualResetEventSlim(false);
        _previewThread = new Thread(() =>
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            _previewForm = new Form
            {
                Text = title,
                Width = width,
                Height = height
            };
            _previewToggle = new CheckBox
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Checked = _previewEnabled,
                Text = _previewEnabled ? "Preview On" : "Preview Off",
                Padding = new Padding(8)
            };
            _previewToggle.CheckedChanged += (s, e) =>
            {
                _previewEnabled = _previewToggle.Checked;
                _previewToggle.Text = _previewEnabled ? "Preview On" : "Preview Off";
            };
            _previewPictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            _previewForm.Controls.Add(_previewPictureBox);
            _previewForm.Controls.Add(_previewToggle);
            _previewReady!.Set();
            Application.Run(_previewForm);
        });
        _previewThread.SetApartmentState(ApartmentState.STA);
        _previewThread.IsBackground = true;
        _previewThread.Start();
        _previewReady.Wait();
    }

    private void UpdatePreviewImage(Bitmap bmp)
    {
        if (_previewPictureBox == null || !_previewEnabled) return;

        void SetImage()
        {
            if (!_previewEnabled) return;
            // Clone to avoid disposing in caller affecting UI
            var clone = (Bitmap)bmp.Clone();
            var old = _previewPictureBox!.Image;
            _previewPictureBox.Image = clone;
            old?.Dispose();
        }

        if (_previewPictureBox.InvokeRequired)
            _previewPictureBox.BeginInvoke(new Action(SetImage));
        else
            SetImage();
    }

    private void StopPreviewWindow()
    {
        if (_previewForm == null) return;
        try
        {
            if (_previewForm.InvokeRequired)
                _previewForm.BeginInvoke(new Action(() => _previewForm.Close()));
            else
                _previewForm.Close();
            _previewThread?.Join(2000);
        }
        catch { /* ignore */ }
        finally
        {
            _previewThread = null;
            _previewForm = null;
            _previewPictureBox = null;
            _previewToggle = null;
            _previewReady?.Dispose();
            _previewReady = null;
        }
    }

    public void SplitPdfByTwoBarcodes(PdfScanDocument inputDocument, string outputFolder, ScanningSettings settings)
    {
        Bitmap bmp = new Bitmap(100, 100);

        // Start preview window (non-blocking) if enabled
        if (settings.PreviewWindow.Enabled)
        {
            StartPreviewWindow(settings.PreviewWindow.Width, settings.PreviewWindow.Height, "Preview");
        }

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

                if (settings.PreviewWindow.Enabled)
                {
                    UpdatePreviewImage(bmp);
                }

                BitmapLuminanceSource ls = new BitmapLuminanceSource(bmp);

                var results = barcodeReader.DecodeMultiple(ls);

                if (results != null && results.Length > 0)
                {
                    if (!string.IsNullOrEmpty(outputDocument.OutputFile))
                    {
                        SavePdfFireAndForget(inputDocument.Content, outputDocument);
                        outputDocument = new PdfScanDocument();
                        outputDocument.startPage = page.PageNumber;
                    }
                    string barcode1 = SanitizeFileName(results[0].Text);
                    string barcode2 = SanitizeFileName("Dummy"); // or results[1].Text

                    outputDocument.OutputFile = System.IO.Path.Combine(outputFolder, $"{barcode2}.{barcode1}.pdf");
                }

                outputDocument.endPage = page.PageNumber;
                outputDocument.Pages.Add(page);

                // Dispose or reuse bmp after decoding; keep last instance alive for preview cloning
                bmp.Dispose();
            }

            SavePdf(inputDocument.Content, outputDocument);
        }
        finally
        {
            if (settings.PreviewWindow.Enabled)
                StopPreviewWindow();
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