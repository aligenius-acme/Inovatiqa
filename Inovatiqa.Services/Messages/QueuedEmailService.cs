using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Messages.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Messages
{
    public partial class QueuedEmailService : IQueuedEmailService
    {
        #region Fields

        private readonly IRepository<QueuedEmail> _queuedEmailRepository;

        #endregion

        #region Ctor

        public QueuedEmailService(IRepository<QueuedEmail> queuedEmailRepository)
        {
            _queuedEmailRepository = queuedEmailRepository;
        }

        #endregion

        #region Methods

        public virtual void InsertQueuedEmail(QueuedEmail queuedEmail)
        {
            if (queuedEmail == null)
                throw new ArgumentNullException(nameof(queuedEmail));

            _queuedEmailRepository.Insert(queuedEmail);

            //_eventPublisher.EntityInserted(queuedEmail);
        }

        public virtual void UpdateQueuedEmail(QueuedEmail queuedEmail)
        {
            if (queuedEmail == null)
                throw new ArgumentNullException(nameof(queuedEmail));

            _queuedEmailRepository.Update(queuedEmail);

            //_eventPublisher.EntityUpdated(queuedEmail);
        }

        public virtual void DeleteQueuedEmail(QueuedEmail queuedEmail)
        {
            if (queuedEmail == null)
                throw new ArgumentNullException(nameof(queuedEmail));

            _queuedEmailRepository.Delete(queuedEmail);

            //_eventPublisher.EntityDeleted(queuedEmail);
        }

        public virtual void DeleteQueuedEmails(IList<QueuedEmail> queuedEmails)
        {
            if (queuedEmails == null)
                throw new ArgumentNullException(nameof(queuedEmails));

            foreach (var queuedEmail in queuedEmails)
            {
                _queuedEmailRepository.Delete(queuedEmail);
                //_eventPublisher.EntityDeleted(queuedEmail);
            }
        }

        public virtual QueuedEmail GetQueuedEmailById(int queuedEmailId)
        {
            if (queuedEmailId == 0)
                return null;

            return _queuedEmailRepository.GetById(queuedEmailId);
        }

        public virtual IList<QueuedEmail> GetQueuedEmailsByIds(int[] queuedEmailIds)
        {
            if (queuedEmailIds == null || queuedEmailIds.Length == 0)
                return new List<QueuedEmail>();

            var query = from qe in _queuedEmailRepository.Query()
                        where queuedEmailIds.Contains(qe.Id)
                        select qe;
            var queuedEmails = query.ToList();
            var sortedQueuedEmails = new List<QueuedEmail>();
            foreach (var id in queuedEmailIds)
            {
                var queuedEmail = queuedEmails.Find(x => x.Id == id);
                if (queuedEmail != null)
                    sortedQueuedEmails.Add(queuedEmail);
            }

            return sortedQueuedEmails;
        }

        public virtual IPagedList<QueuedEmail> SearchEmails(string fromEmail,
            string toEmail, DateTime? createdFromUtc, DateTime? createdToUtc,
            bool loadNotSentItemsOnly, bool loadOnlyItemsToBeSent, int maxSendTries,
            bool loadNewest, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            fromEmail = (fromEmail ?? string.Empty).Trim();
            toEmail = (toEmail ?? string.Empty).Trim();

            var query = _queuedEmailRepository.Query();
            if (!string.IsNullOrEmpty(fromEmail))
                query = query.Where(qe => qe.From.Contains(fromEmail));
            if (!string.IsNullOrEmpty(toEmail))
                query = query.Where(qe => qe.To.Contains(toEmail));
            if (createdFromUtc.HasValue)
                query = query.Where(qe => qe.CreatedOnUtc >= createdFromUtc);
            if (createdToUtc.HasValue)
                query = query.Where(qe => qe.CreatedOnUtc <= createdToUtc);
            if (loadNotSentItemsOnly)
                query = query.Where(qe => !qe.SentOnUtc.HasValue);
            if (loadOnlyItemsToBeSent)
            {
                var nowUtc = DateTime.UtcNow;
                query = query.Where(qe => !qe.DontSendBeforeDateUtc.HasValue || qe.DontSendBeforeDateUtc.Value <= nowUtc);
            }

            query = query.Where(qe => qe.SentTries < maxSendTries);
            query = loadNewest ?
                query.OrderByDescending(qe => qe.CreatedOnUtc) :
                query.OrderByDescending(qe => qe.PriorityId).ThenBy(qe => qe.CreatedOnUtc);

            var queuedEmails = new PagedList<QueuedEmail>(query, pageIndex, pageSize);
            return queuedEmails;
        }

        public virtual int DeleteAlreadySentEmails(DateTime? createdFromUtc, DateTime? createdToUtc)
        {
            var query = _queuedEmailRepository.Query();

            query = query.Where(qe => qe.SentOnUtc.HasValue);

            if (createdFromUtc.HasValue)
                query = query.Where(qe => qe.CreatedOnUtc >= createdFromUtc);
            if (createdToUtc.HasValue)
                query = query.Where(qe => qe.CreatedOnUtc <= createdToUtc);

            var emails = query.ToArray();

            DeleteQueuedEmails(emails);

            return emails.Length;
        }

        #endregion
    }
}