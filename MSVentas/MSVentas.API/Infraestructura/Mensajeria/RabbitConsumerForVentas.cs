using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MSVentas.App.DTOs.Eventos;
using MSVentas.Dominio.Puertos.PuertoSalida;
using MSVentas.Dominio.Validadores;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MSVentas.Infraestructura.Mensajeria
{
    public class RabbitConsumerForVentas : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly string _host;
        private readonly int _port;
        private readonly string _user;
        private readonly string _password;
        private readonly string _exchange;
        private readonly string _queue;

        private IConnection? _connection;
        private IModel? _channel;

        public RabbitConsumerForVentas(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            _host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";

            _port = int.TryParse(
                Environment.GetEnvironmentVariable("RABBITMQ_PORT"),
                out int port
            )
                ? port
                : 5672;

            _user = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
            _password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";
            _exchange = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? "vitalcare.saga";

            _queue = "msventas.stock.resultado";
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = _host,
                Port = _port,
                UserName = _user,
                Password = _password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: _exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false
            );

            _channel.QueueDeclare(
                queue: _queue,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            _channel.QueueBind(
                queue: _queue,
                exchange: _exchange,
                routingKey: "stock.actualizado"
            );

            _channel.QueueBind(
                queue: _queue,
                exchange: _exchange,
                routingKey: "stock.fallido"
            );

            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (sender, args) =>
            {
                try
                {
                    string json = Encoding.UTF8.GetString(args.Body.ToArray());

                    if (args.RoutingKey == "stock.actualizado")
                    {
                        ProcesarStockActualizado(json);
                    }
                    else if (args.RoutingKey == "stock.fallido")
                    {
                        ProcesarStockFallido(json);
                    }

                    _channel.BasicAck(args.DeliveryTag, false);
                }
                catch
                {
                    _channel.BasicNack(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        requeue: false
                    );
                }
            };

            _channel.BasicConsume(
                queue: _queue,
                autoAck: false,
                consumer: consumer
            );

            return Task.CompletedTask;
        }

        private void ProcesarStockActualizado(string json)
        {
            StockActualizadoEvent? evento =
                JsonSerializer.Deserialize<StockActualizadoEvent>(json);

            if (evento == null)
                throw new InvalidOperationException("Evento stock.actualizado inválido.");

            using IServiceScope scope = _serviceProvider.CreateScope();

            IVentaRepository repository =
                scope.ServiceProvider.GetRequiredService<IVentaRepository>();

            Result resultado = repository.ConfirmarStockSaga(evento.IdVenta);

            if (!resultado.IsSuccess)
                throw new InvalidOperationException(resultado.Error);
        }

        private void ProcesarStockFallido(string json)
        {
            StockFallidoEvent? evento =
                JsonSerializer.Deserialize<StockFallidoEvent>(json);

            if (evento == null)
                throw new InvalidOperationException("Evento stock.fallido inválido.");

            using IServiceScope scope = _serviceProvider.CreateScope();

            IVentaRepository repository =
                scope.ServiceProvider.GetRequiredService<IVentaRepository>();

            Result resultado = repository.CompensarVentaPorFalloStock(
                evento.IdVenta,
                evento.Motivo
            );

            if (!resultado.IsSuccess)
                throw new InvalidOperationException(resultado.Error);
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();

            base.Dispose();
        }
    }
}