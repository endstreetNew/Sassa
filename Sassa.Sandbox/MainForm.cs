using System.Net.Mail;
using System.Net;
using Sassa.BRM.Services;
using System.Web;
using System.Drawing;
using System;

namespace Sassa.Sandbox
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        //Be advised that you are advised to make amendments to your application SMTP service to now point to the 2019 environment.IP addresses are as follows:

        //10.124.154.18 (PHC) or
        
        //10.117.122.18 (SHC).
        //"SMTPServer": "10.117.122.18",
        //"SMTPHost": "mail.sassa.gov.za",
        //"SMTPPort": 25,
        //"SMTPUser": "no-reply@sassa.gov.za",
        //"SMTPPassword": "D0cum3nt"
        private void button1_Click(object sender, EventArgs e)
        {
            string _smtpServer = "Mail.sassa.gov.za";//"10.117.122.20";//"10.117.122.170";//"10.124.154.25";// "10.117.122.170";//10.117.122.20
            int _port = 25;
            string _smtpUser = "tktest@sassa.gov.za";
            string _smtpPassword = "D0cum3nt";
            NetworkCredential _credential = new System.Net.NetworkCredential(_smtpUser, _smtpPassword);




            using (EmailClient client = new EmailClient(_smtpServer, _port, _credential))
            {
                //send test mail
                string body = $"BRM Email test<br/>";
                client.SendMail($"tktest@sassa.gov.za", "Brutus.Shivambu@alteram.co.za", @"Email test.", body,new List<string>());
            }
        }
    }
}
