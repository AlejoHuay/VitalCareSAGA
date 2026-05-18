namespace FrontendVCare.Dto
{
    public class ProveedorDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? CorreoElectronico { get; set; }
        public string? Direccion { get; set; }
        public short Estado { get; set; } = 1;
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimaActualizacion { get; set; }
        public int? IdUsuario { get; set; }
    }
}
