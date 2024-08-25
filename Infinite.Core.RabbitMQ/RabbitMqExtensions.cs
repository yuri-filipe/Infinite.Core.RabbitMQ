using Infinite.Core.RabbitMQ.Consumer;
using Infinite.Core.RabbitMQ.Consumer.Interfaces;
using Infinite.Core.RabbitMQ.Publisher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Infinite.Core.RabbitMQ
{
    public static class RabbitMqExtensions
    {
        public static IServiceCollection AddRabbitMq(this IServiceCollection services, Action<RabbitMqOptions> configureOptions)
        {
            var options = new RabbitMqOptions();

            configureOptions(options);

            services.AddSingleton(options);

            services.AddSingleton<IConnection>(sp =>
            {
                ILogger<RabbitMqOptions> logger = sp.GetRequiredService<ILogger<RabbitMqOptions>>();

                var factory = new ConnectionFactory()
                {
                    HostName = options.HostName,
                    UserName = options.UserName,
                    Password = options.Password,
                    Port = options.Port
                };

                IConnection connection = null;

                int currentRetry = 0;

                while (currentRetry < options.RetryCount)
                {
                    try
                    {
                        connection = factory.CreateConnection();

                        logger.LogInformation("Successfully connected to RabbitMQ.");

                        break;
                    }
                    catch (Exception ex)
                    {
                        currentRetry++;

                        logger.LogError(ex, "Failed to connect to RabbitMQ. Retry {RetryCount} of {MaxRetries}.", currentRetry, options.RetryCount);

                        if (currentRetry >= options.RetryCount)
                        {
                            logger.LogCritical("Exceeded maximum number of retries to connect to RabbitMQ.");
                            throw;
                        }

                        Task.Delay(options.RetryInterval).Wait();
                    }
                }

                return connection;
            });

            services.AddSingleton<IModel>(sp =>
            {
                IConnection connection = sp.GetRequiredService<IConnection>();

                IModel channel = connection.CreateModel();

                ILogger<RabbitMqOptions> logger = sp.GetRequiredService<ILogger<RabbitMqOptions>>();

                logger.LogInformation("Creating channel and declaring exchange: {ExchangeName}.", options.ExchangeName);

                channel.ExchangeDeclare(options.ExchangeName, ExchangeType.Direct, options.Durable, options.AutoDelete, null);

                channel.QueueDeclare(options.QueueName, options.Durable, options.Exclusive, options.AutoDelete, null);

                channel.QueueBind(options.QueueName, options.ExchangeName, options.QueueName);

                return channel;
            });

            return services;
        }

        public static IServiceCollection AddRabbitMqConsumer<TEvent>(this IServiceCollection services, Action<TEvent> onMessage) where TEvent : class
        {
            services.AddSingleton<IEventConsumer<TEvent>>(sp =>
            {
                IModel channel = sp.GetRequiredService<IModel>();

                RabbitMqOptions options = sp.GetRequiredService<RabbitMqOptions>();

                ILogger<RabbitMqEventConsumer<TEvent>> logger = sp.GetRequiredService<ILogger<RabbitMqEventConsumer<TEvent>>>();

                return new RabbitMqEventConsumer<TEvent>(channel, options, onMessage, logger);
            });

            return services;
        }

        public static IServiceCollection AddRabbitMqPublisher<TEvent>(this IServiceCollection services) where TEvent : class
        {
            services.AddSingleton<IEventPublisher<TEvent>>(sp =>
            {
                IModel channel = sp.GetRequiredService<IModel>();

                RabbitMqOptions options = sp.GetRequiredService<RabbitMqOptions>();

                ILogger<RabbitMqEventPublisher<TEvent>> logger = sp.GetRequiredService<ILogger<RabbitMqEventPublisher<TEvent>>>();

                return new RabbitMqEventPublisher<TEvent>(channel, options, logger);
            });

            return services;
        }
    }
}

