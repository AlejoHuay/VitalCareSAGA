using Npgsql;
using System.Data;

namespace MSUsuarios.Infraestructura.Persistencia.Helpers
{
    public static class RepositoryDbHelper
    {
        public static int ExecuteNonQuery(string connectionString, string query, params NpgsqlParameter[] parameters)
        {
            using var conn = new NpgsqlConnection(connectionString);
            using var cmd = new NpgsqlCommand(query, conn);

            if (parameters != null)
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

        public static NpgsqlDataReader ExecuteReader(string connectionString, string query, params NpgsqlParameter[] parameters)
        {
            var conn = new NpgsqlConnection(connectionString);
            var cmd = new NpgsqlCommand(query, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            conn.Open();
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public static T? ExecuteReaderSingle<T>(string connectionString, NpgsqlCommand command, Func<NpgsqlDataReader, T> mapper)
        {
            using var connection = new NpgsqlConnection(connectionString);
            command.Connection = connection;
            connection.Open();

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return mapper(reader);
            }

            return default;
        }
    }
}
