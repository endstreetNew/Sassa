//using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Sassa.eServices.Admin.Services
{
    public class EmailClient : IDisposable
    {
        private bool disposed;
        private SmtpClient client;
        private string _smtpServer;
        private int _port;
        private NetworkCredential _credential;

        public EmailClient(string smptpserver, int port, NetworkCredential credential)
        {
            _smtpServer = smptpserver;
            _port = port;
            _credential = credential;
        }
        public void SendMail(string from, string to, string subject, string body, List<string> attachments)
        {
            client = new SmtpClient(_smtpServer, _port);
            NetworkCredential basicCredential1 = _credential;
            client.EnableSsl = false;
            client.UseDefaultCredentials = false;
            client.Credentials = basicCredential1;

            MailMessage message = new MailMessage(from, to);

            string mailbody = body;// $
            message.Subject = subject;// "File Request";
            message.Body = body;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            Attachment attachment;
            foreach (string file in attachments)
            {
                attachment = new Attachment(file);
                message.Attachments.Add(attachment);
            }

            try
            {
                client.Send(message);
            }
            catch
            {
                throw;
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed objects
                if (client != null)
                {
                    client.Dispose();
                }
            }
            // Dispose unmanaged objects
            disposed = true;
        }
    }
}
