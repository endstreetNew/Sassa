using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sassa.eServices.Admin.Services;
using Sassa.Monitor.Shared;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace Sassa.eServices.Admin.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AssetController : ControllerBase
    {

        private static List<Asset> _Assets = new List<Asset>();

        private CSService _csservice;

        private IWebHostEnvironment _env;
        private readonly ILogger<AssetController> _logger;

        public AssetController(IWebHostEnvironment env,ILogger<AssetController> logger, CSService csservice)
        {
            _logger = logger;
            _env = env;
            _csservice = csservice;
        }

        [HttpGet]
        public async Task<IEnumerable<Asset>> Get()
        {
            if (_Assets.Count > 0) return _Assets;
            _Assets.Add(new Asset {Id=1, AssetType = "WebApp", AssetName = "BRM Production", Connector = "http://brm_prod.sassa.local/brmprod/Unauthorized.aspx", Username = "JSmithA", Password = "Savitri0!", Status = "Unkwown" });
            _Assets.Add(new Asset { Id = 2, AssetType = "WebApp", AssetName = "BRM QA", Connector = "http://ssvsbrmapphc02.sassa.local/QA/Unauthorized.aspx", Username = "JSmithA", Password = "Savitri0!", Status = "Unkwown" });
            _Assets.Add(new Asset { Id = 3, AssetType = "API", AssetName = "Sassa.eUsers.Api (.30)", Connector = "http://10.124.154.30:8085/index.html", Username = "", Password = "", Status = "Unkwown" });
            _Assets.Add(new Asset { Id = 4, AssetType = "API", AssetName = "Sassa.eDocs.Api (.30)", Connector = "http://10.124.154.30:8088/index.html", Username = "", Password = "", Status = "Unkwown" });
            _Assets.Add(new Asset { Id = 5, AssetType = "API", AssetName = "Sassa.eUsers.Api (.29)", Connector = "http://10.124.154.29:8085/index.html", Username = "", Password = "", Status = "Unkwown" });
            _Assets.Add(new Asset { Id = 9, AssetType = "API", AssetName = "Sassa.eDocs.Api (.29)", Connector = "http://10.124.154.29:8088/index.html", Username = "", Password = "", Status = "Unkwown" });
            _Assets.Add(new Asset { Id = 6, AssetType = "API", AssetName = "Sassa.eForms.Api (LO)", Connector = "http://10.117.122.198:8080/index.html", Username = "", Password = "", Status = "Unkwown" });
            _Assets.Add(new Asset { Id = 7, AssetType = "API", AssetName = "Sassa.eDocs.Api (LO)", Connector = "http://10.117.122.198:8088/index.html", Username = "", Password = "", Status = "Unkwown" });
            _Assets.Add(new Asset { Id = 8, AssetType = "ORACLEDB", AssetName = "Sassa.BRMProd.DB", Connector = "Data Source = (DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.124.154.20)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ecsbrm))); user id=contentserver; password=Password123;", Username = "", Password = "", Status = "Unkwown" });
            _Assets.Add(new Asset { Id = 9, AssetType = "ORACLEDB", AssetName = "Sassa.BRMProd.DB", Connector = "Data Source = (DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.124.154.20)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ecsbrm))); user id=contentserver; password=Password123;", Username = "", Password = "", Status = "Unkwown" });
            foreach (Asset a in _Assets)
            {
                a.Status = await CheckAsset(a);
            }
           //await _csservice.UploadDoc(GetHtml(_Assets));
            return _Assets;
        }

        public async Task<string>  CheckAsset(Asset asset,int timeoutSeconds = 1)
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
                            //using (OracleConnection con = new OracleConnection(asset.Connector))
                            //{

                            //    OracleCommand command = new OracleCommand("SELECT 1 FROM DUAL", con);
                            //    command.Connection.Open();
                            //    if (command.ExecuteReader().HasRows) return "UP";
                                return "DOWN";
                            //}
                        default:
                            return "INVALID";
                    }

                }
                catch //(Exception ex)
                {
                    return "UNKNOWN";
                }
            }
            return $"TIMEOUT({timeoutSeconds}s.)";
        }

        //private async Task<string> CheckDriveSpace(Asset asset)
        //{
        //    ConnectionOptions connection = new ConnectionOptions();
        //    connection.Username = "User";
        //    connection.Password = "AStrongPassword";
        //    connection.Authority = "ntlmdomain:DOMAINNAME";
        //    connection.EnablePrivileges = true;
        //    connection.Authentication = AuthenticationLevel.Default;
        //    connection.Impersonation = ImpersonationLevel.Impersonate;
        //}
        /// <summary>
        /// Save pdf to contentserver
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>

        /// <summary>
        /// Save pdf to contentserver
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task Put([FromBody] string html)
        {
            //var Renderer = new HtmlToPdf();
            //var PDF = await Renderer.RenderHtmlAsPdfAsync(html);
            string filename = $"AssetStatus- {DateTime.Now.ToShortDateString().Replace("/", "-")} - {DateTime.Now.ToShortTimeString().Replace(":", "H")}.pdf";
            var file = System.IO.Path.Combine(_env.ContentRootPath + "\\pdf\\", filename);
            try
            {
                //PDF.SaveAs(file);
                await _csservice.UploadDoc(filename, file);

            }
            catch//(Exception ex)
            {
                throw;
            }

        }
    }
}
