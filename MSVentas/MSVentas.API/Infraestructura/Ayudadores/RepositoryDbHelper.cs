using System;
using MySql.Data.MySqlClient;

namespace ProyectoArqSoft.Infrastructure.Helpers
{
    public static class RepositoryDbHelper
    {
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

        public static T? ExecuteReaderSingle<T>(
            string connectionString,
            MySqlCommand command,
            Func<MySqlDataReader, T> mapper)
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
