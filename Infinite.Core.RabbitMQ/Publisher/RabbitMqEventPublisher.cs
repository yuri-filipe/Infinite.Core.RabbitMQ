using Infinite.Core.RabbitMQ.Consumer.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Infinite.Core.RabbitMQ.Publisher
{
    public class RabbitMqEventPublisher<TEvent> : IEventPublisher<TEvent> where TEvent : class
    {
        private readonly IModel _channel;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqEventPublisher<TEvent>> _logger;

        public RabbitMqEventPublisher(IModel channel, RabbitMqOptions options, ILogger<RabbitMqEventPublisher<TEvent>> logger)
        {
            _channel = channel;
            _options = options;
            _logger = logger;
        }

        public void Publish(TEvent @event)
        {
            byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));

            _channel.BasicPublish(_options.ExchangeName, typeof(TEvent).Name, null, body);

            _logger.LogInformation("Published event of type {EventType} to exchange {ExchangeName}.", typeof(TEvent).Name, _options.ExchangeName);
        }
    }
}
