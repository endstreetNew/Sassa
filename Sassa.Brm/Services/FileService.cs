//using HtmlToPDFCore;
namespace Sassa.BRM.Services
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

        //public async Task SaveFile(string uploadFileName)
        //{
        //    string filename = reportFolder + "\\" + uploadFileName;
        //    //    HttpClient client = new HttpClient();
        //    //    string html = "";
        //    //        var handler = new HttpClientHandler() { UseDefaultCredentials = true };
        //    //        client = new HttpClient(handler);

        //    //            using (HttpResponseMessage response = client.GetAsync(url).Result)
        //    //            {
        //    //                using (HttpContent content = response.Content)
        //    //                {
        //    //                     html = content.ReadAsStringAsync().Result;
        //    //                }
        //    //            }

        //    //        var pdf = new HtmlToPDF();
        //    //        var buffer = pdf.ReturnPDF(html);
        //    //        if (File.Exists(pdfFile)) File.Delete(filename);
        //    //        using (var f = new FileStream(filename, FileMode.Create))
        //    //        {
        //    //            f.Write(buffer, 0, buffer.Length);
        //    //            f.Flush();
        //    //            f.Close();
        //    //        }
        //}
        //public async Task SavePage(string url, string pdfFile)
        //{
        //    string filename = reportFolder + "\\" + pdfFile + ".pdf";
        //    HttpClient client = new HttpClient();
        //    string html = "";
        //        var handler = new HttpClientHandler() { UseDefaultCredentials = true };
        //        client = new HttpClient(handler);

        //            using (HttpResponseMessage response = client.GetAsync(url).Result)
        //            {
        //                using (HttpContent content = response.Content)
        //                {
        //                     html = content.ReadAsStringAsync().Result;
        //                }
        //            }

        //        var pdf = new HtmlToPDF();
        //        var buffer = pdf.ReturnPDF(html);
        //        if (File.Exists(pdfFile)) File.Delete(filename);
        //        using (var f = new FileStream(filename, FileMode.Create))
        //        {
        //            f.Write(buffer, 0, buffer.Length);
        //            f.Flush();
        //            f.Close();
        //        }
        //}

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

