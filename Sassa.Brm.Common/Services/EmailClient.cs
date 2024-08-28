using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Sassa.Brm.Common.Services
{
    public class EmailClient 
    {
        //private bool disposed;
        private NetworkCredential _credential;

        private string _SMTPServer;
        private int _SMTPPort;
        private string _SMTPUser;
        private string _SMTPPassword;

        public EmailClient(IConfiguration config)
        {
            _SMTPServer = config.GetValue<string>("Email:SMTPServer")!;
            _SMTPUser = config.GetValue<string>("Email:SMTPUser")!;
            _SMTPPassword = config.GetValue<string>("Email:SMTPPassword")!;
            _SMTPPort = config.GetValue<int>("Email:SMTPPort")!;
            _credential = new NetworkCredential(_SMTPUser, _SMTPPassword);
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
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "BRM Application";
                        eventLog.WriteEntry($"From {from} to {to} err: {ex.Message}", EventLogEntryType.Error, 101, 1);
                    }
                }
            }
        }


        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}
        //protected virtual void Dispose(bool disposing)
        //{
        //    if (disposed)
        //    {
        //        return;
        //    }

        //    if (disposing)
        //    {
        //        // Dispose managed objects
        //        if (client != null)
        //        {
        //            client.Dispose();
        //        }
        //    }
        //    // Dispose unmanaged objects
        //    disposed = true;
        //}
    }
}
