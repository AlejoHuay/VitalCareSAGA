namespace FrontendVCare.Dto.ClasificacionDtos
{
    public class ClasificacionDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
    }
}
