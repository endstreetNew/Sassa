using Docnet.Core.Models;
using iText.Kernel.Geom;

namespace Sassa.Scanning.Models
{
    public class PdfScanDocument
    {
        public PdfScanDocument()
        {
            Content = Array.Empty<byte>();
            OutputFile = string.Empty;
        }
        public int startPage { get; set; }
        public int endPage { get; set; }
        public byte[] Content { get; set; }
        public string OutputFile { get; set; }
        public List<PdfScanPage> Pages { get; set; } = new();
    }
    public class PdfScanPage
    {
        public PdfScanPage()
        {
            ps = new iText.Kernel.Geom.Rectangle(0, 0);
            Content = Array.Empty<byte>();
            PageSize = new PageSize(0, 0);
        }
        public byte[] Content { get; set; }
        public string[] barcodes { get; set; } = Array.Empty<string>();
        public PageSize PageSize { get; set; }
        public int PageNumber { get; set; }
        public PageDimensions pageDimensions { get; set; }
        public iText.Kernel.Geom.Rectangle ps { get; set; }

    }
}
