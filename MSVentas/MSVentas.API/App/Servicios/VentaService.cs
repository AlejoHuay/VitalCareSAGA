using MSVentas.App.DTOs;
using MSVentas.App.Interfaces;
using MSVentas.Dominio.Modelos;
using MSVentas.Dominio.Puertos.PuertoSalida;
using MSVentas.Dominio.Validadores;
using MSVentas.Infraestructura.Ayudadores;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MSVentas.App.DTOs.Eventos;
using MSVentas.Dominio.Puertos.PuertoSalida;

namespace MSVentas.App.Servicios
{
    public class VentaService : IVentaService
    {
        private readonly IVentaRepository _repository;
        private readonly IResult<Venta> _validador;
        private readonly IEventPublisher _eventPublisher;

        public VentaService(IVentaRepository repository, IResult<Venta> validador, IEventPublisher eventPublisher)
        {
            _repository = repository;
            _validador = validador;
            _eventPublisher = eventPublisher;
        }

        public DataTable ObtenerTodos()
        {
            return _repository.GetAll();
        }

        public DataTable ObtenerTodos(string filtro)
        {
            filtro = StringHelper.LimpiarEspacios(filtro);

            return string.IsNullOrWhiteSpace(filtro)
                ? _repository.GetAll()
                : _repository.GetAll(filtro);
        }

        public Venta? ObtenerPorId(int id)
        {
            if (id <= 0)
                return null;

            return _repository.GetById(id);
        }

        public List<DetalleVenta> ObtenerDetallesPorVenta(int idVenta)
        {
            if (idVenta <= 0)
                return new List<DetalleVenta>();

            return _repository.GetDetallesByVentaId(idVenta);
        }

        public Result Crear(
            int idCliente,
            int idUsuario,
            string metodoPago,
            List<DetalleVentaInputDto> detallesInput)
        {
            try
            {
                Venta venta = ConstruirVenta(
                    0,
                    idCliente,
                    idUsuario,
                    metodoPago,
                    detallesInput,
                    null,
                    string.Empty,
                    string.Empty
                );

                Result validacion = _validador.Validar(venta);

                if (!validacion.IsSuccess)
                    return validacion;

                //return _repository.RegistrarVenta(venta);
                Result resultado = _repository.RegistrarVenta(venta);

                if (resultado.IsSuccess)
                {
                    _eventPublisher.Publish("venta.creada", new VentaCreadaEvent
                    {
                        IdVenta = venta.Id,
                        IdCliente = venta.IdCliente,
                        IdUsuario = venta.IdUsuario,
                        Total = venta.Total,
                        MetodoPago = venta.MetodoPago,
                        FechaHora = DateTime.Now,
                        Detalles = venta.Detalles.Select(d => new DetalleVentaCreadaEvent
                        {
                            IdMedicamento = d.IdMedicamento,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario
                        }).ToList()
                    });
                }

                return resultado;
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result Actualizar(
            int idVenta,
            int idCliente,
            string metodoPago,
            List<DetalleVentaInputDto> detallesInput,
            int idUsuarioEditor)
        {
            try
            {
                if (idVenta <= 0)
                    return Result.Fail("El ID de la venta no es válido.");

                if (idUsuarioEditor <= 0)
                    return Result.Fail("El usuario editor no es válido.");

                Venta? ventaExistente = _repository.GetById(idVenta);

                if (ventaExistente == null)
                    return Result.Fail("La venta no existe.");

                if (ventaExistente.Estado == 0)
                    return Result.Fail("No se puede modificar una venta anulada.");

                Venta venta = ConstruirVenta(
                    idVenta,
                    idCliente,
                    ventaExistente.IdUsuario,
                    metodoPago,
                    detallesInput,
                    idUsuarioEditor,
                    ventaExistente.Nit,
                    ventaExistente.RazonSocial
                );

                Result validacion = _validador.Validar(venta);

                if (!validacion.IsSuccess)
                    return validacion;

                return _repository.ActualizarVenta(venta);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result EliminarLogicamente(int idVenta, int idUsuarioEditor)
        {
            try
            {
                if (idVenta <= 0)
                    return Result.Fail("El ID de la venta no es válido.");

                if (idUsuarioEditor <= 0)
                    return Result.Fail("El usuario editor no es válido.");

                Venta? venta = _repository.GetById(idVenta);

                if (venta == null)
                    return Result.Fail("La venta no existe.");

                if (venta.Estado == 0)
                    return Result.Fail("La venta ya se encuentra anulada.");

                return _repository.AnularVentaLogicamente(idVenta, idUsuarioEditor);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        private Venta ConstruirVenta(
            int id,
            int idCliente,
            int idUsuario,
            string metodoPago,
            List<DetalleVentaInputDto> detallesInput,
            int? idUsuarioEditor,
            string? nit,
            string? razonSocial)
        {
            if (idCliente <= 0)
                throw new InvalidOperationException("El cliente no es válido.");

            if (idUsuario <= 0)
                throw new InvalidOperationException("El usuario no es válido.");

            metodoPago = StringHelper.LimpiarEspacios(metodoPago);

            if (string.IsNullOrWhiteSpace(metodoPago))
                throw new InvalidOperationException("Debe indicar el método de pago.");

            List<DetalleVenta> detalles = ConstruirDetalles(detallesInput);

            decimal total = detalles.Sum(detalle =>
                detalle.Cantidad * detalle.PrecioUnitario
            );

            return new Venta
            {
                Id = id,
                IdCliente = idCliente,
                IdUsuario = idUsuario,
                IdUsuarioEditor = idUsuarioEditor,
                MetodoPago = metodoPago,
                Nit = StringHelper.LimpiarEspacios(nit),
                RazonSocial = StringHelper.LimpiarEspacios(razonSocial),
                Total = total,
                Detalles = detalles
            };
        }

        private List<DetalleVenta> ConstruirDetalles(List<DetalleVentaInputDto> detallesInput)
        {
            if (detallesInput == null || detallesInput.Count == 0)
                throw new InvalidOperationException("Debe agregar al menos un medicamento.");

            foreach (DetalleVentaInputDto item in detallesInput)
            {
                if (item.IdMedicamento <= 0)
                    throw new InvalidOperationException("Existe un medicamento con ID no válido.");

                if (item.Cantidad <= 0)
                {
                    throw new InvalidOperationException(
                        $"La cantidad del medicamento {item.IdMedicamento} debe ser mayor que cero."
                    );
                }

                if (item.PrecioUnitario <= 0)
                {
                    throw new InvalidOperationException(
                        $"El precio del medicamento {item.IdMedicamento} debe ser mayor que cero."
                    );
                }
            }

            List<DetalleVenta> detalles = new List<DetalleVenta>();

            foreach (var grupo in detallesInput.GroupBy(item => item.IdMedicamento))
            {
                decimal precioUnitario = grupo.First().PrecioUnitario;

                if (grupo.Any(item => item.PrecioUnitario != precioUnitario))
                {
                    throw new InvalidOperationException(
                        $"El medicamento {grupo.Key} tiene precios diferentes."
                    );
                }

                int cantidadTotal = checked(grupo.Sum(item => item.Cantidad));

                detalles.Add(new DetalleVenta
                {
                    IdMedicamento = grupo.Key,
                    Cantidad = cantidadTotal,
                    PrecioUnitario = precioUnitario
                });
            }

            return detalles;
        }
    }
}

