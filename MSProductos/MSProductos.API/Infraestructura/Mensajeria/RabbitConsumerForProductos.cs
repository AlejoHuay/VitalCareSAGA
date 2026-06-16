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
            _host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
            _port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out int port) ? port : 5672;
            _user = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
            _password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";
            _exchange = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? "vitalcare.saga";
            _queue = "msproductos.venta.creada";
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

            _channel.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true, autoDelete: false);

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

            _channel.QueueBind(
                queue: _queue,
                exchange: _exchange,
                routingKey: "venta.anulada"
            );

            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (sender, args) =>
            {
                try
                {
                    string json = Encoding.UTF8.GetString(args.Body.ToArray());
                    if (args.RoutingKey == "venta.creada")
                    {
                        VentaCreadaEvent? evento = JsonSerializer.Deserialize<VentaCreadaEvent>(json);

                        if (evento == null)
                        {
                            _channel.BasicAck(args.DeliveryTag, false);
                            return;
                        }

                        ProcesarVentaCreada(evento);
                    }
                    else if (args.RoutingKey == "venta.anulada")
                    {
                        VentaAnuladaEvent? evento = JsonSerializer.Deserialize<VentaAnuladaEvent>(json);

                        if (evento == null)
                        {
                            _channel.BasicAck(args.DeliveryTag, false);
                            return;
                        }

                        ProcesarVentaAnulada(evento);
                    }

                    _channel.BasicAck(args.DeliveryTag, false);
                }
                catch
                {
                    _channel.BasicNack(args.DeliveryTag, false, requeue: false);
                }
            };

            _channel.BasicConsume(
                queue: _queue,
                autoAck: false,
                consumer: consumer
            );

            return Task.CompletedTask;
        }

        private void ProcesarVentaCreada(VentaCreadaEvent evento)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();

            IMedicamentoRepository medicamentoRepository =
                scope.ServiceProvider.GetRequiredService<IMedicamentoRepository>();

            IEventPublisher eventPublisher =
                scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            List<DetalleVentaCreadaEvent> descontados = new();

            foreach (DetalleVentaCreadaEvent detalle in evento.Detalles)
            {
                int filas = medicamentoRepository.DescontarStock(
                    detalle.IdMedicamento,
                    detalle.Cantidad,
                    evento.IdUsuario
                );

                if (filas <= 0)
                {
                    foreach (DetalleVentaCreadaEvent revertir in descontados)
                    {
                        medicamentoRepository.RevertirStock(
                            revertir.IdMedicamento,
                            revertir.Cantidad,
                            evento.IdUsuario
                        );
                    }

                    eventPublisher.Publish("stock.fallido", new
                    {
                        evento.IdVenta,
                        Motivo = $"Stock insuficiente o medicamento inactivo. Medicamento: {detalle.IdMedicamento}",
                        Fecha = DateTime.Now
                    });

                    return;
                }

                descontados.Add(detalle);
            }

            eventPublisher.Publish("stock.actualizado", new
            {
                evento.IdVenta,
                evento.IdUsuario,
                Fecha = DateTime.Now,
                Detalles = evento.Detalles
            });
        }

        private void ProcesarVentaAnulada(VentaAnuladaEvent evento)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();

            IMedicamentoRepository medicamentoRepository =
                scope.ServiceProvider.GetRequiredService<IMedicamentoRepository>();

            IEventPublisher eventPublisher =
                scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            foreach (DetalleVentaAnuladaEvent detalle in evento.Detalles)
            {
                int filas = medicamentoRepository.RevertirStock(
                    detalle.IdMedicamento,
                    detalle.Cantidad,
                    evento.IdUsuario
                );

                if (filas <= 0)
                {
                    eventPublisher.Publish("stock.reversion_fallida", new
                    {
                        evento.IdVenta,
                        Motivo = $"No se pudo revertir el stock. Medicamento: {detalle.IdMedicamento}",
                        Fecha = DateTime.Now
                    });

                    return;
                }
            }

            eventPublisher.Publish("stock.revertido", new
            {
                evento.IdVenta,
                evento.IdUsuario,
                Fecha = DateTime.Now,
                Detalles = evento.Detalles
            });
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
