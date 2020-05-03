using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Edi.TemplateEmail.TestConsole
{
    class Program
    {
        public static EmailHelper EmailHelper { get; set; }
        public class AppSettings
        {
            public string SmtpServer { get; set; }
            public string SmtpUserName { get; set; }
            public string SmtpPassword { get; set; }
            public int SmtpServerPort { get; set; }
            public bool EnableSsl { get; set; }

            public string EmailDisplayName { get; set; }
            public string ToAddress { get; set; }
        }
        static async Task Main(string[] args)
        {

            //var message = new MimeMessage();
            //message.From.Add(new MailboxAddress("open@iwenli.org"));
            //message.To.Add(new MailboxAddress("234486036@qq.com"));

            //message.Subject = "星期天去哪里玩？";

            //message.Body = new TextPart("plain") { Text = "我想去故宫玩，如何" };

            //using (var client = new SmtpClient())
            //{
            //    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
            //    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            //    client.Connect("smtp.exmail.qq.com", 465, true);

            //    // Note: since we don't have an OAuth2 token, disable
            //    // the XOAUTH2 authentication mechanism.
            //    client.AuthenticationMechanisms.Remove("XOAUTH2");

            //    // Note: only needed if the SMTP server requires authentication
            //    client.Authenticate("open@iwenli.org", "Zyl521+yx");

            //    client.Send(message);
            //    client.Disconnect(true);
            //}


            var configuration = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json",
                               optional: true,
                               reloadOnChange: true)
                  .AddUserSecrets<AppSettings>()
                  .Build();

            var appSetting = configuration.GetSection("AppSettings").Get<AppSettings>();

            var configSource = $"{Directory.GetCurrentDirectory()}\\mailConfiguration.xml";

            if (EmailHelper == null)
            {
                var settings = new EmailSettings(appSetting.SmtpServer, appSetting.SmtpUserName, appSetting.SmtpPassword, appSetting.SmtpServerPort)
                {
                    EnableSsl = appSetting.EnableSsl,
                    EmailDisplayName = appSetting.EmailDisplayName,
                    SenderName = "测试邮件"
                };

                EmailHelper = new EmailHelper(configSource, settings);
                EmailHelper.EmailSent += (sender, eventArgs) =>
                {
                    Console.WriteLine($"Email is sent, Success: {eventArgs.IsSuccess}, Response: {eventArgs.ServerResponse}");
                };
                EmailHelper.EmailFailed += (sender, eventArgs) =>
                {
                    Console.WriteLine("Failed");
                };
                EmailHelper.EmailCompleted += (sender, e) =>
                {
                    Console.WriteLine("Completed.");
                };
            }

            try
            {
                Console.WriteLine("Sending Email...");
                await TestSendTestMail(appSetting.ToAddress);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.ReadLine();
        }

        public static async Task TestSendTestMail(string toAddress)
        {
            var pipeline = new TemplatePipeline().Map("MachineName", Environment.MachineName)
                .Map("SmtpServer", EmailHelper.Settings.SmtpServer)
                .Map("SmtpServerPort", EmailHelper.Settings.SmtpServerPort)
                .Map("SmtpUserName", EmailHelper.Settings.SmtpUserName)
                .Map("EmailDisplayName", EmailHelper.Settings.EmailDisplayName)
                .Map("EnableSsl", EmailHelper.Settings.EnableSsl);

            await EmailHelper.ApplyTemplate("TestMail", pipeline).SendMailAsync(toAddress);
        }
    }
}
