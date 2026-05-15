namespace FrontendVCare.Dto
{
    public class OperacionApiDto
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;

        public static OperacionApiDto Ok(string mensaje)
        {
            return new OperacionApiDto
            {
                Exito = true,
                Mensaje = mensaje
            };
        }

        public static OperacionApiDto Error(string mensaje)
        {
            return new OperacionApiDto
            {
                Exito = false,
                Mensaje = mensaje
            };
        }
    }
}
