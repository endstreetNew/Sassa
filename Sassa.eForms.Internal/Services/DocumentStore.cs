using Microsoft.AspNetCore.Components.Authorization;
using Sassa.eDocs.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Sassa.eForms.Models;
using Sassa.eForms.Internal.Extentions;
using Microsoft.AspNetCore.Http;
using System.Security.Principal;
using System.DirectoryServices;

namespace Sassa.eForms.Services
{
    public class DocumentStore
    {
        private readonly API Api;
        private ILOService loservice;
        ProtectedSessionStorage _ps;
        UserSession _session;
        public UserSession session
        {
            get
            {
                return _session;
            }
        }
        IHttpContextAccessor _ctx;
        public DocumentStore(IHttpClientFactory clientFactory, ILOService loService, ProtectedSessionStorage ps, IHttpContextAccessor ctx)
        {
            Api = new API(clientFactory.CreateClient("DocumentService"));
            loservice = loService;
            _ps = ps;
            _ctx = ctx;
            GetUserSession(true);
        }

        public event EventHandler SessionInitialized;
        public async void GetUserSession(bool force)
        {
            try
            {
                var username = _ctx.HttpContext.User.Identity.Name.Substring(6);
                var result = await _ps.GetAsync<UserSession>(username);
                if (result.Success && !force)
                {
                    _session = (UserSession)result.Value;
                }
                else
                {
                    _session = new UserSession();
                    try
                    {
                        _session = new UserSession();
                        _session.SamName = username;
                        var identity = (WindowsIdentity)_ctx.HttpContext.User.Identity;
                        var user = new DirectoryEntry($"LDAP://<SID={identity.User.Value}>");
                        _session.Name = (string)user.Properties["name"].Value;
                        _session.Surname = (string)user.Properties["sn"].Value;
                        _session.Roles = _ctx.HttpContext.User.GetRoles();

                        await _ps.SetAsync(_session.SamName, _session);
                    }
                    catch
                    {
                        throw;
                    }
                }
                if (SessionInitialized != null) SessionInitialized(this, null);
            }
            catch// (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Document>> GetRequiredDocuments(string reference, AuthenticationState state)
        {
            try
            {
          
                //Get LO List
                var required = loservice.GetRequiredDocuments(reference);
                if (!required.Any()) throw new Exception("Empty requirement list returned from LO");

                //Delete redundant Documents
                await DeleteUnusedDocuments( required,reference);

                string apicall = "api/Documents";
                //Add to Documents (UPsert)
                foreach (var rd in required)
                {
                    if (rd.LoDocumentTypeId == 123) continue;
                    rd.User = state.User.Identity.Name;
                    rd.InternalDocument = false;
                    rd.DocumentTypeId = await GetDocumentTypeId(rd.LoDocumentTypeId);
                    var u = await Api.PostRequest<Document>(apicall, rd).ConfigureAwait(false);
                }
                apicall = "api/Documents/ref";
                apicall = $"{apicall}/{reference}";
                //fetch for this ref
                return await Api.GetResult<IEnumerable<Document>>(apicall).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                var ss = ex.Message;
                throw;
            }
        }


        private async Task DeleteUnusedDocuments(IEnumerable<Document> rdocuments,string reference)
        {
            string apicall = "api/Documents/ref";
            apicall = $"{apicall}/{reference}";
            List<Document> documents = await Api.GetResult<List<Document>>(apicall).ConfigureAwait(false);
            foreach(var doc in documents)
            {
                //Skip internaldocuments
                if (doc.InternalDocument) continue;
                if (doc.LoDocumentTypeId == 123) continue;
                //is this document required?
                if (rdocuments.Where(e => e.LoDocumentTypeId == doc.LoDocumentTypeId && e.Reference == doc.Reference && e.ChildIdNo == doc.ChildIdNo).Any())
                {
                    //Skip if we only have one 
                    if (documents.Where(d => d.LoDocumentTypeId == doc.LoDocumentTypeId && d.Reference == doc.Reference && d.ChildIdNo == doc.ChildIdNo).Count() == 1)
                    {
                        continue;
                    }
                }
                //Delete
                await DeleteDocument(doc.DocumentId);
            }
        }


        public async Task DeleteInternalDocuments(IEnumerable<Document>  docs)
        {
            foreach(var doc in  docs)
            {
                if (!doc.InternalDocument) continue;
                await DeleteDocument(doc.DocumentId);
            }
        }
        public async Task DeleteDocument(int DocumentId)
        {
            string apicall = "api/Documents";
            apicall = $"{apicall}/{DocumentId}";
            await Api.Delete(apicall);

        }
        public async Task<Document> UpsertAdditionalDocument(Document doc)
        {
            doc.DocumentId = 0;
            doc.InternalDocument = false;
            doc.DocumentTypeId = 53;
            doc.LoDocumentTypeId = 123;
            doc.OtherDocumentType = null;
            doc.Status = "New";
            string apicall = "api/Documents";
            return await Api.PostRequest<Document>(apicall, doc).ConfigureAwait(false);
        }

        public async Task UpdateDocument(Document doc)
        {
            string apicall = $"api/Documents/{doc.DocumentId}";
            await Api.PutRequest<Document>(apicall, doc).ConfigureAwait(false);
        }
        public async Task<IEnumerable<LoDocumentType>> GetLoDocumentTypes()
        {
            string apicall = "api/LoDocumentTypes";
            return await Api.GetResult<IEnumerable<LoDocumentType>>(apicall).ConfigureAwait(false);
        }

        public async Task<string> GetLoDocumentType(int lodocumentTypeId)
        {
            string apicall = $"api/LoDocumentTypes/lo/{lodocumentTypeId}";
            var dt = await Api.GetResult<LoDocumentType>(apicall).ConfigureAwait(false);
            return dt.DisplayName;
        }

        public async Task<Document> GetDocument(int documentId)
        {
            string apicall = $"api/Documents/{documentId}";
            return await Api.GetResult<Document>(apicall).ConfigureAwait(false);
        }

        public async Task<int> GetDocumentTypeId(int lodocumentTypeId)
        {
            string apicall = $"api/LoDocumentTypes/lo/{lodocumentTypeId}";
            var dt = await Api.GetResult<LoDocumentType>(apicall).ConfigureAwait(false);
            return dt.DocumentTypeId;
        }

        public async Task PutImage(DocImage image)
        {
            string apicall = "api/DocImages";
            await Api.PostRequest<DocImage>(apicall, image).ConfigureAwait(false);
        }

        public async Task PutDocumentStatus(int docid, string status)
        {
            string apicall = $"api/Documents/status/{docid}/{status}";

            await Api.PutRequest(apicall).ConfigureAwait(false);
        }


        public async Task PutDocumentRejectReason(int docid, string rejectReason)
        {
            string apicall = $"api/Documents/reject/{docid}/{rejectReason}";

            await Api.PutRequest(apicall).ConfigureAwait(false);
        }

        public async Task<DocImage> GetDocImage(int documentId)
        {
            string apicall = $"api/DocImages/doc/{documentId}";
            return await Api.GetResult<DocImage>(apicall);
        }
        public async Task SetUploaded(string reference)
        {
            string result =  await loservice.SetUploaded(reference);
        }

        public async Task<byte[]> ToByteArray(Stream stream)
        {

            const int MaxFileSize = 10 * 1024 * 1024;
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[MaxFileSize];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = await stream.ReadAsync(readBuffer);
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        public string ApplicationType(int ApplicationTypeId)
        {
            string applicationtype = "";
            switch (ApplicationTypeId)
            {
                case 2:
                    applicationtype = "Old age grant";
                    break;
                case 3:
                    applicationtype = "War veteran grant";
                    break;
                case 4:
                    applicationtype = "Disability grant";
                    break;
                case 5:
                    applicationtype = "Foster child grant";
                    break;
                case 6:
                    applicationtype = "Combination grant";
                    break;
                case 7:
                    applicationtype = "Government institution";
                    break;
                case 8:
                    applicationtype = "Care dependency grant";
                    break;
                case 9:
                    applicationtype = "Child support grant";
                    break;
                case 11:
                    applicationtype = "Social relief of distress grant";
                    break;
                case 12:
                    applicationtype = "State maintenance grant";
                    break;
            }
            return applicationtype;
        }
        public string GetFilename(Document doc, string docTypeName)
        {
            return $"{doc.IdNo}_{doc.Reference}_{doc.DocumentTypeId.ToString().PadLeft(2, '0')} - {docTypeName}_{doc.RegionCode}{DateTime.Now.ToString("yyyy-mm-dd")}_{doc.SupportDocument}.pdf";
        }
    }
}
