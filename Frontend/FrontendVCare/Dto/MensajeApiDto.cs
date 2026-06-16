namespace FrontendVCare.Dto
{
    public class MensajeApiDto
    {
        public string Mensaje { get; set; } = string.Empty;
        public MensajeApiDataDto? Data { get; set; }
    }

    public class MensajeApiDataDto
    {
        public int? IdVenta { get; set; }
    }
}
