using MSVentas.Dominio.Modelos;
using MSVentas.Dominio.Validadores;
using System.Data;

namespace MSVentas.Dominio.Puertos.PuertoSalida
{
    public interface IVentaRepository
    {
        DataTable GetAll();
        DataTable GetAll(string filtro);

        Venta? GetById(int id);
        int Count();

        List<DetalleVenta> GetDetallesByVentaId(int idVenta);

        Result RegistrarVenta(Venta venta);
        Result ActualizarVenta(Venta venta);
        Result AnularVentaLogicamente(int idVenta, int idUsuarioEditor);
        Result ConfirmarStockSaga(int idVenta);

        Result CompensarVentaPorFalloStock(int idVenta, string motivo);
    }
}
