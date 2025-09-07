using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using Sassa.BRM.Models;
using Sassa.Services.Cs;
using Sassa.Services.CsDocuments;
using System.Data;
using System.Reflection.Metadata;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;

namespace Sassa.Services
{
    public class CsUploadService
    {
        private readonly CsServiceSettings _settings;
        private readonly ILogger<CSService> _logger;
        //private  DocumentManagementClient _docClient;
        //private  AuthenticationClient _authClient;
        EndpointAddress _authEndpointAddress;
        EndpointAddress _docEndpointAddress;
        public long NodeId { get; private set; }
        //private string _idNumber = "";

        public CsUploadService(CsServiceSettings config, ILogger<CSService> logger)
        {
            _settings = config ?? throw new ArgumentNullException(nameof(config));
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

        public async Task UploadGrantDoc(string csNode, string filePath)
        {
            try
            {
                using (DocumentManagementClient _docClient = new DocumentManagementClient {
                Endpoint = { Address = _docEndpointAddress } })
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
                    string rootName = _settings.CsBeneficiaryRoot;
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
                throw new Exception(ex.Message);
            }
        }
    }
}
