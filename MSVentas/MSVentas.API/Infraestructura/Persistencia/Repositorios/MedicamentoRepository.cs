using MySql.Data.MySqlClient;
using MSVentas.Infraestructura.Ayudadores;
using MSVentas.Dominio.Modelos;
using System.Data;
using MSVentas.Dominio.Puertos.PuertoSalida;
using MSVentas.Infraestructura.Persistencia.Conexion;

namespace MSVentas.Infraestructura.Persistencia.Repositorios
{
    public class MedicamentoRepository : IMedicamentoRepository
    {
        private readonly string connectionString;

        public MedicamentoRepository()
        {
            connectionString = ConexionStringSingleton.Instancia.CadenaConexion;
        }

        public int Insert(Medicamento t)
        {
            string query = @"INSERT INTO medicamento
                            (nombre, presentacion, id_clasificacion, concentracion, precio, stock, id_usuario)
                            VALUES
                            (@nombre, @presentacion, @id_clasificacion, @concentracion, @precio, @stock, @id_usuario)";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);

                command.Parameters.AddWithValue("@nombre", t.Nombre);
                command.Parameters.AddWithValue("@presentacion", t.Presentacion);
                command.Parameters.AddWithValue("@id_clasificacion", t.IdClasificacion);
                command.Parameters.AddWithValue("@concentracion", t.Concentracion);
                command.Parameters.AddWithValue("@precio", t.Precio);
                command.Parameters.AddWithValue("@stock", t.Stock);
                command.Parameters.AddWithValue("@id_usuario", t.IdUsuario);

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public int Update(Medicamento t)
        {
            string query = @"UPDATE medicamento
                             SET nombre=@nombre,
                                 presentacion=@presentacion,
                                 id_clasificacion=@id_clasificacion,
                                 concentracion=@concentracion,
                                 precio=@precio,
                                 stock=@stock,
                                 id_usuario=@id_usuario,
                                 ultima_actualizacion = NOW()
                             WHERE id=@id";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);

                command.Parameters.AddWithValue("@id", t.Id);
                command.Parameters.AddWithValue("@nombre", t.Nombre);
                command.Parameters.AddWithValue("@presentacion", t.Presentacion);
                command.Parameters.AddWithValue("@id_clasificacion", t.IdClasificacion);
                command.Parameters.AddWithValue("@concentracion", t.Concentracion);
                command.Parameters.AddWithValue("@precio", t.Precio);
                command.Parameters.AddWithValue("@stock", t.Stock);
                command.Parameters.AddWithValue("@id_usuario", t.IdUsuario);

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public int Delete(Medicamento t)
        {
            string query = @"UPDATE medicamento
                             SET estado = 0,
                                 id_usuario = @id_usuario,
                                 ultima_actualizacion = NOW()
                             WHERE id = @id";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", t.Id);
                command.Parameters.AddWithValue("@id_usuario", t.IdUsuario);

                connection.Open();
                return command.ExecuteNonQuery();
            }
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

        public Medicamento? GetById(int id)
        {
            string query = @"SELECT id, nombre, presentacion, id_clasificacion, concentracion, precio, stock
                             FROM medicamento
                             WHERE id = @id";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Medicamento
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Nombre = StringHelper.LimpiarEspacios(reader["nombre"].ToString()),
                            Presentacion = StringHelper.LimpiarEspacios(reader["presentacion"].ToString()),
                            IdClasificacion = Convert.ToInt32(reader["id_clasificacion"]),
                            Concentracion = StringHelper.LimpiarEspacios(reader["concentracion"].ToString()),
                            Precio = Convert.ToDecimal(reader["precio"]),
                            Stock = Convert.ToInt32(reader["stock"])
                        };
                    }
                }
            }

            return null;
        }

        private string ConstruirQuery(string filtro)
        {
            string query = @"SELECT m.id,
                                    m.nombre,
                                    m.presentacion,
                                    c.nombre AS clasificacion,
                                    m.concentracion,
                                    m.precio,
                                    m.stock
                            FROM medicamento m
                            INNER JOIN clasificacion c 
                                ON m.id_clasificacion = c.id
                            WHERE m.estado = 1";

            query += FiltroSqlHelper.ConstruirCondicionLike(
                filtro,
                "m.nombre",
                "m.presentacion",
                "c.nombre"
            );

            query += " ORDER BY m.nombre";

            return query;
        }

        public int Count()
        {
            string query = "SELECT COUNT(*) FROM medicamento";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                connection.Open();

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
        public DataTable GetDestacados()
        {
            DataTable tabla = new DataTable();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = @"SELECT m.nombre,
                                m.presentacion,
                                c.nombre AS clasificacion,
                                m.concentracion,
                                m.precio
                         FROM medicamento m
                         INNER JOIN clasificacion c
                             ON m.id_clasificacion = c.id
                         WHERE m.estado = 1
                         ORDER BY RAND()
                         LIMIT 3";

                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                adapter.Fill(tabla);
            }

            return tabla;
        }

        public int UpdateStock(int idMedicamento, int cantidad, bool esEntrada, int idUsuario)
        {
            string operacion = esEntrada ? "+" : "-";

            string validacionStock = esEntrada
                ? ""
                : " AND stock >= @cantidad";

            string query = $@"UPDATE medicamento
                            SET stock = stock {operacion} @cantidad,
                                id_usuario = @id_usuario,
                                ultima_actualizacion = NOW()
                            WHERE id = @id
                            AND estado = 1
                            {validacionStock}";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);

                command.Parameters.AddWithValue("@id", idMedicamento);
                command.Parameters.AddWithValue("@cantidad", cantidad);
                command.Parameters.AddWithValue("@id_usuario", idUsuario);

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }
    }
}

