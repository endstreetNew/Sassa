
using iText.Html2pdf;
using iText.Kernel.Pdf;
using iText.Kernel.Geom;
using System.IO;
using System.Reflection.Metadata;
using System.Web;
using iText.StyledXmlParser.Css.Media;

namespace Sassa.PdfServices
{
    public static class Converters 
    {
        //public static void ToContentServer(string html, string csName)
        //{
        //    // Convert HTML to PDF using PdfSharp
        //    PdfDocument pdf = PdfGenerator.GeneratePdf(html, PageSize.A4);
        //    pdf.Save(csName);
        //}
        public static void ToReportFolder(string htmlString, string fileName, string folder)
        {

            using (FileStream fs = new FileStream(folder+fileName + ".pdf", FileMode.Create))
            {
                // Create PdfWriter
                PdfWriter writer = new PdfWriter(fs);

                // Create PdfDocument
                PdfDocument pdf = new PdfDocument(writer);
                pdf.SetDefaultPageSize(PageSize.A4);
                // Configure ConverterProperties for better fitting
                ConverterProperties properties = new ConverterProperties();
                // Set media device description to simulate A4 rendering
                var mediaDevice = new  MediaDeviceDescription(MediaType.PRINT);
                mediaDevice.SetWidth(PageSize.A4.GetWidth());
                mediaDevice.SetHeight(PageSize.A4.GetHeight());
                properties.SetMediaDeviceDescription(mediaDevice);
                // Convert HTML to PDF
                HtmlConverter.ConvertToPdf(htmlString, writer,properties);

                // Close the document
               // pdf.Close();
            }

        }   

        //public static void ToDownload(string html, string fileName)
        //{
        //    PdfDocument pdf = PdfGenerator.GeneratePdf(html, PageSize.A4);
        //    pdf.Save(fileName);
        //    //using (MemoryStream ms = new MemoryStream())
        //    //{
        //    //    pdf.Save(ms, false);
        //    //    byte[] pdfBytes = ms.ToArray();
        //    //}
        //}
        //public static void ConvertHtmlToPdf(string htmlContent, string outputPath)
        //{
        //    try
        //    {
        //        // Create a new PDF document
        //        using (PdfDocument document = new PdfDocument())
        //        {
        //            // Create an empty page
        //            PdfPage page = document.AddPage();

        //            // Get an XGraphics object for drawing
        //            using (XGraphics gfx = XGraphics.FromPdfPage(page))
        //            {
        //                // Create HTML renderer
        //                var renderer = new HtmlRenderer(page.Width, page.Height);

        //                // Render HTML to PDF
        //                renderer.RenderHtml(gfx, htmlContent, 0, 0);

        //                // Save the document
        //                document.Save(outputPath);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error converting HTML to PDF: " + ex.Message);
        //    }
        //}

        //// Alternative method using byte array output
        //public static byte[] ConvertHtmlToPdfBytes(string htmlContent)
        //{
        //    try
        //    {
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            using (PdfDocument document = new PdfDocument())
        //            {
        //                PdfPage page = document.AddPage();
        //                using (XGraphics gfx = XGraphics.FromPdfPage(page))
        //                {
        //                    var renderer = new HtmlRenderer(page.Width, page.Height);
        //                    renderer.RenderHtml(gfx, htmlContent, 0, 0);
        //                    document.Save(ms, false);
        //                }
        //            }
        //            return ms.ToArray();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error converting HTML to PDF: " + ex.Message);
        //    }
        //}
    }

    // Usage example
    //public class SampleCode
    //{
    //    public static void Main()
    //    {
    //        string htmlContent = "<html><body><h1>Hello World</h1><p>This is a test PDF conversion.</p></body></html>";

    //        // Save to file
    //        string outputPath = "output.pdf";
    //        HtmlToPdfConverter.ConvertHtmlToPdf(htmlContent, outputPath);

    //        // Or get as bytes
    //        byte[] pdfBytes = HtmlToPdfConverter.ConvertHtmlToPdfBytes(htmlContent);
    //    }
    //}
}
