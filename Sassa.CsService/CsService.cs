using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using Sassa.BRM.Models;
using Sassa.Services.Cs;
using Sassa.Services.CsDocuments;
using System.Data;
using System.ServiceModel;
using System.Text;

namespace Sassa.Services
{
    public class CSService
    {
        private readonly CsServiceSettings _settings;
        private readonly ILogger<CSService> _logger;
        IDbContextFactory<ModelContext> _contextFactory;
        EndpointAddress _authEndpointAddress;
        EndpointAddress _docEndpointAddress;
        public long NodeId { get; private set; }
        private string _idNumber = "";

        public CSService(CsServiceSettings config, IDbContextFactory<ModelContext> contextFactory, ILogger<CSService> logger)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _settings = config ?? throw new ArgumentNullException(nameof(config));
            //_context = context;
            _logger = logger;
            _authEndpointAddress = new System.ServiceModel.EndpointAddress(_settings.CsWSEndpoint + "Authentication");
            _docEndpointAddress = new System.ServiceModel.EndpointAddress(_settings.CsWSEndpoint + "DocumentManagement");
        }


        private async Task<CsDocuments.OTAuthentication> Authenticate()
        {

            try
            {
                AuthenticationClient _authClient = new AuthenticationClient
                {
                    Endpoint = { Address = _authEndpointAddress }
                };
                return new Sassa.Services.CsDocuments.OTAuthentication
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
                using (DocumentManagementClient _docClient = new DocumentManagementClient
                {
                    Endpoint = { Address = _docEndpointAddress }
                })
                {
                    // Get the root node for this id from the db
                    long? nodeId = await GetNodeIdForIdNumber(idNumber);
                    if (nodeId == null) return;

                    NodeId = nodeId.Value;
                    CsDocuments.OTAuthentication _ota = await Authenticate();
                    var result = await _docClient.GetNodesInContainerAsync(_ota, NodeId, new GetNodesInContainerOptions { MaxDepth = 1, MaxResults = 10 });
                    var nodes = result.GetNodesInContainerResult;
                    if (nodes == null) return;

                    //SaveFolder("/", NodeId);

                    foreach (var node in nodes)
                    {
                        await AddRecursive(node, NodeId, _docClient, _ota);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCSDocuments for idNumber: {_idNumber}", _idNumber);
                throw;
            }
        }

        private async Task<long?> GetNodeIdForIdNumber(string idNumber)
        {
            try
            {
                long? result;
                using (var con = new OracleConnection(_settings.CsConnection))
                {
                    if (con.State != ConnectionState.Open)
                    {
                        await con.OpenAsync();
                    }
                    using var cmd = con.CreateCommand();
                    cmd.BindByName = true;
                    cmd.CommandTimeout = 360;
                    cmd.FetchSize *= 8;

                    cmd.CommandText = $"select DATAID from dtree where name=:prefix and parentid = 47634";
                    cmd.Parameters.Add(new OracleParameter("prefix", idNumber.Substring(0, 4)));
                    using var reader = await cmd.ExecuteReaderAsync();
                    //ORA-02396: exceeded maximum idle time, please connect again
                    if (!await reader.ReadAsync()) return null;
                    long periodId = reader.GetInt64(0);

                    cmd.CommandText = $"select DATAID from dtree where name=:id and parentid = :periodId";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new OracleParameter("id", idNumber));
                    cmd.Parameters.Add(new OracleParameter("periodId", periodId));
                    using var reader2 = await cmd.ExecuteReaderAsync();
                    if (!await reader2.ReadAsync()) return null;
                    result = reader2.GetInt64(0);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetNodeIdForIdNumber for idNumber: {_idNumber}", _idNumber);
                return null;
            }
        }

        private async Task AddRecursive(Node node, long parentNode, DocumentManagementClient _docClient, CsDocuments.OTAuthentication _ota)
        {
            try
            {
                if (node.IsContainer)
                {
                    //SaveFolder(node.Name, node.ID);
                    var result = await _docClient.GetNodesInContainerAsync(_ota, node.ID, new GetNodesInContainerOptions { MaxDepth = 1, MaxResults = 10 });
                    var subnodes = result.GetNodesInContainerResult;
                    if (subnodes == null) return;
                    foreach (var snode in subnodes)
                        await AddRecursive(snode, node.ID, _docClient, _ota);
                }
                else if (node.VersionInfo != null)
                {

                    var result = await _docClient.GetVersionContentsAsync(_ota, node.ID, node.VersionInfo.VersionNum);
                    var doc = result.GetVersionContentsResult;
                    SaveAttachment(doc, _idNumber, node.ID, parentNode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddRecursive for nodeId: {NodeId}", node.ID);
            }
        }

        private void SaveAttachment(Attachment doc, string idNo, long nodeId, long parentNode)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {

                //Todo to save document method
                if (!_context.DcDocumentImages.Where(d => d.Filename == doc.FileName).ToList().Any())
                {
                    var image = new DcDocumentImage
                    {
                        Filename = doc.FileName,
                        IdNo = idNo,
                        Image = doc.Contents,
                        Url = $"../CsImages/{doc.FileName}",
                        Csnode = nodeId,
                        Type = true,
                        Parentnode = parentNode
                    };
                    _context.DcDocumentImages.Add(image);
                    _context.SaveChanges();
                }
                var filePath = Path.Combine(_settings.CsDocFolder, doc.FileName);
                if (File.Exists(filePath)) return;
                File.WriteAllBytes(filePath, doc.Contents);
            }
        }

        //Redundant
        private void SaveFolder(string folderName, long nodeId)
        {
            using (var _context = _contextFactory.CreateDbContext())
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
        }

        public Dictionary<string, string> GetFolderList(string idNumber)
        {
            using (var _context = _contextFactory.CreateDbContext())
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
        }

        public List<DcDocumentImage> GetDocumentList(string parentId)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                if (string.IsNullOrEmpty(parentId)) return new List<DcDocumentImage>();
                long parentNode = long.Parse(parentId);
                return _context.DcDocumentImages.Where(d => d.Parentnode == parentNode).ToList();
            }
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
                using (DocumentManagementClient _docClient = new DocumentManagementClient
                {
                    Endpoint = { Address = _docEndpointAddress }
                })
                {
                    await Authenticate();
                    NodeId = 94643845; // Health Checks - BRM node id
                    CsDocuments.OTAuthentication _ota = await Authenticate();
                    await _docClient.CreateDocumentAsync(_ota, NodeId, fileName, "BRM Service", false, new Metadata(), attachment);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task UploadSharedReport(string reportContent, string fileName)
        {
            using (DocumentManagementClient _docClient = new DocumentManagementClient
            {
                Endpoint = { Address = _docEndpointAddress }
            })
            {
                byte[] content = System.Text.Encoding.UTF8.GetBytes(reportContent);
                var attachment = new Attachment
                {
                    FileName = fileName,
                    Contents = content,
                    CreatedDate = DateTime.Now,
                    FileSize = content.Length
                };
                try
                {
                    NodeId = 242046960; //BRM shared Reports node id
                    CsDocuments.OTAuthentication _ota = await Authenticate();
                    await _docClient.CreateDocumentAsync(_ota, NodeId, fileName, "BRM Service", false, new Metadata(), attachment);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

        }

        /// <summary>
        /// Retired Uploads a document to the Content Server under the specified node.
        /// </summary>
        /// <param name="csNode"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task UploadDoc(string csNode, string filePath)
        {


            try
            {
                using (DocumentManagementClient _docClient = new DocumentManagementClient
                {
                    Endpoint = { Address = _docEndpointAddress }
                })
                {
                    Attachment attachment;
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
                    {
                        string fileContent = reader.ReadToEnd();
                        byte[] content = System.Text.Encoding.UTF8.GetBytes(fileContent);
                        attachment = new Attachment
                        {
                            FileName = Path.GetFileName(filePath),
                            Contents = content,
                            CreatedDate = DateTime.Now,
                            FileSize = content.Length
                        };
                    }

                    //docClient.Endpoint.Binding.SendTimeout = new TimeSpan(0, 3, 0);
                    CsDocuments.OTAuthentication ota = await Authenticate();
                    if (NodeId == 0)
                    {
                        //find node
                        //QA
                        //var response = await _docClient.GetNodeAsync(ota, 126638);
                        var response = await _docClient.GetNodeByNameAsync(ota, 2000, "12. Beneficiaries");
                        response = await _docClient.GetNodeByNameAsync(ota, response.GetNodeByNameResult.ID, csNode.Substring(0, 4));
                        response = await _docClient.GetNodeByNameAsync(ota, response.GetNodeByNameResult.ID, csNode.Substring(0, 13));
                        //response = await docClient.GetNodeByNameAsync(ota, response.GetNodeByNameResult.ID, csNode);
                        NodeId = response.GetNodeByNameResult.ID;
                    }


                    await _docClient.CreateDocumentAsync(ota, NodeId, attachment.FileName, "Cs Service", false, new Metadata(), attachment);
                }
            }
            catch (Exception ex)
            {
                _ = ex;
                throw new Exception("Error Uploading to Content server.");
            }
        }

        public async Task UploadGrantDoc(string csNode, string filePath)
        {
            try
            {
                using (DocumentManagementClient _docClient = new DocumentManagementClient
                {
                    Endpoint = { Address = _docEndpointAddress }
                })
                {
                    // Prepare the attachment
                    Attachment attachment;
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
                    {
                        string fileContent = reader.ReadToEnd();
                        byte[] content = System.Text.Encoding.UTF8.GetBytes(fileContent);
                        attachment = new Attachment
                        {
                            FileName = Path.GetFileName(filePath),
                            Contents = content,
                            CreatedDate = DateTime.Now,
                            FileSize = content.Length
                        };
                    }

                    CsDocuments.OTAuthentication ota = await Authenticate();

                    // Node hierarchy: Root (2000) -> "12. Beneficiaries" -> prefix -> id
                    long rootId = 2000;
                    string rootName = "12. Beneficiaries";
                    string prefix = csNode.Substring(0, 4);
                    string id = csNode.Substring(0, 13);

                    // 1. Get or create "12. Beneficiaries"
                    var rootResponse = await _docClient.GetNodeByNameAsync(ota, rootId, rootName);
                    var rootNode = rootResponse.GetNodeByNameResult;
                    if (rootNode == null || rootNode.ID == 0)
                    {
                        var createRootNode = await _docClient.CreateFolderAsync(ota, rootId, rootName, "Auto-created", new Metadata());
                        rootNode = new Sassa.Services.CsDocuments.Node { ID = createRootNode.CreateFolderResult.ID, Name = rootName };
                        //throw new Exception("12. Beneficiaries not found on CS");
                    }

                    // 2. Get or create prefix node
                    var prefixResponse = await _docClient.GetNodeByNameAsync(ota, rootNode.ID, prefix);
                    var prefixNode = prefixResponse.GetNodeByNameResult;
                    if (prefixNode == null || prefixNode.ID == 0)
                    {
                        var createPrefix = await _docClient.CreateFolderAsync(ota, rootNode.ID, prefix, "Auto-created", new Metadata());
                        prefixNode = new Sassa.Services.CsDocuments.Node { ID = createPrefix.CreateFolderResult.ID, Name = prefix };
                    }

                    // 3. Get or create id node
                    var idResponse = await _docClient.GetNodeByNameAsync(ota, prefixNode.ID, id);
                    var idNode = idResponse.GetNodeByNameResult;
                    if (idNode == null || idNode.ID == 0)
                    {
                        var createId = await _docClient.CreateFolderAsync(ota, prefixNode.ID, id, "Auto-created", new Metadata());
                        idNode = new Sassa.Services.CsDocuments.Node { ID = createId.CreateFolderResult.ID, Name = id };
                    }

                    NodeId = idNode.ID;

                    // 4. Upload the document
                    await _docClient.CreateDocumentAsync(ota, NodeId, attachment.FileName, "Cs Service", false, new Metadata(), attachment);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error Uploading to Content server.", ex);
            }
        }

        /// <summary>
        /// CS Webservice authentication
        /// </summary>
        public async Task<bool> CheckService()
        {

            try
            {
                AuthenticationClient _authClient = new AuthenticationClient
                {
                    Endpoint = { Address = _authEndpointAddress }
                };

                var AuthenticationToken = await _authClient.AuthenticateUserAsync(_settings.CsServiceUser, _settings.CsServicePass);
                _authClient.Close();
                return true;


            }
            catch (Exception ex)
            {
                _ = ex;
                return false;
            }
        }

        public async Task<bool> CheckDBConnection()
        {
            try
            {
                using (var con = new OracleConnection(_settings.CsConnection))
                {
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                    }
                    return await Task.FromResult(true);
                }
            }
            catch(Exception ex)
            {
                return await Task.FromResult(false);
            }
        }
    }
}