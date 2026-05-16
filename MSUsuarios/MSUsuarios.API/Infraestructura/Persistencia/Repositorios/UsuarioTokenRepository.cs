using Npgsql;
using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Infraestructura.Persistencia.Conexion;
using MSUsuarios.Infraestructura.Persistencia.Helpers;

namespace MSUsuarios.Infraestructura.Persistencia.Repositorios
{
    public class UsuarioTokenRepository : IUsuarioTokenRepository
    {
        private readonly string _connectionString;

        public UsuarioTokenRepository()
        {
            _connectionString = ConexionStringSingleton.Instancia.CadenaConexion;
        }

        public int Insert(UsuarioToken token)
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
            command.Parameters.AddWithValue("@usuario_id", token.UsuarioIdUsuario);
            command.Parameters.AddWithValue("@token_hash", token.TokenHash);
            command.Parameters.AddWithValue("@tipo_token", token.TipoToken);
            command.Parameters.AddWithValue("@fecha_expiracion", token.FechaExpiracion);

            return RepositoryDbHelper.ExecuteNonQuery(_connectionString, command);
        }

        public UsuarioToken? GetTokenActivo(string tokenHash, string tipoToken)
        {
            const string query = @"
                SELECT id, usuario_id, token_hash, tipo_token, fecha_creacion, fecha_expiracion, 
                       revocado, usado, fecha_uso, fecha_revocacion
                FROM usuario_token
                WHERE token_hash = @token_hash
                  AND tipo_token = @tipo_token
                  AND revocado = 0
                  AND usado = 0
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
                SET usado = 1, fecha_uso = CURRENT_TIMESTAMP
                WHERE id = @id";

            NpgsqlCommand command = new NpgsqlCommand(query);
            command.Parameters.AddWithValue("@id", idUsuarioToken);

            return RepositoryDbHelper.ExecuteNonQuery(_connectionString, command);
        }

        public int RevocarTokensActivos(int idUsuario, string tipoToken)
        {
            const string query = @"
                UPDATE usuario_token
                SET revocado = 1, fecha_revocacion = CURRENT_TIMESTAMP
                WHERE usuario_id = @usuario_id
                  AND tipo_token = @tipo_token
                  AND revocado = 0
                  AND usado = 0
                  AND fecha_expiracion > CURRENT_TIMESTAMP";

            NpgsqlCommand command = new NpgsqlCommand(query);
            command.Parameters.AddWithValue("@usuario_id", idUsuario);
            command.Parameters.AddWithValue("@tipo_token", tipoToken);

            return RepositoryDbHelper.ExecuteNonQuery(_connectionString, command);
        }

        public int EliminarTokensObsoletos(int dias)
        {
            if (dias <= 0)
                return 0;

            const string query = @"
                DELETE FROM usuario_token
                WHERE fecha_expiracion < CURRENT_TIMESTAMP - (@dias * INTERVAL '1 day')
                   OR (usado = 1 AND fecha_uso IS NOT NULL AND fecha_uso < CURRENT_TIMESTAMP - (@dias * INTERVAL '1 day'))
                   OR (revocado = 1 AND fecha_revocacion IS NOT NULL AND fecha_revocacion < CURRENT_TIMESTAMP - (@dias * INTERVAL '1 day'))";

            NpgsqlCommand command = new NpgsqlCommand(query);
            command.Parameters.AddWithValue("@dias", dias);

            return RepositoryDbHelper.ExecuteNonQuery(_connectionString, command);
        }

        private UsuarioToken MapearUsuarioToken(NpgsqlDataReader reader)
        {
            return new UsuarioToken
            {
                IdUsuarioToken = reader.GetInt32(reader.GetOrdinal("id")),
                UsuarioIdUsuario = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                TokenHash = reader.GetString(reader.GetOrdinal("token_hash")),
                TipoToken = reader.GetString(reader.GetOrdinal("tipo_token")),
                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("fecha_creacion")),
                FechaExpiracion = reader.GetDateTime(reader.GetOrdinal("fecha_expiracion")),
                Revocado = (sbyte)(reader.GetBoolean(reader.GetOrdinal("revocado")) ? 1 : 0),
                Usado = (sbyte)(reader.GetBoolean(reader.GetOrdinal("usado")) ? 1 : 0),
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
