using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSProductos.Aplicacion.Helpers
{
    public static class FiltroHelper
    {
        public static string LimpiarFiltro(string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return string.Empty;

            return string.Join(
                " ",
                filtro.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            );
        }

        public static string ValidarFiltro(string filtro, int minimoCaracteres = 3, int maximoCaracteres = 45)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return string.Empty;

            filtro = LimpiarFiltro(filtro);

            if (filtro.Length < minimoCaracteres)
                return $"El criterio debe tener al menos {minimoCaracteres} caracteres.";

            if (filtro.Length > maximoCaracteres)
                return $"El criterio no puede tener mas de {maximoCaracteres} caracteres.";

            bool invalido = filtro.Any(c =>
                !char.IsLetterOrDigit(c)
                && c != ' '
                && c != '-'
                && c != '.'
                && c != '@'
                && c != '_');

            return invalido ? "Criterio invalido." : string.Empty;
        }

        public static string[] ObtenerPartes(string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return Array.Empty<string>();

            return LimpiarFiltro(filtro)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
    }
}
