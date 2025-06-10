using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using Sassa.BRM.Models;
using Sassa.Services.Cs;
using Sassa.Services.CsDocuments;
using System.Data;

namespace Sassa.Services
{
    public class CSService
    {
        CsServiceSettings _settings = new CsServiceSettings();
        private readonly ModelContext _context;
        private readonly DocumentManagementClient _docClient;
        private readonly AuthenticationClient _authClient;
        private CsDocuments.OTAuthentication? _ota;
        private ILogger<CSService> _logger;
        public long NodeId { get; private set; }
        private string _idNumber = "";

        public CSService(CsServiceSettings config, ModelContext context, ILogger<CSService> logger)
        {
            _settings = config ?? throw new ArgumentNullException(nameof(config));
            _context = context;
            _logger = logger;
            _authClient = new AuthenticationClient
            {
                Endpoint = { Address = new System.ServiceModel.EndpointAddress(_settings.CsWSEndpoint + "Authentication") }
            };
            _docClient = new DocumentManagementClient
            {
                Endpoint = { Address = new System.ServiceModel.EndpointAddress(_settings.CsWSEndpoint + "DocumentManagement") }
            };
        }

        /// <summary>
        /// CS Webservice authentication
        /// </summary>
        private async Task Authenticate()
        {

            try
            {
                _ota = new Sassa.Services.CsDocuments.OTAuthentication
                {
                    AuthenticationToken = await _authClient.AuthenticateUserAsync(_settings.CsServiceUser, _settings.CsServicePass)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Authenticate Contentserver WS.");
                throw new Exception("Failed to Authenticate Contentserver WS.");
            }
        }

        public async Task GetCSDocuments(string idNumber)
        {
            _idNumber = idNumber;
            try
            {
                await Authenticate();
                // Get the root node for this id
                long? nodeId = await GetNodeIdForIdNumber(idNumber);
                if (nodeId == null) return;

                NodeId = nodeId.Value;

                var result = await _docClient.GetNodesInContainerAsync(_ota, NodeId, new GetNodesInContainerOptions { MaxDepth = 1, MaxResults = 10 });
                var nodes = result.GetNodesInContainerResult;
                if (nodes == null) return;

                //SaveFolder("/", NodeId);

                foreach (var node in nodes)
                {
                    await AddRecursive(node, NodeId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
            finally
            {
                _ota = null;
            }
        }

        private async Task<long?> GetNodeIdForIdNumber(string idNumber)
        {
            using var con = new OracleConnection(_settings.CsConnection);
            await con.OpenAsync();
            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandTimeout = 0;
            cmd.FetchSize *= 8;

            cmd.CommandText = $"select DATAID from dtree where name=:prefix and parentid = 47634";
            cmd.Parameters.Add(new OracleParameter("prefix", idNumber.Substring(0, 4)));
            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;
            long periodId = reader.GetInt64(0);

            cmd.CommandText = $"select DATAID from dtree where name=:id and parentid = :periodId";
            cmd.Parameters.Clear();
            cmd.Parameters.Add(new OracleParameter("id", idNumber));
            cmd.Parameters.Add(new OracleParameter("periodId", periodId));
            using var reader2 = await cmd.ExecuteReaderAsync();
            if (!await reader2.ReadAsync()) return null;
            return reader2.GetInt64(0);
        }

        private async Task AddRecursive(Node node, long parentNode)
        {
            if (node.IsContainer)
            {
                //SaveFolder(node.Name, node.ID);
                var result = await _docClient.GetNodesInContainerAsync(_ota, node.ID, new GetNodesInContainerOptions { MaxDepth = 1, MaxResults = 10 });
                var subnodes = result.GetNodesInContainerResult;
                if (subnodes == null) return;
                foreach (var snode in subnodes)
                    await AddRecursive(snode, node.ID);
            }
            else if (node.VersionInfo != null)
            {
                var result = await _docClient.GetVersionContentsAsync(_ota, node.ID, node.VersionInfo.VersionNum);
                var doc = result.GetVersionContentsResult;
                SaveAttachment(doc, _idNumber, node.ID, parentNode);
            }
        }

        private void SaveAttachment(Attachment doc, string idNo, long nodeId, long parentNode)
        {
            //if (!_context.DcDocumentImages.Where(d => d.Filename == doc.FileName).ToList().Any())
            //{
            //    var image = new DcDocumentImage
            //    {
            //        Filename = doc.FileName,
            //        IdNo = idNo,
            //        Image = doc.Contents,
            //        Url = $"../CsImages/{doc.FileName}",
            //        Csnode = nodeId,
            //        Type = true,
            //        Parentnode = parentNode
            //    };
            //    _context.DcDocumentImages.Add(image);
            //    _context.SaveChanges();
            //}
            var filePath = Path.Combine(_settings.CsDocFolder, doc.FileName);
            if (File.Exists(filePath)) return;
            File.WriteAllBytes(filePath, doc.Contents);
        }

        //Redundant
        private void SaveFolder(string folderName, long nodeId)
        {
            if (!_context.DcDocumentImages.Where(d => d.Filename == folderName && d.IdNo == _idNumber).ToList().Any())
            {
                var image = new DcDocumentImage
                {
                    Filename = folderName,
                    IdNo = _idNumber,
                    Image = null,
                    Url = $"../CsImages",
                    Csnode = nodeId,
                    Type = false
                };
                _context.DcDocumentImages.Add(image);
                _context.SaveChanges();
            }
        }

        public Dictionary<string, string> GetFolderList(string idNumber)
        {
            //Todo: try this code again whe Oracle fixed the boolean error..
            //return _context.DcDocumentImages
            //    .Where(d => d.IdNo == idNumber && !(bool)d.Type && d.Csnode != null)
            //    .ToDictionary(d => d.Csnode.ToString()!, d => d.Filename);
            Dictionary<string, string> folders = new Dictionary<string, string>();

            var DocumentList = _context.DcDocumentImages.Where(d => d.IdNo == idNumber).ToList();
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
            return _context.DcDocumentImages.Where(d => d.Parentnode == parentNode).ToList();
        }

        public async Task UploadHealthDoc(string html)
        {
            byte[] content = System.Text.Encoding.UTF8.GetBytes(html);
            string fileName = $"AssetStatus-{DateTime.Now.ToShortDateString().Trim()}.html";
            var attachment = new Attachment
            {
                FileName = fileName,
                Contents = content,
                CreatedDate = DateTime.Now,
                FileSize = content.Length
            };
            try
            {
                await Authenticate();
                NodeId = 94643845; // Health Checks - BRM node id
                await _docClient.CreateDocumentAsync(_ota, NodeId, fileName, "BRM Service", false, new Metadata(), attachment);
            }
            catch (Exception ex)
            {
                _ota = null;
                throw new Exception(ex.Message);
            }

        }
    }
}