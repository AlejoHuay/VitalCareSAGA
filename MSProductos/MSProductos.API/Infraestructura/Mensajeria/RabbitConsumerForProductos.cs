using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MSProductos.Aplicacion.DTOs.Eventos;
using MSProductos.Dominio.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MSProductos.Infraestructura.Mensajeria
{
    public class RabbitConsumerForProductos : BackgroundService
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

        public RabbitConsumerForProductos(IServiceProvider serviceProvider)
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
            _queue = "msproductos.venta.creada";
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
                routingKey: "venta.creada"
            );

            _channel.BasicQos(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false
            );

            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (_, args) =>
            {
                try
                {
                    string json = Encoding.UTF8.GetString(args.Body.ToArray());

                    VentaCreadaEvent? evento =
                        JsonSerializer.Deserialize<VentaCreadaEvent>(json);

                    if (evento == null)
                    {
                        _channel.BasicNack(
                            deliveryTag: args.DeliveryTag,
                            multiple: false,
                            requeue: false
                        );

                        return;
                    }

                    ProcesarVentaCreada(evento);

                    _channel.BasicAck(
                        deliveryTag: args.DeliveryTag,
                        multiple: false
                    );
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

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private void ProcesarVentaCreada(VentaCreadaEvent evento)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();

            IMedicamentoRepository medicamentoRepository =
                scope.ServiceProvider.GetRequiredService<IMedicamentoRepository>();

            IEventPublisher eventPublisher =
                scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            List<DetalleVentaCreadaEvent> descontados = new List<DetalleVentaCreadaEvent>();

            foreach (DetalleVentaCreadaEvent detalle in evento.Detalles)
            {
                int filas = medicamentoRepository.DescontarStock(
                    detalle.IdMedicamento,
                    detalle.Cantidad,
                    evento.IdUsuario
                );

                if (filas <= 0)
                {
                    foreach (DetalleVentaCreadaEvent detalleRevertir in descontados)
                    {
                        medicamentoRepository.RevertirStock(
                            detalleRevertir.IdMedicamento,
                            detalleRevertir.Cantidad,
                            evento.IdUsuario
                        );
                    }

                    eventPublisher.Publish(
                        "stock.fallido",
                        new
                        {
                            evento.IdVenta,
                            Motivo =
                                $"Stock insuficiente o medicamento inactivo. Medicamento: {detalle.IdMedicamento}",
                            Fecha = DateTime.Now
                        }
                    );

                    return;
                }

                descontados.Add(detalle);
            }

            eventPublisher.Publish(
                "stock.actualizado",
                new
                {
                    evento.IdVenta,
                    evento.IdUsuario,
                    Fecha = DateTime.Now,
                    Detalles = evento.Detalles
                }
            );
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