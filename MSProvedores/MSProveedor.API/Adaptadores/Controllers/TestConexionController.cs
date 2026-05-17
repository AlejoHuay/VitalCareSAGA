using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using MSProveedor.Infraestructura.Conexion;

namespace MSProveedor.API.Adaptadores.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestConexionController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Verificar()
    {
        try
        {
            // Llama a tu Singleton para obtener la cadena
            string cadena = ConexionStringSingleton.Instancia.CadenaConexion;
            
            // Intenta abrir la conexión a Postgres
            using var conexion = new NpgsqlConnection(cadena);
            
            // Hacemos una consulta rápida a la tabla que creamos en pgAdmin
            var sql = "SELECT nombre FROM farmacia.proveedor LIMIT 1;";
            var nombreProveedor = await conexion.QueryFirstOrDefaultAsync<string>(sql);

            // Si llega aquí, la conexión fue un éxito
            return Ok(new 
            { 
                Estado = "🟢 ¡CONEXIÓN EXITOSA A POSTGRESQL!",
                BaseDeDatos = conexion.Database,
                DatoDePruebaEncontrado = nombreProveedor ?? "Tabla vacía"
            });
        }
        catch (Exception ex)
        {
            // Si la contraseña está mal o pgAdmin está apagado, caerá aquí
            return StatusCode(500, new 
            { 
                Estado = "🔴 ERROR DE CONEXIÓN", 
                Detalle = ex.Message 
            });
        }
    }
}