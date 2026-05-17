using System.Text.RegularExpressions;

namespace MSClientes.API.CasosDeUso.Utilidades
{
    public static class StringHelper
    {
        public static string LimpiarEspacios(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            return Regex.Replace(texto.Trim(), @"\s+", " ");
        }

        public static string QuitarEspacios(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            return Regex.Replace(texto, @"\s+", "");
        }
    }
}
