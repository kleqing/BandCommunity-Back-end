using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BandCommunity.Application.Services.Email;

public class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            string fromMail = "";
            string fromPassword = "";
            SmtpClient smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com", // SMTP server
                Port = 587, // TLS port
                EnableSsl = true, // Enable TLS (SSL)
                Credentials = new System.Net.NetworkCredential(fromMail, fromPassword) // My Username and password
            };
            MailMessage mail = new MailMessage();
            mail.Subject = subject;
            //mail.Body = GenerateEmailTemplate(email, subject, htmlMessage);

            //Setting From , To and CC
            mail.From = new MailAddress(fromMail);
            mail.To.Add(new MailAddress(email));
            mail.IsBodyHtml = true;
            smtpClient.Send(mail);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return Task.CompletedTask;
    }
}