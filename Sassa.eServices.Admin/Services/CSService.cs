using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using Sassa.BRM.Data;
using Sassa.BRM.Models;
using Sassa.eDocs.CS;
using Sassa.eDocs.CSDocuments;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sassa.eServices.Admin.Services
{
    public class CSService
    {
        ModelContext _context;
        private string connectionString;
        //private List<DcDocumentImage> DocumentList;
        private DataTable dt;// = new DataTable();

        private string username;
        private string password;
        private Sassa.eDocs.CSDocuments.OTAuthentication ota;
        public long NodeId;

        private DocumentManagementClient docClient;
        string idNumber;
        string imagePath;

        private readonly IConfiguration _config;

        public CSService(IConfiguration config, ModelContext context, IWebHostEnvironment _env)
        {
            _config = config;
            username = _config.GetValue<string>("ContentServer:CSServiceUser");
            password = _config.GetValue<string>("ContentServer:CSServicePass");
            connectionString = _config.GetConnectionString("CsConnection");
            _context = context;
            imagePath = $"{_env.WebRootPath}\\{config.GetValue<string>("Folders:CS")}\\";
            //new System.ServiceModel.EndpointAddress("http://ssvsprdsphc01.sassa.local:18080/cws/services/Authentication");
        }
        /// <summary>
        /// CS Webservice authentication
        /// </summary>
        private async Task Authenticate()
        {
            AuthenticationClient authClient = new AuthenticationClient();
            try
            {
                ota = new Sassa.eDocs.CSDocuments.OTAuthentication();
                //authClient.Endpoint = new System.ServiceModel.EndpointAddress("http://ssvsprdsphc01.sassa.local:18080/cws/services/Authentication");
                ota.AuthenticationToken = await authClient.AuthenticateUserAsync(username, password);
            }
            catch//(Exception ex)
            {
                throw new Exception("Failed to Authenticate Contentserver WS.");
            }
            finally
            {
                await authClient.CloseAsync();
            }
        }
         
        public async Task GetCSDocuments(string _idNumber)
        {
            idNumber = _idNumber;

            if (ota == null)
            {
                await Authenticate();
            }

            //DocumentList = new List<DC_DOCUMENT_IMAGE>();
            DataTable tmp;
            //Get the root node for this id
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                OracleCommand cmd = con.CreateCommand();
                cmd.BindByName = true;
                cmd.CommandTimeout = 0;
                cmd.FetchSize *= 8;
                cmd.CommandText = $"select DATAID from dtree where name='{idNumber.Substring(0, 4)}' and parentid = 47634";
                con.Open();
                tmp = GetResult(cmd);
                if (tmp.Rows.Count == 0) return;
                long PeriodId = long.Parse(tmp.Rows[0].ItemArray[0].ToString());
                cmd.CommandText = $"select DATAID from dtree where name='{idNumber}' and parentid = {PeriodId}";
                tmp = GetResult(cmd);
                if (tmp.Rows.Count == 0) return;
                NodeId = long.Parse(tmp.Rows[0].ItemArray[0].ToString());
            }
            docClient = new DocumentManagementClient();
            try
            {
                //docClient.Endpoint.Binding.SendTimeout = new TimeSpan(0, 1, 30);
                var result = await docClient.GetNodesInContainerAsync(ota, NodeId, new GetNodesInContainerOptions() { MaxDepth = 1, MaxResults = 10 });
                Node[] nodes = result.GetNodesInContainerResult;
                if (nodes == null) return;
                //Save the root folder
                SaveFolder("/", NodeId);
                //Add the nodes
                foreach (Node node in nodes)
                {
                    await AddRecursive(node, NodeId);
                }
            }
            catch(Exception ex)
            {
                //StaticD.WriteEvent(ex.Message);
                ota = null;
                throw new Exception($"An error occurred accessing ContentServer {Environment.NewLine + ex.Message}");
            }

        }

        private async Task AddRecursive(Node node, long parentNode)
        {
            Attachment doc;
            //Go one level  deeeeeper if necesary
            if (node.IsContainer)
            {
                SaveFolder(node.Name, node.ID);
                var result = await docClient.GetNodesInContainerAsync(ota, node.ID, new GetNodesInContainerOptions() { MaxDepth = 1, MaxResults = 10 });
                Node[] subnodes = result.GetNodesInContainerResult;
                foreach (Node snode in subnodes)
                {
                    await AddRecursive(snode, node.ID);
                }

            }
            else
            {
                if (node.VersionInfo != null)
                {
                    var result = await docClient.GetVersionContentsAsync(ota, node.ID, node.VersionInfo.VersionNum);
                    doc = result.GetVersionContentsResult;
                    //if (doc.FileName.EndsWith("jp2")) return;
                    SaveAttachment(doc, idNumber, imagePath, node.ID, parentNode);
                }
            }

        }
        private DataTable GetResult(OracleCommand cmd)
        {
            dt = new DataTable();
            using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
            {
                adapter.Fill(dt);
            }
            return dt;
        }
        private void SaveAttachment(Attachment doc, string IdNo, string imagePath, long nodeId, long parentNode)
        {

            if (!_context.DcDocumentImages.Where(d => d.Filename == doc.FileName).Any()) //skip if its downloaded already
            {
                DcDocumentImage image = new DcDocumentImage();
                image.Filename = doc.FileName;
                image.IdNo = IdNo;
                image.Image = doc.Contents;
                image.Url = $"../DocImages/{doc.FileName}";
                image.Csnode = nodeId;
                image.Type = true;
                image.Parentnode = parentNode;

                _context.DcDocumentImages.Add(image);
                _context.SaveChanges();
            }
            if (File.Exists(imagePath + doc.FileName)) return; //Only add new files to the folder.
            using (FileStream fs = new FileStream(imagePath + doc.FileName, FileMode.Create))
            {
                fs.Write(doc.Contents, 0, doc.Contents.Length);
            }


        }

        private void SaveFolder(string folderName, long nodeId)
        {

            if (!_context.DcDocumentImages.Where(d => d.Filename == folderName && d.IdNo == idNumber).Any()) //skip if folder exists
            {
                DcDocumentImage image = new DcDocumentImage();
                image.Filename = folderName;
                image.IdNo = idNumber;
                image.Image = null;// doc.Contents;
                image.Url = $"../DocImages";
                image.Csnode = nodeId;
                image.Type = false;

                _context.DcDocumentImages.Add(image);
                _context.SaveChanges();
            }

        }

        //public Dictionary<string, string> GetFolderList(string idNumber)
        //{
        //    Dictionary<string, string> folders = new Dictionary<string, string>();

        //    DocumentList = _context.DcDocumentImages.Where(d => d.IdNo == idNumber && d.Type == false).ToList();
        //    foreach (DcDocumentImage doc in DocumentList)
        //    {
        //        folders.Add(doc.Csnode.ToString(), doc.Filename);
        //    }

        //    return folders;
        //}
        //public List<DcDocumentImage> GetDocumentList(string parentId)
        //{
        //    if (string.IsNullOrEmpty(parentId)) return new List<DcDocumentImage>();
        //    long parentNode = long.Parse(parentId);

        //    DocumentList = _context.DcDocumentImages.Where(d => d.Parentnode == parentNode).ToList();

        //    return DocumentList;
        //}

        public async Task UploadDoc(String filename, string filepath)
        {

            if (ota == null)
            {
                await Authenticate();
            }

            Attachment attachment = new Attachment();
            attachment.FileName = filename;
            attachment.Contents = await File.ReadAllBytesAsync(filepath);
            attachment.FileSize = attachment.Contents.Length;
            attachment.CreatedDate = DateTime.Now;

            DocumentManagementClient docClient = new DocumentManagementClient();
            docClient.Endpoint.Binding.SendTimeout = new TimeSpan(0, 3, 0);
            try
            {
                //NodeId = select DATAID from dtree where name='Health Checks - BRM'
                await docClient.CreateDocumentAsync(ota, 94643845, attachment.FileName, "Asset Status Service", false, new Metadata(), attachment);
            }
            catch //(Exception ex)
            {
                throw;
            }
            finally
            {
                await docClient.CloseAsync();
            }
        }
    }
}
