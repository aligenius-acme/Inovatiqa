using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Inovatiqa.Services.Tasks
{
    public class EmailSenderTaskService : BackgroundTaskService
    {
        #region Fields
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IWebHelper _webHelper;
        #endregion

        #region Ctor
        public EmailSenderTaskService(
            IServiceScopeFactory serviceScopeFactory,
            IWebHelper webHelper)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _webHelper = webHelper;
        }
        #endregion

        #region Utilities
        protected virtual Log InsertLog(int logLevel, string shortMessage, string fullMessage = "", Customer customer = null)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var contextService = scope.ServiceProvider.GetRequiredService<Inovatiqa.Database.DbContexts.InovatiqaContext>();
                var log = new Log
                {
                    LogLevelId = logLevel,
                    ShortMessage = shortMessage,
                    FullMessage = fullMessage,
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    CustomerId = customer?.Id,
                    PageUrl = _webHelper.GetThisPageUrl(true),
                    ReferrerUrl = _webHelper.GetUrlReferrer(),
                    CreatedOnUtc = DateTime.UtcNow
                };

                contextService.Log.Add(log);

                return log;
            }
        }
        protected virtual void Error(string message, Exception exception = null, Customer customer = null)
        {
            if (exception is System.Threading.ThreadAbortException)
                return;

            InsertLog(InovatiqaDefaults.Error, message, exception?.ToString() ?? string.Empty, customer);
        }
        protected virtual bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        protected virtual SmtpClient Build(EmailAccount emailAccount = null)
        {
            if (emailAccount is null)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var contextService = scope.ServiceProvider.GetRequiredService<Inovatiqa.Database.DbContexts.InovatiqaContext>();
                    emailAccount = contextService.EmailAccount.Where(x => x.Id == InovatiqaDefaults.DefaultEmailAccountId).FirstOrDefault()
                        ?? throw new InovatiqaException("Email account could not be loaded");
                }             
            }

            var client = new SmtpClient
            {
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
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var maxTries = 3;
                    var loadNewest = true;

                    var contextService = scope.ServiceProvider.GetRequiredService<Inovatiqa.Database.DbContexts.InovatiqaContext>();
                    var query = contextService.QueuedEmail.ToList();

                    var nowUtc = DateTime.UtcNow;
                    query = query.Where(qe => !qe.DontSendBeforeDateUtc.HasValue || qe.DontSendBeforeDateUtc.Value <= nowUtc).ToList();
                    query = query.Where(qe => qe.SentOnUtc == null).ToList();
                    query = query.Where(qe => qe.SentTries < maxTries).ToList();

                    query = loadNewest ?
                        //load the newest records
                        query.OrderByDescending(qe => qe.CreatedOnUtc).ToList() :
                        //load by priority
                        query.OrderByDescending(qe => qe.PriorityId).ThenBy(qe => qe.CreatedOnUtc).ToList();

                    var queuedEmails = query.ToList();

                    foreach (var queuedEmail in queuedEmails)
                    {
                        var bcc = string.IsNullOrWhiteSpace(queuedEmail.Bcc)
                                    ? null
                                    : queuedEmail.Bcc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        var cc = string.IsNullOrWhiteSpace(queuedEmail.Cc)
                                    ? null
                                    : queuedEmail.Cc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                        try
                        {
                            var emailAccount = contextService.EmailAccount.Where(x => x.Id == queuedEmail.EmailAccountId).FirstOrDefault();
                            
                            SendEmail(emailAccount,
                                queuedEmail.Subject,
                                queuedEmail.Body,
                               queuedEmail.From,
                               queuedEmail.FromName,
                               queuedEmail.To,
                               queuedEmail.ToName,
                               queuedEmail.ReplyTo,
                               queuedEmail.ReplyToName,
                               bcc,
                               cc,
                               queuedEmail.AttachmentFilePath,
                               queuedEmail.AttachmentFileName,
                               queuedEmail.AttachedDownloadId);

                            queuedEmail.SentOnUtc = DateTime.UtcNow;
                            queuedEmail.SentTries += 1;
                            contextService.QueuedEmail.Update(queuedEmail);
                            contextService.SaveChanges();


                            //contextService.QueuedEmail.Remove(queuedEmail);
                            //contextService.SaveChanges();

                        }
                        catch (Exception exc)
                        {
                            Error($"Error sending e-mail. {exc.Message}", exc);
                        }
                        finally
                        {
                            //_queuedEmailService.UpdateQueuedEmail(queuedEmail);
                            
                        }
                    }
                }
                await Task.Delay(30000, stoppingToken); // Time to pause this background logic
            }
        }
        protected virtual byte[] ReadAllBytes(string filePath)
        {
            return File.Exists(filePath) ? File.ReadAllBytes(filePath) : Array.Empty<byte>();
        }
        protected virtual DateTime GetCreationTime(string path)
        {
            return File.GetCreationTime(path);
        }
        protected virtual DateTime GetLastWriteTime(string path)
        {
            return File.GetLastWriteTime(path);
        }
        protected virtual DateTime GetLastAccessTime(string path)
        {
            return File.GetLastAccessTime(path);
        }
        protected virtual bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }
        protected MimePart CreateMimeAttachment(Download download)
        {
            if (download is null)
                throw new ArgumentNullException(nameof(download));

            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : download.Id.ToString();

            return CreateMimeAttachment($"{fileName}{download.Extension}", download.DownloadBinary, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow);
        }
        protected MimePart CreateMimeAttachment(string filePath, string attachmentFileName = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (string.IsNullOrWhiteSpace(attachmentFileName))
                attachmentFileName = Path.GetFileName(filePath);

            return CreateMimeAttachment(
                    attachmentFileName,
                    ReadAllBytes(filePath),
                    GetCreationTime(filePath),
                    GetLastWriteTime(filePath),
                    GetLastAccessTime(filePath));
        }
        protected virtual Download GetDownloadById(int downloadId)
        {
            if (downloadId == 0)
                return null;

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var contextService = scope.ServiceProvider.GetRequiredService<Inovatiqa.Database.DbContexts.InovatiqaContext>();
                var query = contextService.Download.Where(x => x.Id == downloadId).FirstOrDefault();
                return query;
            }
        }
        protected MimePart CreateMimeAttachment(string attachmentFileName, byte[] binaryContent, DateTime cDate, DateTime mDate, DateTime rDate)
        {
            if (!ContentType.TryParse(MimeKit.MimeTypes.GetMimeType(attachmentFileName), out var mimeContentType))
                mimeContentType = new ContentType("application", "octet-stream");

            return new MimePart(mimeContentType)
            {
                FileName = attachmentFileName,
                Content = new MimeContent(new MemoryStream(binaryContent), ContentEncoding.Default),
                ContentDisposition = new ContentDisposition
                {
                    CreationDate = cDate,
                    ModificationDate = mDate,
                    ReadDate = rDate
                }
            };
        }
        #endregion

        #region Methods
        public virtual void SendEmail(EmailAccount emailAccount, string subject, string body,
            string fromAddress, string fromName, string toAddress, string toName,
            string replyTo = null, string replyToName = null,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null,
            string attachmentFilePath = null, string attachmentFileName = null,
            int attachedDownloadId = 0, IDictionary<string, string> headers = null)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(new MailboxAddress(toName, toAddress));

            if (!string.IsNullOrEmpty(replyTo))
            {
                message.ReplyTo.Add(new MailboxAddress(replyToName, replyTo));
            }

            //BCC
            if (bcc != null)
            {
                foreach (var address in bcc.Where(bccValue => !string.IsNullOrWhiteSpace(bccValue)))
                {
                    message.Bcc.Add(new MailboxAddress(address.Trim()));
                }
            }

            //CC
            if (cc != null)
            {
                foreach (var address in cc.Where(ccValue => !string.IsNullOrWhiteSpace(ccValue)))
                {
                    message.Cc.Add(new MailboxAddress(address.Trim()));
                }
            }

            //content
            message.Subject = subject;

            //headers
            if (headers != null)
                foreach (var header in headers)
                {
                    message.Headers.Add(header.Key, header.Value);
                }

            var multipart = new Multipart("mixed")
            {
                new TextPart(TextFormat.Html) { Text = body }
            };

            //create the file attachment for this e-mail message
            if (!string.IsNullOrEmpty(attachmentFilePath) && FileExists(attachmentFilePath))
            {
                multipart.Add(CreateMimeAttachment(attachmentFilePath, attachmentFileName));
            }

            //another attachment?
            if (attachedDownloadId > 0)
            {
                var download = GetDownloadById(attachedDownloadId);
                //we do not support URLs as attachments
                if (!download?.UseDownloadUrl ?? false)
                {
                    multipart.Add(CreateMimeAttachment(download));
                }
            }

            message.Body = multipart;

            //send email
            using var smtpClient = Build(emailAccount);
            smtpClient.Send(message);
            smtpClient.Disconnect(true);
        }
        #endregion
    }
}
