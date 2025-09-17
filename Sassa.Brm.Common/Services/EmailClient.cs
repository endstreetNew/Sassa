using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;

namespace Sassa.Brm.Common.Services
{
    public class EmailClient
    {
        //private bool disposed;
        private NetworkCredential _credential;
        private ILogger<EmailClient> _logger;

        private string _SMTPServer;
        private int _SMTPPort;
        private string _SMTPUser;
        private string _SMTPPassword;

        public EmailClient(IConfiguration config, ILogger<EmailClient> logger)
        {
            _SMTPServer = config.GetValue<string>("Email:SMTPHost")!;
            _SMTPUser = config.GetValue<string>("Email:SMTPUser")!;
            _SMTPPassword = config.GetValue<string>("Email:SMTPPassword")!;
            _SMTPPort = config.GetValue<int>("Email:SMTPPort")!;
            _credential = new NetworkCredential(_SMTPUser, _SMTPPassword);
            _logger = logger;
        }
        public void SendMail(string from, string to, string subject, string body, List<string>? attachments)
        {
            using (var client = new SmtpClient(_SMTPServer, _SMTPPort))
            {
                client.Credentials = _credential;
                client.EnableSsl = false;
                //client.UseDefaultCredentials = true;

                MailMessage message = new MailMessage(from, to);

                string mailbody = body;// $
                message.Subject = subject;// "File Request";
                message.Body = body;
                message.BodyEncoding = Encoding.UTF8;
                message.IsBodyHtml = true;
                if (attachments != null)
                {
                    Attachment attachment;
                    foreach (string file in attachments)
                    {
                        attachment = new Attachment(file);
                        message.Attachments.Add(attachment);
                    }
                }

                try
                {
                    client.Send(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error sending email to {to}.");
                }
            }
        }


        public bool SMTPServerTest()
        {

            try
            {
                using var client = new TcpClient();
                client.Connect(_SMTPServer, _SMTPPort);
                return true;
            }
            catch (SocketException ex)
            {
                return false;
            }

        }
    }
}
