using System.Text;
using System.Text.Json;
using MSVentas.Dominio.Puertos.PuertoSalida;
using RabbitMQ.Client;

namespace MSVentas.Infraestructura.Mensajeria
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
            _host = ObtenerConfiguracionObligatoria(
                configuration,
                "RABBITMQ_HOST",
                "RabbitMQ:Host"
            );

            string puertoRabbit = ObtenerConfiguracionObligatoria(
                configuration,
                "RABBITMQ_PORT",
                "RabbitMQ:Port"
            );

            if (!int.TryParse(puertoRabbit, out int puerto)
                || puerto <= 0
                || puerto > 65535)
            {
                throw new InvalidOperationException(
                    "La configuración RABBITMQ_PORT no contiene un puerto válido."
                );
            }

            _port = puerto;

            _user = ObtenerConfiguracionObligatoria(
                configuration,
                "RABBITMQ_USER",
                "RabbitMQ:User"
            );

            _password = ObtenerConfiguracionObligatoria(
                configuration,
                "RABBITMQ_PASSWORD",
                "RabbitMQ:Password"
            );

            _exchange = ObtenerConfiguracionObligatoria(
                configuration,
                "RABBITMQ_EXCHANGE",
                "RabbitMQ:Exchange"
            );
        }

        public void Publish<T>(string routingKey, T evento)
        {
            if (string.IsNullOrWhiteSpace(routingKey))
            {
                throw new ArgumentException(
                    "La routing key es obligatoria.",
                    nameof(routingKey)
                );
            }

            if (evento == null)
            {
                throw new ArgumentNullException(nameof(evento));
            }

            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = _host,
                Port = _port,
                UserName = _user,
                Password = _password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
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
            properties.ContentEncoding = "utf-8";

            channel.BasicPublish(
                exchange: _exchange,
                routingKey: routingKey.Trim(),
                basicProperties: properties,
                body: body
            );
        }

        private static string ObtenerConfiguracionObligatoria(
            IConfiguration configuration,
            string variableEntorno,
            string claveConfiguracion)
        {
            string? valor =
                Environment.GetEnvironmentVariable(variableEntorno)
                ?? configuration[claveConfiguracion];

            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new InvalidOperationException(
                    $"No se encontró '{variableEntorno}' ni '{claveConfiguracion}'."
                );
            }

            return valor.Trim();
        }
    }
}