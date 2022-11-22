using Inovatiqa.Core;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Messages.Interfaces;
using System;
using System.Linq;

namespace Inovatiqa.Services.Messages
{
    public class NewsLetterSubscriptionService : INewsLetterSubscriptionService
    {
        #region Fields

        private readonly IRepository<NewsLetterSubscription> _subscriptionRepository;

        #endregion

        #region Ctor

        public NewsLetterSubscriptionService(IRepository<NewsLetterSubscription> subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        #endregion

        #region Utilities



        #endregion

        #region Methods

        public virtual void DeleteNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
            if (newsLetterSubscription == null) throw new ArgumentNullException(nameof(newsLetterSubscription));

            _subscriptionRepository.Delete(newsLetterSubscription);

            //Publish the unsubscribe event 
            //PublishSubscriptionEvent(newsLetterSubscription, false, publishSubscriptionEvents);

            //event notification
            //_eventPublisher.EntityDeleted(newsLetterSubscription);
        }

        public virtual NewsLetterSubscription GetNewsLetterSubscriptionByEmailAndStoreId(string email, int storeId)
        {
            if (!CommonHelper.IsValidEmail(email))
                return null;

            email = email.Trim();

            var newsLetterSubscriptions = from nls in _subscriptionRepository.Query()
                                          where nls.Email == email && nls.StoreId == storeId
                                          orderby nls.Id
                                          select nls;

            return newsLetterSubscriptions.FirstOrDefault();
        }

        public virtual void UpdateNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
            if (newsLetterSubscription == null)
            {
                throw new ArgumentNullException(nameof(newsLetterSubscription));
            }

            newsLetterSubscription.Email = CommonHelper.EnsureSubscriberEmailOrThrow(newsLetterSubscription.Email);

            var originalSubscription = _subscriptionRepository.GetById(newsLetterSubscription.Id);

            _subscriptionRepository.Update(newsLetterSubscription);

            //if ((originalSubscription.Active == false && newsLetterSubscription.Active) ||
            //    (newsLetterSubscription.Active && originalSubscription.Email != newsLetterSubscription.Email))
            //{
            //    PublishSubscriptionEvent(newsLetterSubscription, true, publishSubscriptionEvents);
            //}

            //if (originalSubscription.Active && newsLetterSubscription.Active &&
            //    originalSubscription.Email != newsLetterSubscription.Email)
            //{
            //    PublishSubscriptionEvent(originalSubscription, false, publishSubscriptionEvents);
            //}

            //if (originalSubscription.Active && !newsLetterSubscription.Active)
            //{
            //    PublishSubscriptionEvent(originalSubscription, false, publishSubscriptionEvents);
            //}

            //_eventPublisher.EntityUpdated(newsLetterSubscription);
        }

        public virtual void InsertNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
            if (newsLetterSubscription == null)
            {
                throw new ArgumentNullException(nameof(newsLetterSubscription));
            }

            newsLetterSubscription.Email = CommonHelper.EnsureSubscriberEmailOrThrow(newsLetterSubscription.Email);

            _subscriptionRepository.Insert(newsLetterSubscription);

            //if (newsLetterSubscription.Active)
            //{
            //    PublishSubscriptionEvent(newsLetterSubscription, true, publishSubscriptionEvents);
            //}

            //_eventPublisher.EntityInserted(newsLetterSubscription);
        }

        #endregion
    }
}