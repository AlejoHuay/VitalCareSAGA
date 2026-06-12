using System.Text;
using System.Text.Json;
using MSProductos.Dominio.Interfaces;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;

namespace MSProductos.Infraestructura.Mensajeria
{
    public class RabbitPublisher : IEventPublisher
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _user;
        private readonly string _password;
        private readonly string _exchange;

        public RabbitPublisher(IConfiguration configuration)
        {
            _host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
            _port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out int port) ? port : 5672;
            _user = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
            _password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";
            _exchange = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? "vitalcare.saga";
        }

        public void Publish<T>(string routingKey, T evento)
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = _host,
                Port = _port,
                UserName = _user,
                Password = _password
            };

            using IConnection connection = factory.CreateConnection();
            using IModel channel = connection.CreateModel();

            channel.ExchangeDeclare(
                exchange: _exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false
            );

            string json = JsonSerializer.Serialize(evento);
            byte[] body = Encoding.UTF8.GetBytes(json);

            IBasicProperties properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";

            channel.BasicPublish(
                exchange: _exchange,
                routingKey: routingKey,
                basicProperties: properties,
                body: body
            );
        }
    }
}