using MySql.Data.MySqlClient;
using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Infraestructura.Ayudadores;
using MSUsuarios.Infraestructura.Persistencia.Conexion;

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
                                       apellido_materno,
                                       apellido_paterno,
                                       ci,
                                       telefono,
                                       activo,
                                       fecha_registro,
                                       ultima_actualizacion,
                                       id_usuario,
                                       ci_extencion,
                                       email,
                                       user_name,
                                       password_hash,
                                       role,
                                       must_change_password
                                   )
                                   VALUES
                                   (
                                       @nombres,
                                       @apellido_materno,
                                       @apellido_paterno,
                                       @ci,
                                       @telefono,
                                       @activo,
                                       @fecha_registro,
                                       @ultima_actualizacion,
                                       @id_usuario,
                                       @ci_extencion,
                                       @email,
                                       @user_name,
                                       @password_hash,
                                       @role,
                                       @must_change_password
                                   )";

            using var conn = new MySqlConnection(_connectionString);
            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@nombres", usuario.Nombres);
            cmd.Parameters.AddWithValue("@apellido_materno", (object?)usuario.ApellidoMaterno ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@apellido_paterno", usuario.ApellidoPaterno);
            cmd.Parameters.AddWithValue("@ci", usuario.Ci);
            cmd.Parameters.AddWithValue("@telefono", usuario.Telefono);
            cmd.Parameters.AddWithValue("@activo", usuario.Activo);
            cmd.Parameters.AddWithValue("@fecha_registro", usuario.FechaRegistro);
            cmd.Parameters.AddWithValue("@ultima_actualizacion", (object?)usuario.UltimaActualizacion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id_usuario", (object?)usuario.IdUsuarioCreador ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ci_extencion", usuario.CiExtencion);
            cmd.Parameters.AddWithValue("@email", usuario.Email);
            cmd.Parameters.AddWithValue("@user_name", usuario.UserName);
            cmd.Parameters.AddWithValue("@password_hash", usuario.PasswordHash);
            cmd.Parameters.AddWithValue("@role", usuario.Role);
            cmd.Parameters.AddWithValue("@must_change_password", usuario.MustChangePassword);

            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public int Update(Usuario usuario)
        {
            return Update(usuario, null);
        }

        public int Update(Usuario usuario, int? idUsuarioSesion)
        {
            const string query = @"UPDATE usuario
                                   SET nombres = @nombres,
                                       apellido_materno = @apellido_materno,
                                       apellido_paterno = @apellido_paterno,
                                       ci = @ci,
                                       telefono = @telefono,
                                       ultima_actualizacion = NOW(),
                                       ci_extencion = @ci_extencion,
                                       email = @email,
                                       user_name = @user_name,
                                       role = @role,
                                       must_change_password = @must_change_password
                                   WHERE id = @id";

            using var conn = new MySqlConnection(_connectionString);
            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", usuario.IdUsuario);
            cmd.Parameters.AddWithValue("@nombres", usuario.Nombres);
            cmd.Parameters.AddWithValue("@apellido_materno", (object?)usuario.ApellidoMaterno ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@apellido_paterno", usuario.ApellidoPaterno);
            cmd.Parameters.AddWithValue("@ci", usuario.Ci);
            cmd.Parameters.AddWithValue("@telefono", usuario.Telefono);
            cmd.Parameters.AddWithValue("@ci_extencion", usuario.CiExtencion);
            cmd.Parameters.AddWithValue("@email", usuario.Email);
            cmd.Parameters.AddWithValue("@user_name", usuario.UserName);
            cmd.Parameters.AddWithValue("@role", usuario.Role);
            cmd.Parameters.AddWithValue("@must_change_password", usuario.MustChangePassword);

            conn.Open();
            return cmd.ExecuteNonQuery();
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
            List<Usuario> usuarios = new List<Usuario>();

            using var conn = new MySqlConnection(_connectionString);
            using var cmd = new MySqlCommand();

            string query = @"SELECT id, nombres, apellido_materno, apellido_paterno, ci, telefono, activo,
                                    fecha_registro, ultima_actualizacion, id_usuario, ci_extencion,
                                    email, user_name, password_hash, role, must_change_password
                             FROM usuario
                             WHERE activo = 1";

            string where = FiltroSqlHelper.ConstruirCondicionLike(
                filtro,
                "nombres",
                "apellido_paterno",
                "apellido_materno",
                "ci",
                "telefono",
                "ci_extencion",
                "email",
                "user_name",
                "role"
            );

            cmd.CommandText = query + where + " ORDER BY nombres, apellido_paterno, apellido_materno";
            cmd.Connection = conn;
            FiltroSqlHelper.AgregarParametrosLike(cmd, filtro);

            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                usuarios.Add(MapearUsuario(reader));
            }

            return usuarios;
        }

        public Usuario? GetById(int id)
        {
            const string query = @"SELECT id, nombres, apellido_materno, apellido_paterno, ci, telefono, activo,
                                          fecha_registro, ultima_actualizacion, id_usuario, ci_extencion,
                                          email, user_name, password_hash, role, must_change_password
                                   FROM usuario
                                   WHERE id = @id
                                   LIMIT 1";

            using var conn = new MySqlConnection(_connectionString);
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapearUsuario(reader) : null;
        }

        public Usuario? GetByEmail(string email)
        {
            const string query = @"SELECT id, nombres, apellido_materno, apellido_paterno, ci, telefono, activo,
                                          fecha_registro, ultima_actualizacion, id_usuario, ci_extencion,
                                          email, user_name, password_hash, role, must_change_password
                                   FROM usuario
                                   WHERE email = @email
                                   LIMIT 1";

            using var conn = new MySqlConnection(_connectionString);
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email.Trim());
            conn.Open();

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapearUsuario(reader) : null;
        }

        public Usuario? GetByUserName(string userName)
        {
            const string query = @"SELECT id, nombres, apellido_materno, apellido_paterno, ci, telefono, activo,
                                          fecha_registro, ultima_actualizacion, id_usuario, ci_extencion,
                                          email, user_name, password_hash, role, must_change_password
                                   FROM usuario
                                   WHERE user_name = @user_name
                                   LIMIT 1";

            using var conn = new MySqlConnection(_connectionString);
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@user_name", userName.Trim());
            conn.Open();

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapearUsuario(reader) : null;
        }

        public bool ExisteEmail(string email)
        {
            const string query = @"SELECT COUNT(1) FROM usuario WHERE email = @email";

            using var conn = new MySqlConnection(_connectionString);
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email.Trim());
            conn.Open();

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public bool ExisteUserName(string userName)
        {
            const string query = @"SELECT COUNT(1) FROM usuario WHERE user_name = @user_name";

            using var conn = new MySqlConnection(_connectionString);
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@user_name", userName.Trim());
            conn.Open();

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public int CambiarPassword(int idUsuario, string nuevoPasswordHash, bool mustChangePassword)
        {
            const string query = @"UPDATE usuario
                                   SET password_hash = @password_hash,
                                       must_change_password = @must_change_password,
                                       ultima_actualizacion = NOW()
                                   WHERE id = @id";

            using var conn = new MySqlConnection(_connectionString);
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", idUsuario);
            cmd.Parameters.AddWithValue("@password_hash", nuevoPasswordHash);
            cmd.Parameters.AddWithValue("@must_change_password", mustChangePassword ? 1 : 0);
            conn.Open();

            return cmd.ExecuteNonQuery();
        }

        public int UpdateDatosEdicion(Usuario usuario, int? idUsuarioSesion)
        {
            return Update(usuario, idUsuarioSesion);
        }

        public int Count()
        {
            const string query = @"SELECT COUNT(1) FROM usuario";

            using var conn = new MySqlConnection(_connectionString);
            using var cmd = new MySqlCommand(query, conn);
            conn.Open();

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public int SoftDelete(Usuario usuario, int? idUsuarioSesion)
        {
            const string query = @"UPDATE usuario
                                   SET activo = 0,
                                       ultima_actualizacion = NOW()
                                   WHERE id = @id";

            using var conn = new MySqlConnection(_connectionString);
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", usuario.IdUsuario);
            conn.Open();

            return cmd.ExecuteNonQuery();
        }

        private static Usuario MapearUsuario(MySqlDataReader reader)
        {
            return new Usuario
            {
                IdUsuario = Convert.ToInt32(reader["id"]),
                Nombres = reader["nombres"]?.ToString() ?? string.Empty,
                ApellidoMaterno = reader["apellido_materno"] == DBNull.Value ? null : reader["apellido_materno"]?.ToString(),
                ApellidoPaterno = reader["apellido_paterno"]?.ToString() ?? string.Empty,
                Ci = reader["ci"]?.ToString() ?? string.Empty,
                Telefono = reader["telefono"]?.ToString() ?? string.Empty,
                Activo = Convert.ToSByte(reader["activo"]),
                FechaRegistro = Convert.ToDateTime(reader["fecha_registro"]),
                UltimaActualizacion = reader["ultima_actualizacion"] == DBNull.Value ? null : Convert.ToDateTime(reader["ultima_actualizacion"]),
                IdUsuarioCreador = reader["id_usuario"] == DBNull.Value ? null : Convert.ToInt32(reader["id_usuario"]),
                CiExtencion = reader["ci_extencion"]?.ToString() ?? string.Empty,
                Email = reader["email"]?.ToString() ?? string.Empty,
                UserName = reader["user_name"]?.ToString() ?? string.Empty,
                PasswordHash = reader["password_hash"]?.ToString() ?? string.Empty,
                Role = reader["role"]?.ToString() ?? string.Empty,
                MustChangePassword = Convert.ToSByte(reader["must_change_password"])
            };
        }
    }
}
