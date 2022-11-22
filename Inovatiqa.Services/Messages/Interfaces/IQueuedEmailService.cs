using System;
using System.Collections.Generic;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Messages.Interfaces
{
    public partial interface IQueuedEmailService
    {
        void InsertQueuedEmail(QueuedEmail queuedEmail);

        void UpdateQueuedEmail(QueuedEmail queuedEmail);

        void DeleteQueuedEmail(QueuedEmail queuedEmail);

        void DeleteQueuedEmails(IList<QueuedEmail> queuedEmails);

        QueuedEmail GetQueuedEmailById(int queuedEmailId);

        IList<QueuedEmail> GetQueuedEmailsByIds(int[] queuedEmailIds);

        IPagedList<QueuedEmail> SearchEmails(string fromEmail,
            string toEmail, DateTime? createdFromUtc, DateTime? createdToUtc, 
            bool loadNotSentItemsOnly, bool loadOnlyItemsToBeSent, int maxSendTries,
            bool loadNewest, int pageIndex = 0, int pageSize = int.MaxValue);

        int DeleteAlreadySentEmails(DateTime? createdFromUtc, DateTime? createdToUtc);
    }
}
