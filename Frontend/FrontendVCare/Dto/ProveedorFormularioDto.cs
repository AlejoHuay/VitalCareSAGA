namespace FrontendVCare.Dto
{
    public class ProveedorFormularioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? CorreoElectronico { get; set; }
        public string? Direccion { get; set; }
        public int? IdUsuario { get; set; }
    }
}
