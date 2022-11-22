using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Messages.Interfaces
{
    public partial interface INewsLetterSubscriptionService
    {
        NewsLetterSubscription GetNewsLetterSubscriptionByEmailAndStoreId(string email, int storeId);

        void UpdateNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true);

        void InsertNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true);

        void DeleteNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true);

    }
}
