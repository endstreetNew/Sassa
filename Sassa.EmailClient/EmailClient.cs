using Serilog;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text;

namespace Sassa.BRM.Services
{
    public class EmailClient : IDisposable
    {
        private bool disposed;
        private SmtpClient client;
        private string _smtpServer;
        private int _port;
        private NetworkCredential _credential;
        // Replace this line:
        // Serilog _logger

        // With the following line:
        ILogger _logger;

        public EmailClient(string smptpserver, int port, NetworkCredential credential, ILogger logger)
        {
            _smtpServer = smptpserver;
            _port = port;
            _credential = credential;
            _logger = logger;
            client = new SmtpClient(_smtpServer, _port);
        }
        public void SendMail(string from, string to, string subject, string body, List<string> attachments)
        {

            NetworkCredential basicCredential1 = _credential;
            client.EnableSsl = false;
            client.UseDefaultCredentials = true;// false;
            //client.Credentials = basicCredential1;

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
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error sending email from {from} to {to} with subject {subject}");
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
