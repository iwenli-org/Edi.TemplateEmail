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
