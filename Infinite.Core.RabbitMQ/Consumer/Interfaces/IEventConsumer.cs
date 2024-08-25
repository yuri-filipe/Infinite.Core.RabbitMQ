namespace Infinite.Core.RabbitMQ.Consumer.Interfaces
{
    public interface IEventConsumer<TEvent>
    {
        void Consume();
    }
}
