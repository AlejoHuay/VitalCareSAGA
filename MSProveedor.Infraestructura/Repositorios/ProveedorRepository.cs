using Dapper;
using Npgsql;
using MSProveedor.Dominio.Entidades;
using MSProveedor.Dominio.Interfaces;
using MSProveedor.Infraestructura.Conexion;

namespace MSProveedor.Infraestructura.Repositorios;

public class ProveedorRepository : IProveedorRepository
{
    public async Task<int> CrearAsync(Proveedor proveedor)
    {
        using var conexion = new NpgsqlConnection(ConexionStringSingleton.Instancia.CadenaConexion);
        
        var sql = @"
            INSERT INTO farmacia.proveedor 
            (nombre, telefono, correo_electronico, direccion, id_usuario) 
            VALUES 
            (@Nombre, @Telefono, @CorreoElectronico, @Direccion, @IdUsuario) 
            RETURNING id;";

        return await conexion.ExecuteScalarAsync<int>(sql, proveedor);
    }

    public async Task<bool> ExisteNombreAsync(string nombre)
    {
        using var conexion = new NpgsqlConnection(ConexionStringSingleton.Instancia.CadenaConexion);
        var sql = "SELECT COUNT(1) FROM farmacia.proveedor WHERE nombre = @Nombre;";
        var count = await conexion.ExecuteScalarAsync<int>(sql, new { Nombre = nombre });
        return count > 0;
    }
}