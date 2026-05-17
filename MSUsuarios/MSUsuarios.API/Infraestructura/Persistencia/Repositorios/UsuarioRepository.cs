using Npgsql;
using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Infraestructura.Ayudadores;
using MSUsuarios.Infraestructura.Persistencia.Conexion;
using MSUsuarios.Infraestructura.Persistencia.Helpers;

namespace MSUsuarios.Infraestructura.Persistencia.Repositorios
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository()
        {
            _connectionString = ConexionStringSingleton.Instancia.CadenaConexion;
        }

        public int Insert(Usuario usuario)
        {
            const string query = @"INSERT INTO usuario
                                   (
                                       nombres,
                                       primer_apellido,
                                       segundo_apellido,
                                       ci,
                                       ci_extension,
                                       telefono,
                                       email,
                                       user_name,
                                       password_hash,
                                       role,
                                       must_change_password,
                                       activo,
                                       fecha_registro,
                                       ultima_actualizacion,
                                       usuario_auditoria_id
                                   )
                                   VALUES
                                   (
                                       @nombres,
                                       @primer_apellido,
                                       @segundo_apellido,
                                       @ci,
                                       @ci_extension,
                                       @telefono,
                                       @email,
                                       @user_name,
                                       @password_hash,
                                       @role,
                                       @must_change_password,
                                       @activo,
                                       @fecha_registro,
                                       @ultima_actualizacion,
                                       @usuario_auditoria_id
                                   )";

            NpgsqlCommand cmd = new NpgsqlCommand(query);

            cmd.Parameters.AddWithValue("@nombres", usuario.Nombres);
            cmd.Parameters.AddWithValue("@primer_apellido", usuario.ApellidoPaterno);
            cmd.Parameters.AddWithValue("@segundo_apellido", (object?)usuario.ApellidoMaterno ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ci", usuario.Ci);
            cmd.Parameters.AddWithValue("@ci_extension", usuario.CiExtencion);
            cmd.Parameters.AddWithValue("@telefono", usuario.Telefono);
            cmd.Parameters.AddWithValue("@email", usuario.Email);
            cmd.Parameters.AddWithValue("@user_name", usuario.UserName);
            cmd.Parameters.AddWithValue("@password_hash", usuario.PasswordHash);
            cmd.Parameters.AddWithValue("@role", usuario.Role);
            cmd.Parameters.AddWithValue("@must_change_password", usuario.MustChangePassword == 1);
            cmd.Parameters.AddWithValue("@activo", usuario.Activo == 1);
            cmd.Parameters.AddWithValue("@fecha_registro", usuario.FechaRegistro);
            cmd.Parameters.AddWithValue("@ultima_actualizacion", (object?)usuario.UltimaActualizacion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@usuario_auditoria_id", (object?)usuario.IdUsuarioCreador ?? DBNull.Value);

            return RepositoryDbHelper.ExecuteNonQuery(_connectionString, cmd);
        }

        public int Update(Usuario usuario)
        {
            return Update(usuario, null);
        }

        public int Update(Usuario usuario, int? idUsuarioSesion)
        {
            const string query = @"UPDATE usuario
                                   SET nombres = @nombres,
                                       primer_apellido = @primer_apellido,
                                       segundo_apellido = @segundo_apellido,
                                       ci = @ci,
                                       ci_extension = @ci_extension,
                                       telefono = @telefono,
                                       email = @email,
                                       user_name = @user_name,
                                       role = @role,
                                       must_change_password = @must_change_password,
                                       ultima_actualizacion = CURRENT_TIMESTAMP,
                                       usuario_auditoria_id = @usuario_auditoria_id
                                   WHERE id = @id";

            NpgsqlCommand cmd = new NpgsqlCommand(query);

            cmd.Parameters.AddWithValue("@id", usuario.IdUsuario);
            cmd.Parameters.AddWithValue("@nombres", usuario.Nombres);
            cmd.Parameters.AddWithValue("@primer_apellido", usuario.ApellidoPaterno);
            cmd.Parameters.AddWithValue("@segundo_apellido", (object?)usuario.ApellidoMaterno ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ci", usuario.Ci);
            cmd.Parameters.AddWithValue("@ci_extension", usuario.CiExtencion);
            cmd.Parameters.AddWithValue("@telefono", usuario.Telefono);
            cmd.Parameters.AddWithValue("@email", usuario.Email);
            cmd.Parameters.AddWithValue("@user_name", usuario.UserName);
            cmd.Parameters.AddWithValue("@role", usuario.Role);
            cmd.Parameters.AddWithValue("@must_change_password", usuario.MustChangePassword == 1);
            cmd.Parameters.AddWithValue("@usuario_auditoria_id", (object?)idUsuarioSesion ?? DBNull.Value);

            return RepositoryDbHelper.ExecuteNonQuery(_connectionString, cmd);
        }

        public int Delete(Usuario usuario)
        {
            return SoftDelete(usuario, null);
        }

        public IEnumerable<Usuario> GetAll()
        {
            return GetAll(string.Empty);
        }

        public IEnumerable<Usuario> GetAll(string filtro)
        {
            string query = @"SELECT id, nombres, primer_apellido, segundo_apellido, ci, ci_extension,
                                    telefono, email, user_name, password_hash, role,
                                    must_change_password, activo, fecha_registro,
                                    ultima_actualizacion, usuario_auditoria_id
                             FROM usuario
                             WHERE activo = TRUE";

            string where = FiltroSqlHelper.ConstruirCondicionLike(
                filtro,
                "nombres",
                "primer_apellido",
                "segundo_apellido",
                "ci",
                "telefono",
                "ci_extension",
                "email",
                "user_name",
                "role"
            );

            NpgsqlCommand cmd = new NpgsqlCommand(query + where + " ORDER BY nombres, primer_apellido, segundo_apellido");
            FiltroSqlHelper.AgregarParametrosLike(cmd, filtro);

            using var reader = RepositoryDbHelper.ExecuteReader(_connectionString, cmd.CommandText, cmd.Parameters.Cast<NpgsqlParameter>().ToArray());
            var usuarios = new List<Usuario>();
            while (reader.Read())
            {
                usuarios.Add(MapearUsuario(reader));
            }
            return usuarios;
        }

        public Usuario? GetById(int id)
        {
            const string query = @"SELECT id, nombres, primer_apellido, segundo_apellido, ci, ci_extension,
                                          telefono, email, user_name, password_hash, role,
                                          must_change_password, activo, fecha_registro,
                                          ultima_actualizacion, usuario_auditoria_id
                                   FROM usuario
                                   WHERE id = @id
                                   LIMIT 1";

            NpgsqlCommand cmd = new NpgsqlCommand(query);
            cmd.Parameters.AddWithValue("@id", id);

            return RepositoryDbHelper.ExecuteReaderSingle(_connectionString, cmd, MapearUsuario);
        }

        public Usuario? GetByEmail(string email)
        {
            const string query = @"SELECT id, nombres, primer_apellido, segundo_apellido, ci, ci_extension,
                                          telefono, email, user_name, password_hash, role,
                                          must_change_password, activo, fecha_registro,
                                          ultima_actualizacion, usuario_auditoria_id
                                   FROM usuario
                                   WHERE email = @email
                                   LIMIT 1";

            NpgsqlCommand cmd = new NpgsqlCommand(query);
            cmd.Parameters.AddWithValue("@email", email.Trim());

            return RepositoryDbHelper.ExecuteReaderSingle(_connectionString, cmd, MapearUsuario);
        }

        public Usuario? GetByUserName(string userName)
        {
            const string query = @"SELECT id, nombres, primer_apellido, segundo_apellido, ci, ci_extension,
                                          telefono, email, user_name, password_hash, role,
                                          must_change_password, activo, fecha_registro,
                                          ultima_actualizacion, usuario_auditoria_id
                                   FROM usuario
                                   WHERE user_name = @user_name
                                   LIMIT 1";

            NpgsqlCommand cmd = new NpgsqlCommand(query);
            cmd.Parameters.AddWithValue("@user_name", userName.Trim());

            return RepositoryDbHelper.ExecuteReaderSingle(_connectionString, cmd, MapearUsuario);
        }

        public bool ExisteEmail(string email)
        {
            const string query = @"SELECT COUNT(1) FROM usuario WHERE email = @email";

            NpgsqlCommand cmd = new NpgsqlCommand(query);
            cmd.Parameters.AddWithValue("@email", email.Trim());

            var resultado = RepositoryDbHelper.ExecuteScalar(_connectionString, cmd);
            return resultado != null && Convert.ToInt32(resultado) > 0;
        }

        public bool ExisteUserName(string userName)
        {
            const string query = @"SELECT COUNT(1) FROM usuario WHERE user_name = @user_name";

            NpgsqlCommand cmd = new NpgsqlCommand(query);
            cmd.Parameters.AddWithValue("@user_name", userName.Trim());

            var resultado = RepositoryDbHelper.ExecuteScalar(_connectionString, cmd);
            return resultado != null && Convert.ToInt32(resultado) > 0;
        }

        public int CambiarPassword(int idUsuario, string nuevoPasswordHash, bool mustChangePassword)
        {
            const string query = @"UPDATE usuario
                                   SET password_hash = @password_hash,
                                       must_change_password = @must_change_password,
                                       ultima_actualizacion = CURRENT_TIMESTAMP
                                   WHERE id = @id";

            NpgsqlCommand cmd = new NpgsqlCommand(query);
            cmd.Parameters.AddWithValue("@id", idUsuario);
            cmd.Parameters.AddWithValue("@password_hash", nuevoPasswordHash);
            cmd.Parameters.AddWithValue("@must_change_password", mustChangePassword);

            return RepositoryDbHelper.ExecuteNonQuery(_connectionString, cmd);
        }

        public int UpdateDatosEdicion(Usuario usuario, int? idUsuarioSesion)
        {
            return Update(usuario, idUsuarioSesion);
        }

        public int Count()
        {
            const string query = @"SELECT COUNT(1) FROM usuario";

            NpgsqlCommand cmd = new NpgsqlCommand(query);

            var resultado = RepositoryDbHelper.ExecuteScalar(_connectionString, cmd);
            return resultado != null ? Convert.ToInt32(resultado) : 0;
        }

        public int SoftDelete(Usuario usuario, int? idUsuarioSesion)
        {
            const string query = @"UPDATE usuario
                                   SET activo = FALSE,
                                       ultima_actualizacion = CURRENT_TIMESTAMP,
                                       usuario_auditoria_id = @usuario_auditoria_id
                                   WHERE id = @id";

            NpgsqlCommand cmd = new NpgsqlCommand(query);
            cmd.Parameters.AddWithValue("@id", usuario.IdUsuario);
            cmd.Parameters.AddWithValue("@usuario_auditoria_id", (object?)idUsuarioSesion ?? DBNull.Value);

            return RepositoryDbHelper.ExecuteNonQuery(_connectionString, cmd);
        }

        private static Usuario MapearUsuario(NpgsqlDataReader reader)
        {
            return new Usuario
            {
                IdUsuario = Convert.ToInt32(reader["id"]),
                Nombres = reader["nombres"]?.ToString() ?? string.Empty,
                ApellidoPaterno = reader["primer_apellido"]?.ToString() ?? string.Empty,
                ApellidoMaterno = reader["segundo_apellido"] == DBNull.Value ? null : reader["segundo_apellido"]?.ToString(),
                Ci = reader["ci"]?.ToString() ?? string.Empty,
                CiExtencion = reader["ci_extension"]?.ToString() ?? string.Empty,
                Telefono = reader["telefono"]?.ToString() ?? string.Empty,
                Email = reader["email"]?.ToString() ?? string.Empty,
                UserName = reader["user_name"]?.ToString() ?? string.Empty,
                PasswordHash = reader["password_hash"]?.ToString() ?? string.Empty,
                Role = reader["role"]?.ToString() ?? string.Empty,
                MustChangePassword = Convert.ToBoolean(reader["must_change_password"]) ? (sbyte)1 : (sbyte)0,
                Activo = Convert.ToBoolean(reader["activo"]) ? (sbyte)1 : (sbyte)0,
                FechaRegistro = Convert.ToDateTime(reader["fecha_registro"]),
                UltimaActualizacion = reader["ultima_actualizacion"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["ultima_actualizacion"]),
                IdUsuarioCreador = reader["usuario_auditoria_id"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["usuario_auditoria_id"])
            };
        }
    }
}