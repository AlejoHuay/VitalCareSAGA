namespace FrontendVCare.Dto.Reportes
{
    public class ArchivoDescargaDto
    {
        public string NombreArchivo { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Contenido { get; set; } = Array.Empty<byte>();
    }
}
