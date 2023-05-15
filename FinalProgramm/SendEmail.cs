using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FinalProgramm
{
    public class SendEmail
    {
        public int recordNumber = 1;
        public SmtpClient GetSmtpClient(dynamic? emailConfig)
        {
            if (emailConfig is null)
            {
                return null;
            }

            JsonElement gmail = emailConfig.GetProperty("smpt").GetProperty("gmail");

            String host = gmail.GetProperty("host").GetString()!;
            int port = gmail.GetProperty("port").GetInt32();
            String mailbox = gmail.GetProperty("email").ToString()!;
            String password = gmail.GetProperty("password").GetString()!;
            bool ssl = gmail.GetProperty("ssl").GetBoolean();

            return new(host)
            {
                Port = port,
                EnableSsl = ssl,
                Credentials = new NetworkCredential(mailbox, password)
            };
        }

        public async Task SendText(dynamic? emailConfig, string filepath, string recipientMail)
        {
            emailConfig = EmailConfigReader.ReadConfig();
            if (emailConfig is null)
            {
                // Обработка ситуации, когда emailConfig равен null
                MessageBox.Show("Email configuration is missing.");
                return;
            }

            using (SmtpClient smtpClient = GetSmtpClient(emailConfig))
            {
                JsonElement smtpElement;
                if (!emailConfig.TryGetProperty("smpt", out smtpElement))
                {
                    // Обработка ситуации, когда свойство "smpt" отсутствует в emailConfig
                    MessageBox.Show("SMTP configuration is missing.");
                    return;
                }

                JsonElement gmailElement;
                if (!smtpElement.TryGetProperty("gmail", out gmailElement))
                {
                    // Обработка ситуации, когда свойство "gmail" отсутствует в "smpt"
                    MessageBox.Show("Gmail configuration is missing.");
                    return;
                }

                string mailbox = gmailElement.GetProperty("email").GetString();
                if (string.IsNullOrEmpty(mailbox))
                {
                    // Обработка ситуации, когда свойство "email" в "gmail" отсутствует или пустое
                    MessageBox.Show("Email address is missing.");
                    return;
                }

                MailMessage mailMessage = new MailMessage()
                {
                    From = new MailAddress(mailbox),
                    IsBodyHtml = true,
                    Subject = "Test Message"
                };
                mailMessage.To.Add(new MailAddress(recipientMail));

                // Имя ПК и текущая дата
                string body = $"This email was sent from computer '{Environment.MachineName}' on {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
                mailMessage.Body = body;

                string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                // Путь к текстовому файлу
                string filename = Path.Combine(downloadsPath, $"{recordNumber}.txt"); ;
                if (!File.Exists(filename))
                {
                    MessageBox.Show($"File '{filename}' not found");
                    return;
                }

                // Читаем содержимое файла
                string fileContent = File.ReadAllText(filename);

                // MIME-тип для текстового файла
                string mimeType = "text/plain";

                // Создаем вложение с содержимым файла
                Attachment attachment = new Attachment(new MemoryStream(Encoding.UTF8.GetBytes(fileContent)), mimeType);
                attachment.Name = Path.GetFileName(filename);

                mailMessage.Attachments.Add(attachment);
                try
                {
                    await smtpClient.SendMailAsync(mailMessage);

                    // Очищаем содержимое файла после отправки
                    File.WriteAllText(filename, string.Empty);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Sent error '{ex.Message}'");
                }
            }
        }
    }
}