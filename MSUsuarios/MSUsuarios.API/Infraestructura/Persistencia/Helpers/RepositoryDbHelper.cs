using Npgsql;

namespace MSUsuarios.Infraestructura.Persistencia.Helpers
{
    public static class RepositoryDbHelper
    {
        public static int ExecuteNonQuery(string connectionString, string query, params NpgsqlParameter[] parameters)
        {
            using var conn = new NpgsqlConnection(connectionString);
            using var cmd = new NpgsqlCommand(query, conn);

            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public static int ExecuteNonQuery(string connectionString, NpgsqlCommand command)
        {
            using var connection = new NpgsqlConnection(connectionString);
            command.Connection = connection;
            connection.Open();
            return command.ExecuteNonQuery();
        }

        public static object? ExecuteScalar(string connectionString, NpgsqlCommand command)
        {
            using var connection = new NpgsqlConnection(connectionString);
            command.Connection = connection;
            connection.Open();
            return command.ExecuteScalar();
        }

        public static T? ExecuteReaderSingle<T>(string connectionString, NpgsqlCommand command, Func<NpgsqlDataReader, T> mapper)
        {
            using var connection = new NpgsqlConnection(connectionString);
            command.Connection = connection;
            connection.Open();

            using var reader = command.ExecuteReader();
            return reader.Read() ? mapper(reader) : default;
        }
    }
}
