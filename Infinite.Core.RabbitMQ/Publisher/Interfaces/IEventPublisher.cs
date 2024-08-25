namespace Infinite.Core.RabbitMQ.Consumer.Interfaces
{
    public interface IEventPublisher<TEvent>
    {
        void Publish(TEvent @event);
    }
}
