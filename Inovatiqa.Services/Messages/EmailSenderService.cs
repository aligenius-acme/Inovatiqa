using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Media.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MimeTypes = MimeKit.MimeTypes;

namespace Inovatiqa.Services.Messages
{
    public partial class EmailSenderService : IEmailSenderService
    {
        #region Fields

        private readonly IDownloadService _downloadService;
        private readonly IInovatiqaFileProvider _fileProvider;
        private readonly ISmtpBuilderService _smtpBuilder;

        #endregion

        #region Ctor

        public EmailSenderService(IDownloadService downloadService, IInovatiqaFileProvider fileProvider, ISmtpBuilderService smtpBuilder)
        {
            _downloadService = downloadService;
            _fileProvider = fileProvider;
            _smtpBuilder = smtpBuilder;
        }

        #endregion

        #region Utilities

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
                    _fileProvider.ReadAllBytes(filePath),
                    _fileProvider.GetCreationTime(filePath),
                    _fileProvider.GetLastWriteTime(filePath),
                    _fileProvider.GetLastAccessTime(filePath));
        }

        protected MimePart CreateMimeAttachment(string attachmentFileName, byte[] binaryContent, DateTime cDate, DateTime mDate, DateTime rDate)
        {
            if (!ContentType.TryParse(MimeTypes.GetMimeType(attachmentFileName), out var mimeContentType))
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

            if (bcc != null)
            {
                foreach (var address in bcc.Where(bccValue => !string.IsNullOrWhiteSpace(bccValue)))
                {
                    message.Bcc.Add(new MailboxAddress(address.Trim()));
                }
            }

            if (cc != null)
            {
                foreach (var address in cc.Where(ccValue => !string.IsNullOrWhiteSpace(ccValue)))
                {
                    message.Cc.Add(new MailboxAddress(address.Trim()));
                }
            }

            message.Subject = subject;

            if (headers != null)
                foreach (var header in headers)
                {
                    message.Headers.Add(header.Key, header.Value);
                }

            var multipart = new Multipart("mixed")
            {
                new TextPart(TextFormat.Html) { Text = body }
            };

            if (!string.IsNullOrEmpty(attachmentFilePath) && _fileProvider.FileExists(attachmentFilePath))
            {
                multipart.Add(CreateMimeAttachment(attachmentFilePath, attachmentFileName));
            }

            if (attachedDownloadId > 0)
            {
                var download = _downloadService.GetDownloadById(attachedDownloadId);
                if (!download?.UseDownloadUrl ?? false)
                {
                    multipart.Add(CreateMimeAttachment(download));
                }
            }

            message.Body = multipart;

            using var smtpClient = _smtpBuilder.Build(emailAccount);
            smtpClient.Send(message);
            smtpClient.Disconnect(true);
        }

        #endregion
    }
}