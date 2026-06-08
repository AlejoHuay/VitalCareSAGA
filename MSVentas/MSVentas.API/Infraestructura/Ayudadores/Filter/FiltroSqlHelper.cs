using MySql.Data.MySqlClient;
using System.Text;

namespace MSVentas.Infraestructura.Ayudadores
{
    public static class FiltroSqlHelper
    {
        public static string ConstruirCondicionLike(string filtro, params string[] columnas)
        {
            string[] partes = FiltroHelper.ObtenerPartes(filtro);

            if (partes.Length == 0 || columnas.Length == 0)
                return string.Empty;

            StringBuilder condicion = new StringBuilder();

            for (int i = 0; i < partes.Length; i++)
            {
                condicion.Append(" AND (");

                for (int j = 0; j < columnas.Length; j++)
                {
                    condicion.Append($"REPLACE({columnas[j]}, ' ', '') LIKE @valor{i}");

                    if (j < columnas.Length - 1)
                        condicion.Append(" OR ");
                }

                condicion.Append(")");
            }

            return condicion.ToString();
        }

        public static void AgregarParametrosLike(MySqlCommand command, string filtro)
        {
            string[] partes = FiltroHelper.ObtenerPartes(filtro);

            for (int i = 0; i < partes.Length; i++)
            {
                string valor = StringHelper.QuitarEspacios(partes[i]);
                command.Parameters.AddWithValue($"@valor{i}", $"%{valor}%");
            }
        }
    }
}
