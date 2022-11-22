using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Messages.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Inovatiqa.Services.Messages
{
    public class SmtpBuilderService : ISmtpBuilderService
    {
        #region Fields

        private readonly IEmailAccountService _emailAccountService;

        #endregion

        #region Ctor

        public SmtpBuilderService(IEmailAccountService emailAccountService)
        {
            _emailAccountService = emailAccountService;
        }

        #endregion

        #region Methods

        public virtual SmtpClient Build(EmailAccount emailAccount = null)
        {
            if (emailAccount is null)
            {
                emailAccount = _emailAccountService.GetEmailAccountById(InovatiqaDefaults.DefaultEmailAccountId)
                ?? throw new InovatiqaException("Email account could not be loaded");
            }

            var client = new SmtpClient {
                ServerCertificateValidationCallback = ValidateServerCertificate
            };

            try
            {
                client.Connect(
                    emailAccount.Host,
                    emailAccount.Port,
                    emailAccount.EnableSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable);

                if (emailAccount.UseDefaultCredentials)
                {
                    client.Authenticate(CredentialCache.DefaultNetworkCredentials);
                } 
                else if (!string.IsNullOrWhiteSpace(emailAccount.Username))
                {
                    client.Authenticate(new NetworkCredential(emailAccount.Username, emailAccount.Password));
                }

                return client;
            }
            catch (Exception ex)
            {
                client.Dispose();
                throw new InovatiqaException(ex.Message, ex);
            }
        }

        public virtual bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        #endregion
    }
}