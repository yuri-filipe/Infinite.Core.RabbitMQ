using Infinite.Core.RabbitMQ.Consumer.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Infinite.Core.RabbitMQ.Consumer
{
    public class RabbitMqEventConsumer<TEvent> : IEventConsumer<TEvent> where TEvent : class
    {
        private readonly IModel _channel;
        private readonly RabbitMqOptions _options;
        private readonly Action<TEvent> _onMessage;
        private readonly ILogger<RabbitMqEventConsumer<TEvent>> _logger;

        public RabbitMqEventConsumer(IModel channel, RabbitMqOptions options, Action<TEvent> onMessage, ILogger<RabbitMqEventConsumer<TEvent>> logger)
        {
            _channel = channel;
            _options = options;
            _onMessage = onMessage;
            _logger = logger;
        }

        public void Consume()
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();

                TEvent message = JsonConvert.DeserializeObject<TEvent>(Encoding.UTF8.GetString(body));

                _logger.LogInformation("Received event of type {EventType} from queue {QueueName}.", typeof(TEvent).Name, _options.QueueName);

                _onMessage(message);
            };

            _channel.BasicConsume(queue: _options.QueueName, autoAck: true, consumer: consumer);

            _logger.LogInformation("Started consuming queue {QueueName}.", _options.QueueName);
        }
    }
}
