using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Messages.Interfaces
{
    public partial interface IEmailSenderService
    {
        void SendEmail(EmailAccount emailAccount, string subject, string body,
            string fromAddress, string fromName, string toAddress, string toName,
             string replyToAddress = null, string replyToName = null,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null,
            string attachmentFilePath = null, string attachmentFileName = null,
            int attachedDownloadId = 0, IDictionary<string, string> headers = null);
    }
}
