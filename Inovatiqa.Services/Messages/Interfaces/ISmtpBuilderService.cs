using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Inovatiqa.Database.Models;
using MailKit.Net.Smtp;

namespace Inovatiqa.Services.Messages.Interfaces
{
    public interface ISmtpBuilderService
    {
        SmtpClient Build(EmailAccount emailAccount = null);

        bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors);
    }
}
