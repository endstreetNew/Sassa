using Docnet.Core;
using Docnet.Core.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using Tesseract;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;


string pdfPath = @"C:\RawScan\OKdurban_kzn_lo2738.pdf";
//<div id="sizer" style="width: 1133px; height: 193482px;"></div>
using (var docReader = DocLib.Instance.GetDocReader(pdfPath, new PageDimensions(1080, 1920)))
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

    // Now 'bitmap' is a System.Drawing.Bitmap of the first page at 300 DPI
    // You can use it for barcode/QR/OCR processing
    var luminanceSource = new BitmapLuminanceSource(bitmap);
    var reader = new BarcodeReaderGeneric
    {
        AutoRotate = true,
        Options = { TryInverted = true, TryHarder = true }
    };
    var result = reader.Decode(luminanceSource);

    Console.WriteLine("📦 Barcodes found:");
    //foreach (var result in result)
    //{
        Console.WriteLine($"Type: {result.BarcodeFormat}, Value: {result.Text}");
    //}
}
    // Convert rawBytes to Bitmap as needed



//// Load image
//var bitmap = (Bitmap)Image.FromFile(imagePath);


//// Barcode reader setup
//var reader = new BarcodeReaderGeneric
//{
//    AutoRotate = true,
//    Options = { TryInverted = true, TryHarder = true }
//};

//// Convert Bitmap to ZXing luminance source
//// Create the luminance source from the bitmap
//var luminanceSource = new BitmapLuminanceSource(bitmap);

//// Optionally, create a BinaryBitmap for advanced decoding
////var binaryBitmap = new BinaryBitmap(new HybridBinarizer(luminanceSource));

//// Use the reader to decode
//var results = reader.DecodeMultiple(luminanceSource);

////var results = reader.DecodeMultiple(bitmap);
//Console.WriteLine("📦 Barcodes found:");
//foreach (var result in results)
//{
//    Console.WriteLine($"Type: {result.BarcodeFormat}, Value: {result.Text}");
//}

//// OCR setup
//using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
//using var img = Pix.LoadFromFile(imagePath);
//using var page = engine.Process(img);
//string text = page.GetText();

//Console.WriteLine("\n🔍 OCR Text:");
//Console.WriteLine(text.Trim());
