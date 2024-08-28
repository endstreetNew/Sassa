using SelectPdf;
using Microsoft.AspNetCore.Hosting;
using Oracle.ManagedDataAccess.Client;
using Sassa.Monitor.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sassa.eServices.Admin.Services
{
    public  class AssetService
    {
        IWebHostEnvironment _env;
        CSService _csservice;
        public AssetService(IWebHostEnvironment env, CSService csservice)
        {
            _env = env;
            _csservice = csservice;
        }
        
        public async Task<List<Asset>> GetAssets() 
        {
                List<Asset> _Assets = new List<Asset>();
                // _Assets.Add(new Asset { Id = 1, AssetType = "WebApp", AssetName = "BRM Production", Connector = "http://brm_prod.sassa.local/brmprod/Unauthorized.aspx", Username = "JSmithA", Password = "Savitri0$", Status = "Unkwown" });
                //_Assets.Add(new Asset { Id = 12, AssetType = "WebApp", AssetName = "BRM QA", Connector = "http://ssvsbrmapphc02.sassa.local", Username = "JSmithA", Password = "Savitri0^", Status = "Unkwown" });
                _Assets.Add(new Asset { Id = 4, AssetType = "WebApp", AssetName = "eServicePortal (.30)", Connector = "http://10.124.154.30", Username = "", Password = "", Status = "Unkwown" });
                _Assets.Add(new Asset { Id = 5, AssetType = "WebApp", AssetName = "eServicePortal (.29)", Connector = "http://10.124.154.29", Username = "", Password = "", Status = "Unkwown" });
                _Assets.Add(new Asset { Id = 2, AssetType = "WebService", AssetName = "CSAuthentication", Connector = "http://10.124.154.218/cws/Authentication.svc?wsdl", Username = "lo-upload", Password = "!0-Up10@d", Status = "Unkwown" });
                _Assets.Add(new Asset { Id = 3, AssetType = "WebService", AssetName = "CSDocuments", Connector = "http://10.124.154.218/cws/DocumentManagement.svc?wsdl", Username = "lo-upload", Password = "!0-Up10@d", Status = "Unkwown" });
                _Assets.Add(new Asset { Id = 6, AssetType = "API", AssetName = "Sassa.eUsers.Api (.30)", Connector = "http://10.124.154.30:8085/index.html", Username = "", Password = "", Status = "Unkwown" });
                _Assets.Add(new Asset { Id = 7, AssetType = "API", AssetName = "Sassa.eDocs.Api (.30)", Connector = "http://10.124.154.30:8088/index.html", Username = "", Password = "", Status = "Unkwown" });
                _Assets.Add(new Asset { Id = 8, AssetType = "API", AssetName = "Sassa.eUsers.Api (.29)", Connector = "http://10.124.154.29:8085/index.html", Username = "", Password = "", Status = "Unkwown" });
                _Assets.Add(new Asset { Id = 9, AssetType = "API", AssetName = "Sassa.eDocs.Api (.29)", Connector = "http://10.124.154.29:8088/index.html", Username = "", Password = "", Status = "Unkwown" });
                _Assets.Add(new Asset { Id = 10, AssetType = "API", AssetName = "Sassa.eForms.Api (LO)", Connector = "http://10.117.122.198:8080/index.html", Username = "", Password = "", Status = "Unkwown" });
                _Assets.Add(new Asset { Id = 11, AssetType = "API", AssetName = "Sassa.eDocs.Api (LO)", Connector = "http://10.117.122.198:8088/index.html", Username = "", Password = "", Status = "Unkwown" });
                _Assets.Add(new Asset { Id = 13, AssetType = "ORACLEDB", AssetName = "Sassa.eDocs.DB", Connector = "DATA SOURCE = SSVSCSODBPHC01.SASSA.LOCAL:1521/ecsprod; USER ID = EDOCS; Password = Password123; ", Username = "", Password = "", Status = "Unkwown" });
                _Assets.Add(new Asset { Id = 14, AssetType = "ORACLEDB", AssetName = "Sassa.eUser.DB", Connector = "DATA SOURCE=SSVSCSODBPHC01.SASSA.LOCAL:1521/ecsprod;USER ID=ESERVICES;Password=Password123;", Username = "", Password = "", Status = "Unkwown" });
                _Assets.Add(new Asset { Id = 15, AssetType = "ORACLEDB", AssetName = "Sassa.BRMProd.DB", Connector = "Data Source = (DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.124.154.20)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ecsbrm))); user id=contentserver; password=Password123;", Username = "", Password = "", Status = "Unkwown" });
                _Assets.Add(new Asset { Id = 16, AssetType = "ORACLEDB", AssetName = "Sassa.LOProd.DB", Connector = "DATA SOURCE = 10.124.154.21:1521/ecslo;USER ID=lo_admin;Password=sassa123;", Username = "", Password = "", Status = "Unkwown" });
                _Assets.Add(new Asset { Id = 17, AssetType = "ORACLEDB", AssetName = "Sassa.CSProd.DB", Connector = "Data Source = (DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.124.154.224)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ecsprod))); user id=contentserver; password=Password123;", Username = "", Password = "", Status = "Unkwown" });

                foreach (Asset a in _Assets)
                {
                    a.Status = await CheckAsset(a);
                }
                //await _csservice.UploadDoc(GetHtml(_Assets));
                return _Assets;
        }

        public async Task<string> CheckAsset(Asset asset, int timeoutSeconds = 1)
        {
            DateTime endTime = DateTime.Now.AddSeconds(timeoutSeconds);
            while (DateTime.Now < endTime)
            {
                try
                {
                    switch (asset.AssetType)
                    {
                        case "WebApp":
                        case "WebService":
                        case "API":

                            HttpClient client = new HttpClient();
                            if (asset.Username != "")
                            {
                                var handler = new HttpClientHandler() { UseDefaultCredentials = true };
                                client = new HttpClient(handler);
                            }

                            HttpResponseMessage response = await client.GetAsync(asset.Connector);

                            if (!response.IsSuccessStatusCode)
                            {
                                return "DOWN";
                            }
                            return "UP";
                        //case "DB":
                        //    string connString = asset.Connector;
                        //    string cmdText = "select 1";
                        //    using (OracleConnection sqlConnection = new OracleConnection(connString))
                        //    {
                        //        sqlConnection.Open();
                        //        using (OracleCommand sqlCmd = new OracleCommand(cmdText, sqlConnection))
                        //        {
                        //            int nRet = sqlCmd.ExecuteNonQuery();
                        //            if (nRet <= 0)
                        //            {
                        //                return "DOWN";
                        //            }
                        //            else
                        //            {
                        //                return "UP";
                        //            }
                        //        }
                        //    }
                        case "ORACLEDB":
                            using (OracleConnection con = new OracleConnection(asset.Connector))
                            {

                                OracleCommand command = new OracleCommand("SELECT 1 FROM DUAL", con);
                                command.Connection.Open();
                                if (command.ExecuteReader().HasRows) return "UP";
                                return "DOWN";
                            }
                        default:
                            return "INVALID";
                    }

                }
                catch 
                {
                    return "UNKNOWN";
                }
            }
            return $"TIMEOUT({timeoutSeconds}s.)";
        }

        public async Task SaveCSRepors(string html)
        {
            //var Renderer = new HtmlToPdf();
            //var PDF = await Renderer.RenderHtmlAsPdfAsync(html);
            PdfPageSize pageSize = PdfPageSize.A4;
            PdfPageOrientation pdfOrientation = PdfPageOrientation.Portrait;
            int webPageWidth = 1024;
            int webPageHeight = 0;
            // instantiate a html to pdf converter object
            HtmlToPdf converter = new HtmlToPdf();
            // set converter options
            converter.Options.PdfPageSize = pageSize;
            converter.Options.PdfPageOrientation = pdfOrientation;
            converter.Options.WebPageWidth = webPageWidth;
            converter.Options.WebPageHeight = webPageHeight;

            // create a new pdf document converting an url
            PdfDocument doc = converter.ConvertHtmlString(html, "");

            // save pdf document
            //doc.Save("Sample.pdf");

            // close pdf document
            
            string filename = $"AssetStatus- {DateTime.Now.ToShortDateString().Replace("/", "-")} - {DateTime.Now.ToShortTimeString().Replace(":", "H")}.pdf";
            var file = System.IO.Path.Combine(_env.WebRootPath + "\\pdf\\", filename);
            try
            {
                doc.Save(file);
                doc.Close();
                await Task.Delay(100);// _csservice.UploadDoc(filename, file);

            }
            catch //(Exception ex)
            {
                throw;
            }

        }
    }
}
