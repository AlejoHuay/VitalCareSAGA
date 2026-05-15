using MySql.Data.MySqlClient;
using System.Data;

namespace MSUsuarios.Infraestructura.Persistencia.Helpers
{
    public static class RepositoryDbHelper
    {
        public static int ExecuteNonQuery(string connectionString, string query, params MySqlParameter[] parameters)
        {
            using var conn = new MySqlConnection(connectionString);
            using var cmd = new MySqlCommand(query, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public static int ExecuteNonQuery(string connectionString, MySqlCommand command)
        {
            using var connection = new MySqlConnection(connectionString);
            command.Connection = connection;
            connection.Open();
            return command.ExecuteNonQuery();
        }

        public static object? ExecuteScalar(string connectionString, MySqlCommand command)
        {
            using var connection = new MySqlConnection(connectionString);
            command.Connection = connection;
            connection.Open();
            return command.ExecuteScalar();
        }

        public static MySqlDataReader ExecuteReader(string connectionString, string query, params MySqlParameter[] parameters)
        {
            var conn = new MySqlConnection(connectionString);
            var cmd = new MySqlCommand(query, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            conn.Open();
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public static T? ExecuteReaderSingle<T>(string connectionString, MySqlCommand command, Func<MySqlDataReader, T> mapper)
        {
            using var connection = new MySqlConnection(connectionString);
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
