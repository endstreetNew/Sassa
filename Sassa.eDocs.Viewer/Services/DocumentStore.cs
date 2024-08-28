using Microsoft.AspNetCore.Components.Authorization;
using Sassa.eDocs.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sassa.eDocs.Viewer.Services
{
    public class DocumentStore
    {
        private readonly API Api;

        public DocumentStore(IHttpClientFactory clientFactory)
        {
            Api = new API(clientFactory.CreateClient("DocumentService"));
        }

        public async Task<IEnumerable<Document>> GetRequiredDocuments(string reference, AuthenticationState state)
        {
            string apicall = $"api/Documents/ref/{reference}";
            //fetch for this ref
            return await Api.GetResult<IEnumerable<Document>>(apicall).ConfigureAwait(false);
        }
        public async Task<IEnumerable<Document>> GetRequiredDocuments(string reference)
        {
            string apicall = $"api/Documents/ref/{reference}";
            //fetch for this ref
            return await Api.GetResult<IEnumerable<Document>>(apicall).ConfigureAwait(false);
        }
        public async Task PurgeDocuments(string reference)
        {
            string apicall = $"api/Documents/purge/{reference}";
            await Api.PutRequest(apicall).ConfigureAwait(false);
        }
        public async Task RemoveDocument(int docid)
        {
            string apicall = $"api/Documents/{docid}";
            await Api.Delete(apicall).ConfigureAwait(false);
        }
        public async Task<IEnumerable<DocumentType>> GetDocumentTypes()
        {
            string apicall = "api/DocumentTypes";
            return await Api.GetResult<IEnumerable<DocumentType>>(apicall).ConfigureAwait(false);
        }

        public async Task<string> GetDocumentTypes(int documentTypeId)
        {
            string apicall = $"api/DocumentTypes/{documentTypeId}";
            var dt = await Api.GetResult<DocumentType>(apicall).ConfigureAwait(false);
            return dt.Name;
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
        public async Task<IEnumerable<RejectReason>> GetRejectReasons()
        {
            string apicall = $"api/RejectReasons";
            return await Api.GetResult<IEnumerable<RejectReason>>(apicall).ConfigureAwait(false);
        }

        public async Task<Document> PostSocpenDocument(Document spd)
        {
            string apicall = "api/Documents";
            //Add Document 
            spd = await Api.PostRequest<Document>(apicall, spd).ConfigureAwait(false);
            return spd;
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
        public async Task PutDocumentType(int docid, string type)
        {
            string apicall = $"api/Documents/type/{docid}/{type}";

            await Api.PutRequest(apicall).ConfigureAwait(false);
        }

        public async Task PutDocumentRejectReason(int docid, string rejectReason)
        {
            string apicall = $"api/Documents/reject/{docid}/{rejectReason}";
            await Api.PutRequest(apicall).ConfigureAwait(false);
        }

        public async Task<RejectHistory> PostRejectHistory(RejectHistory history)
        {
            string apicall = "api/RejectHistories";
            //Add Document 
            history = await Api.PostRequest<RejectHistory>(apicall, history).ConfigureAwait(false);
            return history;
        }

        public async Task<IEnumerable<RejectHistory>> GetRejectHistory(string reference)
        {
            string apicall = $"api/RejectHistories/ref/{reference}";
            return await Api.GetResult<IEnumerable<RejectHistory>>(apicall).ConfigureAwait(false);
        }
        public async Task<DocImage> GetDocImage(int documentId)
        {
            string apicall = $"api/DocImages/doc/{documentId}";
            return await Api.GetResult<DocImage>(apicall);
        }

        public async Task<byte[]> ToByteArray(Stream stream)
        {

            const int MaxFileSize = 4 * 1024 * 1024;
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
