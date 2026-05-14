using Npgsql;
using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Infraestructura.Persistencia.Conexion;
using MSUsuarios.Infraestructura.Persistencia.Helpers;

namespace MSUsuarios.Infraestructura.Persistencia.Repositorios
{
    public class TokenRepository : ITokenRepository
    {
        private readonly string _connectionString;

        public TokenRepository()
        {
            _connectionString = ConexionStringSingleton.Instancia.CadenaConexion;
        }

        public int Insert(Token token)
        {
            const string query = @"
                INSERT INTO usuario_token
                (
                    usuario_id,
                    token_hash,
                    tipo_token,
                    fecha_expiracion
                )
                VALUES
                (
                    @usuario_id,
                    @token_hash,
                    @tipo_token,
                    @fecha_expiracion
                )";

            NpgsqlCommand command = new NpgsqlCommand(query);
            command.Parameters.AddWithValue("@usuario_id", token.UsuarioId);
            command.Parameters.AddWithValue("@token_hash", token.TokenHash);
            command.Parameters.AddWithValue("@tipo_token", token.TipoToken);
            command.Parameters.AddWithValue("@fecha_expiracion", token.FechaExpiracion);

            return RepositoryDbHelper.ExecuteNonQuery(_connectionString, command);
        }

        public Token? GetByTokenHash(string tokenHash)
        {
            const string query = @"
                SELECT *
                FROM usuario_token
                WHERE token_hash = @token_hash
                LIMIT 1";

            NpgsqlCommand command = new NpgsqlCommand(query);
            command.Parameters.AddWithValue("@token_hash", tokenHash);

            return RepositoryDbHelper.ExecuteReaderSingle(_connectionString, command, MapearUsuarioToken);
        }

        public Token? GetTokenActivo(string tokenHash, string tipoToken)
        {
            const string query = @"
                SELECT *
                FROM usuario_token
                WHERE token_hash = @token_hash
                  AND tipo_token = @tipo_token
                  AND revocado = FALSE
                  AND usado = FALSE
                  AND fecha_expiracion > CURRENT_TIMESTAMP
                LIMIT 1";

            NpgsqlCommand command = new NpgsqlCommand(query);
            command.Parameters.AddWithValue("@token_hash", tokenHash);
            command.Parameters.AddWithValue("@tipo_token", tipoToken);

            return RepositoryDbHelper.ExecuteReaderSingle(_connectionString, command, MapearUsuarioToken);
        }

        public int MarcarComoUsado(int idUsuarioToken)
        {
            const string query = @"
                UPDATE usuario_token
                SET usado = TRUE
                WHERE id = @id";

            NpgsqlCommand command = new NpgsqlCommand(query);
            command.Parameters.AddWithValue("@id", idUsuarioToken);

            return RepositoryDbHelper.ExecuteNonQuery(_connectionString, command);
        }

        public int RevocarToken(int idUsuarioToken)
        {
            const string query = @"
                UPDATE usuario_token
                SET revocado = TRUE
                WHERE id = @id";

            NpgsqlCommand command = new NpgsqlCommand(query);
            command.Parameters.AddWithValue("@id", idUsuarioToken);

            return RepositoryDbHelper.ExecuteNonQuery(_connectionString, command);
        }

        public int EliminarTokensObsoletos(int dias)
        {
            if (dias <= 0)
                return 0;

            const string query = @"
                DELETE FROM usuario_token
                WHERE fecha_expiracion < CURRENT_TIMESTAMP - (@dias * INTERVAL '1 day')
                   OR (usado = TRUE AND fecha_uso IS NOT NULL AND fecha_uso < CURRENT_TIMESTAMP - (@dias * INTERVAL '1 day'))
                   OR (revocado = TRUE AND fecha_revocacion IS NOT NULL AND fecha_revocacion < CURRENT_TIMESTAMP - (@dias * INTERVAL '1 day'))";

            NpgsqlCommand command = new NpgsqlCommand(query);
            command.Parameters.AddWithValue("@dias", dias);

            return RepositoryDbHelper.ExecuteNonQuery(_connectionString, command);
        }

        private Token MapearUsuarioToken(NpgsqlDataReader reader)
        {
            return new Token
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                TokenHash = reader.GetString(reader.GetOrdinal("token_hash")),
                TipoToken = reader.GetString(reader.GetOrdinal("tipo_token")),
                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("fecha_creacion")),
                FechaExpiracion = reader.GetDateTime(reader.GetOrdinal("fecha_expiracion")),
                Revocado = reader.GetBoolean(reader.GetOrdinal("revocado")),
                Usado = reader.GetBoolean(reader.GetOrdinal("usado")),
                FechaUso = reader.IsDBNull(reader.GetOrdinal("fecha_uso"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("fecha_uso")),
                FechaRevocacion = reader.IsDBNull(reader.GetOrdinal("fecha_revocacion"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("fecha_revocacion"))
            };
        }
    }
}