using iText.Html2pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.StyledXmlParser.Css.Media;

namespace Sassa.Services
{
    public static class PdfService
    {
        public static void ToPdfFile(string htmlString, string fileName, string folder)
        {

            using (FileStream fs = new FileStream(folder + fileName + ".pdf", FileMode.Create))
            {
                // Create PdfWriter
                PdfWriter writer = new PdfWriter(fs, new WriterProperties().SetFullCompressionMode(true));
                // Create PdfDocument
                PdfDocument pdf = new PdfDocument(writer);
                pdf.SetDefaultPageSize(PageSize.A4);
                // Configure ConverterProperties for better fit
                ConverterProperties properties = new ConverterProperties();
                var mediaDevice = new MediaDeviceDescription(MediaType.PRINT);
                mediaDevice.SetWidth(PageSize.A4.GetWidth());
                mediaDevice.SetHeight(PageSize.A4.GetHeight());
                properties.SetMediaDeviceDescription(mediaDevice);
                // Convert HTML to PDF
                HtmlConverter.ConvertToPdf(htmlString, writer, properties);
            }

        }
    }
}
