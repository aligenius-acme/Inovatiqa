using Inovatiqa.Services.Events.Interfaces;

namespace Inovatiqa.Services.Events
{
    public partial class EventPublisher : IEventPublisher
    {
        #region Methods

        public virtual void Publish<TEvent>(TEvent @event)
        {

        }

        #endregion
    }
}