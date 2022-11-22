namespace Inovatiqa.Services.Events.Interfaces
{
    public partial interface IEventPublisher
    {
        void Publish<TEvent>(TEvent @event);
    }
}