using Inovatiqa.Web.Models.Newsletter;

namespace Inovatiqa.Web.Factories.Interfaces
{
    public partial interface INewsletterModelFactory
    {
        NewsletterBoxModel PrepareNewsletterBoxModel();

        SubscriptionActivationModel PrepareSubscriptionActivationModel(bool active);
    }
}
