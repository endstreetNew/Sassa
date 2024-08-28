using Microsoft.Extensions.Configuration;
using Sassa.eDocs.CS;
using Sassa.eDocs.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Sassa.eDocs.CSDocuments;

namespace Sassa.eDocs.Services
{
    public class CSService
    {
		private readonly IConfiguration _config;
		private DocumentStore _dstore;
		private string username;
		private string password;
		private CSDocuments.OTAuthentication ota;
		private long NodeId;

		public CSService(IConfiguration config, DocumentStore dstore)
		{
			_config = config;
			username = _config.GetValue<string>("ContentServer:User");
			password = _config.GetValue<string>("ContentServer:Password");
			_dstore = dstore;
		}
		
		public async Task Run(string reference)
		{
			NodeId = 0;
			//Get the files to be uploaded
			IEnumerable<Document> files = await _dstore.GetRequiredDocuments(reference);
			foreach (Document file in files)
            {
				if (file.Status == "Processed" || file.Status == "New") continue;
				Attachment attachment = new Attachment();
				attachment.Contents = await  LoadFile(file.DocumentId);
				attachment.FileSize = attachment.Contents.Length;
				attachment.FileName = file.FileName;
				attachment.CreatedDate = DateTime.Now;
				attachment.ModifiedDate = DateTime.Now;
				//Upload the Document
				await UploadDoc(file, attachment);
			}

		}

		protected async Task<byte[]> LoadFile(int docid)
		{
			DocImage image = await _dstore.GetDocImage(docid);
			return image.Image;
			//FileStream fls = new FileStream("wwwroot/" + tmpFileName, FileMode.OpenOrCreate, FileAccess.Write);
			//fls.Write(rawData, 0, (int)FileSize);
			//fls.Close();

		}
		public async Task Authenticate()
		{
			AuthenticationClient authClient = new AuthenticationClient();
			try
			{
				ota = new CSDocuments.OTAuthentication();
				ota.AuthenticationToken = await authClient.AuthenticateUserAsync(username, password);
			}
			catch 
			{
				throw;
			}
			finally
			{
				await authClient.CloseAsync();
			}
		}
		public async Task UploadDoc(Document doc,Attachment attachment)
		{
			if (ota == null)
			{
				await Authenticate();
			}

			DocumentManagementClient docClient = new DocumentManagementClient();
			docClient.Endpoint.Binding.SendTimeout = new TimeSpan(0, 3, 0);
			try
			{
				if(NodeId == 0)
                {
					//find node
					var response = await docClient.GetNodeByNameAsync(ota, 2000, "12. Beneficiaries");
					response = await docClient.GetNodeByNameAsync(ota, response.GetNodeByNameResult.ID, doc.IdNo.Substring(0, 4));
					response = await docClient.GetNodeByNameAsync(ota, response.GetNodeByNameResult.ID, doc.IdNo);
					response = await docClient.GetNodeByNameAsync(ota, response.GetNodeByNameResult.ID, doc.CSNode);
					NodeId = response.GetNodeByNameResult.ID;
				}


				await docClient.CreateDocumentAsync(ota, NodeId, attachment.FileName, "eDocs Service", false, new Metadata(), attachment);
				await _dstore.PutDocumentStatus(doc.DocumentId, "Processed");

			}
			catch// (Exception ex)
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
