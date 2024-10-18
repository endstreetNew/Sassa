namespace Sassa.Audit.Services
{
    public class FileService
    {
        private string connectionString = string.Empty;
        private string reportFolder;

        public FileService(IConfiguration config, IWebHostEnvironment env)
        {
            connectionString = config.GetConnectionString("BrmConnection")!;
            reportFolder = Path.Combine(env.WebRootPath, Path.Combine("brmfiles"))!;
        }

        public void PageFromTemplate(string html, string pdfFile)
        {
            string filename = reportFolder + "\\" + pdfFile + ".html";
            //var pdf = new HtmlToPDF();
            //var buffer = pdf.ReturnPDF(html);
            //if (File.Exists(pdfFile)) File.Delete(filename);
            File.WriteAllTextAsync(filename, html);

        }
    }
}


