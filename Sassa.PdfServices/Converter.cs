using iText.Html2pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.StyledXmlParser.Css.Media;



namespace Sassa.PdfServices
{
    public static class Converters
    {
        public static void ToReportFolder(string htmlString, string fileName, string folder)
        {

            using (FileStream fs = new FileStream(folder + fileName + ".pdf", FileMode.Create))
            {
                // Create PdfWriter
                PdfWriter writer = new PdfWriter(fs, new WriterProperties().SetFullCompressionMode(true));

                // Create PdfDocument
                PdfDocument pdf = new PdfDocument(writer);
                pdf.SetDefaultPageSize(PageSize.A4);
                // Configure ConverterProperties for better fitting
                ConverterProperties properties = new ConverterProperties();
                // Set media device description to simulate A4 rendering
                var mediaDevice = new MediaDeviceDescription(MediaType.PRINT);
                mediaDevice.SetWidth(PageSize.A4.GetWidth());
                mediaDevice.SetHeight(PageSize.A4.GetHeight());

                properties.SetMediaDeviceDescription(mediaDevice);

                // Convert HTML to PDF
                HtmlConverter.ConvertToPdf(htmlString, writer, properties);

                // Close the document
                // pdf.Close();
            }

        }
    }
}
