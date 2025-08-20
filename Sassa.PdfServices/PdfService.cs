using iText.Html2pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Layout.Element;
using iText.StyledXmlParser.Css.Media;

namespace Sassa.Services
{
    public static class PdfService
    {
        /// <summary>
        /// Brm Save cover sheet to pdf file
        /// </summary>
        /// <param name="htmlString"></param>
        /// <param name="fileName"></param>
        /// <param name="folder"></param>
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
        /// <summary>
        /// Kofax Api
        /// </summary>
        /// <param name="htmlString"></param>
        /// <param name="filePath"></param>
        /// <param name="targetPath"></param>
        public static void AddFileToCover(string htmlString, string filePath, string targetPath)
        {
            try
            {
                byte[] coverBytes;
                using (var coverStream = new MemoryStream())
                {
                    HtmlConverter.ConvertToPdf(htmlString, coverStream);
                    coverBytes = coverStream.ToArray(); // Copy before stream is closed
                }

                using (var twriter = new PdfWriter(targetPath))
                using (var mergedPdf = new PdfDocument(twriter))
                using (var coverPdf = new PdfDocument(new PdfReader(new MemoryStream(coverBytes))))
                using (var pdfDocs = new PdfDocument(new PdfReader(filePath)))
                {
                    var merger = new PdfMerger(mergedPdf);
                    merger.Merge(coverPdf, 1, coverPdf.GetNumberOfPages());
                    merger.Merge(pdfDocs, 1, pdfDocs.GetNumberOfPages());
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
