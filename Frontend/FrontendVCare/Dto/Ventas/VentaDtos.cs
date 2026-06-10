namespace FrontendVCare.Dto.Ventas
{
    public class VentaDto
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public int IdCliente { get; set; }

        public string Cliente { get; set; } = string.Empty;

        public int IdUsuario { get; set; }

        public string Usuario { get; set; } = string.Empty;

        public decimal Total { get; set; }

        public string Estado { get; set; } = string.Empty;

        public List<VentaDetalleDto> Detalles { get; set; } = new();
    }

    public class VentaFormularioDto
    {
        public int IdCliente { get; set; }

        public int IdUsuario { get; set; }

        public List<VentaDetalleFormularioDto> Detalles { get; set; } = new();
    }

    public class VentaDetalleDto
    {
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
