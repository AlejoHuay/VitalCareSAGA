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

            _host = ObtenerVariableObligatoria("RABBITMQ_HOST");

            string puertoRabbit = ObtenerVariableObligatoria("RABBITMQ_PORT");

            if (!int.TryParse(puertoRabbit, out int puerto)
                || puerto <= 0
                || puerto > 65535)
            {
                throw new InvalidOperationException(
                    "La variable RABBITMQ_PORT no contiene un puerto válido."
                );
            }

            _port = puerto;
            _user = ObtenerVariableObligatoria("RABBITMQ_USER");
            _password = ObtenerVariableObligatoria("RABBITMQ_PASSWORD");
            _exchange = ObtenerVariableObligatoria("RABBITMQ_EXCHANGE");
            _queue = "msventas.stock.resultado";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = _host,
                Port = _port,
                UserName = _user,
                Password = _password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            IConnection connection = factory.CreateConnection();
            IModel channel = connection.CreateModel();

            _connection = connection;
            _channel = channel;

            channel.ExchangeDeclare(
                exchange: _exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false
            );

            channel.QueueDeclare(
                queue: _queue,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            channel.QueueBind(
                queue: _queue,
                exchange: _exchange,
                routingKey: "stock.actualizado"
            );

            channel.QueueBind(
                queue: _queue,
                exchange: _exchange,
                routingKey: "stock.fallido"
            );

            channel.BasicQos(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false
            );

            EventingBasicConsumer consumer = new EventingBasicConsumer(channel);

            consumer.Received += (_, args) =>
            {
                try
                {
                    string json = Encoding.UTF8.GetString(args.Body.ToArray());

                    switch (args.RoutingKey)
                    {
                        case "stock.actualizado":
                            ProcesarStockActualizado(json);
                            break;

                        case "stock.fallido":
                            ProcesarStockFallido(json);
                            break;

                        default:
                            throw new InvalidOperationException(
                                $"Routing key no soportada: {args.RoutingKey}"
                            );
                    }

                    channel.BasicAck(
                        deliveryTag: args.DeliveryTag,
                        multiple: false
                    );
                }
                catch
                {
                    channel.BasicNack(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        requeue: false
                    );
                }
            };

            channel.BasicConsume(
                queue: _queue,
                autoAck: false,
                consumer: consumer
            );

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private void ProcesarStockActualizado(string json)
        {
            StockActualizadoEvent? evento =
                JsonSerializer.Deserialize<StockActualizadoEvent>(json);

            if (evento == null || evento.IdVenta <= 0)
            {
                throw new InvalidOperationException(
                    "Evento stock.actualizado inválido."
                );
            }

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

            if (evento == null || evento.IdVenta <= 0)
            {
                throw new InvalidOperationException(
                    "Evento stock.fallido inválido."
                );
            }

            string motivo = string.IsNullOrWhiteSpace(evento.Motivo)
                ? "El procesamiento del stock falló."
                : evento.Motivo.Trim();

            using IServiceScope scope = _serviceProvider.CreateScope();

            IVentaRepository repository =
                scope.ServiceProvider.GetRequiredService<IVentaRepository>();

            Result resultado = repository.CompensarVentaPorFalloStock(
                evento.IdVenta,
                motivo
            );

            if (!resultado.IsSuccess)
                throw new InvalidOperationException(resultado.Error);
        }

        private static string ObtenerVariableObligatoria(string nombre)
        {
            string? valor = Environment.GetEnvironmentVariable(nombre);

            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new InvalidOperationException(
                    $"No se encontró la variable de entorno '{nombre}'."
                );
            }

            return valor.Trim();
        }

        public override void Dispose()
        {
            if (_channel?.IsOpen == true)
                _channel.Close();

            if (_connection?.IsOpen == true)
                _connection.Close();

            _channel?.Dispose();
            _connection?.Dispose();

            base.Dispose();
        }
    }
}