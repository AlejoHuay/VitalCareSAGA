using MSVentas.App.DTOs;
using MSVentas.Dominio.Modelos;
using MSVentas.Dominio.Validadores;
using System.Data;

namespace MSVentas.App.Interfaces
{
    public interface IVentaService
    {
        DataTable ObtenerTodos();
        DataTable ObtenerTodos(string filtro);

        Venta? ObtenerPorId(int id);
        List<DetalleVenta> ObtenerDetallesPorVenta(int idVenta);

        Result Crear(
            int idCliente,
            int idUsuario,
            string metodoPago,
            string? nit,
            string? razonSocial,
            List<DetalleVentaInputDto> detallesInput);

        Result Actualizar(
            int idVenta,
            int idCliente,
            string metodoPago,
            List<DetalleVentaInputDto> detallesInput,
            int idUsuarioEditor);

        Result EliminarLogicamente(int idVenta, int idUsuarioEditor);
    }
}
