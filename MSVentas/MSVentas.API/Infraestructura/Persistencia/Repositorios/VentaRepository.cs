using MySql.Data.MySqlClient;
using MSVentas.Dominio.Puertos.PuertoSalida;
using MSVentas.Dominio.Validadores;
using MSVentas.Dominio.Modelos;
using MSVentas.Infraestructura.Ayudadores;
using MSVentas.Infraestructura.Persistencia.Conexion;
using System.Data;

namespace MSVentas.Infraestructura.Persistencia.Repositorios
{
    public class VentaRepository : IVentaRepository
    {
        private readonly string connectionString;

        public VentaRepository()
        {
            connectionString = ConexionStringSingleton.Instancia.CadenaConexion;
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

                string query = @"SELECT v.id,
                                        v.fecha_hora,
                                        v.total,
                                        v.metodo_pago,
                                        CONCAT(v.nit, ' - ', v.razon_social) AS cliente,
                                        v.razon_social,
                                        v.nit,
                                        u.user_name AS usuario
                                 FROM venta v
                                 INNER JOIN usuario u ON v.usuario_idUsuario = u.id
                                 WHERE v.estado = 1";

                query += FiltroSqlHelper.ConstruirCondicionLike(
                    filtro,
                    "v.metodo_pago",
                    "v.razon_social",
                    "v.nit",
                    "u.user_name"
                );

                query += " ORDER BY v.fecha_hora DESC";

                MySqlCommand command = new MySqlCommand(query, connection);
                FiltroSqlHelper.AgregarParametrosLike(command, filtro);

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                adapter.Fill(tabla);
            }

            return tabla;
        }

        public Venta? GetById(int id)
        {
            string query = @"SELECT id,
                                    fecha_hora,
                                    total,
                                    metodo_pago,
                                    Cliente_idCliente,
                                    usuario_idUsuario,
                                    estado,
                                    fecha_registro,
                                    ultima_actualizacion,
                                    Id_usuario_editor,
                                    nit,
                                    razon_social
                             FROM venta
                             WHERE id = @id";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new Venta
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        FechaHora = Convert.ToDateTime(reader["fecha_hora"]),
                        Total = Convert.ToDecimal(reader["total"]),
                        MetodoPago = StringHelper.LimpiarEspacios(reader["metodo_pago"]?.ToString()),
                        IdCliente = Convert.ToInt32(reader["Cliente_idCliente"]),
                        IdUsuario = Convert.ToInt32(reader["usuario_idUsuario"]),
                        Estado = Convert.ToInt16(reader["estado"]),
                        FechaRegistro = Convert.ToDateTime(reader["fecha_registro"]),
                        UltimaActualizacion = reader["ultima_actualizacion"] == DBNull.Value
                            ? null
                            : Convert.ToDateTime(reader["ultima_actualizacion"]),
                        IdUsuarioEditor = reader["Id_usuario_editor"] == DBNull.Value
                            ? null
                            : Convert.ToInt32(reader["Id_usuario_editor"]),
                        Nit = StringHelper.LimpiarEspacios(reader["nit"]?.ToString()),
                        RazonSocial = StringHelper.LimpiarEspacios(reader["razon_social"]?.ToString())
                    };
                }
            }
        }

        public List<DetalleVenta> GetDetallesByVentaId(int idVenta)
        {
            List<DetalleVenta> detalles = new();

            string query = @"SELECT cantidad,
                                    precio_unitario,
                                    id_venta,
                                    id_medicamento
                             FROM detalle_venta
                             WHERE id_venta = @id_venta
                             ORDER BY id_medicamento";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id_venta", idVenta);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        detalles.Add(new DetalleVenta
                        {
                            Cantidad = Convert.ToInt32(reader["cantidad"]),
                            PrecioUnitario = Convert.ToDecimal(reader["precio_unitario"]),
                            IdVenta = Convert.ToInt32(reader["id_venta"]),
                            IdMedicamento = Convert.ToInt32(reader["id_medicamento"])
                        });
                    }
                }
            }

            return detalles;
        }

        public Result RegistrarVenta(Venta venta)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    Result validacionDb = ValidarVentaParaRegistro(connection, transaction, venta);
                    if (validacionDb.IsSuccess == false)
                    {
                        transaction.Rollback();
                        return validacionDb;
                    }

                    string queryCliente = @"SELECT nit, razon_social
                                            FROM cliente
                                            WHERE id = @idCliente AND estado = 1";

                    MySqlCommand commandCliente = new MySqlCommand(queryCliente, connection, transaction);
                    commandCliente.Parameters.AddWithValue("@idCliente", venta.IdCliente);

                    using (MySqlDataReader reader = commandCliente.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            transaction.Rollback();
                            return Result.Fail("No se pudo obtener los datos del cliente.");
                        }

                        venta.Nit = reader["nit"].ToString()!;
                        venta.RazonSocial = reader["razon_social"].ToString()!;
                    }

                    string queryVenta = @"INSERT INTO venta
                                        (total, metodo_pago, Cliente_idCliente, usuario_idUsuario, nit, razon_social)
                                        VALUES
                                        (@total, @metodo_pago, @idCliente, @idUsuario, @nit, @razon_social)";

                    MySqlCommand commandVenta = new MySqlCommand(queryVenta, connection, transaction);
                    commandVenta.Parameters.AddWithValue("@total", venta.Total);
                    commandVenta.Parameters.AddWithValue("@metodo_pago", venta.MetodoPago);
                    commandVenta.Parameters.AddWithValue("@idCliente", venta.IdCliente);
                    commandVenta.Parameters.AddWithValue("@idUsuario", venta.IdUsuario);
                    commandVenta.Parameters.AddWithValue("@nit", venta.Nit);
                    commandVenta.Parameters.AddWithValue("@razon_social", venta.RazonSocial);

                    commandVenta.ExecuteNonQuery();
                    int idVenta = Convert.ToInt32(commandVenta.LastInsertedId);

                    foreach (DetalleVenta detalle in venta.Detalles)
                    {
                        Result resultadoStock = DescontarStock(connection, transaction, detalle.IdMedicamento, detalle.Cantidad);
                        if (resultadoStock.IsSuccess == false)
                        {
                            transaction.Rollback();
                            return resultadoStock;
                        }

                        string queryDetalle = @"INSERT INTO detalle_venta
                                                (cantidad, precio_unitario, id_venta, id_medicamento)
                                                VALUES
                                                (@cantidad, @precio_unitario, @id_venta, @id_medicamento)";

                        MySqlCommand commandDetalle = new MySqlCommand(queryDetalle, connection, transaction);
                        commandDetalle.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                        commandDetalle.Parameters.AddWithValue("@precio_unitario", detalle.PrecioUnitario);
                        commandDetalle.Parameters.AddWithValue("@id_venta", idVenta);
                        commandDetalle.Parameters.AddWithValue("@id_medicamento", detalle.IdMedicamento);

                        commandDetalle.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return Result.Ok();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Result.Fail($"No se pudo registrar la venta. {ex.Message}");
                }
            }
        }

        public Result ActualizarVenta(Venta venta)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    Result estadoVenta = ValidarVentaEditable(connection, transaction, venta.Id);
                    if (estadoVenta.IsSuccess == false)
                    {
                        transaction.Rollback();
                        return estadoVenta;
                    }

                    Result validacionBase = ValidarVentaParaActualizacion(connection, transaction, venta);
                    if (validacionBase.IsSuccess == false)
                    {
                        transaction.Rollback();
                        return validacionBase;
                    }   

                    string queryCliente = @"SELECT nit, razon_social
                                    FROM cliente
                                    WHERE id = @idCliente AND estado = 1";

                    MySqlCommand commandCliente = new MySqlCommand(queryCliente, connection, transaction);
                    commandCliente.Parameters.AddWithValue("@idCliente", venta.IdCliente);

                    using (MySqlDataReader reader = commandCliente.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            transaction.Rollback();
                            return Result.Fail("No se pudo obtener los datos del cliente.");
                        }

                        venta.Nit = reader["nit"].ToString()!;
                        venta.RazonSocial = reader["razon_social"].ToString()!;
                    }

                    List<DetalleVenta> detallesActuales = GetDetallesByVentaIdTransaccional(connection, transaction, venta.Id);

                    foreach (DetalleVenta detalleActual in detallesActuales)
                    {
                        string queryRestore = @"UPDATE medicamento
                                                SET stock = stock + @cantidad
                                                WHERE id = @id_medicamento";

                        MySqlCommand commandRestore = new MySqlCommand(queryRestore, connection, transaction);
                        commandRestore.Parameters.AddWithValue("@cantidad", detalleActual.Cantidad);
                        commandRestore.Parameters.AddWithValue("@id_medicamento", detalleActual.IdMedicamento);
                        commandRestore.ExecuteNonQuery();
                    }

                    Result validacionStock = ValidarMedicamentosYStock(connection, transaction, venta.Detalles);
                    if (validacionStock.IsSuccess == false)
                    {
                        transaction.Rollback();
                        return validacionStock;
                    }

                    string queryDeleteDetalles = @"DELETE FROM detalle_venta
                                                  WHERE id_venta = @id_venta";

                    MySqlCommand commandDeleteDetalles = new MySqlCommand(queryDeleteDetalles, connection, transaction);
                    commandDeleteDetalles.Parameters.AddWithValue("@id_venta", venta.Id);
                    commandDeleteDetalles.ExecuteNonQuery();

                    string queryUpdateVenta = @"UPDATE venta
                                                SET total = @total,
                                                    metodo_pago = @metodo_pago,
                                                    Cliente_idCliente = @idCliente,
                                                    nit = @nit,
                                                    razon_social = @razon_social,
                                                    ultima_actualizacion = NOW(),
                                                    Id_usuario_editor = @id_usuario_editor
                                                WHERE id = @id
                                                  AND estado = 1";

                    MySqlCommand commandUpdateVenta = new MySqlCommand(queryUpdateVenta, connection, transaction);
                    commandUpdateVenta.Parameters.AddWithValue("@total", venta.Total);
                    commandUpdateVenta.Parameters.AddWithValue("@metodo_pago", venta.MetodoPago);
                    commandUpdateVenta.Parameters.AddWithValue("@idCliente", venta.IdCliente);
                    commandUpdateVenta.Parameters.AddWithValue("@id_usuario_editor", venta.IdUsuarioEditor);
                    commandUpdateVenta.Parameters.AddWithValue("@id", venta.Id);
                    commandUpdateVenta.Parameters.AddWithValue("@nit", venta.Nit);
                    commandUpdateVenta.Parameters.AddWithValue("@razon_social", venta.RazonSocial);

                    int filasVenta = commandUpdateVenta.ExecuteNonQuery();
                    if (filasVenta <= 0)
                    {
                        transaction.Rollback();
                        return Result.Fail("No se pudo actualizar la venta.");
                    }

                    foreach (DetalleVenta detalle in venta.Detalles)
                    {
                        Result resultadoStock = DescontarStock(connection, transaction, detalle.IdMedicamento, detalle.Cantidad);
                        if (resultadoStock.IsSuccess == false)
                        {
                            transaction.Rollback();
                            return resultadoStock;
                        }

                        string queryDetalle = @"INSERT INTO detalle_venta
                                                (cantidad, precio_unitario, id_venta, id_medicamento)
                                                VALUES
                                                (@cantidad, @precio_unitario, @id_venta, @id_medicamento)";

                        MySqlCommand commandDetalle = new MySqlCommand(queryDetalle, connection, transaction);
                        commandDetalle.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                        commandDetalle.Parameters.AddWithValue("@precio_unitario", detalle.PrecioUnitario);
                        commandDetalle.Parameters.AddWithValue("@id_venta", venta.Id);
                        commandDetalle.Parameters.AddWithValue("@id_medicamento", detalle.IdMedicamento);
                        commandDetalle.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return Result.Ok();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Result.Fail($"No se pudo actualizar la venta. {ex.Message}");
                }
            }
        }

        public Result AnularVentaLogicamente(int idVenta, int idUsuarioEditor)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    string queryEstado = @"SELECT estado
                                           FROM venta
                                           WHERE id = @id";

                    MySqlCommand commandEstado = new MySqlCommand(queryEstado, connection, transaction);
                    commandEstado.Parameters.AddWithValue("@id", idVenta);

                    object? estadoObj = commandEstado.ExecuteScalar();

                    if (estadoObj == null)
                    {
                        transaction.Rollback();
                        return Result.Fail("La venta no existe.");
                    }

                    int estado = Convert.ToInt32(estadoObj);
                    if (estado == 0)
                    {
                        transaction.Rollback();
                        return Result.Fail("La venta ya está anulada lógicamente.");
                    }

                    if (!UsuarioActivo(connection, transaction, idUsuarioEditor))
                    {
                        transaction.Rollback();
                        return Result.Fail("El usuario editor no existe o está inactivo.");
                    }

                    List<DetalleVenta> detalles = GetDetallesByVentaIdTransaccional(connection, transaction, idVenta);

                    foreach (DetalleVenta detalle in detalles)
                    {
                        string queryRestore = @"UPDATE medicamento
                                                SET stock = stock + @cantidad
                                                WHERE id = @id_medicamento";

                        MySqlCommand commandRestore = new MySqlCommand(queryRestore, connection, transaction);
                        commandRestore.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                        commandRestore.Parameters.AddWithValue("@id_medicamento", detalle.IdMedicamento);
                        commandRestore.ExecuteNonQuery();
                    }

                    string queryAnular = @"UPDATE venta
                                           SET estado = 0,
                                               ultima_actualizacion = NOW(),
                                               Id_usuario_editor = @id_usuario_editor
                                           WHERE id = @id
                                             AND estado = 1";

                    MySqlCommand commandAnular = new MySqlCommand(queryAnular, connection, transaction);
                    commandAnular.Parameters.AddWithValue("@id_usuario_editor", idUsuarioEditor);
                    commandAnular.Parameters.AddWithValue("@id", idVenta);

                    int filas = commandAnular.ExecuteNonQuery();
                    if (filas <= 0)
                    {
                        transaction.Rollback();
                        return Result.Fail("No se pudo anular la venta.");
                    }

                    transaction.Commit();
                    return Result.Ok();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Result.Fail($"No se pudo anular la venta. {ex.Message}");
                }
            }
        }

        private Result ValidarVentaParaRegistro(MySqlConnection connection, MySqlTransaction transaction, Venta venta)
        {
            if (!ClienteActivo(connection, transaction, venta.IdCliente))
                return Result.Fail("El cliente no existe o está inactivo.");

            if (!UsuarioActivo(connection, transaction, venta.IdUsuario))
                return Result.Fail("El usuario que registra la venta no existe o está inactivo.");

            return ValidarMedicamentosYStock(connection, transaction, venta.Detalles);
        }

        private Result ValidarVentaParaActualizacion(MySqlConnection connection, MySqlTransaction transaction, Venta venta)
        {
            if (!ClienteActivo(connection, transaction, venta.IdCliente))
                return Result.Fail("El cliente no existe o está inactivo.");

            if (venta.IdUsuarioEditor == null || !UsuarioActivo(connection, transaction, venta.IdUsuarioEditor.Value))
                return Result.Fail("El usuario editor no existe o está inactivo.");

            return Result.Ok();
        }

        private Result ValidarVentaEditable(MySqlConnection connection, MySqlTransaction transaction, int idVenta)
        {
            string query = @"SELECT estado
                             FROM venta
                             WHERE id = @id";

            MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@id", idVenta);

            object? estadoObj = command.ExecuteScalar();

            if (estadoObj == null)
                return Result.Fail("La venta no existe.");

            int estado = Convert.ToInt32(estadoObj);
            if (estado == 0)
                return Result.Fail("No se puede modificar una venta anulada.");

            return Result.Ok();
        }

        private Result ValidarMedicamentosYStock(MySqlConnection connection, MySqlTransaction transaction, List<DetalleVenta> detalles)
        {
            foreach (DetalleVenta detalle in detalles)
            {
                string query = @"SELECT nombre, stock, estado
                                 FROM medicamento
                                 WHERE id = @id_medicamento";

                MySqlCommand command = new MySqlCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@id_medicamento", detalle.IdMedicamento);

                using MySqlDataReader reader = command.ExecuteReader();

                if (!reader.Read())
                    return Result.Fail($"El medicamento con id {detalle.IdMedicamento} no existe.");

                string nombreMedicamento = StringHelper.LimpiarEspacios(reader["nombre"]?.ToString());
                int stock = Convert.ToInt32(reader["stock"]);
                int estado = Convert.ToInt32(reader["estado"]);

                reader.Close();

                if (estado != 1)
                    return Result.Fail($"El medicamento {nombreMedicamento} esta inactivo.");

                if (stock <= 0)
                    return Result.Fail($"El medicamento {nombreMedicamento} no tiene stock disponible.");

                if (stock < detalle.Cantidad)
                    return Result.Fail($"Stock insuficiente para el medicamento {nombreMedicamento}.");

                if (estado != 1)
                    return Result.Fail($"El medicamento con id {detalle.IdMedicamento} está inactivo.");

                if (stock <= 0)
                    return Result.Fail($"El medicamento con id {detalle.IdMedicamento} no tiene stock disponible.");

                if (stock < detalle.Cantidad)
                    return Result.Fail($"Stock insuficiente para el medicamento con id {detalle.IdMedicamento}.");
            }

            return Result.Ok();
        }

        private Result DescontarStock(MySqlConnection connection, MySqlTransaction transaction, int idMedicamento, int cantidad)
        {
            string query = @"UPDATE medicamento
                             SET stock = stock - @cantidad
                             WHERE id = @id_medicamento
                               AND estado = 1
                               AND stock >= @cantidad";

            MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@cantidad", cantidad);
            command.Parameters.AddWithValue("@id_medicamento", idMedicamento);

            int filas = command.ExecuteNonQuery();

            if (filas <= 0)
                return Result.Fail($"No se pudo descontar stock del medicamento con id {idMedicamento}.");

            return Result.Ok();
        }

        private bool ClienteActivo(MySqlConnection connection, MySqlTransaction transaction, int idCliente)
        {
            string query = @"SELECT COUNT(*)
                             FROM cliente
                             WHERE id = @id
                               AND estado = 1";

            MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@id", idCliente);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        private bool UsuarioActivo(MySqlConnection connection, MySqlTransaction transaction, int idUsuario)
        {
            string query = @"SELECT COUNT(*)
                             FROM usuario
                             WHERE id = @id
                               AND activo = 1";

            MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@id", idUsuario);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        private List<DetalleVenta> GetDetallesByVentaIdTransaccional(MySqlConnection connection, MySqlTransaction transaction, int idVenta)
        {
            List<DetalleVenta> detalles = new();

            string query = @"SELECT cantidad,
                                    precio_unitario,
                                    id_venta,
                                    id_medicamento
                             FROM detalle_venta
                             WHERE id_venta = @id_venta";

            MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@id_venta", idVenta);

            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                detalles.Add(new DetalleVenta
                {
                    Cantidad = Convert.ToInt32(reader["cantidad"]),
                    PrecioUnitario = Convert.ToDecimal(reader["precio_unitario"]),
                    IdVenta = Convert.ToInt32(reader["id_venta"]),
                    IdMedicamento = Convert.ToInt32(reader["id_medicamento"])
                });
            }

            return detalles;
        }
        public int Count()
        {
            string query = "SELECT COUNT(*) FROM venta";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                connection.Open();

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }        
    }
}

