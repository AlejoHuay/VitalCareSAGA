namespace FrontendVCare.Dto
{
    public class OperacionApiDto
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int? Id { get; set; }

        public static OperacionApiDto Ok(string mensaje, int? id = null)
        {
            return new OperacionApiDto
            {
                Exito = true,
                Mensaje = mensaje,
                Id = id
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
