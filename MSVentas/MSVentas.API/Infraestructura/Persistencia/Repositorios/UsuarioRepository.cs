using System.Data;
using MySql.Data.MySqlClient;
using MSVentas.Dominio.Modelos;
using MSVentas.Infraestructura.Ayudadores;
using MSVentas.Dominio.Puertos.PuertoSalida;
using MSVentas.Infraestructura.Persistencia.Conexion;

namespace MSVentas.Infraestructura.Persistencia.Repositorios
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly string connectionString;

        public UsuarioRepository()
        {
            connectionString = ConexionStringSingleton.Instancia.CadenaConexion;
        }

        public int Insert(Usuario t)
        {
            string query = @"INSERT INTO usuario
                            (
                                nombres,
                                apellido_materno,
                                apellido_paterno,
                                ci,
                                telefono,
                                activo,
                                ci_extencion,
                                email,
                                user_name,
                                password_hash,
                                role,
                                must_change_password,
                                id_usuario
                            )
                            VALUES
                            (
                                @nombres,
                                @apellido_materno,
                                @apellido_paterno,
                                @ci,
                                @telefono,
                                @activo,
                                @ci_extencion,
                                @email,
                                @user_name,
                                @password_hash,
                                @role,
                                @must_change_password,
                                @id_usuario
                            )";

            MySqlCommand command = new MySqlCommand(query);
            command.Parameters.AddWithValue("@nombres", t.Nombres);
            command.Parameters.AddWithValue("@apellido_materno", (object?)t.ApellidoMaterno ?? DBNull.Value);
            command.Parameters.AddWithValue("@apellido_paterno", t.ApellidoPaterno);
            command.Parameters.AddWithValue("@ci", t.Ci);
            command.Parameters.AddWithValue("@telefono", t.Telefono);
            command.Parameters.AddWithValue("@activo", t.Activo);
            command.Parameters.AddWithValue("@ci_extencion", t.CiExtencion);
            command.Parameters.AddWithValue("@email", t.Email);
            command.Parameters.AddWithValue("@user_name", t.UserName);
            command.Parameters.AddWithValue("@password_hash", t.PasswordHash);
            command.Parameters.AddWithValue("@role", t.Role);
            command.Parameters.AddWithValue("@must_change_password", t.MustChangePassword);
            command.Parameters.AddWithValue("@id_usuario", t.IdUsuarioCreador);

            return RepositoryDbHelper.ExecuteNonQuery(connectionString, command);
        }

        public int Update(Usuario t)
        {
            return Update(t, null);
        } 

        public int Delete(Usuario usuario)
        {
            return SoftDelete(usuario, null);
        }

        public Usuario? GetById(int id)
        {
            string query = @"SELECT *
                             FROM usuario
                             WHERE id = @id";

            MySqlCommand command = new MySqlCommand(query);
            command.Parameters.AddWithValue("@id", id);

            return RepositoryDbHelper.ExecuteReaderSingle(connectionString, command, MapearUsuario);
        }

        public Usuario? GetByEmail(string email)
        {
            string query = @"SELECT *
                             FROM usuario
                             WHERE email = @email
                             LIMIT 1";

            MySqlCommand command = new MySqlCommand(query);
            command.Parameters.AddWithValue("@email", email);

            return RepositoryDbHelper.ExecuteReaderSingle(connectionString, command, MapearUsuario);
        }

        public Usuario? GetByUserName(string userName)
        {
            string query = @"SELECT *
                             FROM usuario
                             WHERE user_name = @user_name
                             LIMIT 1";

            MySqlCommand command = new MySqlCommand(query);
            command.Parameters.AddWithValue("@user_name", userName);

            return RepositoryDbHelper.ExecuteReaderSingle(connectionString, command, MapearUsuario);
        }

        public bool ExisteEmail(string email)
        {
            string query = @"SELECT COUNT(*)
                             FROM usuario
                             WHERE email = @email";

            MySqlCommand command = new MySqlCommand(query);
            command.Parameters.AddWithValue("@email", email);

            var result = RepositoryDbHelper.ExecuteScalar(connectionString, command);
            return Convert.ToInt32(result) > 0;
        }

        public bool ExisteUserName(string userName)
        {
            string query = @"SELECT COUNT(*)
                             FROM usuario
                             WHERE user_name = @user_name";

            MySqlCommand command = new MySqlCommand(query);
            command.Parameters.AddWithValue("@user_name", userName);

            var result = RepositoryDbHelper.ExecuteScalar(connectionString, command);
            return Convert.ToInt32(result) > 0;
        }

        public int CambiarPassword(int idUsuario, string nuevoPasswordHash, bool mustChangePassword)
        {
            string query = @"UPDATE usuario
                             SET password_hash = @password_hash,
                                 must_change_password = @must_change_password,
                                 ultima_actualizacion = NOW()
                             WHERE id = @id";

            MySqlCommand command = new MySqlCommand(query);
            command.Parameters.AddWithValue("@id", idUsuario);
            command.Parameters.AddWithValue("@password_hash", nuevoPasswordHash);
            command.Parameters.AddWithValue("@must_change_password", mustChangePassword);

            return RepositoryDbHelper.ExecuteNonQuery(connectionString, command);
        }

        public DataTable GetAll()
        {
            return GetAll(string.Empty);
        }

        public DataTable GetAll(string filtro)
        {
            DataTable tabla = new DataTable();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = ConstruirQuery(filtro);
                MySqlCommand command = new MySqlCommand(query, connection);

                FiltroSqlHelper.AgregarParametrosLike(command, filtro);

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                adapter.Fill(tabla);
            }

            return tabla;
        }

        private string ConstruirQuery(string filtro)
        {
            string query = @"SELECT id,
                                    nombres,
                                    apellido_paterno,
                                    apellido_materno,
                                    ci,
                                    telefono,
                                    ci_extencion,
                                    email,
                                    user_name,
                                    role,
                                    activo
                             FROM usuario
                             WHERE activo = 1";

            query += FiltroSqlHelper.ConstruirCondicionLike(
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

            query += " ORDER BY nombres, apellido_paterno, apellido_materno";

            return query;
        }

        private Usuario MapearUsuario(MySqlDataReader reader)
        {
            return new Usuario
            {
                IdUsuario = reader.GetInt32("id"),
                Nombres = reader.GetString("nombres"),
                ApellidoMaterno = reader.IsDBNull(reader.GetOrdinal("apellido_materno"))
                    ? null
                    : reader.GetString("apellido_materno"),
                ApellidoPaterno = reader.GetString("apellido_paterno"),
                Ci = reader.GetString("ci"),
                Telefono = reader.GetString("telefono"),
                Activo = reader.GetSByte("activo"),
                FechaRegistro = reader.GetDateTime("fecha_registro"),
                UltimaActualizacion = reader.IsDBNull(reader.GetOrdinal("ultima_actualizacion"))
                    ? (DateTime?)null
                    : reader.GetDateTime("ultima_actualizacion"),
                IdUsuarioCreador = reader.IsDBNull(reader.GetOrdinal("id_usuario"))
                    ? (int?)null
                    : reader.GetInt32("id_usuario"),
                CiExtencion = reader.GetString("ci_extencion"),
                Email = reader.GetString("email"),
                UserName = reader.GetString("user_name"),
                PasswordHash = reader.GetString("password_hash"),
                Role = reader.GetString("role"),
                MustChangePassword = reader.GetSByte("must_change_password")
            };
        }

        public int UpdateDatosEdicion(Usuario usuario, int? idUsuarioSesion)
        {
            string query = @"UPDATE usuario
                            SET
                                email = @email,
                                user_name = @user_name,
                                role = @role,
                                activo = @activo,
                                Id_usuario = @idUsuarioSesion,
                                ultima_actualizacion = NOW()
                             WHERE id = @id";
                            

            using MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            using MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@idUsuarioSesion", idUsuarioSesion);
            command.Parameters.AddWithValue("@email", usuario.Email);
            command.Parameters.AddWithValue("@user_name", usuario.UserName);
            command.Parameters.AddWithValue("@role", usuario.Role);
            command.Parameters.AddWithValue("@activo", usuario.Activo);
            command.Parameters.AddWithValue("@id", usuario.IdUsuario);

            return command.ExecuteNonQuery();
        }
        public int SoftDelete(Usuario usuario, int? idUsuarioSesion)
        {
            string query = @"UPDATE usuario
                            SET activo = 0,
                                ultima_actualizacion = NOW(),
                                id_usuario = @idUsuarioSesion
                            WHERE id = @id";

            MySqlCommand command = new MySqlCommand(query);
            command.Parameters.AddWithValue("@idUsuarioSesion", idUsuarioSesion);
            command.Parameters.AddWithValue("@id", usuario.IdUsuario);

            return RepositoryDbHelper.ExecuteNonQuery(connectionString, command);
        }

        public int Update(Usuario usuario, int? idUsuarioSesion)
        {
            string query = @"UPDATE usuario
                            SET nombres = @nombres,
                                apellido_materno = @apellido_materno,
                                apellido_paterno = @apellido_paterno,
                                ci = @ci,
                                telefono = @telefono,
                                ci_extencion = @ci_extencion,
                                email = @email,
                                user_name = @user_name,
                                role = @role,
                                activo = @activo,
                                id_usuario = @idUsuarioSesion,
                                ultima_actualizacion = NOW()
                            WHERE id = @id";

            using MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            using MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@nombres", usuario.Nombres);
            command.Parameters.AddWithValue("@apellido_materno", usuario.ApellidoMaterno);
            command.Parameters.AddWithValue("@apellido_paterno", usuario.ApellidoPaterno);
            command.Parameters.AddWithValue("@ci", usuario.Ci);
            command.Parameters.AddWithValue("@telefono", usuario.Telefono);
            command.Parameters.AddWithValue("@ci_extencion", usuario.CiExtencion);
            command.Parameters.AddWithValue("@email", usuario.Email);
            command.Parameters.AddWithValue("@user_name", usuario.UserName);
            command.Parameters.AddWithValue("@role", usuario.Role);
            command.Parameters.AddWithValue("@activo", usuario.Activo);
            command.Parameters.AddWithValue("@idUsuarioSesion", idUsuarioSesion);
            command.Parameters.AddWithValue("@id", usuario.IdUsuario);

            return command.ExecuteNonQuery();
        }
    
        public int Count()
        {
            string query = "SELECT COUNT(*) FROM usuario";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                connection.Open();

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }
}

