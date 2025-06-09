using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Models;
using Sassa.eDocs.CS;
using Sassa.eDocs.CSDocuments;
using System.Data;

namespace Sassa.BRM.Services
{
    public class CSService
    {
        ModelContext _context;
        private string connectionString;
        private List<DcDocumentImage> DocumentList = new List<DcDocumentImage>();
        private DataTable dt = new DataTable();

        private string username;
        private string password;
        private string _wsEndpoint; //= "http://ssvsprdsphc01.sassa.local:18080/cws/services/Authentication"; //wrong endpoint, this is a test endpoint, change to production later
        private Sassa.eDocs.CSDocuments.OTAuthentication? ota; //= new Sassa.eDocs.CSDocuments.OTAuthentication();
        public long NodeId;

        private DocumentManagementClient docClient = new DocumentManagementClient();
        string idNumber = "";

        public CSService(IConfiguration config, ModelContext context, IWebHostEnvironment _env)
        {

            username = config.GetValue<string>("ContentServer:CSServiceUser")!;
            password = config.GetValue<string>("ContentServer:CSServicePass")!;
            connectionString = config.GetConnectionString("CsConnection")!;
            _context = context;
            _wsEndpoint = config.GetValue<string>("ContentServer:CSWSEndpoint")!;
            docClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(_wsEndpoint + "DocumentManagement"); ;
        }
        /// <summary>
        /// CS Webservice authentication
        /// </summary>
        private async Task Authenticate()
        {
            AuthenticationClient authClient = new AuthenticationClient();
            var endpointAddress = new System.ServiceModel.EndpointAddress(_wsEndpoint + "Authentication");
            authClient.Endpoint.Address = endpointAddress; 
            try
            {
                ota = new Sassa.eDocs.CSDocuments.OTAuthentication();
                ota.AuthenticationToken = await authClient.AuthenticateUserAsync(username, password);
            }
            catch
            {
                throw new Exception("Failed to Authenticate Contentserver WS.");
            }
            finally
            {
                await authClient.CloseAsync();
            }
        }

        public async Task Authenticate(string userName, string passWord)
        {
            username = userName;
            password = passWord;
            await Authenticate();
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
                long PeriodId = long.Parse(tmp.Rows[0].ItemArray[0]!.ToString()!);
                cmd.CommandText = $"select DATAID from dtree where name='{idNumber}' and parentid = {PeriodId}";
                tmp = GetResult(cmd);
                if (tmp.Rows.Count == 0) return;
                NodeId = long.Parse(tmp.Rows[0].ItemArray[0]!.ToString()!);
            }

            try
            {
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
            catch (Exception ex)
            {
                StaticDataService.WriteEvent(ex.Message);
                ota = null;
                throw;// new Exception("An error occurred accessing ContentServer");
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
                if (subnodes == null) return;
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
                    SaveAttachment(doc, idNumber, StaticDataService.DocumentFolder!, node.ID, parentNode);
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
        private void SaveAttachment(eDocs.CSDocuments.Attachment doc, string IdNo, string imagePath, long nodeId, long parentNode)
        {

            if (!_context.DcDocumentImages.Where(d => d.Filename == doc.FileName).ToList().Any()) //skip if its downloaded already
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

            if (!_context.DcDocumentImages.Where(d => d.Filename == folderName && d.IdNo == idNumber).ToList().Any()) //skip if folder exists
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

        public Dictionary<string, string> GetFolderList(string idNumber)
        {
            Dictionary<string, string> folders = new Dictionary<string, string>();

            DocumentList = _context.DcDocumentImages.Where(d => d.IdNo == idNumber).ToList();
            if (!DocumentList.Any()) return folders;
            foreach (DcDocumentImage doc in DocumentList)
            {
                if (doc.Type != null && doc.Csnode != null && !(bool)doc.Type)
                {
                    folders.Add(doc.Csnode.ToString()!, doc.Filename);
                }
            }

            return folders;
        }
        public List<DcDocumentImage> GetDocumentList(string parentId)
        {
            if (string.IsNullOrEmpty(parentId)) return new List<DcDocumentImage>();
            long parentNode = long.Parse(parentId);

            DocumentList = _context.DcDocumentImages.Where(d => d.Parentnode == parentNode).ToList();

            return DocumentList;
        }

        public async Task UploadHealthDoc(string fileName, string html)
        {
            byte[] content = System.Text.Encoding.UTF8.GetBytes(html);
            Attachment attachment = new eDocs.CSDocuments.Attachment()
            {
                FileName = fileName,
                Contents = content,
                CreatedDate = DateTime.Now,
                FileSize = content.Length
            };
            if (ota == null)
            {
                await Authenticate();
            }


            //https://edrms.sassa.gov.za/otcs/cs.exe?func=ll&objId=20241506&objAction=browse&viewType=1
            NodeId = 94643845; //Health Checks - BRM node id, this is a static value for now, can be changed later to be dynamic
            try
            {
                await docClient.CreateDocumentAsync(ota, NodeId, fileName, "BRM Service", false, new Metadata(), attachment);
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                await docClient.CloseAsync();
            }
        }
        //        public async Task UploadDoc(Document doc, Attachment attachment)
        //        { 
        //            if (ota == null)
        //			{
        //				await Authenticate();
        //			}

        //			DocumentManagementClient docClient = new DocumentManagementClient();

        //            docClient.Endpoint.Binding.SendTimeout = new TimeSpan(0, 3, 0);
        //			try
        //			{
        //				if(NodeId == 0)
        //                {
        //					//find node
        //					var response = await docClient.GetNodeByNameAsync(ota, 2000, "12. Beneficiaries");
        //                    response = await docClient.GetNodeByNameAsync(ota, response.GetNodeByNameResult.ID, doc.IdNo.Substring(0, 4));
        //					response = await docClient.GetNodeByNameAsync(ota, response.GetNodeByNameResult.ID, doc.IdNo);
        //                    response = await docClient.GetNodeByNameAsync(ota, response.GetNodeByNameResult.ID, doc.CSNode);
        //                    NodeId = response.GetNodeByNameResult.ID;
        //				}


        //                await docClient.CreateDocumentAsync(ota, NodeId, attachment.FileName, "eDocs Service", false, new Metadata(), attachment);
        //                await _dstore.PutDocumentStatus(doc.DocumentId, "Processed");

        //			}
        //			catch// (Exception ex)
        //			{
        //    throw;
        //}
        //			finally
        //			{
        //    await docClient.CloseAsync();
        //}
        //        }
    }
}
