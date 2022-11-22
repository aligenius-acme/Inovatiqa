using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Newsletter;

namespace Inovatiqa.Web.Factories
{
    public partial class NewsletterModelFactory : INewsletterModelFactory
    {
        #region Fields

        #endregion

        #region Ctor

        public NewsletterModelFactory()
        {
        }

        #endregion

        #region Methods

        public virtual NewsletterBoxModel PrepareNewsletterBoxModel()
        {
            var model = new NewsletterBoxModel()
            {
                AllowToUnsubscribe = true
            };
            return model;
        }

        public virtual SubscriptionActivationModel PrepareSubscriptionActivationModel(bool active)
        {
            var model = new SubscriptionActivationModel
            {
                Result = active
                ? "Your subscription has been activated."
                : "Your subscription has been deactivated."
            };

            return model;
        }

        #endregion
    }
}
