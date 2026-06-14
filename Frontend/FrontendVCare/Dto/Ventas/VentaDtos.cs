namespace FrontendVCare.Dto.Ventas
{
    public class VentaDto
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public DateTime FechaHora { get; set; }

        public int IdCliente { get; set; }

        public string Cliente { get; set; } = string.Empty;

        public string Nit { get; set; } = string.Empty;

        public string RazonSocial { get; set; } = string.Empty;

        public int IdUsuario { get; set; }

        public string Usuario { get; set; } = string.Empty;

        public string MetodoPago { get; set; } = string.Empty;

        public decimal Total { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string EstadoSaga { get; set; } = string.Empty;

        public string? MotivoFalloSaga { get; set; }

        public DateTime? FechaConfirmacionSaga { get; set; }

        public DateTime? FechaCompensacionSaga { get; set; }

        public List<VentaDetalleDto> Detalles { get; set; } = new();
    }

    public class VentaFormularioDto
    {
        public int IdCliente { get; set; }

        public int IdUsuario { get; set; }

        public string MetodoPago { get; set; } = string.Empty;

        public string Nit { get; set; } = string.Empty;

        public string RazonSocial { get; set; } = string.Empty;

        public List<VentaDetalleFormularioDto> Detalles { get; set; } = new();
    }

    public class VentaDetalleDto
    {
        public int IdVenta { get; set; }

        public int IdMedicamento { get; set; }

        public string Medicamento { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }
    }

    public class VentaDetalleFormularioDto
    {
        public int IdMedicamento { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
    }

    public class ReporteVentasPorRolDto
    {
        public string Rol { get; set; } = string.Empty;

        public int CantidadVentas { get; set; }

        public decimal TotalVentas { get; set; }
    }
}
