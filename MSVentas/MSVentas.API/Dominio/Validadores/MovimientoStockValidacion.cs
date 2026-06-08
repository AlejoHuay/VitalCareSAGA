using MSVentas.App.Interfaces;
using MSVentas.App.DTOs;

namespace MSVentas.Dominio.Validadores
{
    public class MovimientoStockValidacion : IResult<MovimientoStockDTO>
    {
        public Result Validar(MovimientoStockDTO movimiento)
        {
            return ValidarIdMedicamento(movimiento.IdMedicamento)
                ?? ValidarCantidad(movimiento.Cantidad)
                ?? ValidarStockActual(movimiento.StockActual)
                ?? ValidarStockSuficiente(movimiento)
                ?? Result.Ok();
        }

        private Result? ValidarIdMedicamento(int idMedicamento)
        {
            if (idMedicamento <= 0)
                return Result.Fail("El medicamento no es válido.");

            return null;
        }

        private Result? ValidarCantidad(int cantidad)
        {
            if (cantidad <= 0)
                return Result.Fail("La cantidad del movimiento debe ser mayor a cero.");

            if (cantidad > 100000)
                return Result.Fail("La cantidad del movimiento no puede ser mayor a 100000 unidades.");

            return null;
        }


        private Result? ValidarStockActual(int stockActual)
        {
            if (stockActual < 0)
                return Result.Fail("El stock actual del medicamento no puede ser negativo.");

            return null;
        }

        private Result? ValidarStockSuficiente(MovimientoStockDTO movimiento)
        {
            if (movimiento.EsEntrada)
                return null;

            if (movimiento.StockActual < movimiento.Cantidad)
                return Result.Fail("Stock insuficiente para realizar la venta.");

            return null;
        }
    }
}