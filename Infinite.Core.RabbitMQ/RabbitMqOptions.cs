using System.Reflection;

namespace Infinite.Core.RabbitMQ
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; } = "localhost";
        public string ExchangeName { get; set; } = Assembly.GetExecutingAssembly().GetName().Name;
        public string QueueName { get; set; }

        // Parâmetros de erros e tentativas
        public int RetryCount { get; set; } = 3;
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(5);

        // Configurações adicionais de conexão
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public int Port { get; set; } = 5672;

        // Adicionando configurações de fila específicas
        public bool Durable { get; set; } = true;
        public bool Exclusive { get; set; } = false;
        public bool AutoDelete { get; set; } = false;
    }
}
